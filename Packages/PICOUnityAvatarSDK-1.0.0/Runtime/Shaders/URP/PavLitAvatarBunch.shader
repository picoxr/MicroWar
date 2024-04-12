Shader "PAV/URP/LitAvatarBunch"
{
    Properties
    {
        // Specular vs Metallic workflow
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0

        _AdditiveGI("AdditiveGI", Range(0.0, 1.0)) = 0.0

        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        _ColorRegionMap("ColorRegionMap", 2D) = "black" {}
        _UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        _ColorRegion1("ColorRegion1", Vector) = (0,0,0,0)
        _ColorRegion2("ColorRegion2", Vector) = (0,0,0,0)
        _ColorRegion3("ColorRegion3", Vector) = (0,0,0,0)
        _ColorRegion4("ColorRegion4", Vector) = (0,0,0,0)

        _BaseMapArray("Albedo Array", 2DArray) = "white"{}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _OutlineColor("OutlineColor", Color) = (0,0,0,1)

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        _BaseColorAmplify("BaseColorAmplify", Float) = 0.8

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
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}
        
        _CustomVec_0("_CustomVec_0", Vector) = (0.1, 0.2, 1.0, 1)
        _LaserRampNoise("Laser RampNoise", 2D) = "white" {}
        _LaserColorRamp("Laser ColorRamp", 2D) = "white" {}

        _MipBias("_MipBias", Float) = 0

        // SRP batching compatibility for Clear Coat (Not used in Lit)
        [HideInInspector] _ClearCoatMask("_ClearCoatMask", Float) = 0.0
        [HideInInspector] _ClearCoatSmoothness("_ClearCoatSmoothness", Float) = 0.0

            // Blending state
            [HideInInspector] _Surface("__surface", Float) = 0.0
            [HideInInspector] _Blend("__blend", Float) = 0.0
            [HideInInspector] _AlphaClip("__clip", Float) = 0.0
            [HideInInspector] _SrcBlend("__src", Float) = 1.0
            [HideInInspector] _DstBlend("__dst", Float) = 0.0
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
    }

        SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"}
        LOD 300

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]
            ColorMask[_ColorMask]

            HLSLPROGRAM
            #pragma target 4.5

            //-------------------------------------
            //Pipeline Keywords . 
            #pragma multi_compile PAV_AVATAR_BUNCH
            #pragma multi_compile PAV_LIT_ONLY_DIFFUSE
            #pragma multi_compile PAV_COLOR_REGION_BAKED
            #pragma multi_compile PAV_NO_TANGENTS

            // -------------------------------------
            // Material Keywords
            //#pragma multi_compile _ _NORMALMAP
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            //#pragma shader_feature_local_fragment _EMISSION
            //#pragma multi_compile _ _METALLICSPECGLOSSMAP
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //#pragma shader_feature_local_fragment _OCCLUSIONMAP
            //#pragma shader_feature_local _PARALLAXMAP
            //#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            //#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            //#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            //#pragma shader_feature_local_fragment _SPECULAR_SETUP
            //#pragma shader_feature_local _SECOND_BASEMAP
            //#pragma shader_feature_local_fragment _SECOND_NORMALMAP
            //#pragma shader_feature_local_fragment _SECOND_METALLICSPECGLOSSMAP
            //#pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            ///#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ADDITIONAL_LIGHTS
            //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT
            //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            //#pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimplest

            #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
            #include "./Universal/Shaders/PavSimpleLitForwardPass.hlsl"
            ENDHLSL
        }

        //Pass
        //{
        //    Name "LodLoutline"
        //    Tags{"LightMode" = "AvatarLodOutline"}
        //    
        //    //Cull Front
        //    ZWrite On
        //    ColorMask RGB
        //    Blend SrcAlpha OneMinusSrcAlpha
        //
        //    HLSLPROGRAM
        //
        //    // -------------------------------------
        //    // Pipeline Keywords
        //    #pragma multi_compile PAV_AVATAR_EXTRUDE_OUTLINE
        //    #pragma shader_feature_vertex PAV_VERTEX_FROM_BUFFER
        //    #pragma shader_feature_local PAV_MERGED_TEXTURE
        //
        //    #pragma multi_compile_fog
        //
        //    #pragma vertex LitPassVertexSimple
        //    #pragma fragment LitPassFragmentSimple
        //
        //    #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
        //    #include "./Universal/Shaders/PavSimpleLitForwardPass.hlsl"
        //
        // ENDHLSL
        //}

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
            //Pipeline Keywords
            #pragma multi_compile PAV_LIT_ONLY_DIFFUSE
            #pragma multi_compile PAV_AVATAR_BUNCH
            //#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
            //#pragma multi_compile _ PAV_MERGED_TEXTURE

            // -------------------------------------
            // Material Keywords
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
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
        //    //Pipeline Keywords
        //    #pragma multi_compile PAV_LIT_ONLY_DIFFUSE
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
        //    #pragma multi_compile_instancing
        //    #pragma multi_compile _ DOTS_INSTANCING_ON
        //
        //     #pragma vertex LitPassVertexSimple
        //    #pragma fragment LitPassFragmentSimple
        //
        //    #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
        //    #include "./Universal/Shaders/PavSimpleLitGBufferPass.hlsl"
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

            // -------------------------------------
            //Pipeline Keywords..
            #pragma multi_compile PAV_LIT_ONLY_DIFFUSE
            #pragma multi_compile PAV_AVATAR_BUNCH
            //#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
            //#pragma multi_compile _ PAV_MERGED_TEXTURE

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords..
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
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
        //    // -------------------------------------
        //    // Pipeline Keywords
        //    #pragma multi_compile PAV_LIT_ONLY_DIFFUSE
        //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
        //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
        //    #pragma multi_compile _ PAV_MERGED_TEXTURE
        //
        //    #pragma vertex DepthNormalsVertex
        //    #pragma fragment DepthNormalsFragment
        //
        //    // -------------------------------------
        //    // Material Keywords
        //    #pragma multi_compile _ _NORMALMAP
        //    #pragma shader_feature_local_fragment _ALPHATEST_ON
        //    #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //
        //    //--------------------------------------
        //    // GPU Instancing
        //    #pragma multi_compile_instancing
        //    #pragma multi_compile _ DOTS_INSTANCING_ON
        //
        //    #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
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
        //    // -------------------------------------
        //    // Pipeline Keywords
        //    #pragma multi_compile PAV_LIT_ONLY_DIFFUSE
        //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
        //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
        //    #pragma multi_compile _ PAV_MERGED_TEXTURE
        //
        //    #pragma vertex UniversalVertexMeta
        //    #pragma fragment UniversalFragmentMetaSimple
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
        //    #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
        //    #include "./Universal/Shaders/PavSimpleLitMetaPass.hlsl"
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
        //    #pragma multi_compile PAV_LIT_ONLY_DIFFUSE
        //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
        //    //#pragma shader_feature_local PAV_MERGED_TEXTURE
        //    #pragma multi_compile _ PAV_MERGED_TEXTURE
        //    
        //    #pragma vertex vert
        //    #pragma fragment frag
        //    #pragma shader_feature_local_fragment _ALPHATEST_ON
        //    #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
        //
        //    #include "./Universal/Shaders/PavSimpleLitInput.hlsl"
        //    #include "./Universal/Shaders/Utils/Universal2D.hlsl" 
        //    ENDHLSL
        //}
    }

        //FallBack "Hidden/Universal Render Pipeline/FallbackError"
    //CustomEditor "Pico.Avatar.URPShaderInspector"
}
