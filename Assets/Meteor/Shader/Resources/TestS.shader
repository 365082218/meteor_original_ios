// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Frag/Light" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		Pass{
		Tags{ "LightMode" = "ForwardBase" }

		CGPROGRAM

#pragma multi_compile_fwdbase  

#pragma vertex vert  
#pragma fragment frag  

#include "UnityCG.cginc"  
#include "Lighting.cginc"  
#include "AutoLight.cginc"  

		sampler2D _MainTex;

	float4 _MainTex_ST;

	struct a2v {
		float4 vertex : POSITION;
		fixed3 normal : NORMAL;
		fixed4 texcoord : TEXCOORD0;
		fixed4 color : COLOR;
	};

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float3 worldNormal : TEXCOORD1;
		float3 lightDir : TEXCOORD2;
		float3 viewDir : TEXCOORD3;
		LIGHTING_COORDS(4,5)                //1  
	};

	v2f vert(a2v v) {
		v2f o;

		//Transform the vertex to projection space  
		o.pos = UnityObjectToClipPos(v.vertex);
		//Get the UV coordinates  
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		//Normal  
		o.worldNormal = mul(SCALED_NORMAL, (float3x3)unity_WorldToObject);            //2  
																				//Light Direction  
		o.lightDir = mul((float3x3)unity_ObjectToWorld, ObjSpaceLightDir(v.vertex));  //3  
																				//View Direction  
		o.viewDir = mul((float3x3)unity_ObjectToWorld, ObjSpaceViewDir(v.vertex));    //4  

																				//Shadow  
		TRANSFER_VERTEX_TO_FRAGMENT(o);                                         //5  



		return o;
	}

	inline fixed4 LightingFragLambert(fixed4 fcol, fixed3 lightDir, fixed atten, half3 worldNormal)
	{
		fixed difLight = max(0, dot(normalize(worldNormal), normalize(lightDir)));
		fixed4 col;
		col.rgb = fcol.rgb * _LightColor0.rgb * (difLight * atten);
		col.a = fcol.a;

		return col;
	}

	half3 calDiffuse(half3 pos, half3 worldNormal)
	{
		half3 ambient = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, pos, worldNormal);
		ambient = ShadeSHPerVertex(worldNormal, ambient);
		return ShadeSHPerPixel(worldNormal, ambient, pos);
	}

	float4 frag(v2f i) : COLOR{
		fixed4 texColor = tex2D(_MainTex, i.uv);

	fixed4 fragColor = LightingFragLambert(texColor, i.lightDir, LIGHT_ATTENUATION(i), i.worldNormal);//6  
	half3 diffuse = calDiffuse(i.pos, i.worldNormal);                           //7  
	fragColor.rgb += texColor.rgb * diffuse;//8  

	return fragColor;
	}
		ENDCG
	}
	}
		FallBack "Diffuse"
}