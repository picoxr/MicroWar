#include"Dependency/Core/ShaderLibrary/Common.hlsl"
#include"Lighting.hlsl"
#include"ToonyUtils.hlsl"

#define SS_PROFILE_SAMPLE_COUNT 6
//Dependency input
TEXTURE2D(_SSLUT);          SAMPLER(sampler_SSLUT);

half3 ComputeSkinGI(BSDFData bsdfData,
    half3 bakedGI,
    half occulsion,
    half3 occulsionTint,
    half subsurfaceMask,
    half smoothness,
    half3 normalWS,
    half3 viewDirectionWS)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));

    half3 colorBleedAO = ColorBleedAO(occulsion, occulsionTint * occulsion);
    colorBleedAO = lerp(occulsion, colorBleedAO, subsurfaceMask);
    
    half3 indirectDiffuse = bakedGI * colorBleedAO;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, bsdfData.perceptualRoughness, occulsion);
    
    indirectDiffuse *= bsdfData.diffuseColor;
    indirectSpecular *= EnvironmentBRDF(smoothness,NoV,bsdfData.fresnel0);
    
    half3 color = indirectDiffuse + indirectSpecular;
    return color;
}

half3 SubsurfaceScattering(half3x3 tangentSpaceTransform,
    half3 lightDir,
    half3 normalWS,
    half3 SSSFalloff,
    TEXTURE2D_PARAM(normalMap,sampler_normalMap),
    half shadowAttenuation,
    half2 uv)
{

    half3 weights = 0.0;
    half scattering = 0.0;
    half NdotL = 0.0;
    half brdfLookup = 0.0;
    half directDiffuse = 0.0;
    half3 brdf = 0.0;
    half3 worldNormal = normalWS;

    // Ref: HDRP's DiffusionProfileSettings.cs #105
    // We importance sample the color channel with the widest scattering distance.
    half radius = max(max(SSSFalloff.x, SSSFalloff.y), SSSFalloff.z); 
    
    /////////////////////////////////////////////////////////////////////
    //	                        Skin Profile                           //
    /////////////////////////////////////////////////////////////////////

    half3 c = min(1.0, SSSFalloff.xyz);

    // Modified using Color Tint with weight from the highest color value of the human skin profile
    half3 profileWeights[6] = {
    (1 - c) * 0.649,
    (1 - c) * 0.366,
    c * 0.198,
    c * 0.113,
    c * 0.358,
    c * 0.078 };

    const half profileVariance[6] = {
    0.0064,
    0.0484,
    0.187,
    0.567,
    1.99,
    7.41 };

    const half profileVarianceSqrt[6] = {
    0.08,	    // sqrt(0.0064)
    0.219,	    // sqrt(0.0484)
    0.432,	    // sqrt(0.187)
    0.753,	    // sqrt(0.567)
    1.410,	    // sqrt(1.99)
    2.722 };	// sqrt(7.41)
    
    // mip count can be calculate in the shader editor and caches it in material property?
    int mipCount = GetMipCount(TEXTURE2D_ARGS(normalMap, sampler_normalMap));
    
    // approximation mip level
    half blur = radius * PI * mipCount;
    // add simple penumbra offset for soft shadow lookup 
    half shadow = min(1.0, 0.25 + shadowAttenuation);
    
    half r = rcp(radius); // 1 / r
    half s = -r * r;

    /////////////////////////////////////////////////////////////////////
    //	              Six Layer Subsurface Scattering                  //
    /////////////////////////////////////////////////////////////////////
    [unroll]
    for (int i = 0; i < SS_PROFILE_SAMPLE_COUNT; i++)
    {
        weights = profileWeights[i];
        scattering = exp(s / profileVarianceSqrt[i]);

    #ifdef _NORMALMAP
        // blur normal map via mip
        worldNormal = UnpackNormalRG(SAMPLE_TEXTURE2D_LOD(normalMap,sampler_normalMap,uv,lerp(0.0, blur, profileVariance[i])));
        worldNormal = TransformTangentToWorld(worldNormal, tangentSpaceTransform);
    #endif

        // Direct Diffuse Lookup
        NdotL = dot(worldNormal, lightDir);
        brdfLookup = mad(NdotL, 0.5, 0.5);

        directDiffuse = SAMPLE_TEXTURE2D(_SSLUT,sampler_SSLUT, half2(brdfLookup * shadow, scattering)).r;

        brdf += weights * directDiffuse;
    }
    return brdf;
}

// Reference based on <<Real-Time Realistic Skin Translucency>> paper,
// See http://www.iryoku.com/translucency/
// Also Next-Generation-Character-Rendering-v6.ppt #182
half3 Transmittance(float thickness,
    half shadowAtten,
    float NdotL,
    half3 transmissionColor,
    half3 scatteringColor)
{
    half s = exp(-thickness * thickness);

    // Simplified version of Profile
    half3 translucencyProfile = s * (transmissionColor * scatteringColor);

    half irradiance = max(0.0, 0.3 - NdotL);

    // Allow some light bleeding through the shadow 
    half shadow = saturate(min(0.3, s) + shadowAtten);
    return  translucencyProfile * (irradiance * shadow);
}

void CharacterLighting(half3 viewDir,
    half3 normalWS,
    half NoV,
    half clampedNoV,
    BSDFData bsdfData,
    Light light,
    out RenderData renderData)
{
    half NoL = dot(normalWS, light.direction);
    half3 shadowRemap = lerp(_LightShadow * _ShadowColor, 1, light.shadowAttenuation);
    half remapValue = NoL;
    
    half clampedNoL = saturate(NoL);
    
    half LoV, NoH, LoH, invLenLV;
    GetBSDFAngle(viewDir,light.direction,NoL,NoV,LoV,NoH,LoH,invLenLV);
    
    half3 cosWeight = 0;
    
    cosWeight = clampedNoL.xxx;
    
    half remap = smoothstep(-_ShadowRange, _ShadowSmoothing -_ShadowRange, remapValue);
    half3 halfLambert = NoL * 0.5 + 0.5;
    half3 remapColor = lerp(_ShadowColor.rgb, half3(1,1,1),remap) * halfLambert;
    
    //remap = Remap_Half(remap, half2(0,1), half2(_LightShadow,1));
    cosWeight = remapColor;
    
    //----Compute BRDF----
    half3 radiance = cosWeight  * light.color * light.distanceAttenuation * shadowRemap;
    
    //Diffuse Term, we multi PI as engine itself auto times PI when baking lighting.
    half diffuseTerm = Lambert() * PI;

    //Specular Term
    half3 specularTerm;
    
#ifndef _SPECULARHIGHLIGHTS_OFF
    half3 F = F_Schlick(bsdfData.fresnel0,LoH);
    
    #ifdef _DUAL_SPECULAR
        half DV1 = DV_SmithJointGGX(NoH,abs(NoL),clampedNoV, bsdfData.roughness,bsdfData.partLambdaV);
        half DV2 = DV_SmithJointGGX(NoH,abs(NoL),clampedNoV, bsdfData.roughnessSec,bsdfData.partLambdaVSec);
        half3 specularTerm1 = DV1 * F * _SpecularLobe1;
        half3 specularTerm2 = DV2 * F * _SpecularLobe2;
        specularTerm = lerp(specularTerm1,specularTerm2,_LobeMix);
    #else
        half DV = DV_SmithJointGGX(NoH,abs(NoL),clampedNoV,bsdfData.roughness,bsdfData.partLambdaV);
        specularTerm = DV * F;
    #endif
#else
    specularTerm = 0;
#endif
    
    renderData.diffuse = diffuseTerm * bsdfData.diffuseColor * radiance;
    renderData.specular = specularTerm * radiance;
    renderData.debug = 0;
    
}

//Pico xr lighting
half4 StylizedCharacterLighting(AdaptiveInputData inputData,AdaptiveSurfaceData surfaceData)
{
    
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inputData.shadowMask;
    #elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
    #else
    half4 shadowMask = half4(1, 1, 1, 1);
    #endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

    half3 N = surfaceData.normalWS;
    half3 V = inputData.viewDirectionWS;
    half NoV = dot(N,V);
    half clampedNoV = max(NoV,0.0001);
    //half NoL = dot(N,mainLight.direction);
    
    BSDFData bsdfData;
    InitializeBSDFData(surfaceData ,clampedNoV,V,bsdfData);
    
    half3 color = ComputeSkinGI(bsdfData,inputData.bakedGI,surfaceData.occlusion,surfaceData.occlusionTint,1.0h,surfaceData.smoothness,N,V);
    
#if _DEBUG_DIRECTONLY
    color = 0;
#elif _DEBUG_INDIRECTONLY
    return half4(color,1.0);
#endif

    RenderData renderData;
    CharacterLighting(V,N,NoV,clampedNoV,bsdfData,mainLight,renderData);

#ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
        RenderData addRenderData;
        DirectBRDFRendering(V,N,NoV,clampedNoV,bsdfData,light,addRenderData);
        renderData.diffuse += addRenderData.diffuse;
        renderData.specular += addRenderData.specular;
    }
#endif
    color += renderData.diffuse + renderData.specular * PI + surfaceData.emission;

#if defined(_RIM_LIGHT)
    half NdotV = dot(inputData.rimNormalWS, inputData.viewDirectionWS);
    half3 rimlighting = RimLighting(NdotV, _RimColor, _RimPower, _RimMin, _RimMax);

#if defined(_RIMMASK_LIGHT)
    half rimMask = dot(inputData.rimNormalWS, mainLight.direction);
    rimMask = smoothstep(_RimLengthThreshold, _RimLengthThreshold +  _RimLengthSmoothing,rimMask);
#elif defined(_RIMMASK_VIEW)
    half s,c;
    sincos(_RimRotation * TWO_PI, s, c);
    half2 constVector = half2(s,c);
    half rimMask = dot(inputData.rimNormalVS.xy, constVector);
    rimMask = smoothstep(_RimLengthThreshold, _RimLengthThreshold +  _RimLengthSmoothing,rimMask);
#else
    half rimMask = 1.0h;
#endif
    
    rimlighting *= rimMask * _RimColor.rgb;
    color += rimlighting;
#endif

    
#ifdef _DEBUG_METALLIC
        return half4(surfaceData.metallic.rrr,1);
#elif _DEBUG_ROUGHNESS
        return half4(surfaceData.smoothness.rrr,1);
#elif _DEBUG_NORMAL
        return half4(surfaceData.normalWS,1.0);
#endif

    return DoPostProcessing(color, _Tonemapping,surfaceData.alpha);
}


