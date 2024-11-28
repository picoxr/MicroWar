#ifndef PAV_TOON_OUTLINE_PASS_INCLUDED
#define PAV_TOON_OUTLINE_PASS_INCLUDED

#include "../ShaderLibrary/Core.hlsl"


CBUFFER_START(UnityPerMaterial)
half _AdditiveGI;
float _Outline;
float4 _OutlineColor;
CBUFFER_END

struct Attributes 
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    PAV_VERTEX_ID
};

struct Varyings 
{
    float4 positionCS : SV_POSITION;
    half fogCoord : TEXCOORD0;
    half4 color : COLOR;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings ToonOutlineVertex(Attributes input) 
{
    // replace vertex.
    PAV_GET_VERTEX_PN(input.vid, input.positionOS, input.normalOS);

    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 materialIndex = 0;
    PAV_GET_MATERIAL_INDEX(input.vid, materialIndex);
    PAV_GET_MATERIAL_DATA(materialIndex.z);
    PAV_GET_OUTLINE(outline);
    PAV_GET_OUTLINE_COLOR(outlineColor);

    input.positionOS.xyz += input.normalOS.xyz * outline;
    
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = vertexInput.positionCS;
    
    output.color = outlineColor;
    output.fogCoord = ComputeFogFactor(output.positionCS.z);
    return output;
}

half4 ToonOutlineFragment(Varyings i) : SV_Target
{
	i.color.rgb = MixFog(i.color.rgb, i.fogCoord);
	return i.color;
}



#endif
