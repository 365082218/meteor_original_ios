Shader "GOD/CustomFog" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FogColor ("Fog Color", Color) = (1,1,1,1)
		_FogStart ("Fog Start", Range(0, 100)) = 0
		_FogEnd ("Fog End", Range(0, 100)) = 50
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		// Non-lightmapped
		Pass {
			Tags { "LightMode" = "Vertex" }
			Lighting Off
			Fog {Mode Linear Color [_FogColor] Range [_FogStart],[_FogEnd]}
			SetTexture [_MainTex] { combine texture } 
		}

		// Lightmapped, encoded as dLDR
		Pass {
			Tags { "LightMode" = "VertexLM" }
			Lighting Off
			Fog {Mode Linear Color [_FogColor] Range [_FogStart],[_FogEnd]}
			BindChannels {
				Bind "Vertex", vertex
				Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
				Bind "texcoord", texcoord1 // main uses 1st uv
			}
			SetTexture [unity_Lightmap] {
				matrix [unity_LightmapMatrix]
				combine texture
			}
			SetTexture [_MainTex] {
				combine texture * previous DOUBLE, texture * primary
			}
		}

		// Lightmapped, encoded as RGBM
		Pass {
			Tags { "LightMode" = "VertexLMRGBM" }
			Lighting Off
			Fog {Mode Linear Color [_FogColor] Range [_FogStart],[_FogEnd]}
			BindChannels {
				Bind "Vertex", vertex
				Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
				Bind "texcoord", texcoord1 // main uses 1st uv
			}
			SetTexture [unity_Lightmap] {
				matrix [unity_LightmapMatrix]
				combine texture * texture alpha DOUBLE
			}
			SetTexture [_MainTex] {
				combine texture * previous QUAD, texture * primary
			}
		} 
	}

}
