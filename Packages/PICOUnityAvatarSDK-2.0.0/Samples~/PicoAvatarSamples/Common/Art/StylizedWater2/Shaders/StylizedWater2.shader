//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

Shader "Universal Render Pipeline/FX/Stylized Water 2"
{
	Properties
	{
		//[Header(Rendering)]
		[MaterialEnum(Off,0,On,1)]_ZWrite("Depth writing", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Render faces", Float) = 2
		[MaterialEnum(Simple,0,Advanced,1)] _ShadingMode("Shading mode", Float) = 1

		//[Header(Feature switches)]
		_DisableDepthTexture("Disable depth texture", Float) = 0
		_AnimationParams("XY=Direction, Z=Speed", Vector) = (1,1,1,0)
		//TODO: Split up component into separate properties. This way multi-material selection works
		_SlopeParams("Slope (X=Stretch) (Y=Speed)", Vector) = (0.5, 2, 0, 0)
		_SlopeThreshold("Slope threshold", Range(0 , 1)) = 0.25
		[MaterialEnum(Mesh UV,0,World XZ projected ,1)]_WorldSpaceUV("UV Source", Float) = 1

		//[Header(Color)]
		[HDR]_BaseColor("Deep", Color) = (0, 0.44, 0.62, 1)
		[HDR]_ShallowColor("Shallow", Color) = (0.1, 0.9, 0.89, 0.02)
		[HDR]_HorizonColor("Horizon", Color) = (0.84, 1, 1, 0.15)

		//TODO: Split up component into separate properties. This way multi-material selection works
		_VertexColorMask ("Vertex color mask", vector) = (0,0,0,0)
		
		//_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.9
		//_Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		
		_HorizonDistance("Horizon Distance", Range(0.01 , 32)) = 8
		_DepthVertical("View Depth", Range(0.01 , 16)) = 4
		_DepthHorizontal("Vertical Height Depth", Range(0.01 , 8)) = 1
		_DepthExp("Exponential Blend", Range(0 , 1)) = 1
		_WaveTint("Wave tint", Range( -0.1 , 0.1)) = 0
		_TranslucencyParams("Translucency", Vector) = (1,8,1,0)
		//X: Strength
		//Y: Exponent
		//Z: Curvature mask
		_TranslucencyReflectionMask("Translucency Reflection mask", Range(0, 1)) = 0
		_EdgeFade("Edge Fade", Float) = 0.1
		_ShadowStrength("Shadow Strength", Range(0 , 1)) = 1

		//[Header(Underwater)]
		[Toggle(_CAUSTICS)] _CausticsOn("Caustics ON", Float) = 1
		_CausticsBrightness("Brightness", Float) = 2
		_CausticsTiling("Tiling", Float) = 0.5
		_CausticsSpeed("Speed multiplier", Float) = 0.1
		_CausticsDistortion("Distortion", Range(0, 1)) = 0.15
		[NoScaleOffset][SingleLineTexture]_CausticsTex("Caustics Mask", 2D) = "black" {}
		
		_UnderwaterSurfaceSmoothness("Underwater Surface Smoothness", Range(0, 1)) = 0.8
		_UnderwaterRefractionOffset("Underwater Refraction Offset", Range(0, 1)) = 0.2
		
		[Toggle(_REFRACTION)] _RefractionOn("_REFRACTION", Float) = 1
		_RefractionStrength("_RefractionStrength", Range(0 , 3)) = 0.1

		//[Header(Intersection)]
		[MaterialEnum(Camera Depth,0,Vertex Color (R),1,Both combined,2)] _IntersectionSource("Intersection source", Float) = 0
		[MaterialEnum(None,0,Sharp,1,Smooth,2)] _IntersectionStyle("Intersection style", Float) = 1

		[NoScaleOffset][SingleLineTexture]_IntersectionNoise("Intersection noise", 2D) = "white" {}
		_IntersectionColor("Color", Color) = (1,1,1,1)
		_IntersectionLength("Distance", Range(0.01 , 5)) = 2
		_IntersectionClipping("Cutoff", Range(0.01, 1)) = 0.5
		_IntersectionFalloff("Falloff", Range(0.01 , 1)) = 0.5
		_IntersectionTiling("Noise Tiling", float) = 0.2
		_IntersectionSpeed("Speed multiplier", float) = 0.1
		_IntersectionRippleDist("Ripple distance", float) = 32
		_IntersectionRippleStrength("Ripple Strength", Range(0 , 1)) = 0.5

		//[Header(Foam)]
		[NoScaleOffset][SingleLineTexture]_FoamTex("Foam Mask", 2D) = "black" {}
		_FoamColor("Color", Color) = (1,1,1,1)
		_FoamSize("Cutoff", Range(0.01 , 0.999)) = 0.01
		_FoamSpeed("Speed multiplier", float) = 0.1
		_FoamWaveMask("Wave mask", Range(0 , 1)) = 0
		_FoamWaveMaskExp("Wave mask exponent", Range(1 , 8)) = 1
		_FoamTiling("Tiling", float) = 0.1

		//[Header(Normals)]
		[Toggle(_NORMALMAP)] _NormalMapOn("_NORMALMAP", Float) = 1
		[NoScaleOffset][Normal][SingleLineTexture]_BumpMap("Normals", 2D) = "bump" {}
		_NormalTiling("Tiling", Float) = 1
		_NormalStrength("Strength", Range(0 , 1)) = 0.5
		_NormalSpeed("Speed multiplier", Float) = 0.2
		//X: Start
		//Y: End
		//Z: Tiling multiplier
		_DistanceNormalParams("Distance normals", vector) = (100, 300, 0.25, 0)
		[NoScaleOffset][Normal][SingleLineTexture]_BumpMapLarge("Normals (Distance)", 2D) = "bump" {}

		_SparkleIntensity("Sparkle Intensity", Range(0 , 10)) = 00
		_SparkleSize("Sparkle Size", Range( 0 , 1)) = 0.280

		//[Header(Sun Reflection)]
		_SunReflectionSize("Sun Size", Range(0 , 1)) = 0.5
		_SunReflectionStrength("Sun Strength", Float) = 10
		_SunReflectionDistortion("Sun Distortion", Range( 0 , 2)) = 0.49
		_PointSpotLightReflectionExp("Point/spot light exponent", Range(0.01 , 128)) = 64

		//[Header(World Reflection)]
		_ReflectionStrength("Strength", Range( 0 , 1)) = 0
		_ReflectionDistortion("Distortion", Range( 0 , 2)) = 0.05
		_ReflectionBlur("Blur", Range( 0 , 1)) = 0	
		_ReflectionFresnel("Curvature mask", Range( 0.01 , 20)) = 5	
		_ReflectionLighting("Lighting influence", Range( 0 , 1)) = 0	
		_PlanarReflectionLeft("Planar Reflections", 2D) = "" {} //Instanced
		_PlanarReflectionsEnabled("Planar Enabled", float) = 0 //Instanced
		
		//[Header(Waves)]
		[Toggle(_WAVES)] _WavesOn("_WAVES", Float) = 0

		_WaveSpeed("Speed", Float) = 2
		_WaveHeight("Height", Range(0 , 10)) = 0.25
		_WaveNormalStr("Normal Strength", Range(0 , 6)) = 0.5
		_WaveDistance("Distance", Range(0 , 1)) = 0.8
		_WaveFadeDistance("Fade Distance", vector) = (150, 300, 0, 0)

		_WaveSteepness("Steepness", Range(0 , 5)) = 0.1
		_WaveCount("Count", Range(1 , 5)) = 1
		_WaveDirection("Direction", vector) = (1,1,1,1)

		/* start Tessellation */
		//_TessValue("Max subdivisions", Range(1, 32)) = 16
		//_TessMin("Start Distance", Float) = 0
		//_TessMax("End Distance", Float) = 15
 		/* end Tessellation */
		
		//[CurvedWorldBendSettings] _CurvedWorldBendSettings("0|1|1", Vector) = (0, 0, 0, 0)
	}

	SubShader
	{		
		Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
				
		Pass
		{	
			Name "ForwardLit"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite [_ZWrite]
			Cull [_Cull]
			/* start COZY */
//			Stencil { Ref 221 Comp Always Pass Replace }
			/* end COZY */
			ZTest LEqual
			
			HLSLPROGRAM

			#pragma multi_compile_instancing
			/* start UnityFog */
			#pragma multi_compile_fog
			/* end UnityFog */

			#pragma target 3.0
			
			// Material Keywords
			//Note: _fragment suffix fails to work on GLES. Keywords would always be stripped
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _DISTANCE_NORMALS
			#pragma shader_feature_local _WAVES
			#pragma shader_feature_local _FOAM
			#pragma shader_feature_local _UNLIT
			#pragma shader_feature_local _TRANSLUCENCY
			#pragma shader_feature_local _CAUSTICS
			#pragma shader_feature_local _REFRACTION
			#pragma shader_feature_local _ADVANCED_SHADING
			#pragma shader_feature_local _FLAT_SHADING
			#pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local _ENVIRONMENTREFLECTIONS_OFF
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local _DISABLE_DEPTH_TEX
			#pragma shader_feature_local _ _SHARP_INERSECTION _SMOOTH_INTERSECTION
			#pragma shader_feature_local _RIVER

			//Will be stripped, if extensions aren't installed
			#pragma multi_compile _ UNDERWATER_ENABLED
			#pragma multi_compile _ MODIFIERS_ENABLED
			//#pragma multi_compile _ WAVE_SIMULATION

			#if !_ADVANCED_SHADING
			#define _SIMPLE_SHADING
			#endif

			#if _RIVER
			#undef _WAVES
			#undef UNDERWATER_ENABLED
			#endif

			//Required to differentiate between skybox and scene geometry
			#if UNDERWATER_ENABLED
			#undef _DISABLE_DEPTH_TEX 
			#endif
			
			 //Caustics require depth texture
			#if _DISABLE_DEPTH_TEX
			#undef _CAUSTICS
			#endif
			
			//Requires some form of per-pixel offset
			#if !_NORMALMAP && !_WAVES
			#undef _REFRACTION
			#endif
			
			//Unity global keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS //URP 11+
			//Tiny use-case, disabled to reduce variants (each adds about 200-500)
			//#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE			

			//Stripped during building on older versions
			//URP 12+ only
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#include "Libraries/URP.hlsl"

			/* start AtmosphericHeightFog */
//			#pragma multi_compile AHF_NOISEMODE_OFF AHF_NOISEMODE_PROCEDURAL3D
			/* end AtmosphericHeightFog */

			//Defines
			#define SHADERPASS_FORWARD
			#if !defined(_DISABLE_DEPTH_TEX) || defined(_REFRACTION) || defined(_CAUSTICS) || UNDERWATER_ENABLED
			//Required to read depth/opaque texture or other screen-space buffers
			#define SCREEN_POS
			#endif
			
			/* start Tessellation */
//			#define TESSELLATION_ON
//			#pragma require tessellation tessHW
//			#pragma hull Hull
//			#pragma domain Domain
			/* end Tessellation */
			
			#pragma vertex Vertex
			#pragma fragment ForwardPassFragment

			#include "Libraries/Input.hlsl"

			//Uncommenting and rewriting is handled by the Curved World 2020 asset
			//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
			//#define CURVEDWORLD_BEND_ID_1
			//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
			//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
			//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"
			
			#include "Libraries/Common.hlsl"
			#include "Libraries/Fog.hlsl"
			#include "Libraries/Waves.hlsl"
			#include "Libraries/Lighting.hlsl"

			#ifdef UNDERWATER_ENABLED
			#include "Underwater/UnderwaterFog.hlsl"
			#include "Underwater/UnderwaterShading.hlsl"
			#endif

			#ifdef WAVE_SIMULATION
			#include "Libraries/Simulation/Simulation.hlsl"
			#endif

			#include "Libraries/Features.hlsl"
			#include "Libraries/Caustics.hlsl"

			#if MODIFIERS_ENABLED
			#include "SurfaceModifiers/SurfaceModifiers.hlsl"
			#endif
			
			#include "Libraries/Vertex.hlsl"

			/* start Tessellation */
//			#include "Libraries/Tesselation.hlsl"
			/* end Tessellation */

			Varyings Vertex(Attributes v)
			{
				return LitPassVertex(v);
			}

			#include "Libraries/ForwardPass.hlsl"
			
			ENDHLSL
		}
		
		//Currently unused, except for prototypes (such as depth texture injection)
		Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            
            ZWrite On
			//ColorMask RG
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma multi_compile_instancing

            #pragma shader_feature_local _WAVES

            /* start Tessellation */
//			#define TESSELLATION_ON
//			#pragma require tessellation tessHW
//			#pragma hull Hull
//			#pragma domain Domain
			/* end Tessellation */
            
            #pragma vertex Vertex
            #pragma fragment DepthOnlyFragment

            #define SHADERPASS_DEPTHONLY

            #include "Libraries/URP.hlsl"
            #include "Libraries/Input.hlsl"

			//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
			//#define CURVEDWORLD_BEND_ID_1
			//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
			//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
			//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"

            #include "Libraries/Common.hlsl"
            #include "Libraries/Fog.hlsl"
            #include "Libraries/Waves.hlsl"

            #include "Libraries/Vertex.hlsl"

            /* start Tessellation */
//          #include "Libraries/Tesselation.hlsl"
            /* end Tessellation */

            Varyings Vertex(Attributes v)
            {
                return LitPassVertex(v);
            }

            half4 DepthOnlyFragment(Varyings input, FRONT_FACE_TYPE facing : FRONT_FACE_SEMANTIC) : SV_TARGET
            {
				UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            	float depth = input.positionCS.z;

                return float4(depth, facing, 0, 0);
            }

            ENDHLSL

        }
	}

	CustomEditor "StylizedWater2.MaterialUI"
	Fallback "Hidden/InternalErrorShader"	
}
