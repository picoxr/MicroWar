#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Lighting.hlsl"

// GLES2 has limited amount of interpolators
#if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;
#if defined(_SECOND_BASEMAP)
    float2 uv2          : TEXCOORD2;
#endif
#if defined(_ENABLE_STATIC_MESH_BATCHING)
    float2 mtlIndex     : TEXCOORD4;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    PAV_VERTEX_ID
};

struct Varyings
{
    float3 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD2;
#endif

    float3 normalWS                 : TEXCOORD3;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
#endif
    float3 viewDirWS                : TEXCOORD5;

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    float3 viewDirTS                : TEXCOORD8;
#endif

#if defined(_SECOND_BASEMAP)
    float2 uv2                      : TEXCOORD9;
#endif

#if defined(_ENABLE_STATIC_MESH_BATCHING)
    nointerpolation float2 mtlIndex : TEXCOORD10;
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = SafeNormalize(input.viewDirWS);
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
#else
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    //inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.bakedGI = SampleSH(inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    // replace vertex.
#ifdef PAV_NO_TANGENTS
    PAV_GET_VERTEX_PN(input.vid, input.positionOS, input.normalOS);

#   if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
    PAV_GPUINSTANCE_SKIN_PNT(unity_InstanceID, input.vid, input.positionOS, input.normalOS);
#   endif
#else
    PAV_GET_VERTEX_PNT(input.vid, input.positionOS,  input.normalOS, input.tangentOS);

#   if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
    PAV_GPUINSTANCE_SKIN_PNT(unity_InstanceID, input.vid, input.positionOS, input.normalOS, input.tangentOS);
#   endif
#endif



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

    output.uv.xy = TRANSFORM_TEX(input.texcoord, _BaseMap);
    PAV_GET_MATERIAL_INDEX(input.vid, output.uv);

#if defined(_SECOND_BASEMAP)
    output.uv2 = input.uv2;
#endif

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

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

#if defined(_ENABLE_STATIC_MESH_BATCHING)
    output.mtlIndex = input.mtlIndex;
#endif

    return output;
}

// Temporarily add a rim profile.
//#define PAV_RIM_PROFILE
#ifdef PAV_RIM_PROFILE
float3 ProfileRim(float3 normalWS, float3 viewDirWS)
{
    //下面代码使Android上看不到
    //#ifdef  MacroUseDoubleFacedLighting
    //		if (!s.frontSide) return 0;
    //#endif
    float NoV = dot(normalWS, viewDirWS);
    float t = saturate(1 - NoV);

    return  pow(t, 4.1);
    //t = s.rimCurvature.x * pow(t, _ProfileRimPow);
    //float rimAmplify = (_ProfileRimColor.a - 0.5) * 2.0;

    //float rimAmplify = 1.0;
    //float3 rimScatter = 0;

    //_ProfileRimColor.a 小于0.5采用正片叠底。 否则是加亮模式.
    //if (rimAmplify < 0.0)
    //{
    //    // 如果小于0.5，施加到正片叠底通道.
    //    s.multiplyColor.rgb = lerp(1, _ProfileRimColor.rgb, t * -rimAmplify);
    //    //s.multiplyColor.rgb = t * -rimAmplify;
    //    return 0;
    //}
    //else
    //{
        //rimScatter = t * rimAmplify * _ProfileRimColor.rgb;// *roughness;// *roughness;// *roughness * roughness;
    //}

//#	ifdef MacroProfileInnerRim
//    half secondRimScatter = s.rimCurvature.x * _ProfileInnerRimColor.a * pow(t, _ProfileInnerRimPow);// *roughness;// *roughness;// *roughness * roughness;
//    rimScatter.rgb += secondRimScatter * _ProfileInnerRimColor.rgb;
//#	endif
    //
    //return rimScatter;
}
#endif

// float _RampScale: _CustomVec_0.x
// float _IridInnerPow: _CustomVec_0.y
// float _IridOutPow: _CustomVec_0.z
TEXTURE2D(_LaserRampNoise); SAMPLER(sampler_LaserRampNoise);
TEXTURE2D(_LaserColorRamp); SAMPLER(sampler_LaserColorRamp);

float3 Laser(float2 uv, float nDotv, float3 Albedo)
{
    PAV_GET_CUSTOM_VECTOR(customVec0, 0);
    float _RampScale = customVec0.x;
    float _IridInnerPow = customVec0.y;
    float _IridOutPow = customVec0.z;

    float2 Noise = SAMPLE_TEXTURE2D(_LaserRampNoise, sampler_LaserRampNoise, uv).xy * _RampScale;
    float2 NoiseUV = Noise + nDotv;
    float3 col1 = SAMPLE_TEXTURE2D(_LaserColorRamp, sampler_LaserColorRamp, NoiseUV).xyz * 0.5;
    float IridInnerPow = _IridInnerPow + -nDotv;
    float a59 = -nDotv + 1.0;
    float a57 = a59 + IridInnerPow;
    a59 *= _IridOutPow;
    float3 col0 = col1 * Albedo + col1;
    col0 = col0 * (1.0 + a57) * a59 + Albedo;
    return col0;
}

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    PAV_FLIP_UV_Y(input.uv);
#ifdef _SECOND_BASEMAP
    PAV_FLIP_UV_Y(input.uv2);
#endif
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    PAV_GET_SECOND_UV(input);

#ifdef _ENABLE_STATIC_MESH_BATCHING
    mtlIndex = (uint)input.mtlIndex.x;
    currentData.u1 = _MtlData[mtlIndex].uniform1;
    currentData.u2 = _MtlData[mtlIndex].uniform2;
    float shaderType = currentData.u1.x;

#else
    PAV_GET_MATERIAL_DATA(input.uv.z);
    PAV_GET_SHADER_TYPE(shaderType);

#endif

#if defined(_PARALLAXMAP)
#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
#else
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, input.viewDirWS);
#endif
    PAV_APPLY_PER_PIXEL_DISPLACEMENT(viewDirTS, input.uv);
#endif

    SurfaceData surfaceData;
    PAV_INIT_SURFACE_DATA(input.uv, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

    // laser
    if (shaderType == PAV_SHADER_TYPE_CLOTH_LASER)
    {
        half NoV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
        surfaceData.albedo = Laser(input.uv, NoV, surfaceData.albedo);
    }

    half4 color = UniversalFragmentPBR(inputData, surfaceData);

    // add extra rim profile.
#ifdef PAV_RIM_PROFILE
    {
        float rim = ProfileRim(inputData.normalWS, inputData.viewDirectionWS) * 0.15;
        // lower part remove rim profile.
        float rimFadeRange = 0.7;
        float rimFadeStartY = 0.3;
        float upperRim = saturate((input.positionWS.y - rimFadeStartY) / rimFadeRange); 
        color.rgb += rim * upperRim;
    }
#endif

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);


    //color.rgb = surfaceData.albedo.rgb;

//    float4 ret = float4(0,0,0,1);
////
//#ifdef UNITY_STEREO_INSTANCING_ENABLED
//    ret.r = 1.0;
//#endif
//
//#ifdef UNITY_SINGLE_PASS_STEREOß
//    ret.g = 1.0;
//#endif
//
//#ifdef STEREO_INSTANCING_ON
//    ret.b = 1.0;
//#endif
//
//    return ret;
//    return lerp(float4(1,0,0,1), float4(0,1,0,1), unity_StereoEyeIndex.x);

    // debug.
    // color.rgb = inputData.bakedGI;
    //color.rgb = SampleSH(inputData.normalWS).rgb;
    //color.r = surfaceData.metallic;//_Metallic;
    //color.g = surfaceData.smoothness;
    //color.b = 0;
    return color;
}

#endif
