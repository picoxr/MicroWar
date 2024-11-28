Shader "PicoAvatar/Eye"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        [ToggleOff(_RECEIVE_SHADOWS_OFF)]
        _ReceiveShadows             ("Receive Shadows", Float) = 1.0
        [Toggle(_ACES)]
        _Tonemapping            ("ACES Tone mapping", Float) = 1.0
        _BaseMap                ("Albedo",2D) = "white"{}
        
        [Toggle(_COLOR_SHIFT)]
        _ColorShift                 ("Color Shift", Float) = 0
        [Toggle]_UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        _ColorRegionMap("ColorRegionMap", 2D) = "black" {}
        _ColorRegion1("ColorRegion1", Vector) = (0,0,0,0) 
        _ColorRegion2("ColorRegion2", Vector) = (0,0,0,0) 
        _ColorRegion3("ColorRegion3", Vector) = (0,0,0,0) 
        _ColorRegion4("ColorRegion4", Vector) = (0,0,0,0)
        
        [Toggle(_NORMALMAP)]
        _EnableNormalMap        ("Enale Normal Map", Float) = 0
        
        _BumpMap                ("Normal Map",2D) = "bump"{}
        _BumpScale              ("Normal Scale",Float) = 1
        
        _ScleraSmoothness       ("Sclera Smoothness",Range(0,1)) = 1
        _ScleraSpecSmoothness   ("Sclera Highlight Smoothness", Range(0,1)) = 0
        [HDR]
        _ScleraSpecColor        ("Sclera Highlight Color",Color) = (1,1,1,1)
        
        _IrisSmoothness         ("Iris Smoothness",Range(0,1)) = 1
        _IrisSpecSmoothness     ("Iris Highlight Smoothness", Range(0,1)) = 1
        [HDR]
        _IrisSpecColor          ("Iris Highlight Color", Color) = (1,1,1,1)
        _IrisColor              ("Iris Color", Color) = (1,1,1,1)
        
        [KeywordEnum(Simple,POM,PBR)]
        _RefMode            ("Refraction Mode",Float) = 0
        _ParallaxScale      ("Parallax Scale", Range(0.0,0.4)) = 0.05
        _MinParallaxSamples ("Min Parallax Samples",Range(0.0,49)) = 25
        _MaxParallaxSamples ("Max Parallax Samples",Range(6,100)) = 75
        
        _HighlightOffset    ("Highlight Offset",Vector) = (0, 0, 0, 0)
        
        _CausticIntensity   ("Caustic Intensity",Range(1,3)) = 1
        _CausticRange       ("Caustic Range",Range(0.5,1.0)) = 0.8
        // High light settings
        // [Toggle(_MATCAP)]
        // _EnableMatCap       ("Enable MatCap", Float) = 1
        // _MatCap             ("MatCap Texture", 2D) = "black"{}
        // _MatCapStrength     ("MatCap Strength", Range(0,1)) = 1
        
        [Toggle(_SHARP_HIGHTLIGHT)]
        _EnableSharpHighlight   ("Enable Sharp Highlight", Float) = 0
        _SharpParam             ("Sharp Param", Vector) = (0.1,0.1,0.1,0.1)
        
        // SRP Batcher compatible
        [HideInInspector] _Cutoff("",Range(0,1)) = 0.5
        
        //_PupilAperture("Pupil Size",Range(0,5)) = 0.5
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel"="4.5"}
        LOD 100
        
        Cull Back

        Pass
        {
            Name "Eye"

            HLSLPROGRAM
            #pragma target 4.5

            #pragma shader_feature_local __ _REFMODE_POM _REFMODE_PBR
            #pragma shader_feature_local _SHARP_HIGHTLIGHT
            //#pragma shader_feature_local _MATCAP
            #pragma multi_compile _ FLIP_Y
            #pragma multi_compile _ _COLOR_SHIFT
            
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #ifndef _COLOR_SHIFT
            #define PAV_COLOR_REGION_BAKED
            #endif
            
            #define AVATAR_EYE
            #include "Config.hlsl"
            #include "EyeInput.hlsl"
            #include "EyePass.hlsl"
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
            #pragma multi_compile_fragment _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #include "EyeInput.hlsl"
            #include "DepthOnlyPass.hlsl"
            
            ENDHLSL
        }

    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "Pico.Avatar.URPShaderInspector"
    
}
