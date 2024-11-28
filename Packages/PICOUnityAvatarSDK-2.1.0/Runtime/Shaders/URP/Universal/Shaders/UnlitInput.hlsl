#ifndef UNIVERSAL_UNLIT_INPUT_INCLUDED
#define UNIVERSAL_UNLIT_INPUT_INCLUDED

#include "../../PavConfig.hlsl"

//#include "../../PavConfig.hlsl"
#include "../ShaderLibrary/Core.hlsl"
//#include "../../Core/ShaderLibrary/CommonMaterial.hlsl"
//
//#include "../../Core/ShaderLibrary/ParallaxMapping.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _Cutoff;
    half _Surface;
    half _ShaderType;
    half _AdditiveGI;
    float  _BaseColorAmplify;
    float _MipBias;
CBUFFER_END

#ifdef UNITY_DOTS_INSTANCING_ENABLED
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float , _Surface)
    UNITY_DOTS_INSTANCED_PROP(float, _ShaderType)
    UNITY_DOTS_INSTANCED_PROP(float , _AdditiveGI)
    UNITY_DOTS_INSTANCED_PROP(float, _BaseColorAmplify)
    UNITY_DOTS_INSTANCED_PROP(float, _MipBias)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

#define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__BaseColor)
#define _Cutoff             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Cutoff)
#define _Surface            UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Surface)
#define _ShaderType         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ShaderType)
#define _AdditiveGI         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__AdditiveGI)
#define _BaseColorAmplify   UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__BaseColorAmplify)
#define _MipBias            UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__MipBias)
#endif

#include "../ShaderLibrary/SurfaceInput.hlsl"

#endif
