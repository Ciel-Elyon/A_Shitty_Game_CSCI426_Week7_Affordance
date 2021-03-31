using UnityEngine;

[System.Serializable]
public class PS2DColliderPoint:System.Object{
	public Vector2 position=Vector2.zero;
	public Vector2 wPosition=Vector2.zero;
	public Vector2 normal=Vector2.zero;
	public float signedAngle=0;
	public PS2DDirection direction;
	public PS2DColliderPoint(Vector2 position){
		this.position=position;
	}
}
