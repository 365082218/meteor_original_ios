Shader "DoubleSide/DDiffuse"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" }
		Material
	{
		Diffuse[_Color]
		Ambient(1,1,1,1)
	}
		Pass
	{

		Lighting On
		Cull off
		Blend SrcAlpha OneMinusSrcAlpha

		SetTexture[_MainTex]
	{
		constantColor[_Color]
		Combine texture * primary DOUBLE, texture * constant
	}
	}
	}
		FallBack "Diffuse", 1
}