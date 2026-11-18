// Water surface for URP, mobile-minded. See docs/systems/water.md.
// Waves, colours and the clock are globals set by Game.Gameplay.Water.WaterSurface every frame;
// GerstnerSum below must stay the same sum as WaveField.Displacement in C#, so floating
// things sit on the surface drawn here.
Shader "unity-setup/Water"
{
    Properties
    {
        [NoScaleOffset] _NormalMap ("Detail normal map (tiling)", 2D) = "bump" {}
        _RippleStrength ("Ripple normal strength", Float) = 0.6
        _RippleFoam ("Ripple foam", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_NormalMap);      SAMPLER(sampler_NormalMap);
            TEXTURE2D(_WaterDepthMap);  SAMPLER(sampler_WaterDepthMap);
            TEXTURE2D(_WaterRipples);   SAMPLER(sampler_WaterRipples);

            CBUFFER_START(UnityPerMaterial)
                float _RippleStrength;
                float _RippleFoam;
            CBUFFER_END

            // Globals from WaterSurface (and WaterRipples).
            float4 _WaterWaveA, _WaterWaveB, _WaterWaveC, _WaterWaveD; // dir x, dir z, steepness, wavelength
            float  _WaterTime;
            float  _WaterSeaLevel;
            half4  _WaterShallow, _WaterDeep, _WaterSky, _WaterFoam;
            float4 _WaterDetail;      // normal tiling, normal strength, normal scroll, deep at
            float4 _WaterFoamParams;  // shore foam depth, shore band speed, crest foam, calm depth
            float4 _WaterDepthRect;   // x min, z min, size, depth range
            float4 _WaterRippleRect;  // x min, z min, size (0 when no ripples), texel size in uv

            static const float Gravity = 9.81;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 restXZ     : TEXCOORD2;
                float  crest      : TEXCOORD3;
                float  fogFactor  : TEXCOORD4;
            };

            // One Gerstner wave: adds its displacement and its tangent and binormal terms.
            void Gerstner(float4 wave, float2 p, inout float3 offset, inout float3 tangent, inout float3 binormal)
            {
                float k = 2.0 * PI / wave.w;
                float speed = sqrt(Gravity / k);
                float a = wave.z / k;
                float2 d = wave.xy;
                float f = k * (dot(d, p) - speed * _WaterTime);
                float s = sin(f);
                float c = cos(f);
                offset += float3(d.x * a * c, a * s, d.y * a * c);
                tangent += float3(-d.x * d.x * wave.z * s, d.x * wave.z * c, -d.x * d.y * wave.z * s);
                binormal += float3(-d.x * d.y * wave.z * s, d.y * wave.z * c, -d.y * d.y * wave.z * s);
            }

            void GerstnerSum(float2 p, out float3 offset, out float3 normal)
            {
                offset = 0;
                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);
                Gerstner(_WaterWaveA, p, offset, tangent, binormal);
                Gerstner(_WaterWaveB, p, offset, tangent, binormal);
                Gerstner(_WaterWaveC, p, offset, tangent, binormal);
                Gerstner(_WaterWaveD, p, offset, tangent, binormal);
                normal = normalize(cross(binormal, tangent));
            }

            Varyings Vert(Attributes input)
            {
                Varyings o;
                float3 rest = TransformObjectToWorld(input.positionOS.xyz);
                float3 offset, normal;
                GerstnerSum(rest.xz, offset, normal);

                // Waves calm down over shallow water, so they do not climb onto the land.
                float2 depthUV = (rest.xz - _WaterDepthRect.xy) / _WaterDepthRect.z;
                float depth = SAMPLE_TEXTURE2D_LOD(_WaterDepthMap, sampler_WaterDepthMap, depthUV, 0).r * _WaterDepthRect.w;
                float calm = saturate(depth / max(_WaterFoamParams.w, 1e-3));
                offset *= calm;
                normal = normalize(lerp(float3(0, 1, 0), normal, calm));
                float3 positionWS = float3(rest.x + offset.x, _WaterSeaLevel + offset.y, rest.z + offset.z);

                // How high this point rides relative to the waves' total amplitude: crest foam.
                float amplitude = _WaterWaveA.z * _WaterWaveA.w + _WaterWaveB.z * _WaterWaveB.w
                                + _WaterWaveC.z * _WaterWaveC.w + _WaterWaveD.z * _WaterWaveD.w;
                amplitude = max(amplitude / (2.0 * PI), 1e-3);

                o.positionWS = positionWS;
                o.positionCS = TransformWorldToHClip(positionWS);
                o.normalWS = normal;
                o.restXZ = rest.xz;
                o.crest = saturate(offset.y / amplitude);
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // Seabed depth under this point, metres below the still surface.
                float2 depthUV = (i.restXZ - _WaterDepthRect.xy) / _WaterDepthRect.z;
                float depth = SAMPLE_TEXTURE2D(_WaterDepthMap, sampler_WaterDepthMap, depthUV).r * _WaterDepthRect.w;

                // Detail normals: two scrolls of one tiling map, crossing.
                float tiling = _WaterDetail.x;
                float scroll = _WaterDetail.z * _WaterTime;
                float2 uv1 = i.positionWS.xz * tiling + float2(scroll, scroll * 0.4);
                float2 uv2 = i.positionWS.xz * tiling * 1.7 + float2(-scroll * 0.6, scroll * 0.9);
                half3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv1));
                half3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv2));
                float2 bend = (n1.xy + n2.xy) * _WaterDetail.y;

                // Ripples: a height field around the action; its slope bends the normal, its height foams.
                float ripple = 0;
                if (_WaterRippleRect.z > 0)
                {
                    float2 rippleUV = (i.positionWS.xz - _WaterRippleRect.xy) / _WaterRippleRect.z;
                    float t = _WaterRippleRect.w;
                    float h = SAMPLE_TEXTURE2D(_WaterRipples, sampler_WaterRipples, rippleUV).r;
                    float hx = SAMPLE_TEXTURE2D(_WaterRipples, sampler_WaterRipples, rippleUV + float2(t, 0)).r;
                    float hz = SAMPLE_TEXTURE2D(_WaterRipples, sampler_WaterRipples, rippleUV + float2(0, t)).r;
                    float edge = saturate(min(min(rippleUV.x, rippleUV.y), min(1 - rippleUV.x, 1 - rippleUV.y)) * 10);
                    float texelWorld = t * _WaterRippleRect.z;
                    bend -= float2(hx - h, hz - h) / texelWorld * _RippleStrength * edge;
                    ripple = abs(h) * _RippleFoam * edge;
                }
                float3 n = normalize(float3(i.normalWS.x + bend.x, i.normalWS.y, i.normalWS.z + bend.y));

                // Colour: shallow over sand, deep further out.
                half3 water = lerp(_WaterShallow.rgb, _WaterDeep.rgb, saturate(depth / _WaterDetail.w));

                Light sun = GetMainLight();
                float3 v = normalize(GetWorldSpaceViewDir(i.positionWS));
                float nl = saturate(dot(n, sun.direction));
                half3 lit = water * (0.45 + 0.55 * nl) * sun.color;

                // Sky at grazing angles (Schlick, F0 of water), the sun's glint.
                // Fresnel on a mostly geometric normal: the detail would sparkle sky everywhere at a low angle.
                float3 nf = normalize(lerp(i.normalWS, n, 0.35));
                float fresnel = 0.02 + 0.98 * pow(1.0 - saturate(dot(nf, v)), 5.0);
                half3 colour = lerp(lit, _WaterSky.rgb, fresnel * 0.6);
                float3 h = normalize(sun.direction + v);
                colour += sun.color * pow(saturate(dot(n, h)), 220.0) * 1.6;

                // Foam: a line at the waterline, bands running up the beach, crests, ripples.
                float shoreDepth = _WaterFoamParams.x;
                float t = saturate(depth / shoreDepth);
                float breakup = n1.x * 0.5 + 0.5;
                float band = frac(t * 2.5 + _WaterTime * _WaterFoamParams.y);
                float bands = smoothstep(0.75, 0.95, band) * (1.0 - t) * smoothstep(0.25, 0.75, breakup);
                // The waterline swashes up and down the beach, out of step along the shore.
                float swash = 0.5 + 0.5 * sin(_WaterTime * 1.1 + i.restXZ.x * 0.15 + i.restXZ.y * 0.11);
                float waterline = 1.0 - smoothstep(0.0, 0.08 + 0.14 * swash, depth);
                float crestFoam = smoothstep(0.85, 1.0, i.crest) * _WaterFoamParams.z * smoothstep(0.55, 0.9, breakup);
                float foam = saturate(max(max(bands, waterline), max(crestFoam, saturate(ripple))));
                colour = lerp(colour, _WaterFoam.rgb * (0.6 + 0.4 * nl), foam);

                // Shallow water is see-through to the sand; deep is nearly opaque; none over land.
                float alpha = lerp(0.25, 0.94, saturate(depth / 1.2));
                alpha = max(alpha, foam);
                clip(depth - 0.02);

                colour = MixFog(colour, i.fogFactor);
                return half4(colour, alpha);
            }
            ENDHLSL
        }
    }
}
