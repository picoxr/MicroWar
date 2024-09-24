#ifndef UNIVERSAL_LIGHTING_PBRSKIN_INCLUDED
#define UNIVERSAL_LIGHTING_PBRSKIN_INCLUDED

#include "../../Core/ShaderLibrary/Common.hlsl"
#include "../../Core/ShaderLibrary/CommonMaterial.hlsl"
#include "../../Core/ShaderLibrary/EntityLighting.hlsl"
#include "../../Core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "../../Core/ShaderLibrary/BSDF.hlsl"
#include "./Core.hlsl"
#include "./Deprecated.hlsl"
#include "./SurfaceData.hlsl"
#include "./Shadows.hlsl"

// If lightmap is not defined than we evaluate GI (ambient + probes) from SH
// We might do it fully or partially in vertex to save shader ALU
//#if !defined(LIGHTMAP_ON)
//// TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
//    #if defined(SHADER_API_GLES) || !defined(_NORMALMAP)
//        // Evaluates SH fully in vertex
//        #define EVALUATE_SH_VERTEX
//    #elif !SHADER_HINT_NICE_QUALITY
//        // Evaluates L2 SH in vertex and L0L1 in pixel
//        #define EVALUATE_SH_MIXED
//    #endif
//        // Otherwise evaluate SH fully per-pixel
//#endif

#ifdef LIGHTMAP_ON
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) float2 lmName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw;
    #define OUTPUT_SH(normalWS, OUT)
#else
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) half3 shName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
    #define OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)
#endif

// Renamed -> LIGHTMAP_SHADOW_MIXING
#if !defined(_MIXED_LIGHTING_SUBTRACTIVE) && defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
    #define _MIXED_LIGHTING_SUBTRACTIVE
#endif

///////////////////////////////////////////////////////////////////////////////
//                          Light Helpers                                    //
///////////////////////////////////////////////////////////////////////////////

// Abstraction over Light shading data.
struct Light
{
    half3   direction;
    half3   color;
    half    distanceAttenuation;
    half    shadowAttenuation;
};

///////////////////////////////////////////////////////////////////////////////
//                        Attenuation Functions                               /
///////////////////////////////////////////////////////////////////////////////

// Matches Unity Vanila attenuation
// Attenuation smoothly decreases to light range.
float DistanceAttenuation(float distanceSqr, half2 distanceAttenuation)
{
    // We use a shared distance attenuation for additional directional and puctual lights
    // for directional lights attenuation will be 1
    float lightAtten = rcp(distanceSqr);

#if SHADER_HINT_NICE_QUALITY
    // Use the smoothing factor also used in the Unity lightmapper.
    half factor = distanceSqr * distanceAttenuation.x;
    half smoothFactor = saturate(1.0h - factor * factor);
    smoothFactor = smoothFactor * smoothFactor;
#else
    // We need to smoothly fade attenuation to light range. We start fading linearly at 80% of light range
    // Therefore:
    // fadeDistance = (0.8 * 0.8 * lightRangeSq)
    // smoothFactor = (lightRangeSqr - distanceSqr) / (lightRangeSqr - fadeDistance)
    // We can rewrite that to fit a MAD by doing
    // distanceSqr * (1.0 / (fadeDistanceSqr - lightRangeSqr)) + (-lightRangeSqr / (fadeDistanceSqr - lightRangeSqr)
    // distanceSqr *        distanceAttenuation.y            +             distanceAttenuation.z
    half smoothFactor = saturate(distanceSqr * distanceAttenuation.x + distanceAttenuation.y);
#endif

    return lightAtten * smoothFactor;
}

half AngleAttenuation(half3 spotDirection, half3 lightDirection, half2 spotAttenuation)
{
    // Spot Attenuation with a linear falloff can be defined as
    // (SdotL - cosOuterAngle) / (cosInnerAngle - cosOuterAngle)
    // This can be rewritten as
    // invAngleRange = 1.0 / (cosInnerAngle - cosOuterAngle)
    // SdotL * invAngleRange + (-cosOuterAngle * invAngleRange)
    // SdotL * spotAttenuation.x + spotAttenuation.y

    // If we precompute the terms in a MAD instruction
    half SdotL = dot(spotDirection, lightDirection);
    half atten = saturate(SdotL * spotAttenuation.x + spotAttenuation.y);
    return atten * atten;
}

///////////////////////////////////////////////////////////////////////////////
//                      Light Abstraction                                    //
///////////////////////////////////////////////////////////////////////////////

Light GetMainLight()
{
    Light light;
    light.direction = _MainLightPosition.xyz;
    light.distanceAttenuation = unity_LightData.z; // unity_LightData.z is 1 when not culled by the culling mask, otherwise 0.
    light.shadowAttenuation = 1.0;
    light.color = _MainLightColor.rgb;

    return light;
}

Light GetMainLight(float4 shadowCoord)
{
    Light light = GetMainLight();
    light.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
    return light;
}

Light GetMainLight(float4 shadowCoord, float3 positionWS, half4 shadowMask)
{
    Light light = GetMainLight();
    light.shadowAttenuation = MainLightShadow(shadowCoord, positionWS, shadowMask, _MainLightOcclusionProbes);
    return light;
}

// Fills a light struct given a perObjectLightIndex
Light GetAdditionalPerObjectLight(int perObjectLightIndex, float3 positionWS)
{
    // Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
    half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
#else
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
#endif

    // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
    // This way the following code will work for both directional and punctual lights.
    float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    half attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = 1.0;
    light.color = color;

    return light;
}

uint GetPerObjectLightIndexOffset()
{
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    return unity_LightData.x;
#else
    return 0;
#endif
}

// Returns a per-object index given a loop index.
// This abstract the underlying data implementation for storing lights/light indices
int GetPerObjectLightIndex(uint index)
{
/////////////////////////////////////////////////////////////////////////////////////////////
// Structured Buffer Path                                                                   /
//                                                                                          /
// Lights and light indices are stored in StructuredBuffer. We can just index them.         /
// Currently all non-mobile platforms take this path :(                                     /
// There are limitation in mobile GPUs to use SSBO (performance / no vertex shader support) /
/////////////////////////////////////////////////////////////////////////////////////////////
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    uint offset = unity_LightData.x;
    return _AdditionalLightsIndices[offset + index];

/////////////////////////////////////////////////////////////////////////////////////////////
// UBO path                                                                                 /
//                                                                                          /
// We store 8 light indices in float4 unity_LightIndices[2];                                /
// Due to memory alignment unity doesn't support int[] or float[]                           /
// Even trying to reinterpret cast the unity_LightIndices to float[] won't work             /
// it will cast to float4[] and create extra register pressure. :(                          /
/////////////////////////////////////////////////////////////////////////////////////////////
#elif !defined(SHADER_API_GLES)
    // since index is uint shader compiler will implement
    // div & mod as bitfield ops (shift and mask).

    // TODO: Can we index a float4? Currently compiler is
    // replacing unity_LightIndicesX[i] with a dp4 with identity matrix.
    // u_xlat16_40 = dot(unity_LightIndices[int(u_xlatu13)], ImmCB_0_0_0[u_xlati1]);
    // This increases both arithmetic and register pressure.
    return unity_LightIndices[index / 4][index % 4];
#else
    // Fallback to GLES2. No bitfield magic here :(.
    // We limit to 4 indices per object and only sample unity_4LightIndices0.
    // Conditional moves are branch free even on mali-400
    // small arithmetic cost but no extra register pressure from ImmCB_0_0_0 matrix.
    half2 lightIndex2 = (index < 2.0h) ? unity_LightIndices[0].xy : unity_LightIndices[0].zw;
    half i_rem = (index < 2.0h) ? index : index - 2.0h;
    return (i_rem < 1.0h) ? lightIndex2.x : lightIndex2.y;
#endif
}

// Fills a light struct given a loop i index. This will convert the i
// index to a perObjectLightIndex
Light GetAdditionalLight(uint i, float3 positionWS)
{
    int perObjectLightIndex = GetPerObjectLightIndex(i);
    return GetAdditionalPerObjectLight(perObjectLightIndex, positionWS);
}

Light GetAdditionalLight(uint i, float3 positionWS, half4 shadowMask)
{
    int perObjectLightIndex = GetPerObjectLightIndex(i);
    Light light = GetAdditionalPerObjectLight(perObjectLightIndex, positionWS);

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    half4 occlusionProbeChannels = _AdditionalLightsBuffer[perObjectLightIndex].occlusionProbeChannels;
#else
    half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[perObjectLightIndex];
#endif
    light.shadowAttenuation = AdditionalLightShadow(perObjectLightIndex, positionWS, shadowMask, occlusionProbeChannels);

    return light;
}

int GetAdditionalLightsCount()
{
    // TODO: we need to expose in SRP api an ability for the pipeline cap the amount of lights
    // in the culling. This way we could do the loop branch with an uniform
    // This would be helpful to support baking exceeding lights in SH as well
    return min(_AdditionalLightsCount.x, unity_LightData.y);
}

///////////////////////////////////////////////////////////////////////////////
//                         BRDF Functions                                    //
///////////////////////////////////////////////////////////////////////////////

#define kDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)

struct BRDFData
{
    half3 diffuse;
    half3 specular;
    half reflectivity;
    half perceptualRoughness;
    half roughness;
    half roughness2;
    half grazingTerm;

    // We save some light invariant BRDF terms so we don't have to recompute
    // them in the light loop. Take a look at DirectBRDF function for detailed explaination.
    half normalizationTerm;     // roughness * 4.0 + 2.0
    half roughness2MinusOne;    // roughness^2 - 1.0
};

half ReflectivitySpecular(half3 specular)
{
#if defined(SHADER_API_GLES)
    return specular.r; // Red channel - because most metals are either monocrhome or with redish/yellowish tint
#else
    return max(max(specular.r, specular.g), specular.b);
#endif
}

half OneMinusReflectivityMetallic(half metallic)
{
    // We'll need oneMinusReflectivity, so
    //   1-reflectivity = 1-lerp(dielectricSpec, 1, metallic) = lerp(1-dielectricSpec, 0, metallic)
    // store (1-dielectricSpec) in kDielectricSpec.a, then
    //   1-reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) =
    //                  = alpha - metallic * alpha
    half oneMinusDielectricSpec = kDielectricSpec.a;
    return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

inline void InitializeBRDFDataDirect(half3 diffuse, half3 specular, half reflectivity, half oneMinusReflectivity, half smoothness, inout half alpha, out BRDFData outBRDFData)
{
    outBRDFData.diffuse = diffuse;
    outBRDFData.specular = specular;
    outBRDFData.reflectivity = reflectivity;

    outBRDFData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
    outBRDFData.roughness           = max(PerceptualRoughnessToRoughness(outBRDFData.perceptualRoughness), HALF_MIN_SQRT);
    outBRDFData.roughness2          = max(outBRDFData.roughness * outBRDFData.roughness, HALF_MIN);
    outBRDFData.grazingTerm         = saturate(smoothness + reflectivity);
    outBRDFData.normalizationTerm   = outBRDFData.roughness * 4.0h + 2.0h;
    outBRDFData.roughness2MinusOne  = outBRDFData.roughness2 - 1.0h;

#ifdef _ALPHAPREMULTIPLY_ON
    outBRDFData.diffuse *= alpha;
    alpha = alpha * oneMinusReflectivity + reflectivity; // NOTE: alpha modified and propagated up.
#endif
}

inline void InitializeBRDFData(half3 albedo, half metallic, half3 specular, half smoothness, inout half alpha, out BRDFData outBRDFData)
{
#ifdef _SPECULAR_SETUP
    half reflectivity = ReflectivitySpecular(specular);
    half oneMinusReflectivity = 1.0 - reflectivity;
    half3 brdfDiffuse = albedo * (half3(1.0h, 1.0h, 1.0h) - specular);
    half3 brdfSpecular = specular;
#else
    half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
    half reflectivity = 1.0 - oneMinusReflectivity;
    half3 brdfDiffuse = albedo * oneMinusReflectivity;
    half3 brdfSpecular = lerp(kDieletricSpec.rgb, albedo, metallic);
#endif

    InitializeBRDFDataDirect(brdfDiffuse, brdfSpecular, reflectivity, oneMinusReflectivity, smoothness, alpha, outBRDFData);
}

half3 ConvertF0ForClearCoat15(half3 f0)
{
#if defined(SHADER_API_MOBILE)
    return ConvertF0ForAirInterfaceToF0ForClearCoat15Fast(f0);
#else
    return ConvertF0ForAirInterfaceToF0ForClearCoat15(f0);
#endif
}

inline void InitializeBRDFDataClearCoat(half clearCoatMask, half clearCoatSmoothness, inout BRDFData baseBRDFData, out BRDFData outBRDFData)
{
    // Calculate Roughness of Clear Coat layer
    outBRDFData.diffuse             = kDielectricSpec.aaa; // 1 - kDielectricSpec
    outBRDFData.specular            = kDielectricSpec.rgb;
    outBRDFData.reflectivity        = kDielectricSpec.r;

    outBRDFData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(clearCoatSmoothness);
    outBRDFData.roughness           = max(PerceptualRoughnessToRoughness(outBRDFData.perceptualRoughness), HALF_MIN_SQRT);
    outBRDFData.roughness2          = max(outBRDFData.roughness * outBRDFData.roughness, HALF_MIN);
    outBRDFData.normalizationTerm   = outBRDFData.roughness * 4.0h + 2.0h;
    outBRDFData.roughness2MinusOne  = outBRDFData.roughness2 - 1.0h;
    outBRDFData.grazingTerm         = saturate(clearCoatSmoothness + kDielectricSpec.x);

// Relatively small effect, cut it for lower quality
#if !defined(SHADER_API_MOBILE)
    // Modify Roughness of base layer using coat IOR
    half ieta                        = lerp(1.0h, CLEAR_COAT_IETA, clearCoatMask);
    half coatRoughnessScale          = Sq(ieta);
    half sigma                       = RoughnessToVariance(PerceptualRoughnessToRoughness(baseBRDFData.perceptualRoughness));

    baseBRDFData.perceptualRoughness = RoughnessToPerceptualRoughness(VarianceToRoughness(sigma * coatRoughnessScale));

    // Recompute base material for new roughness, previous computation should be eliminated by the compiler (as it's unused)
    baseBRDFData.roughness          = max(PerceptualRoughnessToRoughness(baseBRDFData.perceptualRoughness), HALF_MIN_SQRT);
    baseBRDFData.roughness2         = max(baseBRDFData.roughness * baseBRDFData.roughness, HALF_MIN);
    baseBRDFData.normalizationTerm  = baseBRDFData.roughness * 4.0h + 2.0h;
    baseBRDFData.roughness2MinusOne = baseBRDFData.roughness2 - 1.0h;
#endif

    // Darken/saturate base layer using coat to surface reflectance (vs. air to surface)
    baseBRDFData.specular = lerp(baseBRDFData.specular, ConvertF0ForClearCoat15(baseBRDFData.specular), clearCoatMask);
    // TODO: what about diffuse? at least in specular workflow diffuse should be recalculated as it directly depends on it.
}

// Computes the specular term for EnvironmentBRDF
half3 EnvironmentBRDFSpecular(BRDFData brdfData, half fresnelTerm)
{
    float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
    return surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm);
}

half3 EnvironmentBRDF(BRDFData brdfData, half3 indirectDiffuse, half3 indirectSpecular, half fresnelTerm)
{
    half3 c = indirectDiffuse * brdfData.diffuse;
    c += indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm);
    return c;
}

// Environment BRDF without diffuse for clear coat
half3 EnvironmentBRDFClearCoat(BRDFData brdfData, half clearCoatMask, half3 indirectSpecular, half fresnelTerm)
{
    float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
    return indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm) * clearCoatMask;
}

half3 DirectBRDFDiffcuse(BRDFData brdfData, half3 lightingColor, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS, half lightAttenuation)
{
    // return half3(0,0,0);
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    half3 diffColor = brdfData.diffuse * NdotL * lightAttenuation * lightingColor;
    //--SubSurface
    const float _SSSDistortion = 0.25;
    const float _SSSPower = 1.0;//PAV_CustomData2.y; //3.0;
    const float _SSSThick = 0.5;
    const half3 _SSSColor = half3(1.0,0.0,0.0);
    const float _SSSScale = 0.25;
    const half3 _SSSDiffBlurCol = brdfData.diffuse * half3(0.55,0.55,0.55);
    //forwardScatter base on https:/1/colinbarrebrisebois.com/2011/03/07/gdc-2011-approximating-translucency-for-a-fast-cheap-and-convincing-subsurface-scattering-look/
    float3 vLight = lightDirectionWS + normalWS * _SSSDistortion;
    float vdl = max(0.0, dot(viewDirectionWS, -lightDirectionWS));
    float3 forwardScatter = pow(vdl, _SSSPower) * (1.0 - _SSSThick) * _SSSColor; //Transmittance

    //backScatter base on http://blog.stevemcauley.com/2011/12/03/energy-conserving-wrapped-diffuse/
    float3 normal = lerp(normalWS*0.88, normalWS, _SSSScale);
    float ndl = dot(normal, lightDirectionWS);
    float3 one = float3(1.0, 1.0, 1.0);
    float3 onePlusW = one + _SSSColor;
    float3 ndlPlusW = float3(ndl, ndl, ndl) + _SSSColor;
    float warpPower = 2.0;
    float3 warpPowered = pow(saturate(ndlPlusW / onePlusW), float3(warpPower, warpPower, warpPower));
    float3 divider = 2.0 * onePlusW / (1.0 + warpPower);
    float3 warp = warpPowered / divider;

    float3 backScatter = saturate(warp);

    float3 scaterlighting = (forwardScatter + backScatter) * _SSSDiffBlurCol * INV_PI;
    diffColor = lerp(diffColor, scaterlighting , _SSSScale);
    return diffColor;
}

float SafePow(float v, float e)
{
    v = max(v, 1e-5);
    e = max(e, 1e-5);
    return pow(v, e);
}

float3 SafePow(float3 v, float3 e)
{
    v = max(v, float3(1e-5,1e-5,1e-5));
    e = max(e, float3(1e-5,1e-5,1e-5));
    return pow(v, e);
}

float V_Const()
{
    return 0.25;
}

float V_SmithJointApprox(float a, float ndv, float ndl)
{
    float lambdaV = ndl * (ndv * (1.0 - a) + a);
    float lambdaL = ndv * (ndl * (1.0 - a) + a);
    return 0.5 / (lambdaV + lambdaL + 1e-5);
}

float Rs(float n1, float n2, float cosI, float cosT) {
    return (n1 * cosI - n2 * cosT) / (n1 * cosI + n2 * cosT);
}

/* Amplitude reflection coefficient (p-polarized) */
float Rp(float n1, float n2, float cosI, float cosT) {
    return (n2 * cosI - n1 * cosT) / (n1 * cosT + n2 * cosI);
}

/* Amplitude transmission coefficient (s-polarized) */
float Ts(float n1, float n2, float cosI, float cosT) {
    return 2.0 * n1 * cosI / (n1 * cosI + n2 * cosT);
}

/* Amplitude transmission coefficient (p-polarized) */
float Tp(float n1, float n2, float cosI, float cosT) {
    return 2.0 * n1 * cosI / (n1 * cosT + n2 * cosI);
}

// cosI is the cosine of the incident angle, that is, cos0 = dot(view angle, normal)
// lambda is the wavelength of the incident light (e.g. lambda = 510 for green)
// From http://www.gamedev.net/page/resources/_/technical/graphics-programming-and-theory/thin-film-interference-for-computer-graphics-r2962
float ThickFilmReflectance(float cos0, float lambda, float thick, float n0, float n1, float n2) {
    // compute the phase change term (constant)
    float d10 = (n1 > n0) ? 0.0 : PI;
    float d12 = (n1 > n2) ? 0.0 : PI;
    float delta = d10 + d12;
    
    // now, compute cos1, the cosine of the reflected angle
    float sin1 = SafePow(n0 / n1, 2.0) * (1.0 - SafePow(cos0, 2.0));
    if (sin1 > 1.0) return 1.0; // total internal reflection
    float cos1 = sqrt(1.0 - sin1);
    
    // compute cos2, the cosine of the final transmitted angle, i.e. cos(theta_2)
    // we need this angle for the Fresnel terms at the bottom interface
    float sin2 = SafePow(n0 / n2, 2.0) * (1.0 - SafePow(cos0, 2.0));
    if (sin2 > 1.0) return 1.0; // total internal reflection
    float cos2 = sqrt(1.0 - sin2);
    
    // get the reflection transmission amplitude Fresnel coefficients
    float alpha_s = Rs(n1, n0, cos1, cos0) * Rs(n1, n2, cos1, cos2); // rho_10 * rho_12 (s-polarized)
    float alpha_p = Rp(n1, n0, cos1, cos0) * Rp(n1, n2, cos1, cos2); // rho_10 * rho_12 (p-polarized)
    
    float beta_s = Ts(n0, n1, cos0, cos1) * Ts(n1, n2, cos1, cos2); // tau_01 * tau_12 (s-polarized)
    float beta_p = Tp(n0, n1, cos0, cos1) * Tp(n1, n2, cos1, cos2); // tau_01 * tau_12 (p-polarized)
        
    // compute the phase term (phi)
    float phi = (2.0 * PI / lambda) * (2.0 * n1 * thick * cos1) + delta;
        
    // finally, evaluate the transmitted intensity for the two possible polarizations
    float ts = SafePow(beta_s, 2.0) / (SafePow(alpha_s, 2.0) - 2.0 * alpha_s * cos(phi) + 1.0);
    float tp = SafePow(beta_p, 2.0) / (SafePow(alpha_p, 2.0) - 2.0 * alpha_p * cos(phi) + 1.0);
    
    // we need to take into account conservation of energy for transmission
    float beamRatio = (n2 * cos2) / (n0 * cos0);
    
    // calculate the average transmitted intensity (if you know the polarization distribution of your
    // light source, you should specify it here. if you don't, a 50%/50% average is generally used)
    float t = beamRatio * (ts + tp) / 2.0;
    
    // and finally, derive the reflected intensity
    return 1.0 - t;
}

float3 F_ThinFlim(float ior, float thinFlimThick, float vdh)
{   
    float thickFlimNmedium = 1.0;
    float thickFlimNinternal = 1.25;
    float red = ThickFilmReflectance(vdh, 650.0,thinFlimThick, thickFlimNmedium, ior, thickFlimNinternal);
    float green = ThickFilmReflectance(vdh, 510.0, thinFlimThick, thickFlimNmedium, ior, thickFlimNinternal);
    float blue = ThickFilmReflectance(vdh, 475.0, thinFlimThick, thickFlimNmedium, ior, thickFlimNinternal);
    float3 reflColor = float3(red, green, blue);
    return reflColor;
}

// Computes the scalar specular term for Minimalist CookTorrance BRDF
// NOTE: needs to be multiplied with reflectance f0, i.e. specular color to complete
half3 DirectBRDFSpecular(BRDFData brdfData, half3 lightingColor, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS, half lightAttenuation)
{
    float3 halfDir = SafeNormalize(float3(lightDirectionWS) + float3(viewDirectionWS));
    float NoL = saturate(dot(normalWS, lightDirectionWS));
    float NoH = saturate(dot(normalWS, halfDir));
    float NoV = saturate(dot(normalWS, viewDirectionWS));
    float LoH = saturate(dot(lightDirectionWS, halfDir));
    float VoH = saturate(dot(viewDirectionWS, halfDir));
    //--GGX
    #if defined(ThinFlim) && !defined(CoatThinFlim)
        const float u_IOR = 1.5;
        const float thinFlimThick = 0.5;
        float3 F =  F_ThinFlim(u_IOR, thinFlimThick, VoH); 
    #else
        float3 f0 = brdfData.specular;
        float3 F =  F_Schlick(f0, VoH);
    #endif
    float a = brdfData.roughness;
    float a2 = brdfData.roughness2;
    float V = V_SmithJointApprox(a, NoV, NoL);
    float D = D_GGX(NoH, a2);
    half3 specular = D * V * F * NoL * lightingColor * lightAttenuation;
    return specular;
}

///////////////////////////////////////////////////////////////////////////////
//                      Global Illumination                                  //
///////////////////////////////////////////////////////////////////////////////

// Ambient occlusion
TEXTURE2D_X(_ScreenSpaceOcclusionTexture);
SAMPLER(sampler_ScreenSpaceOcclusionTexture);

struct AmbientOcclusionFactor
{
    half indirectAmbientOcclusion;
    half directAmbientOcclusion;
};

half SampleAmbientOcclusion(float2 normalizedScreenSpaceUV)
{
    float2 uv = UnityStereoTransformScreenSpaceTex(normalizedScreenSpaceUV);
    return SAMPLE_TEXTURE2D_X(_ScreenSpaceOcclusionTexture, sampler_ScreenSpaceOcclusionTexture, uv).x;
}

AmbientOcclusionFactor GetScreenSpaceAmbientOcclusion(float2 normalizedScreenSpaceUV)
{
    AmbientOcclusionFactor aoFactor;
    aoFactor.indirectAmbientOcclusion = SampleAmbientOcclusion(normalizedScreenSpaceUV);
    aoFactor.directAmbientOcclusion = lerp(1.0, aoFactor.indirectAmbientOcclusion, _AmbientOcclusionParam.w);
    return aoFactor;
}

// Samples SH L0, L1 and L2 terms
half3 SampleSH(half3 normalWS)
{
    // LPPV is not supported in Ligthweight Pipeline
    real4 SHCoefficients[7];
    SHCoefficients[0] = unity_SHAr;
    SHCoefficients[1] = unity_SHAg;
    SHCoefficients[2] = unity_SHAb;
    SHCoefficients[3] = unity_SHBr;
    SHCoefficients[4] = unity_SHBg;
    SHCoefficients[5] = unity_SHBb;
    SHCoefficients[6] = unity_SHC;

    return max(half3(0, 0, 0), SampleSH9(SHCoefficients, normalWS));
}

// SH Vertex Evaluation. Depending on target SH sampling might be
// done completely per vertex or mixed with L2 term per vertex and L0, L1
// per pixel. See SampleSHPixel
half3 SampleSHVertex(half3 normalWS)
{
#if defined(EVALUATE_SH_VERTEX)
    return SampleSH(normalWS);
#elif defined(EVALUATE_SH_MIXED)
    // no max since this is only L2 contribution
    return SHEvalLinearL2(normalWS, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);
#endif

    // Fully per-pixel. Nothing to compute.
    return half3(0.0, 0.0, 0.0);
}

// SH Pixel Evaluation. Depending on target SH sampling might be done
// mixed or fully in pixel. See SampleSHVertex
half3 SampleSHPixel(half3 L2Term, half3 normalWS)
{
#if defined(EVALUATE_SH_VERTEX)
    return L2Term;
#elif defined(EVALUATE_SH_MIXED)
    half3 L0L1Term = SHEvalLinearL0L1(normalWS, unity_SHAr, unity_SHAg, unity_SHAb);
    half3 res = L2Term + L0L1Term;
#ifdef UNITY_COLORSPACE_GAMMA
    res = LinearToSRGB(res);
#endif
    return max(half3(0, 0, 0), res);
#endif

    // Default: Evaluate SH fully per-pixel
    return SampleSH(normalWS);
}

#if defined(UNITY_DOTS_INSTANCING_ENABLED)
#define LIGHTMAP_NAME unity_Lightmaps
#define LIGHTMAP_INDIRECTION_NAME unity_LightmapsInd
#define LIGHTMAP_SAMPLER_NAME samplerunity_Lightmaps
#define LIGHTMAP_SAMPLE_EXTRA_ARGS lightmapUV, unity_LightmapIndex.x
#else
#define LIGHTMAP_NAME unity_Lightmap
#define LIGHTMAP_INDIRECTION_NAME unity_LightmapInd
#define LIGHTMAP_SAMPLER_NAME samplerunity_Lightmap
#define LIGHTMAP_SAMPLE_EXTRA_ARGS lightmapUV
#endif

// Sample baked lightmap. Non-Direction and Directional if available.
// Realtime GI is not supported.
half3 SampleLightmap(float2 lightmapUV, half3 normalWS)
{
#ifdef UNITY_LIGHTMAP_FULL_HDR
    bool encodedLightmap = false;
#else
    bool encodedLightmap = true;
#endif

    half4 decodeInstructions = half4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h);

    // The shader library sample lightmap functions transform the lightmap uv coords to apply bias and scale.
    // However, universal pipeline already transformed those coords in vertex. We pass half4(1, 1, 0, 0) and
    // the compiler will optimize the transform away.
    half4 transformCoords = half4(1, 1, 0, 0);

#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
    return SampleDirectionalLightmap(TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_NAME, LIGHTMAP_SAMPLER_NAME),
        TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_INDIRECTION_NAME, LIGHTMAP_SAMPLER_NAME),
        LIGHTMAP_SAMPLE_EXTRA_ARGS, transformCoords, normalWS, encodedLightmap, decodeInstructions);
#elif defined(LIGHTMAP_ON)
    return SampleSingleLightmap(TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_NAME, LIGHTMAP_SAMPLER_NAME), LIGHTMAP_SAMPLE_EXTRA_ARGS, transformCoords, encodedLightmap, decodeInstructions);
#else
    return half3(0.0, 0.0, 0.0);
#endif
}

// We either sample GI from baked lightmap or from probes.
// If lightmap: sampleData.xy = lightmapUV
// If probe: sampleData.xyz = L2 SH terms
#if defined(LIGHTMAP_ON)
#define SAMPLE_GI(lmName, shName, normalWSName) SampleLightmap(lmName, normalWSName)
#else
#define SAMPLE_GI(lmName, shName, normalWSName) SampleSHPixel(shName, normalWSName)
#endif

half3 GlossyEnvironmentReflection(half3 reflectVector, half perceptualRoughness, half occlusion)
{
#if !defined(_ENVIRONMENTREFLECTIONS_OFF)
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip);

#if defined(UNITY_USE_NATIVE_HDR)
    half3 irradiance = encodedIrradiance.rgb;
#else
    half3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
#endif

    return irradiance * occlusion;
#endif // GLOSSY_REFLECTIONS

    return _GlossyEnvironmentColor.rgb * occlusion;
}

half3 SubtractDirectMainLightFromLightmap(Light mainLight, half3 normalWS, half3 bakedGI)
{
    // Let's try to make realtime shadows work on a surface, which already contains
    // baked lighting and shadowing from the main sun light.
    // Summary:
    // 1) Calculate possible value in the shadow by subtracting estimated light contribution from the places occluded by realtime shadow:
    //      a) preserves other baked lights and light bounces
    //      b) eliminates shadows on the geometry facing away from the light
    // 2) Clamp against user defined ShadowColor.
    // 3) Pick original lightmap value, if it is the darkest one.


    // 1) Gives good estimate of illumination as if light would've been shadowed during the bake.
    // We only subtract the main direction light. This is accounted in the contribution term below.
    half shadowStrength = GetMainLightShadowStrength();
    half contributionTerm = saturate(dot(mainLight.direction, normalWS));
    half3 lambert = mainLight.color * contributionTerm;
    half3 estimatedLightContributionMaskedByInverseOfShadow = lambert * (1.0 - mainLight.shadowAttenuation);
    half3 subtractedLightmap = bakedGI - estimatedLightContributionMaskedByInverseOfShadow;

    // 2) Allows user to define overall ambient of the scene and control situation when realtime shadow becomes too dark.
    half3 realtimeShadow = max(subtractedLightmap, _SubtractiveShadowColor.xyz);
    realtimeShadow = lerp(bakedGI, realtimeShadow, shadowStrength);

    // 3) Pick darkest color
    return min(bakedGI, realtimeShadow);
}

// -----------------------Uber Indirect Lighting -----------------------
   
    // Jimenez et al. 2016, "Practical Realtime Strategies for Accurate Indirect Occlusion"
    /**
     * Returns a color ambient occlusion based on a pre-computed visibility term.
     * The albedo term is meant to be the diffuse color or f0 for the diffuse and
     * specular terms respectively.
     */
half3 GTAO_MultiBounce(half visibility, half3 albedo) {
    //#if defined(HIGH) || defined(ULTRA)
        half3 a =  2.0404 * albedo - 0.3324;
        half3 b = -4.7951 * albedo + 0.6417;
        half3 c =  2.7552 * albedo + 0.6903;
        return max(half3(visibility,visibility,visibility), ((visibility * a + b) * visibility + c) * visibility);
    //#else
        //return vec3(visibility);
    //#endif
}

half SpecularAO(half occlusion,half perceptualRoughness,half3 normalWS,half3 viewDirectionWS,half NoV,half3 reflectVectorWS)
{
    // #if defined(LOW) || defined(MEDIUM)
    //     return S.occParams.x;
    // #else
    //     #if defined(BentNormal)
    //         // Jimenez et al. 2016, "Practical Realtime Strategies for Accurate Indirect Occlusion"
    //         // aperture from ambient occlusion
    //         float cosAv = sqrt(1.0 - S.occParams.x);
    //         // aperture from roughness, log(10) / log(2) = 3.321928
    //         float cosAs = exp2(-3.321928 * S.roughParams.z);
    //         // angle betwen bent normal and reflection direction
    //         float cosB  = dot(S.bnDir, S.rDir);

    //         // Remove the 2 * PI term from the denominator, it cancels out the same term from
    //         // sphericalCapsIntersection()
    //         float ao = sphericalCapsIntersection(cosAv, cosAs, cosB) / (1.0 - cosAs);
    //         // Smoothly kill specular AO when entering the perceptual roughness range [0.1..0.8] for metals
    //         // Without this, specular AO can remove all reflections, which looks bad on metals
    //         return mix(1.0, ao, smoothstep(0.01, 0.09, S.roughParams.y));
    //     #else
            half visibility = occlusion;
            half roughness = perceptualRoughness;
            // Lagarde and de Rousiers 2014, "Moving Frostbite to PBR"
            half lagardeAO = saturate(SafePow(NoV + visibility, exp2(-16.0 * roughness - 1.0)) - 1.0 + visibility);
            // horizon occlusion with falloff, should be computed for direct specular too
            half horizon = min(1.0 + dot(reflectVectorWS, normalWS), 1.0);
            half horizonAO = horizon * horizon;

            return lagardeAO * horizonAO;
    //     #endif
    // #endif
}
//it is similiar to fresnel but consider roughness
half3 EnvBRDFApprox(half3 F0, half perceptualRoughness, half ndv)
{
    // [ Lazarov 2013, "Getting More Physical in Call of Duty: Black Ops II" ]
    // Adaptation to fit our G term.
    const half4 c0 = half4(-1, -0.0275, -0.572, 0.022);
    const half4 c1 = half4(1, 0.0425, 1.04, -0.04);
    half4 r = perceptualRoughness * c0 + c1;
    half a004 = min(r.x * r.x, exp2(-9.28 * ndv)) * r.x + r.y;
    half2 AB = half2(-1.04, 1.04) * a004 + r.zw;

    // Anything less than 2% is physically impossible and is instead considered to be shadowing
    // Note: this is needed for the 'specular' show flag to work, since it uses a SpecularColor of 0
    AB.y *= saturate(50.0 * F0.g); //when F0 is 0.04, it is 2, saturate to 1

    return F0 * AB.x + AB.y; //return F0 * AB.x + F90 * AB.y;
}

//https://www.unrealengine.com/zh-CN/blog/physically-based-shading-on-mobile
half3 EnvBRDF(BRDFData brdfData,half NoV )
{
    // #if defined(ThinFlim) && !defined(CoatThinFlim)
    //     return F_ThinFlim(u_ThinFlimIOR, S, S.ndv); 
    // #else
    //     #ifdef ULTRA
            //use 1.0 as F0 at Energy Compensation, reference: MaterialX on github
            half3 Ess = EnvBRDFApprox(half3(1.0.xxx), brdfData.perceptualRoughness, NoV);
            half3 energyCompensation = 1.0 + brdfData.specular * (1.0 - Ess) / Ess;
            return EnvBRDFApprox(brdfData.specular, brdfData.perceptualRoughness, NoV) * energyCompensation;
    //     #endif
    // #endif
    // return EnvBRDFApprox(S.specCol, S.roughParams.x, S.ndv);
}
half3 GlobalIllumination(BRDFData brdfData, BRDFData brdfDataClearCoat, half clearCoatMask,
    half3 bakedGI, half occlusion,
    half3 normalWS, half3 viewDirectionWS)
{
   // #ifdef UBER_SCATER
        //half3 indirectDiffuse = brdfData.diffuse * bakedGI * occlusion;
        //TODO
    half3 indirectDiffuse = bakedGI * occlusion;
    // #else
    //     half3 multiBounceColor = GTAO_MultiBounce(occlusion, brdfData.diffuse);
    //     half3 indirectDiffuse = bakedGI * multiBounceColor;
    // #endif
    // if omit specular, just return diffuse part.
#ifndef PAV_HAS_SPECULAR    
    return indirectDiffuse;
#endif

    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    //Uber occlusion
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, occlusion);
    // #ifdef _OCCLUSIONMAP
    //     half occlusSpecular = SpecularAO(occlusion,brdfData.perceptualRoughness,normalWS,viewDirectionWS,NoV,reflectVector);
    // #else
    //     half occlusSpecular = 1.0;
    // #endif
    // half3 multiBounceColor_Specular = GTAO_MultiBounce(occlusSpecular, brdfData.specular);
    // indirectSpecular *= multiBounceColor_Specular;
    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
    //half3 color = indirectDiffuse * brdfData.diffuse + EnvBRDF(brdfData,NoV) * indirectSpecular;


#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half3 coatIndirectSpecular = GlossyEnvironmentReflection(reflectVector, brdfDataClearCoat.perceptualRoughness, occlusion);
    // TODO: "grazing term" causes problems on full roughness
    half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, coatIndirectSpecular, fresnelTerm);

    // Blend with base layer using khronos glTF recommended way using NoV
    // Smooth surface & "ambiguous" lighting
    // NOTE: fresnelTerm (above) is pow4 instead of pow5, but should be ok as blend weight.
    half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
    return color * (1.0 - coatFresnel * clearCoatMask) + coatColor;
#else
    return color;
#endif
}

// Backwards compatiblity
half3 GlobalIllumination(BRDFData brdfData, half3 bakedGI, half occlusion, half3 normalWS, half3 viewDirectionWS)
{
    const BRDFData noClearCoat = (BRDFData)0;
    return GlobalIllumination(brdfData, noClearCoat, 0.0, bakedGI, occlusion, normalWS, viewDirectionWS);
}

void MixRealtimeAndBakedGI(inout Light light, half3 normalWS, inout half3 bakedGI)
{
#if defined(LIGHTMAP_ON) && defined(_MIXED_LIGHTING_SUBTRACTIVE)
    bakedGI = SubtractDirectMainLightFromLightmap(light, normalWS, bakedGI);
#endif
}

// Backwards compatiblity
void MixRealtimeAndBakedGI(inout Light light, half3 normalWS, inout half3 bakedGI, half4 shadowMask)
{
    MixRealtimeAndBakedGI(light, normalWS, bakedGI);
}

///////////////////////////////////////////////////////////////////////////////
//                      Lighting Functions                                   //
///////////////////////////////////////////////////////////////////////////////
half3 LightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat,
    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
    half3 normalWS, half3 viewDirectionWS,
    half clearCoatMask, bool specularHighlightsOff)
{
    half3 LPBR_diff = DirectBRDFDiffcuse(brdfData, lightColor, normalWS, lightDirectionWS, viewDirectionWS, lightAttenuation);
    half3 LPBR_spec = DirectBRDFSpecular(brdfData, lightColor, normalWS, lightDirectionWS, viewDirectionWS, lightAttenuation);
    return LPBR_diff + LPBR_spec;
}

half3 LightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff)
{
    return LightingPhysicallyBased(brdfData, brdfDataClearCoat, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff);
}

// Backwards compatibility
half3 LightingPhysicallyBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS)
{
    #ifdef _SPECULARHIGHLIGHTS_OFF
    bool specularHighlightsOff = true;
#else
    bool specularHighlightsOff = false;
#endif
    const BRDFData noClearCoat = (BRDFData)0;
    return LightingPhysicallyBased(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff);
}

half3 LightingPhysicallyBased(BRDFData brdfData, half3 lightColor, half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half3 viewDirectionWS)
{
    Light light;
    light.color = lightColor;
    light.direction = lightDirectionWS;
    light.distanceAttenuation = lightAttenuation;
    light.shadowAttenuation   = 1;
    return LightingPhysicallyBased(brdfData, light, normalWS, viewDirectionWS);
}

half3 LightingPhysicallyBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff)
{
    const BRDFData noClearCoat = (BRDFData)0;
    return LightingPhysicallyBased(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff);
}

half3 LightingPhysicallyBased(BRDFData brdfData, half3 lightColor, half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff)
{
    Light light;
    light.color = lightColor;
    light.direction = lightDirectionWS;
    light.distanceAttenuation = lightAttenuation;
    light.shadowAttenuation   = 1;
    return LightingPhysicallyBased(brdfData, light, viewDirectionWS, specularHighlightsOff, specularHighlightsOff);
}

half3 VertexLighting(float3 positionWS, half3 normalWS)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < lightsCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, positionWS);
        half3 lightColor = light.color * light.distanceAttenuation;
        vertexLightColor += LightingLambert(lightColor, light.direction, normalWS);
    }
#endif

    return vertexLightColor;
}

///////////////////////////////////////////////////////////////////////////////
//                      Fragment Functions                                   //
//       Used by ShaderGraph and others builtin renderers                    //
///////////////////////////////////////////////////////////////////////////////
half4 UniversalFragmentPBR(InputData inputData, SurfaceData surfaceData)
{
#ifdef PAV_HAS_ADDITIVE_GI
    // add extra gi for some dark scene.
    inputData.bakedGI += _AdditiveGI;
#endif

#ifdef PAV_LIT_ONLY_GI_DIFFUSE
    // only direct diffuse part.
    half3 color = surfaceData.albedo * inputData.bakedGI * surfaceData.occlusion;

#elif defined(PAV_LIT_TOON)
    
    BRDFData brdfData;
    
    // NOTE: can modify alpha
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
        half4 shadowMask = inputData.shadowMask;
    #elif !defined (LIGHTMAP_ON)
        half4 shadowMask = unity_ProbesOcclusion;
    #else
        half4 shadowMask = half4(1, 1, 1, 1);
    #endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

    half NdotL = saturate(dot(inputData.normalWS, mainLight.direction));
    half toonShadow = (NdotL + inputData.bakedGI) * surfaceData.occlusion;

    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        toonShadow *= aoFactor.directAmbientOcclusion;
        surfaceData.occlusion = min(surfaceData.occlusion, aoFactor.indirectAmbientOcclusion);
    #endif

    //.. if in shadow side, use shadoe albedo..
    #ifdef PAV_ToonShadowMap
    if(toonShadow < 0.3){ // cut off color.
        surfaceData.albedo = surfaceData.toonShadow;
    }
    #endif

    half lightAttenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    half3 radiance = mainLight.color.rgb * (lightAttenuation * NdotL);

    half3 color = surfaceData.albedo.rgb * (inputData.bakedGI + radiance) * surfaceData.occlusion;

    // spec..
    #ifndef _SPECULARHIGHLIGHTS_OFF
    if(toonShadow > 0.5)
    {
        float3 halfVec = SafeNormalize(float3(mainLight.direction) + float3(inputData.viewDirectionWS));
        half NdotH = saturate(dot(inputData.normalWS, halfVec));
        if(pow(NdotH, 10) > 0.6)
        {
            color += surfaceData.albedo.rgb * surfaceData.specular * lightAttenuation;// * surfaceData.albedo.rgb * 0.5;
        }
    }
    #endif
 
#else

    #ifdef _SPECULARHIGHLIGHTS_OFF
        bool specularHighlightsOff = true;
    #else
        bool specularHighlightsOff = false;
    #endif
    
        BRDFData brdfData;
    
        // NOTE: can modify alpha
        InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);
    
        BRDFData brdfDataClearCoat = (BRDFData)0;
    
    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // base brdfData is modified here, rely on the compiler to eliminate dead computation by InitializeBRDFData()
        InitializeBRDFDataClearCoat(surfaceData.clearCoatMask, surfaceData.clearCoatSmoothness, brdfData, brdfDataClearCoat);
    #endif
    
        // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
        half4 shadowMask = inputData.shadowMask;
    #elif !defined (LIGHTMAP_ON)
        half4 shadowMask = unity_ProbesOcclusion;
    #else
        half4 shadowMask = half4(1, 1, 1, 1);
    #endif
    
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    
    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        #ifdef PAV_HAS_MAINLIGHT
            mainLight.color *= aoFactor.directAmbientOcclusion;
        #endif
        surfaceData.occlusion = min(surfaceData.occlusion, aoFactor.indirectAmbientOcclusion);
    #endif
    
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    half3 color = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                         inputData.bakedGI, surfaceData.occlusion,
                                         inputData.normalWS, inputData.viewDirectionWS);
    
    color += LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                                         mainLight,
                                         inputData.normalWS, inputData.viewDirectionWS,
                                         surfaceData.clearCoatMask, specularHighlightsOff);
    
    #ifdef _ADDITIONAL_LIGHTS
        uint pixelLightCount = GetAdditionalLightsCount();
        for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
        {
            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
            #if defined(_SCREEN_SPACE_OCCLUSION)
                light.color *= aoFactor.directAmbientOcclusion;
            #endif
            color += LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                                             light,
                                             inputData.normalWS, inputData.viewDirectionWS,
                                             surfaceData.clearCoatMask, specularHighlightsOff);
        }
    #endif
    
    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        color += inputData.vertexLighting * brdfData.diffuse;
    #endif

#endif//Pav_LIT_ONLY_GI_DIFFUSE


    #ifdef PAV_HAS_EMISSION
        color += surfaceData.emission;
    #endif

    return half4(color, surfaceData.alpha);
}

half4 UniversalFragmentPBR(InputData inputData, half3 albedo, half metallic, half3 specular,
    half smoothness, half occlusion, half3 emission, half alpha)
{
    SurfaceData s;
    s.albedo              = albedo;
    s.metallic            = metallic;
    s.specular            = specular;
    s.smoothness          = smoothness;
    s.occlusion           = occlusion;
    s.emission            = emission;
    s.alpha               = alpha;
    s.clearCoatMask       = 0.0;
    s.clearCoatSmoothness = 1.0;
    return UniversalFragmentPBR(inputData, s);
}

//LWRP -> Universal Backwards Compatibility
half4 LightweightFragmentPBR(InputData inputData, half3 albedo, half metallic, half3 specular,
    half smoothness, half occlusion, half3 emission, half alpha)
{
    return UniversalFragmentPBR(inputData, albedo, metallic, specular, smoothness, occlusion, emission, alpha);
}
#endif
