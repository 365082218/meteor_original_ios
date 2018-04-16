Shader "Custom/MeshDeform"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_Pos("Position",Vector) = (0,0,0,0)
		_Direction("Direction",Vector) = (0,0,0,0)
		_TimeScale("TimeScale",float) = 1
		_TimeDelay("TimeDelay",float) = 1
	}
		SubShader
	{
		LOD 100
		Tags
	{
		"RenderType" = "Opaque"
		"Queue" = "Transparent"
	}

		CGPROGRAM
#pragma surface surf nolight vertex:vert alpha

		sampler2D _MainTex;
	fixed4 _Pos;
	fixed4 _Direction;
	half _TimeScale;
	half _TimeDelay;

	struct Input
	{
		half2 uv_MainTex;
	};

	float4 Lightingnolight(SurfaceOutput s, float3 lightDir, half3 viewDir, half atten)
	{
		float4 c;
		c.rgb = s.Albedo;
		c.a = s.Alpha;
		return c;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}

	void vert(inout appdata_full v)
	{
		half dis = distance(v.vertex ,_Pos);
		half time = (_Time.y + _TimeDelay) * _TimeScale;
		v.vertex.xyz += dis * (sin(time) * cos(time * 2 / 3) + 1) * _Direction.xyz;	//核心，动态顶点变换
	}

	ENDCG
	}

		FallBack "Transparent/Cutout/VertexLit"
}