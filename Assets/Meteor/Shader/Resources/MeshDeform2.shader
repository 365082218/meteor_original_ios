// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MeshDeform2"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_deformsize("deformsize", Range(0, 10)) = 0.1//决定偏移的限度
		_deformrange("deformrange", Range(0, 4)) = 1.5//波函数范围限定 1.5代表 PI为一个周期,X轴从0到这个值，对应的Y值，一起构成函数
		_deformref("deformref", Vector) = (0,0,0,0)//决定哪些轴做变形动画
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"}
		LOD 200
		cull off Lighting Off
		CGPROGRAM
#pragma target 4.0
#pragma surface surf nolight vertex:vert alpha

	sampler2D _MainTex;
	float _deformsize;
	float _deformrange;
	float4 _deformref;

	struct Input
	{
		float2 uv_MainTex;
		float3 vertColor;
		float4 vertex;
	};

	float4 Lightingnolight(SurfaceOutput s, float3 lightDir, half3 viewDir, half atten)
	{
		float4 c;
		c.rgb = s.Albedo;
		c.a = s.Alpha;
		return c;
	}

	void vert(inout appdata_full v, out Input o)
	{
		v.vertex.xyz = mul((float3x3)unity_WorldToObject, v.vertex.xyz + _deformsize * _deformref * float3(sin(_deformrange * 3.14 * ((1 - v.texcoord.y) - 0.5f)) * _SinTime.w, sin(_deformrange * 3.14 * ((1 - v.texcoord.y) - 0.5)) * _SinTime.w,  sin(_deformrange * 3.14 * (1 - v.texcoord.y)) * _SinTime.w));
		//v.normal = normalize(float3(v.normal.x + waveValueA, v.normal.y, v.normal.z));
		o.vertex = v.vertex;
		o.vertColor = v.color;
		o.uv_MainTex = v.texcoord;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = c.rgb * (IN.vertColor / 255.0f);
		o.Alpha = c.a;
	}
	ENDCG
	}
	FallBack "Diffuse"
}
