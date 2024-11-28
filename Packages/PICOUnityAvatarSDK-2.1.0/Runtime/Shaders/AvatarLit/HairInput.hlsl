#ifndef HAIR_INPUT_INCLUDED
#define HAIR_INPUT_INCLUDED

#include "Dependency/URP/ShaderLibrary/Core.hlsl"
#include "Dependency/Core/ShaderLibrary/CommonMaterial.hlsl"
#include "Dependency/URP/ShaderLibrary/SurfaceInput.hlsl"
#include "Common.hlsl"


// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;

half _UsingAlbedoHue;
half4 _ColorRegion1;
half4 _ColorRegion2;
half4 _ColorRegion3;
half4 _ColorRegion4;

half _Cutoff;
half _BumpScale;
half3 _ShadowColor;
half _ShadowSmoothing;
half _ShadowRange;

half _EnvIntensity;
half _ReflectionRoughness;
half _ReflectionIntensity;

half _StrandDir;
half _SpecularShift;
half _ShiftStrength;
half4 _SpecularTint;
half _SpecularExponent;

//Secondary
half _SecondarySpecularShift;
half _SecondaryShiftStrength;
half4 _SecondarySpecularTint;
half _SecondarySpecularExponent;
half _OcclusionStrength;

half _Tonemapping;
CBUFFER_END

struct  AdaptiveSurfaceData
{
    half3 albedo;
    half3 flowDir;
    
    half specularExp1;
    half3 specularColor1;
    half shfit1;

    half specularExp2;
    half3 specularColor2;
    half shfit2;
    
    half occlusion;
    half specularOcclusion;
    
    half3 normalWS;
    half3 tangentWS;
    half3 bitangentWS;

    half perceptualRoughness;
    
};

TEXTURE2D(_FlowMap);        SAMPLER(sampler_FlowMap);


inline void InitializeAdaptiveSurfaceData(float2 uv,half3 normalWS,half4 tangentWS, out AdaptiveSurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.albedo = ShiftTextureColor(albedoAlpha.rgb, _BaseColor.rgb,uv, _ColorRegion1, _ColorRegion2, _ColorRegion3, _ColorRegion4, _UsingAlbedoHue);

    outSurfaceData.occlusion = LerpWhiteTo(albedoAlpha.a, _OcclusionStrength);
    outSurfaceData.perceptualRoughness  = _ReflectionRoughness;
    outSurfaceData.specularExp1         = _SpecularExponent;
    outSurfaceData.specularColor1       = _SpecularTint.rgb * _SpecularTint.a;
    outSurfaceData.specularExp2         = _SecondarySpecularExponent;
    outSurfaceData.specularColor2       = _SecondarySpecularTint.rgb * _SecondarySpecularTint.a;
    
    float sgn = tangentWS.w;      // should be either +1 or -1
    outSurfaceData.tangentWS = tangentWS.xyz;
    outSurfaceData.bitangentWS = sgn * cross(normalWS.xyz, tangentWS.xyz);
    
#ifdef _NORMALMAP
    half4 data = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
    data.z -= 0.5;
    half3 normalTS          = UnpackNormalXYScale(data.xy, _BumpScale);
    outSurfaceData.normalWS             = TransformTangentToWorld(normalTS, half3x3(tangentWS.xyz, outSurfaceData.bitangentWS.xyz, normalWS.xyz));
    outSurfaceData.shfit1               = _SpecularShift + data.z * _ShiftStrength;
    outSurfaceData.shfit2               = _SecondarySpecularShift + data.z * _SecondaryShiftStrength;
    outSurfaceData.specularOcclusion    = data.w;
#else
    outSurfaceData.normalWS             = normalWS.xyz;
    outSurfaceData.shfit1               = _SpecularShift;
    outSurfaceData.shfit2               = _SecondarySpecularShift;
    outSurfaceData.specularOcclusion    = 1.0h;
#endif
    outSurfaceData.normalWS = NormalizeNormalPerPixel(outSurfaceData.normalWS);
    
#if defined(_STRANDDIR_BITANGENT) || defined(_STRANDDIR_BITANGENT)
    outSurfaceData.flowDir = (0).xxx;
#else
    // NOTE: Do we need SAMPLE_TEXTURE2D_LOD
    half3 flowDirTS = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, uv).rgb;
    flowDirTS.rg = flowDirTS.rg * 2.0h - 1.0h;
    half3 flowDirWS = TransformTangentToWorld(flowDirTS, half3x3(tangentWS.xyz, outSurfaceData.bitangentWS.xyz, normalWS.xyz));
    outSurfaceData.flowDir = (flowDirWS);
#endif
    
    
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
