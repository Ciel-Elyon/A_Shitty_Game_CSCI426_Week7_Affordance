using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension{
	
	public static Vector2 Rotate(this Vector2 v,float degrees){
		float radians=degrees*Mathf.Deg2Rad;
		float sin=Mathf.Sin(radians);
		float cos=Mathf.Cos(radians);
		return new Vector2(cos*v.x-sin*v.y,sin*v.x+cos*v.y);
	}

	public static float Angle(this Vector2 v){
		return Vector2.Angle(Vector2.right,v);
	}
	/*
	public static float SignedAngle(this Vector2 v){
		return Vector2.Angle(Vector2.right,v)*Mathf.Sign(v.y);
	}
	*/
	public static float SignedAngle(Vector2 v1,Vector2 v2){
		return Vector2.Angle(v1,v2)*Mathf.Sign(v1.x*v2.y-v1.y*v2.x);
	}

}
