#ifndef EYE_INCLUDED
#define EYE_INCLUDED

#include "Dependency/URP/ShaderLibrary/SurfaceInput.hlsl"
#include "Common.hlsl"

#if defined(_SCLERA_NORMALMAP) || defined(_IRIS_NORMALMAP)
#define REQUIRE_TANGENT
#endif

CBUFFER_START(UnityPerMaterial)

    half _UsingAlbedoHue;
    half4 _ColorRegion1;
    half4 _ColorRegion2;
    half4 _ColorRegion3;
    half4 _ColorRegion4;

    half _BumpScale;
    half4 _IrisColor;

// Parallax Settings
    half _ParallaxScale;
    half _MinParallaxSamples;
    half _MaxParallaxSamples;

// Highlight Settings
    half _ScleraSmoothness;
    half _ScleraSpecSmoothness;
    half4 _ScleraSpecColor;

    half _IrisSmoothness;
    half _IrisSpecSmoothness;
    half4 _IrisSpecColor;

    half4 _SharpParam;
    half3 _HighlightOffset;

    half _CausticIntensity;
    half _CausticRange;

// MatCap
    //half _MatCapStrength;

    half _Tonemapping;
CBUFFER_END

//TEXTURE2D(_HeightMap);      SAMPLER(sampler_HeightMap);
//TEXTURE2D(_MatCap);         SAMPLER(sampler_MatCap);


#endif
