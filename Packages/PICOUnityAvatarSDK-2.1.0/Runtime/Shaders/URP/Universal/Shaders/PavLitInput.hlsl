#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "../../PavConfig.hlsl"
#include "../ShaderLibrary/Core.hlsl"
#include "../../Core/ShaderLibrary/CommonMaterial.hlsl"

#include "../../Core/ShaderLibrary/ParallaxMapping.hlsl"

#if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
#define _DETAIL
#endif

#define _USE_SPECULAR_AA

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

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
half4 _DetailAlbedoMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _Parallax;
half _OcclusionStrength;
half _ClearCoatMask;
half _ClearCoatSmoothness;
half _ShaderType;
half _DetailAlbedoMapScale;
half _DetailNormalMapScale;
half _Surface;
half _UsingAlbedoHue;
half4 _BaseMap_ST;

half _AdditiveGI;
half4 _ColorRegion1;
half4 _ColorRegion2;
half4 _ColorRegion3;
half4 _ColorRegion4;

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

// NOTE: Do not ifdef the properties for dots instancing, but ifdef the actual usage.
// Otherwise you might break CPU-side as property constant-buffer offsets change per variant.
// NOTE: Dots instancing is orthogonal to the constant buffer above.
#ifdef UNITY_DOTS_INSTANCING_ENABLED
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
    UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float , _Smoothness)
    UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
    UNITY_DOTS_INSTANCED_PROP(float , _BumpScale)
    UNITY_DOTS_INSTANCED_PROP(float , _Parallax)
    UNITY_DOTS_INSTANCED_PROP(float , _OcclusionStrength)
    UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatMask)
    UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatSmoothness)
    UNITY_DOTS_INSTANCED_PROP(float , _ShaderType)
    UNITY_DOTS_INSTANCED_PROP(float , _DetailAlbedoMapScale)
    UNITY_DOTS_INSTANCED_PROP(float , _DetailNormalMapScale)
    UNITY_DOTS_INSTANCED_PROP(float , _Surface)
    UNITY_DOTS_INSTANCED_PROP(float , _UsingAlbedoHue)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float,  _AdditiveGI)
    UNITY_DOTS_INSTANCED_PROP(float4, _ColorRegion1)
    UNITY_DOTS_INSTANCED_PROP(float4, _ColorRegion2)
    UNITY_DOTS_INSTANCED_PROP(float4, _ColorRegion3)
    UNITY_DOTS_INSTANCED_PROP(float4, _ColorRegion4)
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

#define _BaseColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__BaseColor)
#define _SpecColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__SpecColor)
#define _EmissionColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__EmissionColor)
#define _Cutoff                 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Cutoff)
#define _Smoothness             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Smoothness)
#define _Metallic               UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Metallic)
#define _BumpScale              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__BumpScale)
#define _Parallax               UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Parallax)
#define _OcclusionStrength      UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__OcclusionStrength)
#define _ClearCoatMask          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ClearCoatMask)
#define _ClearCoatSmoothness    UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ClearCoatSmoothness)
#define _ShaderType             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ShaderType)
#define _DetailAlbedoMapScale   UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__DetailAlbedoMapScale)
#define _DetailNormalMapScale   UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__DetailNormalMapScale)
#define _Surface                UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Surface)
#define _UsingAlbedoHue         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__UsingAlbedoHue)
#define _BaseMap_ST             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__BaseMap_ST)
#define _AdditiveGI             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__AdditiveGI)
#define _ColorRegion1           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__ColorRegion1)
#define _ColorRegion2           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__ColorRegion2)
#define _ColorRegion3           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__ColorRegion3)
#define _ColorRegion4           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__ColorRegion4)
#define _SpecularAAScreenSpaceVariance           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float , Metadata__ColorRegion4)
#define _SpecularAAThreshold           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float , Metadata__ColorRegion4)

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
#endif

#include "../ShaderLibrary/SurfaceInput.hlsl"

TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
//TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);
TEXTURE2D(_ClearCoatMap);       SAMPLER(sampler_ClearCoatMap);

#ifdef _SPECULAR_SETUP
    #define SAMPLE_METALLICSPECULAR(uv) PAV_SAMPLE_SPEC_GLOSS_MAP(uv)
#else
    #define SAMPLE_METALLICSPECULAR(uv) PAV_SAMPLE_METALLIC_GLOSS_MAP(uv)
#endif

#define PAV_SAMPLE_OCCLUSION(uv) SampleOcclusion(uv)
#define PAV_SAMPLE_METALLIC_SPEC_GLOSS(uv, albedoAlpha) SampleMetallicSpecGloss(uv, albedoAlpha)
#define PAV_APPLY_PER_PIXEL_DISPLACEMENT(uv, albedoAlpha) ApplyPerPixelDisplacement(viewDirTS, uv)
#define PAV_APPLY_DETAIL_ALBEDO(detailUv, albedo, detailMask) ApplyDetailAlbedo(detailUv, albedo, detailMask)
#define PAV_APPLY_DETAIL_NORMAL(detailUv, normalTS, detailMask) ApplyDetailNormal(detailUv, normalTS, detailMask)
#define PAV_SAMPLE_CLEAR_COAT(uv) SampleClearCoat(uv)
#define PAV_INIT_SURFACE_DATA(uv, outSurfaceData) InitializeStandardLitSurfaceData(uv, outSurfaceData)




half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;

    PAV_GET_SMOOTHNESS(smoothness);
    PAV_GET_METALLIC(metallic);
    PAV_GET_SPEC_COLOR(specColor);

#ifdef _METALLICSPECGLOSSMAP
    specGloss = SAMPLE_METALLICSPECULAR(uv);
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * smoothness;
    #else
        specGloss.a *= smoothness;
    #endif
    //specGloss.rgb *= specColor.rgb;
#else // _METALLICSPECGLOSSMAP
    #if _SPECULAR_SETUP
        specGloss.rgb = specColor.rgb;
    #else
        specGloss.rgb = metallic.rrr;
    #endif

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * smoothness;
    #else
        specGloss.a = smoothness;
    #endif
#endif

    return specGloss;
}

half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
// TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
//#if defined(SHADER_API_GLES)
//    return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
//#else
    PAV_GET_OCCLUSION_STRENGTH(occlusionStrength);

    half occ = PAV_SAMPLE_OCCLUSION_MAP(uv);
    return LerpWhiteTo(occ, occlusionStrength);
//#endif
#else
    return 1.0;
#endif
}

// Returns clear coat parameters
// .x/.r == mask
// .y/.g == smoothness
half2 SampleClearCoat(float2 uv)
{
#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    PAV_GET_CLEAR_COAT_MASK(clearCoatMask);
    PAV_GET_CLEAR_COAT_SMOOTHNESS(clearCoatSmoothness);

    half2 clearCoatMaskSmoothness = half2(clearCoatMask, clearCoatSmoothness);

#if defined(_CLEARCOATMAP)
    clearCoatMaskSmoothness *= SAMPLE_TEXTURE2D(_ClearCoatMap, sampler_ClearCoatMap, uv).rg;
#endif

    return clearCoatMaskSmoothness;
#else
    return half2(0.0, 1.0);
#endif  // _CLEARCOAT
}

void ApplyPerPixelDisplacement(half3 viewDirTS, inout float3 uv)
{
#if defined(_PARALLAXMAP)
    PAV_GET_PARALLAX(parallax);
    uv.xy += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, parallax, uv.xy);
#endif
}

// Used for scaling detail albedo. Main features:
// - Depending if detailAlbedo brightens or darkens, scale magnifies effect.
// - No effect is applied if detailAlbedo is 0.5.
half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
{
    // detailAlbedo = detailAlbedo * 2.0h - 1.0h;
    // detailAlbedo *= _DetailAlbedoMapScale;
    // detailAlbedo = detailAlbedo * 0.5h + 0.5h;
    // return detailAlbedo * 2.0f;

    // A bit more optimized
    return 2.0h * detailAlbedo * scale - scale + 1.0h;
}

half3 ApplyDetailAlbedo(float2 detailUv, half3 albedo, half detailMask)
{
#if defined(_DETAIL)
    half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, detailUv).rgb;

    // In order to have same performance as builtin, we do scaling only if scale is not 1.0 (Scaled version has 6 additional instructions)
#if defined(_DETAIL_SCALED)
    PAV_GET_DETAIL_ALBEDO_MAP_SCALE(detailAlbedoMapScale);

    detailAlbedo = ScaleDetailAlbedo(detailAlbedo, detailAlbedoMapScale);
#else
    detailAlbedo = 2.0h * detailAlbedo;
#endif

    return albedo * LerpWhiteTo(detailAlbedo, detailMask);
#else
    return albedo;
#endif
}

half3 ApplyDetailNormal(float2 detailUv, half3 normalTS, half detailMask)
{
#if defined(_DETAIL)
#if BUMP_SCALE_NOT_SUPPORTED
    half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv));
#else
    PAV_GET_DETAIL_NORMAL_MAP_SCALE(detailNormalMapScale);

    half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv), detailNormalMapScale);
#endif

    // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
    // For visual consistancy we going to do in all cases
    detailNormalTS = normalize(detailNormalTS);

    return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
#else
    return normalTS;
#endif
}

inline void InitializeStandardLitSurfaceData(float3 uv3, out SurfaceData outSurfaceData)
{
    float2 uv = uv3.xy;
    half4 albedoAlpha = PAV_SAMPLE_ALBEDO_ALPHA(uv);
    half4 albedoMaskAlpha = PAV_SAMPLE_COLOR_REGIONS(uv);
    half4 specGloss = PAV_SAMPLE_METALLIC_SPEC_GLOSS(uv, albedoAlpha.a);

    PAV_GET_CUTOFF(cutoff);
    PAV_GET_METALLIC(metallic);
    PAV_GET_BUMP_SCALE(bumpScale);
    PAV_GET_BASE_COLOR(baseColor);
    PAV_GET_USING_ALBEDO_HUE(usingAlbedoHue);
    PAV_GET_BASE_COLOR_MASK1(colorMask1);
    PAV_GET_BASE_COLOR_MASK2(colorMask2);
    PAV_GET_BASE_COLOR_MASK3(colorMask3);
    PAV_GET_BASE_COLOR_MASK4(colorMask4);
    PAV_GET_EMISSION_COLOR(emissionColor);
    PAV_GET_SHADER_TYPE(shaderType);

    outSurfaceData.alpha = Alpha(albedoAlpha.a, baseColor, cutoff);

    //outSurfaceData.albedo = ApplyAlbedo(albedoAlpha.rgb, baseColor.rgb, shaderType);

    uint shaderTypeI = (uint) round(shaderType);

    // modified by dujing and tianshengcai
    if (//shaderTypeI == PAV_SHADER_TYPE_BODY_BASE 
        shaderTypeI == PAV_SHADER_TYPE_HAIR_BASE
        //shaderTypeI == PAV_SHADER_TYPE_CLOTH_BASE
        )
    {
        colorMask3.rgb = uv3;
    }
    
    outSurfaceData.albedo = ApplyAlbedo(albedoAlpha.rgb, baseColor.rgb, shaderType, albedoMaskAlpha, colorMask1, colorMask2, colorMask3, colorMask4, usingAlbedoHue);

#if _SPECULAR_SETUP
    outSurfaceData.metallic = 1.0h;
    outSurfaceData.specular = specGloss.rgb;
#else
    outSurfaceData.metallic = specGloss.r * metallic;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
#endif

    outSurfaceData.smoothness = specGloss.a;

    outSurfaceData.normalTS = PAV_SAMPLE_NORMAL(uv, bumpScale);
    outSurfaceData.occlusion = PAV_SAMPLE_OCCLUSION(uv);
    outSurfaceData.emission = PAV_SAMPLE_EMISSION(uv, emissionColor.rgb);

#ifdef PAV_ToonShadowMap
    outSurfaceData.toonShadow = PAV_SAMPLE_TOON_SHADOW(uv);
#endif

#if (defined(_CLEARCOAT) || defined(_CLEARCOATMAP)) && !defined(_ENABLE_STATIC_MESH_BATCHING)
    half2 clearCoat = PAV_SAMPLE_CLEAR_COAT(uv);
    outSurfaceData.clearCoatMask       = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
#else
    outSurfaceData.clearCoatMask       = 0.0h;
    outSurfaceData.clearCoatSmoothness = 0.0h;
#endif

#if defined(_DETAIL) && !defined(_ENABLE_STATIC_MESH_BATCHING)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = PAV_APPLY_DETAIL_ALBEDO(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = PAV_APPLY_DETAIL_NORMAL(detailUv, outSurfaceData.normalTS, detailMask);

#endif
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
