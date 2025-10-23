Shader "UI/CrosshairPulse"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PulseSpeed ("Pulse Speed", Range(0.1, 5)) = 1.0
        _PulseMin ("Pulse Min Brightness", Range(0, 1)) = 0.8
        _PulseMax ("Pulse Max Brightness", Range(1, 3)) = 1.2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [GetActive]
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _PulseSpeed;
            float _PulseMin;
            float _PulseMax;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Calcular el factor de pulso usando una onda sinusoidal basada en el tiempo
                float pulseFactor = sin(_Time.y * _PulseSpeed);
                // Mapear la onda de [-1, 1] a [0, 1]
                pulseFactor = pulseFactor * 0.5 + 0.5;
                // Mapear el resultado al rango [_PulseMin, _PulseMax]
                float brightness = lerp(_PulseMin, _PulseMax, pulseFactor);

                fixed4 textureColor = tex2D(_MainTex, IN.texcoord) * IN.color;
                textureColor.rgb *= brightness; // Aplicar el brillo solo a los canales de color
                return textureColor;
            }
            ENDCG
        }
    }
}