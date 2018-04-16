// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "UnlitAdditive" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_TintColor("TintColor", Color) = (1,1,1,1)
	_u("u", float) = 0
	_v("v", float) = 0
}

SubShader {
	Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	Blend SrcAlpha One
	//Blend SrcAlpha OneMinusSrcAlpha//Í¸Ã÷¶È»ìºÏ
	Cull Off Lighting Off ZWrite Off
	
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
			};

			struct v2f {
				float4 vertex : SV_POSITION;

				half2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _u;
			float _v;
			fixed4 _TintColor;
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv2 = float2(i.texcoord.x, i.texcoord.y);
				uv2.x -= 2 * _Time * _u;
				uv2.y -= 2 * _Time * _v;
				fixed4 col = tex2D(_MainTex, uv2) * _TintColor;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		ENDCG
	}
}

}
