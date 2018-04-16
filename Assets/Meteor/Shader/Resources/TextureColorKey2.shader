// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TextureColorKey2" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_FilterfColor("Ridof (RGB)",Color) = (1,1,1,1)
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		pass
	{
		CGPROGRAM

#pragma vertex vertext_convert
#pragma fragment fragment_convert
#include "UnityCG.cginc" 

			sampler2D  _MainTex;
		sampler2D  _MainTex1;
		float4  _FilterfColor;
		struct Inputvrite
		{
			float4 vertex : POSITION;
			float4 texcoord : TEXCOORD0;
		};
		struct Inputfragment
		{
			float4 pos : SV_POSITION;
			float4 uv : TEXCOORD0;
		};

		float ColorLerp(float3 tmp_nowcolor,float3 tmp_FilterfColor)
		{
			float3 dis = float3(abs(tmp_nowcolor.x - tmp_FilterfColor.x),abs(tmp_nowcolor.y - tmp_FilterfColor.y),abs(tmp_nowcolor.z - tmp_FilterfColor.z));
			float dis0 = sqrt(pow(dis.x,2) + pow(dis.y,2) + pow(dis.z,2));
			float maxdis = sqrt(3);
			float dis1 = lerp(0,maxdis,dis0);
			return dis1;
		}

		Inputfragment vertext_convert(Inputvrite i)
		{
			Inputfragment o;
			o.pos = UnityObjectToClipPos(i.vertex);
			o.uv = float4(i.texcoord.xy,1,1);
			return o;
		}

		float4 fragment_convert(Inputfragment o) : COLOR
		{
			float4 c = tex2D(_MainTex,o.uv);
			c.a *= ColorLerp(c.rgb,_FilterfColor.rgb);
			return c;
		}


			ENDCG
	}
	}
		FallBack "Diffuse"
}
