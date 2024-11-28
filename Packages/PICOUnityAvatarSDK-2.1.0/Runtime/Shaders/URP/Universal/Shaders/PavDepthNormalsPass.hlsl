#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED

#include "../ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionOS     : POSITION;
    float4 tangentOS      : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float3 normal       : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    PAV_VERTEX_ID
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float3 uv           : TEXCOORD1;
    float3 normalWS                 : TEXCOORD2;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings DepthNormalsVertex(Attributes input)
{
    // replace vertex.
#ifdef PAV_NO_TANGENTS
    PAV_GET_VERTEX_P(input.vid, input.positionOS);
#else
    float3 normal;
    PAV_GET_VERTEX_PNT(input.vid, input.positionOS, normal, input.tangentOS);
#endif

    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv.xy         = TRANSFORM_TEX(input.texcoord, _BaseMap);
    PAV_GET_MATERIAL_INDEX(input.vid, output.uv);
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangentOS);
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);

    return output;
}

float4 DepthNormalsFragment(Varyings input) : SV_TARGET
{
    PAV_FLIP_UV_Y(input.uv);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    PAV_GET_MATERIAL_DATA(input.uv.z);
    PAV_GET_CUTOFF(cutoff);
    PAV_GET_BASE_COLOR(baseColor);

    Alpha(PAV_SAMPLE_ALBEDO_ALPHA(input.uv.xy).a, baseColor, cutoff);
    return float4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS, true)), 0.0, 0.0);
}
#endif
