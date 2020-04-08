Shader "X-Ray/OccluTransparent" {
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
	_OccluColor("Occlu Color", Color) = (1,1,1,1)
	}
		SubShader
	{
		Tags{ "Queue" = "Geometry+100" "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		Blend One OneMinusSrcAlpha
		ZWrite Off
		ZTest Greater

		CGPROGRAM
#include "Lighting.cginc"  
		fixed4 _OccluColor;
	uniform float _OutlineWidth;

	struct v2f
	{
		float4 pos : SV_POSITION;
		float3 normal : normal;
		float3 viewDir : TEXCOORD0;
	};

	v2f vert(appdata_base v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.viewDir = ObjSpaceViewDir(v.vertex);
		o.normal = v.normal;
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		float3 normal = normalize(i.normal);
		float3 viewDir = normalize(i.viewDir);
		float rim = 1 - dot(normal, viewDir);
		return fixed4(_OccluColor.rgb * rim, rim * 0.5);
	}
#pragma vertex vert  
#pragma fragment frag  
		ENDCG
	}

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"


		sampler2D _MainTex;
	float4 _MainTex_ST;
	uniform float4 _Color;
	uniform float4 _LightColor0;

	struct v2f {
		float4 pos:SV_POSITION;
		float3 lightDir:TEXCOORD0;
		float3 viewDir:TEXCOORD1;
		float3 normal:TEXCOORD2;
		float2 texcoord:TEXCOORD3;
	};

	v2f vert(appdata_full v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.normal = v.normal;
		o.lightDir = ObjSpaceLightDir(v.vertex);
		o.viewDir = ObjSpaceViewDir(v.vertex);
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	float4 frag(v2f i) :COLOR
	{
		float4 c = 1;
		float3 N = normalize(i.normal);
		c = _Color*_LightColor0;
		c *= tex2D(_MainTex, i.texcoord);
		return c;
	}
		ENDCG
	}

		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}