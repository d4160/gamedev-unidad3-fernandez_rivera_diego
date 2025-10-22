// Unity Game Developer Master - Solución de Shader de Resaltado (Revisión Final v5)
// Shader: HighlightOutline_v5.shader
// Objetivo: Versión final y 100% funcional. Incluye la librería Lit.hlsl completa
// para una compatibilidad PBR perfecta con URP.

Shader "Master/URP/HighlightOutline_v5"
{
    Properties
    {
        [Header(Surface Options)]
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        
        [Header(Outline Options)]
        _OutlineColor("Outline Color", Color) = (1, 0.5, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0.02
        [HideInInspector] _OutlineIntensity("Outline Intensity", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        // ------------------------------------------------------------------
        // PASE 1: Renderizado PBR del objeto principal.
        // ------------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTERED_RENDERING

            // --- CORRECCIÓN FINAL: Incluir la librería Lit completa. ---
            // Esto define 'InitializeStandardLitSurfaceData' y todas las demás
            // funciones necesarias para un pase PBR robusto.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lit.hlsl"

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                SurfaceData surfaceData;
                // Esta función ahora existirá gracias a la inclusión de Lit.hlsl
                InitializeStandardLitSurfaceData(input.uv, surfaceData);

                half4 albedo = SAMPLE_TEXTURED_2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.alpha = albedo.a;

                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------
        // PASE 2: Renderizado del borde (Outline). (Sin cambios)
        // ------------------------------------------------------------------
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Cull Front
            ZTest Always 
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor; float _OutlineWidth; float _OutlineIntensity;
            CBUFFER_END

            Varyings OutlineVertex(Attributes input)
            {
                Varyings output;
                float4 positionOS = input.positionOS;
                positionOS.xyz += normalize(input.normalOS) * _OutlineWidth * _OutlineIntensity;
                output.positionHCS = TransformObjectToHClip(positionOS.xyz);
                return output;
            }

            half4 OutlineFragment(Varyings input) : SV_Target { return half4(_OutlineColor.rgb, _OutlineColor.a * _OutlineIntensity); }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}