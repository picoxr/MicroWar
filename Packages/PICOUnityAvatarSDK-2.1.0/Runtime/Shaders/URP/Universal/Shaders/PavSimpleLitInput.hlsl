#ifndef PAV_SIMPLE_LIT_INPUT_INCLUDED
#define PAV_SIMPLE_LIT_INPUT_INCLUDED

#include "../../PavConfig.hlsl"
#include "../ShaderLibrary/Core.hlsl"


/**
_PARALLAXMAP  TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
_OCCLUSIONMAP TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
_DETAIL       TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
_DETAIL       TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
_DETAIL       TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);
!_SPECULAR_SETUP && _METALLICSPECGLOSSMAP     TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
 _SPECULAR_SETUP                              TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
(_CLEARCOAT || _CLEARCOATMAP)                 TEXTURE2D(_ClearCoatMap);       SAMPLER(sampler_ClearCoatMap);
*/

#ifndef UNITY_INSTANCING_ENABLED
CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
    half4 _SpecColor;
    half  _UsingAlbedoHue;
    half4 _ColorRegion1;
    half4 _ColorRegion2;
    half4 _ColorRegion3;
    half4 _ColorRegion4;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;
    half _ShaderType;
    float4 _BaseMap_ST;
    half _AdditiveGI;
    half _SpecularAAScreenSpaceVariance;
    half _SpecularAAThreshold;
    float  _BaseColorAmplify;
    float4 _CustomVec_0;
    float4 _CustomVec_1;
    float4 _CustomVec_2;
    float4 _CustomVec_3;
    float4 _CustomVec_4;
    float4 _CustomVec_5;
    float4 _CustomVec_6;
    float4 _CustomVec_7;
    float4 _CustomVec_8;
    float  _MipBias;
CBUFFER_END
#endif

#ifdef UNITY_DOTS_INSTANCING_ENABLED
    UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
        UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
        UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
        UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
        UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
        UNITY_DOTS_INSTANCED_PROP(float , _Surface)
        UNITY_DOTS_INSTANCED_PROP(float , _ShaderType)
        UNITY_DOTS_INSTANCED_PROP(float , _OcclusionStrength)
        UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_ST)
        UNITY_DOTS_INSTANCED_PROP(float , _AdditiveGI)
        UNITY_DOTS_INSTANCED_PROP(float, _SpecularAAScreenSpaceVariance)
        UNITY_DOTS_INSTANCED_PROP(float, _SpecularAAThreshold)
        UNITY_DOTS_INSTANCED_PROP(float, _BaseColorAmplify)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_0)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_1)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_2)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_3)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_4)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_5)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_6)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_7)
        UNITY_DOTS_INSTANCED_PROP(float4, _CustomVec_8)
        UNITY_DOTS_INSTANCED_PROP(float, _MipBias)
    UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

    #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__BaseColor)
    #define _SpecColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__SpecColor)
    #define _EmissionColor      UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__EmissionColor)
    #define _Cutoff             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Cutoff)
    #define _Surface            UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Surface)
    #define _ShaderType         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ShaderType)
    #define _OcclusionStrength  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__OcclusionStrength)
    #define _BaseMap_ST         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__BaseMap_ST)
    #define _AdditiveGI         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__AdditiveGI)
    #define _SpecularAAScreenSpaceVariance         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__SpecularAAScreenSpaceVariance)
    #define _SpecularAAThreshold         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__SpecularAAThreshold)
    #define _BaseColorAmplify UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata__BaseColorAmplify)
    #define _CustomVec_0 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_0)
    #define _CustomVec_1 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_1)
    #define _CustomVec_2 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_2)
    #define _CustomVec_3 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_3)
    #define _CustomVec_4 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_4)
    #define _CustomVec_5 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_5)
    #define _CustomVec_6 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_6)
    #define _CustomVec_7 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_7)
    #define _CustomVec_8 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata__CustomVec_8)
    #define _MipBias UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata__MipBias)
#elif defined(UNITY_INSTANCING_ENABLED)
    // for simple avatar, remove not used parts
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
        //UNITY_DEFINE_INSTANCED_PROP(float4, _SpecColor)
        //UNITY_DEFINE_INSTANCED_PROP(float4, _EmissionColor)
        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
        //UNITY_DEFINE_INSTANCED_PROP(float4, _ColorRegion1)
        //UNITY_DEFINE_INSTANCED_PROP(float4, _ColorRegion2)
        //UNITY_DEFINE_INSTANCED_PROP(float4, _ColorRegion3)
        //UNITY_DEFINE_INSTANCED_PROP(float4, _ColorRegion4)
        //UNITY_DEFINE_INSTANCED_PROP(float, _UsingAlbedoHue)
        //UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
        //UNITY_DEFINE_INSTANCED_PROP(float, _Surface)
        //UNITY_DEFINE_INSTANCED_PROP(float, _ShaderType)
        UNITY_DEFINE_INSTANCED_PROP(float, _AdditiveGI)

        UNITY_DEFINE_INSTANCED_PROP(float, _SpecularAAScreenSpaceVariance)
        UNITY_DEFINE_INSTANCED_PROP(float, _SpecularAAThreshold)

    UNITY_DEFINE_INSTANCED_PROP(float  ,_BaseColorAmplify)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_0)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_1)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_2)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_3)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_4)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_5)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_6)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_7)
    UNITY_DEFINE_INSTANCED_PROP(float4 ,_CustomVec_8)
    UNITY_DEFINE_INSTANCED_PROP(float  ,_MipBias)

    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

    #define _BaseColor          UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial , _BaseColor)
    #define _SpecColor          (float4)0
    #define _EmissionColor      (float4)0
    #define _BaseMap_ST         UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial , _BaseMap_ST)
    #define _ColorRegion1       (float4)0
    #define _ColorRegion2       (float4)0
    #define _ColorRegion3       (float4)0
    #define _ColorRegion4       (float4)0
    #define _Cutoff             1
    #define _Surface            0
    #define _UsingAlbedoHue     0
    #define _ShaderType         0
    #define _AdditiveGI         UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial  , _AdditiveGI)
    #define _SpecularAAScreenSpaceVariance         UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial  , _SpecularAAScreenSpaceVariance)
    #define _SpecularAAThreshold         UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial  , _SpecularAAThreshold)
    
    #define  _BaseColorAmplify    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColorAmplify)
    #define  _CustomVec_0    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_0)
    #define  _CustomVec_1    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_1)
    #define  _CustomVec_2    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_2)
    #define  _CustomVec_3    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_3)
    #define  _CustomVec_4    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_4)
    #define  _CustomVec_5    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_5)
    #define  _CustomVec_6    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_6)
    #define  _CustomVec_7    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_7)
    #define  _CustomVec_8    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CustomVec_8)
    #define  _MipBias    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MipBias)


#endif

#include "../ShaderLibrary/SurfaceInput.hlsl"

#define PAV_SAMPLE_SPECULAR_SMOOTHNESS(uv, alpha, specColor) SampleSpecularSmoothness(uv, alpha, specColor)
#define PAV_INIT_SIMPLE_SURFACE_DATA(uv, outSurfaceData) InitializeSimpleLitSurfaceData(uv, outSurfaceData)

half4 SampleSpecularSmoothness(half2 uv, half alpha, half4 specColor)
{
    half4 specularSmoothness = specColor;// half4(0.0h, 0.0h, 0.0h, 1.0h); 
#ifdef _SPECGLOSSMAP
    specularSmoothness = PAV_SAMPLE_SPEC_GLOSS_MAP(uv) * specColor;
#elif defined(_SPECULAR_SETUP)
    specularSmoothness = specColor;
#endif

#ifdef _GLOSSINESS_FROM_BASE_ALPHA
    specularSmoothness.a = exp2(10 * alpha + 1);
#else
    specularSmoothness.a = exp2(10 * specularSmoothness.a + 1);
#endif

    return specularSmoothness;
}

half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
// TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
//#if defined(SHADER_API_GLES)
//    return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
//#else
    half occ = PAV_SAMPLE_OCCLUSION_MAP(uv);
    return occ;
    //return LerpWhiteTo(occ, _OcclusionStrength);
//#endif
#else
    return 1.0;
#endif
}

inline void InitializeSimpleLitSurfaceData(float3 uv3, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    PAV_GET_CUTOFF(cutoff);
    PAV_GET_BASE_COLOR(baseColor);
    PAV_GET_SPEC_COLOR(specColor);
    PAV_GET_EMISSION_COLOR(emissionColor);
    PAV_GET_SHADER_TYPE(shaderType);

    float2 uv = uv3.xy;
    half4 albedoAlpha = PAV_SAMPLE_ALBEDO_ALPHA(uv);
    
    outSurfaceData.alpha = albedoAlpha.a * baseColor.a;

#ifdef _ALPHATEST_ON
    AlphaDiscard(outSurfaceData.alpha, cutoff);
#endif
    
    half4 albedoMaskAlpha = PAV_SAMPLE_COLOR_REGIONS(uv);
    PAV_GET_USING_ALBEDO_HUE(usingAlbedoHue);
    outSurfaceData.albedo = ApplyAlbedo(albedoAlpha.rgb, baseColor.rgb, shaderType, albedoMaskAlpha, _ColorRegion1, _ColorRegion2, _ColorRegion3, _ColorRegion4, usingAlbedoHue); //need fix
#ifdef _ALPHAPREMULTIPLY_ON
    outSurfaceData.albedo *= outSurfaceData.alpha;
#endif
    
    half4 specularSmoothness = PAV_SAMPLE_SPECULAR_SMOOTHNESS(uv, outSurfaceData.alpha, specColor);
    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = specularSmoothness.rgb;
    outSurfaceData.smoothness = specularSmoothness.a;
    outSurfaceData.normalTS = PAV_SAMPLE_NORMAL(uv, 1.0h);
    outSurfaceData.occlusion = 1.0; // unused
    outSurfaceData.emission = PAV_SAMPLE_EMISSION(uv, emissionColor.rgb);
}

#endif
