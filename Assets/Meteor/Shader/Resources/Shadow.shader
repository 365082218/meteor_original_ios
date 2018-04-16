// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Shadow"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags
	{
		"Queue" = "Transparent-100"
		"RenderType" = "Transparent"
	}
		Pass
	{
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
#pragma vertex vert  
#pragma fragment frag  

#include "UnityCG.cginc"  
		uniform float4x4 _WorldToCameraMatrix;
	uniform float4x4 _ProjectionMatrix;

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);

		float4 worldCoord = mul(unity_ObjectToWorld, v.vertex);
		float4 cameraCoord = mul(_WorldToCameraMatrix, worldCoord);
		float4 projectionCoord = mul(_ProjectionMatrix, cameraCoord);
		o.uv = projectionCoord / projectionCoord.w;
		o.uv = 0.5f*o.uv + float2(0.5f, 0.5f);
		return o;
	}

	sampler2D _MainTex;

	fixed4 frag(v2f i) : SV_Target
	{
		float dis = distance(i.uv, float2(0.5f, 0.5f));
	fixed4 col = tex2D(_MainTex, i.uv);
	if (col.r > 0 || col.g > 0 || col.b > 0)
	{
		col.a = 1.0f;
	}
	col.rgb = 0;
	col.a = (col.a - dis*1.8f)*0.6f;
	//if (i.uv.y < 0.0f || i.uv.y > 1.0f || i.uv.x < 0.0f || i.uv.x > 1.0f)
	//{
	//	discard;
	//}
	return col;
	}
		ENDCG
	}
	}
}