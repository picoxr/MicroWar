#include "../../GpuSkinning/GPUSkin.cginc"

sampler2D _MainTex;
sampler2D _MetallicGlossMap;
sampler2D _BumpMap;
sampler2D _ColorRegionMap;

//half4 BaseMapArray(float3 uv);
//half4 MetallicMapArray(float3 uv);
//half4 BumpMapArray(float3 uv);

UNITY_DECLARE_TEX2DARRAY(_BaseMapArray);
UNITY_DECLARE_TEX2DARRAY(_MetallicGlossMapArray);
UNITY_DECLARE_TEX2DARRAY(_BumpMapArray);
UNITY_DECLARE_TEX2DARRAY(_ColorRegionMapArray);

sampler2D _CustomMap_0;
sampler2D _CustomMap_1;
sampler2D _CustomMap_2;
sampler2D _CustomMap_3;
sampler2D _CustomMap_4;
sampler2D _CustomMap_5;
sampler2D _CustomMap_6;
sampler2D _CustomMap_7;
sampler2D _CustomMap_8;
sampler2D _CustomMap_9;
sampler2D _CustomMap_10;
sampler2D _CustomMap_11;
sampler2D _CustomMap_12;
sampler2D _CustomMap_13;
sampler2D _CustomMap_14;
sampler2D _CustomMap_15;
sampler2D _CustomMap_16;

UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_0);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_1);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_2);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_3);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_4);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_5);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_6);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_7);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_8);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_9);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_10);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_11);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_12);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_13);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_14);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_15);
UNITY_DECLARE_TEX2DARRAY(_CustomMapArray_16);

uniform float4 _CustomVec_0;
uniform float4 _CustomVec_1;
uniform float4 _CustomVec_2;
uniform float4 _CustomVec_3;
uniform float4 _CustomVec_4;
uniform float4 _CustomVec_5;
uniform float4 _CustomVec_6;
uniform float4 _CustomVec_7;
uniform float4 _CustomVec_8;



half4 BaseMapArray(float3 uv)
{
    half4 c = half4(1.0, 1.0, 1.0, 1.0);
    if (uv.z >= 0)
    {
        c = UNITY_SAMPLE_TEX2DARRAY(_BaseMapArray, uv);
    }
    return c;
}

half4 MetallicMapArray(float3 uv)
{
    half4 c = half4(1.0, 1.0, 1.0, 1.0);
    if (uv.z >= 0)
    {
        c = UNITY_SAMPLE_TEX2DARRAY(_MetallicGlossMapArray, uv);
    }
    return c;
}

half4 BumpMapArray(float3 uv)
{
    half4 c = half4(0.5, 0.5, 1.0, 0.5);
    if (uv.z >= 0)
    {
        c = UNITY_SAMPLE_TEX2DARRAY(_BumpMapArray, uv);
    }
    return c;
}

#ifdef PAV_MERGED_TEXTURE

void GetMergedTextureUVFrag(inout float2 uv, out float textureArrayIndex, uint textureIndex)
{
    float4 uvScaleOffset;
    GetTextureInfo(textureIndex, textureArrayIndex, uvScaleOffset);
    uv = fmod(uv, 1.0);
    if (uv.x < 0) uv.x += 1.0;
    if (uv.y < 0) uv.y += 1.0;
    //PAV_FLIP_UV_Y(uv);
    uv = uv * uvScaleOffset.xy + uvScaleOffset.zw;
}

half4 SampleAlbedoAlpha(float2 uv)
{
    float textureArrayIndex; 
    GetMergedTextureUVFrag(uv, textureArrayIndex, PAV_TEXTURE_KEY_BASE_MAP);
    return BaseMapArray(float3(uv, textureArrayIndex));
}


half4 SampleMetallicGlossMap(float2 uv)
{
    float textureArrayIndex = 0;
    GetMergedTextureUVFrag(uv, textureArrayIndex ,PAV_TEXTURE_KEY_METALLIC_GLOSS_MAP);
    return MetallicMapArray(float3(uv, textureArrayIndex));
}

half4 SampleBumpMap(float2 uv)
{
    float textureArrayIndex = 0;
    GetMergedTextureUVFrag(uv, textureArrayIndex, PAV_TEXTURE_KEY_BUMP_MAP);
    return BumpMapArray(float3(uv, textureArrayIndex));
}



#define PAV_SAMPLE_CUSTOM_MAP_FUNC(index) \
    half4 SampleCustomMap_##index(float2 uv) \
    { \
        float textureArrayIndex = 0; \
        GetMergedTextureUVFrag(uv, textureArrayIndex, PAV_TEXTURE_KEY_CUSTOM_MAP_0 + index); \
        half4 c = half4(1.0, 1.0, 1.0, 1.0); \
        if (textureArrayIndex >= 0) \
        { \
            c = UNITY_SAMPLE_TEX2DARRAY(_CustomMapArray_##index, float3(uv, textureArrayIndex)); \
        } \
        return c; \
    }

#define PAV_GET_MATERIAL_DATA(materialIndex)  _mergedMaterialData = GetMaterialData((uint) round(materialIndex))
#define PAV_GET_OUTLINE(v) float v = _mergedMaterialData.outline
#ifdef _ALPHATEST_ON
#   define PAV_GET_CUTOFF(v) float v = _mergedMaterialData.cutoff
#else
#   define PAV_GET_CUTOFF(v) float v = 1.0
#endif
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

#define PAV_SAMPLE_ALBEDO_ALPHA(uv) SampleAlbedoAlpha(uv)
#define PAV_SAMPLE_METALLIC_GLOSS_MAP(uv) SampleMetallicGlossMap(uv)
#define PAV_SAMPLE_BUMP_MAP(uv) SampleBumpMap(uv)
#else

half4 SampleAlbedoAlpha(float2 uv)
{
    return tex2D(_MainTex, uv);
}

half4 SampleMetallicGlossMap(float2 uv)
{
    return tex2D(_MetallicGlossMap, uv);
}

half4 SampleBumpMap(float2 uv)
{
    return tex2D(_BumpMap, uv);
}

half4 SampleColorRegionsMap(float2 uv)
{
    //PAV_FLIP_UV_Y(uv);
    half4 c = tex2D(_ColorRegionMap,uv);

    return c;
}

#define PAV_SAMPLE_CUSTOM_MAP_FUNC(index) \
    half4 SampleCustomMap_##index(float2 uv) \
    { \
        return tex2D(_CustomMap_##index, uv); \
    }

#define PAV_GET_MATERIAL_DATA(index)
#define PAV_GET_OUTLINE(v) float v = _Outline

#ifdef _ALPHATEST_ON
#   define PAV_GET_CUTOFF(v) float v = _Cutoff
#else
#   define PAV_GET_CUTOFF(v) float v = 1.0
#endif
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
#define PAV_GET_BASE_COLOR(v) half4 v = _Color
#define PAV_GET_SPEC_COLOR(v) half4 v = _SpecColor
#define PAV_GET_EMISSION_COLOR(v) half4 v = _EmissionColor
#define PAV_GET_BASE_MAP_ST(v) half4 v = _BaseMap_ST
#define PAV_GET_BASE_COLOR_MASK1(v) half4 v = _ColorRegion1
#define PAV_GET_BASE_COLOR_MASK2(v) half4 v = _ColorRegion2
#define PAV_GET_BASE_COLOR_MASK3(v) half4 v = _ColorRegion3
#define PAV_GET_BASE_COLOR_MASK4(v) half4 v = _ColorRegion4
#define PAV_GET_CUSTOM_VECTOR(v, index) half4 v = _CustomVec_##index



// UnityStandardInput.cginc
//half4       _Color;
//half        _Cutoff;
//float4      _MainTex_ST;

//sampler2D   _DetailAlbedoMap;
//float4      _DetailAlbedoMap_ST;

//half        _BumpScale;

//sampler2D   _DetailMask;
//sampler2D   _DetailNormalMap;
//half        _DetailNormalMapScale;

//sampler2D   _SpecGlossMap;
//half        _Metallic;
//float       _Glossiness;
//float       _GlossMapScale;

//sampler2D   _OcclusionMap;
//half        _OcclusionStrength;

//sampler2D   _ParallaxMap;
//half        _Parallax;
//half        _UVSec;

//half4       _EmissionColor;
//sampler2D   _EmissionMap;

//half        _ShaderType;

// URP side properties
//UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
//UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
//UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
//UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
//UNITY_DOTS_INSTANCED_PROP(float , _Smoothness)
//UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
//UNITY_DOTS_INSTANCED_PROP(float , _BumpScale)
//UNITY_DOTS_INSTANCED_PROP(float , _Parallax)
//UNITY_DOTS_INSTANCED_PROP(float , _OcclusionStrength)
//UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatMask)
//UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatSmoothness)
//UNITY_DOTS_INSTANCED_PROP(float , _ShaderType)
//UNITY_DOTS_INSTANCED_PROP(float , _DetailAlbedoMapScale)
//UNITY_DOTS_INSTANCED_PROP(float , _DetailNormalMapScale)
//UNITY_DOTS_INSTANCED_PROP(float , _Surface)

#define PAV_SAMPLE_ALBEDO_ALPHA(uv) SampleAlbedoAlpha(uv)
#define PAV_SAMPLE_METALLIC_GLOSS_MAP(uv) SampleMetallicGlossMap(uv)
#define PAV_SAMPLE_BUMP_MAP(uv) SampleBumpMap(uv)
#ifdef PAV_COLOR_REGION_BAKED
#   define PAV_SAMPLE_COLOR_REGIONS(uv) 0
#else
#   define PAV_SAMPLE_COLOR_REGIONS(uv) SampleColorRegionsMap(uv)
#endif

#endif

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
