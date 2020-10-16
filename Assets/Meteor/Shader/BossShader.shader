// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Custom/BossShader" {
    Properties {
        _NormalTex ("NormalTex", 2D) = "bump" {}
        //_height ("height", 2D) = "white" {}
        _MainTex ("MainTex", 2D) = "white" {}
        _SpecularTex ("SpecularTex", 2D) = "white" {}
        _RimColor ("RimColor", Color) = (0,0,0,1)
        _gloss ("gloss", Range(0, 1)) = 0.35
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
           // "Queue"="AlphaTest"
           "Queue"="Transparent+10"
            "RenderType"="TransparentCutout"
        }
        
       
			  
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _NormalTex; uniform float4 _NormalTex_ST;
            //uniform sampler2D _height; uniform float4 _height_ST;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _SpecularTex; uniform float4 _SpecularTex_ST;
            uniform float4 _RimColor;
            uniform float _gloss;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _nomal_var = UnpackNormal(tex2D(_NormalTex,TRANSFORM_TEX(i.uv0, _NormalTex)));
                float3 normalLocal = _nomal_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                
                float nSign = sign( dot( viewDirection, i.normalDir ) ); // Reverse normal if this is a backface
                i.normalDir *= nSign;
                normalDirection *= nSign;
                
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip(_MainTex_var.a - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _gloss;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _ill_var = tex2D(_SpecularTex,TRANSFORM_TEX(i.uv0, _SpecularTex));
                float3 specularColor = _ill_var.rgb;
                float3 directSpecular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),specPow);
                float3 indirectSpecular = (0 + (_LightColor0.rgb*attenuation*(max(0,dot(lightDirection,i.normalDir))+max(0,dot(i.normalDir,halfDirection)))*UNITY_LIGHTMODEL_AMBIENT.rgb));
                float3 specular = (directSpecular + indirectSpecular) * specularColor;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 indirectDiffuse = float3(0,0,0);
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                indirectDiffuse += float3(attenuation,attenuation,attenuation); // Diffuse Ambient Light
              // float3 diffuse = (directDiffuse + indirectDiffuse) * (_MainTex_var.rgb+_kcolor.rgb);
                float3 diffuse = (directDiffuse + indirectDiffuse) *_MainTex_var.rgb;
      
////// Emissive:
                /*float4 _height_var = tex2D(_height,TRANSFORM_TEX(i.uv0, _height));
               float3 emissive = _height_var.rgb;*/
// rim color 
                half rim = 1.0 - saturate(dot( viewDirection, i.normalDir ));
             //  float rimColor=_kcolor.rgb * pow (rim, 1);
               float rimColor = _RimColor.rgb * rim;
/// Final Color:
         
                 
			   float3 finalColor = diffuse + specular + rimColor;
                //float3 finalColor = diffuse + specular + emissive+rimColor;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ForwardAdd"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            Cull Off
            
            
            Fog { Color (0,0,0,0) }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _NormalTex; uniform float4 _NormalTex_ST;
            //uniform sampler2D _height; uniform float4 _height_ST;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _SpecularTex; uniform float4 _SpecularTex_ST;
            uniform float4 _RimColor;
            uniform float _gloss;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _nomal_var = UnpackNormal(tex2D(_NormalTex,TRANSFORM_TEX(i.uv0, _NormalTex)));
                float3 normalLocal = _nomal_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                
                float nSign = sign( dot( viewDirection, i.normalDir ) ); // Reverse normal if this is a backface
                i.normalDir *= nSign;
                normalDirection *= nSign;
                
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip(_MainTex_var.a - 0.5);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _gloss;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _ill_var = tex2D(_SpecularTex,TRANSFORM_TEX(i.uv0, _SpecularTex));
                float3 specularColor = _ill_var.rgb;
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow);
                float3 specular = directSpecular * specularColor;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 diffuse = directDiffuse * (_MainTex_var.rgb);
/// Final Color:

               half rim = 1.0 - saturate(dot( viewDirection, i.normalDir ));
              // float rimColor=_kcolor.rgb * pow (rim, 2);
                float rimColor= _RimColor.rgb *rim;

                float3 finalColor = diffuse + specular;
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
  
			
//        Pass {
//            Name "ShadowCollector"
//            Tags {
//                "LightMode"="ShadowCollector"
//            }
//            Cull Off
//            
//            Fog {Mode Off}
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #define UNITY_PASS_SHADOWCOLLECTOR
//            #define SHADOW_COLLECTOR_PASS
//            #include "UnityCG.cginc"
//            #include "Lighting.cginc"
//            #pragma fragmentoption ARB_precision_hint_fastest
//            #pragma multi_compile_shadowcollector
//            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
//            #pragma target 3.0
//            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
//            struct VertexInput {
//                float4 vertex : POSITION;
//                float2 texcoord0 : TEXCOORD0;
//            };
//            struct VertexOutput {
//                V2F_SHADOW_COLLECTOR;
//                float2 uv0 : TEXCOORD5;
//            };
//            VertexOutput vert (VertexInput v) {
//                VertexOutput o = (VertexOutput)0;
//                o.uv0 = v.texcoord0;
//                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//                TRANSFER_SHADOW_COLLECTOR(o)
//                return o;
//            }
//            fixed4 frag(VertexOutput i) : COLOR {
///////// Vectors:
//                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
//                clip(_MainTex_var.a - 0.5);
//                SHADOW_COLLECTOR_FRAGMENT(i)
//            }
//            ENDCG
//        }
//        Pass {
//            Name "ShadowCaster"
//            Tags {
//                "LightMode"="ShadowCaster"
//            }
//            Cull Off
//            Offset 1, 1
//            
//            Fog {Mode Off}
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #define UNITY_PASS_SHADOWCASTER
//            #include "UnityCG.cginc"
//            #include "Lighting.cginc"
//            #pragma fragmentoption ARB_precision_hint_fastest
//            #pragma multi_compile_shadowcaster
//            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
//            #pragma target 3.0
//            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
//            struct VertexInput {
//                float4 vertex : POSITION;
//                float2 texcoord0 : TEXCOORD0;
//            };
//            struct VertexOutput {
//                V2F_SHADOW_CASTER;
//                float2 uv0 : TEXCOORD1;
//            };
//            VertexOutput vert (VertexInput v) {
//                VertexOutput o = (VertexOutput)0;
//                o.uv0 = v.texcoord0;
//                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//                TRANSFER_SHADOW_CASTER(o)
//                return o;
//            }
//            fixed4 frag(VertexOutput i) : COLOR {
///////// Vectors:
//                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
//                clip(_MainTex_var.a - 0.5);
//                SHADOW_CASTER_FRAGMENT(i)
//            }
//            ENDCG
//        }
//        
        
           
         
	      
    }
      
    FallBack "Diffuse"
}
