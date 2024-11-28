#ifndef AVATAR_COMMON
#define AVATAR_COMMON

#include "Dependency/Core/ShaderLibrary/Color.hlsl"

#ifndef PAV_COLOR_REGION_BAKED
TEXTURE2D(_ColorRegionMap); SAMPLER(sampler_ColorRegionMap);
#endif

#if defined(_ENABLE_STATIC_MESH_BATCHING)

TEXTURE2D_ARRAY(_BaseMapArray);                 SAMPLER(sampler_BaseMapArray);
TEXTURE2D_ARRAY(_ColorRegionMapArray);          SAMPLER(sampler_ColorRegionMapArray);
TEXTURE2D_ARRAY(_BumpMapArray);                 SAMPLER(sampler_BumpMapArray);
TEXTURE2D_ARRAY(_EmissionMapArray);             SAMPLER(sampler_EmissionMapArray);
TEXTURE2D_ARRAY(_MatCapMapArray);               SAMPLER(sampler_MatCapMapArray);

//struct UVAtlasInfo
//{
//    float4 flag;
//    float4 transform;
//};

struct MergedMaterialUniformData
{
    float4 atlas[5];
    float4 uniform1;
    float4 uniform2;
    float4 uniform3;
    float4 baseMapST;
    float4 baseColor;
    float4 colorRegion1;
    float4 colorRegion2;
    float4 colorRegion3;
    float4 colorRegion4;
    float4 emissiveColor;
};
StructuredBuffer<MergedMaterialUniformData> _MtlData;

uint mtlIndex;

void GetTextureInfo(uint textureIndex, uint mtlIndex, out float textureArrayIndex, out float4 uvScaleOffset)
{
    float4 v0 = _MtlData[mtlIndex].atlas[textureIndex];
    float2 floorXY = floor(v0.xy);
    textureArrayIndex = floor(floorXY.x) > 0.0f ? floorXY.y : -1.0f;
    uvScaleOffset = float4(v0.xy - floorXY, v0.zw);
    /*textureArrayIndex = v0.y;
    uvScaleOffset = _MtlData[mtlIndex].atlas[textureIndex].transform;*/
}

half4 SampleTexArray(uint textureIndex, uint materialIndex, float2 uv, TEXTURE2D_ARRAY(texArray), SAMPLER(texArraySampler), half4 defaultColor)
{
    float texArrIndex;
    float4 texTransform;
    GetTextureInfo(textureIndex, materialIndex, texArrIndex, texTransform);
    return texArrIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY(texArray, texArraySampler, uv * texTransform.zw + texTransform.xy, texArrIndex) : defaultColor;
}

half4 SampleTexArrayLod(uint textureIndex, uint materialIndex, float3 uv, TEXTURE2D_ARRAY(texArray), SAMPLER(texArraySampler), half4 defaultColor)
{
    float texArrIndex;
    float4 texTransform;
    GetTextureInfo(textureIndex, materialIndex, texArrIndex, texTransform);
    return texArrIndex >= 0.0f ? SAMPLE_TEXTURE2D_ARRAY_LOD(texArray, texArraySampler, uv.xy * texTransform.zw + texTransform.xy, texArrIndex, uv.z) : defaultColor;
}

#endif

inline half Remap_Half(half input, half2 inMinMax, half2 outMinMax)
{
    return outMinMax.x + (input - inMinMax.x) * (outMinMax.y - outMinMax.x) / (inMinMax.y - inMinMax.x);
}

inline half3 UnpackNormalXYScale(half2 normalTS_XY, half scale)
{
    normalTS_XY = mad(normalTS_XY,2.0h, -1.0h);
    half normalTS_Z = max(1.0e-16, sqrt(1.0 - saturate(dot(normalTS_XY, normalTS_XY))));
    normalTS_XY *= scale;
    return half3(normalTS_XY, normalTS_Z);
}

//https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/
inline half3 ACESFilm(in half3 x){
    return saturate((x * (2.51f * x + 0.03f)) / (x * (2.43f * x + 0.59f) + 0.14f));
}

// Do every local post processing here
half4 DoPostProcessing(in half3 color,  in half aces, in half alpha = 1.0h)
{
    half3 acesColor = ACESFilm(color.rgb * 0.6);
    color.rgb = lerp(color.rgb,acesColor,aces);
    return half4(color,alpha); 
}

inline half3 ShiftHSV(half3 hsv, half4 hsvAlpha)
{
    return half3(hsv.x + hsvAlpha.x, (hsv.yz * hsvAlpha.yz));
}

inline half3 ShiftTextureColor(half3 texColor, half3 color, float2 uv, half4 HSVAlpha1, half4 HSVAlpha2, half4 HSVAlpha3, half4 HSVAlpha4, half useAlbedoHue)
{
#ifdef PAV_COLOR_REGION_BAKED
    return texColor * color;
#else

#if defined(_ENABLE_STATIC_MESH_BATCHING)
    half4 colorMask = SampleTexArray(1, mtlIndex, uv, _ColorRegionMapArray, sampler_ColorRegionMapArray, half4(0.0f, 0.0f, 0.0f, 0.0f));
#else
    half4 colorMask = SAMPLE_TEXTURE2D(_ColorRegionMap, sampler_ColorRegionMap, uv);
#endif
    texColor = LinearToGamma22(texColor.rgb);
    half3 hsv = RgbToHsv(texColor);
    hsv.r *= useAlbedoHue;
    half3 swiftColor = lerp(texColor,saturate(HsvToRgb(ShiftHSV(hsv, HSVAlpha1))), saturate(colorMask.x * HSVAlpha1.w));
    swiftColor = lerp(swiftColor,saturate(HsvToRgb(ShiftHSV(hsv, HSVAlpha2))), saturate(colorMask.y * HSVAlpha2.w));
    swiftColor = lerp(swiftColor,saturate(HsvToRgb(ShiftHSV(hsv, HSVAlpha3))), saturate(colorMask.z * HSVAlpha3.w));
    swiftColor = lerp(swiftColor,saturate(HsvToRgb(ShiftHSV(hsv, HSVAlpha4))), saturate(colorMask.w * HSVAlpha4.w));
    swiftColor.rgb = Gamma22ToLinear(swiftColor.rgb);
    return swiftColor * color;
#endif
}

#endif