Shader "artificial-pi/Water"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.22, 0.62, 0.82, 1.0)
        _DeepColor ("Deep Color", Color) = (0.05, 0.22, 0.50, 1.0)
        _FoamColor ("Foam/Highlight Color", Color) = (0.80, 0.92, 1.0, 1.0)
        _WaveSpeed ("Wave Speed", Float) = 0.30
        _WaveFreq ("Wave Frequency", Float) = 1.80
        _WaveAmp ("Wave Amplitude", Float) = 0.15
        _PatternScale ("Pattern Scale", Float) = 3.0
        _FoamThreshold ("Foam Threshold", Range(0.0, 1.0)) = 0.72
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100
        Cull Back
        ZWrite On

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FoamColor;
                float _WaveSpeed;
                float _WaveFreq;
                float _WaveAmp;
                float _PatternScale;
                float _FoamThreshold;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float  fogFactor   : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // Two-axis sine wave for vertex displacement.
                float t = _Time.y * _WaveSpeed;
                float wave = sin(_WaveFreq * IN.positionOS.x + t)
                           * cos(_WaveFreq * IN.positionOS.z + t * 0.7);
                float3 pos = IN.positionOS.xyz;
                pos.y += wave * _WaveAmp;

                float4 clipPos = TransformObjectToHClip(pos);
                OUT.positionHCS = clipPos;
                OUT.uv = IN.uv;
                OUT.fogFactor = ComputeFogFactor(clipPos.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y * _WaveSpeed;

                // Scroll UVs in two directions for animated surface.
                float2 uvA = IN.uv * _PatternScale + float2(t * 0.10, t * 0.06);
                float2 uvB = IN.uv * _PatternScale * 0.7 + float2(-t * 0.07, t * 0.09);

                float patA = sin(uvA.x * 6.2831) * 0.5 + 0.5;
                float patB = cos(uvB.y * 6.2831) * 0.5 + 0.5;
                float pattern = patA * patB;

                // Depth blend.
                half4 waterColor = lerp(_DeepColor, _ShallowColor, pattern);

                // Foam highlight on pattern peaks.
                float foamMask = saturate((pattern - _FoamThreshold) / (1.0 - _FoamThreshold));
                waterColor.rgb = lerp(waterColor.rgb, _FoamColor.rgb, foamMask * 0.45);

                // Apply URP fog.
                waterColor.rgb = MixFog(waterColor.rgb, IN.fogFactor);
                return waterColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
