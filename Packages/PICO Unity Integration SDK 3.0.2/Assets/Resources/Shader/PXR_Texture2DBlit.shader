﻿Shader "PXR_SDK/PXR_Texture2DBlit" {
    Properties{
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _premultiply("Pre-multiply alpha", Int) = 0
    }
    SubShader{
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        Pass{
            ZWrite Off
            ColorMask RGBA

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    half2 texcoord : TEXCOORD0;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                int _premultiply;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    return o;
                }

                fixed4 frag (v2f i) : COLOR
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord);

                    if (_premultiply)
                        col.rgb *= col.a;

                    return col;
                }
            ENDCG
        }
    }
}
