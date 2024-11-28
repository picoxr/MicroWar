#ifndef SKIN_LIT_PASS_INCLUDED
#define SKIN_LIT_PASS_INCLUDED

#include "SkinLighting.hlsl"

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
    float4 uv                       : TEXCOORD0; //xy for main texture and zw for detail map
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
    float3 positionWS               : TEXCOORD2;
    float3 normalWS                 : TEXCOORD3;
    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
    float3 viewDirWS                : TEXCOORD5;
    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

    #if defined(REQUIRES_VIEWSAPCE_NORMAL_COORD_INTERPOLATOR)
    float3 normalVS                 : TEXCOORD7;
    #endif
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD8;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    float3 viewDirTS                : TEXCOORD9;
    #endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

inline void InitializeAdaptiveInputData(Varyings input,half3 normalWS, out AdaptiveInputData inputData)
{
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

    #if defined(_RIM_LIGHT)
    inputData.rimNormalWS = NormalizeNormalPerPixel(input.normalWS);
        #if defined(REQUIRES_VIEWSAPCE_NORMAL_COORD_INTERPOLATOR)
        inputData.rimNormalVS = input.normalVS;
        #endif
    #endif
    
    
    
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

    output.uv = float4(TRANSFORM_TEX(input.texcoord, _BaseMap),TRANSFORM_TEX(input.texcoord, _DetailMap));

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

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

#if defined(REQUIRES_VIEWSAPCE_NORMAL_COORD_INTERPOLATOR)
    output.normalVS = mul((float3x3)UNITY_MATRIX_V, normalInput.normalWS);
#endif

    output.positionCS = vertexInput.positionCS;

#ifdef AVATAR_SKIN_OFFSET
    float offsetY = 1.0f / _ScreenParams.y;
#if UNITY_UV_STARTS_AT_TOP
    output.positionCS.y += offsetY * _SkinOffset;
#else
    output.positionCS.y -= offsetY * _SkinOffset;
#endif
#endif

    return output;
}

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    AdaptiveSurfaceData surfaceData;
    InitializeAdaptiveSurfaceData(input.uv,input.normalWS,input.tangentWS, surfaceData);

    AdaptiveInputData inputData;
    InitializeAdaptiveInputData(input, surfaceData.normalWS, inputData);
    
    half4 color = StylizedCharacterLighting(inputData, surfaceData);
    
    //Debug Code
    #ifndef _DEBUG_NONE
        return color;
    #endif
    
    ///TODO：
    ///Fog,emission is not support now.
    return color;
}

#endif
