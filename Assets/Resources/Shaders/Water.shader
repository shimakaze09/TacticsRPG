Shader "Custom/URP_StylizedWater_TileToon"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.1,0.55,0.8,1)
        _ShallowColor("Shallow Color", Color) = (0.2,0.75,0.9,1)
        _DeepColor("Deep Color", Color) = (0.02,0.12,0.18,1)
        _HighlightColor("Highlight Color", Color) = (0.9,0.95,1.0,1)

        _WaveTiling("Wave Tiling", Vector) = (1,1,0,0)
        _WaveSpeed("Wave Speed", Vector) = (0.3,0.25,0,0)
        _WaveHeight("Wave Height", Range(0,1)) = 0.2
        _NoiseStrength("Noise Strength", Range(0,2)) = 1.0

        _SecondaryTiling("Secondary Noise Tiling", Vector) = (2,2,0,0)
        _SecondarySpeed("Secondary Noise Speed", Vector) = (0.1,0.15,0,0)
        _SecondaryStrength("Secondary Noise Strength", Range(0,2)) = 0.6

        _RippleTiling("Rippling Tiling", Float) = 14.0
        _RippleSpeed("Rippling Speed", Float) = 2.0
        _RippleStrength("Rippling Strength", Range(0,1)) = 0.08

        _NormalStrength("Normal Strength", Range(0,2)) = 1.0
        _FresnelPower("Fresnel Power", Range(0.1,10)) = 2.5
        _Specular("Specular Intensity", Range(0,2)) = 0.8
        _Smoothness("Smoothness", Range(0,1)) = 0.85
        _TimeScale("Global Time Scale", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 300
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float3 localPos : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            // === Properties ===
            float4 _BaseColor, _ShallowColor, _DeepColor, _HighlightColor;
            float4 _WaveTiling, _WaveSpeed;
            float _WaveHeight, _NoiseStrength;

            float4 _SecondaryTiling, _SecondarySpeed;
            float _SecondaryStrength;

            float _RippleTiling, _RippleSpeed, _RippleStrength;
            float _NormalStrength, _FresnelPower;
            float _Specular, _Smoothness, _TimeScale;

            // --- Noise functions ---
            float hash(float n) { return frac(sin(n) * 43758.5453); }
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f*f*(3.0-2.0*f);
                float n = i.x + i.y * 57.0;
                return lerp(lerp(hash(n+0.0), hash(n+1.0), f.x),
                            lerp(hash(n+57.0), hash(n+58.0), f.x), f.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                for(int i=0;i<5;i++)
                {
                    v = v * noise(p) + a; // multiply instead of add
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }

            // === Waves using tile-local coordinates ===
            float sampleWaves(float3 localPos, float time)
            {
                float2 uv = localPos.xz * _WaveTiling.xy + _WaveSpeed.xy * time;
                float mainNoise = fbm(uv) * _NoiseStrength;

                float2 uv2 = localPos.xz * _SecondaryTiling.xy + _SecondarySpeed.xy * time * 1.5;
                float secondaryNoise = fbm(uv2*2.0) * _SecondaryStrength;

                return mainNoise * secondaryNoise * _WaveHeight; // multiply for irregularity
            }

            float sampleRipple(float3 localPos, float time)
            {
                float2 uv = localPos.xz * _RippleTiling + _RippleSpeed * time;
                float r = sin((uv.x + uv.y)*6.28318 + time) * 0.5 + 0.5;
                return r * _RippleStrength;
            }

            float3 computeNormal(float3 localPos, float time)
            {
                float eps = 0.05;
                float h = sampleWaves(localPos,time);
                float hx = sampleWaves(localPos + float3(eps,0,0), time);
                float hz = sampleWaves(localPos + float3(0,0,eps), time);
                float dhdx = (hx-h)/eps;
                float dhdz = (hz-h)/eps;

                float3 n = normalize(float3(-dhdx,1.0,-dhdz));
                n += sampleRipple(localPos,time) * float3(0.2,0.05,0.2) * _NormalStrength;
                return normalize(n);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 localPos = v.vertex.xyz;
                float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                float time = _Time.y * _TimeScale;
                float h = sampleWaves(localPos,time);
                localPos += worldNormal * h;

                float3 worldPos = mul(unity_ObjectToWorld,float4(localPos,1)).xyz;
                o.localPos = v.vertex.xyz;
                o.worldPos = worldPos;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.pos = mul(UNITY_MATRIX_VP,float4(worldPos,1));
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float time = _Time.y * _TimeScale;
                float3 n = computeNormal(i.localPos,time);
                float3 viewDir = normalize(i.viewDir);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                float diff = saturate(dot(n,lightDir));
                float3 H = normalize(lightDir + viewDir);
                float spec = pow(saturate(dot(n,H)), lerp(8,64,_Smoothness)) * _Specular;
                float fres = pow(1.0 - saturate(dot(n,viewDir)), _FresnelPower);

                float h = sampleWaves(i.localPos,time);
                float depthFactor = saturate(0.5 + h*2.0);

                // --- Toon shading: quantize colors ---
                float3 color = lerp(_DeepColor.rgb, _ShallowColor.rgb, depthFactor);
                color = lerp(color, _BaseColor.rgb, diff);
                color = lerp(color, _HighlightColor.rgb, saturate((h-(_WaveHeight*0.35))*8.0));

                // Quantize (3 levels)
                color = floor(color*3.0)/3.0;

                // Add specular + fresnel
                color += spec*0.8 + fres*0.25*_HighlightColor.rgb;

                float alpha = saturate(0.55 + fres*0.45);
                return float4(color,alpha);
            }

            ENDHLSL
        }
    }
    FallBack Off
}
