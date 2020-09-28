// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color
//��Ҫ���������ˣ���������
Shader "Custom/AlphaTexture" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Alpha("Alpha", float) = 1.0
	_TintColor("Color", Color) = (1,1,1,1)
}

SubShader {
	Tags{ "Queue" = "Transparent-6" "RenderType" = "Transparent"}
	LOD 200
	Lighting Off
	Blend SrcAlpha OneMinusSrcAlpha//͸���Ȼ��
	//Blend OneMinusDstColor One//soft Additive���ʺϽ�ɫʧ������״̬������չʾ
	//Blend One OneMinusSrcColor
	//Blend Zero SrcColor//��Ƭ����
	//Blend DstColor SrcColor �������
	//Blend DstColor Zero//��Ƭ����
	Pass {  
			CGPROGRAM
#pragma multi_compile_fwdbase  
#pragma vertex vert  
#pragma fragment frag  
#include "UnityCG.cginc"  
#include "Lighting.cginc"  
#include "AutoLight.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 lightDir : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				LIGHTING_COORDS(4, 5)                //1  
				UNITY_FOG_COORDS(1)
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
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				fixed4 fragColor;
				fragColor.rgb = col.rgb;
				fragColor.a = _Alpha * col.a;
				return fragColor * _TintColor;
			}
		ENDCG
	}
}
	FallBack "Unlit/Texture"
}
