// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "GOD/DoubleSide" {
	Properties {
		_Color ("Main Color", Color) = (0,0,0,0.52)
		_PassThroughColor ("PassThrough Color", Color) = (0.09,0.15,0.31,0)
		_Emission ("Emmisive Color", Color) = (0.6,0.6,0.6,0.7)
		//_AlphaCutoff ("AlphaCutoff", Range (0, 1)) = 0.5
		_AlphaCutoff ("AlphaCutoff", Float ) = 0.2
		_MainTex ("Base (RGB)", 2D) = "white" { }
	}
	SubShader {
	    Lod 200
		Tags { "RenderType"="Opaque"}
		Pass {
			Lighting Off
			ZTest Greater
			ZWrite Off

			Color [_PassThroughColor]
		}

		Pass {
			Material {
				Diffuse [_Color]
				Ambient [_Color]
				Emission [_Emission]
			}
			Lighting On
			Cull Off

			AlphaTest Greater [_AlphaCutoff]
			SetTexture [_MainTex] {
				constantColor [_Color]
				Combine texture * primary 
			}
			SetTexture [_MainTex] {
				constantColor [_Color]
				Combine Previous + constant DOUBLE, texture * constant
			}
		}
	}
	SubShader {
	    Lod 100
		Pass {
			Lighting Off
			ZTest Greater
			ZWrite Off
			Color [_PassThroughColor]
		}

		Pass {
			Material {
				Diffuse [_Color]
				Ambient [_Color]
				Emission [_Emission]
			}
			Lighting On
			Cull Off
			AlphaTest Greater [_AlphaCutoff]
			SetTexture [_MainTex] {
				constantColor [_Color]
				Combine texture * primary 
			}
			SetTexture [_MainTex] {
				constantColor [_Color]
				Combine Previous + constant DOUBLE, texture * constant
			}
		}
		pass {   
		  ZWrite Off
		  ZTest LEqual
			//Tags { "LightMode" = "ForwardAdd" } 

			//阴影不被遮挡处理
			//ZWrite Off
			//ZTest Always
			Blend DstColor SrcColor 
			Offset -2,-1
			CGPROGRAM
			#pragma vertex vert 
			#pragma fragment frag
			#include "UnityCG.cginc"
			float4x4 _World2Ground;
			float4x4 _Ground2World;
			//float4 vert(float4 vertex: POSITION) : SV_POSITION
			float4 vert(appdata_tan v) : SV_POSITION
			{
				float3 litDir;
					//将顶点灯光数据和物体顶点数据转为世界坐标系
					//litDir=normalize(WorldSpaceLightDir(vertex)); 
					litDir=normalize(float3(3,6,-1)); 
					//litDir = normalize(mul(UNITY_MATRIX_MVP, float4(1,1,1,0)));
				    litDir=mul(_World2Ground,float4(litDir,0)).xyz;
				    float4 vt;
					vt= mul(unity_ObjectToWorld, v.vertex);
					vt=mul(_World2Ground,vt);
					vt.xz=vt.xz-(vt.y/litDir.y)*litDir.xz;
					vt.y=0;
					vt=mul(_Ground2World,vt);//back to world
					vt=mul(unity_WorldToObject,vt);

					return UnityObjectToClipPos(vt);
			}
 			float4 frag(void) : COLOR 
			{
				//return float4(0,0,0,1);
				return float4(0.3,0.3,0.3,0.1);
			}
 			ENDCG 
		}

	}
	
	
	Fallback "VertexLit"
}
