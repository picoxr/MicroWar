Shader "PAV/BuiltIn/PavColorTemperature"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
#ifdef UNITY_UV_STARTS_AT_TOP
                //o.uv.y = 1.0 - o.uv.y;
#endif
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // From PC to Pico JDI screen.

                float3x3 colorConvert = float3x3(
                1.08620299728099, -0.0890268759392285, -0.00702073839516210,
                0.0163238132661583, 0.794161363862696, 0.0267542035815233,
                0.000618114675098461, -0.00267007458512025, 0.745179042910558
                );

                // float3x3 colorConvert = float3x3(
                // 1.02135321698904, -0.0624349286439725, 0.0141160214651746,
                // 0.0733614397152257, 0.762328359482322, 0.0649614626121963,
                // 0.00843026851797903, 0.00257098249999859, 0.685835024019548
                // );
                
                colorConvert = transpose(colorConvert);
                col.rgb = mul(col.rgb, colorConvert);
                //col = fixed4(1.0, 0.0, 0.0, 1.0);

                return col;
            }
            ENDCG
        }
    }
}
