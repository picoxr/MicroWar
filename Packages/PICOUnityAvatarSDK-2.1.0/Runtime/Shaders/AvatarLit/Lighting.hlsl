#ifndef PICO_LIGHTING_INCLUDED
#define PICO_LIGHTING_INCLUDED

#include "Dependency/URP/ShaderLibrary/Lighting.hlsl"

#if defined(LIGHTMAP_ON) && defined(_FORCESPECULAR_ON)
#define _FORCE_SPECULARE
#endif

struct BSDFData
{
    half3 diffuseColor;
    half3 fresnel0;
    half perceptualRoughness; 
    half roughness;
    half partLambdaV;

    half roughnessSec;
    half partLambdaVSec;
};

struct RenderData
{
    half3 diffuse;
    half3 specular;
    
    //Only for debug
    half3 debug;
};

//@NoV: clamped NoV
void InitializeBSDFData(AdaptiveSurfaceData surfaceData,half NoV,half3 V,
out BSDFData outBSDFData)
{
    outBSDFData.diffuseColor = surfaceData.albedo * (1.0 - surfaceData.metallic);
    half perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surfaceData.smoothness);
    outBSDFData.perceptualRoughness = perceptualRoughness;
    outBSDFData.fresnel0 = lerp(DEFAULT_SPECULAR_VALUE, surfaceData.albedo, surfaceData.metallic);
    half  roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    outBSDFData.roughness = ClampRoughnessForAnalyticalLights(roughness);
    outBSDFData.partLambdaV = GetSmithJointGGXPartLambdaV(NoV,roughness);
    
    outBSDFData.roughnessSec = 0.0;
    outBSDFData.partLambdaVSec = 0.0;
#ifdef _DUAL_SPECULAR
    half perceptualRoughnessSec = PerceptualSmoothnessToPerceptualRoughness(surfaceData.smoothnessSec);
    outBSDFData.roughnessSec = PerceptualRoughnessToRoughness(perceptualRoughnessSec);
    outBSDFData.partLambdaVSec = GetSmithJointGGXPartLambdaV(NoV,outBSDFData.roughnessSec);
#endif
    
}

// https://game.watch.impress.co.jp/docs/news/575412.html
// Next-Generation-Character-Rendering-v6.ppt #115
half3 ColorBleedAO(half occlusion, half3 colorBleed)
{
    return pow(abs(occlusion), 1.0 - colorBleed);
}

//From https://blog.selfshadow.com/publications/s2013-shading-course/lazarov/s2013_pbs_black_ops_2_notes.pdf
half3 EnvironmentBRDF(half g,half NoV,half3 fresnel)
{
    half4 t =  half4(1 / 0.96, 0.475, (0.0275 - 0.25 * 0.04) / 0.96, 0.25);
    t *= float4(g, g, g, g);
    t += float4(0, 0, (0.015 - 0.75 * 0.04) / 0.96, 0.75);
    float a0 = t.x * min(t.y, exp2(-9.28 * NoV)) + t.z;
    float a1 = t.w;
    return saturate(a0 + fresnel * (a1 - a0));
}


void DirectBRDFRendering(half3 viewDir,half3 normalWS,half NoV,half clampedNoV,BSDFData bsdfData,Light light, out RenderData renderData)
{
    half NoL = dot(normalWS, light.direction);
    half clampedNoL = saturate(NoL);
    
    half LoV, NoH, LoH, invLenLV;
    GetBSDFAngle(viewDir,light.direction,NoL,NoV,LoV,NoH,LoH,invLenLV);
    
    half3 radiance = clampedNoL  * light.color * light.distanceAttenuation * light.shadowAttenuation;
    half diffuseTerm = Lambert() * PI;
    
    half3 specularTerm;
#ifndef _SPECULARHIGHLIGHTS_OFF
    half3 F = F_Schlick(bsdfData.fresnel0,LoH);
    half DV = DV_SmithJointGGX(NoH,abs(NoL),clampedNoV,bsdfData.roughness,bsdfData.partLambdaV);
    specularTerm = DV * F;
#else
    specularTerm = 0;
#endif
    renderData.diffuse = diffuseTerm * bsdfData.diffuseColor * radiance;
    renderData.specular = specularTerm * radiance;
    renderData.debug = 0;
}


#endif
