Shader "Custom/DDiffuse2Pass"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" }
		Pass
		{
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
		struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		half2 texcoord : TEXCOORD0;
		UNITY_FOG_COORDS(1)
	}; sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv2 = float2(i.texcoord.x, i.texcoord.y);
				fixed4 col = tex2D(_MainTex, uv2) * _Color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
		Pass
		{
			Cull Front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			}; sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color;
			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv2 = float2(1 - i.texcoord.x, i.texcoord.y);
				fixed4 col = tex2D(_MainTex, uv2) * _Color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				//col.Albedo = .rgb;
				//col.Alpha = .a;
				return col;
			}
			ENDCG
			//SetTexture[_MainTex]
			//{
			//	constantColor[_Color]
			//	Combine texture * primary DOUBLE, texture * constant
			//}
				
		}
	}
	FallBack "Diffuse"
}