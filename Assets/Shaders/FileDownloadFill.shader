Shader "OldTech/FileDownloadFill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Highlight ("Highlight", Color) = (1, 0.7, 0.22, 0.85)
        _Fill ("Fill", Range(0, 1)) = 0
        _Flash ("Flash", Range(0, 1)) = 0
        _FillBottom ("Fill Bottom", Float) = -0.5
        _FillHeight ("Fill Height", Float) = 1
        _PixelsPerUnit ("Pixels Per Unit", Float) = 100
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "Download2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex FileDownloadVert
            #pragma fragment FileDownloadFrag
            #include "FileDownloadFill.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DownloadForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex FileDownloadVert
            #pragma fragment FileDownloadFrag
            #include "FileDownloadFill.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DownloadUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex FileDownloadVert
            #pragma fragment FileDownloadFrag
            #include "FileDownloadFill.hlsl"
            ENDHLSL
        }
    }
}
