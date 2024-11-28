Shader "PicoAvatar/Hair"
{
    Properties
    {
        
        [ToggleOff(_RECEIVE_SHADOWS_OFF)]
        _ReceiveShadows             ("Receive Shadows", Float) = 1.0
        [Toggle(_ACES)]
        _Tonemapping                ("ACES Tone mapping", Float) = 1.0
        _BaseMap                    ("Albedo", 2D) = "white" {}
        _BaseColor                  ("Color", Color) = (0.05,0.05,0.05,1)
        
        [Toggle(_COLOR_SHIFT)]
        _ColorShift                 ("Color Shift", Float) = 0
        [Toggle]_UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        _ColorRegionMap("ColorRegionMap", 2D) = "black" {}
        _ColorRegion1("ColorRegion1", Vector) = (0,0,0,0) 
        _ColorRegion2("ColorRegion2", Vector) = (0,0,0,0) 
        _ColorRegion3("ColorRegion3", Vector) = (0,0,0,0) 
        _ColorRegion4("ColorRegion4", Vector) = (0,0,0,0) 
        
        _ShadowColor                ("Shadow Color", Color) = (0,0,0,1)
        _ShadowSmoothing            ("Shadow Smoothing",Range(0.001,2)) = 0.001
        _ShadowRange                ("Shadow Range",Range(-0.75,0.75)) = 0
        _EnvIntensity               ("Environment Intensity",Range(0,1.0)) = 0
        _ReflectionRoughness        ("Reflection Roughness",Range(0, 1.0)) = 1
        _ReflectionIntensity        ("Reflection Intensity",Range(0,3.0)) = 1
        
        [Space(5)]
        [Toggle(_NORMALMAP)]
        _ApplyNormal                ("Enable Normal Map", Float) = 0.0
        [NoScaleOffset]
        _BumpMap                    ("Normal Map", 2D) = "bump" {}
        _BumpScale                  ("Normal Scale", Float) = 1.0
        _OcclusionStrength          ("OcclusionStrength",Range(0,1)) = 1
        
        //Hair Settings
        [Header(Hair Lighting)]
        [Space(5)]
        [KeywordEnum(Bitangent,Tangent,Flowmap)]
        _StrandDir                  ("Strand Direction", Float) = 0
        [NoScaleOffset]_FlowMap     ("Flow Map", 2D) = "grey"{}
        
        [Space(5)]
        _SpecularShift              ("Primary Specular Shift", Range(-1.0, 1.0)) = 0.1
        _ShiftStrength              ("Primary Shift Strength",Range(0,1.0)) = 1
        [HDR] _SpecularTint         ("Primary Specular Tint", Color) = (0.02, 0.02, 0.02, 1)
        _SpecularExponent           ("Primary Smoothness", Float) = 60
        //_SpecularParam              ("Primary Param", Vector) = (1,1,1,1)
        
        [Space(5)]
        _SecondarySpecularShift     ("Secondary Specular Shift", Range(-1.0, 1.0)) = 0.1
        _SecondaryShiftStrength     ("Secondary Shift Strength",Range(0,1.0)) = 1
        [HDR] _SecondarySpecularTint("Secondary Specular Tint", Color) = (0.02, 0.02, 0.02, 1)
        _SecondarySpecularExponent  ("Secondary Smoothness", Float) = 60
        
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        [HideInInspector] _Cutoff("__alpha test", Float) = 1.0
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"  "IgnoreProjector" = "True"}
        LOD 0

        
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Cull Back
            HLSLPROGRAM
            
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma multi_compile _ _COLOR_SHIFT

             #pragma shader_feature _ _BAKETEXTURE _BAKETEXTURE_SMOOTHNESS

            // -------------------------------------
            // Universal Pipeline keywords
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT
            //#pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            // #pragma multi_compile _ LIGHTMAP_ON
            // #pragma multi_compile_fog
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            
            //#define _SHADOWS_SOFT
            //#define _MAIN_LIGHT_SHADOWS
            
            //Hair Keywords
            #pragma multi_compile __ _STRANDDIR_BITANGENT _STRANDDIR_TANGENT
            //#pragma shader_feature_local _SECONDARYLOBE
            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #ifndef _COLOR_SHIFT
            #define PAV_COLOR_REGION_BAKED
            #endif

            #define AVATAR_HAIR
            #include "Config.hlsl"
            #include "HairInput.hlsl"
            #include "HairPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "HairInput.hlsl"
            #include "Dependency/URP/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
