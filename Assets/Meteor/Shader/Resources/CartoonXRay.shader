Shader "SharedShader/CartoonOccluTransparent"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
	_OutlineWidth("Outline Width", Range(0, 0.05)) = 0
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OccluColor("Occlu Color", Color) = (1,1,1,1)
		_ToonEffect("Toon Effect",range(0,1)) = 0.5
		_Steps("Steps of toon",range(0,9)) = 3
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

		Pass{
		Name "Outline"
		Tags{
	}
	Cull Front

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		uniform float _OutlineWidth;
	uniform float4 _OutlineColor;

	struct VertexInput {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct VertexOutput {
		float4 pos : SV_POSITION;
	};

	VertexOutput vert(VertexInput v) {
		VertexOutput o = (VertexOutput)0;
		o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal*_OutlineWidth,1));
		return o;
	}

	float4 frag(VertexOutput i) : COLOR{
		return fixed4(_OutlineColor.rgb,0);
	}
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
	float _Steps;
	float _ToonEffect;

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
		float diff = max(0,dot(N,i.lightDir));
		diff = (diff + 1) / 2;
		diff = smoothstep(0,1,diff);
		float toon = floor(diff*_Steps) / _Steps;
		diff = lerp(diff,toon,_ToonEffect);
		c = _Color*_LightColor0*(diff);
		c *= tex2D(_MainTex, i.texcoord);
		return c;
	}
		ENDCG
	}

		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}