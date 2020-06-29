Shader "Custom/MeteorVertexL"
{
	//卡通渲染进阶 = toonlighting + outline + rimlighting + hair specular
	//卡通光影+描边+边缘光晕+头发高光.
	//Rim Lighting 和 描边 需要学习
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha("Alpha", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderQueue" = "Transparent" }
		LOD 100
		Cull off Lighting Off
		ZWrite on
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color:COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 color:COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Alpha;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color.a = v.color.a / 255.0f;
				o.color.g = v.color.g / 255.0f;
				o.color.b = v.color.b / 255.0f;
				o.color.r = v.color.r / 255.0f;
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//i.color.a = 1;
				//i.color.r = 1;
				//i.color.g = 0;
				//i.color.b = 0;
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				col.a = _Alpha;
				return col;
			}
			ENDCG
		}
	}
}
