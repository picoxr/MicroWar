Shader "PicoAvatar/Skin"
{
    Properties
    {
        [Main(_SurfaceOptions, _, on, off)]             _SurfaceOptions ("Surface Options", Float) = 0
        
        [Preset(_SurfaceOptions, SurfaceTypePreset)] _TypeMode ("Surface Type", float) = 0
        [SubEnum(_SurfaceOptions, UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 1.0
        [SubEnum(_SurfaceOptions, UnityEngine.Rendering.BlendMode)] _DstBlend("DstBlend", Float) = 0.0
        [SubEnum(_SurfaceOptions, UnityEngine.Rendering.BlendMode)] _SrcAlphaBlend("SrcAlphaBlend", Float) = 1.0
        [SubEnum(_SurfaceOptions, UnityEngine.Rendering.BlendMode)] _DstAlphaBlend("DstAlphaBlend", Float) = 0.0
        
        [Toggle(_RECEIVE_SHADOWS)]                      _ReceiveShadows ("Receive Shadows", Float) = 1.0
        [Toggle(_ACES)]                                 _Tonemapping    ("ACES Tone mapping", Float) = 1.0
        
        //Issue: can not add more keywords
        [KWEnum(_SurfaceOptions, None, _DEBUG_NONE, DirectOnly, _DEBUG_DIRECTONLY, IndirectOnly, _DEBUG_INDIRECTONLY, Metallic, _DEBUG_METALLIC,Roughness, _DEBUG_ROUGHNESS)] _Debug("Debug",Float) = 0
        
        [Main(_SurfaceInputs, _, on, off)]      _SurfaceInputs  ("Surface Inputs", Float) = 0
        [Title(_SurfaceInputs, Base)]
        [Sub(_SurfaceInputs)][HDR][MainColor]   _BaseColor      ("Base Color", Color) = (1,1,1,1)
        [Sub(_SurfaceInputs)][MainTexture]      _BaseMap        ("Base Color(RGB),AO(A)", 2D) = "white" {}
        
        [Sub(_SurfaceInputs)][Toggle(_COLOR_SHIFT)]
        _ColorShift                         ("Color Shift", Float) = 0
        [Sub(_SurfaceInputs)][Toggle]_UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        [Sub(_SurfaceInputs)]_ColorRegionMap("ColorRegionMap", 2D) = "black" {}
        [Sub(_SurfaceInputs)]_ColorRegion1("ColorRegion1", Vector) = (0,0,0,0) 
        [Sub(_SurfaceInputs)]_ColorRegion2("ColorRegion2", Vector) = (0,0,0,0) 
        [Sub(_SurfaceInputs)]_ColorRegion3("ColorRegion3", Vector) = (0,0,0,0) 
        [Sub(_SurfaceInputs)]_ColorRegion4("ColorRegion4", Vector) = (0,0,0,0) 
        
        [Title(_SurfaceInputs, NMR)]
        [ExtendSub(_SurfaceInputs, _NORMALMAP)] _BumpMap("Normal(RG),Metallic(B),Smoothness(A)", 2D) = "bump" {}
        [Sub(_SurfaceInputs)]       _BumpScale          ("Normal Scale", Float) = 1.0
        [Sub(_SurfaceInputs)]       _Metallic           ("Metallic Scale", Range(0.0, 1.0)) = 0.0
	    [Sub(_SurfaceInputs)]       _Smoothness1        ("Smoothness Scale", Range(0.0, 1.0)) = 0.5
        [Sub(_SurfaceInputs)]       _OcclusionStrength  ("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        [Sub(_SurfaceInputs)][HDR]  _OcclusionTint      ("Occlusion Tint", Color) = (0,0,0,0)
        [Sub(_SurfaceInputs)][HDR]  _EmissionColor      ("Emission Color", Color) = (0,0,0)
        
        [Main(_DualSpecularOptions, _DUAL_SPECULAR)] _DualSpecularOptions ("Dual Specular Options", Float) = 0
        [Sub(_DualSpecularOptions)] _Smoothness2("Smoothness Seccondary", Range(0.0, 1.0)) = 0.5
        [Sub(_DualSpecularOptions)] _SpecularLobe1("Specular Lobe 1", Float) = 0.85
        [Sub(_DualSpecularOptions)] _SpecularLobe2("Specular Lobe 2", Float) = 0.15
        [Sub(_DualSpecularOptions)] _LobeMix("Dual Lobe Mix",Range(0, 1.0)) = 0.5
        
        [Main(_DetailInputs, _DETAIL)]_DetailInputs("Detail Inputs", Float) = 0
        [Sub(_DetailInputs)]    _DetailMap      ("Detail Normal Map", 2D) = "bump" {}
        [Sub(_DetailInputs)]    _DetailBumpScale("Detail Normal Scale", Float) = 1
        
        /*** Subsurface Scattering
        [Main(_SSS, _, on, off)] _SSS ("Subsurface Scattering Options", Float) = 0
        [SubToggle(_SSS,_SSS_HIGH_QUALITY)]_HighQuality("High Quality", Float) = 0.0
        [Sub(_SSS)] _SSMask             ("SSS Mask",2D) = "white"{}
        [Sub(_SSS)] _SSRemap            ("SS Remap",Vector) = (0, 1, 0, 3)
        [Sub(_SSS)] _SSLUT              ("SSLUT",2D) = "black"{}
        [Sub(_SSS)] _SSCurveScale       ("SS Curve Scale", Range(0,3)) = 1
        [Sub(_SSS)] _SSNormalBlurScale  ("SS Normal Blur", Range(0,1)) = 1
        [Sub(_SSS)] _SSSColor           ("Scattering Color",Color) = (1, 0, 0)
        [Sub(_SSS)] _SSTransColor       ("Transmission Color", Color) = (1, 0, 0)
        ***/
        
        [Main(_Ramp, _, on, off)] _Ramp ("Ramp Options", Float) = 0
        [Sub(_Ramp)] _ShadowSmoothing            ("Shadow Smoothing",Range(0.001,2)) = 0.1
        [Sub(_Ramp)] _ShadowRange                ("Shadow Range",Range(-0.5,0.5)) = 0
        [Sub(_Ramp)] _LightShadow                ("Shadow Light",Range(0,1)) = 0
        [Sub(_Ramp)] _ShadowColor                ("Shadow Color",Color) = (0,0,0,1.0)
        
        // Rim Settings
        [Main(_RimLight, _RIM_LIGHT)]_RimLight  ("Rim Light", Float) = 0
        [Sub(_RimLight)][HDR]_RimColor          ("Rim Color", Color) = (1, 1, 1, 1)
        [KWEnum(_RimLight, None, _RIMMASK_NONE,Light, _RIMMASK_LIGHT, View, _RIMMASK_VIEW)] _RimMask("Rim Mask", Float) = 0
        [Sub(_RimLight)]_RimPower               ("Rim Power", Float) = 2
        [Sub(_RimLight)]_RimMin                 ("Rim Scale Min", Range(0,2)) = 0.5
        [Sub(_RimLight)]_RimMax                 ("Rim Scale Max", Range(0,2)) = 1
        [Sub(_RimLight)]_RimRotation            ("Rim Roatation", Range(0,1)) = 1
        [Sub(_RimLight)]_RimLengthThreshold     ("Rim Length Threshold", Range(0,1)) = 0.1
        [Sub(_RimLight)]_RimLengthSmoothing     ("Rim Length Smoothing", Range(0,1)) = 0.5
        
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        _SkinOffset("Skin Offset", FLoat) = 0.0
        
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__mode", Float) = 0.0
        [HideInInspector] _Cutoff("__alpha test", Float) = 1.0
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
        
        // -------------------------------------
        // Render State Commands
        Blend [_SrcBlend][_DstBlend], [_SrcAlphaBlend][_DstAlphaBlend]

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
            
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            //#pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            //Additional Material Keywords
            #pragma shader_feature_local _RECEIVE_SHADOWS
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _DETAIL
            //#pragma shader_feature_local _SSS_HIGH_QUALITY
            
            #pragma shader_feature_local _RIM_LIGHT
            #pragma shader_feature_local _ _RIMMASK_LIGHT _RIMMASK_VIEW
            
            #pragma shader_feature_local _DUAL_SPECULAR
            //#pragma multi_compile _ _COLOR_SHIFT

            //Debug keywords
            #pragma shader_feature_local _DEBUG_NONE  _DEBUG_DIRECTONLY _DEBUG_INDIRECTONLY _DEBUG_METALLIC _DEBUG_ROUGHNESS _DEBUG_NORMAL
            
            // -------------------------------------
            // Compile -- keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            //#ifndef _COLOR_SHIFT
            //#define PAV_COLOR_REGION_BAKED
            //#endif

            #define AVATAR_SKIN
            #include "Config.hlsl"
            #include "SkinInput.hlsl"
            #include "SkinForwardPass.hlsl"
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
            #pragma target 3.5
            

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "SkinInput.hlsl"
            #include "Dependency/URP/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }


        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "SkinInput.hlsl"
            #include "Dependency/URP/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

    }



    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"
}
