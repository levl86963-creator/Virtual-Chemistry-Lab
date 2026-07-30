// Glass Panel Shader — VR Chem Lab
// URP-compatible transparent surface with:
//   • Adjustable transparency / refraction tint
//   • Fresnel-driven cyan edge glow
//   • Subtle specular highlight
// Assign this shader to the GlassPanel material that sits flush on the lab table.

Shader "VRChemLab/GlassPanel"
{
    Properties
    {
        // ── Base glass ────────────────────────────────────────────────────────
        [Header(Glass Surface)]
        _BaseColor      ("Base Tint",         Color)      = (0.75, 0.95, 1.0, 0.15)
        _Smoothness     ("Smoothness",         Range(0,1)) = 0.92
        _Metallic       ("Metallic",           Range(0,1)) = 0.05

        // ── Edge / Fresnel glow ───────────────────────────────────────────────
        [Header(Edge Glow)]
        _EdgeColor      ("Edge Glow Color",    Color)      = (0.0, 0.9, 1.0, 1.0)
        _EdgePower      ("Edge Fresnel Power", Range(0.5,8)) = 3.0
        _EdgeIntensity  ("Edge Glow Intensity",Range(0,5)) = 1.8

        // ── Emission / inner glow ─────────────────────────────────────────────
        [Header(Emission)]
        [HDR]
        _EmissionColor  ("Emission Color",     Color)      = (0.0, 0.25, 0.3, 1.0)

        // ── Render settings ───────────────────────────────────────────────────
        [HideInInspector] _SrcBlend   ("__src",  Float) = 5   // SrcAlpha
        [HideInInspector] _DstBlend   ("__dst",  Float) = 10  // OneMinusSrcAlpha
        [HideInInspector] _ZWrite     ("__zw",   Float) = 0
        [HideInInspector] _Cull       ("__cull", Float) = 0   // Off — double-sided glass
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        // ── Forward Lit pass ─────────────────────────────────────────────────
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend   [_SrcBlend] [_DstBlend]
            ZWrite  [_ZWrite]
            Cull    [_Cull]

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ── Properties ───────────────────────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EdgeColor;
                half  _EdgePower;
                half  _EdgeIntensity;
                half4 _EmissionColor;
                half  _Smoothness;
                half  _Metallic;
            CBUFFER_END

            // ── Structs ───────────────────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float  fogFactor   : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ── Vertex ────────────────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = nrmInputs.normalWS;
                OUT.fogFactor   = ComputeFogFactor(posInputs.positionCS.z);

                return OUT;
            }

            // ── Fragment ──────────────────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                // Normalize interpolated normal
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));

                // ── Fresnel edge glow ─────────────────────────────────────────
                half fresnel     = pow(1.0h - saturate(dot(N, V)), _EdgePower);
                half3 edgeGlow   = _EdgeColor.rgb * fresnel * _EdgeIntensity;

                // ── Simple diffuse from main light ────────────────────────────
                Light mainLight  = GetMainLight();
                half  NdotL      = saturate(dot(N, mainLight.direction));
                half3 diffuse    = _BaseColor.rgb * mainLight.color * NdotL;

                // ── Specular (Blinn-Phong, cheap) ─────────────────────────────
                float3 H         = normalize(V + mainLight.direction);
                half   NdotH     = saturate(dot(N, H));
                half   specPow   = exp2(_Smoothness * 10.0h + 1.0h);
                half3  specular  = mainLight.color * pow(NdotH, specPow) * _Smoothness;

                // ── Combine ───────────────────────────────────────────────────
                half3 color = diffuse + specular + edgeGlow + _EmissionColor.rgb;

                // Alpha: base glass alpha boosted at edges
                half alpha = _BaseColor.a + fresnel * _EdgeColor.a * 0.5h;
                alpha = saturate(alpha);

                half4 result = half4(color, alpha);
                result.rgb = MixFog(result.rgb, IN.fogFactor);
                return result;
            }
            ENDHLSL
        }

        // ── Shadow caster (transparent — no shadows cast) ─────────────────────
        // Intentionally omitted so the glass doesn't cast harsh shadows.
    }

    // Fallback for non-URP projects
    Fallback "Universal Render Pipeline/Lit"
}
