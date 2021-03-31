[System.Serializable]
public enum PS2DType{
	Simple,
	Bezier
}

[System.Serializable]
public enum PS2DPivotType{
	Manual,
	Auto
}

[System.Serializable]
public enum PS2DPivotPosition{
	//Disabled,
	Center,
	Top,
	Right,
	Bottom,
	Left
}

[System.Serializable]
public enum PS2DFillType{
	Color,
	Gradient,
	Texture,
	TextureWithColor,
	TextureWithGradient,
	CustomMaterial,
	None
}

[System.Serializable]
public enum PS2DColliderType{
	None,
	PolygonStatic,
	PolygonDynamic,
	Edge,
	TopEdge,
	MeshStatic,
	MeshDynamic
}

[System.Serializable]
public enum PS2DDirection{
	Up,
	Right,
	Down,
	Left
}

[System.Serializable]
public enum PS2DPointType{
	None,
	Sharp,
	Rounded
}

[System.Serializable]
public enum PS2DSnapType{
	Points,
	WorldGrid,
	LocalGrid
}