#ifndef HAIR_PASS_INCLUDED
#define HAIR_PASS_INCLUDED

#include "Dependency/URP/ShaderLibrary/Lighting.hlsl"

#if (defined(_NORMALMAP) || !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD2;
#endif

    float3 normalWS                 : TEXCOORD3;
    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
    float3 viewDirWS                : TEXCOORD5;

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    float3 viewDirTS                : TEXCOORD8;
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void Unity_Remap_float4(float4 In, float2 InMinMax, float2 OutMinMax, out float4 Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}


void InitializeInputData(Varyings input, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = SafeNormalize(input.viewDirWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
    
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    output.tangentWS = tangentWS;
    
    
    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

    #if defined(_BAKETEXTURE) || defined(_BAKETEXTURE_SMOOTHNESS)
    float2 remappedUV = input.texcoord * 2 - 1;
    output.positionCS = float4(remappedUV.x, -remappedUV.y, UNITY_NEAR_CLIP_VALUE, 1.0);
    #endif

    return output;
}



float KajiyaKay(float3 T, float3 H, float specularExponent)
{
    float TdotH = dot(T, H);
    float sinTHSq = saturate(1.0 - TdotH * TdotH);

    float dirAttn = saturate(TdotH + 1.0); // Evgenii: this seems like a hack? Do we really need this?

    // Note: Kajiya-Kay is not energy conserving.
    // We attempt at least some energy conservation by approximately normalizing Blinn-Phong NDF.
    // We use the formulation with the NdotL.
    // See http://www.thetenthplanet.de/archives/255.
    float n    = specularExponent;
    float norm = (n + 2) * rcp(2 * PI);

    return dirAttn * norm * PositivePow(sinTHSq, 0.5 * n);
}

float KajiyaKaySimple(float3 T, float3 H, float specularExponent)
{
    float TdotH = dot(T, H);
    float sinTHSq = saturate(1.0 - TdotH * TdotH);
    
    float dirAttn = smoothstep(-1.0,0.0,TdotH);

    return dirAttn  * PositivePow(sinTHSq, specularExponent);
}

float KajiyaShaderToy(float3 T, float3 H, float specularExponent)
{
    float cosang = dot(T, H);
    float sinang = sqrt(1.0-cosang*cosang);
    float spechair = pow(sinang, specularExponent);
    return spechair;
}


float RoughnessToBlinnPhongSpecularExponent(float roughness)
{
    return clamp(2 * rcp(roughness * roughness) - 2, FLT_EPS, rcp(FLT_EPS));//FLT_EPS  5.960464478e-8  // 2^-24, machine epsilon: 1 + EPS = 1 (half of the ULP for 1.0f)
}


half3 GlobalIlluminationHair(half3 normalWS, half3 viewDirectionWS, half3 albedo,half occlusion,half roughness,half3 bakedGI)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnel = Pow4(1-NoV);
    half3 indirectDiffuse = bakedGI  * albedo;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector,roughness , 1.0h);
    half grazingTerm = saturate(1-roughness + 0.04);
    half3 surfaceReduction = lerp(kDieletricSpec.rgb, grazingTerm, fresnel);
    half3 color = (indirectDiffuse + indirectSpecular * surfaceReduction * _ReflectionIntensity) * occlusion;
    return color;
}

half3 HairLighting(half NdotL, half3 albedo,half occlusion, half specularOcclusion, Light light,half3 normalWS,half3 viewDirectionWS,
    half3 specularTint1,half3 t1,half specularExp1,
    half3 specularTint2,half3 t2,half specularExp2)
{
    half shadow = light.shadowAttenuation * 2 - 1;
    half value = min(shadow,NdotL);
    
    half LdotV = dot(light.direction, viewDirectionWS);
    float invLenLV = rsqrt(max(2.0 * LdotV + 2.0, FLT_EPS));

    half3 H = (light.direction + viewDirectionWS) * invLenLV;//

    half spec1Term = KajiyaKaySimple(t1,H,specularExp1);
    half spec2Term = KajiyaKaySimple(t2,H,specularExp2);
    
    half3 hairSpec1 = specularTint1 * spec1Term;
    half3 hairSpec2 = specularTint2 * spec2Term;
    
    half remap = smoothstep(-_ShadowRange, _ShadowSmoothing -_ShadowRange, value);
    
    half3 diffuse = lerp(_ShadowColor.rgb, _BaseColor.rgb, remap) * albedo * occlusion;
    
    half3 color = (diffuse + (hairSpec1 + hairSpec2) * remap * specularOcclusion ) * light.distanceAttenuation  * light.color;
    return color * light.shadowAttenuation;
}

half4 PicoHairRendering(AdaptiveSurfaceData surfaceData,InputData inputData)
{
#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inputData.shadowMask;
#elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
#else
    half4 shadowMask = half4(1, 1, 1, 1);
#endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    half3 normalWS = surfaceData.normalWS;

#if defined(_STRANDDIR_BITANGENT)
    half3 strandDirWS = cross(normalWS, surfaceData.tangentWS);
#elif defined(_STRANDDIR_TANGENT)
    half3 strandDirWS = cross(normalWS, surfaceData.bitangentWS);
#else
    half3 strandDirWS = surfaceData.flowDir;
#endif

    //flowDir = strandDirWS;
    half NdotL = dot(normalWS, mainLight.direction);
    half clampedNoL = saturate(NdotL);
    half lightAO = LerpWhiteTo(surfaceData.occlusion,1-clampedNoL);
    half occlusion = (lightAO + surfaceData.occlusion) * 0.5;

    half3 color = GlobalIlluminationHair(normalWS, inputData.viewDirectionWS, surfaceData.albedo * _BaseColor.rgb ,occlusion,surfaceData.perceptualRoughness,inputData.bakedGI);
    color *= _EnvIntensity;

    half3 t1 = ShiftTangent(strandDirWS,normalWS,surfaceData.shfit1);
    half3 t2 = ShiftTangent(strandDirWS,normalWS,surfaceData.shfit2);
    color += HairLighting(NdotL, surfaceData.albedo,surfaceData.occlusion, surfaceData.specularOcclusion, mainLight,normalWS,inputData.viewDirectionWS,surfaceData.specularColor1,t1,surfaceData.specularExp1,surfaceData.specularColor2,t2,surfaceData.specularExp2);

#ifdef _ADDITIONAL_LIGHTS
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i) {
        Light light = GetAdditionalLight(i, inputData.positionWS);
        half NdotL = dot(normalWS, light.direction);
        color += HairLighting(NdotL,surfaceData.albedo,surfaceData.occlusion, surfaceData.specularOcclusion, light, normalWS, inputData.viewDirectionWS, surfaceData.specularColor1, t1, surfaceData.specularExp1, surfaceData.specularColor2, t2, surfaceData.specularExp2);
    }
#endif

    return DoPostProcessing(color, _Tonemapping);
}

half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    
    AdaptiveSurfaceData surfaceData;
    InitializeAdaptiveSurfaceData(input.uv,input.normalWS,input.tangentWS, surfaceData);
    InputData inputData;
    InitializeInputData(input, inputData);
    
    half4 color = PicoHairRendering(surfaceData, inputData);
    
    return color;
}

half4 UnlitFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = input.uv;
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    return half4(0,0,0,albedoAlpha.w);
}

#endif
