using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System;

[CustomEditor(typeof(ProtoShape2D))]
public class PS2DEditor:Editor{

	private ProtoShape2D script;
	private bool isDraggingPoint=false; //If the point is being dragged
	private bool isDraggingAnything=false; //If point or handle is being dragged
	private PS2DSnap snap=new PS2DSnap(); //For snap detection
	private double lastClickTime=0; //For double-click detection
	private Rect windowRect; //For point properties window
	private int markerID=0; //To hold ID of add point marker
	private bool dontDrawAddPoint=false; //For disabling + point marker when needed
	private Plane objectPlane; //Mostly to position controls using plane's normal

	private int pointClicked=-1; //To keep number of point which was clicked on mousedown
	private Vector2 dragOrigin; //To keep the coordinates of the mouse cursor before you started to drag the point. Needed in older Unity versions
	private int lastControlID=0; //Used to get control IDs for each control we create

	
	private bool pivotMoveMode=false;
	private bool draggingPivot=false;
	private Vector3 pivotStart;
	private Tool rememberTool;

	#region Adding and setting up new object

	[MenuItem("GameObject/2D Object/ProtoShape 2D")]
	static void CreateSimple(){
		AddObject(PS2DType.Simple);
	}

	static void AddObject(PS2DType type){
		GameObject go=new GameObject();
		go.AddComponent<ProtoShape2D>();
		go.GetComponent<ProtoShape2D>().SetSpriteMaterial();
		go.GetComponent<ProtoShape2D>().type=type;
		go.name="ProtoShape 2D";
		SceneView sc=SceneView.lastActiveSceneView!=null?SceneView.lastActiveSceneView:SceneView.sceneViews[0] as SceneView;
		go.transform.position=new Vector3(sc.pivot.x,sc.pivot.y,0f);
		if(Selection.activeGameObject!=null) go.transform.parent=Selection.activeGameObject.transform;
		Selection.activeGameObject=go;
		Tools.current=Tool.Move;
	}

	void Awake(){
		//string appVersionBase=Application.unityVersion.Substring(0,Application.unityVersion.LastIndexOf('.')); //Get base version of Unity editor: 5.1 or 2017.2 - removing the last part
		script=(ProtoShape2D)target;
		if(script.points.Count==0){
			SceneView sc=SceneView.lastActiveSceneView!=null?SceneView.lastActiveSceneView:SceneView.sceneViews[0] as SceneView;
			float size=sc.size/6f;
			AddPoint((new Vector2(-2f,1f)+(UnityEngine.Random.insideUnitCircle*0.1f))*size);
			AddPoint((new Vector2(2f,1f)+(UnityEngine.Random.insideUnitCircle*0.1f))*size);
			AddPoint((new Vector2(2f,-1f)+(UnityEngine.Random.insideUnitCircle*0.1f))*size);
			AddPoint((new Vector2(-2f,-1f)+(UnityEngine.Random.insideUnitCircle*0.1f))*size);
			script.color1=RandomColor();
			script.color2=RandomColor();
			script.outlineColor=RandomColor();
			DeselectAllPoints();
			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<MeshFilter>(),false);
			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<MeshRenderer>(),false);
		}
		SceneView.RepaintAll();
		script.UpdateMaterialSettings();
		script.UpdateMesh();
	}

	private void OnDestroy(){
		PivotMoveModeOff();
	}

	#endregion

	#region Inspector controls

	public override void OnInspectorGUI(){

		//Force repaint on undo/redo
		bool forceRepaint =false;
		if(Event.current.type==EventType.ValidateCommand){
			if(Event.current.commandName=="UndoRedoPerformed") forceRepaint=true;
		}

		//Convert between simple and bezier
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Curve type","Select how the curves work"));
		int typeCurrent=(script.type==PS2DType.Simple?0:1);
		GUIContent[] buttons=new GUIContent[]{
			new GUIContent("Simple","Create simple curves by rounding the corners using a slider in point's properties window"),
			new GUIContent("Bezier","Create more precise curves by using Bezier controls similar to vector editors")
		};
		int typeState=GUILayout.Toolbar(typeCurrent,buttons);
		if(typeState!=typeCurrent){
			bool confirm=true;
			//If switching from Bezier to Simple, check if there are curves
			//and warn user that details can be lost
			if(typeState==0){
				bool hasCurves=false;
				for(int i=0;i<script.points.Count;i++){
					if(script.points[i].pointType!=PS2DPointType.None){
						hasCurves=true;
						break;
					}
				}
				if(hasCurves) confirm=EditorUtility.DisplayDialog("Convert to Simple","When converting from Bezier to Simple you will loose the details of the curves.","Continue","Cancel");
			}
			if(confirm){
				Undo.RecordObject(script,"Simple/Bezier convert");
				if(typeState==0){ //Converting to Simple
					script.type=PS2DType.Simple;
				}else{ //Converting to Bezier
					script.type=PS2DType.Bezier;
					for(int i=0;i<script.points.Count;i++){
						if(script.points[i].curve==0) script.points[i].pointType=PS2DPointType.None;
						else if(script.points[i].curve>0) script.points[i].pointType=PS2DPointType.Rounded;
					}
				}
				EditorUtility.SetDirty(script);
			}
		}
		EditorGUILayout.EndHorizontal();

		//Fill settings
		script.showFillSettings=EditorGUILayout.Foldout(script.showFillSettings,"Fill",true);
		if(script.showFillSettings){
			PS2DFillType fillType=(PS2DFillType)EditorGUILayout.EnumPopup(new GUIContent("Fill type","Which fill to use for this object. Single color is optimal for mobile games since it uses built-in sprite shader"),script.fillType);
			//If fill type changed
			if(fillType!=script.fillType){
				Undo.RecordObject(script,"Change fill type");
				//If setting changed to single color or to no-fill, we use Unity's built-in sprite material
				if(fillType==PS2DFillType.Color || fillType==PS2DFillType.None) script.SetSpriteMaterial(); 
				//If setting changed to custom material, we use a material provided by user
				else if(fillType==PS2DFillType.CustomMaterial) script.SetCustomMaterial();
				//Otherwise we use our own shader that supports gradient and texture
				else script.SetDefaultMaterial(); 
				//Update the object
				script.fillType=fillType;
				script.UpdateMaterialSettings();
				EditorUtility.SetDirty(script);
			}
			//Texture
			if(script.fillType==PS2DFillType.Texture || script.fillType==PS2DFillType.TextureWithColor || script.fillType==PS2DFillType.TextureWithGradient){
				Texture2D texture=(Texture2D)EditorGUILayout.ObjectField(new GUIContent("Texture","An image for tiling. Needs to have \"Wrap Mode\" property set to \"Repeat\""),script.texture,typeof(Texture2D),false);
				if(script.texture!=texture){
					Undo.RecordObject(script,"Change texture");
					script.texture=texture;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float textureScale=EditorGUILayout.FloatField(new GUIContent("Texture scale","Change size of the texture"),script.textureScale);
				if(textureScale!=script.textureScale){
					if(textureScale<0) textureScale=0;
					Undo.RecordObject(script,"Change texture size");
					script.textureScale=textureScale;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float textureRotation=EditorGUILayout.Slider(new GUIContent("Texture rotation","Set angle of rotation for texture"),script.textureRotation,-180f,180f);
				if(textureRotation!=script.textureRotation){
					Undo.RecordObject(script,"Change texture rotation");
					script.textureRotation=textureRotation;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float textureOffsetX=EditorGUILayout.Slider(new GUIContent("Texture offset X","Offset texture on X axis"),script.textureOffset.x,-1f,1f);
				if(textureOffsetX!=script.textureOffset.x){
					Undo.RecordObject(script,"Change texture offset");
					script.textureOffset.x=Mathf.Clamp(textureOffsetX,-1f,1f);
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float textureOffsetY=EditorGUILayout.Slider(new GUIContent("Texture offset Y","Offset texture on Y axis"),script.textureOffset.y,-1f,1f);
				if(textureOffsetY!=script.textureOffset.y){
					Undo.RecordObject(script,"Change texture offset");
					script.textureOffset.y=Mathf.Clamp(textureOffsetY,-1f,1f);
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}

			}
			//Single color setup
			if(script.fillType==PS2DFillType.Color || script.fillType==PS2DFillType.TextureWithColor){
				#if UNITY_2018_1_OR_NEWER
				Color color1=EditorGUILayout.ColorField(new GUIContent("Color","Color to fill the object with"),script.color1,true,true,script.HDRColors);
				#else
				Color color1=EditorGUILayout.ColorField(new GUIContent("Color","Color to fill the object with"),script.color1);
				#endif
				if(script.color1!=color1){
					Undo.RecordObject(script,"Change color");
					script.color1=color1;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
			}
			//Two color setup
			if(script.fillType==PS2DFillType.Gradient || script.fillType==PS2DFillType.TextureWithGradient){
				#if UNITY_2018_1_OR_NEWER
				Color gcolor1=EditorGUILayout.ColorField(new GUIContent("Color one","Top color for the gradient"),script.color1,true,true,script.HDRColors);
				#else
				Color gcolor1=EditorGUILayout.ColorField(new GUIContent("Color one","Top color for the gradient"),script.color1);
				#endif
				if(script.color1!=gcolor1){
					Undo.RecordObject(script,"Change color one");
					script.color1=gcolor1;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				#if UNITY_2018_1_OR_NEWER
				Color gcolor2=EditorGUILayout.ColorField(new GUIContent("Color two","Bottom color for the gradient"),script.color2,true,true,script.HDRColors);
				#else
				Color gcolor2=EditorGUILayout.ColorField(new GUIContent("Color two","Bottom color for the gradient"),script.color2);
				#endif
				if(script.color2!=gcolor2){
					Undo.RecordObject(script,"Change color two");
					script.color2=gcolor2;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float gradientScale=EditorGUILayout.Slider(new GUIContent("Gradient scale","Zoom gradient in and out relatively to height of the object."),script.gradientScale,0f,2f);
				if(gradientScale!=script.gradientScale){
					Undo.RecordObject(script,"Change gradient scale");
					script.gradientScale=gradientScale;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float gradientRotation=EditorGUILayout.Slider(new GUIContent("Gradient rotation","Set angle of rotation for gradient"),script.gradientRotation,-180f,180f);
				if(gradientRotation!=script.gradientRotation){
					Undo.RecordObject(script,"Change gradient rotation");
					script.gradientRotation=gradientRotation;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float gradientOffset=EditorGUILayout.Slider(new GUIContent("Gradient offset","Offset gradient up or down."),script.gradientOffset,-1f,1f);
				if(gradientOffset!=script.gradientOffset){
					Undo.RecordObject(script,"Change gradient offset");
					script.gradientOffset=gradientOffset;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
			}
			//Custom material setup
			if(script.fillType==PS2DFillType.CustomMaterial){
				Material material=(Material)EditorGUILayout.ObjectField(new GUIContent("Custom material","If you provide same material for multiple objects, it will lower the number of DrawCalls therefor optimizing the rendering process."),script.customMaterial,typeof(Material),false);
				if(script.customMaterial!=material){
					Undo.RecordObject(script,"Change custom material");
					script.SetCustomMaterial(material);
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
			}
		}

		//Outline settings
		script.showOutlineSettings=EditorGUILayout.Foldout(script.showOutlineSettings,"Outline",true);
		if(script.showOutlineSettings){
			float outlineWidth=EditorGUILayout.FloatField(new GUIContent("Width","Outline width. Outline is disabled when this number is zero."),script.outlineWidth);
			if(outlineWidth!=script.outlineWidth){
				outlineWidth=Mathf.Max(0f,outlineWidth);
				Undo.RecordObject(script,"Change outline width");
				script.outlineWidth=outlineWidth;
				if(outlineWidth>0) script.antialias=false; //Turn off antialising
				script.UpdateMesh();
				EditorUtility.SetDirty(script);					
			}
			#if UNITY_2018_1_OR_NEWER
			Color outlineColor=EditorGUILayout.ColorField(new GUIContent("Color","Color of the outline"),script.outlineColor,true,true,script.HDRColors);
			#else
			Color outlineColor=EditorGUILayout.ColorField(new GUIContent("Color","Color of the outline"),script.outlineColor);
			#endif
			if(outlineColor!=script.outlineColor){
				Undo.RecordObject(script,"Change outline color");
				script.outlineColor=outlineColor;
				script.UpdateMesh();
				EditorUtility.SetDirty(script);
			}
			bool outlineLoop=EditorGUILayout.Toggle(new GUIContent("Loop outline","Connect start and end of the outline."),script.outlineLoop);
			if(outlineLoop!=script.outlineLoop){
				Undo.RecordObject(script,"Change outline looping");
				script.outlineLoop=outlineLoop;
				EditorUtility.SetDirty(script);
			}

			bool outlineUseCustomMaterial=EditorGUILayout.Toggle(new GUIContent("Use custom material","Allows to assign your own material to the outline."),script.outlineUseCustomMaterial);
			if(outlineUseCustomMaterial!=script.outlineUseCustomMaterial){
				Undo.RecordObject(script,"Change outline custom material setting");
				script.outlineUseCustomMaterial=outlineUseCustomMaterial;
				script.UpdateMaterialSettings();
				EditorUtility.SetDirty(script);
			}

			//Custom material setup
			if(script.outlineUseCustomMaterial){
				Material outlineCustomMaterial=(Material)EditorGUILayout.ObjectField(new GUIContent("Custom material","If you provide same material for multiple objects, it will lower the number of DrawCalls therefore optimizing the rendering process."),script.outlineCustomMaterial,typeof(Material),false);
				if(script.outlineCustomMaterial!=outlineCustomMaterial){
					Undo.RecordObject(script,"Change outline custom material");
					script.SetOutlineCustomMaterial(outlineCustomMaterial);
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
			}

		}

		script.showMeshSetting=EditorGUILayout.Foldout(script.showMeshSetting,"Mesh",true);
		if(script.showMeshSetting){
			//Edit curve iterations
			int curveIterations=EditorGUILayout.IntSlider(new GUIContent("Curve iterations","How many points a curved line should have"),script.curveIterations,1,30);
			if(curveIterations!=script.curveIterations){
				Undo.RecordObject(script,"Change curve iterations");
				script.curveIterations=curveIterations;
				EditorUtility.SetDirty(script);
			}
			//"Anti-aliasing"
			EditorGUI.BeginDisabledGroup(script.outlineWidth>0);
			bool antialias=EditorGUILayout.Toggle(new GUIContent("Anti-aliasing","Create an anti-aliasing effect by adding a thin transparent gradient outline to the mesh. Doesn't work with outline."),script.antialias);
			if(antialias!=script.antialias){
				Undo.RecordObject(script,"Change anti-aliasing");
				script.antialias=antialias;
				script.aaridge=0.002f*(Camera.main!=null?Camera.main.orthographicSize*2:10f);
				EditorUtility.SetDirty(script);
			}
			EditorGUI.EndDisabledGroup();
			//Edit antialiasing ridge
			if(antialias){
				float aaridge=EditorGUILayout.FloatField(new GUIContent("Anti-aliasing ridge","Width of anti-aliasing border"),script.aaridge);
				if(aaridge!=script.aaridge){
					if(aaridge<0f) aaridge=0f;
					Undo.RecordObject(script,"Changed anti-aliasing width");
					script.aaridge=aaridge;
					EditorUtility.SetDirty(script);
				}
			}

			//Show triangle count
			GUILayout.Box(new GUIContent(script.triangleCount>1?"The mesh has "+script.triangleCount.ToString()+" triangles":(script.triangleCount==1?"The mesh is just one triangle":"The mesh has no triangles")),EditorStyles.helpBox);

			/*
			//Allows to expose point positions in inspector
			Vector2[] pPositions=new Vector2[script.points.Count];
			for(int i=0;i<script.points.Count;i++){
				pPositions[i]=EditorGUILayout.Vector2Field(script.points[i].name,script.points[i].position);
				if(pPositions[i]!=script.points[i].position){
					script.points[i].position=pPositions[i];
					script.UpdateMesh();
				}
			}  
			*/
		}

		//Snap settings
		script.showSnapSetting=EditorGUILayout.Foldout(script.showSnapSetting,"Snap",true);
		if(script.showSnapSetting){
			PS2DSnapType snapType=(PS2DSnapType)EditorGUILayout.EnumPopup(new GUIContent("Snap type","Which type of snapping is used when Shift is pressed"),script.snapType);
			if(snapType!=script.snapType){
				Undo.RecordObject(script,"Change snap type");
				script.snapType=snapType;
				EditorUtility.SetDirty(script);
			}
			if(script.snapType!=PS2DSnapType.Points){
				float gridSize=EditorGUILayout.FloatField(new GUIContent("Grid size","Distance between grid lines"),script.gridSize);
				if(gridSize!=script.gridSize){
					gridSize=Mathf.Max(0.01f,gridSize);
					Undo.RecordObject(script,"Change gradient offset");
					script.gridSize=gridSize;
					EditorUtility.SetDirty(script);
				}
			}
		}

		//Collider settings
		script.showColliderSettings=EditorGUILayout.Foldout(script.showColliderSettings,"Collider",true);
		if(script.showColliderSettings){
			PS2DColliderType colliderType=(PS2DColliderType)EditorGUILayout.EnumPopup(new GUIContent("Auto collider 2D","Automatically create a collider. Set to \"None\" if you want to create your collider by hand"),script.colliderType);
			if(colliderType!=script.colliderType){
				if(RemoveCollider(colliderType)){
					Undo.RecordObject(script,"Change collider type");
					script.colliderType=colliderType;
					AddCollider();
					script.UpdateMesh();
					EditorUtility.SetDirty(script);
				}
				EditorGUIUtility.ExitGUI();
			}
			if(script.colliderType!=PS2DColliderType.None && script.colliderType!=PS2DColliderType.MeshStatic && script.colliderType!=PS2DColliderType.MeshDynamic){
				float colliderTopAngle=EditorGUILayout.Slider(new GUIContent("Top edge arc","Decides which edges are considered to be facing up"),script.colliderTopAngle,1,180);
				if(colliderTopAngle!=script.colliderTopAngle){
					Undo.RecordObject(script,"Change top edge arc");
					script.colliderTopAngle=colliderTopAngle;
					EditorUtility.SetDirty(script);
				}
				float colliderOffsetTop=EditorGUILayout.Slider(new GUIContent("Offset top","Displace part of collider that is considered to be facing up"),script.colliderOffsetTop,-1,1);
				if(colliderOffsetTop!=script.colliderOffsetTop){
					Undo.RecordObject(script,"Change offset top");
					script.colliderOffsetTop=colliderOffsetTop;
					EditorUtility.SetDirty(script);
				}
				bool showNormals=EditorGUILayout.Toggle(new GUIContent("Show normals","Visually shows which edges are facing which side. Just to better understand how \"Top edge arc\" works"),script.showNormals);
				if(showNormals!=script.showNormals){
					Undo.RecordObject(script,"Show normals");
					script.showNormals=showNormals;
					EditorUtility.SetDirty(script);
				}
			}
			if(script.colliderType==PS2DColliderType.MeshStatic || script.colliderType==PS2DColliderType.MeshDynamic){
				float cMeshDepth=EditorGUILayout.Slider(new GUIContent("Collider Depth","The distance between two sides of the collider"),script.cMeshDepth,0,10);
				if(cMeshDepth!=script.cMeshDepth){
					Undo.RecordObject(script,"Change mesh collider depth");
					script.cMeshDepth=cMeshDepth;
					script.UpdateMesh();
					EditorUtility.SetDirty(script);
				}
			}
			if(script.colliderType==PS2DColliderType.MeshDynamic){
				GUILayout.Box(new GUIContent("Note that dynamic mesh colliders can only be convex even if they appear to be not."),EditorStyles.helpBox);
			}
		}

		script.showTools=EditorGUILayout.Foldout(script.showTools,"Tools",true);
		if(script.showTools){
			//Z-sorting
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(new GUIContent("Z-sorting","Adds and substracts 0.01 on Z axis for very basic sorting"));
			if(GUILayout.Button(new GUIContent("Pull","Subtract 0.01 on Z axis"))){
				Undo.RecordObject(script.transform,"Pull");
				script.transform.position-=Vector3.forward*0.01f;
				EditorUtility.SetDirty(script.transform);
			}
			if(GUILayout.Button(new GUIContent("Push","Add 0.01 on Z axis"))){
				Undo.RecordObject(script.transform,"Push");
				script.transform.position+=Vector3.forward*0.01f;
				EditorUtility.SetDirty(script.transform);
			}
			EditorGUILayout.EndHorizontal();

			//Storing padding for toolbar buttons so we can restore it later
			RectOffset oPaddingButtonLeft=GUI.skin.GetStyle("ButtonLeft").padding;
			RectOffset oPaddingButtonMid=GUI.skin.GetStyle("ButtonMid").padding;
			RectOffset oPaddingButtonRight=GUI.skin.GetStyle("ButtonRight").padding;
			//Changing padding for toolbar buttons to fit more stuff
			GUI.skin.GetStyle("ButtonLeft").padding=new RectOffset(0,0,3,3);
			GUI.skin.GetStyle("ButtonMid").padding=new RectOffset(0,0,3,3);
			GUI.skin.GetStyle("ButtonRight").padding=new RectOffset(0,0,3,3);

			//Pivot type
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(new GUIContent("Pivot","Move and configure object's pivot"));

			//Move pivot manually
			GUIStyle pivotButtonStyle=new GUIStyle(GUI.skin.button);
			pivotButtonStyle.padding=new RectOffset(3,3,3,3);
			pivotButtonStyle.stretchWidth=false;
			pivotButtonStyle.fixedWidth=21;
			pivotButtonStyle.fixedHeight=GUI.skin.GetStyle("ButtonMid").CalcHeight(new GUIContent("Text"),10f);
			bool newPivotMoveMode=GUILayout.Toggle(pivotMoveMode,new GUIContent((Texture)Resources.Load("Icons/movePivot"),"Move pivot manually"),pivotButtonStyle);
			if(newPivotMoveMode!=pivotMoveMode){ 
				pivotMoveMode=newPivotMoveMode;
				if(pivotMoveMode){
					PivotMoveModeOn();
				}else{
					PivotMoveModeOff();
				}
			}

			//Pivot type switch
			int pivotType=GUILayout.Toolbar((int)script.pivotType,EnumToGUI<PS2DPivotType>());
			if(pivotType!=(int)script.pivotType){
				GUI.FocusControl(null);
				script.pivotType=(PS2DPivotType)pivotType;
				if(script.pivotType==PS2DPivotType.Auto){
					MovePivot();
					if(pivotMoveMode) PivotMoveModeOff();
				}
			}
			EditorGUILayout.EndHorizontal();

			//Pivot position
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(EditorGUIUtility.labelWidth);
			int pivotPosition=GUILayout.Toolbar(script.pivotType==PS2DPivotType.Manual?-1:(int)script.PivotPosition,EnumToGUI<PS2DPivotPosition>("Icons/pivot"));
			if((script.pivotType==PS2DPivotType.Manual && pivotPosition!=-1) || (script.pivotType==PS2DPivotType.Auto && pivotPosition!=(int)script.PivotPosition)){
				script.PivotPosition=(PS2DPivotPosition)pivotPosition;
				MovePivot();
				pivotStart=script.transform.position;
			}
			EditorGUILayout.EndHorizontal();

			//Restoring original button padding
			GUI.skin.GetStyle("ButtonLeft").padding=oPaddingButtonLeft;
			GUI.skin.GetStyle("ButtonMid").padding=oPaddingButtonMid;
			GUI.skin.GetStyle("ButtonRight").padding=oPaddingButtonRight;

			#if UNITY_2018_1_OR_NEWER
			//HDR colors
			bool HDRColors=EditorGUILayout.Toggle(new GUIContent("HDR Colors","Treat colors as HDR values. Works with some post-processing effects."),script.HDRColors);
			if(HDRColors!=script.HDRColors){
				Undo.RecordObject(script,"Change HDR colors");
				script.HDRColors=HDRColors;
				EditorUtility.SetDirty(script);
			}
			#endif
			//Export
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(new GUIContent("Export","Export the shape to enother form of object"));
			if(GUILayout.Button(new GUIContent("Mesh","Save current object as a Mesh asset in the root of your project"))){
				ExportMesh();
			}
			if(GUILayout.Button(new GUIContent("PNG","Save current object as a PNG file in the root of your project"))){
				ExportPNG();
			}
			EditorGUILayout.EndHorizontal();

			/*
			//Extrude
			if(AssetDatabase.FindAssets("ProtoShape2DExtruder").Length>0){
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("Extrude"));
				if(script.gameObject.GetComponent<PS2DExtruder>()==null){
					if(GUILayout.Button(new GUIContent("Extrude","Extrude the shape into 3D space"))){
						script.gameObject.AddComponent<PS2DExtruder>();
					}
				}else{ 
					if(GUILayout.Button(new GUIContent("Remove","Remove the extrusion component and child objects"))){
						script.gameObject.GetComponent<PS2DExtruder>().Remove();
						DestroyImmediate(script.gameObject.GetComponent<PS2DExtruder>());
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			*/

			//Sprite sorting
			GUILayout.Space(10);
			//Get sorting layers
			int[] layerIDs=GetSortingLayerUniqueIDs();
			string[] layerNames=GetSortingLayerNames();
			//Get selected sorting layer
			int selected=-1;
			for(int i=0;i<layerIDs.Length;i++){
				if(layerIDs[i]==script.sortingLayer){
					selected=i;
				}
			}
			//Select Default layer if no other is selected
			if(selected==-1){
				for(int i=0;i<layerIDs.Length;i++){
					if(layerIDs[i]==0){
						selected=i;
					}
				}
			}
			//Sorting layer dropdown
			EditorGUI.BeginChangeCheck();
			GUIContent[] dropdown=new GUIContent[layerNames.Length+2];
			for(int i=0;i<layerNames.Length;i++){
				dropdown[i]=new GUIContent(layerNames[i]);
			}
			dropdown[layerNames.Length]=new GUIContent();
			dropdown[layerNames.Length+1]=new GUIContent("Add Sorting Layer...");
			selected=EditorGUILayout.Popup(new GUIContent("Sorting Layer","Name of the Renderer's sorting layer"),selected,dropdown);
			if(EditorGUI.EndChangeCheck()){
				Undo.RecordObject(script,"Change sorting layer");
				if(selected==layerNames.Length+1){
					EditorApplication.ExecuteMenuItem("Edit/Project Settings/Tags and Layers");
				}else{
					script.sortingLayer=layerIDs[selected];
				}
				EditorUtility.SetDirty(script);
			}
			//Order in layer field
			EditorGUI.BeginChangeCheck();
			int order=EditorGUILayout.IntField(new GUIContent("Order in Layer","Renderer's order within a sorting layer"),script.orderInLayer);
			if(EditorGUI.EndChangeCheck()){
				Undo.RecordObject(script,"Change order in layer");
				script.orderInLayer=order;
				EditorUtility.SetDirty(script);
			}
			//Mask dropdown
			EditorGUI.BeginChangeCheck();
			GUIContent[] dropdownMask=new GUIContent[3];
			dropdownMask[0]=new GUIContent("None");
			dropdownMask[1]=new GUIContent("Visible Inside Mask");
			dropdownMask[2]=new GUIContent("Visible Outside Mask");
			int selectedMaskOption=EditorGUILayout.Popup(new GUIContent("Mask Interaction","Interaction with a Sprite Mask"),script.selectedMaskOption,dropdownMask);
			if(EditorGUI.EndChangeCheck()){
				Undo.RecordObject(script,"Change mask interaction");
				script.selectedMaskOption=selectedMaskOption;
				//script.UpdateMaterialSettings();
				EditorUtility.SetDirty(script);
			}
		}
		//React to changes in GUI
		if(GUI.changed || forceRepaint){
			script.UpdateMesh();
			SceneView.RepaintAll();
		}
	}

	private void PivotMoveModeOn(){ 
		script.pivotType=PS2DPivotType.Manual;
		rememberTool=Tools.current;
		Tools.current=Tool.None;
		pivotStart=script.transform.position;
		pivotMoveMode=true;
	}

	private void PivotMoveModeOff(){ 
		if(rememberTool==Tool.None) rememberTool=Tool.Move;
		if(Tools.current==Tool.None) Tools.current=rememberTool;
		pivotMoveMode=false;
	}

    #endregion

    #region Scene GUI - drawing points, lines, grid in the scene view

    void OnSceneGUI(){
		Tools.pivotMode=PivotMode.Pivot;
		EventType et=Event.current.type; //Need to save this because it can be changed to Used by other functions
		//Create an object plane
		objectPlane=new Plane(
			script.transform.TransformPoint(new Vector3(0,0,0)),
			script.transform.TransformPoint(new Vector3(0,1,0)),
			script.transform.TransformPoint(new Vector3(1,0,0))
		);
		//Detecting if object is being dragged
		if(et==EventType.MouseDrag) isDraggingAnything=true;
		if(isDraggingAnything && et==EventType.MouseUp) isDraggingAnything=false;
		//If current tool is none, we're probably in collider edit mode
		if(Tools.current!=Tool.None && script.isActiveAndEnabled){
			PivotMoveModeOff();
			//Draw a snapping grid
			if(script.snapType!=PS2DSnapType.Points) DrawGrid();
			//Draw outline
			DrawLines();
			//Deselect all points on ESC
			if(et==EventType.KeyDown){
				if(Event.current.keyCode==KeyCode.Escape){
					DeselectAllPoints();
					SceneView.RepaintAll();
				}
			}
			//When CTRL is pressed, draw only deleteable points
			if(Event.current.control || (Event.current.command && (Application.platform==RuntimePlatform.OSXEditor || Application.platform==RuntimePlatform.OSXPlayer))){
				if(script.points.Count>2){
					for(int i=0;i<script.points.Count;i++){
						DrawDeletePoint(i);
					}
				}
			}else{
				//Controls for gradient
				if(script.fillType==PS2DFillType.Gradient || script.fillType==PS2DFillType.TextureWithGradient){
					DrawGradientControls();
				}
				//Controls for texture
				if(script.fillType==PS2DFillType.Texture || script.fillType==PS2DFillType.TextureWithColor || script.fillType==PS2DFillType.TextureWithGradient){
					if(script.texture!=null) DrawTextureControls();
				}
				//Draw a marker to add new points, but only if nothing is being dragged right now
				if(!dontDrawAddPoint && !isDraggingAnything) {
					DrawAddPointMarker(et);
				}
				//Draw draggable points
				for(int i=0;i<script.points.Count;i++){
					DrawPoint(i);
				}
				//This tracks if mouse is over bezier point
				dontDrawAddPoint=false;
				//Draw draggable bezier controls
				for(int i=0;i<script.points.Count;i++){
					DrawBezierControls(i);
				}
				//Remember mouse position on mouse down
				if(et==EventType.MouseDown) dragOrigin=GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
				//On drag end check the distance of dragging and if it's zero, this was just a click, not a drag
				if(et==EventType.MouseUp && !Event.current.shift && pointClicked!=-1 && Mathf.Approximately((GUIUtility.GUIToScreenPoint(Event.current.mousePosition)-dragOrigin).magnitude,0f)){
					DeselectAllPoints();
					SelectPoint(pointClicked,true);
					pointClicked=-1;
				}
				//Select/deselect points
				if(et==EventType.MouseDown && GUIUtility.hotControl!=0){
					pointClicked=-1;
					bool PS2DControlsClicked=false; //Track if any point or bezier control was clicked
					for(int i=0;i<script.points.Count;i++){
						//Detect click on a point
						if(GUIUtility.hotControl==script.points[i].controlID){
							PS2DControlsClicked=true;
							pointClicked=i;
							//Detect double click on a control and invert bezier handles in simple mode
							if(EditorApplication.timeSinceStartup-lastClickTime<0.3f){
								script.points[i].median*=-1;
								script.UpdateMesh();
								lastClickTime=0f;
							}else{
								lastClickTime=EditorApplication.timeSinceStartup;
							}
							break;
						}
						//Check for a click on bezier controls
						if(!PS2DControlsClicked && (GUIUtility.hotControl==script.points[i].controlPID || GUIUtility.hotControl==script.points[i].controlNID)){
							PS2DControlsClicked=true;
						}
					}
					//If we found a point that corresponds to current hotControl
					if(pointClicked>-1){
						//If shift is being held we toggle selection of this point
						if(Event.current.shift){
							SelectPoint(pointClicked,!script.points[pointClicked].selected);
						//If shift is now down we just set this point to selected
						}else{
							//If this point wasn't selected we deselect all other poings
							if(!script.points[pointClicked].selected) DeselectAllPoints();
							SelectPoint(pointClicked,true);
						}
					}
					//If we found that no point or bezier control was clicked, we deselect all points
					if(!PS2DControlsClicked) DeselectAllPoints();
				}
				//Draw collider lines
				DrawCollider();
				//Draw properties of selected points
				DrawPointsProperties();
			}
		//Move pivot mode
		}else if(Tools.current==Tool.None && pivotMoveMode && script.isActiveAndEnabled){
			Handles.color=Color.red;
			Handles.DrawSolidDisc(pivotStart,objectPlane.normal,HandleUtility.GetHandleSize(pivotStart)*0.03f);
			Handles.color=Color.white;
			EditorGUI.BeginChangeCheck();
			pivotStart=Handles.FreeMoveHandle(pivotStart,Quaternion.identity,HandleUtility.GetHandleSize(pivotStart)*0.1f,Vector3.zero,Handles.CircleHandleCap);
			bool changed=EditorGUI.EndChangeCheck();
			if(changed && draggingPivot==false){ 
				draggingPivot=true;
				
			}
			//React to drag stop
			if(et==EventType.MouseUp && draggingPivot){
				draggingPivot=false;
				MovePivot(script.transform.InverseTransformPoint(pivotStart));
				EditorUtility.SetDirty(script);
				pivotStart=script.transform.position;
			}
		}else{
			DeselectAllPoints();
		}
		SceneView.RepaintAll();
	}

	void DrawLines(){
		Handles.color=new Color(1f,1f,1f,0.6f);
		for(int i=0;i<script.outlineConnect-(script.outlineLoop?0:1);i++){
			Handles.DrawLine(
				script.transform.TransformPoint(script.pointsFinal[i]),
				script.transform.TransformPoint(script.pointsFinal.Loop(i+1))
			);
		}
		Handles.color=Color.white;
	}

	void DrawPoint(int pointID){
		EventType et=Event.current.type;
		float size=HandleUtility.GetHandleSize(script.points[pointID].position)*0.1f;
		//Circle around drag point
		Handles.color=new Color(1,1,1,0.5f);
		Handles.DrawWireDisc(script.transform.TransformPoint(script.points[pointID].position),objectPlane.normal,size);
		//Drag point
		Handles.color=Color.clear;
		EditorGUI.BeginChangeCheck();
		Vector3 point=Handles.FreeMoveHandle(
			script.transform.TransformPoint(script.points[pointID].position),
			script.transform.rotation,
			size,
			Vector3.zero,
			CircleHandleCapSaveID
		);
		bool changed=EditorGUI.EndChangeCheck();
		//Assign last control id (retrieved by CircleHandleCapSaveID function)
		//Doing it during a layout event because that's when IDs are being set
		if(et==EventType.Layout) script.points[pointID].controlID=lastControlID;
		//Compensate for Unity's incorrect treatment of displays with higher pixel density
		if(changed && !isDraggingPoint){
			//Select point if it's being dragged
			if(!script.points[pointID].selected) SelectPoint(pointID,true);
			isDraggingPoint=true;
		}
		//Change to default cursor if mouse is near the point
		Handles.color=Color.white;
		if(Vector2.Distance(script.transform.TransformPoint(script.points[pointID].position),GetMouseWorldPosition())<size){
			SetCursor(MouseCursor.Arrow);
		}
		//Snapping. Setting point's position depending on proximity to the grid
		if(script.points[pointID].controlID==GUIUtility.hotControl && isDraggingPoint && Event.current.shift){
			if(script.snapType==PS2DSnapType.Points){
				snap.Reset(size);
				for(int i=0;i<script.points.Count;i++){
					if(i==pointID || script.points[i].selected) continue; //Don't snap to itself or to other selected points
					snap.CheckPoint(i,point,script.transform.TransformPoint(script.points[i].position));
				}
				if(snap.GetClosestAxes()>0){
					point=snap.snapLocation;
					Vector3 ab;
					if(snap.snapPoint1>-1){
						ab=((Vector3)snap.snapLocation-script.transform.TransformPoint(script.points[snap.snapPoint1].position)).normalized*(size*8);
						Handles.DrawLine(
							(Vector3)snap.snapLocation+ab,
							script.transform.TransformPoint(script.points[snap.snapPoint1].position)-ab
						);
					}
					if(snap.snapPoint2>-1){
						ab=((Vector3)snap.snapLocation-script.transform.TransformPoint(script.points[snap.snapPoint2].position)).normalized*(size*8);
						Handles.DrawLine(
							(Vector3)script.transform.TransformPoint(script.points[pointID].position)+ab,
							script.transform.TransformPoint(script.points[snap.snapPoint2].position)-ab
						);
					}
				}
			}else if(script.snapType==PS2DSnapType.WorldGrid || script.snapType==PS2DSnapType.LocalGrid){
				Vector2 pointTr=point;
				if(script.snapType==PS2DSnapType.LocalGrid) pointTr=script.transform.InverseTransformPoint(pointTr);
				Vector2[] snapPoints=new Vector2[4]{
					new Vector2(Mathf.Floor(pointTr.x/script.gridSize),Mathf.Floor(pointTr.y/script.gridSize))*script.gridSize,
					new Vector2(Mathf.Floor(pointTr.x/script.gridSize),Mathf.Ceil(pointTr.y/script.gridSize))*script.gridSize,
					new Vector2(Mathf.Ceil(pointTr.x/script.gridSize),Mathf.Ceil(pointTr.y/script.gridSize))*script.gridSize,
					new Vector2(Mathf.Ceil(pointTr.x/script.gridSize),Mathf.Floor(pointTr.y/script.gridSize))*script.gridSize
				};
				int snapPoint=-1;
				float closestDistance=script.gridSize;
				float dist;
				for(int j=0;j<snapPoints.Length;j++){
					if(script.snapType==PS2DSnapType.LocalGrid) snapPoints[j]=script.transform.TransformPoint(snapPoints[j]);
					dist=Vector2.Distance(point,snapPoints[j]);
					if(dist<closestDistance){
						snapPoint=j;
						closestDistance=dist;
					}
				}
				if(snapPoint>-1) point=snapPoints[snapPoint];
			}
		}
		//Actual dragging
		if(script.points[pointID].selected && (changed || (isDraggingPoint && Event.current.shift))){
			Undo.RecordObject(script,"Move point");
			Vector3 diff=(Vector2)script.transform.InverseTransformPoint(point)-script.points[pointID].position;
			//Move all selected points
			for(int i=0;i<script.points.Count;i++){
				if(script.points[i].selected) script.points[i].Move(diff,script.type==PS2DType.Bezier?true:false);
			}
			script.UpdateMesh();
		}
		//If point is selected, draw a circle
		if(script.points[pointID].selected==true){
			Handles.DrawWireDisc(point,objectPlane.normal,size);
			Handles.DrawSolidDisc(script.transform.TransformPoint(script.points[pointID].position),objectPlane.normal,size*0.75f);
		}
		//React to drag stop
		if(et==EventType.MouseUp && isDraggingPoint){
			isDraggingPoint=false;
			EditorUtility.SetDirty(script);
			if(script.pivotType==PS2DPivotType.Auto){
				MovePivot();
			}
		}
	}

	void DrawDeletePoint(int pointID){
		Handles.color=Color.white;
		float size=HandleUtility.GetHandleSize(script.points[pointID].position)*0.1f;
		Handles.DrawWireDisc(script.transform.TransformPoint(script.points[pointID].position),objectPlane.normal,size);
		Handles.DrawLine(
			script.transform.TransformPoint(script.points[pointID].position+((Vector2.up+Vector2.left)*(size*0.5f))),
			script.transform.TransformPoint(script.points[pointID].position+((Vector2.down+Vector2.right)*(size*0.5f)))
		);
		Handles.DrawLine(
			script.transform.TransformPoint(script.points[pointID].position+((Vector2.up+Vector2.right)*(size*0.5f))),
			script.transform.TransformPoint(script.points[pointID].position+((Vector2.down+Vector2.left)*(size*0.5f)))
		);
		if(Vector2.Distance(script.transform.TransformPoint(script.points[pointID].position),GetMouseWorldPosition())<size){
			SetCursor(MouseCursor.ArrowMinus);
		}
		if(Handles.Button(script.transform.TransformPoint(script.points[pointID].position),Quaternion.identity,0,size,Handles.CircleHandleCap)){
			Undo.RecordObject(script,"Delete point");
			DeletePoint(pointID);
			script.UpdateMesh();
			EditorUtility.SetDirty(script);
			if(script.pivotType==PS2DPivotType.Auto){
				MovePivot();
			}
		}
	}

	void DrawAddPointMarker(EventType et){
		bool drawn=false;
		float size=HandleUtility.GetHandleSize(script.transform.position)*0.05f;
		//Get position of cursor in the world
		//The cursor is the point where mouse ray intersects with object's plane
		Ray mRay=HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		float mRayDist;
		if(objectPlane.Raycast(mRay,out mRayDist)){
			Vector3 cursor=mRay.GetPoint(mRayDist);
			//Get closest line
			Vector2 cursorIn=script.transform.InverseTransformPoint(cursor);
			Vector3 marker=Vector3.zero;
			Vector3 newMarker=Vector3.zero;
			int markerPoint=-1;
			for(int i=0;i<script.outlineConnect-(script.outlineLoop?0:1);i++){
				//Get point where perpendicular meets the line
				newMarker=GetBasePoint(
					script.pointsFinal[i],
					script.pointsFinal.Loop(i+1),
					cursorIn
				);
				//If perpendicular doesn't meet the line, take closest end of the line
				if(newMarker==Vector3.zero){
					if(Vector2.Distance(cursorIn,script.pointsFinal[i])<Vector2.Distance(cursorIn,script.pointsFinal.Loop(i+1))){
						newMarker=script.pointsFinal[i];
					} else{
						newMarker=script.pointsFinal.Loop(i+1);
					}
				}
				//Save shortest marker distance
				if(marker==Vector3.zero || Vector3.Distance(cursorIn,newMarker)<Vector3.Distance(cursorIn,marker)){
					markerPoint=i;
					marker=newMarker;
				}
			}
			//Check if cursor is not too close to the point handle
			bool tooclose=false;
			for(int i=0;i<script.points.Count;i++){
				if(Vector3.Distance(script.points[i].position,marker)<size*5){
					tooclose=true;
					break;
				}
			}
			if(!tooclose && Vector3.Distance(cursorIn,marker)<size*2.5){
				marker=script.transform.TransformPoint(marker);
				Handles.color=Color.green;
				Handles.DrawSolidDisc(marker,objectPlane.normal,size);
				Handles.Button(marker,Quaternion.identity,0,size*2,CircleHandleCapSaveID);
				if(et==EventType.Layout) markerID=lastControlID; //Save control's ID if it's a layout event
				if(et==EventType.MouseDown && markerID==GUIUtility.hotControl){
					DeselectAllPoints();
					Undo.RecordObject(script,"Add point");
					//Find after which point we should add a new one by iterating through them
					int pointSum=0;
					int pointAfter=-1;
					for(int i=0;i<script.points.Count;i++){
						if(
							(script.type==PS2DType.Simple && (script.points[i].curve>0f || script.points.Loop(i+1).curve>0f)) || 
							(script.type==PS2DType.Bezier && (script.points[i].pointType!=PS2DPointType.None || script.points.Loop(i+1).pointType!=PS2DPointType.None))
						){
							pointSum+=script.curveIterations;
						}else{
							pointSum++;
						}
						if(markerPoint<pointSum){
							pointAfter=i;
							break;
						}
					}
					AddPoint(script.transform.InverseTransformPoint(marker),pointAfter);
					GUIUtility.hotControl=0;
					SelectPoint(pointAfter+1,true);
					EditorUtility.SetDirty(script);
					script.UpdateMesh();


				}
				Handles.color=Color.white;
				drawn=true;
			}
		}
		if(drawn){
			SetCursor(MouseCursor.ArrowPlus);
		}else{
			markerID=0;
		}
	}

	void DrawBezierControls(int i){
		bool showAllControls=false;
		int drawControls=2;
		float size=HandleUtility.GetHandleSize(script.points[i].position)*0.1f;
		//Simple controls
		if(script.type==PS2DType.Simple){
			//Draw a curve indicator circle
			if(script.points[i].curve>0f){
				if(script.points[i].selected==true) Handles.color=new Color(1,1,0,0.7f);
				else Handles.color=new Color(1,1,0,0.3f);
				Handles.DrawWireDisc(script.transform.TransformPoint(script.points[i].position),objectPlane.normal,size*1.2f+(size*2f)*script.points[i].curve);
				Handles.color=Color.white;
			}
		//Bezier controls
		}else if(script.type==PS2DType.Bezier && script.points[i].pointType!=PS2DPointType.None && (script.points[i].selected || script.points.Loop(i-1).selected || script.points.Loop(i+1).selected || showAllControls)){
			//Bezier controls: Previous handle
			if(script.points.Loop(i-1).selected || script.points[i].selected || showAllControls){
				DrawBezierHandle(i,ref script.points[i].handleP);
				drawControls--;
			}
			//Bezier controls: Next handle
			if(script.points.Loop(i+1).selected || script.points[i].selected || showAllControls){
				DrawBezierHandle(i,ref script.points[i].handleN);
				drawControls--;
			}
			Handles.color=Color.white;
		}
		//Draw non-working controls to always keep same number of controls
		if(drawControls>0){
			Handles.color=Color.white;
			for(int j=0;j<drawControls;j++){
				Handles.FreeMoveHandle(script.transform.TransformPoint(Vector3.zero),Quaternion.identity,0.0f,Vector3.zero,Handles.DotHandleCap);
			}
		}
	}

	void DrawBezierHandle(int i,ref Vector2 handlePosition){
		bool previous=script.points[i].handleP==handlePosition?true:false;
		bool next=script.points[i].handleN==handlePosition?true:false;
		float size=HandleUtility.GetHandleSize(script.points[i].position)*0.1f;
		EventType et=Event.current.type;
		Handles.color=new Color(1,1,1,0.5f);
		Handles.DrawLine(script.transform.TransformPoint(script.points[i].position),script.transform.TransformPoint(handlePosition));
		Handles.color=Color.white;
		Handles.DrawSolidDisc(script.transform.TransformPoint(handlePosition),Vector3.back,size*0.5f);
		Handles.color=Color.clear;
		EditorGUI.BeginChangeCheck();
		Vector3 handlePoint=Handles.FreeMoveHandle(
			script.transform.TransformPoint(handlePosition),
			script.transform.rotation,
			size*0.5f,
			Vector3.zero,
			CircleHandleCapSaveID
		);
		//Assign last control id (retrieved by CircleHandleCapSaveID function)
		//It looks like only within these two events controls are drawn and lastControlID is set
		//Otherwise we just get the last lastControlID each time
		if(et==EventType.Layout || et==EventType.Repaint){
			if(previous){
				script.points[i].controlPID=lastControlID;
			}else if(next){
				script.points[i].controlNID=lastControlID;
			}
		}
		if(EditorGUI.EndChangeCheck()){
			if(!isDraggingPoint) isDraggingPoint=true;
			//Snapping. Setting point's position depending on proximity to the grid
			if((script.points[i].controlPID==GUIUtility.hotControl || script.points[i].controlNID==GUIUtility.hotControl) && isDraggingPoint && Event.current.shift){
				if(script.snapType==PS2DSnapType.Points){
					snap.Reset(size);
					snap.CheckPoint(i,handlePoint,script.transform.TransformPoint(script.points[i].position));
					if(snap.GetClosestAxes()>0){
						handlePoint=snap.snapLocation;
					}
				}else if(script.snapType==PS2DSnapType.WorldGrid || script.snapType==PS2DSnapType.LocalGrid){
					Vector2 pointTr=handlePoint;
					if(script.snapType==PS2DSnapType.LocalGrid) pointTr=script.transform.InverseTransformPoint(pointTr);
					Vector2[] snapPoints=new Vector2[4]{
						new Vector2(Mathf.Floor(pointTr.x/script.gridSize),Mathf.Floor(pointTr.y/script.gridSize))*script.gridSize,
						new Vector2(Mathf.Floor(pointTr.x/script.gridSize),Mathf.Ceil(pointTr.y/script.gridSize))*script.gridSize,
						new Vector2(Mathf.Ceil(pointTr.x/script.gridSize),Mathf.Ceil(pointTr.y/script.gridSize))*script.gridSize,
						new Vector2(Mathf.Ceil(pointTr.x/script.gridSize),Mathf.Floor(pointTr.y/script.gridSize))*script.gridSize
					};
					int snapPoint=-1;
					float closestDistance=script.gridSize;
					float dist;
					for(int j=0;j<snapPoints.Length;j++){
						if(script.snapType==PS2DSnapType.LocalGrid) snapPoints[j]=script.transform.TransformPoint(snapPoints[j]);
						dist=Vector2.Distance(handlePoint,snapPoints[j]);
						if(dist<closestDistance){
							snapPoint=j;
							closestDistance=dist;
						}
					}
					if(snapPoint>-1) handlePoint=snapPoints[snapPoint];
				}
			}
			Undo.RecordObject(script,"Edit point");
			handlePosition=script.transform.InverseTransformPoint(handlePoint);
			//Move other bezier handle accordingly
			if(script.points[i].pointType==PS2DPointType.Rounded){
				if(previous){
					script.points[i].handleN=script.points[i].position-(script.points[i].handleP-script.points[i].position).normalized*(script.points[i].handleN-script.points[i].position).magnitude;
				}else{
					script.points[i].handleP=script.points[i].position-(script.points[i].handleN-script.points[i].position).normalized*(script.points[i].handleP-script.points[i].position).magnitude;
				}
			}
			script.UpdateMesh();
		}
		//Find if mouse is near this handle
		if(Vector2.Distance(script.transform.TransformPoint(handlePosition),GetMouseWorldPosition())<=size*1.5f){
			dontDrawAddPoint=true;
		}
	}

	void DrawCollider(){
		if(script.colliderType!=PS2DColliderType.None && script.colliderType!=PS2DColliderType.MeshStatic && script.colliderType!=PS2DColliderType.MeshDynamic){
			float size=HandleUtility.GetHandleSize(script.transform.position)*0.05f;
			Handles.color=Color.green;
			for(int i=0;i<script.cpointsFinal.Length;i++){
				if(i==0 && script.colliderType==PS2DColliderType.TopEdge) continue;
				Handles.DrawLine(
					script.transform.TransformPoint(script.cpointsFinal.Loop(i-1)),
					script.transform.TransformPoint(script.cpointsFinal[i])
				);
			}
			Handles.color=Color.white;
			//Debug edge normals
			if(script.showNormals){
				for(int i=0;i<script.cpoints.Count;i++){
					if(script.cpoints[i].direction==PS2DDirection.Up) Handles.color=Color.green;
					if(script.cpoints[i].direction==PS2DDirection.Right) Handles.color=Color.magenta;
					if(script.cpoints[i].direction==PS2DDirection.Left) Handles.color=Color.blue;
					if(script.cpoints[i].direction==PS2DDirection.Down) Handles.color=Color.yellow;
					Handles.DrawLine(
						(Vector2)script.transform.TransformPoint(Vector2.Lerp(script.cpoints[i].position,script.cpoints.Loop(i+1).position,0.5f)+(Vector2)script.cpoints[i].normal*(size*1)),
						(Vector2)script.transform.TransformPoint(Vector2.Lerp(script.cpoints[i].position,script.cpoints.Loop(i+1).position,0.5f)+(Vector2)script.cpoints[i].normal*(size*7))
					);
				}
				Handles.color=Color.white;
			}
		}
		if(script.colliderType==PS2DColliderType.MeshStatic || script.colliderType==PS2DColliderType.MeshDynamic){
			Handles.color=Color.green;
			int clength=script.cMesh.vertices.Length/2;
			for(int i=0;i<clength;i++){
				Handles.DrawLine(
					script.transform.TransformPoint(script.cMesh.vertices[i]),
					script.transform.TransformPoint(script.cMesh.vertices[(i==clength-1?0:i+1)])
				);
				Handles.DrawLine(
					script.transform.TransformPoint(script.cMesh.vertices[clength+i]),
					script.transform.TransformPoint(script.cMesh.vertices[((clength+i==clength*2-1)?clength:clength+i+1)])
				);
				Handles.DrawLine(
					script.transform.TransformPoint(script.cMesh.vertices[i]),
					script.transform.TransformPoint(script.cMesh.vertices[clength+i])
				);
			}
			Handles.color=Color.white;
		}
	}

	void DrawGradientControls(){
		//Size of controls
		float size=HandleUtility.GetHandleSize(script.transform.position)*0.1f;
		//Get bigger and smaller dimension of the shape
		float maxSize=Mathf.Max(script.maxPoint.x-script.minPoint.x,script.maxPoint.y-script.minPoint.y);
		float minSize=Mathf.Min(script.maxPoint.x-script.minPoint.x,script.maxPoint.y-script.minPoint.y);
		//Get geometric center of the shape
		Vector2 center=Vector2.LerpUnclamped(script.minPoint,script.maxPoint,0.5f);
		//Get center of the gradient
		Vector2 gCenter=center+(Vector2.up*(maxSize*script.gradientOffset)).Rotate(script.gradientRotation-180f);
		//Get vectors needed to draw gradient rectangle control
		Vector2 gWidth=(Vector2.right*(minSize*0.4f)).Rotate(script.gradientRotation);
		Vector2 gHeight=(Vector2.up*((maxSize*script.gradientScale)/2)).Rotate(script.gradientRotation);
		if(gHeight==Vector2.zero) gHeight=gWidth.Rotate(90).normalized*0.001f;
		//Draw gradient bounds
		Handles.color=new Color(1,1,1,0.5f);
		Handles.DrawLine(
			script.transform.TransformPoint(gCenter-gWidth+gHeight),
			script.transform.TransformPoint(gCenter+gWidth+gHeight)
		);
		Handles.DrawLine(
			script.transform.TransformPoint(gCenter-gWidth-gHeight),
			script.transform.TransformPoint(gCenter+gWidth-gHeight)
		);
		Handles.color=new Color(1,1,1,1f);
		//Move handle
		Handles.color=Color.white;
		Handles.DrawWireDisc(script.transform.TransformPoint(gCenter),objectPlane.normal,size*2f);
		Handles.color=Color.clear;
		EditorGUI.BeginChangeCheck();
		Vector3 pointCenter=Handles.FreeMoveHandle(
			script.transform.TransformPoint(gCenter),
			script.transform.rotation,
			size*2f,
			Vector3.zero,
			Handles.CircleHandleCap
		);
		if(Vector2.Distance(pointCenter,GetMouseWorldPosition())<size*2){
			dontDrawAddPoint=true;
			SetCursor(MouseCursor.MoveArrow);
		}
		if(EditorGUI.EndChangeCheck()){
			GUI.FocusControl(null);
			Undo.RecordObject(script,"Gradient move");
			Vector2 pCenterLocal=(Vector2)script.transform.InverseTransformPoint(pointCenter);
			Vector2 middlePoint=NearestPointOnLine(center,gWidth,pCenterLocal);
			script.gradientOffset=((pCenterLocal-middlePoint).magnitude/maxSize)*Mathf.Sign(-(pCenterLocal-middlePoint).y)*Mathf.Sign(gHeight.y);
			script.gradientOffset=Mathf.Clamp(script.gradientOffset,-1f,1f);
			script.UpdateMaterialSettings();
		}
		//Scale handle
		Handles.color=Color.white;
		Handles.DrawAAConvexPolygon(
			new Vector3[]{
				script.transform.TransformPoint(gCenter-gHeight+(gHeight.normalized+gWidth.normalized)*(size*0.5f)),
				script.transform.TransformPoint(gCenter-gHeight+(-gHeight.normalized+gWidth.normalized)*(size*0.5f)),
				script.transform.TransformPoint(gCenter-gHeight+(-gHeight.normalized-gWidth.normalized)*(size*0.5f)),
				script.transform.TransformPoint(gCenter-gHeight+(gHeight.normalized-gWidth.normalized)*(size*0.5f))
			}
		);
		Handles.color=Color.clear;
		EditorGUI.BeginChangeCheck();
		Vector3 pointScale=Handles.FreeMoveHandle(
			script.transform.TransformPoint(gCenter-gHeight),
			script.transform.rotation,
			size*0.5f,
			Vector3.zero,
			Handles.CircleHandleCap
		);
		if(Vector2.Distance(pointScale,GetMouseWorldPosition())<size){
			dontDrawAddPoint=true;
			float a=Vector2Extension.SignedAngle(Vector2.right,-gHeight);
			if(Mathf.Abs(a)<22.5f) SetCursor(MouseCursor.ResizeHorizontal);
			else if(Mathf.Abs(a)<67.5f) SetCursor(Math.Sign(a)>0?MouseCursor.ResizeUpRight:MouseCursor.ResizeUpLeft);
			else if(Mathf.Abs(a)<112.5f) SetCursor(MouseCursor.ResizeVertical);
			else if(Mathf.Abs(a)<157.5f)  SetCursor(Math.Sign(a)>0?MouseCursor.ResizeUpLeft:MouseCursor.ResizeUpRight);
			else SetCursor(MouseCursor.ResizeHorizontal);
		}
		if(EditorGUI.EndChangeCheck()){
			GUI.FocusControl(null);
			Undo.RecordObject(script,"Gradient scale");
			script.gradientScale=(((Vector2)script.transform.InverseTransformPoint(pointScale)-NearestPointOnLine(gCenter,gWidth,script.transform.InverseTransformPoint(pointScale))).magnitude/maxSize)*2;
			if(script.gradientScale>2f)script.gradientScale=2f;
			script.UpdateMaterialSettings();
		}
		//Rotation handle
		Handles.color=Color.white;
		Handles.DrawSolidDisc(script.transform.TransformPoint(gCenter+gHeight+gWidth),objectPlane.normal,size*0.2f);
		Handles.DrawWireDisc(script.transform.TransformPoint(gCenter+gHeight+gWidth),objectPlane.normal,size*0.6f);
		Handles.color=Color.clear;
		EditorGUI.BeginChangeCheck();
		Vector3 pointRotation=Handles.FreeMoveHandle(
			script.transform.TransformPoint(gCenter+gHeight+gWidth),
			script.transform.rotation,
			size*0.6f,
			Vector3.zero,
			Handles.CircleHandleCap
		);
		if(Vector2.Distance(pointRotation,GetMouseWorldPosition())<=size){
			dontDrawAddPoint=true;
			SetCursor(MouseCursor.RotateArrow);
		}
		if(EditorGUI.EndChangeCheck()){
			GUI.FocusControl(null);
			Undo.RecordObject(script,"Gradient rotate");
			float angle=Vector2Extension.SignedAngle(
				gCenter+gHeight+gWidth-center,
				gHeight
			);
			script.gradientRotation=360f-90f+angle-Vector2Extension.SignedAngle(
				(Vector2)script.transform.InverseTransformPoint(pointRotation)-center,
				Vector2.right
			);
			while(script.gradientRotation<-180f) script.gradientRotation+=360f;
			while(script.gradientRotation>180f) script.gradientRotation-=360f;
			script.UpdateMaterialSettings();
		}
	}

	void DrawTextureControls(){
		//Size of controls
		float size=HandleUtility.GetHandleSize(script.transform.position)*0.1f;
		//Calculate center and bounds based on texture size, scaling, rotation and offset
		Vector2 tCenter=(Vector2)script.transform.position+new Vector2(script.textureOffset.x*((float)script.texture.width/100f),(float)script.textureOffset.y*(script.texture.height/100f))*script.textureScale;
		Vector2 tWidth=(Vector2.right*(script.texture.width/200f)*script.textureScale).Rotate(script.textureRotation);
		Vector2 tHeight=(Vector2.up*(script.texture.height/200f)*script.textureScale).Rotate(script.textureRotation);
		//Draw texture outline
		Handles.color=Color.white;
		Handles.DrawDottedLines(new Vector3[]{
				(Vector3)(tCenter-tWidth+tHeight),(Vector3)(tCenter+tWidth+tHeight),
				(Vector3)(tCenter+tWidth+tHeight),(Vector3)(tCenter+tWidth-tHeight),
				(Vector3)(tCenter+tWidth-tHeight),(Vector3)(tCenter-tWidth-tHeight),
				(Vector3)(tCenter-tWidth-tHeight),(Vector3)(tCenter-tWidth+tHeight)
			},
			0.1f/size
		);
		//Move handle
		float handleSize=((float)Mathf.Min(script.texture.width,script.texture.height)*script.textureScale)/400f;
		Handles.color=Color.white;
		Handles.DrawWireDisc(tCenter,objectPlane.normal,handleSize);
		Handles.color=Color.clear;
		EditorGUI.BeginChangeCheck();
		Vector3 pointCenter=Handles.FreeMoveHandle(
			tCenter,
			script.transform.rotation,
			handleSize,
			Vector3.zero,
			Handles.CircleHandleCap
		);
		if(Vector2.Distance(pointCenter,GetMouseWorldPosition())<handleSize){
			dontDrawAddPoint=true;
			SetCursor(MouseCursor.MoveArrow);
		}
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(script,"Texture move");
			script.textureOffset=Vector2.Scale(
				(Vector2)(pointCenter-script.transform.position),
				new Vector2(
					1f/((script.texture.width/100f)*script.textureScale),
					1f/((script.texture.height/100f)*script.textureScale)
				)
			);
			script.textureOffset.x=Mathf.Clamp(script.textureOffset.x,-1f,1f);
			script.textureOffset.y=Mathf.Clamp(script.textureOffset.y,-1f,1f);
			script.UpdateMaterialSettings();
		}
		//Scale handle
		Handles.color=Color.white;
		Handles.DrawAAConvexPolygon(
			new Vector3[]{
				tCenter-tHeight+tWidth+(tHeight.normalized+tWidth.normalized)*(size*0.5f),
				tCenter-tHeight+tWidth+(-tHeight.normalized+tWidth.normalized)*(size*0.5f),
				tCenter-tHeight+tWidth+(-tHeight.normalized-tWidth.normalized)*(size*0.5f),
				tCenter-tHeight+tWidth+(tHeight.normalized-tWidth.normalized)*(size*0.5f)
			}
		);
		Handles.color=Color.clear;
		EditorGUI.BeginChangeCheck();
		Vector3 pointScale=Handles.FreeMoveHandle(
			tCenter-tHeight+tWidth,
			script.transform.rotation,
			size*0.5f,
			Vector3.zero,
			Handles.CircleHandleCap
		);
		if(Vector2.Distance(pointScale,GetMouseWorldPosition())<size){
			dontDrawAddPoint=true;
			float a=Vector2Extension.SignedAngle(Vector2.right,-tHeight+tWidth);
			if(Mathf.Abs(a)<22.5f) SetCursor(MouseCursor.ResizeHorizontal);
			else if(Mathf.Abs(a)<67.5f) SetCursor(Math.Sign(a)>0?MouseCursor.ResizeUpRight:MouseCursor.ResizeUpLeft);
			else if(Mathf.Abs(a)<112.5f) SetCursor(MouseCursor.ResizeVertical);
			else if(Mathf.Abs(a)<157.5f)  SetCursor(Math.Sign(a)>0?MouseCursor.ResizeUpLeft:MouseCursor.ResizeUpRight);
			else SetCursor(MouseCursor.ResizeHorizontal);
		}
		if(EditorGUI.EndChangeCheck()){
			GUI.FocusControl(null);
			Undo.RecordObject(script,"Texture scale");
			script.textureScale=Vector2.Distance(tCenter,pointScale)/Vector2.Distance(Vector2.zero,new Vector2(script.texture.width/200f,script.texture.height/200f));
			script.UpdateMaterialSettings();
		}
		//Rotation handle
		Handles.color=Color.white;
		Handles.DrawSolidDisc(tCenter+tHeight+tWidth,objectPlane.normal,size*0.2f);
		Handles.DrawWireDisc(tCenter+tHeight+tWidth,objectPlane.normal,size*0.6f);
		Handles.color=Color.clear;
		EditorGUI.BeginChangeCheck();
		Vector3 pointRotation=Handles.FreeMoveHandle(
			tCenter+tHeight+tWidth,
			script.transform.rotation,
			size*0.6f,
			Vector3.zero,
			Handles.CircleHandleCap
		);
		if(Vector2.Distance(pointRotation,GetMouseWorldPosition())<=size){
			dontDrawAddPoint=true;
			SetCursor(MouseCursor.RotateArrow);
		}
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(script,"Ttexture rotate");
			float angle=Vector2Extension.SignedAngle(tHeight+tWidth,tHeight);
			script.textureRotation=360f-90f+angle-Vector2Extension.SignedAngle((Vector2)pointRotation-tCenter,Vector2.right);
			while(script.textureRotation<-180f) script.textureRotation+=360f;
			while(script.textureRotation>180f) script.textureRotation-=360f;
			script.UpdateMaterialSettings();
		}
	}

	void DrawGrid(){
		Handles.color=new Color(1f,1f,1f,0.3f);
		//Get start and end points for the grid
		Vector2 start=script.transform.TransformPoint(script.minPoint);
		Vector2 end=script.transform.TransformPoint(script.maxPoint);

		/*
		Handles.DrawSolidDisc(start,Vector3.back,0.1f);
		Handles.DrawSolidDisc(end,Vector3.back,0.1f);
		*/

		//If local grid is selected, we transform to local coordinates
		if(script.snapType==PS2DSnapType.LocalGrid){
			start=script.transform.InverseTransformPoint(start);
			end=script.transform.InverseTransformPoint(end);
		}
		//Round start and end points to be on the grid
		start.x=Mathf.Floor(start.x/script.gridSize)*script.gridSize;
		start.y=Mathf.Floor(start.y/script.gridSize)*script.gridSize;
		end.x=Mathf.Ceil(end.x/script.gridSize)*script.gridSize;
		end.y=Mathf.Ceil(end.y/script.gridSize)*script.gridSize;
		//Now that we've rounded the coordinates, if it's a local grid, we convert them back to global
		if(script.snapType==PS2DSnapType.LocalGrid){
			start=script.transform.TransformPoint(start);
			end=script.transform.TransformPoint(end);
		}

		/*
		Handles.color=Color.green;
		Handles.DrawSolidDisc(start,Vector3.back,0.1f);
		Handles.color=Color.red;
		Handles.DrawSolidDisc(end,Vector3.back,0.1f);
		Handles.color=new Color(1f,1f,1f,0.3f);
		*/

		//How many lines to draw
		int linesX=(int)Mathf.Round((end.x-start.x)/script.gridSize);
		int linesY=(int)Mathf.Round((end.y-start.y)/script.gridSize);
		//Drow vertical lines
		for(int i=0;i<=linesX;i++){
			Handles.DrawDottedLine(
				new Vector3(start.x+i*script.gridSize,start.y-script.gridSize,0f),
				new Vector3(start.x+i*script.gridSize,end.y+script.gridSize,0f),
				1f
			);
		}
		//Draw horyzontal lines
		for(int i=0;i<=linesY;i++){
			Handles.DrawDottedLine(
				new Vector3(start.x-script.gridSize,start.y+i*script.gridSize,0f),
				new Vector3(end.x+script.gridSize,start.y+i*script.gridSize,0f),
				1f
			);
		}
		Handles.color=Color.white;
	}

    #endregion

    #region Point properties window

    void DrawPointsProperties(){
		EventType et=Event.current.type;
		string selected="";
		List<int> selPoints=new List<int>();
		for(int i=0;i<script.points.Count;i++){
			if(script.points[i].selected){
				if(selected.Length>0) selected+=",";
				selected+=" "+i.ToString();
				selPoints.Add(i);
			}
		}
		//EditorWindow
		if(selPoints.Count>0 && et!=EventType.Repaint){
			windowRect=new Rect(
				Camera.current.pixelRect.width/EditorGUIUtility.pixelsPerPoint-143,
				Camera.current.pixelRect.height/EditorGUIUtility.pixelsPerPoint-45,
				142,
				60
			);
			int cid=GUIUtility.GetControlID(FocusType.Passive);
			GUILayout.Window(
				cid,
				windowRect,
				(id)=>{
					//Working with temporary vars for undo
					Vector2 pos=script.points[selPoints[0]].position;
					float curve=script.points[selPoints[0]].curve;
					//Define the window
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth=20;
					pos.x=EditorGUILayout.FloatField("X",pos.x,GUILayout.Width(64));
					pos.y=EditorGUILayout.FloatField("Y",pos.y,GUILayout.Width(64));
					EditorGUILayout.EndHorizontal();
					//Bezier types switch
					PS2DPointType type=script.points[selPoints[0]].pointType;
					bool updateType=false;
					bool updateCurve=false;
					bool autoHandles=false;
					bool straightenHandles=false;
					//See if all points have same curve radius and/or curve type
					bool allPointsHaveSameCurve=true;
					bool allPointsHaveSameType=true;
					for(int i=0;i<selPoints.Count;i++){
						if(script.points[selPoints[i]].pointType!=script.points[selPoints.Loop(i+1)].pointType && allPointsHaveSameType){
							allPointsHaveSameType=false;
						}
						if(script.points[selPoints[i]].curve!=script.points[selPoints.Loop(i+1)].curve && allPointsHaveSameCurve){
							allPointsHaveSameCurve=false;
						}
					}
					if(script.type==PS2DType.Simple){
						EditorGUI.showMixedValue=!allPointsHaveSameCurve;
						float _curve=EditorGUILayout.Slider(curve,0f,1f);
						if(_curve!=curve){
							curve=_curve;
							updateCurve=true;
						}
						EditorGUI.showMixedValue=false;
					}else if(script.type==PS2DType.Bezier){
						EditorGUILayout.BeginHorizontal();
						if(GUILayout.Toggle((type==PS2DPointType.None && allPointsHaveSameType),(Texture)Resources.Load("Icons/bezierNone"),"Button")){
							curve=0f;
							updateCurve=true;
							type=PS2DPointType.None;
							updateType=true;
						}
						if(GUILayout.Toggle((type==PS2DPointType.Sharp && allPointsHaveSameType),(Texture)Resources.Load("Icons/bezierSharp"),"Button")){
							if(curve==0){
								curve=0.5f;
								updateCurve=true;
								autoHandles=true;
							}
							type=PS2DPointType.Sharp;
							updateType=true;
						}
						if(GUILayout.Toggle((type==PS2DPointType.Rounded && allPointsHaveSameType),(Texture)Resources.Load("Icons/bezierRounded"),"Button")){
							if(curve==0){
								curve=0.5f;
								updateCurve=true;
								autoHandles=true;
							}else if(script.points[selPoints[0]].pointType==PS2DPointType.Sharp){
								straightenHandles=true;
							}
							type=PS2DPointType.Rounded;
							updateType=true;
						}
						EditorGUILayout.EndHorizontal();
					}
					//React to change
					if(GUI.changed){
						Undo.RecordObject(script,"Edit point");
						//Calculate the movement
						Vector2 move=pos-script.points[selPoints[0]].position;
						//Update each selected point
						for(int i=0;i<selPoints.Count;i++){
							script.points[selPoints[i]].Move(move,script.type==PS2DType.Bezier?true:false);
							if(updateCurve){
								script.points[selPoints[i]].curve=Mathf.Round(curve*100f)/100;
								if(script.points[selPoints[i]].curve==0){
									script.points[selPoints[i]].handleN=script.points[selPoints[i]].position;
									script.points[selPoints[i]].handleP=script.points[selPoints[i]].position;
								}
							}
							if(updateType) script.points[selPoints[i]].pointType=type;
							if(autoHandles) script.GenerateHandles(selPoints[i]);
							if(straightenHandles) script.StraightenHandles(selPoints[i]);
						}
						script.UpdateMesh();
						EditorUtility.SetDirty(script);
					}
					//Make window dragable but don't actually allow to drag it
					//It's a hack so window wouldn't disappear on click
					if(Event.current.type!=EventType.MouseDrag) GUI.DragWindow();
				},
				"Point"+selected,
				new GUIStyle(GUI.skin.window),
				GUILayout.MinWidth(windowRect.width),
				GUILayout.MaxWidth(windowRect.width),
				GUILayout.MinHeight(windowRect.height),
				GUILayout.MaxHeight(windowRect.height)
			);
			GUI.FocusWindow(cid);
		}
	}

    #endregion

    #region Add, remove, select, move points

    void AddPoint(Vector2 pos,int after=-1){
		if(after==-1 || after==script.points.Count-1){
			after=script.points.Count-1;
			script.points.Add(new PS2DPoint(pos));
		}else{
			script.points.Insert(after+1,new PS2DPoint(pos));
		}
		//Give all points new names
		for(int i=0;i<script.points.Count;i++){
			script.points[i].name="point"+script.uniqueName+"_"+i.ToString();
		}
		//Set curve value based on neghboring points
		script.points.Loop(after+1).curve=Mathf.Lerp(script.points.Loop(after).curve,script.points.Loop(after+2).curve,0.5f)*0.5f;
	}

	void DeletePoint(int at){
		script.points.RemoveAt(at);
	}

	void DeselectAllPoints(){
		for(int i=0;i<script.points.Count;i++){
			script.points[i].selected=false;
		}
		Repaint();
	}

	void SelectPoint(int i,bool state){
		if(script.points[i].selected!=state){
			script.points[i].selected=state;
			Repaint();
		}
	}

	//Move pivot of the object
	//To do this, we rearrange all points around the new pivot and then just move the object
	private void MovePivot(){
		//Get min and max positions
		Vector2 min=Vector2.one*Mathf.Infinity;
		Vector2 max=-Vector2.one*9999f;
		for(int i=0;i<script.pointsFinal.Count;i++){
			if(script.pointsFinal[i].x<min.x) min.x=script.pointsFinal[i].x;
			if(script.pointsFinal[i].y<min.y) min.y=script.pointsFinal[i].y;
			if(script.pointsFinal[i].x>max.x) max.x=script.pointsFinal[i].x;
			if(script.pointsFinal[i].y>max.y) max.y=script.pointsFinal[i].y;
		}
		//Calculate the difference
		Vector2 newPivot=new Vector2();
		if(script.PivotPosition==PS2DPivotPosition.Center) newPivot=Vector2.Lerp(min,max,0.5f);
		if(script.PivotPosition==PS2DPivotPosition.Top) newPivot=new Vector2(Mathf.Lerp(min.x,max.x,0.5f),max.y);
		if(script.PivotPosition==PS2DPivotPosition.Right) newPivot=new Vector2(max.x,Mathf.Lerp(min.y,max.y,0.5f));
		if(script.PivotPosition==PS2DPivotPosition.Bottom) newPivot=new Vector2(Mathf.Lerp(min.x,max.x,0.5f),min.y);
		if(script.PivotPosition==PS2DPivotPosition.Left) newPivot=new Vector2(min.x,Mathf.Lerp(min.y,max.y,0.5f));
		//Do the moving
		MovePivot(newPivot);
	}

	private void MovePivot(Vector2 newPivot){
		//Difference between projected and real pivots converted to lcoal scale
		Vector2 diff=newPivot-(Vector2)script.transform.InverseTransformPoint((Vector2)script.transform.position);
		//To record full state we need to use RegisterFullObjectHierarchyUndo
		Undo.RegisterFullObjectHierarchyUndo(script,"Moving pivot");
		//Use it to move the points and children
		for(int i=0;i<script.points.Count;i++){
			script.points[i].Move(-diff,true);
		}
		if(script.transform.childCount>0){
			for(int i=0;i<script.transform.childCount;i++){
				script.transform.GetChild(i).transform.localPosition-=(Vector3)diff;
			}
		}
		//Convert projected pivot to world coordinates and move object to it
		Vector2 projectedPivotWorld=script.transform.TransformPoint(newPivot);
		script.transform.position=new Vector3(projectedPivotWorld.x,projectedPivotWorld.y,script.transform.position.z);
		script.UpdateMesh();
		//For undo
		EditorUtility.SetDirty(script);
	}

    #endregion

    #region Helper methods

	/*
	public void ArrayAppend<T>(ref T[] source,T[] append){
		int sourceLength=source.Length;
		Array.Resize(ref source,source.Length+append.Length);
		Array.Copy(append,0,source,sourceLength,append.Length);
	}

	public void ArrayRemoveAt<T>(ref T[] source,int index){ 
		T[] replace=new T[source.Length-1];
		if(index>0) Array.Copy(source,0,replace,0,index);
		if(index<source.Length-1) Array.Copy(source,index+1,replace,index,source.Length-index-1);
		source=replace;
	}

	public void AddStyles(ref GUISkin skin,GUIStyle[] styles){
		GUIStyle[] customStyles=skin.customStyles;
		ArrayAppend(ref customStyles,styles);
		skin.customStyles=customStyles;
	}

	public void OverrideStyle(ref GUISkin skin,string name,GUIStyle style){
		for(int i=0;i<skin.customStyles.Length;i++){ 
			if(skin.customStyles[i].name==name){ 
				skin.customStyles[i]=style;
				break;
			}
		}
	}

	public void RemoveStyle(ref GUISkin skin,string name){
		int deleteAt=-1;
		for(int i=0;i<skin.customStyles.Length;i++){ 
			if(skin.customStyles[i].name==name){ 
				deleteAt=i;
				break;
			}
		}
		if(deleteAt>0){ 
			GUIStyle[] customStyles=skin.customStyles;
			ArrayRemoveAt(ref customStyles,deleteAt);
			skin.customStyles=customStyles;
		}
	}
	*/

	void ExportMesh(){
		script.UpdateMesh();
		Mesh mesh=script.GetMesh();
		if(System.IO.File.Exists("Assets/"+mesh.name.ToString()+".asset") && !EditorUtility.DisplayDialog("Warning","Asset with this name already exists in root of your project.","Overwrite","Cancel")){
			return;
		}
		AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(mesh),"Assets/"+mesh.name.ToString()+".asset");
		AssetDatabase.SaveAssets();
	}

	void ExportPNG(){
		script.UpdateMesh();
		//Move current object to the root of the scene
		Transform sparent=script.transform.parent;
		script.transform.parent=null;
		//Disable all root game objects except the current one and main camera
		GameObject[] rootList=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
		List<GameObject> disableList=new List<GameObject>(50);
		for(int i=0;i<rootList.Length;i++){
			if(rootList[i].activeSelf && rootList[i]!=script.gameObject){
				disableList.Add(rootList[i]);
				rootList[i].SetActive(false);
			}
		}
		//Create the temporary camera
		GameObject scameraGO=new GameObject();
		scameraGO.name="Screenshot Camera";
		Camera scamera=scameraGO.AddComponent<Camera>();
		scamera.cameraType=CameraType.Game;
		scamera.orthographic=true;
		scamera.enabled=false;
		scamera.clearFlags=CameraClearFlags.Color;
		//Center camera on the object and set the size
		Bounds meshBounds=script.GetComponent<MeshRenderer>().bounds;
		scameraGO.transform.position=meshBounds.center+Vector3.back;
		scamera.orthographicSize=Mathf.Max(meshBounds.size.x/2,meshBounds.size.y);
		//Crete render texture with antialiasing
		RenderTexture rt=new RenderTexture((int)(meshBounds.size.x*200),(int)(meshBounds.size.y*200),0);
		rt.antiAliasing=8;
		rt.autoGenerateMips=false;
		scamera.targetTexture=rt;
		RenderTexture.active=scamera.targetTexture;
		//Render image with white background
		scamera.backgroundColor=Color.white;
		scamera.Render();
		Texture2D imageWhite=new Texture2D(scamera.targetTexture.width,scamera.targetTexture.height,TextureFormat.RGB24,false);
		imageWhite.ReadPixels(new Rect(0,0,scamera.targetTexture.width,scamera.targetTexture.height),0,0);
		//File.WriteAllBytes(Application.dataPath + "/_TESTW.png",imageWhite.EncodeToPNG());
		//Render image with black background
		scamera.backgroundColor=Color.black;
		scamera.Render();
		Texture2D imageBlack=new Texture2D(scamera.targetTexture.width,scamera.targetTexture.height,TextureFormat.RGB24,false);
		imageBlack.ReadPixels(new Rect(0,0,scamera.targetTexture.width,scamera.targetTexture.height),0,0);
		//File.WriteAllBytes(Application.dataPath + "/_TESTB.png",imageBlack.EncodeToPNG());
		//Create image with alpha by comparing black and white bg images
		Texture2D imageTrans=new Texture2D(scamera.targetTexture.width,scamera.targetTexture.height,TextureFormat.RGBA32,false);
		Color color;
		for(int y=0;y<imageTrans.height;++y){
			for(int x=0;x<imageTrans.width;++x){
				float alpha=imageWhite.GetPixel(x,y).r-imageBlack.GetPixel(x,y).r;
				alpha=1.0f-alpha;
				if(alpha==0){
					color=Color.clear;
				}else{
					color=imageBlack.GetPixel(x,y)/alpha;
				}
				color.a=alpha;
				imageTrans.SetPixel(x,y,color);
			}
		}
		//File.WriteAllBytes(Application.dataPath + "/_TEST_TRANS.png",imageTrans.EncodeToPNG());
		//Crop excessive transparent color from image
		Texture2D cropImage=CropImageColor(imageTrans,Color.clear);
		//Come up with a unique name for an image
		string filename;
		int iterator=0;
		do{
			filename=Application.dataPath + "/"+script.name+(iterator>0?" ("+iterator.ToString()+")":"")+".png";
			iterator++;
		}while(File.Exists(filename));
		//Save the image to PNG
		File.WriteAllBytes(filename,cropImage.EncodeToPNG());
		//Return thing to their original state
		RenderTexture.active=null;
		DestroyImmediate(scameraGO);
		AssetDatabase.Refresh();
		//Enable the objects we disabled previously
		for(int i=0;i<disableList.Count;i++){
			disableList[i].SetActive(true);
		}
		//Return object to its original parent
		script.transform.parent=sparent;
	}

	private Texture2D CropImageColor(Texture2D image,Color cropColor){
		int[] cropRect=new int[]{0,0,image.width,image.height};
		bool[] cropSet=new bool[]{false,false,false,false};
		//Find all the transparent area we can crop
		for(int x=0;x<image.width;x++){
			for(int y=0;y<image.height;y++){
				if(image.GetPixel(x,y)!=cropColor){
					if(!cropSet[0] || x<cropRect[0]){
						cropRect[0]=x;
						cropSet[0]=true;
					}
					if(!cropSet[1] || y<cropRect[1]){
						cropRect[1]=y;
						cropSet[1]=true;
					}
					if(cropSet[0] && (!cropSet[2] || x+1>cropRect[2])){
						cropRect[2]=x+1;
						cropSet[2]=true;
					}
					if(cropSet[1] && (!cropSet[3] || y+1>cropRect[3])){
						cropRect[3]=y+1;
						cropSet[3]=true;
					}
				}
			}
		}
		//Crop out all the transparent area
		Texture2D cropImage=new Texture2D(cropRect[2]-cropRect[0],cropRect[3]-cropRect[1],TextureFormat.RGBA32,false);
		cropImage.SetPixels(image.GetPixels(cropRect[0],cropRect[1],cropRect[2]-cropRect[0],cropRect[3]-cropRect[1]));
		return cropImage;
	}

	void AddCollider(){
		if(script.colliderType==PS2DColliderType.PolygonStatic){
			script.gameObject.AddComponent<Rigidbody2D>();
			script.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
			script.gameObject.AddComponent<PolygonCollider2D>();
		}else if(script.colliderType==PS2DColliderType.PolygonDynamic){
			script.gameObject.AddComponent<Rigidbody2D>();
			script.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Dynamic;
			script.gameObject.AddComponent<PolygonCollider2D>();
		}else if(script.colliderType==PS2DColliderType.Edge){
			script.gameObject.AddComponent<Rigidbody2D>();
			script.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
			script.gameObject.AddComponent<EdgeCollider2D>();
		}else if(script.colliderType==PS2DColliderType.TopEdge){
			script.gameObject.AddComponent<Rigidbody2D>();
			script.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
			script.gameObject.AddComponent<EdgeCollider2D>();
			script.GetComponent<EdgeCollider2D>().usedByEffector=true;
			script.gameObject.AddComponent<PlatformEffector2D>();
			script.GetComponent<PlatformEffector2D>().surfaceArc=90f;
		}else if(script.colliderType==PS2DColliderType.MeshStatic){
			script.gameObject.AddComponent<MeshCollider>();
		}else if(script.colliderType==PS2DColliderType.MeshDynamic){
			script.gameObject.AddComponent<Rigidbody>();
			script.gameObject.AddComponent<MeshCollider>();
			script.GetComponent<MeshCollider>().convex=true;
		}
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<PolygonCollider2D>(),false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<EdgeCollider2D>(),false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<Rigidbody2D>(),false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<MeshCollider>(),false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<Rigidbody>(),false);
	}

	bool RemoveCollider(PS2DColliderType nextType){
		bool ok=true;
		if(script.GetComponent<Collider2D>()!=null || script.GetComponent<MeshCollider>()){
			if(nextType==PS2DColliderType.None){
				ok=EditorUtility.DisplayDialog("Warning","This will remove existing Collider and RigidBody with all their settings.","Remove","Keep existing collider");
			}else{
				ok=EditorUtility.DisplayDialog("Warning","This will remove existing Collider and RigidBody with all their settings and create new Collider and RigidBody in their place.","Overwrite","Keep existing collider");
			}
			if(ok){
				while(script.GetComponent<Collider2D>()!=null){
					DestroyImmediate(script.GetComponent<Collider2D>());
				}
				while(script.GetComponent<MeshCollider>()!=null){
					DestroyImmediate(script.GetComponent<MeshCollider>());
				}
				while(script.GetComponent<PlatformEffector2D>()!=null){
					DestroyImmediate(script.GetComponent<PlatformEffector2D>());
				}
				while(script.GetComponent<Rigidbody2D>()!=null){
					DestroyImmediate(script.GetComponent<Rigidbody2D>());
				}
				while(script.GetComponent<Rigidbody>()!=null){
					DestroyImmediate(script.GetComponent<Rigidbody>());
				}
			}
		}
		return ok;
	}

    //This gets position of that green cursor for creating points
    private Vector2 GetBasePoint(Vector2 b1,Vector2 b2, Vector2 t,float sizeCap=0f){
		float d1=Vector2.Distance(b1,t);
		float d2=Vector2.Distance(b2,t);
		float db=Vector2.Distance(b1,b2);
		//Find one of the angles
		float angle1=Mathf.Acos((Mathf.Pow(d1,2)+Mathf.Pow(db,2)-Mathf.Pow(d2,2))/(2*d1*db));
		//Find distance to point
		float dist=Mathf.Cos(angle1)*d1;
		//Make sure it's within the line
		if(dist<sizeCap || dist>db-sizeCap) return Vector2.zero;
		else return (b1+(dist*(b2-b1).normalized));
	}

	//Find a point on an infinite line. Same as above but with infinite line
	public static Vector2 NearestPointOnLine(Vector2 lineStart,Vector2 lineDirection,Vector2 point){
		lineDirection.Normalize();
		return lineStart+lineDirection*Vector2.Dot(lineDirection,point-lineStart);
	}

	//Generate a random mild color
	private Color RandomColor(){
		float hue=UnityEngine.Random.Range(0f,1f);
		while(hue*360f>=236f && hue*360f<=246f){
			hue=UnityEngine.Random.Range(0f,1f);
		}
		return Color.HSVToRGB(hue,UnityEngine.Random.Range(0.2f,0.7f),UnityEngine.Random.Range(0.8f,1f));
	}

	//Convert any enum to array of GUIContent
	GUIContent[] EnumToGUI<K>(){ 
		if(typeof(K).BaseType!=typeof(Enum)) throw new InvalidCastException();
		string[] strings=Enum.GetNames(typeof(K));
		GUIContent[] buttons=new GUIContent[strings.Length];
		for(int i=0;i<buttons.Length;i++){
			buttons[i]=new GUIContent(strings[i]);
		}
		return buttons;
	}

	//Convert any enum to array of GUIContent with images and tooltips, given a path to images
	GUIContent[] EnumToGUI<K>(string resourcePrefix){
		if(typeof(K).BaseType!=typeof(Enum)) throw new InvalidCastException();
		string[] strings=Enum.GetNames(typeof(K));
		GUIContent[] buttons=new GUIContent[strings.Length];
		for(int i=0;i<buttons.Length;i++){
			buttons[i]=new GUIContent((Texture)Resources.Load(resourcePrefix+strings[i]),strings[i]);
		}
		return buttons;
	}

	//Get the sorting layer IDs
	public int[] GetSortingLayerUniqueIDs() {
		Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty=internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",BindingFlags.Static|BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null,new object[0]);
	}

	//Get the sorting layer names
	public string[] GetSortingLayerNames(){
		Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty=internalEditorUtilityType.GetProperty("sortingLayerNames",BindingFlags.Static|BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null,new object[0]);
	}

	//Wrapper function for CircleHandleCap that stores lastControlID
	public void CircleHandleCapSaveID(int controlID,Vector3 position,Quaternion rotation,float size,EventType et){
		lastControlID=controlID;
		Handles.CircleHandleCap(controlID,position,rotation,size,et);
	}

	//Get mouse position
	Vector2 GetMouseWorldPosition(){
		Ray mRay=HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		float mRayDist;
		if(objectPlane.Raycast(mRay,out mRayDist)) return (Vector2)mRay.GetPoint(mRayDist);
		return Vector2.zero;
	}

	//Set cursor
	void SetCursor(MouseCursor cursor){
		EditorGUIUtility.AddCursorRect(Camera.current.pixelRect,cursor);
	}

    #endregion

}