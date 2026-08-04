Shader "UI/CircleGlow"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SoftEdge ("Soft Edge", Range(0, 0.5)) = 0.02
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SoftEdge;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                o.color  = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex    = tex2D(_MainTex, i.uv);
                float2 center = float2(0.5, 0.5);
                float dist    = distance(i.uv, center);
                float alpha   = smoothstep(0.5, 0.5 - _SoftEdge, dist);
                clip(alpha - 0.001);
                return fixed4(i.color.rgb * tex.rgb, i.color.a * alpha * tex.a);
            }
            ENDCG
        }
    }
}
