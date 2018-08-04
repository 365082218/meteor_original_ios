// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "TextureMask2" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_MaskTex("Alpha (RGB) Trans (A)", 2D) = "white" {}
	_Color("Color", Color) = (1,1,1,1)
		//变形动画部分
	//_deformsize("deformsize", Range(0, 10)) = 0.1//决定偏移的限度
	//_deformrange("deformrange", Range(0, 4)) = 1.5//波函数范围限定 1.5代表 PI为一个周期,X轴从0到这个值，对应的Y值，一起构成函数
	//_deformref("deformref", Vector) = (0,0,0,0)//决定哪些轴做变形动画
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Transparent"}
	//Tags{ "Queue" = "Geometry+50" "RenderType" = "Opaque" }
	LOD 100
	cull off Lighting Off
	//ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	//Blend DstColor SrcColor
	//Blend SrcAlpha DstColor
	//Blend One One
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				half2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			sampler2D _MaskTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _deformsize;
			float _deformrange;
			float4 _deformref;
			v2f vert (appdata_t v)
			{
				v2f o;
				//o.vertex = UnityObjectToClipPos(v.vertex);
				//v.vertex.xyz = mul((float3x3)unity_WorldToObject, v.vertex.xyz + _deformsize * _deformref * float3(sin(_deformrange * 3.14 * ((1 - v.texcoord.y) - 0.5f)) * _SinTime.w, sin(_deformrange * 3.14 * (1 - v.texcoord.y)) * _SinTime.w, sin(_deformrange * 3.14 * (1 - v.texcoord.y)) * _SinTime.w));
				//v.normal = normalize(float3(v.normal.x + waveValueA, v.normal.y, v.normal.z));
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
				fixed4 mas = tex2D(_MaskTex, i.texcoord);
				//clip(mas.a - 0.1);
				v2f o;
				o.color.a = i.color.a / 255.0f;
				o.color.g = i.color.g / 255.0f;
				o.color.b = i.color.b / 255.0f;
				o.color.r = i.color.r / 255.0f;
				//if (o.color.a == 1)
				//	col.rgb *= mas.rgb * o.color;
				//else
				col.rgb *= mas.rgb * o.color;
				col.a = mas.a;
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		ENDCG
	}
}
FallBack "Diffuse"
}
