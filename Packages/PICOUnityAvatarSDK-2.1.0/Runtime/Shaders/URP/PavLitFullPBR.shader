Shader"PAV/URP/PicoPBR"
{
    Properties
    {
        [Enum(Pico.Avatar.AvatarShaderType)]_ShaderType("ShaderType", Float) = 0
        // Specular vs Metallic workflow
        [KeywordEnum(URP_STANDARD, UBERSTANDARD, UBERSKIN, UBERHAIR, UBERFABRIC, UBEREYE, NPRSTANDARD, NPRSKIN, NPRHAIR, NPRFABRIC, NPREYE)] PAV_LITMODE("Lit mode", Float) = 0
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0

        _AdditiveGI("AdditiveGI", Range(0.0, 1.0)) = 0.0

        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
         _ColorRegionMap("ColorRegionMap", 2D) = "black" {}
         _BodyMaskMap("BodyMaskMap (not used in shader)",2D) = "black" {}

        _UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        _ColorRegion1("ColorRegion1", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
        _ColorRegion2("ColorRegion2", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
        _ColorRegion3("ColorRegion3", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
        _ColorRegion4("ColorRegion4", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1

        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _OutlineColor("OutlineColor", Color) = (0,0,0,1)
        

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness("Smoothness", Range(1.0, 1.0)) = 1.0
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _Metallic("Metallic", Range(1.0, 1.0)) = 1.0
        _MetallicGlossMap("Metallic", 2D) = "black" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0


        _SpecularAAScreenSpaceVariance("Specular AA Screen Space Variance", Range(0,1)) = 0.1
        _SpecularAAThreshold("Specular AA Threshold", Range(0,1)) = 0.2

        _BaseColorAmplify("BaseColorAmplify", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax("Scale", Range(0.005, 0.08)) = 0.005
        _ParallaxMap("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMapScale("Scale", Range(0.0, 2.0)) = 1.0
        _DetailAlbedoMap("Detail Albedo x2", 2D) = "linearGrey" {}
        _DetailNormalMapScale("Scale", Range(0.0, 2.0)) = 1.0
        [Normal] _DetailNormalMap("Detail Normal Map", 2D) = "bump" {}

        _CustomVec_0("_CustomVec_0", Vector) = (0.1, 0.2, 1.0, 1)
        _LaserRampNoise("Laser RampNoise", 2D) = "white" {}
        _LaserColorRamp("Laser ColorRamp", 2D) = "white" {}

        _MipBias("_MipBias", Float) = -1

        _CustomVec_0("", Vector) = (0.0, 0.0, 0.0, 0.0)
        _CustomVec_1("", Vector) = (0.0, 0.0, 0.0, 0.0)
        _CustomVec_2("", Vector) = (0.0, 0.0, 0.0, 0.0)
        _CustomVec_3("", Vector) = (0.0, 0.0, 0.0, 0.0)
        _CustomVec_4("", Vector) = (0.0, 0.0, 0.0, 0.0)
        _CustomVec_5("", Vector) = (0.0, 0.0, 0.0, 0.0)
        _CustomVec_6("", Vector) = (0.0, 0.0, 0.0, 0.0)
        _CustomVec_7("", Vector) = (0.0, 0.0, 0.0, 0.0)
        _CustomVec_8("", Vector) = (0.0, 0.0, 0.0, 0.0)

        // For static mesh batching
        [HideInInspector] _BaseMapArray("BaseMap Array", 2DArray) = "white"{}
        [HideInInspector] _ColorRegionMapArray("ColorRegionMap Array", 2DArray) = "white"{}
        [HideInInspector] _BumpMapArray("BumpMap Array", 2DArray) = "bump"{}
        [HideInInspector] _MetallicGlossMapArray("MetallicGlossMap Array", 2DArray) = "black"{}

        // SRP batching compatibility for Clear Coat (Not used in Lit)
        [HideInInspector] _ClearCoatMask("_ClearCoatMask", Float) = 0.0
        [HideInInspector] _ClearCoatSmoothness("_ClearCoatSmoothness", Float) = 0.0

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        //[HideInInspector] _SrcBlend("__src", Float) = 1.0
        //[HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _ColorMask("__colorMask", Float) = 15.0

        _ReceiveShadows("Receive Shadows", Float) = 1.0
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlossMapScale("Smoothness", Float) = 0.0
        [HideInInspector] _Glossiness("Smoothness", Float) = 0.0
        [HideInInspector] _GlossyReflections("EnvironmentReflections", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

        // PAV_CustomData1("PAV_CustomData1", Vector) = (0,0,0,0)
        // PAV_CustomData2("PAV_CustomData2", Vector) = (0,0,0,0)
        // PAV_CustomData3("PAV_CustomData3", Vector) = (0,0,0,0)
        // PAV_CustomData4("PAV_CustomData4", Vector) = (0,0,0,0)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcAlphaBlend ("Src Alpha Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstAlphaBlend ("Dst Alpha Blend", Float) = 0
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }
        LOD 300

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog 
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend],[_SrcAlphaBlend][_DstAlphaBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]
            //ColorMask[_ColorMask]

            HLSLPROGRAM
            #pragma target 4.5

            // -------------------------------------
            // Pipeline Keywords.
            #pragma multi_compile PAV_LIT_FULL_PBR
            //#pragma multi_compile PAV_LITMODE_URPSTANDARD PAV_LITMODE_UBERSTANDARD PAV_LITMODE_UBERSKIN PAV_LITMODE_UBERHAIR PAV_LITMODE_UBERFABRIC PAV_LITMODE_UBEREYE PAV_LITMODE_NPRSTANDARD PAV_LITMODE_NPRSKIN PAV_LITMODE_NPRHAIR PAV_LITMODE_NPRFABRIC PAV_LITMODE_NPREYE
            #pragma multi_compile PAV_LITMODE_UBERSTANDARD
            #pragma multi_compile _ PAV_COLOR_REGION_BAKED
            #pragma multi_compile _ _ALPHATEST_ON

            // -------------------------------------
            // Material Keywords
            #pragma multi_compile _NORMALMAP
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            //#pragma shader_feature_local_fragment _EMISSION
            #pragma multi_compile _METALLICSPECGLOSSMAP
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //#pragma shader_feature_local_fragment _OCCLUSIONMAP
            //#pragma shader_feature_local _PARALLAXMAP
            //#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            //#pragma multi_compile _ _SPECULARHIGHLIGHTS_OFF
            //#pragma multi_compile _  _ENVIRONMENTREFLECTIONS_OFF
            //#pragma multi_compile _  _SPECULAR_SETUP
            //#pragma shader_feature_local _SECOND_BASEMAP
            //#pragma shader_feature_local_fragment _SECOND_NORMALMAP
            //#pragma shader_feature_local_fragment _SECOND_METALLICSPECGLOSSMAP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ADDITIONAL_LIGHTS
            //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT
            //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords.
            //#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            //#pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            //#pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON
            //#pragma enable_d3d11_debug_symbols

            // For static mesh batching
            #pragma multi_compile _ _ENABLE_STATIC_MESH_BATCHING

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
    
            #include "./Universal/Shaders/PavLitInput.hlsl"
            #include "./Universal/Shaders/PavLitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            // -------------------------------------
            // Pipeline Keywords
            #pragma multi_compile PAV_LIT_FULL_PBR
            //#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
            //#pragma multi_compile _ PAV_MERGED_TEXTURE

            // -------------------------------------
            // Material Keywords
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            //#pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "./Universal/Shaders/PavLitInput.hlsl"
            #include "./Universal/Shaders/PavShadowCasterPass.hlsl"
            ENDHLSL
        }

        //Pass
        //{
        //    // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
        //    // no LightMode tag are also rendered by Universal Render Pipeline
        //    Name "GBuffer"
        //    Tags{"LightMode" = "UniversalGBuffer"}
        //
        //    ZWrite[_ZWrite]
        //    ZTest LEqual
        //    Cull[_Cull]
        //
        //    HLSLPROGRAM
        //    #pragma target 4.5
        //
        //    // -------------------------------------
        //    // Pipeline Keywords
        //    #pragma multi_compile PAV_LIT_FULL_PBR
        //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
        //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
        //    #pragma multi_compile _ PAV_MERGED_TEXTURE
        //
        //    // -------------------------------------
        //    // Material Keywords
        //    #pragma multi_compile _ _NORMALMAP
        //    #pragma shader_feature_local_fragment _ALPHATEST_ON
        //    //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
        //    #pragma shader_feature_local_fragment _EMISSION
        //    #pragma multi_compile _ _METALLICSPECGLOSSMAP
        //    #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //    #pragma shader_feature_local_fragment _OCCLUSIONMAP
        //    #pragma shader_feature_local _PARALLAXMAP
        //    #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
        //
        //    #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
        //    #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
        //    #pragma shader_feature_local_fragment _SPECULAR_SETUP
        //    #pragma shader_feature_local _SECOND_BASEMAP
        //    #pragma shader_feature_local_fragment _SECOND_NORMALMAP
        //    #pragma shader_feature_local_fragment _SECOND_METALLICSPECGLOSSMAP
        //    #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
        //
        //    // -------------------------------------
        //    // Universal Pipeline keywords
        //    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        //    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        //    #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        //    #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        //    #pragma multi_compile _ _SHADOWS_SOFT
        //    #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        //
        //    // -------------------------------------
        //    // Unity defined keywords
        //    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        //    #pragma multi_compile _ LIGHTMAP_ON
        //    #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        //
        //    //--------------------------------------
        //    // GPU Instancing
        //    //#pragma multi_compile_instancing
        //    //#pragma multi_compile _ DOTS_INSTANCING_ON
        //
        //    #pragma vertex LitGBufferPassVertex
        //    #pragma fragment LitGBufferPassFragment
        //
        //    #include "./Universal/Shaders/PavLitInput.hlsl"
        //    #include "./Universal/Shaders/PavLitGBufferPass.hlsl"
        //    ENDHLSL
        //}

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Pipeline Keywords..
            #pragma multi_compile PAV_LIT_FULL_PBR
            //#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
            //#pragma multi_compile _ PAV_MERGED_TEXTURE
            // -------------------------------------
            // Material Keywords..
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            //#pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #include "./Universal/Shaders/PavLitInput.hlsl"
            #include "./Universal/Shaders/PavDepthOnlyPass.hlsl"
            ENDHLSL
        }

        //// This pass is used when drawing to a _CameraNormalsTexture texture
        //Pass
        //{
        //    Name "DepthNormals"
        //    Tags{"LightMode" = "DepthNormals"}
        //
        //    ZWrite On
        //    Cull[_Cull]
        //
        //    HLSLPROGRAM
        //    #pragma target 4.5
        //
        //    #pragma vertex DepthNormalsVertex
        //    #pragma fragment DepthNormalsFragment
        //
        //    // -------------------------------------
        //    // Pipeline Keywords..
        //    #pragma multi_compile PAV_LIT_FULL_PBR
        //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
        //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
        //    #pragma multi_compile _ PAV_MERGED_TEXTURE
        //
        //    // -------------------------------------
        //    // Material Keywords..
        //    #pragma multi_compile _ _NORMALMAP
        //    #pragma shader_feature_local_fragment _ALPHATEST_ON
        //    #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //
        //    //--------------------------------------
        //    // GPU Instancing
        //    //#pragma multi_compile_instancing
        //    //#pragma multi_compile _ DOTS_INSTANCING_ON
        //
        //    #include "./Universal/Shaders/PavLitInput.hlsl"
        //    #include "./Universal/Shaders/PavDepthNormalsPass.hlsl"
        //    ENDHLSL
        //}

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
        //    // -------------------------------------
        //    // Pipeline Keywords
        //    #pragma multi_compile PAV_LIT_FULL_PBR
        //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
        //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
        //    #pragma multi_compile _ PAV_MERGED_TEXTURE
        //
        //    #pragma vertex UniversalVertexMeta
        //    #pragma fragment UniversalFragmentMeta
        //
        //    #pragma shader_feature_local_fragment _SPECULAR_SETUP
        //    #pragma shader_feature_local_fragment _EMISSION
        //    #pragma multi_compile _ _METALLICSPECGLOSSMAP
        //    #pragma shader_feature_local_fragment _ALPHATEST_ON
        //    #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //    #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
        //
        //    #pragma shader_feature_local_fragment _SPECGLOSSMAP
        //    #pragma shader_feature_local _SECOND_BASEMAP
        //    //#pragma shader_feature_local_fragment _SECOND_NORMALMAP
        //    #pragma shader_feature_local_fragment _SECOND_METALLICSPECGLOSSMAP
        //
        //    #include "./Universal/Shaders/PavLitInput.hlsl"
        //    #include "./Universal/Shaders/PavLitMetaPass.hlsl"
        //
        //    ENDHLSL
        //}
        //Pass
        //{
        //    Name "Universal2D"
        //    Tags{ "LightMode" = "Universal2D" }
        //
        //    Blend[_SrcBlend][_DstBlend]
        //    ZWrite[_ZWrite]
        //    Cull[_Cull]
        //
        //    HLSLPROGRAM
        //    #pragma target 4.5
        //
        //    // -------------------------------------
        //    // Pipeline Keywords
        //    #pragma multi_compile PAV_LIT_FULL_PBR
        //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
        //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
        //    #pragma multi_compile _ PAV_MERGED_TEXTURE
        //    
        //    #pragma vertex vert
        //    #pragma fragment frag
        //    #pragma shader_feature_local_fragment _ALPHATEST_ON
        //    #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
        //
        //    #include "./Universal/Shaders/PavLitInput.hlsl"
        //    #include "./Universal/Shaders/Utils/Universal2D.hlsl"
        //    ENDHLSL
        //}
    }

        FallBack "Hidden/Universal Render Pipeline/FallbackError"
        CustomEditor "Pico.Avatar.URPShaderInspector"
}

