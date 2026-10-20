#ifndef OLDTECH_FILE_DOWNLOAD_FILL_INCLUDED
#define OLDTECH_FILE_DOWNLOAD_FILL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct Attributes
{
    float3 positionOS : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    half4 color : COLOR;
    float2 uv : TEXCOORD0;
    float localY : TEXCOORD1;
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    half4 _Highlight;
    float _Fill;
    float _Flash;
    float _FillBottom;
    float _FillHeight;
    float _PixelsPerUnit;
CBUFFER_END

Varyings FileDownloadVert(Attributes input)
{
    Varyings output;
    output.positionCS = TransformObjectToHClip(input.positionOS);
    output.uv = input.uv;
    output.color = input.color;
    output.localY = input.positionOS.y;
    return output;
}

half4 FileDownloadFrag(Varyings input) : SV_Target
{
    half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    half4 col = tex * input.color;

    float height = max(_FillHeight, 0.0001);
    float rows = max(height * max(_PixelsPerUnit, 1.0), 1.0);
    float ny = saturate((input.localY - _FillBottom) / height);
    float row = min(floor(ny * rows), rows - 1.0);
    float filled = step(row, _Fill * rows - 0.001);

    half3 warm = saturate(col.rgb * 1.18 + _Highlight.rgb * _Highlight.a);
    col.rgb = lerp(col.rgb, warm, filled);
    col.rgb = lerp(col.rgb, half3(1.0, 0.96, 0.82), _Flash);
    col.a = tex.a * input.color.a;
    return col;
}

#endif
