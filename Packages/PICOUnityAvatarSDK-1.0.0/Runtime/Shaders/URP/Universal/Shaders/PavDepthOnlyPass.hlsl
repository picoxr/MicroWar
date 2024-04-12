#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED

#include "../../PavConfig.hlsl"
#include "../ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionOS     : POSITION;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    PAV_VERTEX_ID
};

struct Varyings
{
    float3 uv           : TEXCOORD0;
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
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

half4 DepthOnlyFragment(Varyings input) : SV_TARGET
{
    PAV_FLIP_UV_Y(input.uv);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    PAV_GET_MATERIAL_DATA(input.uv.z);
    PAV_GET_CUTOFF(cutoff);
    PAV_GET_BASE_COLOR(baseColor);

    Alpha(PAV_SAMPLE_ALBEDO_ALPHA(input.uv.xy).a, baseColor, cutoff);
    return 0;
}
#endif
