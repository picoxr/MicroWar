#ifndef AVATAR_LIT_INPUT_INCLUDED
#define AVATAR_LIT_INPUT_INCLUDED

#include "Dependency/URP/ShaderLibrary/Core.hlsl"
#include "Dependency/Core/ShaderLibrary/CommonMaterial.hlsl"
#include "Dependency/Core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Dependency/URP/ShaderLibrary/SurfaceInput.hlsl"
#include "Common.hlsl"

#ifndef _RECEIVE_SHADOWS
#define _RECEIVE_SHADOWS_OFF
#endif

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;

half _UsingAlbedoHue;
half4 _ColorRegion1;
half4 _ColorRegion2;
half4 _ColorRegion3;
half4 _ColorRegion4;

half4 _EmissionColor;

half _Cutoff;
half _Smoothness;
half _SmoothnessMin;
half _SmoothnessMax;

half _Metallic;
half _BumpScale;
half _OcclusionStrength;

half _MatCapStrength;
half _Tonemapping;
CBUFFER_END

#ifdef _MATCAP
TEXTURE2D(_MatCapTex); SAMPLER(sampler_MatCapTex);
#endif

#if defined(_ENABLE_STATIC_MESH_BATCHING)

struct CurrentUniform
{
    float4 u1, u2, u3;
} currentData;

#define USING_ALBEDO_HUE_VALUE          currentData.u1.x
#define SMOOTHNESS_VALUE                currentData.u1.y
#define SMOOTHNESS_MIN_VALUE            currentData.u1.z
#define SMOOTHNESS_MAX_VALUE            currentData.u1.w
#define CUTOFF_VALUE                    currentData.u2.x
#define METALLIC_VALUE                  currentData.u2.y
#define BUMP_SCALE_VALUE                currentData.u2.z
#define OCCLUSION_STRENGTH_VALUE        currentData.u2.w
#define MAT_CAP_STRENGTH_VALUE          currentData.u3.x
#define BASE_MAP_ST_VALUE               _MtlData[mtlIndex].baseMapST
#define BASE_COLOR_VALUE                _MtlData[mtlIndex].baseColor
#define COLOR_REGION_VALUE(index)       _MtlData[mtlIndex].colorRegion##index
#define EMISSION_COLOR                  _MtlData[mtlIndex].emissiveColor

#else

#define USING_ALBEDO_HUE_VALUE          _UsingAlbedoHue
#define SMOOTHNESS_VALUE                _Smoothness
#define SMOOTHNESS_MIN_VALUE            _SmoothnessMin 
#define SMOOTHNESS_MAX_VALUE            _SmoothnessMax
#define CUTOFF_VALUE                    _Cutoff
#define METALLIC_VALUE                  _Metallic
#define BUMP_SCALE_VALUE                _BumpScale
#define OCCLUSION_STRENGTH_VALUE        _OcclusionStrength
#define MAT_CAP_STRENGTH_VALUE          _MatCapStrength
#define BASE_MAP_ST_VALUE               _BaseMap_ST 
#define BASE_COLOR_VALUE                _BaseColor 
#define COLOR_REGION_VALUE(index)       _ColorRegion##index
#define EMISSION_COLOR                  _EmissionColor

#endif

struct AdaptiveSurfaceData
{
    half3 albedo;
    half metallic;
    half smoothness;
    half3 emission;
    half occlusion;

    //Do we real need half ??
    half3 normalWS;
    half3 tangentWS;
    half3 bitangntWS;
    half alpha;
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
};

#ifdef _SPECULAR_SETUP
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif


inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
#if defined(_ENABLE_STATIC_MESH_BATCHING)
    half4 albedoAlpha = SampleTexArray(0, mtlIndex, uv, _BaseMapArray, sampler_BaseMapArray, half4(1.0, 1.0, 1.0, 1.0));
#else
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
#endif
    half4 baseColor = BASE_COLOR_VALUE;
    outSurfaceData.albedo = ShiftTextureColor(albedoAlpha.rgb, baseColor.rgb, uv, COLOR_REGION_VALUE(1), COLOR_REGION_VALUE(2), COLOR_REGION_VALUE(3), COLOR_REGION_VALUE(4), USING_ALBEDO_HUE_VALUE);
    outSurfaceData.alpha = albedoAlpha.a * baseColor.a;
    
    #ifdef _NORMALMAP
    #if defined(_ENABLE_STATIC_MESH_BATCHING)
    half4 nmrColor = SampleTexArray(2, mtlIndex, uv, _BumpMapArray, sampler_BumpMapArray, half4(0.5, 0.5, 1.0, 0.5));
    #else
    half4 nmrColor = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
    #endif
    nmrColor.a = Remap_Half(nmrColor.a,half2(0,1),half2(SMOOTHNESS_MIN_VALUE,SMOOTHNESS_MAX_VALUE));
    half2 normalTS_XY = nmrColor.rg * 2.0 - 1.0;
    half normalTS_Z = max(1.0e-16, sqrt(1.0 - saturate(dot(normalTS_XY, normalTS_XY))));
    normalTS_XY *= BUMP_SCALE_VALUE;
    half3 normalTS = half3(normalTS_XY, normalTS_Z);
    #else
    half4 nmrColor = half4(0.0h,0.0h,1.0h,1.0h);
    half3 normalTS = half3(0.0h, 0.0h, 1.0h);
    #endif
    
    outSurfaceData.metallic =  nmrColor.b * METALLIC_VALUE;
    outSurfaceData.smoothness = nmrColor.a * SMOOTHNESS_VALUE;
    #if defined(_ENABLE_STATIC_MESH_BATCHING)
    outSurfaceData.emission = SampleTexArray(3, mtlIndex, uv, _EmissionMapArray, sampler_EmissionMapArray, half4(0.0, 0.0, 0.0, 1.0)).xyz * EMISSION_COLOR.rgb;
    #else
    outSurfaceData.emission = SampleEmission(uv, EMISSION_COLOR.rgb, TEXTURE2D_ARGS(_EmissionMap,sampler_EmissionMap));
    #endif
    outSurfaceData.occlusion = LerpWhiteTo(albedoAlpha.a, OCCLUSION_STRENGTH_VALUE);
    outSurfaceData.normalTS =  normalTS;

    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
    
}






#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
