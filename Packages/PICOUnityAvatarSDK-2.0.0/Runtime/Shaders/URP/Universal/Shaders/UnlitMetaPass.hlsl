#ifndef UNIVERSAL_UNLIT_META_PASS_INCLUDED
#define UNIVERSAL_UNLIT_META_PASS_INCLUDED

#include "../../PavConfig.hlsl"
#include "../ShaderLibrary/MetaInput.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 uv0          : TEXCOORD0;
    float2 uv1          : TEXCOORD1;
    float2 uv2          : TEXCOORD2;
#ifdef _TANGENT_TO_WORLD
    float4 tangentOS     : TANGENT;
#endif
    PAV_VERTEX_ID
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float3 uv           : TEXCOORD0;
};

Varyings UniversalVertexMeta(Attributes input)
{
    // replace vertex
    PAV_GET_VERTEX_PN(input.vid, input.positionOS, input.normalOS);

    Varyings output;
    output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2,
        unity_LightmapST, unity_DynamicLightmapST);
    output.uv.xy = TRANSFORM_TEX(input.uv0, _BaseMap);
    PAV_GET_MATERIAL_INDEX(input.vid, output.uv);
    return output;
}

half4 UniversalFragmentMetaUnlit(Varyings input) : SV_Target
{
    MetaInput metaInput = (MetaInput)0;

    PAV_GET_MATERIAL_DATA(input.uv.z);
    PAV_GET_BASE_COLOR(baseColor);
    PAV_GET_SHADER_TYPE(shaderType);

    metaInput.Albedo = ApplyAlbedo(PAV_SAMPLE_ALBEDO_ALPHA(input.uv.xy).rgb, baseColor.rgb, shaderType);

    return MetaFragment(metaInput);
}

//LWRP -> Universal Backwards Compatibility
Varyings LightweightVertexMeta(Attributes input)
{
    return UniversalVertexMeta(input);
}

half4 LightweightFragmentMetaUnlit(Varyings input) : SV_Target
{
    return UniversalFragmentMetaUnlit(input);
}

#endif
