using UnityEngine;

[System.Serializable]
public class PS2DPoint:System.Object{
	public Vector2 position=Vector2.zero;
	public float curve=0f;
	public string name;
	public bool selected=false;
	public bool clockwise=true;
	public Vector2 median=Vector2.zero;
	public Vector2 handleP=Vector2.zero;
	public Vector2 handleN=Vector2.zero;
	//IDs if point handle and bezier handles
	public int controlID=0;
	public int controlPID=0;
	public int controlNID=0;
	public PS2DPointType pointType=PS2DPointType.None;
	public PS2DPoint(Vector2 position,string name=""){
		this.position=position;
		this.name=name;
		this.handleP=position;
		this.handleN=position;
	}
	public void Move(Vector2 diff,bool moveHandles){
		position+=diff;
		//This is for when handles aren't calculaed automatically
		if(moveHandles){
			handleN+=diff;
			handleP+=diff;
		}
	}
}
