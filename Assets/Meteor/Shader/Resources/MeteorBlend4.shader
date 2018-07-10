//专门负责显示特效
Shader "Custom/MeteorBlend4" {
	Properties {
		//_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Intensity("Alpha", Range(0,1)) = 1.0
		_TintColor("Color", Color) = (1,1,1,1)
		_u("u", float) = 0
		_v("v", float) = 0
	}
		/*Opaque: 用于大多数着色器（法线着色器、自发光着色器、反射着色器以及地形的着色器）。
Transparent:用于半透明着色器（透明着色器、粒子着色器、字体着色器、地形额外通道的着色器）。
TransparentCutout: 蒙皮透明着色器（Transparent Cutout，两个通道的植被着色器）。
Background: Skybox shaders. 天空盒着色器。
Overlay: GUITexture, Halo, Flare shaders. 光晕着色器、闪光着色器。
TreeOpaque: terrain engine tree bark. 地形引擎中的树皮。
TreeTransparentCutout: terrain engine tree leaves. 地形引擎中的树叶。
TreeBillboard: terrain engine billboarded trees. 地形引擎中的广告牌树。
Grass: terrain engine grass. 地形引擎中的草。
GrassBillboard: terrain engine billboarded grass. 地形引擎何中的广告牌草。
渲染队列	渲染队列描述	渲染队列值
Background	这个队列被最先渲染。它被用于skyboxes等。	1000
Geometry	这是默认的渲染队列。它被用于绝大多数对象。不透明几何体使用该队列。	2000
AlphaTest	通道检查的几何体使用该队列。它和Geometry队列不同，对于在所有立体物体绘制后渲染的通道检查的对象，它更有效。	2450
Transparent	该渲染队列在Geometry和AlphaTest队列后被渲染。任何通道混合的（也就是说，那些不写入深度缓存的Shaders）对象使用该队列，例如玻璃和粒子效果。	3000
Overlay	该渲染队列是为覆盖物效果服务的。任何最后被渲染的对象使用该队列，例如镜头光晕。	4000
*/
	SubShader {
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+10" }
		LOD 100
		ZWrite Off Lighting Off
		//流星很多特效都是把高光的透明度降低做的
		Blend SrcAlpha One
		//Blend One OneMinusSrcColor  //忽略了透明
			//Blend OneMinusDstColor One
		//Blend One One
		Cull Off
		Lighting Off
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		//#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		float _u;
		float _v;
		fixed4 _TintColor;
		float _Intensity;
		void surf (Input IN, inout SurfaceOutput  o) {
			// Albedo comes from a texture tinted by color

			float2 uv2 = float2(IN.uv_MainTex.x, IN.uv_MainTex.y);
			uv2.x +=  16 * _Time * _u;
			uv2.y +=  16 * _Time * _v;

			fixed4 c = tex2D (_MainTex, uv2) * _TintColor;
			o.Albedo = c.rgb * _Intensity;

			o.Emission = c.rgb * _Intensity;
			// Metallic and smoothness come from slider variables
			//o.Alpha = (c.r + c.g + c.b) *_TransVal;
			//o.Alpha = 0.5f;
		}
		ENDCG
	}
	//FallBack "Diffuse" //去掉不要影子，特效都这样的
}
