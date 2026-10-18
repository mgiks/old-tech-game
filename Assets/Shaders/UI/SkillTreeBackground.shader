Shader "OldTech/UI/SkillTreeBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TopColor ("Center Color", Color) = (0.05, 0.07, 0.16, 1)
        _BottomColor ("Edge Color", Color) = (0.0, 0.0, 0.02, 1)
        [HDR] _GridColor ("Grid Color", Color) = (0.15, 0.35, 0.8, 1)
        [HDR] _WaveColor ("Wave Color", Color) = (0.2, 1.0, 0.7, 1)
        [HDR] _StarColor ("Star Color", Color) = (0.8, 0.9, 1.0, 1)
        _GridScale ("Grid Scale", Float) = 7
        _WaveSpeed ("Wave Speed", Float) = 0.35
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.06

        _Center ("Tree Center", Vector) = (0, 0, 0, 0)
        _Zoom ("Zoom", Float) = 1
        _FxTime ("Fx Time", Float) = 0
        _ShockCenter ("Shock Center", Vector) = (0, 0, 0, 0)
        _ShockTime ("Shock Time", Float) = -100
        [HDR] _ShockColor ("Shock Color", Color) = (0.3, 1, 0.8, 1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _TopColor;
            float4 _BottomColor;
            float4 _GridColor;
            float4 _WaveColor;
            float4 _StarColor;
            float _GridScale;
            float _WaveSpeed;
            float _ScanlineStrength;
            float4 _Center;
            float _Zoom;
            float _FxTime;
            float4 _ShockCenter;
            float _ShockTime;
            float4 _ShockColor;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Stars(float2 p, float density, float t)
            {
                float2 id = floor(p);
                float2 f = frac(p) - 0.5;
                float h = Hash21(id);
                float2 offset = float2(Hash21(id + 17.1), Hash21(id + 31.7)) - 0.5;
                float d = length(f - offset * 0.7);
                float star = smoothstep(0.08, 0.0, d) * step(1.0 - density, h);
                float twinkle = 0.55 + 0.45 * sin(t * (1.5 + h * 4.0) + h * 60.0);
                return star * twinkle * (0.5 + h);
            }

            float4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = (uv - 0.5) * float2(aspect, 1.0);
                float t = _FxTime;
                float2 c = _Center.xy;
                float zoom = max(_Zoom, 0.01);

                float r = length(p);
                float3 col = lerp(_TopColor.rgb, _BottomColor.rgb, saturate(r * 1.25));

                float2 g = (p - c) * _GridScale / zoom;
                float2 gw = max(fwidth(g), 1e-4);
                float2 gl = abs(frac(g + 0.5) - 0.5) / gw;
                float gridLine = 1.0 - saturate(min(gl.x, gl.y));
                float2 cd = abs(frac(g + 0.5) - 0.5);
                float dots = smoothstep(0.07, 0.0, length(cd));

                float dist = length(p - c) / zoom;
                float wave = frac(dist * 1.1 - t * _WaveSpeed);
                float ring = smoothstep(0.0, 0.03, wave) * (1.0 - smoothstep(0.03, 0.14, wave));
                ring *= exp(-dist * 0.9);

                col += _GridColor.rgb * (gridLine * 0.12 + dots * 0.35);
                col += _WaveColor.rgb * ring * (gridLine * 1.6 + dots * 2.5 + 0.08);

                float2 farStars = (p - c * 0.35) * 18.0;
                float2 nearStars = (p - c * 0.6) * 9.0;
                col += _StarColor.rgb * (Stars(farStars, 0.12, t) * 0.6 + Stars(nearStars, 0.07, t) * 1.2);

                float shockAge = t - _ShockTime;
                if (shockAge >= 0.0 && shockAge < 1.6)
                {
                    float fade = 1.0 - shockAge / 1.6;
                    float radius = shockAge * 1.4;
                    float sd = abs(length(p - _ShockCenter.xy) - radius);
                    float width = 0.015 + shockAge * 0.05;
                    col += _ShockColor.rgb * smoothstep(width, 0.0, sd) * fade * 3.0;
                    col += _ShockColor.rgb * (gridLine + dots) * smoothstep(width * 4.0, 0.0, sd) * fade * 2.0;
                }

                col *= (1.0 - _ScanlineStrength) + _ScanlineStrength * sin(uv.y * _ScreenParams.y * 3.14159);
                col *= 1.0 - smoothstep(0.45, 1.05, r) * 0.65;
                col += (Hash21(uv * _ScreenParams.xy + frac(t) * 100.0) - 0.5) * 0.02;

                return float4(col, 1.0) * IN.color;
            }
        ENDCG
        }
    }
}
