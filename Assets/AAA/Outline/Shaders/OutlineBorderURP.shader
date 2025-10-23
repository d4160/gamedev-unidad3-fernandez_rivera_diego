Shader "AAA/Effects/URP/PerObjectOutline"
{
    Properties
    {
        [MainColor]_OutlineColor("Outline Color (HDR)", Color) = (1,0.6,0,1)
        _OutlineThickness("Thickness (Pixels)", Float) = 2.0
        _WorldThickness("Thickness (World Units)", Float) = 0.01
        [Toggle(_OUTLINE_WORLD_SPACE)] _UseWorldSpace("Use World Space Thickness", Float) = 0
        [Toggle(_OUTLINE_FRESNEL)] _UseFresnel("Use Fresnel", Float) = 0
        _FresnelPower("Fresnel Power", Float) = 3.0
        _FresnelBias("Fresnel Bias", Float) = 0.0
        [Toggle(_OUTLINE_PULSE)] _UsePulse("Use Pulse", Float) = 0
        _PulseAmplitude("Pulse Amplitude", Float) = 0.2
        _PulseSpeed("Pulse Speed", Float) = 3.0
        _EdgeSoftness("Edge Softness (Alpha Fade)", Range(0,1)) = 0.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4 // 4 = LEqual
        [Enum(Off,0,On,1)] _ZWrite("ZWrite", Float) = 0
        [Enum(Back,2,Front,1,Off,0)] _Cull("Cull", Float) = 1 // "Front": render backfaces => silhouette
        _Alpha("Alpha", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry+1"
            "IgnoreProjector"="True"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "OutlinePass"
            Tags { "LightMode"="UniversalForward" }

            Cull [_Cull]
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma target 3.0

            // Feature toggles
            #pragma shader_feature_local _OUTLINE_WORLD_SPACE
            #pragma shader_feature_local _OUTLINE_FRESNEL
            #pragma shader_feature_local _OUTLINE_PULSE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Per-material constant buffer (SRP Batcher)
            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineThickness;
                float _WorldThickness;
                float _FresnelPower;
                float _FresnelBias;
                float _PulseAmplitude;
                float _PulseSpeed;
                float _EdgeSoftness;
                float _ZTest;
                float _ZWrite;
                float _Cull;
                float _Alpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 viewNormal   : TEXCOORD0;
                float3 worldNormal  : TEXCOORD1;
                float  outlineMask  : TEXCOORD2; // for fresnel/softness
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Helpers
            float3 TransformObjectToWorldDirSafe(float3 dirOS)
            {
                return normalize(TransformObjectToWorldDir(dirOS));
            }

            Varyings Vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 nWS   = TransformObjectToWorldNormal(input.normalOS);
                nWS = normalize(nWS);

                #ifdef _OUTLINE_WORLD_SPACE
                    // Extrusión en unidades del mundo.
                    posWS += nWS * _WorldThickness;
                    float4 posCS = TransformWorldToHClip(posWS);
                #else
                    // Extrusión en pantalla (grosor en píxeles) usando espacio de vista.
                    float3 posVS = TransformWorldToView(posWS);
                    float3 nVS   = TransformWorldToViewDir(nWS);

                    // Dirección en pantalla: usamos la proyección del normal en XY
                    float2 dir2 = nVS.xy;
                    float dirLen = max(length(dir2), 1e-5);
                    dir2 /= dirLen;

                    // p11 = cot(fovY/2) en matriz de proyección
                    float p11 = UNITY_MATRIX_P._m11;

                    // Escala de píxel en espacio de vista (derivada de la proyección en Y)
                    // dy_v por pixel = (2 / screenHeight) * (|z| / p11)
                    float viewZ = abs(posVS.z);
                    float dyPerPixel = (2.0f / _ScreenParams.y) * (viewZ / p11);

                    // Grosor final en vista (en unidades de vista)
                    float2 offsetVS = dir2 * (_OutlineThickness * dyPerPixel);

                    // Aplicar extrusión en vista (solo XY)
                    posVS.xy += offsetVS;

                    // Volver a clip
                    float4 posCS = mul(UNITY_MATRIX_P, float4(posVS, 1.0));
                #endif

                o.positionCS  = posCS;
                o.viewNormal  = TransformWorldToViewDir(nWS);
                o.worldNormal = nWS;

                // outlineMask por ahora 1; se ajusta en Frag para softness/fresnel
                o.outlineMask = 1.0;

                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // Fresnel opcional para variar intensidad de color/borde
                float fres = 1.0;
                #ifdef _OUTLINE_FRESNEL
                    float3 V = float3(0,0,1); // en espacio de vista, la cámara mira hacia -Z; usamos magnitud XY para borde
                    float ndotv = saturate(1.0 - abs(i.viewNormal.z)); // aprox borde cuando normal es perpendicular a vista
                    fres = pow(saturate(ndotv + _FresnelBias), _FresnelPower);
                #endif

                // Pulso opcional
                float pulse = 1.0;
                #ifdef _OUTLINE_PULSE
                    pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmplitude;
                #endif

                // Softness: atenuamos alpha
                float alphaSoft = lerp(1.0, saturate(fres), _EdgeSoftness);

                float4 col = _OutlineColor;
                col.rgb *= fres * pulse;
                col.a = col.a * _Alpha * alphaSoft;

                return half4(col);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
