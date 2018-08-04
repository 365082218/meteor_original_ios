// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "UnlitUV" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_TintColor("TintColor", Color) = (1,1,1,1)
	_u("u", float) = 0
	_v("v", float) = 0
}

SubShader {
	Tags { "Queue" = "Transparent+5" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	Cull Off Lighting Off
		CGPROGRAM
#pragma surface surf Lambert alpha:fade

		sampler2D _MainTex;
	fixed4 _TintColor;
	float _u;
	float _v;
	struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutput o) {
		
		float2 uv2 = float2(IN.uv_MainTex.x, IN.uv_MainTex.y);
		uv2.x -= 2 * _Time * _u;
		uv2.y -= 2 * _Time * _v;
		fixed4 c = tex2D(_MainTex, uv2) * _TintColor;
		o.Emission = c.rgb;
		o.Alpha = 1;
	}
	ENDCG  
	}
		Fallback "Legacy Shaders/Transparent/VertexLit"
}
