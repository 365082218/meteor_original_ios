Shader "Outfit7/UI/ProgressBar" {
Properties {
    _MainTex ("Main Texture", 2D) = "white" {}
    _AlphaTex ("Alpha Texture", 2D) = "white" {}
    _OverlayTex ("Overlay Texture", 2D) = "white" {}
    _Aspect ("Aspect", Float) = 1
    _Color ("Color", Color) = (1,1,1,1)
    _ShineColor("Shine Color", Color) = (1,1,1,1)
    _ShineIntensity ("Shine Intensity", Range(0, 3)) = 1
    _Progress ("Progress", Range(0, 1)) = 1

    [Enum(Off,0,On,1)] _Zwrite("Zwrite", Float) = 0
    [Enum(UnityEngine.Rendering.CompareFunction)] _Ztest("Ztest", Float) = 4
    [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
}

SubShader {
    Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
    Pass {
        Blend SrcAlpha OneMinusSrcAlpha
        Name "BASE"

        ZWrite [_Zwrite]
        Ztest [_Ztest]
        Cull [_Cull]
        
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        
        sampler2D _MainTex;
        sampler2D _AlphaTex;
        sampler2D _OverlayTex;
        float _Aspect;
        fixed4 _Color;
        fixed4 _ShineColor;
        fixed _Progress;
        float _ShineIntensity;

        struct vertexInput {
            half4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
        };

        struct vertexOutput {
            half4 pos : SV_POSITION;
            float2 texcoord : TEXCOORD0;
            float2 texcoord_left : TEXCOORD1;
        };

        vertexOutput vert(vertexInput i)
        {
            float ofset = _Progress * -_Aspect.x + 1;

            vertexOutput o;
            o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
            o.texcoord = i.texcoord  * float2(_Aspect, 1) + float2(ofset, 0);
            o.texcoord_left = 1.0 - (i.texcoord * float2(_Aspect, 1));
            return o;
        }
         
        fixed4 frag(vertexOutput i) : COLOR
        {
            fixed tex_progress_mask_inverted_x = tex2D(_AlphaTex, i.texcoord_left).a;

            fixed4 tex_progress = tex2D(_MainTex, i.texcoord);
            fixed tex_progress_alpha = tex2D(_AlphaTex, i.texcoord).a;
            fixed tex_progress_overlay_alpha = tex2D(_OverlayTex, i.texcoord).a;

            fixed3 progress_color = tex_progress.rgb * _Color.rgb;
            progress_color += _ShineColor.rgb * tex_progress_overlay_alpha * _ShineIntensity;

            return fixed4(progress_color, tex_progress_alpha * tex_progress_mask_inverted_x * _Color.a);
        }

        ENDCG
        }
    }
}