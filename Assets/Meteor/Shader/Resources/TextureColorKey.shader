Shader "Custom/TextureColorKey" {
Properties
{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Alpha("Alpha", float) = 1.0
		_ColorKey("ColorKey", Color) = (1,1,1,1)
		_Color("Color", Color) = (1,1,1,1)
}

		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		//ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass{
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
		float _Alpha;
		fixed4 _ColorKey;
		fixed4 _Color;
		v2f vert(appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.texcoord);
			UNITY_APPLY_FOG(i.fogCoord, col);
			if (abs(col.r - _ColorKey.r) <= 0.05 && abs(col.g - _ColorKey.g) <= 0.05 && abs(col.b - _ColorKey.b) <= 0.05)
				col.a = 0;
			else
				col.a = _Alpha;
			col.rgb *= _Color.rgb;
		return col;
		}
			ENDCG
		}
		}

}
