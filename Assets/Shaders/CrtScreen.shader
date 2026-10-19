Shader "OldTech/CrtScreen"
{
    Properties
    {
        _MainTex ("Screen", 2D) = "black" {}
        _Scan ("Scan Strength", Range(0, 1)) = 0.28
        _Vignette ("Vignette", Range(0, 1)) = 0.45
        _Chroma ("Chroma", Range(0, 1)) = 0.35
        _Noise ("Noise", Range(0, 1)) = 0.06
        _Flicker ("Flicker", Range(0, 1)) = 0.04
        _Glass ("Glass", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "MonitorForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "CrtScreen.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "MonitorUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "CrtScreen.hlsl"
            ENDHLSL
        }
    }
}
