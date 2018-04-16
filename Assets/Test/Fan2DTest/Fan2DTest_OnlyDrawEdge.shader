Shader "XDYX/Fan2DTest_OnlyDrawEdge" 
{
   Properties 
    {
    _Color ("Main Color", Color) = (1,1,1,1)
	_EdgeTex ("Edge (RGB) Trans (A)", 2D) = "white" {}
    }
    Category 
    {
        Lighting Off
        ZWrite Off
        Cull back
        Fog { Mode Off }
        //Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
		Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" }
        //Blend SrcAlpha OneMinusSrcAlpha

	Blend SrcAlpha One
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Mode Off }
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}

        SubShader 
        {
				ZWrite Off
		ZTest Always
		Cull Off

			Pass {
				Lighting Off    
				SetTexture [_EdgeTex] 
				{
					constantColor [_Color]
					matrix [_Rotation01]
					combine texture * constant
				}
			}

			Pass {
				Lighting Off    
				SetTexture [_EdgeTex] 
				{
					constantColor [_Color]
					matrix [_Rotation02]
					combine texture * constant
				}
			}
        }
    }
    Fallback "Transparent/VertexLit"
}