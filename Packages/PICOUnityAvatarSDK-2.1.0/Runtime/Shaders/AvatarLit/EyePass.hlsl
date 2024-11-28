#ifndef EYE_PASS_INCLUDED
#define EYE_PASS_INCLUDED

#include "Dependency/URP/ShaderLibrary/Lighting.hlsl"
#include "Dependency/Core/ShaderLibrary/ParallaxMapping.hlsl"

#define HEIGHT_CHANNEL 2

struct Attributes
{
    float4 positionOS       : POSITION;
    float3 normalOS         : NORMAL;
    float4 tangentOS        : TANGENT;
    
    float2 uv               : TEXCOORD0;
    float2 staticLightmapUV    : TEXCOORD1;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv        : TEXCOORD0;
    half3 normalWS  : TEXCOORD1;
    half3 viewDirWS : TEXCOORD3;
    half3 viewDirTS : TXCOORD5;
    half4 tangentWS : TEXCOORD7; 
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 6);

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD8;
    #endif
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD9;
    #endif
    
    float4 vertex : SV_POSITION;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void CirclePupilAnimation(float2 irusUV, float pupilRadius, float pupilAperture, float minimalPupilAperture, float maximalPupilAperture, out float2 animatedIrisUV)
{
    // Compute the normalized iris position
    float2 irisUVCentered = (irusUV - 0.5f) * 2.0f;

    // Compute the radius of the point inside the eye
    float localIrisRadius = length(irisUVCentered);

    // First based on the pupil aperture, let's define the new position of the pupil
    float newPupilRadius = pupilAperture > 0.5 ? lerp(pupilRadius, maximalPupilAperture, (pupilAperture - 0.5) * 2.0) : lerp(minimalPupilAperture, pupilRadius, pupilAperture * 2.0);

    // If we are inside the pupil
    float newIrisRadius = localIrisRadius < newPupilRadius ? ((pupilRadius / newPupilRadius) * localIrisRadius) : 1.0 - ((1.0 - pupilRadius) / (1.0 - newPupilRadius)) * (1.0 - localIrisRadius);
    animatedIrisUV = irisUVCentered / localIrisRadius * newIrisRadius;

    // Convert it back to UV space.
    animatedIrisUV = (animatedIrisUV * 0.5 + float2(0.5, 0.5));
}

void SimpleParallaxMapping(TEXTURE2D_PARAM(heightMap, sampler_heightMap), half3 viewDirTS, half scale, float2 uv, out float2 offset)
{
    half h = SAMPLE_TEXTURE2D(heightMap, sampler_heightMap, uv)[HEIGHT_CHANNEL];
    offset = ParallaxOffset1Step(h, scale, viewDirTS);
}

void ParallaxOcclusionMapping(TEXTURE2D_PARAM(heightMap, sampler_heightMap),half parallaxRatio,float minSampleCount, float maxSampleCount, float3 viewDirTS, half3 normalTS, float2 uv, out float2 offset)
{

    float parallaxLimit = -length(viewDirTS.xy) / viewDirTS.z;
    parallaxLimit *= parallaxRatio;

    float2 offsetDir = normalize(viewDirTS.xy);
    float2 maxOffset = offsetDir * parallaxLimit;
    //Unity simplify it.
    //maxOffset = rayDir.xy / -rayDir.z *_HeightMapScale;
    
    int sampleCount = (int)lerp(maxSampleCount, minSampleCount, dot(viewDirTS, normalTS));
    float stepSize = 1.0f / (float)sampleCount;

    float2 dx = ddx(uv);
    float2 dy = ddy(uv);

    float currentRayHeight = 1.0f;
    float2 currentOffset = float2(0.0,0.0);
    float2 stepOffset = float2(0.0,0.0);
    float2 lastOffset = float2(0.0,0.0);

    float lastSampleHeight = 1.0f;
    float currentSampleHeight = 1.0f;

    int currentSample = 0;
    // Convention: 1.0 is top, 0.0 is bottom - POM is always inward, no extrusion
    while (currentSample < sampleCount)
    {
        currentSampleHeight = SAMPLE_TEXTURE2D_GRAD(heightMap,sampler_heightMap,uv + stepOffset, dx,dy)[HEIGHT_CHANNEL];
        
        if(currentSampleHeight > currentRayHeight)
        {
            float delta1 = currentSampleHeight - currentRayHeight;
            float delta2 = (currentRayHeight + stepSize) - lastSampleHeight;
            float ratio = delta1 / (delta1 + delta2);

            currentOffset = lerp(currentOffset, lastOffset, ratio);

            lastSampleHeight = lerp(currentSampleHeight, lastSampleHeight, ratio);
            currentSample = sampleCount + 1;
        }
        else
        {
            currentSample++;

            currentRayHeight -= stepSize;
            lastOffset = currentOffset;
            currentOffset += stepSize * maxOffset;

            lastSampleHeight = currentSampleHeight;
        }
        #ifdef FLIP_Y
        stepOffset = float2(currentOffset.x, -currentOffset.y);
        #else
        stepOffset = currentOffset;
        #endif
    }
    offset = stepOffset;
}

half3 LightingSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half3 specular, half smoothness, half2 sharpParam)
{
    float3 customLightDir = (lightDir + _HighlightOffset);
    float3 halfVec = SafeNormalize(customLightDir + float3(viewDir));
    half NdotH = half(saturate(dot(normal, halfVec)));
    half modifier = pow(NdotH, smoothness);
    
    #if defined(_SHARP_HIGHTLIGHT)
    modifier = smoothstep(sharpParam.x,sharpParam.x + sharpParam.y,modifier);
    #endif
    
    half3 specularReflection = specular * modifier;
    return lightColor * specularReflection;
}

half3 LightingLambert(half3 lightColor, half3 lightDir, half3 normal, half mask)
{
    half NdotL = saturate(dot(normal, lightDir));
    half fakeCaustic = smoothstep(_CausticRange,1, NdotL) * mask;
    half remaped = Remap_Half(NdotL, half2(0,1), half2(0, _CausticIntensity));
    half radiance = lerp(NdotL, remaped, fakeCaustic); 
    return lightColor * radiance;
}

Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    output.normalWS = half3(normalInput.normalWS);
    
    float sign = input.tangentOS.w * float(GetOddNegativeScale());
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    
    output.viewDirTS = viewDirTS;
    output.viewDirWS = viewDirWS;
    output.tangentWS = tangentWS;
    output.vertex = vertexInput.positionCS;
    output.uv = input.uv;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
    #endif
    
    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
    
    return output;
}

half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half2 uv = input.uv;
    float2 currentOffset = (0.0).xx;
    
    half irisMask = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a;

    // Issue: use half3 type will make parallax map flicker.
    float3 viewDirWS = SafeNormalize(input.viewDirWS);
    
    #if defined(_NORMALMAP)
    half3 viewDirTS = SafeNormalize(input.viewDirTS);
    #if defined(_REFMODE_POM)
    ParallaxOcclusionMapping(TEXTURE2D_ARGS(_BumpMap,sampler_BumpMap),_ParallaxScale,_MinParallaxSamples,_MaxParallaxSamples,viewDirTS,half3(0,0,1),uv,currentOffset);
    #elif defined(_REFMODE_PBR)
    currentOffset = (0.0).xx;
    #else
    SimpleParallaxMapping(TEXTURE2D_ARGS(_BumpMap,sampler_BumpMap),viewDirTS,_ParallaxScale * 2,uv,currentOffset);
    #endif
    
    half3 diffuseNormalTS = SampleNormal(uv + currentOffset, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    half3 specularNormalTS = lerp(diffuseNormalTS, half3(0,0,1), irisMask);
    
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

    half3 diffuseNormalWS = normalize(mul(diffuseNormalTS, tangentToWorld));
    half3 specularNormalWS = normalize(mul(specularNormalTS, tangentToWorld));
    #else
    half3 diffuseNormalWS = normalize(input.normalWS);
    half3 specularNormalWS = diffuseNormalWS;
    #endif

    float2 colorMapUV = uv + currentOffset;
    half3 colorMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, colorMapUV).rgb;
    colorMap = ShiftTextureColor(colorMap, half3(1.0h,1.0h,1.0h),colorMapUV, _ColorRegion1, _ColorRegion2, _ColorRegion3, _ColorRegion4, _UsingAlbedoHue);
    
    half3 albedo = colorMap * kDielectricSpec.a * lerp(half3(1,1,1),_IrisColor.rgb,irisMask);
    
    half smoothness = lerp(_ScleraSmoothness, _IrisSmoothness, irisMask);
    
    half specularSmoothness = lerp(_ScleraSpecSmoothness, _IrisSpecSmoothness, irisMask);
    specularSmoothness = exp2(10 * specularSmoothness + 1);
    half perceptualRoughness = 1 - smoothness;
    half3 specularColor = lerp(_ScleraSpecColor.rgb * _ScleraSpecColor.a, _IrisSpecColor.rgb * _IrisSpecColor.a, irisMask);

    #if defined(_SHARP_HIGHTLIGHT)
    half2 sharpParam = lerp(_SharpParam.zw, _SharpParam.xy,  irisMask);
    #else
    half2 sharpParam = (0).xx;
    #endif
    
    
    #if defined(_MATCAP)
    half matCapStrength = lerp(_MatCapStrength*0.131, _MatCapStrength, irisMask);
    #endif 

    
    half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, diffuseNormalWS);

    half3 reflectVector = reflect(-viewDirWS, specularNormalWS);
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
    half3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
    half roughness = perceptualRoughness * perceptualRoughness;
    half roughness2 = roughness * roughness;
    float surfaceReduction = 1.0 / (roughness2 + 1.0);
    half3 indirectSpec =  surfaceReduction * kDielectricSpec.rgb * irradiance;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS = input.positionWS;
    #else
    float3 positionWS = (0.0).xxx;
    #endif
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    #else
    float4 shadowCoord = float4(0, 0, 0, 0);
    #endif
    
    Light mainLight = GetMainLight(shadowCoord, positionWS, half4(1, 1, 1, 1));

    half3 diffuse = LightingLambert(mainLight.color,mainLight.direction,diffuseNormalWS,irisMask);
    half3 specular = LightingSpecular(mainLight.color,mainLight.direction,specularNormalWS,viewDirWS,specularColor,specularSmoothness, sharpParam);
    
    #if defined(_MATCAP)
    float3 specularNormalVS =  mul((float3x3)GetWorldToViewMatrix(), specularNormalWS);
    float2 matCapUV = specularNormalVS.xy * 0.5 + 0.5;
    half3 matCapTex = SAMPLE_TEXTURE2D_LOD(_MatCap, sampler_MatCap, matCapUV,mip).rgb * surfaceReduction;
    half3 matCapColor = matCapTex * matCapStrength;
    #else
    half3 matCapColor = 0;
    #endif
    
    half3 color = (diffuse * albedo) * mainLight.distanceAttenuation * mainLight.shadowAttenuation + specular
    + bakedGI * albedo + matCapColor + indirectSpec;
    
    return DoPostProcessing(color,_Tonemapping);
}

#endif
