using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PS2DSnap{
	private float snapDistance=1f;
	private List<PS2DSnapAxis> axes=new List<PS2DSnapAxis>(4);
	public Vector2 snapLocation;
	public int snapPoint1;
	public int snapPoint2;
	private int snapAxis1;
	private int snapAxis2;
	public PS2DSnap(){
		axes.Add(new PS2DSnapAxis(Vector2.right)); //X axis
		axes.Add(new PS2DSnapAxis(Vector2.up)); //Y axis
		axes.Add(new PS2DSnapAxis(Vector2.right+Vector2.up)); //Diagonal X axis rotated to 45 degrees clockwise
		axes.Add(new PS2DSnapAxis(Vector2.left+Vector2.up)); //Diagonal axis Y rotated to 45 degrees clockwise
		snapPoint1=-1;
		snapPoint2=-1;
	}
	public void Reset(float size){
		for(int i=0;i<axes.Count;i++){
			axes[i].Reset(snapDistance,size);
		}
	}
	//Find closest points on all axes
	public void CheckPoint(int pointId,Vector2 pointDragging,Vector2 pointStatic){
		for(int i=0;i<axes.Count;i++){
			axes[i].CheckPoint(pointId,pointDragging,pointStatic);
		}
	}
	//Return up to 2 axes to snap to
	public int GetClosestAxes(){
		snapAxis1=-1;
		snapAxis2=-1;
		//Find a closest axis
		float closest=-5f;
		for(int i=0;i<axes.Count;i++){
			if(axes[i].snapPoint!=-1 && (closest<-1 || axes[i].snapDist<closest)){
				snapAxis1=i;
				closest=axes[i].snapDist;
			}
		}
		//If found, find a second closest axis
		if(snapAxis1!=-1){
			closest=-5f;
			for(int i=0;i<axes.Count;i++){
				if(i!=snapAxis1 && (axes[i].snapPoint!=-1 && (closest<-1 || axes[i].snapDist<closest))){
					snapAxis2=i;
					closest=axes[i].snapDist;
				}
			}
		}
		//Getting the snap point
		if(snapAxis2>-1){
			snapLocation=axes[snapAxis1].GetIntersection(axes[snapAxis2]);
			snapPoint1=axes[snapAxis1].snapPoint;
			snapPoint2=axes[snapAxis2].snapPoint;
			return 2;
		}else if(snapAxis1>-1){
			snapLocation=axes[snapAxis1].baseLocation;
			snapPoint1=axes[snapAxis1].snapPoint;
			snapPoint2=-1;
			return 1;
		}else{
			snapPoint1=-1;
			snapPoint2=-1;
			return 0;
		}
	}
}

public class PS2DSnapAxis{
	public int snapPoint;
	public float snapDist;
	public float snapSize;
	public Vector2 baseLocation;
	private Vector2 direction;
	public PS2DSnapAxis(Vector2 dir){
		direction=dir;
	}
	public void Reset(float dist,float size){
		snapPoint=-1;
		snapSize=size;
		snapDist=dist;
	}
	//Find closest point on this axis
	public void CheckPoint(int pointId,Vector2 pointDragging,Vector2 pointStatic){
		Vector2 basePoint=GetPoint(pointStatic,pointStatic+direction,pointDragging);
		float baseDist=Vector2.Distance(pointDragging,basePoint);
		if(baseDist<snapDist*snapSize && baseDist<snapDist){
			snapPoint=pointId;
			snapDist=baseDist;
			baseLocation=basePoint;
		}
	}
	//This gets position of that green cursor for creating points
	private Vector2 GetPoint(Vector2 b1,Vector2 b2, Vector2 t){
		float d1=Vector2.Distance(b1,t);
		float d2=Vector2.Distance(b2,t);
		float db=Vector2.Distance(b1,b2);
		//Find one of the angles
		float angle1=Mathf.Acos((Mathf.Pow(d1,2)+Mathf.Pow(db,2)-Mathf.Pow(d2,2))/(2*d1*db));
		//Not sure about this
		if(System.Single.IsNaN(angle1)) return t;
		//Find distance to point
		float dist=Mathf.Cos(angle1)*d1;
		//Make sure it's within the line
		return (b1+(dist*(b2-b1).normalized));
	}
	//Intersect with another axis and get the point
	public Vector2 GetIntersection(PS2DSnapAxis another){
		return LineIntersectionPoint(baseLocation,baseLocation+direction,another.baseLocation,another.baseLocation+another.direction);
	}

	private Vector2 LineIntersectionPoint(Vector2 l1s, Vector2 l1e, Vector2 l2s, Vector2 l2e) {
		//Get A,B,C of first line
		float A1=l1e.y-l1s.y;
		float B1=l1s.x-l1e.x;
		float C1=A1*l1s.x+B1*l1s.y;
		//Get A,B,C of second line
		float A2=l2e.y-l2s.y;
		float B2=l2s.x-l2e.x;
		float C2=A2*l2s.x+B2*l2s.y;
		//Get delta and check if the lines are parallel
		float delta=A1*B2-A2*B1;
		if(delta==0) throw new System.Exception("Lines are parallel");
		// now return the Vector2 intersection point
		return new Vector2(
			(B2*C1-B1*C2)/delta,
			(A1*C2-A2*C1)/delta
		);
	}

}
