Shader "PAV/AvatarEditor/FakePointLight"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        [Space(5)]
        _CenterPosition("_CenterPosition", Vector) = (0.0, 0.0, 2.0)
        [Space(5)]
        _ShadowFadeRange("_ShadowFadeRange", Range(0.1, 10.0)) = 1.2
        [Space(5)]
        _ShadowScale("_ShadowScale", Range(-1.0, 1.0)) = 1

        [Header(__________Base Light________________________)]
        [Space(10)]
        _Color("基础颜色Color(RGBA)", Color) = (0,0,0,0)

        [Header(__________Top Light________________________)]
        [Space(10)]
        _TopLightColor("顶部光Color(RGBA)", Color) = (1,1,1,1)
        _TopLightPosition("位置(x,y,scale,scale)", Vector) = (0,1.0,1.0,1.0)
        _TopLightParam("参数(半径,pow,0,0)", Vector) = (2.0,3.0,0,0)

       [Toggle(_ENABLE_TEXTURE)] _EnableTexture("EnableTexture", Float) = 0
    }

    SubShader
    {
           Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel" = "4.5"
                    "Queue" = "Background"
                }
           
           Blend[_SrcBlend][_DstBlend]
           ZWrite[_ZWrite]
           Cull[_Cull]
           
           Pass
           {
               Name "Unlit"
               Tags{"LightMode" = "UniversalForward"}

               HLSLPROGRAM
            
                #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
              
                // -------------------------------------
                // Universal Pipeline keywords.
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
                #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
                #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile_fragment _ _SHADOWS_SOFT
                #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
                #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
                #pragma multi_compile _ SHADOWS_SHADOWMASK

               #pragma target 4.5
           
               #pragma vertex vert
               #pragma fragment frag
           

                // -------------------------------------
                // Unity defined keywords
                #pragma multi_compile_fog
                
                #include "../Universal/Shaders/PavSimpleLitInput.hlsl"
                #include "../Universal/Shaders/PavSimpleLitForwardPass.hlsl"
                
                struct ExAttributes
                {
                    float4 positionOS    : POSITION;
                    float2 texcoord      : TEXCOORD0;
                };

                struct ExVaryings
                {
                    float4 vertex   : SV_POSITION;
                    float3 uv       : TEXCOORD0;
                    float fogCoord  : TEXCOORD1;
                    float4 positionWS : TEXCOORD2;
                    float4 shadowCoord  : TEXCOORD3;
                };
                
                uniform float4 _CenterPosition;
                uniform float _ShadowFadeRange;
                uniform float _ShadowScale;
                //
                float4 _Color;
                //
                float4 _TopLightColor;
                float4 _TopLightPosition;
                float4 _TopLightParam;
                //
                float3 HUEtoRGB(in float H)
                {
                    float R = abs(H * 6 - 3) - 1;
                    float G = 2 - abs(H * 6 - 2);
                    float B = 2 - abs(H * 6 - 4);
                    return saturate(float3(R, G, B));
                }

                static const float Epsilon = 1e-10;

                float3 RGBtoHCV(in float3 RGB)
                {
                    // Based on work by Sam Hocevar and Emil Persson
                    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
                    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
                    float C = Q.x - min(Q.w, Q.y);
                    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
                    return float3(H, C, Q.x);
                }

                float3 HSVtoRGB(in float3 HSV)
                {
                    float3 RGB = HUEtoRGB(HSV.x);
                    return ((RGB - 1) * HSV.y + 1) * HSV.z;
                }

                float3 RGBtoHSV(in float3 RGB)
                {
                    float3 HCV = RGBtoHCV(RGB);
                    float S = HCV.y / (HCV.z + Epsilon);
                    return float3(HCV.x, S, HCV.z);
                }

                ExVaryings vert(ExAttributes input)
                {
                    ExVaryings output = (ExVaryings)0;
                    output.positionWS = float4(TransformObjectToWorld(input.positionOS.xyz), 1);
                    float4 positionCS = TransformWorldToHClip(output.positionWS.xyz);
                    output.vertex = positionCS;
                    output.uv.xy = TRANSFORM_TEX(input.texcoord, _BaseMap);
                    output.fogCoord = ComputeFogFactor(positionCS.z);
                    output.shadowCoord = TransformWorldToShadowCoord(output.positionWS.xyz);

                    return output;
                }
                
                half4 frag(ExVaryings input) : SV_Target
                {
                    float2 screenPos = input.uv.xy * 2.0 - 1.0;
                    if (_ProjectionParams.x > 0) {
                        //screenPos.y *= -1.0;
                    }

                    //
                    float shadow = 1.0;
#ifdef _MAIN_LIGHT_SHADOWS
                    shadow = MainLightShadow(input.shadowCoord, input.positionWS, 1, _MainLightOcclusionProbes);

                    float shadowRange = saturate(length(input.positionWS.xyz - _CenterPosition.xyz) * (1.0 / _ShadowFadeRange));
                    
                    //
                    shadow += (1.0 - shadow) * _ShadowScale;
                    shadow = lerp(shadow, 1, shadowRange);
#endif

                    // x:(-1, 1) y:(-1,1)
                    float4 gray = _Color;
                    float a;
                    float3 hsv;

                    // first light
                    a = 1.001 - saturate(length((screenPos.xy - _TopLightPosition.xy) * _TopLightPosition.zw) / _TopLightParam.x);
                    hsv = RGBtoHSV(_TopLightColor.xyz);
                    hsv.z *= _TopLightColor.a * pow(a, _TopLightParam.y);
                    gray.xyz += HSVtoRGB(hsv.xyz);


#ifdef _ENABLE_TEXTURE
                    float4 ret = tex2D(_BaseMap, input.texcoord);
                    return float4(ret.xyz * gray.xyz, 1.0);
#else
                    // 混合颜色.
                    return float4(gray.xyz * shadow, 1);/// float4(gray, gray, gray, 1.0);
#endif
                }
                ENDHLSL
            }

            Pass
            {
                Name "DepthOnly"
                Tags{"LightMode" = "DepthOnly"}

                ZWrite On
                ColorMask 0

                HLSLPROGRAM
                #pragma target 4.5

                #pragma vertex DepthOnlyVertex
                #pragma fragment DepthOnlyFragment

                // -------------------------------------
                // Material Keywords
                #pragma shader_feature_local_fragment _ALPHATEST_ON

                #include "../Universal/Shaders/UnlitInput.hlsl"
                #include "../Universal/Shaders/PavDepthOnlyPass.hlsl"
                ENDHLSL
            }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}
