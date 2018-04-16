Shader "Lindean/ldaDepthMask1" {
	SubShader {
		// Render the mask after regular geometry, but before masked geometry and
		// transparent things.
 
		Tags {"Queue" = "Geometry+10" }
		//"Queue" = "Transparent+10"//比BossShader 10 渲染队列前 add by Lindean 20150613
 
		// Don't draw in the RGBA channels; just the depth buffer
 
		ColorMask 0
		ZWrite On
 
		// Do nothing specific in the pass:
 
		Pass {}
	}
}
