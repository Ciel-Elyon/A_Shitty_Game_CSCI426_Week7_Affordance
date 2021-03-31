Shader "ProtoShape2D/Color"{
    Properties{
        _Color("Tint",Color)=(1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor",Color)=(1,1,1,1)
        _StencilComp("Stencil Comparison",Float)=0
    }

    SubShader{
        Tags{
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass{
            Stencil{
				Ref 1
                Comp [_StencilComp] //Disabled, Equal, NotEqual
                Pass Keep
            }
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag
				#include "UnityCG.cginc"

				fixed4 _Color;
				#ifndef UNITY_INSTANCING_ENABLED
					fixed4 _RendererColor;
				#endif

				struct appdata_t{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f{
					float4 vertex   : SV_POSITION;
					fixed4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f Vert(appdata_t IN){
					v2f OUT;
					UNITY_SETUP_INSTANCE_ID(IN);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
					OUT.vertex=UnityObjectToClipPos(IN.vertex);
					OUT.texcoord=IN.texcoord;
					OUT.color=IN.color*_Color*_RendererColor;
					return OUT;
				}

				fixed4 Frag(v2f IN):SV_Target{
					fixed4 c=IN.color;
					c.rgb*=c.a;
					return c;
				}

			ENDCG
        }
    }
}