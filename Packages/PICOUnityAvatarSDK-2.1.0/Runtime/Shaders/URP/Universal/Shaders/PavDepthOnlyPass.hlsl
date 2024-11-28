#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED

#include "../../PavConfig.hlsl"
#include "../ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionOS     : POSITION;
    float2 texcoord     : TEXCOORD0;
#if defined(_ENABLE_STATIC_MESH_BATCHING)
    float2 mtlIndex     : TEXCOORD4;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    PAV_VERTEX_ID
};

struct Varyings
{
    float3 uv           : TEXCOORD0;
#if defined(_ENABLE_STATIC_MESH_BATCHING)
    nointerpolation float2 mtlIndex : TEXCOORD10;
#endif

    float4 positionCS   : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings DepthOnlyVertex(Attributes input)
{
    // replace vertex.
    PAV_GET_VERTEX_P(input.vid, input.positionOS);


    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv.xy = TRANSFORM_TEX(input.texcoord, _BaseMap);
    PAV_GET_MATERIAL_INDEX(input.vid, output.uv);
#if defined(_ENABLE_STATIC_MESH_BATCHING)
    output.mtlIndex = input.mtlIndex;
#endif

    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

half4 DepthOnlyFragment(Varyings input) : SV_TARGET
{
    PAV_FLIP_UV_Y(input.uv);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#ifdef _ENABLE_STATIC_MESH_BATCHING    
    mtlIndex = (uint)input.mtlIndex.x;
    currentData.u1 = _MtlData[mtlIndex].uniform1;
#else
    PAV_GET_MATERIAL_DATA(input.uv.z);
#endif
    float2 uv = input.uv.xy;
    PAV_GET_CUTOFF(cutoff);
    PAV_GET_BASE_COLOR(baseColor);

    Alpha(PAV_SAMPLE_ALBEDO_ALPHA(uv).a, baseColor, cutoff);
    return 0;
}
#endif
