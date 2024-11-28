#ifndef UNIVERSAL_SIMPLE_LIT_PASS_INCLUDED
#define UNIVERSAL_SIMPLE_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;
#if defined(_SECOND_BASEMAP)
    float2 uv2           : TEXCOORD2;
#endif
#if defined(_ENABLE_STATIC_MESH_BATCHING)
    float2 mtlIndex     : TEXCOORD4;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    PAV_VERTEX_ID
};

#ifdef PAV_AVATAR_LOD_OUTLINE

struct Varyings
{
    //float4 lodOutline                  : TEXCOORD0;

    float4 positionCS                  : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

#elif PAV_AVATAR_EXTRUDE_OUTLINE

struct Varyings
{
    float4 outlineColor                : COLOR0;

    float4 positionCS                  : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

#else

struct Varyings
{
    float3 uv                       : TEXCOORD0;
    
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

#ifdef _NORMALMAP
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    float3  normal                  : TEXCOORD3;
    float3 viewDir                  : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

#if defined(_SECOND_BASEMAP)
    float2 uv2                      : TEXCOORD8;
#endif

#if defined(_ENABLE_STATIC_MESH_BATCHING)
    nointerpolation float2 mtlIndex : TEXCOORD9;
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData.positionWS = input.posWS;

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
#else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

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
#endif

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

//    // replace vertex.
#ifdef PAV_NO_TANGENTS
    PAV_GET_VERTEX_PN(input.vid, input.positionOS, input.normalOS);

#   if (defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED))
    PAV_GPUINSTANCE_SKIN_PN(unity_InstanceID, input.vid, input.positionOS, input.normalOS);
#   endif
#else
    PAV_GET_VERTEX_PNT(input.vid, input.positionOS, input.normalOS, input.tangentOS);

#   if (defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED))
    PAV_GPUINSTANCE_SKIN_PNT(unity_InstanceID, input.vid, input.positionOS, input.normalOS, input.tangentOS);
#   endif
#endif

#ifdef PAV_AVATAR_LOD_OUTLINE
    
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(positionWS);

#elif PAV_AVATAR_EXTRUDE_OUTLINE

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 positionVS = TransformWorldToView(positionWS.xyz);

    float3 normalWS = TransformObjectToWorldDir(input.normalOS.xyz, false);
    float3 normalCS = TransformWorldToViewDir(normalWS, true);

    float normalizedDist = saturate(-positionVS.z / 8.0);
    positionVS += normalCS * lerp(0.002, 0.01, normalizedDist);
    output.positionCS = TransformWViewToHClip(positionVS.xyz);
    //output.positionCS /= output.positionCS.w;
    output.positionCS.z -= 0.00001;

    output.outlineColor.rgb = 1;
    output.outlineColor.a = 1 - normalizedDist * normalizedDist * normalizedDist;
    
#else//#ifdef PAV_AVATAR_LOD_OUTLINE

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

#ifdef _ENABLE_STATIC_MESH_BATCHING
    output.uv.xy = input.texcoord;
    output.mtlIndex = input.mtlIndex;
#else
    output.uv.xy = TRANSFORM_TEX(input.texcoord, _BaseMap); 
    PAV_GET_MATERIAL_INDEX(input.vid, output.uv);
#endif

#if defined(_SECOND_BASEMAP)
    output.uv2 = input.uv2;
#endif

    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

#ifdef _NORMALMAP
    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDir = viewDirWS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

#endif//#ifdef PAV_AVATAR_LOD_OUTLINE

    return output;
}

// Used for StandardSimpleLighting shader

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

half4 LitPassFragmentSimple(Varyings input) : SV_Target
{
    PAV_FLIP_UV_Y(input.uv);
#ifdef _SECOND_BASEMAP
    PAV_FLIP_UV_Y(input.uv2);
#endif

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#ifdef PAV_AVATAR_LOD_OUTLINE

    return 0.0;
    
    //return input.lodOutline;
#elif PAV_AVATAR_EXTRUDE_OUTLINE

    return input.outlineColor;

#else // #ifdef PAV_AVATAR_LOD_OUTLINE

#ifdef _ENABLE_STATIC_MESH_BATCHING
    mtlIndex = (uint)input.mtlIndex.x;
    currentData.u1 = _MtlData[mtlIndex].uniform1;
#else
    PAV_GET_MATERIAL_DATA(input.uv.z);
#endif
    PAV_GET_SECOND_UV(input);
    PAV_GET_CUTOFF(cutoff);
    PAV_GET_BASE_COLOR(baseColor);
    PAV_GET_SPEC_COLOR(specColor);
    PAV_GET_EMISSION_COLOR(emissionColor);
    PAV_GET_SHADER_TYPE(shaderType);
    PAV_GET_USING_ALBEDO_HUE(usingAlbedoHue);

    float2 uv = input.uv.xy;
    half4 diffuseAlpha = PAV_SAMPLE_ALBEDO_ALPHA(uv);
    half4 colorRegions = PAV_SAMPLE_COLOR_REGIONS(uv);
    PAV_GET_BASE_COLOR_MASK1(colorMask1);
    PAV_GET_BASE_COLOR_MASK2(colorMask2);
    PAV_GET_BASE_COLOR_MASK3(colorMask3);
    PAV_GET_BASE_COLOR_MASK4(colorMask4);
    half3 diffuse = ApplyAlbedo(diffuseAlpha.rgb, baseColor.rgb, shaderType, colorRegions, colorMask1, colorMask2, colorMask3, colorMask4, usingAlbedoHue); // need fix
    half alpha = diffuseAlpha.a * baseColor.a;
    AlphaDiscard(alpha, cutoff);

    #ifdef _ALPHAPREMULTIPLY_ON
        diffuse *= alpha;
    #endif

    half3 normalTS = PAV_SAMPLE_NORMAL(uv, 1.0h);
    half3 emission = PAV_SAMPLE_EMISSION(uv, emissionColor.rgb);
    half4 specular = PAV_SAMPLE_SPECULAR_SMOOTHNESS(uv, alpha, specColor);
    half smoothness = specular.a;

    half occlusion = 1.0;//SampleOcclusion(uv);

#ifdef PAV_ToonShadowMap
    half4 toonShadowAlbedo = PAV_SAMPLE_TOON_SHADOW(uv);
 #endif

    InputData inputData;
    InitializeInputData(input, normalTS, inputData);
 
    // laser
    if (shaderType == PAV_SHADER_TYPE_CLOTH_LASER)
    {
        half NoV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
        diffuse = Laser(input.uv, NoV, diffuse);
    }

    half4 color = UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha, occlusion
	#ifdef PAV_ToonShadowMap
		, toonShadowAlbedo
	 #endif
	);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);

    //
    //color.rgb = inputData.bakedGI;
    return color;
    //PAV_GET_CUSTOM_VECTOR(customVec, 0);
    //return customVec;
#endif //#ifdef PAV_AVATAR_LOD_OUTLINE
}

half4 LitPassFragmentSimplest(Varyings input) : SV_Target
{
    PAV_FLIP_UV_Y(input.uv);

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if _ENABLE_STATIC_MESH_BATCHING
    mtlIndex = (uint)input.mtlIndex.x;
#endif
    float2 uv = input.uv.xy;

    PAV_GET_BASE_COLOR(baseColor);

    half4 diffuseAlpha = PAV_SAMPLE_ALBEDO_ALPHA(uv);
    half4 diffuse = diffuseAlpha * baseColor;// ApplyAlbedo(diffuseAlpha.rgb, baseColor.rgb, shaderType, colorRegions, _ColorRegion1, _ColorRegion2, _ColorRegion3, _ColorRegion4, usingAlbedoHue); // need fix

    half3 bakedGI = SampleSH(input.normal);

#ifdef PAV_HAS_ADDITIVE_GI
    // add extra gi for some dark scene.
    bakedGI += _AdditiveGI;
#endif

    diffuse.rgb = (bakedGI + _MainLightColor.rgb * dot(_MainLightPosition.xyz, input.normal)) * diffuse.rgb;
    return diffuse;
}

#endif
