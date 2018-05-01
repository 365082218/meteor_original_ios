// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color
//��Ҫ���������ˣ���������
Shader "Item00" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags{ "Queue" = "Transparent" "RenderType" = "Transparent"}
	LOD 200
	Lighting Off
	ZWrite Off
	//Cull back
		//Blend One One
	Blend SrcAlpha One//͸���Ȼ��
	//Blend OneMinusDstColor One//soft Additive���ʺϽ�ɫʧ������״̬������չʾ
	//Blend One OneMinusSrcColor
	//Blend Zero SrcColor//��Ƭ����
	//Blend DstColor SrcColor �������
	//Blend DstColor Zero//��Ƭ����
	Pass {  
		//Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
#pragma multi_compile_fwdbase  
#pragma vertex vert  
#pragma fragment frag  
#include "UnityCG.cginc"  
#include "Lighting.cginc"  
#include "AutoLight.cginc"
			struct appdata_t {
				float4 vertex : POSITION;
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

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color.a = v.color.a / 255.0f;
				o.color.g = v.color.g / 255.0f;
				o.color.b = v.color.b / 255.0f;
				o.color.r = v.color.r / 255.0f;
				//TRANSFER_VERTEX_TO_FRAGMENT(o); //�����android����pc���ֶβ�һ��һ����vertex һ����pos
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
				return col;
			}
		ENDCG
	}
}
	FallBack "Unlit/Texture"
}
