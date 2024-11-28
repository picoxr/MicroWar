Shader "PicoAvatar/AvatarLit"
{
    Properties
    {
        [Header(Settings)]
        
        [Space(10)]
        [KeywordEnum(Simple, Full)]
        _PBR                ("PBR Mode",Float) = 0
        [Toggle(_ACES)]
        _Tonemapping        ("ACES Tone mapping", Float) = 1.0
        [Toggle(_RECEIVE_SHADOWS)] 
        _ReceiveShadows     ("Receive Shadows", Float) = 1.0
        
        [KeywordEnum(None,DirectOnly,IndirectOnly,Metallic,Roughness,Normal)] 
        _Debug              ("Debug",Float) = 0
        
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        
        [Toggle(_COLOR_SHIFT)]
        _ColorShift("Color Shift", Float) = 0
        [Toggle]_UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        _ColorRegionMap("ColorRegionMap", 2D) = "black" {}
        _ColorRegion1("ColorRegion1", Vector) = (0,0,0,0) 
        _ColorRegion2("ColorRegion2", Vector) = (0,0,0,0) 
        _ColorRegion3("ColorRegion3", Vector) = (0,0,0,0) 
        _ColorRegion4("ColorRegion4", Vector) = (0,0,0,0) 
        _BodyMaskMap("Body Mask Map (not used in shader)", 2D) = "black" {}
        
        [Toggle(_NORMALMAP)]
        _EnableNormalMap    ("Enable Normal Map",Float) = 0 
        
        [NoScaleOffset]
        _BumpMap            ("Normal(RG),Metallic(B),Smoothness(A)", 2D) = "bump" {}
        _BumpScale          ("Normal Scale", Float) = 1.0
        
        _Smoothness         ("Smoothness", Range(0.0, 1.0)) = 0.5
        _SmoothnessMin      ("Smoothness Remap Min", Range(0.0, 0.5)) = 0
        _SmoothnessMax      ("Smoothness Remap Max", Range(0.5001,1.0)) = 1
        
        _Metallic           ("Metallic", Range(0.0, 1.0)) = 0.0
        
        _OcclusionStrength  ("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        
        [Toggle(_EMISSION)]
        _Emission           ("Enable Emission", Float) = 0.0
        _EmissionMap        ("Emission Texture",2D) = "black"{}
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
        
         [Toggle(_MATCAP)]
         _MatCap             ("Use MatCap Reflection", Float) = 0.0
        
         [NoScaleOffset]
         _MatCapTex          ("MatCap Tex",2D) = "black"{}
         _MatCapStrength     ("MatCap Strength",Range(0,3.0)) = 1.0
        
        [Toggle(_MATCAP_FIX_EDGE_FLAW)]
        _FixFlaw            ("Fix Edge Flaw",Float) = 0.0
        
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
        
        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _Cutoff("__alpha test", Float) = 1.0
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

        // [HideInInspector][NoScaleOffset]_BaseMapArray("BaseMap Array", 2DArray) = "white" {}
        // [HideInInspector][NoScaleOffset]_ColorRegionMapArray("ColorRegionMap Array", 2DArray) = "back" {}
        // [HideInInspector][NoScaleOffset]_BumpMapArray("BumpMap Array", 2DArray) = "bump" {}
        // [HideInInspector][NoScaleOffset]_EmissionMapArray("EmissionMap Array", 2DArray) = "black" {}
        // [HideInInspector][NoScaleOffset]_MatCapMapArray("MatCapMap Array", 2DArray) = "black" {}
    }

    SubShader
    {
        //HLSLINCLUDE
        //#pragma enable_d3d11_debug_symbols
        //ENDHLSL
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel"="3.5"}
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

            HLSLPROGRAM
            #pragma target 3.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local __ _PBR _FULL
            #pragma shader_feature_local _RECEIVE_SHADOWS
            #pragma shader_feature_local _EMISSION
            //#pragma shader_feature_local _MATCAP
            #pragma shader_feature_local _MATCAP_FIX_EDGE_FLAW
            #pragma shader_feature_local _NORMALMAP
            
            #pragma multi_compile _ _COLOR_SHIFT

            //Debug keywords
            #pragma shader_feature_local _DEBUG_NONE  _DEBUG_DIRECTONLY _DEBUG_INDIRECTONLY _DEBUG_METALLIC _DEBUG_ROUGHNESS _DEBUG_NORMAL
            //注意：如果没有主光源，该feature会被关闭
            //# define  _MAIN_LIGHT_SHADOWS
            
            //Define Keywords
            //#define _SHADOWS_SOFT
            //#define _MAIN_LIGHT_SHADOWS
            // -------------------------------------
            // Compile -- keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _ENABLE_STATIC_MESH_BATCHING

            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            //#pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile_fog

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #ifndef _COLOR_SHIFT
            #define PAV_COLOR_REGION_BAKED
            #endif

            #define AVATAR_LIT
            #include "Config.hlsl"
            #include "AvatarLitInput.hlsl"
            #include "AvatarLitForwardPass.hlsl"
            
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
            #pragma target 3.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "AvatarLitInput.hlsl"
            #include "Dependency/URP/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }


        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "AvatarLitInput.hlsl"
            #include "Dependency/URP/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "AvatarLitInput.hlsl"
            #include "Dependency/URP/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }

    }



    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    //CustomEditor "PicoPBRShaderEditor"
}
