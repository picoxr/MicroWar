#ifndef UNIVERSAL_INPUT_SURFACE_INCLUDED
#define UNIVERSAL_INPUT_SURFACE_INCLUDED

#include "./Core.hlsl"
#include "./SurfaceData.hlsl"
#include "../../PavConfig.hlsl"
#include "../../Core/ShaderLibrary/Packing.hlsl"
#include "../../Core/ShaderLibrary/Color.hlsl"
#include "../../Core/ShaderLibrary/CommonMaterial.hlsl"
#include "../../../GpuSkinning/GPUSkin.cginc"

/**
always       TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
_NORMALMAP   TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
_EMISSION    TEXTURE2D(_EmissionMap);        SAMPLER(sampler_EmissionMap);
PAV_ToonShadowMap TEXTURE2D(_ToonShadowMap); SAMPLER(sampler_ToonShadowMap);
 */
 // amplify to adjust baked albedo texture.

#if defined(PAV_MERGED_TEXTURE) || defined(_ENABLE_STATIC_MESH_BATCHING)
TEXTURE2D_ARRAY(_BaseMapArray);                 SAMPLER(sampler_BaseMapArray);
TEXTURE2D_ARRAY(_ColorRegionMapArray);          SAMPLER(sampler_ColorRegionMapArray);
TEXTURE2D_ARRAY(_BumpMapArray);                 SAMPLER(sampler_BumpMapArray);
TEXTURE2D_ARRAY(_EmissionMapArray);             SAMPLER(sampler_EmissionMapArray);
TEXTURE2D_ARRAY(_SpecGlossMapArray);            SAMPLER(sampler_SpecGlossMapArray);
TEXTURE2D_ARRAY(_MetallicGlossMapArray);        SAMPLER(sampler_MetallicGlossMapArray);
#ifdef PAV_ToonShadowMap
TEXTURE2D_ARRAY(_ToonShadowMapArray);           SAMPLER(sampler_ToonShadowMapArray);
#endif
#ifdef _OCCLUSIONMAP
TEXTURE2D_ARRAY(_OcclusionMapArray);            SAMPLER(sampler_OcclusionMapArray);
#endif
TEXTURE2D_ARRAY(_CustomMapArray_0);             SAMPLER(sampler_CustomMapArray_0);
TEXTURE2D_ARRAY(_CustomMapArray_1);             SAMPLER(sampler_CustomMapArray_1);
TEXTURE2D_ARRAY(_CustomMapArray_2);             SAMPLER(sampler_CustomMapArray_2);
TEXTURE2D_ARRAY(_CustomMapArray_3);             SAMPLER(sampler_CustomMapArray_3);
TEXTURE2D_ARRAY(_CustomMapArray_4);             SAMPLER(sampler_CustomMapArray_4);
TEXTURE2D_ARRAY(_CustomMapArray_5);             SAMPLER(sampler_CustomMapArray_5);
TEXTURE2D_ARRAY(_CustomMapArray_6);             SAMPLER(sampler_CustomMapArray_6);
TEXTURE2D_ARRAY(_CustomMapArray_7);             SAMPLER(sampler_CustomMapArray_7);
TEXTURE2D_ARRAY(_CustomMapArray_8);             SAMPLER(sampler_CustomMapArray_8);
TEXTURE2D_ARRAY(_CustomMapArray_9);             SAMPLER(sampler_CustomMapArray_9);
TEXTURE2D_ARRAY(_CustomMapArray_10);            SAMPLER(sampler_CustomMapArray_10);
TEXTURE2D_ARRAY(_CustomMapArray_11);            SAMPLER(sampler_CustomMapArray_11);
TEXTURE2D_ARRAY(_CustomMapArray_12);            SAMPLER(sampler_CustomMapArray_12);
TEXTURE2D_ARRAY(_CustomMapArray_13);            SAMPLER(sampler_CustomMapArray_13);
TEXTURE2D_ARRAY(_CustomMapArray_14);            SAMPLER(sampler_CustomMapArray_14);
TEXTURE2D_ARRAY(_CustomMapArray_15);            SAMPLER(sampler_CustomMapArray_15);
TEXTURE2D_ARRAY(_CustomMapArray_16);            SAMPLER(sampler_CustomMapArray_16);

#endif

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
TEXTURE2D(_ColorRegionMap);     SAMPLER(sampler_ColorRegionMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
TEXTURE2D(_EmissionMap);        SAMPLER(sampler_EmissionMap);
TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_CustomMap_0);        SAMPLER(sampler_CustomMap_0);
TEXTURE2D(_CustomMap_1);        SAMPLER(sampler_CustomMap_1);
TEXTURE2D(_CustomMap_2);        SAMPLER(sampler_CustomMap_2);
TEXTURE2D(_CustomMap_3);        SAMPLER(sampler_CustomMap_3);
TEXTURE2D(_CustomMap_4);        SAMPLER(sampler_CustomMap_4);
TEXTURE2D(_CustomMap_5);        SAMPLER(sampler_CustomMap_5);
TEXTURE2D(_CustomMap_6);        SAMPLER(sampler_CustomMap_6);
TEXTURE2D(_CustomMap_7);        SAMPLER(sampler_CustomMap_7);
TEXTURE2D(_CustomMap_8);        SAMPLER(sampler_CustomMap_8);
TEXTURE2D(_CustomMap_9);        SAMPLER(sampler_CustomMap_9);
TEXTURE2D(_CustomMap_10);       SAMPLER(sampler_CustomMap_10);
TEXTURE2D(_CustomMap_11);       SAMPLER(sampler_CustomMap_11);
TEXTURE2D(_CustomMap_12);       SAMPLER(sampler_CustomMap_12);
TEXTURE2D(_CustomMap_13);       SAMPLER(sampler_CustomMap_13);
TEXTURE2D(_CustomMap_14);       SAMPLER(sampler_CustomMap_14);
TEXTURE2D(_CustomMap_15);       SAMPLER(sampler_CustomMap_15);
TEXTURE2D(_CustomMap_16);       SAMPLER(sampler_CustomMap_16);



#ifdef PAV_ToonShadowMap
TEXTURE2D(_ToonShadowMap);        SAMPLER(sampler_ToonShadowMap);
#endif
#ifdef _OCCLUSIONMAP
TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
#endif

#ifdef _SECOND_BASEMAP
static float3 _uv2;
TEXTURE2D(_SecondBaseMap);                          SAMPLER(sampler_SecondBaseMap);
#if defined(PAV_MERGED_TEXTURE) || defined(_ENABLE_STATIC_MESH_BATCHING)
TEXTURE2D_ARRAY(_SecondBaseMapArray);               SAMPLER(sampler_SecondBaseMapArray);
#endif
#define PAV_GET_SECOND_UV(input) _uv2.xy = input.uv2
#else
#define PAV_GET_SECOND_UV(input)
#endif

#ifdef _SECOND_NORMALMAP
TEXTURE2D(_SecondBumpMap);                          SAMPLER(sampler_SecondBumpMap);
#if defined(PAV_MERGED_TEXTURE) || defined(_ENABLE_STATIC_MESH_BATCHING)
TEXTURE2D_ARRAY(_SecondBumpMapArray);               SAMPLER(sampler_SecondBumpMapArray);
#endif
#endif

#ifdef _SECOND_METALLICSPECGLOSSMAP
TEXTURE2D(_SecondMetallicSpecGlossMap);             SAMPLER(sampler_SecondMetallicSpecGlossMap);
#if defined(PAV_MERGED_TEXTURE) || defined(_ENABLE_STATIC_MESH_BATCHING)
TEXTURE2D_ARRAY(_SecondMetallicSpecGlossMapArray);  SAMPLER(sampler_SecondMetallicSpecGlossMapArray);
#endif
#endif

///////////////////////////////////////////////////////////////////////////////
//                      Material Property Helpers                            //
///////////////////////////////////////////////////////////////////////////////
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

half3 ApplyAlbedo(half3 albedo, half3 baseColor, half shaderType)
{
    half3 result;
    half3 a = albedo;
    half3 b = baseColor;

    uint shaderTypeI = (uint)round(shaderType);

    // modified by dujing and tianshengcai
    if (//shaderTypeI == PAV_SHADER_TYPE_BODY_BASE 
        shaderTypeI == PAV_SHADER_TYPE_HAIR_BASE
        //shaderTypeI == PAV_SHADER_TYPE_CLOTH_BASE
        )
    {
        //
        // albedo = dot(albedo, half3(0.2126729, 0.7151522, 0.0721750));
        albedo = pow(albedo, 1.0 / 2.2);

        // overlay
        result.r = albedo.r < 0.5 ? (2.0 * albedo.r * baseColor.r) : (1.0 - 2.0 * (1.0 - albedo.r) * (1.0 - baseColor.r));
        result.g = albedo.g < 0.5 ? (2.0 * albedo.g * baseColor.g) : (1.0 - 2.0 * (1.0 - albedo.g) * (1.0 - baseColor.g));
        result.b = albedo.r < 0.5 ? (2.0 * albedo.b * baseColor.b) : (1.0 - 2.0 * (1.0 - albedo.b) * (1.0 - baseColor.b));
    }
    else
    {
        ////albedo * baseColor;
        //half3 hsv = RgbToHsv(albedo * baseColor);
        //hsv.y *= 1.05;// LinearToSRGB(baseColor.r) * 2.0;
        //result = HsvToRgb(hsv);
        //return result;

        // multiply
        result = a * b;
        result *= _BaseColorAmplify;
    }

    return result;
}

half3 ApplyAlbedo(half3 albedo, half3 baseColor, half shaderType, half4 albedoMaskAlpha, half4 colorMask1, half4 colorMask2, half4 colorMask3, half4 colorMask4, float usingAlbedoHue)
{
    half3 result;

    half3 a = albedo;
    half3 b = baseColor;

#ifdef PAV_COLOR_REGION_BAKED
    result = a * b;
#else
    uint shaderTypeI = (uint)round(shaderType);

    // multiply
    half3 colorBase = LinearToGamma22(a.rgb);
    result.rgb = colorBase.rgb;
    float3 colorBaseHSV = RgbToHsv(colorBase.rgb);

    // layer1
    {
        float baseHue = colorBaseHSV.r * usingAlbedoHue;
        float3 colorLayer = float3(colorMask1.r + baseHue, colorBaseHSV.gb * colorMask1.gb);
        result.rgb = lerp(result, saturate(HsvToRgb(colorLayer.rgb)), saturate(albedoMaskAlpha.r * colorMask1.a));
    }
    {
        // layer2
        float3 colorLayer1 = (float3(colorMask2.r, colorBaseHSV.gb * colorMask2.gb));
        result.rgb = lerp(result, saturate(HsvToRgb(colorLayer1.rgb)), saturate(albedoMaskAlpha.g * colorMask2.a));
    }
    {
        // layer3
        float3 colorLayer2 = (float3(colorMask3.r, colorBaseHSV.gb * colorMask3.gb));
        result.rgb = lerp(result, saturate(HsvToRgb(colorLayer2.rgb)), saturate(albedoMaskAlpha.b * colorMask3.a));
    }
    {
        // layer4
        float3 colorLayer3 = (float3(colorMask4.r, colorBaseHSV.gb * colorMask4.gb));
        result.rgb = lerp(result, saturate(HsvToRgb(colorLayer3.rgb)), saturate(albedoMaskAlpha.a * colorMask4.a));
    }
    result.rgb = Gamma22ToLinear(result.rgb);

    result *= b;
#endif

    return result;
}

float GetMipBias()
{
    return _MipBias;
}


#if defined(PAV_MERGED_TEXTURE) || defined(_ENABLE_STATIC_MESH_BATCHING)

#if defined(_ENABLE_STATIC_MESH_BATCHING)

//struct UVAtlasInfo
//{
//    float4 flag;
//    float4 transform;
//};

struct MergedMaterialUniformData
{
    // UVAtlasInfo atlas[32];
    float4 atlas[32];
    float4 uniform1;
    float4 uniform2;
    float4 uniform3;
    float4 uniform4;
    float4 baseColor;
    float4 outlineColor;
    float4 specColor;
    float4 emissiveColor;
    float4 baseMapST;
    float4 colorRegion[4];
    float4 customVec[9];
};
StructuredBuffer<MergedMaterialUniformData> _MtlData;

struct CurrentUniform
{
    float4 u1, u2, u3, u4;
} currentData;

#define PAV_TEXTURE_KEY_BASE_MAP 0
#define PAV_TEXTURE_KEY_COLOR_REGION_MAP 1
#define PAV_TEXTURE_KEY_METALLIC_GLOSS_MAP 2
#define PAV_TEXTURE_KEY_BUMP_MAP 3
#define PAV_TEXTURE_KEY_SPEC_GLOSS_MAP 4 
#define PAV_TEXTURE_KEY_OCCLUSION_MAP 6
#define PAV_TEXTURE_KEY_EMISSION_MAP 7
#define PAV_TEXTURE_KEY_TOON_SHADOW_MAP 11
#define PAV_TEXTURE_KEY_SECOND_BASE_MAP 12
#define PAV_TEXTURE_KEY_SECOND_BUMP_MAP 13
#define PAV_TEXTURE_KEY_SECOND_METALLIC_SPEC_GLOSS_MAP 14

void GetTextureSize(uint textureIndex, out float2 textureSize)
{
    textureSize = float2(0.0f, 0.0f);
}

void GetTextureInfo(uint textureIndex, uint mtlIndex, out float textureArrayIndex, out float4 uvScaleOffset)
{
    /*float4 v0 = _MtlData[mtlIndex].atlas[textureIndex].flag;
    textureArrayIndex = v0.y;
    uvScaleOffset = _MtlData[mtlIndex].atlas[textureIndex].transform;*/
    float4 v0 = _MtlData[mtlIndex].atlas[textureIndex];
    float2 floorXY = floor(v0.xy);
    textureArrayIndex = floor(floorXY.x) > 0.0f ? floorXY.y : -1.0f;
    uvScaleOffset = float4(v0.xy - floorXY, v0.zw);
}

#endif

uint mtlIndex = 0;

float GetMipLevel(float2 uv)
{
    float2 dx = ddx(uv);
    float2 dy = ddy(uv);
    float delta = max(dot(dx, dx), dot(dy, dy));
    return 0.5 * log2(delta);
}

void GetMergedTextureUVFrag(inout float2 uv, out float textureArrayIndex, out float mip, uint textureIndex)
{
    float4 uvScaleOffset = float4(1.0f, 1.0f, 0.0f, 0.0f);
    // GetTextureAtlasInfo(textureIndex, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = fmod(uv, 1.0);
    if (uv.x < 0) uv.x += 1.0;
    if (uv.y < 0) uv.y += 1.0;
    PAV_FLIP_UV_Y(uv);

    // add offset if sample on border
    float2 textureSize;
    GetTextureSize(textureIndex, textureSize); // texture array size
    textureSize *= uvScaleOffset.xy; // raw texture size
    mip = max(GetMipLevel(uv * textureSize) + GetMipBias(), 0.0);
    float2 textureSizeInvHalf = 0.5 / textureSize * pow(2.0, mip) * 2.0;

    if (uv.x <= textureSizeInvHalf.x) uv.x += textureSizeInvHalf.x;
    if (uv.x >= 1.0 - textureSizeInvHalf.x) uv.x -= textureSizeInvHalf.x;
    if (uv.y <= textureSizeInvHalf.y) uv.y += textureSizeInvHalf.y;
    if (uv.y >= 1.0 - textureSizeInvHalf.y) uv.y -= textureSizeInvHalf.y;

    uv = uv * uvScaleOffset.xy + uvScaleOffset.zw;
}

half4 SampleAlbedoAlpha(float2 uv)
{
    float4 uvScaleOffset;

    float textureArrayIndex;
    // float mip;
    GetTextureInfo(PAV_TEXTURE_KEY_BASE_MAP, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = uv * uvScaleOffset.zw + uvScaleOffset.xy;
    // GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_BASE_MAP);
    half4 c = textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_BaseMapArray, sampler_BaseMapArray, uv, textureArrayIndex) : half4(1.0, 1.0, 1.0, 1.0);

#ifdef _SECOND_BASEMAP
    {
        float2 uv2 = _uv2.xy;
        float secondTextureArrayIndex;
        GetTextureInfo(PAV_TEXTURE_KEY_SECOND_BASE_MAP, mtlIndex, secondTextureArrayIndex, uvScaleOffset);
        uv2 = uv2 * uvScaleOffset.zw + uvScaleOffset.xy;
        // GetMergedTextureUVFrag(float3(uv2, uv.z), secondTextureArrayIndex, PAV_TEXTURE_KEY_SECOND_BASE_MAP);
        half4 c2 = secondTextureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_SecondBaseMapArray, sampler_SecondBaseMapArray, uv2, secondTextureArrayIndex) : half4(c.rgb, 0);
        _uv2.z = c2.a;
        c.rgb = lerp(c.rgb, c2.rgb, _uv2.z);
    }
#endif

    return c;
}

half4 SampleColorRegionsMap(float2 uv)
{
    float4 uvScaleOffset;

    float textureArrayIndex;
    // float mip;
    GetTextureInfo(PAV_TEXTURE_KEY_COLOR_REGION_MAP, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = uv * uvScaleOffset.zw + uvScaleOffset.xy;
    // GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_COLOR_REGION_MAP);
    return textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_ColorRegionMapArray, sampler_ColorRegionMapArray, uv, textureArrayIndex) : half4(0.0, 0.0, 0.0, 0.0);
}

half4 SampleToonShadow(float2 uv)
{
#ifndef PAV_ToonShadowMap
    return 0;
#else
    float4 uvScaleOffset;
    float textureArrayIndex = 0;
    // float mip;
    GetTextureInfo(PAV_TEXTURE_KEY_TOON_SHADOW_MAP, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = uv * uvScaleOffset.zw + uvScaleOffset.xy;
    // GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_TOON_SHADOW_MAP);
    return textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_ToonShadowMapArray, sampler_ToonShadowMapArray, uv, textureArrayIndex) : half4(0.0, 0.0, 0.0, 1.0);
#endif
}

half3 SampleNormal(float2 uv, half scale = 1.0h)
{
#ifdef _NORMALMAP
    float4 uvScaleOffset;

    float textureArrayIndex = 0;
    // float mip;
    GetTextureInfo(PAV_TEXTURE_KEY_BUMP_MAP, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = uv * uvScaleOffset.zw + uvScaleOffset.xy;
    // GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_BUMP_MAP);
    half4 n = textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_BumpMapArray, sampler_BumpMapArray, uv, textureArrayIndex) : half4(0.5, 0.5, 1.0, 0.5);

#ifdef _SECOND_NORMALMAP
    {
        float2 uv2 = _uv2.xy;
        float secondTextureArrayIndex;
        GetTextureInfo(PAV_TEXTURE_KEY_SECOND_BUMP_MAP, mtlIndex, secondTextureArrayIndex, uvScaleOffset);
        uv2 = uv2 * uvScaleOffset.zw + uvScaleOffset.xy;
        // GetMergedTextureUVFrag(float3(uv2, uv.z), secondTextureArrayIndex, PAV_TEXTURE_KEY_SECOND_BUMP_MAP);
        half3 n2 = secondTextureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_SecondBumpMapArray, sampler_SecondBumpMapArray, uv2, secondTextureArrayIndex).rgb : n.rgb;
        n.rgb = lerp(n.rgb, n2, _uv2.z);
    }
#endif

#if BUMP_SCALE_NOT_SUPPORTED
    return UnpackNormal(n);
#else
    return UnpackNormalScale(n, scale);
#endif
#else
    return half3(0.0h, 0.0h, 1.0h);
#endif
}

half3 SampleEmission(float2 uv, half3 emissionColor)
{
#ifndef _EMISSION
    return 0;
#else
    float4 uvScaleOffset;
    float textureArrayIndex = 0;
    // float mip;
    GetTextureInfo(PAV_TEXTURE_KEY_EMISSION_MAP, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = uv * uvScaleOffset.zw + uvScaleOffset.xy;
    // GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_EMISSION_MAP);
    half4 c = textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_EmissionMapArray, sampler_EmissionMapArray, uv, textureArrayIndex) : half4(0.0, 0.0, 0.0, 1.0);
    return c.rgb * emissionColor;
#endif
}

half SampleOcclusionMap(float2 uv)
{
#ifdef _OCCLUSIONMAP
    float4 uvScaleOffset;
    float textureArrayIndex = 0;
    // float mip;
    GetTextureInfo(PAV_TEXTURE_KEY_OCCLUSION_MAP, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = uv * uvScaleOffset.zw + uvScaleOffset.xy;
    // GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_OCCLUSION_MAP);
    half4 c = textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_OcclusionMapArray, sampler_OcclusionMapArray, uv, textureArrayIndex) : half4(1.0, 1.0, 1.0, 1.0);
    return c.g;
#else
    return 1.0;
#endif
}

half4 SampleSpecGlossMap(float2 uv)
{
    float4 uvScaleOffset;
    float textureArrayIndex = 0;
    // float mip;
    GetTextureInfo(PAV_TEXTURE_KEY_SPEC_GLOSS_MAP, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = uv + uvScaleOffset.zw + uvScaleOffset.xy;
    // GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_SPEC_GLOSS_MAP);
    half4 c = textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_SpecGlossMapArray, sampler_SpecGlossMapArray, uv, textureArrayIndex) : half4(1.0, 1.0, 1.0, 1.0);

#ifdef _SECOND_METALLICSPECGLOSSMAP
    {
        float2 uv2 = _uv2.xy;
        float secondTextureArrayIndex;
        GetTextureInfo(PAV_TEXTURE_KEY_SECOND_METALLIC_SPEC_GLOSS_MAP, mtlIndex, secondTextureArrayIndex, uvScaleOffset);
        uv2 = uv2 * uvScaleOffset.zw + uvScaleOffset.xy;
        // GetMergedTextureUVFrag(uv2, secondTextureArrayIndex, PAV_TEXTURE_KEY_SECOND_METALLIC_SPEC_GLOSS_MAP);
        half4 c2 = secondTextureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_SecondMetallicSpecGlossMapArray, sampler_SecondMetallicSpecGlossMapArray, uv2, secondTextureArrayIndex) : c;
        c = lerp(c, c2, _uv2.z);
    }
#endif

    return c;
}

half4 SampleMetallicGlossMap(float2 uv)
{
    float4 uvScaleOffset;
    float textureArrayIndex = 0;
    // float mip;
    GetTextureInfo(PAV_TEXTURE_KEY_METALLIC_GLOSS_MAP, mtlIndex, textureArrayIndex, uvScaleOffset);
    uv = uv * uvScaleOffset.zw + uvScaleOffset.xy;
    // GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_METALLIC_GLOSS_MAP);
    half4 c = textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_MetallicGlossMapArray, sampler_MetallicGlossMapArray, uv, textureArrayIndex) : half4(0.0, 0.0, 0.0, 0.0);

#ifdef _SECOND_METALLICSPECGLOSSMAP
    {
        float2 uv2 = _uv2.xy;
        float secondTextureArrayIndex;
        GetTextureInfo(PAV_TEXTURE_KEY_SECOND_METALLIC_SPEC_GLOSS_MAP, mtlIndex, secondTextureArrayIndex, uvScaleOffset);
        uv2 = uv2 * uvScaleOffset.zw + uvScaleOffset.xy;
        // GetMergedTextureUVFrag(float3(uv2, uv.z), secondTextureArrayIndex, PAV_TEXTURE_KEY_SECOND_METALLIC_SPEC_GLOSS_MAP);
        half4 c2 = secondTextureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(_SecondMetallicSpecGlossMapArray, sampler_SecondMetallicSpecGlossMapArray, uv2, secondTextureArrayIndex) : c;
        c = lerp(c, c2, _uv2.z);
    }
#endif

    return c;
}

#define PAV_SAMPLE_CUSTOM_MAP_FUNC(index) \
    half4 SampleCustomMap_##index(float2 uv) \
    { \
        float textureArrayIndex = 0; \
        float mip; \
        GetMergedTextureUVFrag(uv, textureArrayIndex, mip, PAV_TEXTURE_KEY_CUSTOM_MAP_0 + index); \
        return textureArrayIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY_LOD(_CustomMapArray_##index, sampler_CustomMapArray_##index, uv, textureArrayIndex, mip) : half4(1.0, 1.0, 1.0, 1.0); \
    }

#ifdef _ENABLE_STATIC_MESH_BATCHING

#define PAV_GET_CUTOFF(v) float v = currentData.u1.y; 
#define PAV_GET_SMOOTHNESS(v) float v = currentData.u2.y; 
#define PAV_GET_METALLIC(v) float v = currentData.u2.x; 
#define PAV_GET_BUMP_SCALE(v) float v = currentData.u2.z; 
#define PAV_GET_PARALLAX(v) float v = currentData.u1.w; 
#define PAV_GET_OCCLUSION_STRENGTH(v) float v = currentData.u2.w;
#define PAV_GET_SHADER_TYPE(v) float v = currentData.u1.x; 
#define PAV_GET_USING_ALBEDO_HUE(v) float v = currentData.u1.z;
#define PAV_GET_BASE_COLOR(v) half4 v = _MtlData[mtlIndex].baseColor;
#define PAV_GET_SPEC_COLOR(v) half4 v = _MtlData[mtlIndex].specColor;
#define PAV_GET_EMISSION_COLOR(v) half4 v = _MtlData[mtlIndex].emissiveColor;
#define PAV_GET_BASE_COLOR_MASK1(v) half4 v = _MtlData[mtlIndex].colorRegion[0];
#define PAV_GET_BASE_COLOR_MASK2(v) half4 v = _MtlData[mtlIndex].colorRegion[1];
#define PAV_GET_BASE_COLOR_MASK3(v) half4 v = _MtlData[mtlIndex].colorRegion[2];
#define PAV_GET_BASE_COLOR_MASK4(v) half4 v = _MtlData[mtlIndex].colorRegion[3];
#define PAV_GET_CUSTOM_VECTOR(v, index) half4 v = _MtlData[mtlIndex].customVec[index];

#else

#define PAV_GET_MATERIAL_DATA(materialIndex) _mergedMaterialData = GetMaterialData((uint) round(materialIndex)); GetTextureSizes()
#define PAV_GET_OUTLINE(v) float v = _mergedMaterialData.outline
#define PAV_GET_CUTOFF(v) float v = _mergedMaterialData.cutoff
#define PAV_GET_SMOOTHNESS(v) float v = _mergedMaterialData.smoothness
#define PAV_GET_GLOSS_MAP_SCALE(v) float v = _mergedMaterialData.glossMapScale
#define PAV_GET_SMOOTH_TEXTURE_CHANNEL(v) float v = _mergedMaterialData.smoothnessTextureChannel
#define PAV_GET_METALLIC(v) float v = _mergedMaterialData.metallic
#define PAV_GET_BUMP_SCALE(v) float v = _mergedMaterialData.bumpScale
#define PAV_GET_PARALLAX(v) float v = _mergedMaterialData.parallax
#define PAV_GET_OCCLUSION_STRENGTH(v) float v = _mergedMaterialData.occlusionStrength
#define PAV_GET_DETAIL_ALBEDO_MAP_SCALE(v) float v = _mergedMaterialData.detailAlbedoMapScale
#define PAV_GET_DETAIL_NORMAL_MAP_SCALE(v) float v = _mergedMaterialData.detailNormalMapScale
#define PAV_GET_CLEAR_COAT_MASK(v) float v = _mergedMaterialData.clearCoatMask
#define PAV_GET_CLEAR_COAT_SMOOTHNESS(v) float v = _mergedMaterialData.clearCoatSmoothness
#define PAV_GET_SHADER_TYPE(v) float v = _mergedMaterialData.shaderType
#define PAV_GET_USING_ALBEDO_HUE(v) float v = _mergedMaterialData.usingAlbedoHue
#define PAV_GET_OUTLINE_COLOR(v) float4 v = _mergedMaterialData.outlineColor
#define PAV_GET_BASE_COLOR(v) half4 v = _mergedMaterialData.baseColor
#define PAV_GET_SPEC_COLOR(v) half4 v = _mergedMaterialData.specColor
#define PAV_GET_EMISSION_COLOR(v) half4 v = _mergedMaterialData.emissionColor
#define PAV_GET_BASE_MAP_ST(v) half4 v = _mergedMaterialData.baseMap_ST
#define PAV_GET_BASE_COLOR_MASK1(v) half4 v = _mergedMaterialData.colorRegion1
#define PAV_GET_BASE_COLOR_MASK2(v) half4 v = _mergedMaterialData.colorRegion2
#define PAV_GET_BASE_COLOR_MASK3(v) half4 v = _mergedMaterialData.colorRegion3
#define PAV_GET_BASE_COLOR_MASK4(v) half4 v = _mergedMaterialData.colorRegion4
#define PAV_GET_CUSTOM_VECTOR(v, index) half4 v = _mergedMaterialData.customVecs[index]

#endif

#else // !PAV_MERGED_TEXTURE

half4 SampleAlbedoAlpha(float2 uv)
{
    //PAV_FLIP_UV_Y(uv);
    half4 c = SAMPLE_TEXTURE2D_BIAS(_BaseMap, sampler_BaseMap, uv, GetMipBias());

#ifdef _SECOND_BASEMAP
    {
        float2 uv2 = _uv2.xy;
        //PAV_FLIP_UV_Y(uv2);
        half4 c2 = SAMPLE_TEXTURE2D_BIAS(_SecondBaseMap, sampler_SecondBaseMap, uv2, GetMipBias());
        _uv2.z = c2.a;
        c.rgb = lerp(c.rgb, c2.rgb, _uv2.z);
    }
#endif

    return c;
}

half4 SampleColorRegionsMap(float2 uv)
{
    //PAV_FLIP_UV_Y(uv);
    half4 c = SAMPLE_TEXTURE2D_BIAS(_ColorRegionMap, sampler_ColorRegionMap, uv, GetMipBias());

    return c;
}

half4 SampleToonShadow(float2 uv)
{
#ifndef PAV_ToonShadowMap
    return 0;
#else
    //PAV_FLIP_UV_Y(uv);
    return SAMPLE_TEXTURE2D_BIAS(_ToonShadowMap, sampler_ToonShadowMap, uv, GetMipBias());
#endif
}

half3 SampleNormal(float2 uv, half scale = 1.0h)
{
#ifdef _NORMALMAP
    //PAV_FLIP_UV_Y(uv);
    half4 n = SAMPLE_TEXTURE2D_BIAS(_BumpMap, sampler_BumpMap, uv, GetMipBias());

#ifdef _SECOND_NORMALMAP
    {
        float2 uv2 = _uv2.xy;
        //PAV_FLIP_UV_Y(uv2);
        half3 n2 = SAMPLE_TEXTURE2D_BIAS(_SecondBumpMap, sampler_SecondBumpMap, uv2, GetMipBias()).rgb;
        n.rgb = lerp(n.rgb, n2, _uv2.z);
    }
#endif

#if BUMP_SCALE_NOT_SUPPORTED
    return UnpackNormal(n);
#else
    return UnpackNormalScale(n, scale);
#endif
#else
    return half3(0.0h, 0.0h, 1.0h);
#endif
}

half3 SampleEmission(float2 uv, half3 emissionColor)
{
#ifndef _EMISSION
    return 0;
#else
    //PAV_FLIP_UV_Y(uv);
    return SAMPLE_TEXTURE2D_BIAS(_EmissionMap, sampler_EmissionMap, uv, GetMipBias()).rgb * emissionColor;
#endif
}

half SampleOcclusionMap(float2 uv)
{
#ifdef _OCCLUSIONMAP
    //PAV_FLIP_UV_Y(uv);
    return SAMPLE_TEXTURE2D_BIAS(_OcclusionMap, sampler_OcclusionMap, uv, GetMipBias()).g;
#else
    return 1.0;
#endif
}

half4 SampleSpecGlossMap(float2 uv)
{
    //PAV_FLIP_UV_Y(uv);
    half4 c = SAMPLE_TEXTURE2D_BIAS(_SpecGlossMap, sampler_SpecGlossMap, uv, GetMipBias());

#ifdef _SECOND_METALLICSPECGLOSSMAP
    {
        float2 uv2 = _uv2.xy;
        half4 c2 = SAMPLE_TEXTURE2D_BIAS(_SecondMetallicSpecGlossMap, sampler_SecondMetallicSpecGlossMap, uv2, GetMipBias());
        c = lerp(c, c2, _uv2.z);
    }
#endif

    return c;
}

half4 SampleMetallicGlossMap(float2 uv)
{
    //PAV_FLIP_UV_Y(uv);
    half4 c = SAMPLE_TEXTURE2D_BIAS(_MetallicGlossMap, sampler_MetallicGlossMap, uv, GetMipBias());

#ifdef _SECOND_METALLICSPECGLOSSMAP
    {
        float2 uv2 = _uv2.xy;
        half4 c2 = SAMPLE_TEXTURE2D_BIAS(_SecondMetallicSpecGlossMap, sampler_SecondMetallicSpecGlossMap, uv2, GetMipBias());
        c = lerp(c, c2, _uv2.z);
    }
#endif

    return c;
}

#define PAV_SAMPLE_CUSTOM_MAP_FUNC(index) \
    half4 SampleCustomMap_##index(float2 uv) \
    { \
        return SAMPLE_TEXTURE2D_BIAS(_CustomMap_##index, sampler_CustomMap_##index, uv, GetMipBias()); \
    }

#define PAV_GET_MATERIAL_DATA(index)
#define PAV_GET_OUTLINE(v) float v = _Outline
#define PAV_GET_CUTOFF(v) float v = _Cutoff
#define PAV_GET_SMOOTHNESS(v) float v = _Smoothness
#define PAV_GET_GLOSS_MAP_SCALE(v) float v = _GlossMapScale
#define PAV_GET_SMOOTH_TEXTURE_CHANNEL(v) float v = _SmoothnessTextureChannel
#define PAV_GET_METALLIC(v) float v = _Metallic
#define PAV_GET_BUMP_SCALE(v) float v = _BumpScale
#define PAV_GET_PARALLAX(v) float v = _Parallax
#define PAV_GET_OCCLUSION_STRENGTH(v) float v = _OcclusionStrength
#define PAV_GET_DETAIL_ALBEDO_MAP_SCALE(v) float v = _DetailAlbedoMapScale
#define PAV_GET_DETAIL_NORMAL_MAP_SCALE(v) float v = _DetailNormalMapScale
#define PAV_GET_CLEAR_COAT_MASK(v) float v = _ClearCoatMask
#define PAV_GET_CLEAR_COAT_SMOOTHNESS(v) float v = _ClearCoatSmoothness
#define PAV_GET_SHADER_TYPE(v) float v = _ShaderType
#define PAV_GET_USING_ALBEDO_HUE(v) float v = _UsingAlbedoHue
#define PAV_GET_OUTLINE_COLOR(v) float4 v = _OutlineColor
#define PAV_GET_BASE_COLOR(v) half4 v = _BaseColor
#define PAV_GET_SPEC_COLOR(v) half4 v = _SpecColor
#define PAV_GET_EMISSION_COLOR(v) half4 v = _EmissionColor
#define PAV_GET_BASE_MAP_ST(v) half4 v = _BaseMap_ST
#define PAV_GET_BASE_COLOR_MASK1(v) half4 v = _ColorRegion1
#define PAV_GET_BASE_COLOR_MASK2(v) half4 v = _ColorRegion2
#define PAV_GET_BASE_COLOR_MASK3(v) half4 v = _ColorRegion3
#define PAV_GET_BASE_COLOR_MASK4(v) half4 v = _ColorRegion4
#define PAV_GET_CUSTOM_VECTOR(v, index) half4 v = _CustomVec_##index

#endif // PAV_MERGED_TEXTURE

#define PAV_SAMPLE_ALBEDO_ALPHA(uv) SampleAlbedoAlpha(uv)

#ifdef PAV_COLOR_REGION_BAKED
#   define PAV_SAMPLE_COLOR_REGIONS(uv) 0
#else
#   define PAV_SAMPLE_COLOR_REGIONS(uv) SampleColorRegionsMap(uv)
#endif

#define PAV_SAMPLE_TOON_SHADOW(uv) SampleToonShadow(uv)
#define PAV_SAMPLE_NORMAL(uv, scale) SampleNormal(uv, scale)
#define PAV_SAMPLE_EMISSION(uv, emissionColor) SampleEmission(uv, emissionColor)
#define PAV_SAMPLE_OCCLUSION_MAP(uv) SampleOcclusionMap(uv)
#define PAV_SAMPLE_SPEC_GLOSS_MAP(uv) SampleSpecGlossMap(uv)
#define PAV_SAMPLE_METALLIC_GLOSS_MAP(uv) SampleMetallicGlossMap(uv)

PAV_SAMPLE_CUSTOM_MAP_FUNC(0)
PAV_SAMPLE_CUSTOM_MAP_FUNC(1)
PAV_SAMPLE_CUSTOM_MAP_FUNC(2)
PAV_SAMPLE_CUSTOM_MAP_FUNC(3)
PAV_SAMPLE_CUSTOM_MAP_FUNC(4)
PAV_SAMPLE_CUSTOM_MAP_FUNC(5)
PAV_SAMPLE_CUSTOM_MAP_FUNC(6)
PAV_SAMPLE_CUSTOM_MAP_FUNC(7)
PAV_SAMPLE_CUSTOM_MAP_FUNC(8)
PAV_SAMPLE_CUSTOM_MAP_FUNC(9)
PAV_SAMPLE_CUSTOM_MAP_FUNC(10)
PAV_SAMPLE_CUSTOM_MAP_FUNC(11)
PAV_SAMPLE_CUSTOM_MAP_FUNC(12)
PAV_SAMPLE_CUSTOM_MAP_FUNC(13)
PAV_SAMPLE_CUSTOM_MAP_FUNC(14)
PAV_SAMPLE_CUSTOM_MAP_FUNC(15)
PAV_SAMPLE_CUSTOM_MAP_FUNC(16)

#define PAV_SAMPLE_CUSTOM_MAP(uv, mapIndex) SampleCustomMap_##mapIndex(uv)

#endif
