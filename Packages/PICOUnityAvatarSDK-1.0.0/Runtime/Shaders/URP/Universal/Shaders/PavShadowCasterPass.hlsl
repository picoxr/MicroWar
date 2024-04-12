#ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
#define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED

#include "../ShaderLibrary/Core.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"

float3 _LightDirection;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    PAV_VERTEX_ID
};

struct Varyings
{
    float3 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
};

float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
    // replace vertex
    PAV_GET_VERTEX_PN(input.vid, input.positionOS, input.normalOS);

    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    output.uv.xy = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.uv.z = 0.0;
    PAV_GET_MATERIAL_INDEX(input.vid, output.uv);
    output.positionCS = GetShadowPositionHClip(input);
    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    PAV_FLIP_UV_Y(input.uv);
    PAV_GET_MATERIAL_DATA(input.uv.z);
    PAV_GET_CUTOFF(cutoff);
    PAV_GET_BASE_COLOR(baseColor);

    Alpha(PAV_SAMPLE_ALBEDO_ALPHA(input.uv.xy).a, baseColor, cutoff);
    return 0;
}

#endif
