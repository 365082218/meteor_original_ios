// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "TextureYing" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_AlphaTimes("Times", float) = 1.0
	[Toggle]_Reverse("Reverse", Float) = 0
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	cull off
	ZWrite Off Lighting Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
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
			float _AlphaTimes;
			bool _Reverse;
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
				fixed4 col = tex2D(_MainTex, i.texcoord);
				if (_Reverse)
				{
					col.r = 1.0f - col.r;
					col.g = 1.0f - col.g;
					col.b = 1.0f - col.b;
				}
				col.a = _AlphaTimes * col.a;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		ENDCG
	}
}

}
