Shader "PAV/BuiltIn/PavStandard"
{
    Properties
    {
        _ShaderType("_ShaderType", Float) = 0
        _AdditiveGI("AdditiveGI", Range(0.0, 5.0)) = 0.0
 
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        _BaseMapArray("Albedo Array", 2DArray) = "white"{}
        
         _ColorRegionMap("ColorRegionMap", 2D) = "black" {}
        _UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        _ColorRegion1("ColorRegion1", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
        _ColorRegion2("ColorRegion2", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
        _ColorRegion3("ColorRegion3", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
        _ColorRegion4("ColorRegion4", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
         


        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5 

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0 

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        _MetallicGlossMapArray("Metallic Array", 2DArray) = "white"{}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BaseColorAmplify("BaseColorAmplify", Float) = 1.0 

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {} 
        _BumpMapArray("Normal Array", 2DArray) = "bump"{}    

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}     

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {} 

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0  
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    // 300
    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull[_Cull]

            CGPROGRAM
            #pragma target 3.0 
            // -------------------------------------
            //#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER 
            //#pragma multi_compile _ PAV_MERGED_TEXTURE

            #pragma multi_compile _ _NORMALMAP
            //#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            //#pragma shader_feature_local _DETAIL_MULX2 
            //#pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
            //#pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdbase  
            #pragma multi_compile_fog  
            //#pragma multi_compile_instancing  
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE     

            #pragma vertex vertBase
            #pragma fragment fragBase  
            #include "./CGIncludes/UnityStandardCoreForward.cginc"   

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual
            Cull[_Cull]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------
            //#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
            //#pragma multi_compile _ PAV_MERGED_TEXTURE
            
            #pragma multi_compile _ _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON 
            //#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            //#pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            //#pragma shader_feature_local _DETAIL_MULX2
            //#pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAdd
            #pragma fragment fragAdd 
            #include "./CGIncludes/UnityStandardCoreForward.cginc"   

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------
            //#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
            //#pragma multi_compile _ PAV_MERGED_TEXTURE

            //#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON
            //#pragma shader_feature_local _METALLICGLOSSMAP
            //#pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //#pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            //#pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster  
            #pragma fragment fragShadowCaster 

            #include "./CGIncludes/UnityStandardShadow.cginc"

            ENDCG
        }
        //// ------------------------------------------------------------------
        ////  Deferred pass
        //Pass
        //{
        //    Name "DEFERRED"
        //    Tags { "LightMode" = "Deferred" }

        //    CGPROGRAM
        //    #pragma target 3.0
        //    #pragma exclude_renderers nomrt


        //    // -------------------------------------

        //    #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
        //    #pragma shader_feature_local PAV_MERGED_TEXTURE  

        //    #pragma multi_compile _ _NORMALMAP
        //    #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
        //    #pragma shader_feature _EMISSION
        //    #pragma shader_feature_local _METALLICGLOSSMAP
        //    #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //    #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
        //    #pragma shader_feature_local _DETAIL_MULX2
        //    #pragma shader_feature_local _PARALLAXMAP

        //    #pragma multi_compile_prepassfinal
        //    #pragma multi_compile_instancing
        //    // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
        //    //#pragma multi_compile _ LOD_FADE_CROSSFADE

        //    #pragma vertex vertDeferred
        //    #pragma fragment fragDeferred

        //    #include "./CGIncludes/UnityStandardCore.cginc"

        //    ENDCG
        //}

        //// ------------------------------------------------------------------
        //// Extracts information for lightmapping, GI (emission, albedo, ...)
        //// This pass it not used during regular rendering.
        //Pass
        //{
        //    Name "META"
        //    Tags { "LightMode"="Meta" }

        //    Cull Off

        //    CGPROGRAM
        //    #pragma vertex vert_meta
        //    #pragma fragment frag_meta

        //    #pragma shader_feature _EMISSION
        //    #pragma shader_feature_local _METALLICGLOSSMAP
        //    #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //    #pragma shader_feature_local _DETAIL_MULX2
        //    #pragma shader_feature EDITOR_VISUALIZATION

        //    #include "./CGIncludes/UnityStandardMeta.cginc"
        //    ENDCG
        //}
    }
    
    //// 250
    //SubShader
    //{
    //    Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
    //    LOD 250

    //    // ------------------------------------------------------------------
    //    //  Base forward pass (directional light, emission, lightmaps, ...)
    //    Pass
    //    {
    //        Name "FORWARD"
    //        Tags { "LightMode" = "ForwardBase" }

    //        Blend [_SrcBlend] [_DstBlend]
    //        ZWrite [_ZWrite]
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature _EMISSION
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
    //        // SM2.0: NOT SUPPORTED shader_feature_local _DETAIL_MULX2
    //        // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP

    //        #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

    //        #pragma multi_compile_fwdbase
    //        #pragma multi_compile_fog

    //        #pragma vertex vertBase
    //        #pragma fragment fragBase
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Additive forward pass (one light per pass)
    //    Pass
    //    {
    //        Name "FORWARD_DELTA"
    //        Tags { "LightMode" = "ForwardAdd" }
    //        Blend [_SrcBlend] One
    //        Fog { Color (0,0,0,0) } // in additive pass fog should be black
    //        ZWrite Off
    //        ZTest LEqual
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP
    //        #pragma skip_variants SHADOWS_SOFT

    //        #pragma multi_compile_fwdadd_fullshadows
    //        #pragma multi_compile_fog

    //        #pragma vertex vertAdd
    //        #pragma fragment fragAdd
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Shadow rendering pass
    //    Pass {
    //        Name "ShadowCaster"
    //        Tags { "LightMode" = "ShadowCaster" }

    //        ZWrite On ZTest LEqual

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma skip_variants SHADOWS_SOFT
    //        #pragma multi_compile_shadowcaster

    //        #pragma vertex vertShadowCaster
    //        #pragma fragment fragShadowCaster

    //        #include "./CGIncludes/UnityStandardShadow.cginc"

    //        ENDCG
    //    }

    //    // ------------------------------------------------------------------
    //    // Extracts information for lightmapping, GI (emission, albedo, ...)
    //    // This pass it not used during regular rendering.
    //    Pass
    //    {
    //        Name "META"
    //        Tags { "LightMode"="Meta" }

    //        Cull Off

    //        CGPROGRAM
    //        #pragma vertex vert_meta
    //        #pragma fragment frag_meta

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma shader_feature _EMISSION
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        #pragma shader_feature EDITOR_VISUALIZATION

    //        #include "./CGIncludes/UnityStandardMeta.cginc"
    //        ENDCG
    //    }
    //}
    
    //// 200
    //SubShader
    //{
    //    Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
    //    LOD 200

    //    // ------------------------------------------------------------------
    //    //  Base forward pass (directional light, emission, lightmaps, ...)
    //    Pass
    //    {
    //        Name "FORWARD"
    //        Tags { "LightMode" = "ForwardBase" }

    //        Blend [_SrcBlend] [_DstBlend]
    //        ZWrite [_ZWrite]
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        // #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        // #pragma shader_feature _EMISSION
    //        // #pragma shader_feature_local _METALLICGLOSSMAP
    //        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        // #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
    //        // SM2.0: NOT SUPPORTED shader_feature_local _DETAIL_MULX2
    //        // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP

    //        #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

    //        #pragma multi_compile_fwdbase
    //        #pragma multi_compile_fog

    //        #pragma vertex vertBase
    //        #pragma fragment fragBase
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Additive forward pass (one light per pass)
    //    Pass
    //    {
    //        Name "FORWARD_DELTA"
    //        Tags { "LightMode" = "ForwardAdd" }
    //        Blend [_SrcBlend] One
    //        Fog { Color (0,0,0,0) } // in additive pass fog should be black
    //        ZWrite Off
    //        ZTest LEqual
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        // #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        // #pragma shader_feature_local _METALLICGLOSSMAP
    //        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        // #pragma shader_feature_local _DETAIL_MULX2
    //        // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP
    //        #pragma skip_variants SHADOWS_SOFT

    //        #pragma multi_compile_fwdadd_fullshadows
    //        #pragma multi_compile_fog

    //        #pragma vertex vertAdd
    //        #pragma fragment fragAdd
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Shadow rendering pass
    //    Pass {
    //        Name "ShadowCaster"
    //        Tags { "LightMode" = "ShadowCaster" }

    //        ZWrite On ZTest LEqual

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        // #pragma shader_feature_local _METALLICGLOSSMAP
    //        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma skip_variants SHADOWS_SOFT
    //        #pragma multi_compile_shadowcaster

    //        #pragma vertex vertShadowCaster
    //        #pragma fragment fragShadowCaster

    //        #include "./CGIncludes/UnityStandardShadow.cginc"

    //        ENDCG
    //    }

    //    // ------------------------------------------------------------------
    //    // Extracts information for lightmapping, GI (emission, albedo, ...)
    //    // This pass it not used during regular rendering.
    //    Pass
    //    {
    //        Name "META"
    //        Tags { "LightMode"="Meta" }

    //        Cull Off

    //        CGPROGRAM
    //        #pragma vertex vert_meta
    //        #pragma fragment frag_meta

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        // #pragma shader_feature _EMISSION
    //        // #pragma shader_feature_local _METALLICGLOSSMAP
    //        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        #pragma shader_feature EDITOR_VISUALIZATION

    //        #include "./CGIncludes/UnityStandardMeta.cginc"
    //        ENDCG
    //    }
    //}
    
    //// 150
    //SubShader
    //{
    //    Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
    //    LOD 150


    //    // ------------------------------------------------------------------
    //    //  Base forward pass (directional light, emission, lightmaps, ...)
    //    Pass
    //    {
    //        Name "FORWARD"
    //        Tags { "LightMode" = "ForwardBase" }

    //        Blend [_SrcBlend] [_DstBlend]
    //        ZWrite [_ZWrite]
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 3.0

    //        // -------------------------------------

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature _EMISSION
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
    //        #pragma shader_feature_local _PARALLAXMAP

    //        #pragma multi_compile_fwdbase
    //        #pragma multi_compile_fog
    //        #pragma multi_compile_instancing
    //        // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
    //        //#pragma multi_compile _ LOD_FADE_CROSSFADE

    //        #pragma vertex vertBase
    //        #pragma fragment fragBase
    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Additive forward pass (one light per pass)
    //    Pass
    //    {
    //        Name "FORWARD_DELTA"
    //        Tags { "LightMode" = "ForwardAdd" }
    //        Blend [_SrcBlend] One
    //        Fog { Color (0,0,0,0) } // in additive pass fog should be black
    //        ZWrite Off
    //        ZTest LEqual
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 3.0

    //        // -------------------------------------

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE
            
    //        #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        #pragma shader_feature_local _PARALLAXMAP

    //        #pragma multi_compile_fwdadd_fullshadows
    //        #pragma multi_compile_fog
    //        // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
    //        //#pragma multi_compile _ LOD_FADE_CROSSFADE

    //        #pragma vertex vertAdd
    //        #pragma fragment fragAdd
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Shadow rendering pass
    //    Pass {
    //        Name "ShadowCaster"
    //        Tags { "LightMode" = "ShadowCaster" }

    //        ZWrite On ZTest LEqual

    //        CGPROGRAM
    //        #pragma target 3.0

    //        // -------------------------------------

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE  

    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _PARALLAXMAP
    //        #pragma multi_compile_shadowcaster
    //        #pragma multi_compile_instancing
    //        // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
    //        //#pragma multi_compile _ LOD_FADE_CROSSFADE

    //        #pragma vertex vertShadowCaster
    //        #pragma fragment fragShadowCaster

    //        #include "./CGIncludes/UnityStandardShadow.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Deferred pass
    //    Pass
    //    {
    //        Name "DEFERRED"
    //        Tags { "LightMode" = "Deferred" }

    //        CGPROGRAM
    //        #pragma target 3.0
    //        #pragma exclude_renderers nomrt


    //        // -------------------------------------

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE  

    //        #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature _EMISSION
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        #pragma shader_feature_local _PARALLAXMAP

    //        #pragma multi_compile_prepassfinal
    //        #pragma multi_compile_instancing
    //        // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
    //        //#pragma multi_compile _ LOD_FADE_CROSSFADE

    //        #pragma vertex vertDeferred
    //        #pragma fragment fragDeferred

    //        #include "./CGIncludes/UnityStandardCore.cginc"

    //        ENDCG
    //    }

    //    // ------------------------------------------------------------------
    //    // Extracts information for lightmapping, GI (emission, albedo, ...)
    //    // This pass it not used during regular rendering.
    //    Pass
    //    {
    //        Name "META"
    //        Tags { "LightMode"="Meta" }

    //        Cull Off

    //        CGPROGRAM
    //        #pragma vertex vert_meta
    //        #pragma fragment frag_meta

    //        #pragma shader_feature _EMISSION
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        #pragma shader_feature EDITOR_VISUALIZATION

    //        #include "./CGIncludes/UnityStandardMeta.cginc"
    //        ENDCG
    //    }
    //}
    
    //// 100
    //SubShader
    //{
    //    Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
    //    LOD 100

    //    // ------------------------------------------------------------------
    //    //  Base forward pass (directional light, emission, lightmaps, ...)
    //    Pass
    //    {
    //        Name "FORWARD"
    //        Tags { "LightMode" = "ForwardBase" }

    //        Blend [_SrcBlend] [_DstBlend]
    //        ZWrite [_ZWrite]
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature _EMISSION
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
    //        // SM2.0: NOT SUPPORTED shader_feature_local _DETAIL_MULX2
    //        // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP

    //        #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

    //        #pragma multi_compile_fwdbase
    //        #pragma multi_compile_fog

    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #pragma vertex vertBase
    //        #pragma fragment fragBase
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Additive forward pass (one light per pass)
    //    Pass
    //    {
    //        Name "FORWARD_DELTA"
    //        Tags { "LightMode" = "ForwardAdd" }
    //        Blend [_SrcBlend] One
    //        Fog { Color (0,0,0,0) } // in additive pass fog should be black
    //        ZWrite Off
    //        ZTest LEqual
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP
    //        #pragma skip_variants SHADOWS_SOFT

    //        #pragma multi_compile_fwdadd_fullshadows
    //        #pragma multi_compile_fog
    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #pragma vertex vertAdd
    //        #pragma fragment fragAdd
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Shadow rendering pass
    //    Pass {
    //        Name "ShadowCaster"
    //        Tags { "LightMode" = "ShadowCaster" }

    //        ZWrite On ZTest LEqual

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma skip_variants SHADOWS_SOFT
    //        #pragma multi_compile_shadowcaster

    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #pragma vertex vertShadowCaster
    //        #pragma fragment fragShadowCaster

    //        #include "./CGIncludes/UnityStandardShadow.cginc"

    //        ENDCG
    //    }

    //    // ------------------------------------------------------------------
    //    // Extracts information for lightmapping, GI (emission, albedo, ...)
    //    // This pass it not used during regular rendering.
    //    Pass
    //    {
    //        Name "META"
    //        Tags { "LightMode"="Meta" }

    //        Cull Off

    //        CGPROGRAM
    //        #pragma vertex vert_meta
    //        #pragma fragment frag_meta

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma shader_feature _EMISSION
    //        #pragma shader_feature_local _METALLICGLOSSMAP
    //        #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        #pragma shader_feature EDITOR_VISUALIZATION
    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #include "./CGIncludes/UnityStandardMeta.cginc"
    //        ENDCG
    //    }
    //}
    
    //// 50
    //SubShader
    //{
    //    Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
    //    LOD 50

    //    // ------------------------------------------------------------------
    //    //  Base forward pass (directional light, emission, lightmaps, ...)
    //    Pass
    //    {
    //        Name "FORWARD"
    //        Tags { "LightMode" = "ForwardBase" }

    //        Blend [_SrcBlend] [_DstBlend]
    //        ZWrite [_ZWrite]
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        // #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        // #pragma shader_feature _EMISSION
    //        // #pragma shader_feature_local _METALLICGLOSSMAP
    //        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        // #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
    //        // SM2.0: NOT SUPPORTED shader_feature_local _DETAIL_MULX2
    //        // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP

    //        #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

    //        #pragma multi_compile_fwdbase
    //        #pragma multi_compile_fog
    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #pragma vertex vertBase
    //        #pragma fragment fragBase
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Additive forward pass (one light per pass)
    //    Pass
    //    {
    //        Name "FORWARD_DELTA"
    //        Tags { "LightMode" = "ForwardAdd" }
    //        Blend [_SrcBlend] One
    //        Fog { Color (0,0,0,0) } // in additive pass fog should be black
    //        ZWrite Off
    //        ZTest LEqual
    //        Cull[_Cull]

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        // #pragma multi_compile _ _NORMALMAP
    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        // #pragma shader_feature_local _METALLICGLOSSMAP
    //        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
    //        // #pragma shader_feature_local _DETAIL_MULX2
    //        // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP
    //        #pragma skip_variants SHADOWS_SOFT

    //        #pragma multi_compile_fwdadd_fullshadows
    //        #pragma multi_compile_fog
    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #pragma vertex vertAdd
    //        #pragma fragment fragAdd
    //        #include "./CGIncludes/UnityStandardCoreForward.cginc"

    //        ENDCG
    //    }
    //    // ------------------------------------------------------------------
    //    //  Shadow rendering pass
    //    Pass {
    //        Name "ShadowCaster"
    //        Tags { "LightMode" = "ShadowCaster" }

    //        ZWrite On ZTest LEqual

    //        CGPROGRAM
    //        #pragma target 2.0

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    //        // #pragma shader_feature_local _METALLICGLOSSMAP
    //        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma skip_variants SHADOWS_SOFT
    //        #pragma multi_compile_shadowcaster
    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #pragma vertex vertShadowCaster
    //        #pragma fragment fragShadowCaster

    //        #include "./CGIncludes/UnityStandardShadow.cginc"

    //        ENDCG
    //    }

    //    // ------------------------------------------------------------------
    //    // Extracts information for lightmapping, GI (emission, albedo, ...)
    //    // This pass it not used during regular rendering.
    //    Pass
    //    {
    //        Name "META"
    //        Tags { "LightMode"="Meta" }

    //        Cull Off

    //        CGPROGRAM
    //        #pragma vertex vert_meta
    //        #pragma fragment frag_meta

    //        #pragma shader_feature_local PAV_VERTEX_FROM_BUFFER
    //        #pragma shader_feature_local PAV_MERGED_TEXTURE

    //        // #pragma shader_feature _EMISSION
    //        // #pragma shader_feature_local _METALLICGLOSSMAP
    //        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    //        #pragma shader_feature_local _DETAIL_MULX2
    //        #pragma shader_feature EDITOR_VISUALIZATION

    //        #define UNITY_NO_FULL_STANDARD_SHADER
    //        #include "./CGIncludes/UnityStandardMeta.cginc"
    //        ENDCG
    //    }
    //}

    // FallBack "PAV/BuiltIn/VertexLit"
      CustomEditor "Pico.Avatar.URPShaderInspector" 
}
