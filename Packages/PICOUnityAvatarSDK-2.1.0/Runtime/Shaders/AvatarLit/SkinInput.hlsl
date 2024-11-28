#ifndef BODY_INPUT_INCLUDED
#define BODY_INPUT_INCLUDED

#include "Dependency/URP/ShaderLibrary/Core.hlsl"
#include "Dependency/Core/ShaderLibrary/CommonMaterial.hlsl"
#include "Common.hlsl"

#ifndef _RECEIVE_SHADOWS
#define _RECEIVE_SHADOWS_OFF
#endif

#if defined(_RIM_LIGHT) && defined(_RIMMASK_VIEW)
#define REQUIRES_VIEWSAPCE_NORMAL_COORD_INTERPOLATOR
#endif



// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _DetailMap_ST;

half _UsingAlbedoHue;
half4 _ColorRegion1;
half4 _ColorRegion2;
half4 _ColorRegion3;
half4 _ColorRegion4;

half4 _BaseColor;
half4 _EmissionColor;
half _Cutoff;

half _Smoothness1;
half _Metallic;
half _BumpScale;
half _DetailBumpScale;
half _OcclusionStrength;
half3 _OcclusionTint;


half _Smoothness2;
half _SpecularLobe1;
half _SpecularLobe2;
half _LobeMix;

// SSS
// half _SSCurveScale;
// half _SSNormalBlurScale;
// half4 _SSRemap;
// half3 _SSSColor;
// half3 _SSTransColor;

// Ramp
half _ShadowSmoothing;
half _ShadowRange;
half3 _ShadowColor;
half _LightShadow;

// Rim 
half3 _RimColor;
half _RimPower;
half _RimMin; 
half _RimMax;
half _RimRotation;
half _RimLengthThreshold;
half _RimLengthSmoothing;

half _Surface;
half _Tonemapping;

half _SkinOffset;
CBUFFER_END

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
// TEXTURE2D(_SSMask);             SAMPLER(sampler_SSMask);
TEXTURE2D(_DetailMap);          SAMPLER(sampler_DetailMap);


struct AdaptiveSurfaceData
{
    half3 albedo;
    half alpha;
    half metallic;
    half smoothness;

#ifdef _DUAL_SPECULAR
    half smoothnessSec;
#endif
    
    half3 emission;
    half occlusion;
    
    //Do we real need half ??
    half3 normalWS;
    half3 occlusionTint;
};

struct AdaptiveInputData
{
    float3  positionWS;
    half3   viewDirectionWS;
    float4  shadowCoord;
    //Fog?
    half    fogCoord;
    half3   bakedGI;
    half4   shadowMask;

    #if defined(_RIM_LIGHT)
    half3 rimNormalWS;
        #if defined(REQUIRES_VIEWSAPCE_NORMAL_COORD_INTERPOLATOR)
        half3 rimNormalVS;
        #endif
    #endif
};

#ifdef _SPECULAR_SETUP
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif

//优雅。
half Alpha(half albedoAlpha, half4 color, half cutoff)
{
    #if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
    half alpha = albedoAlpha * color.a;
    #else
    half alpha = color.a;
    #endif

    #if defined(_ALPHATEST_ON)
    clip(alpha - cutoff);
    #endif

    return alpha;
}

half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
{
    return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
}

// half3 ApplyDetailNormal(float2 detailUV,half3 normalTS)
// {
//     half3 detailNormalTS = UnpackNormalScale(,_DetailNormalMapScale);
//     detailNormalTS = normalize(detailNormalTS);
//     return BlendNormalRNM(normalTS,detailNormalTS);
// }

real3 UnpackNormalRG(real4 packedNormal, real scale = 1.0)
{
    real3 normal;
    normal.xy = packedNormal.rg * 2.0 - 1.0;
    normal.z = max(1.0e-16, sqrt(1.0 - saturate(dot(normal.xy, normal.xy))));

    normal.xy *= scale;
    return normal;
}

inline void InitializeAdaptiveSurfaceData(float4 uv,float3 normalWS,float4 tangentWS, out AdaptiveSurfaceData outSurfaceData)
{
    half4 albedoAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv.xy);
    
    outSurfaceData.albedo = ShiftTextureColor(albedoAlpha.rgb, _BaseColor.rgb,uv.xy, _ColorRegion1, _ColorRegion2, _ColorRegion3, _ColorRegion4, _UsingAlbedoHue);

    //if surface type == transparent, _Surface = 1
    outSurfaceData.alpha = lerp(1,albedoAlpha.a * _BaseColor.a,_Surface);
    outSurfaceData.occlusion = LerpWhiteTo(albedoAlpha.a, _OcclusionStrength * (1-_Surface));

#ifdef _NORMALMAP
    half3 bitangntWS = tangentWS.w * cross(normalWS.xyz, tangentWS.xyz);
    half4 nmr  = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv.xy);

    half3 normalTS = UnpackNormalRG(nmr,_BumpScale);
    
    #ifdef _DETAIL
        half4 detailTex = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uv.zw);
        half3 detailNormalTS = UnpackNormalRG(detailTex,_DetailBumpScale);
        normalTS = BlendNormalRNM(normalTS,detailNormalTS);
    #endif
    
    outSurfaceData.normalWS = TransformTangentToWorld(normalTS, half3x3(tangentWS.xyz, bitangntWS, normalWS.xyz));
    outSurfaceData.metallic = nmr.b;
    outSurfaceData.smoothness = nmr.a * _Smoothness1;
    
    #ifdef _DUAL_SPECULAR
        outSurfaceData.smoothnessSec = nmr.a * _Smoothness2;
    #endif
    
    outSurfaceData.emission = _EmissionColor.xyz;
#else
    outSurfaceData.normalWS = normalWS;
    outSurfaceData.metallic = _Metallic;
    outSurfaceData.smoothness = _Smoothness1;
    #ifdef _DUAL_SPECULAR
        outSurfaceData.smoothnessSec =  _Smoothness2;
    #endif
    outSurfaceData.emission = _EmissionColor.xyz;
#endif
    
    outSurfaceData.normalWS =  NormalizeNormalPerPixel(outSurfaceData.normalWS);
    outSurfaceData.occlusionTint = _OcclusionTint;
}


#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
