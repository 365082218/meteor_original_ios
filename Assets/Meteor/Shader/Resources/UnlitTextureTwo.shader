// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "UnlitTexture2" {
		Properties{
			_MainTex("Base (RGB)", 2D) = "white" {}
		}
			SubShader{
			Tags{ "RenderType" = "Opaque" }
			LOD 150
			Cull off
			CGPROGRAM
#pragma surface surf Lambert noforwardadd

			sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Emission = c.rgb;
		}
		ENDCG
		}

			Fallback "Mobile/VertexLit"
	}
