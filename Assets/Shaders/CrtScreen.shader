Shader "OldTech/CrtScreen"
{
    Properties
    {
        _MainTex ("Screen", 2D) = "black" {}
        _Scan ("Scan Strength", Range(0, 1)) = 0.1
        _Vignette ("Vignette", Range(0, 1)) = 0.16
        _Chroma ("Chroma", Range(0, 1)) = 0.06
        _Noise ("Noise", Range(0, 1)) = 0.015
        _Flicker ("Flicker", Range(0, 1)) = 0.012
        _Glass ("Glass", Range(0, 1)) = 0.12
        _FitScale ("Fit Scale", Vector) = (1, 1, 0, 0)
        _FitOffset ("Fit Offset", Vector) = (0, 0, 0, 0)
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
