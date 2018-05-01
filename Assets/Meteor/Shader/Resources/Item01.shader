// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color
//不要随便改名字了，否则都乱了
Shader "Item01" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags{ "Queue"="Transparent+1" "RenderType"="Opaque"}
	LOD 200
	Lighting Off
	//ZWrite Off
	//Cull back
		//Blend One One
	//Blend OneMinusDstColor One//soft Additive很适合角色失败死亡状态的世界展示
	//Blend One OneMinusSrcColor
	//Blend Zero SrcColor//正片叠底
	//Blend DstColor SrcColor 二倍相乘
	//Blend DstColor Zero//正片叠底
	Pass {  
		//Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
#pragma multi_compile_fwdbase  
#pragma vertex vert  
#pragma fragment frag  
#include "UnityCG.cginc"  
#include "Lighting.cginc"  
#include "AutoLight.cginc"
//#include "UnityStandardUtils.cginc"
			struct appdata_t {
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _Alpha;
			fixed4 _TintColor;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color.a = v.color.a / 255.0f;
				o.color.g = v.color.g / 255.0f;
				o.color.b = v.color.b / 255.0f;
				o.color.r = v.color.r / 255.0f;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				return col;
			}
		ENDCG
	}
}
	FallBack "Unlit/Texture"
}
