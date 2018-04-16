Shader "Custom/MeteorUnitS"
{
	Properties
	{
		_MainTex("Base(RGB) Trans(A)", 2D) = "white" {}
	_BlendMap("Gloss(R) Illum(G) Mask(B)", 2D) = "black" {}
	_BumpMap("Normalmap", 2D) = "bump" {}
	_FlowMap("Flowmap", 2D) = "black" {}
	_MaskColor("Mask Color", Color) = (1, 1, 1, 1)
		_Alpha("Alpha", Range(0, 1)) = 1
		_Specular("Specular", Range(0, 10)) = 1
		_Shininess("Shininess", Range(0.01, 1)) = 0.5
		_Emission("Emission", Range(0, 10)) = 1
		_FlowSpeed("Flow Speed", Range(0, 10)) = 1
		_RimColor("Rim Color", Color) = (0, 0, 0, 1)
		_RimPower("Rim Power", Range(0, 10)) = 1
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"RenderType" = "Transparent"
		"IgnoreProjector" = "True"
	}
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0.1

		CGPROGRAM
#pragma surface surf CustomBlinnPhong nolightmap  

		sampler2D _MainTex;
	sampler2D _BlendMap;
	sampler2D _FlowMap;
	sampler2D _BumpMap;
	fixed3 _MaskColor;
	fixed3 _RimColor;
	fixed _Alpha;
	fixed _Specular;
	fixed _Shininess;
	fixed _Emission;
	fixed _FlowSpeed;
	fixed _RimPower;

	struct Input
	{
		fixed2 uv_MainTex;
		fixed2 uv2_FlowMap;
	};

	inline fixed4 LightingCustomBlinnPhong(SurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
	{
		fixed3 h = normalize(lightDir + viewDir);
		fixed diff = saturate(dot(s.Normal, lightDir));
		fixed nh = saturate(dot(s.Normal, h));
		fixed spec = pow(nh, s.Specular * 128.0) * s.Gloss;
		fixed nv = pow(1 - saturate(dot(s.Normal, viewDir)), _RimPower);

		fixed4 c;
		c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * 2);
		c.rgb += nv * _RimColor;
		c.a = s.Alpha + _LightColor0.a * _SpecColor.a * spec * atten;

		return c;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		fixed3 b = tex2D(_BlendMap, IN.uv_MainTex);
		fixed3 f = tex2D(_FlowMap, IN.uv2_FlowMap + _Time.xx * _FlowSpeed);

		c.rgb = lerp(c.rgb, _MaskColor.rgb, b.b);
		o.Albedo = c.rgb;
		o.Alpha = c.a * _Alpha;
		o.Gloss = b.r * _Specular;
		o.Specular = _Shininess;
		o.Emission = c.rgb * b.g * _Emission + f;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
	}

	ENDCG
	}

		FallBack "Mobile/Diffuse"
}