Shader "FREE Food Pack/Food-New"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _FresnelSize ("FresnelSize", Range(0.5, 5)) = 0.6153846
        _FresnelIntensity ("FresnelIntensity", Float ) = 1
        _FresnelColor ("FresnelColor", Color) = (0.5,0.5,0.5,1)
        _push ("push", Range(0, 0.01)) = 0
        _Speed ("Speed", Float ) = 1
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
        }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _FresnelSize;
            float _FresnelIntensity;
            float4 _FresnelColor;
            float _push;
            float _Speed;
        CBUFFER_END
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };

            VertexOutput vert (VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
                o.normalDir = TransformObjectToWorldNormal(v.normal);
                
                // Vertex offset animation
                float time = _Time.y * _Speed;
                float offset = (_push * (sin(time) * 0.5 + 0.5));
                v.vertex.xyz += offset * v.normal;
                
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            float4 frag(VertexOutput i) : SV_Target
            {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                
                // Fresnel effect
                float fresnel = pow(1.0 - max(0, dot(normalDirection, viewDirection)), _FresnelSize);
                float3 fresnelColor = _FresnelColor.rgb * fresnel * _FresnelIntensity;
                
                // Main texture
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv0);
                
                // Final color
                float3 finalColor = fresnelColor + mainTex.rgb;
                return float4(finalColor, 1);
            }
            ENDHLSL
        }
        
        // Shadow caster pass for proper shadow reception
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            
            struct VertexInputShadow
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };

            float3 ApplyVertexOffset(float3 position, float3 normal)
            {
                float time = _Time.y * _Speed;
                float offset = (_push * (sin(time) * 0.5 + 0.5));
                return position + offset * normal;
            }

            VertexInputShadow ShadowPassVertex(VertexInputShadow v)
            {
                VertexInputShadow o;
                v.vertex.xyz = ApplyVertexOffset(v.vertex.xyz, v.normal);
                o.vertex = TransformWorldToHClip(TransformObjectToWorld(v.vertex.xyz));
                o.normal = v.normal;
                o.texcoord0 = v.texcoord0;
                return o;
            }

            half4 ShadowPassFragment(VertexInputShadow i) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}