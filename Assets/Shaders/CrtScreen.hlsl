#ifndef OLDTECH_CRT_SCREEN_INCLUDED
#define OLDTECH_CRT_SCREEN_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
float _Scan;
float _Vignette;
float _Chroma;
float _Noise;
float _Flicker;
float _Glass;
CBUFFER_END

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float3 positionWS : TEXCOORD2;
};

Varyings Vert(Attributes input)
{
    Varyings output;
    VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = position.positionCS;
    output.positionWS = position.positionWS;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.uv = input.uv;
    return output;
}

float Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

half4 Frag(Varyings input) : SV_Target
{
    float2 uv = input.uv;
    float2 centered = uv - 0.5;
    float radial = dot(centered, centered);

    float2 chroma = centered * radial * _Chroma * 0.18;
    float red = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + chroma).r;
    float green = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).g;
    float blue = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - chroma).b;
    float3 color = float3(red, green, blue);

    float scan = lerp(1.0, 0.72 + 0.28 * sin(uv.y * 820.0 + _Time.y * 2.0), _Scan);
    float interlace = lerp(1.0, 0.9 + 0.1 * sin(input.positionCS.y * 3.14159), _Scan);
    color *= scan * interlace;

    float vignette = smoothstep(0.92, 0.28, length(centered * float2(1.15, 1.35)));
    color *= lerp(1.0, lerp(0.42, 1.0, vignette), _Vignette);

    float grain = Hash(uv * float2(1920.0, 1080.0) + _Time.y * 17.0);
    color += (grain - 0.5) * _Noise;

    float flicker = 1.0 - _Flicker * 0.5 + _Flicker * sin(_Time.y * 53.0) * 0.5;
    color *= flicker;

    float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
    float fresnel = pow(1.0 - saturate(dot(normalize(input.normalWS), viewDir)), 2.4);
    float streak = smoothstep(0.18, 0.0, abs(uv.y - 0.78)) * smoothstep(0.05, 0.45, uv.x) * smoothstep(0.95, 0.55, uv.x);
    color += (fresnel * 0.55 + streak * 0.22) * _Glass;

    float edge = saturate(radial * 2.2 - 0.35);
    color *= lerp(1.0, 0.82, edge);

    return half4(saturate(color), 1);
}

#endif
