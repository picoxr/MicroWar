// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"
#include "../PavBuiltInConfig.hlsl"
#include "SurfaceInput.hlsl"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
    #define _DETAIL 1
#endif

 // amplify to adjust baked albedo texture.
float       _BaseColorAmplify;

//---------------------------------------
half4       _Color;
half        _Cutoff;

float       _UsingAlbedoHue;
half4       _ColorRegion1;
half4       _ColorRegion2;
half4       _ColorRegion3;
half4       _ColorRegion4;

float4      _MainTex_ST;

sampler2D   _DetailAlbedoMap;
float4      _DetailAlbedoMap_ST;

half        _BumpScale;

sampler2D   _DetailMask;
sampler2D   _DetailNormalMap;
half        _DetailNormalMapScale;

sampler2D   _SpecGlossMap;
half        _Metallic;
float       _Glossiness;
float       _GlossMapScale;

sampler2D   _OcclusionMap;
half        _OcclusionStrength;

sampler2D   _ParallaxMap;
half        _Parallax;
half        _UVSec;

half4       _EmissionColor;
sampler2D   _EmissionMap;

half        _ShaderType;

float4      _BaseMap_ST;


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
   
//UNITY_DECLARE_TEX2DARRAY(_BaseMapArray);
//UNITY_DECLARE_TEX2DARRAY(_MetallicGlossMapArray);
//UNITY_DECLARE_TEX2DARRAY(_BumpMapArray);

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
    half4 tangent   : TANGENT;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    PAV_VERTEX_ID
};

float4 TexCoords(VertexInput v)
{
    float4 texcoord;
    texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
    texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
    // PAV_FLIP_UV_Y(texcoord.xy);
    // PAV_FLIP_UV_Y(texcoord.zw);
    return texcoord;
}

half DetailMask(float2 uv)
{
    return tex2D (_DetailMask, uv).a;
}

half3 Albedo(float4 texcoords)
{
    half3 albedo = _Color.rgb * PAV_SAMPLE_ALBEDO_ALPHA(texcoords.xy).rgb;
#if _DETAIL
    #if (SHADER_TARGET < 30)
        // SM20: instruction count limitation
        // SM20: no detail mask
        half mask = 1;
    #else
        half mask = DetailMask(texcoords.xy);
    #endif
    half3 detailAlbedo = tex2D (_DetailAlbedoMap, texcoords.zw).rgb;
    #if _DETAIL_MULX2
        albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
    #elif _DETAIL_MUL
        albedo *= LerpWhiteTo (detailAlbedo, mask);
    #elif _DETAIL_ADD
        albedo += detailAlbedo * mask;
    #elif _DETAIL_LERP
        albedo = lerp (albedo, detailAlbedo, mask);
    #endif
#endif
    return albedo;
}

half Alpha(float2 uv)
{
#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
    return _Color.a;
#else
    return PAV_SAMPLE_ALBEDO_ALPHA(uv).a * _Color.a;
#endif
}

half3 ApplyAlbedo(half3 albedo, half3 baseColor, half shaderType)
{
    half3 result;
    half3 a = albedo;
    half3 b = baseColor;

    uint shaderTypeI = (uint) round(shaderType);

    // modified by dujing and tianshengcai
    if (//shaderTypeI == PAV_SHADER_TYPE_BODY_BASE 
        shaderTypeI == PAV_SHADER_TYPE_HAIR_BASE
        //shaderTypeI == PAV_SHADER_TYPE_CLOTH_BASE
        )
    {
        //
        // albedo = dot(albedo, half3(0.2126729, 0.7151522, 0.0721750));
        albedo = pow(albedo, 1.0/2.2);
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
    uint shaderTypeI = (uint) round(shaderType);

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

half Occlusion(float2 uv)
{
#if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
    // SM20: simpler occlusion
    return tex2D(_OcclusionMap, uv).g;
#else
    half occ = tex2D(_OcclusionMap, uv).g;
    return LerpOneTo (occ, _OcclusionStrength);
#endif
}

half4 SpecularGloss(float2 uv)
{
    half4 sg;
#ifdef _SPECGLOSSMAP
    #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
        sg.rgb = tex2D(_SpecGlossMap, uv).rgb;
        sg.a = PAV_SAMPLE_ALBEDO_ALPHA(uv).a;
    #else
        sg = tex2D(_SpecGlossMap, uv);
    #endif
    sg.a *= _GlossMapScale;
#else
    sg.rgb = _SpecColor.rgb;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        sg.a = PAV_SAMPLE_ALBEDO_ALPHA(uv).a * _GlossMapScale;
    #else
        sg.a = _Glossiness;
    #endif
#endif
    return sg;
}

half2 MetallicGloss(float2 uv)
{
    half2 mg;

#ifdef _METALLICGLOSSMAP
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.r = PAV_SAMPLE_METALLIC_GLOSS_MAP(uv).r;
        mg.g = PAV_SAMPLE_ALBEDO_ALPHA(uv).a ;
    #else
        mg = PAV_SAMPLE_METALLIC_GLOSS_MAP(uv).ra;
    #endif
    mg.g *= _GlossMapScale;
#else
    mg.r = _Metallic;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.g = PAV_SAMPLE_ALBEDO_ALPHA(uv).a * _GlossMapScale;
    #else
        mg.g = _Glossiness;
    #endif
#endif
    return mg;
}

half2 MetallicRough(float2 uv)
{
    half2 mg;
#ifdef _METALLICGLOSSMAP
    mg.r = PAV_SAMPLE_METALLIC_GLOSS_MAP(uv).r;
#else
    mg.r = _Metallic;
#endif

#ifdef _SPECGLOSSMAP
    mg.g = 1.0f - tex2D(_SpecGlossMap, uv).r;
#else
    mg.g = 1.0f - _Glossiness;
#endif
    return mg;
}

half3 Emission(float2 uv)
{
#ifndef _EMISSION
    return 0;
#else
    return tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
#endif
}

//half4 BaseMapArray(float3 uv)
//{
//    half4 c = half4(1.0, 1.0, 1.0, 1.0);
//    if (uv.z >= 0)
//    {
//        c = UNITY_SAMPLE_TEX2DARRAY(_BaseMapArray, uv);
//    }
//    return c;
//}

//half4 MetallicMapArray(float3 uv)
//{
//    half4 c = half4(1.0, 1.0, 1.0, 1.0);
//    if (uv.z >= 0)
//    {
//        c = UNITY_SAMPLE_TEX2DARRAY(_MetallicGlossMapArray, uv);
//    }
//    return c;
//}

//half4 BumpMapArray(float3 uv)
//{
//    half4 c = half4(0.5, 0.5, 1.0, 0.5);
//    if (uv.z >= 0)
//    {
//        c = UNITY_SAMPLE_TEX2DARRAY(_BumpMapArray, uv);
//    }
//    return c;
//}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords)
{
    half3 normalTangent = UnpackScaleNormal(PAV_SAMPLE_BUMP_MAP(texcoords.xy), _BumpScale);

#if _DETAIL && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
    half mask = DetailMask(texcoords.xy);
    half3 detailNormalTangent = UnpackScaleNormal(tex2D (_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
    #if _DETAIL_LERP
        normalTangent = lerp(
            normalTangent,
            detailNormalTangent,
            mask);
    #else
        normalTangent = lerp(
            normalTangent,
            BlendNormals(normalTangent, detailNormalTangent),
            mask);
    #endif
#endif

    return normalTangent;
}
#endif

float4 Parallax (float4 texcoords, half3 viewDir)
{
#if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30)
    // Disable parallax on pre-SM3.0 shader target models
    return texcoords;
#else
    half h = tex2D (_ParallaxMap, texcoords.xy).g;
    float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
    return float4(texcoords.xy + offset, texcoords.zw + offset);
#endif

}

#endif // UNITY_STANDARD_INPUT_INCLUDED
