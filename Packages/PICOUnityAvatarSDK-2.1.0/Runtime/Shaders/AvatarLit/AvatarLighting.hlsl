#include "Dependency/URP/ShaderLibrary/Lighting.hlsl"

struct BSDFData
{
    half3 diffuseColor;
    half3 fresnel0;
    half perceptualRoughness; 
    half roughness;
    half partLambdaV;
};

//@NoV: clamped NoV
void InitializeBSDFData(half3 albedo,half metallic,half smoothness,half NoV,half3 V,
out BSDFData outBSDFData)
{
    outBSDFData.diffuseColor = albedo * (1.0 - metallic);
    half perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
    outBSDFData.perceptualRoughness = perceptualRoughness;
    outBSDFData.fresnel0 = lerp(DEFAULT_SPECULAR_VALUE, albedo, metallic);
    half  roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    outBSDFData.roughness = ClampRoughnessForAnalyticalLights(roughness);
    outBSDFData.partLambdaV = GetSmithJointGGXPartLambdaV(NoV,roughness);
}

//From https://blog.selfshadow.com/publications/s2013-shading-course/lazarov/s2013_pbs_black_ops_2_notes.pdf
//这个看起来会好一点
half3 EnvironmentBRDF(half g,half NoV,half3 fresnel)
{
    half4 t =  half4(1 / 0.96, 0.475, (0.0275 - 0.25 * 0.04) / 0.96, 0.25);
    t *= float4(g, g, g, g);
    t += float4(0, 0, (0.015 - 0.75 * 0.04) / 0.96, 0.75);
    float a0 = t.x * min(t.y, exp2(-9.28 * NoV)) + t.z;
    float a1 = t.w;
    return saturate(a0 + fresnel * (a1 - a0));
}


#if defined(_MATCAP)
half3 MatCapReflection(half3 normalWS, half perceptualRoughness, float3 positionVS)
{
    
    half3 normalVS = mul((float3x3)GetWorldToViewMatrix(), normalWS);
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    #if defined(REQUIRES_VIEW_SPACE_POSITION_INTERPOLATOR)
    float3 vTangent = normalize(cross(positionVS, float3(0,1,0)));
    float3 vBinormal = normalize(cross(-positionVS, vTangent));
    float2 matCapUV = float2(dot(vTangent, normalVS), dot(vBinormal, normalVS)) * 0.5  + 0.5;
    #else
    half2 matCapUV = normalVS.xy * 0.5 + 0.5;
    #endif
   
    #if defined(_ENABLE_STATIC_MESH_BATCHING)
    half3 mapCapTex = SampleTexArrayLod(4, mtlIndex, float3(matCapUV, mip), _MatCapMapArray, sampler_MatCapMapArray, half4(0.0f, 0.0f, 0.0f, 0.0f));
    #else
    half3 matCapTex = SAMPLE_TEXTURE2D_LOD(_MatCapTex, sampler_MatCapTex, matCapUV,mip).rgb;
    #endif
    return matCapTex * _MatCapStrength;
    
}
#endif

half3 ComputeGI(BSDFData bsdfData,
    half3 bakedGI, half occlusion,half smoothness,
    float3 positionWS, float3 positionVS,half3 normalWS, half3 viewDirectionWS)
{
    
    half NoV = saturate(dot(normalWS, viewDirectionWS));

    half3 indirectDiffuse = bakedGI ;
    #if defined(_MATCAP)
    half3 indirectSpecular = MatCapReflection(normalWS, bsdfData.perceptualRoughness, positionVS);
    #else
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, bsdfData.perceptualRoughness, 1.0h);
    #endif
    
    indirectDiffuse *= bsdfData.diffuseColor;
    indirectSpecular *= EnvironmentBRDF(smoothness,abs(NoV),bsdfData.fresnel0);
    
    half3 color = indirectDiffuse + indirectSpecular;
    return color * occlusion;
}

half3 LightingPhysicallyBased(half3 viewDir,half3 normalWS,
    half NoV,half clampedNoV,BSDFData bsdfData,Light light)
{
    half NoL = dot(normalWS, light.direction);
    half clampedNoL = saturate(NoL);
    
    half LoV, NoH, LoH, invLenLV;
    GetBSDFAngle(viewDir,light.direction,NoL,NoV,LoV,NoH,LoH,invLenLV);

    half3 cosWeight = clampedNoL.xxx;
    
    //----计算BRDF----
    half3 radiance = cosWeight  * light.color * light.distanceAttenuation * light.shadowAttenuation;
    //漫反射
    half diffuseTerm = LambertNoPI();

    //镜面反射
    half3 specularTerm;
    
    #ifndef  _SPECULARHIGHLIGHTS_OFF
    
    half3 F = F_Schlick(bsdfData.fresnel0,LoH);
    half DV = DV_SmithJointGGX(NoH,abs(NoL),clampedNoV,bsdfData.roughness,bsdfData.partLambdaV);
    
    specularTerm = DV * F * PI;
    #else
    specularTerm = 0;
    #endif
    
    half3 diffuse = diffuseTerm * bsdfData.diffuseColor * radiance;
    half3 specular = specularTerm * radiance;

    return diffuse + specular;
}

half3 AvatarGlobalIllumination(BRDFData brdfData, half3 bakedGI, half occlusion, float3 positionWS, float3 positionVS, half3 normalWS, half3 viewDirectionWS)
{
    
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;
    #if defined(_MATCAP)
    half3 indirectSpecular = MatCapReflection(normalWS, brdfData.perceptualRoughness, positionVS);
    #else
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h);
    #endif
    
    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
    
    return color * occlusion;
}


half4 AvatarFullPBR(InputData inputData, SurfaceData surfaceData, float3 positionVS)
{
    
    half4 shadowMask = CalculateShadowMask(inputData);
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    
    half3 N = inputData.normalWS;
    half3 V = inputData.viewDirectionWS;
    half NoV = dot(N,V);
    half clampedNoV = max(NoV,0.0001);
    //half NoL = dot(N,mainLight.direction);
    //获取物体的bsdf信息。
    BSDFData bsdfData;
    InitializeBSDFData(surfaceData.albedo,surfaceData.metallic,surfaceData.smoothness ,clampedNoV,V,bsdfData);
    
    //计算间接光
    half3 giColor = ComputeGI(bsdfData,inputData.bakedGI,surfaceData.occlusion,surfaceData.smoothness,inputData.positionWS,positionVS,N, V);
    half3 directLightColor = LightingPhysicallyBased(V,N,NoV,clampedNoV,bsdfData,mainLight);

    #ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
        directLightColor += LightingPhysicallyBased(V,N,NoV,clampedNoV,bsdfData,light);

    }
    #endif

    #if _DEBUG_DIRECTONLY
    half3 color = directLightColor;
    #elif _DEBUG_INDIRECTONLY
    half3 color =  giColor;
    #else
    half3 color = giColor + directLightColor + surfaceData.emission;
    #endif
    
    return DoPostProcessing(color,_Tonemapping);
}


half4 AvatarSimplePBR(InputData inputData, SurfaceData surfaceData, float3 positionVS)
{
    
    half4 shadowMask = CalculateShadowMask(inputData);

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    
    BRDFData brdfData;
    InitializeBRDFData(surfaceData, brdfData);
    
    half3 giColor = AvatarGlobalIllumination(brdfData,inputData.bakedGI,surfaceData.occlusion,inputData.positionWS,positionVS,inputData.normalWS,inputData.viewDirectionWS);
    half3 directLightColor = LightingPhysicallyBased(brdfData,mainLight,inputData.normalWS, inputData.viewDirectionWS);
    
    #ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
        directLightColor += LightingPhysicallyBased(brdfData,light,inputData.normalWS, inputData.viewDirectionWS);
    }
    #endif
    
    #if _DEBUG_DIRECTONLY
    half3 color = directLightColor;
    #elif _DEBUG_INDIRECTONLY
    half3 color =  giColor;
    #else
    half3 color = giColor + directLightColor + surfaceData.emission;
    #endif
    
    return DoPostProcessing(color,_Tonemapping);
}


