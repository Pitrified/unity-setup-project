// One step of the water's ripple height field, blitted from the previous state into the next
// by Game.Gameplay.Water.WaterRipples (two render textures, ping-pong). R holds the height now,
// G the height one step ago; each step is the 2D wave equation, damped, plus this step's impulses.
// See docs/systems/water.md.
Shader "unity-setup/WaterRippleSim"
{
    Properties
    {
        _MainTex ("Previous state", 2D) = "black" {}
        _Damping ("Damping per step", Float) = 0.97
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "Step"

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment Frag
            #pragma target 3.0

            #define MAX_IMPULSES 16

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Damping;
            float4 _Impulses[MAX_IMPULSES]; // uv x, uv y, radius in uv, strength
            float _ImpulseCount;

            float4 Frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 texel = _MainTex_TexelSize.xy;
                float2 here = tex2D(_MainTex, uv).rg;
                float neighbours = tex2D(_MainTex, uv + float2(texel.x, 0)).r + tex2D(_MainTex, uv - float2(texel.x, 0)).r
                                 + tex2D(_MainTex, uv + float2(0, texel.y)).r + tex2D(_MainTex, uv - float2(0, texel.y)).r;
                float next = (neighbours * 0.5 - here.g) * _Damping;

                for (int k = 0; k < MAX_IMPULSES; k++)
                {
                    if (k >= (int)_ImpulseCount)
                    {
                        break;
                    }
                    float4 imp = _Impulses[k];
                    float d = length(uv - imp.xy) / max(imp.z, 1e-4);
                    next += imp.w * saturate(1.0 - d * d);
                }

                // Absorb at the border, so ripples leave the area instead of bouncing back.
                float edge = min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y));
                next *= saturate(edge * 25.0);
                return float4(next, here.r, 0, 0);
            }
            ENDCG
        }
    }
}
