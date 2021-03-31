Shader "ProtoShape2D/TextureAndColors"{
	Properties{
		_Texture("Texture",2D)="white"{}
		_TextureAngle("Texture rotation",Range(-180.0,180.0))=0
		_TextureOffset("Texture offset",Vector)=(0,0,0,0)
		_Color1("Color one",Color)=(1,0.5,0.5,1)
		_Color2("Color two",Color)=(0.5,0.5,1,1)
		_GradientScale("Gradient scale",Range(0.01,10.0))=0
		_GradientAngle("Gradient rotation",Range(-180.0,180.0))=0
		_GradientOffset("Gradient offest",Range(-1.0,1.0))=0
		//Set by C# code. For positioning a gradient and texture
		_WPos("Position of the pivot",Vector)=(0,0,0,0)
		_MinWPos("Min world position",Vector)=(0,0,0,0)
		_MaxWPos("Max world position",Vector)=(0,0,0,0)
		_StencilComp("Stencil Comparison",Float)=0
	}
	SubShader{
		Tags{
			"Queue"="Transparent" 
			"IgnoreProjector"="True"
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"ForceNoShadowCasting"="True"
			"DisableBatching"="False"
		}
		Cull Off 
		Lighting Off 
		ZWrite Off 
		Fog {Mode Off} 
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
            Stencil{
				Ref 1
                Comp [_StencilComp] //Disabled, Equal, NotEqual
                Pass Keep
            }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _Texture;
			fixed4 _Texture_ST;
			fixed4 _Color1,_Color2;
			fixed4 _WPos,_MinWPos,_MaxWPos,_TextureSize,_TextureOffset;
			float _TextureAngle,_GradientScale,_GradientAngle,_GradientOffset;
			struct vertex_in{
				float4 col:COLOR;
				float4 pos:POSITION;
				float4 uv:TEXCOORD0;
			};
			struct fragment_in{
			 	float4 col:COLOR0;
				float4 pos:SV_POSITION;
				float3 wpos:COLOR1;
				half2 uv:TEXCOORD0;
			};
			fragment_in vert(vertex_in v){
				fragment_in f;
				f.col=v.col;
				f.pos=UnityObjectToClipPos(v.pos);
				f.wpos=mul(unity_ObjectToWorld,v.pos).xyz; //2D position of a pixel in world coordinates
				f.uv=v.uv;
				return f;
			}
			half2 Rotate2D(half2 _in,half _angle){
				half s,c;
				sincos(_angle,s,c);
				float2x2 rot={c,s,-s,c};
				return mul(rot,_in);
			}
			fixed4 frag(fragment_in f):SV_Target{
				fixed4 result=fixed4(1,1,1,1);
				fixed small=0.0000000001;
				//If UV is 1,1, we use vertext color. For outline
				if(f.uv.x==1 && f.uv.y==1) result=f.col;
				//In all other cases use UV-free gradient and texturing
				else{
					//Add texture, calculating its scale, position and rotation
					fixed2 tpos=mul(unity_WorldToObject,half4(f.wpos-_WPos,0));
					result*=tex2D(_Texture,(Rotate2D(tpos-(_TextureOffset*_Texture_ST),radians(_TextureAngle))/_Texture_ST)+0.5);
					//If colors are the same, don't calculate the gradient
					if(all(_Color1==_Color2)){
						result*=_Color1;
					}else{
						//Calculate UV-free fragment position
						half2 gpos=(f.wpos-lerp(_MinWPos,_MaxWPos,0.5))/max(_MaxWPos.x-_MinWPos.x,_MaxWPos.y-_MinWPos.y);
						gpos=Rotate2D(gpos,radians(_GradientAngle))+_GradientOffset;
						//Calculate color
						result*=_Color2*clamp(1-(gpos.y/_GradientScale+0.5),0,1)+_Color1*clamp(gpos.y/_GradientScale+0.5,0,1);
						//Weird solution for mirrored gradient bug on OpenGLS3
						//Adding some invisible conditional modification to the image stops it from mirroring
						if(gpos.x>-small && gpos.x<small && gpos.y>-small && gpos.y<small) result.a-=small;
					}
					//Use vertex alpha if it's smaller. For "anti-aliasing"
					result.a=min(result.a,f.col.a);
				}
				return result;
			}
			ENDCG
		}
	}
}