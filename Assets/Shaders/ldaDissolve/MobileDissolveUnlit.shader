//Wrote by Lindean 20150325.
//����˲��ܹ�Ӱ�죬�Է��⴦��͸�������������� �ܽ�ʱ���ܽ��Ե��ɫ��ϰ�͸����ԭ���ǲ�͸����ɫ�ܽ�

Shader "Lindean/MobileDissolveUnlit" 
{

  Properties 
  {
    _DissolvePower ("Dissolve Power", Range(1, 0.2)) = 0.2//��ŷ�Χ 0.65-0.2 ֮��
	//_DissolvePower ("Dissolve Power", float) = 0.2//1-0.4
    _DissolveEmissionColor ("Dissolve Emission Color", Color) = (1,1,1)
    _MainTex ("Main Texture", 2D) = "white"{}
    _DissolveTex ("Dissolve Texture", 2D) = "white"{}

	_ColorX ("Main ColorX", Color) = (0.73,0.73,0.73,1)//RGBA(188,188,188,255)//��Ҫ�� _Color ���ԭ����GODDoubleSide ��ɫ���ˣ���ԭ������ɫ����
	//_Illum ("Illumin (A)", 2D) = "white" {} //��ע��������ͼ����һ��
	//_EmissionLM ("Emission (Lightmapper)", Float) = 0
	_Cutoff ("Cutoff Value", float) = 0.01 //0.01 
	//_AlphaMax ("AlphaMax", float) = 0.2
	//_AlphaMin ("AlphaMin", float) = 0 
  }

	SubShader {
		Tags { "RenderType"="Opaque" }
//		LOD 200
    
	CGPROGRAM
	//#pragma surface surf Unlit alphatest:Zero noforwardadd noambient addshadow
	//#pragma surface surf Lambert alphatest:_Cutoff  
	#pragma surface surf Unlit alphatest:_Cutoff noforwardadd noambient addshadow

	half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
    {
		half4 c;
		c.rgb = s.Albedo;
		c.a = s.Alpha;
		return c;
    } 

	sampler2D _MainTex;
	sampler2D _Illum;
	fixed4 _ColorX;

	sampler2D _DissolveTex;
    float3 _DissolveEmissionColor;
    fixed _DissolvePower;
	//fixed _AlphaMax;
	//fixed _AlphaMin;

	struct Input {
		float2 uv_MainTex;
		float2 uv_Illum;
		float2 uv_DissolveTex;
	};

	void surf (Input IN, inout SurfaceOutput o) {


		half4 tex = tex2D(_MainTex, IN.uv_MainTex);
		half4 texd = tex2D(_DissolveTex, IN.uv_DissolveTex);
		fixed4 c = tex * _ColorX;
		o.Albedo = c.rgb;
		o.Gloss = c.a;
		o.Emission = c.rgb * tex2D(_Illum, IN.uv_Illum).a;
		//o.Alpha = (_DissolvePower - texd.r) * c.a;
		o.Alpha = c.a > 0 ? (_DissolvePower - texd.r) : 0;

		//if ((o.Alpha < _AlphaMax)&&(o.Alpha > _AlphaMin)){
		if ((o.Alpha < 0)&&(o.Alpha > -0.05)){
			o.Alpha = 1;
			o.Albedo = _DissolveEmissionColor;
		}
	}
	ENDCG
  }
}