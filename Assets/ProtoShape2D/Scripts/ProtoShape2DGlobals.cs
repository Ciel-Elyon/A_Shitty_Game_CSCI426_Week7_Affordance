using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName="ProtoShape2DGlobals")]
public class ProtoShape2DGlobals:ScriptableObject{
	//Reference for single color materials
	public Material colorMaterialNoStencil;
	public Material colorMaterialStencilEqual;
	public Material colorMaterialStencilNotEqual;
}
