Shader "PAV/URP/Unlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        _BaseMapArray("Albedo Array", 2DArray) = "white"{}
        [MainColor]   _BaseColor("Color", Color) = (1, 1, 1, 1)
        _OutlineColor("OutlineColor", Color) = (0,0,0,1)
        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5
        _BaseColorAmplify("BaseColorAmplify", Float) = 1.0
        _MipBias("_MipBias", Float) = 0

            // BlendMode
            [HideInInspector] _Surface("__surface", Float) = 0.0
            [HideInInspector] _Blend("__blend", Float) = 0.0
            [HideInInspector] _AlphaClip("__clip", Float) = 0.0
            [HideInInspector] _SrcBlend("Src", Float) = 1.0
            [HideInInspector] _DstBlend("Dst", Float) = 0.0
            [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
            [HideInInspector] _Cull("__cull", Float) = 2.0

            // Editmode props
            [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

            // ObsoleteProperties
            [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
            [HideInInspector] _Color("Base Color", Color) = (0.5, 0.5, 0.5, 1)
            [HideInInspector] _SampleGI("SampleGI", float) = 0.0 // needed from bakedlit
    }
        SubShader
            {
                Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel" = "4.5"}
                LOD 100

                Blend[_SrcBlend][_DstBlend]
                ZWrite[_ZWrite]
                Cull[_Cull]

                Pass
                {
                    Name "Unlit"

                    HLSLPROGRAM
                    #pragma target 4.5

                    #pragma vertex vert
                    #pragma fragment frag
                    //#pragma shader_feature_local_fragment _ALPHATEST_ON
                    //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

                    // 
                    //#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
                    //#pragma multi_compile _ PAV_MERGED_TEXTURE
                    //#pragma shader_feature_local _SECOND_BASEMAP

                    // -------------------------------------
                    // Unity defined keywords
                    //#pragma multi_compile_fog
                    //#pragma multi_compile_instancing
                    //#pragma multi_compile _ DOTS_INSTANCING_ON

                    #include "./Universal/Shaders/UnlitInput.hlsl"

                struct Attributes
                {
                    float4 positionOS       : POSITION;
                    float2 uv               : TEXCOORD0;
    #if defined(_SECOND_BASEMAP)
                    float2 uv2          : TEXCOORD2;
    #endif
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    PAV_VERTEX_ID
                };

                struct Varyings
                {
                    float3 uv        : TEXCOORD0;
                    float fogCoord : TEXCOORD1;
                    float4 vertex : SV_POSITION;

    #if defined(_SECOND_BASEMAP)
                    float2 uv2      : TEXCOORD2;
    #endif

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                Varyings vert(Attributes input)
                {
                    Varyings output = (Varyings)0;

                    PAV_GET_VERTEX_P(input.vid, input.positionOS);

                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_TRANSFER_INSTANCE_ID(input, output);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                    output.vertex = vertexInput.positionCS;
                    output.uv.xy = TRANSFORM_TEX(input.uv, _BaseMap);
                    PAV_GET_MATERIAL_INDEX(input.vid, output.uv);

    #if defined(_SECOND_BASEMAP)
                    output.uv2 = input.uv2;
    #endif

                    output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                    return output;
                }

                half4 frag(Varyings input) : SV_Target
                {
                    PAV_FLIP_UV_Y(input.uv);
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    PAV_GET_MATERIAL_DATA(input.uv.z);
                    PAV_GET_SECOND_UV(input);
                    PAV_GET_CUTOFF(cutoff);
                    PAV_GET_BASE_COLOR(baseColor);
                    PAV_GET_SHADER_TYPE(shaderType);

                    half2 uv = input.uv.xy;
                    half4 texColor = PAV_SAMPLE_ALBEDO_ALPHA(uv);
                    half3 color = ApplyAlbedo(texColor.rgb, baseColor.rgb, shaderType);
                    half alpha = texColor.a * baseColor.a;
                    AlphaDiscard(alpha, cutoff);

    #ifdef _ALPHAPREMULTIPLY_ON
                    color *= alpha;
    #endif

                    color = MixFog(color, input.fogCoord);

                    return half4(color, alpha);
                }
                ENDHLSL
            }
                //Pass
                //{
                //    Name "LodLoutline"
                //    Tags{"LightMode" = "AvatarLodOutline"}
                //
                //    Cull Front
                //    ZWrite On
                //    ColorMask RGB
                //    Blend SrcAlpha OneMinusSrcAlpha
                //
                //    HLSLPROGRAM
                //
                //    // -------------------------------------
                //    // Pipeline Keywords
                //    #pragma multi_compile PAV_AVATAR_LOD_OUTLINE
                //    #pragma shader_feature_vertex PAV_VERTEX_FROM_BUFFER
                //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
                //    #pragma multi_compile _ PAV_MERGED_TEXTURE
                //
                //    #pragma multi_compile_fog
                //
                //    #pragma vertex LitPassVertexSimple
                //    #pragma fragment LitPassFragmentSimple
                //
                //    #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
                //    #include "./Universal/Shaders/PavSimpleLitForwardPass.hlsl"
                //
                //    ENDHLSL
                //}
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

                    // 
                    // #pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
                    // #pragma multi_compile _ PAV_MERGED_TEXTURE

                    // -------------------------------------
                    // Material Keywords
                    //#pragma shader_feature_local_fragment _ALPHATEST_ON

                    //--------------------------------------
                    // GPU Instancing
                    //#pragma multi_compile_instancing
                    //#pragma multi_compile _ DOTS_INSTANCING_ON

                    #pragma multi_compile _ _ENABLE_STATIC_MESH_BATCHING

                    #include "./Universal/Shaders/UnlitInput.hlsl"
                    #include "./Universal/Shaders/PavDepthOnlyPass.hlsl"
                    ENDHLSL
                }

                    //// This pass it not used during regular rendering, only for lightmap baking.
                    //Pass
                    //{
                    //    Name "Meta"
                    //    Tags{"LightMode" = "Meta"}
                    //
                    //    Cull Off
                    //
                    //    HLSLPROGRAM
                    //    #pragma target 4.5
                    //
                    //    #pragma vertex UniversalVertexMeta
                    //    #pragma fragment UniversalFragmentMetaUnlit
                    //    //.
                    //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
                    //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
                    //    #pragma multi_compile _ PAV_MERGED_TEXTURE
                    //
                    //    #include "./Universal/Shaders/UnlitInput.hlsl"
                    //    #include "./Universal/Shaders/UnlitMetaPass.hlsl"
                    //
                    //    ENDHLSL
                    //}
            }

                FallBack "Hidden/Universal Render Pipeline/FallbackError"
                    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}
