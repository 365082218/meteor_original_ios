Shader "Outfit7/UI/UI-Text-Glow"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Alpha (A)", 2D) = "white" {}
		_GlowTex ("Glow (A)", 2D) = "white" {}
		
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 8
		_Stencil ("Stencil ID", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType"="Plane"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Offset -1, -1
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"


				struct appdata_t
				{
					half4 vertex : POSITION;
					half4 color : COLOR;
					half2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					half4 vertex : POSITION;
					half4 color : COLOR;
					half2 texcoord : TEXCOORD0;
					half2 texcoord1 : TEXCOORD1;
				};

				sampler2D _MainTex;
				sampler2D _GlowTex;
				half4 _MainTex_ST;
				fixed4 _Color;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#ifdef UNITY_HALF_TEXEL_OFFSET
					o.vertex.xy += (_ScreenParams.zw-1.0)*half2(-1,1);
#endif
					o.color = v.color * _Color;
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.texcoord1 = o.texcoord;
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{
					fixed4 col = i.color;
					fixed font = tex2D(_MainTex, i.texcoord).a;
					fixed glow = tex2D(_GlowTex, i.texcoord1).a;
					fixed glowStep = step(i.texcoord1.x, 0.999);
					col.a *= lerp(glow, font, glowStep);
					return col;
				}
			ENDCG
		}
	}
}
