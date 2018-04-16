Shader "GOD/TransparentCutoffDiffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_AlphaCutoff ("Alpha cutoff", Range(0,1)) = 0.2
	_RimColor ("Rim Color", Color) = (0,0,0,0.0)
	_RimPower ("Rim Power", Range(0.5,8.0)) = 2.0
}


SubShader {
	Tags { "RenderType"="Opaque" }
CGPROGRAM
#pragma surface surf Lambert  alphatest:_AlphaCutoff

sampler2D _MainTex;
fixed4 _Color;
float4 _RimColor;
float _RimPower;
struct Input {
	float2 uv_MainTex;
   float3 viewDir;
	float3 worldNormal; 
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    half rim = 1.0 - saturate(dot (normalize(IN.viewDir), IN.worldNormal));
    o.Emission = _RimColor.rgb * pow (rim, 2);      
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}
//Fallback "Diffuse"  //暂时屏蔽，测试兼容性
}
