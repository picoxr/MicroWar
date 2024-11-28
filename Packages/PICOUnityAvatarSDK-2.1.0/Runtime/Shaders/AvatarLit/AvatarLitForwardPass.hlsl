#ifndef AVATAR_LIT_PASS_INCLUDED
#define AVATAR_LIT_PASS_INCLUDED

// GLES2 has limited amount of interpolators
#if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

#if defined(_MATCAP) && defined(_MATCAP_FIX_EDGE_FLAW)
#define REQUIRES_VIEW_SPACE_POSITION_INTERPOLATOR
#endif

#include "AvatarLighting.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;
#if defined(_ENABLE_STATIC_MESH_BATCHING)
    float2 mtlIndex     : TEXCOORD4;
#endif
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
    
    #if defined(REQUIRES_VIEW_SPACE_POSITION_INTERPOLATOR)
    float3 positionVS               : TEXCOORD6;
    #endif

    half4 fogFactorAndVertexLight   : TEXCOORD7; // x: fogFactor, yzw: vertex light

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD8;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    float3 viewDirTS                : TEXCOORD9;
    #endif

    #if defined(_ENABLE_STATIC_MESH_BATCHING)
    nointerpolation float2 mtlIndex : TEXCOORD10;
    #endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

inline void InitializeAdaptiveInputData(Varyings input,half3 normalWS, out AdaptiveInputData inputData)
{
    //多余的
    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
    #endif

    inputData.viewDirectionWS = SafeNormalize(input.viewDirWS);

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
    inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = SafeNormalize(input.viewDirWS);;
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

    #if defined(_NORMALMAP)
    inputData.tangentToWorld = tangentToWorld;
    #endif
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
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
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
}


///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
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

#if defined(_ENABLE_STATIC_MESH_BATCHING)
    float4 texTransform = _MtlData[(uint)input.mtlIndex.x].baseMapST;
    output.uv = input.texcoord * texTransform.xy + texTransform.zw;
#else
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
#endif

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
#if defined(_NORMALMAP)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#else
    half4 tangentWS = 0;
#endif
    output.tangentWS = tangentWS;
    

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VIEW_SPACE_POSITION_INTERPOLATOR)
    output.positionVS = TransformWorldToView(output.positionWS);
#endif
    
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

#if defined(_ENABLE_STATIC_MESH_BATCHING)
    output.mtlIndex = input.mtlIndex;
#endif

    output.positionCS = vertexInput.positionCS;

    return output;
}

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if defined(_ENABLE_STATIC_MESH_BATCHING)
    mtlIndex = (uint)input.mtlIndex.x;
    currentData.u1 = _MtlData[mtlIndex].uniform1;
    currentData.u2 = _MtlData[mtlIndex].uniform2;
    currentData.u3 = _MtlData[mtlIndex].uniform3;
#endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

    //_DEBUG_METALLIC _DEBUG_ROUGHNESS _DEBUG_NORMAL
    #ifdef _DEBUG_METALLIC
    return half4(surfaceData.metallic.rrr,1);
    #elif _DEBUG_ROUGHNESS
    return half4(surfaceData.smoothness.rrr,1);
    #elif _DEBUG_NORMAL
    return half4(inputData.normalWS,1.0);
    #endif

    #if defined(REQUIRES_VIEW_SPACE_POSITION_INTERPOLATOR)
    float3 positionVS = input.positionVS;
    #else
    float3 positionVS = (0.0).xxx;
    #endif
    
    
    #if defined(_PBR_FULL)
    half4 color = AvatarFullPBR(inputData, surfaceData, positionVS);
    #else
    half4 color = AvatarSimplePBR(inputData, surfaceData, positionVS);
    #endif
    
    //Debug Code
    #ifndef _DEBUG_NONE
        return color;
    #endif
    
    return color;
}

#endif
