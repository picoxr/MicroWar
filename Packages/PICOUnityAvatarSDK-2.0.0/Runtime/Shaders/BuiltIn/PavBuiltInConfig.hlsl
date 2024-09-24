#ifndef PAV_BUILTIN_INCLUDED
#define PAV_BUILTIN_INCLUDED

#define TEMPLATE_2_REAL TEMPLATE_2_FLT
#define real float
#define real2 float2
#define real3 float3
#define real4 float4

#define real2x2 float2x2
#define real2x3 float2x3
#define real2x4 float2x4
#define real3x2 float3x2
#define real3x3 float3x3
#define real3x4 float3x4
#define real4x3 float4x3
#define real4x4 float4x4

#define REAL_MIN FLT_MIN
#define REAL_MAX FLT_MAX
#define REAL_EPS FLT_EPS
#define TEMPLATE_1_REAL TEMPLATE_1_FLT
#define TEMPLATE_2_REAL TEMPLATE_2_FLT
#define TEMPLATE_3_REAL TEMPLATE_3_FLT

// -----------------------------------------------------------------------------
// Constants

#define HALF_MAX        65504.0 // (2 - 2^-10) * 2^15
#define HALF_MAX_MINUS1 65472.0 // (2 - 2^-9) * 2^15
#define EPSILON         1.0e-4
#define PI              3.14159265359
#define TWO_PI          6.28318530718
#define FOUR_PI         12.56637061436
#define INV_PI          0.31830988618
#define INV_TWO_PI      0.15915494309
#define INV_FOUR_PI     0.07957747155
#define HALF_PI         1.57079632679
#define INV_HALF_PI     0.636619772367

#define FLT_EPSILON     1.192092896e-07 // Smallest positive number, such that 1.0 + FLT_EPSILON != 1.0
#define FLT_MIN         1.175494351e-38 // Minimum representable positive floating-point number
#define FLT_MAX         3.402823466e+38 // Maximum representable floating-point number

static const half3 AP1_RGB2Y = half3(0.272229, 0.674082, 0.0536895);


// Using pow often result to a warning like this
// "pow(f, e) will not work for negative f, use abs(f) or conditionally handle negative values if you expect them"
// PositivePow remove this warning when you know the value is positive and avoid inf/NAN.
float PositivePow(float base, float power)
{
    return pow(max(abs(base), float(FLT_EPSILON)), power);
}

float2 PositivePow(float2 base, float2 power)
{
    return pow(max(abs(base), float2(FLT_EPSILON, FLT_EPSILON)), power);
}

float3 PositivePow(float3 base, float3 power)
{
    return pow(max(abs(base), float3(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
}

float4 PositivePow(float4 base, float4 power)
{
    return pow(max(abs(base), float4(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
}


// Gamma22
real Gamma22ToLinear(real c)
{
	return PositivePow(c, 2.2);
}

real3 Gamma22ToLinear(real3 c)
{
	return PositivePow(c.rgb, real3(2.2, 2.2, 2.2));
}

real4 Gamma22ToLinear(real4 c)
{
	return real4(Gamma22ToLinear(c.rgb), c.a);
}

real LinearToGamma22(real c)
{
	return PositivePow(c, 0.454545454545455);
}

real3 LinearToGamma22(real3 c)
{
	return PositivePow(c.rgb, real3(0.454545454545455, 0.454545454545455, 0.454545454545455));
}

real4 LinearToGamma22(real4 c)
{
	return real4(LinearToGamma22(c.rgb), c.a);
}

//-----------------------------------------------------------------------------
// Color space
//-----------------------------------------------------------------------------

// Convert rgb to luminance
// with rgb in linear space with sRGB primaries and D65 white point
real Luminance(real3 linearRgb)
{
    return dot(linearRgb, real3(0.2126729, 0.7151522, 0.0721750));
}

real Luminance(real4 linearRgba)
{
    return Luminance(linearRgba.rgb);
}

real AcesLuminance(real3 linearRgb)
{
    return dot(linearRgb, AP1_RGB2Y);
}

real AcesLuminance(real4 linearRgba)
{
    return AcesLuminance(linearRgba.rgb);
}

// Scotopic luminance approximation - input is in XYZ space
// Note: the range of values returned is approximately [0;4]
// "A spatial postprocessing algorithm for images of night scenes"
// William B. Thompson, Peter Shirley, and James A. Ferwerda
real ScotopicLuminance(real3 xyzRgb)
{
    float X = xyzRgb.x;
    float Y = xyzRgb.y;
    float Z = xyzRgb.z;
    return Y * (1.33 * (1.0 + (Y + Z) / X) - 1.68);
}

real ScotopicLuminance(real4 xyzRgba)
{
    return ScotopicLuminance(xyzRgba.rgb);
}

// This function take a rgb color (best is to provide color in sRGB space)
// and return a YCoCg color in [0..1] space for 8bit (An offset is apply in the function)
// Ref: http://www.nvidia.com/object/real-time-ycocg-dxt-compression.html
#define YCOCG_CHROMA_BIAS (128.0 / 255.0)
real3 RGBToYCoCg(real3 rgb)
{
    real3 YCoCg;
    YCoCg.x = dot(rgb, real3(0.25, 0.5, 0.25));
    YCoCg.y = dot(rgb, real3(0.5, 0.0, -0.5)) + YCOCG_CHROMA_BIAS;
    YCoCg.z = dot(rgb, real3(-0.25, 0.5, -0.25)) + YCOCG_CHROMA_BIAS;

    return YCoCg;
}

real3 YCoCgToRGB(real3 YCoCg)
{
    real Y = YCoCg.x;
    real Co = YCoCg.y - YCOCG_CHROMA_BIAS;
    real Cg = YCoCg.z - YCOCG_CHROMA_BIAS;

    real3 rgb;
    rgb.r = Y + Co - Cg;
    rgb.g = Y + Cg;
    rgb.b = Y - Co - Cg;

    return rgb;
}

// Following function can be use to reconstruct chroma component for a checkboard YCoCg pattern
// Reference: The Compact YCoCg Frame Buffer
real YCoCgCheckBoardEdgeFilter(real centerLum, real2 a0, real2 a1, real2 a2, real2 a3)
{
    real4 lum = real4(a0.x, a1.x, a2.x, a3.x);
    // Optimize: real4 w = 1.0 - step(30.0 / 255.0, abs(lum - centerLum));
    real4 w = 1.0 - saturate((abs(lum.xxxx - centerLum) - 30.0 / 255.0) * HALF_MAX);
    real W = w.x + w.y + w.z + w.w;
    // handle the special case where all the weights are zero.
    return  (W == 0.0) ? a0.y : (w.x * a0.y + w.y* a1.y + w.z* a2.y + w.w * a3.y) / W;
}

// Converts linear RGB to LMS
real3 LinearToLMS(real3 x)
{
    const real3x3 LIN_2_LMS_MAT = {
        3.90405e-1, 5.49941e-1, 8.92632e-3,
        7.08416e-2, 9.63172e-1, 1.35775e-3,
        2.31082e-2, 1.28021e-1, 9.36245e-1
    };

    return mul(LIN_2_LMS_MAT, x);
}

real3 LMSToLinear(real3 x)
{
    const real3x3 LMS_2_LIN_MAT = {
        2.85847e+0, -1.62879e+0, -2.48910e-2,
        -2.10182e-1,  1.15820e+0,  3.24281e-4,
        -4.18120e-2, -1.18169e-1,  1.06867e+0
    };

    return mul(LMS_2_LIN_MAT, x);
}

// Hue, Saturation, Value
// Ranges:
//  Hue [0.0, 1.0]
//  Sat [0.0, 1.0]
//  Lum [0.0, HALF_MAX]
real3 RgbToHsv(real3 c)
{
    const real4 K = real4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    real4 p = lerp(real4(c.bg, K.wz), real4(c.gb, K.xy), step(c.b, c.g));
    real4 q = lerp(real4(p.xyw, c.r), real4(c.r, p.yzx), step(p.x, c.r));
    real d = q.x - min(q.w, q.y);
    const real e = 1.0e-4;
    return real3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

real3 HsvToRgb(real3 c)
{
    const real4 K = real4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    real3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

real RotateHue(real value, real low, real hi)
{
    return (value < low)
            ? value + hi
            : (value > hi)
                ? value - hi
                : value;
}

// Soft-light blending mode use for split-toning. Works in HDR as long as `blend` is [0;1] which is
// fine for our use case.
float3 SoftLight(float3 base, float3 blend)
{
    float3 r1 = 2.0 * base * blend + base * base * (1.0 - 2.0 * blend);
    float3 r2 = sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend);
    float3 t = step(0.5, blend);
    return r2 * t + (1.0 - t) * r1;
}

// SMPTE ST.2084 (PQ) transfer functions
// 1.0 = 100nits, 100.0 = 10knits
#define DEFAULT_MAX_PQ 100.0

struct ParamsPQ
{
    real N, M;
    real C1, C2, C3;
};

static const ParamsPQ PQ =
{
    2610.0 / 4096.0 / 4.0,   // N
    2523.0 / 4096.0 * 128.0, // M
    3424.0 / 4096.0,         // C1
    2413.0 / 4096.0 * 32.0,  // C2
    2392.0 / 4096.0 * 32.0,  // C3
};

real3 LinearToPQ(real3 x, real maxPQValue)
{
    x = PositivePow(x / maxPQValue, PQ.N);
    real3 nd = (PQ.C1 + PQ.C2 * x) / (1.0 + PQ.C3 * x);
    return PositivePow(nd, PQ.M);
}

real3 LinearToPQ(real3 x)
{
    return LinearToPQ(x, DEFAULT_MAX_PQ);
}

real3 PQToLinear(real3 x, real maxPQValue)
{
    x = PositivePow(x, rcp(PQ.M));
    real3 nd = max(x - PQ.C1, 0.0) / (PQ.C2 - (PQ.C3 * x));
    return PositivePow(nd, rcp(PQ.N)) * maxPQValue;
}

real3 PQToLinear(real3 x)
{
    return PQToLinear(x, DEFAULT_MAX_PQ);
}

// Alexa LogC converters (El 1000)
// See http://www.vocas.nl/webfm_send/964
// Max range is ~58.85666

// Set to 1 to use more precise but more expensive log/linear conversions. I haven't found a proper
// use case for the high precision version yet so I'm leaving this to 0.
#define USE_PRECISE_LOGC 0

struct ParamsLogC
{
    real cut;
    real a, b, c, d, e, f;
};

static const ParamsLogC LogC =
{
    0.011361, // cut
    5.555556, // a
    0.047996, // b
    0.244161, // c
    0.386036, // d
    5.301883, // e
    0.092819  // f
};

real LinearToLogC_Precise(real x)
{
    real o;
    if (x > LogC.cut)
        o = LogC.c * log10(max(LogC.a * x + LogC.b, 0.0)) + LogC.d;
    else
        o = LogC.e * x + LogC.f;
    return o;
}

real3 LinearToLogC(real3 x)
{
#if USE_PRECISE_LOGC
    return real3(
        LinearToLogC_Precise(x.x),
        LinearToLogC_Precise(x.y),
        LinearToLogC_Precise(x.z)
    );
#else
    return LogC.c * log10(max(LogC.a * x + LogC.b, 0.0)) + LogC.d;
#endif
}

real LogCToLinear_Precise(real x)
{
    real o;
    if (x > LogC.e * LogC.cut + LogC.f)
        o = (pow(10.0, (x - LogC.d) / LogC.c) - LogC.b) / LogC.a;
    else
        o = (x - LogC.f) / LogC.e;
    return o;
}

real3 LogCToLinear(real3 x)
{
#if USE_PRECISE_LOGC
    return real3(
        LogCToLinear_Precise(x.x),
        LogCToLinear_Precise(x.y),
        LogCToLinear_Precise(x.z)
    );
#else
    return (pow(10.0, (x - LogC.d) / LogC.c) - LogC.b) / LogC.a;
#endif
}







// has specular in default.
#define PAV_HAS_SPECULAR
// has main light in default.
#define PAV_HAS_MAINLIGHT
//
//#define PAV_HAS_OCCLUSION

//
#ifndef PAV_ADDITIVE_GI
#	define PAV_ADDITIVE_GI
#endif
// only sphere harmonic gi.
#ifdef PAV_LIT_ONLY_GI_DIFFUSE
    #undef _NORMALMAP
    #undef _PARALLAXMAP
    //#   undef _OCCLUSIONMAP.
    #undef _SPECULAR_SETUP
    #undef _ADDITIONAL_LIGHTS
    #undef _ADDITIONAL_LIGHTS_VERTEX
    
    // remove specular. 
    #undef PAV_HAS_SPECULAR
    // disable main light.
    #undef PAV_HAS_MAINLIGHT
    // disable _CLEARCOAT
    #undef _CLEARCOAT

// indirect diffulse + direct diffuse..
#elif defined(PAV_LIT_ONLY_DIFFUSE)
    #undef _NORMALMAP
    #undef _PARALLAXMAP
    //#   undef _OCCLUSIONMAP
    #undef _SPECULAR_SETUP

    // remove specular. 
    #undef PAV_HAS_SPECULAR

// indirect diffuse + direct diffuse  + indirect spec + indirect spec
#elif defined(PAV_LIT_DIFFUSE_SPEC)
    #undef _NORMALMAP
    #undef _PARALLAXMAP
	// force specular.
	#define _SPECULAR_SETUP

// indirect diffuse + direct diffuse  + indirect spec + indirect spec + normals.
#elif defined(PAV_LIT_FULL_PBR)

// toon shader.
#elif defined(PAV_LIT_TOON)
    #undef _PARALLAXMAP
    #undef _ADDITIONAL_LIGHTS
    #undef _ADDITIONAL_LIGHTS_VERTEX

#endif


// if has emission map, must has emission.
#ifdef _EMISSION
#   define PAV_HAS_EMISSION
#endif

#ifndef PAV_HAS_SPECULAR
#   define _SPECULARHIGHLIGHTS_OFF
#endif

// if no tangents, can not support normal and parallax.
#ifdef PAV_NO_TANGENTS
#	undef _NORMALMAP
#	undef _PARALLAXMAP
#endif

// extra gi for dark scene to lighting avatar.
#ifdef PAV_ADDITIVE_GI
#	ifndef PAV_HAS_ADDITIVE_GI
#		define PAV_HAS_ADDITIVE_GI
		uniform float _AdditiveGI;
#	endif
#endif

////////////////////////////////////顶点数据替换/////////////////////////////////////////////////////////////

//struct Attributes
//{
//    float4 positionOS    : POSITION;   float4 position : POSITION
//    float3 normalOS      : NORMAL;
//#ifdef PAV_HAS_TANGENTS
//    float4 tangentOS     : TANGENT;
//#endif
//}

#if defined(PAV_VERTEX_FROM_TEXTURE)

	// position/normal/tangent interpolaped data
	uniform Texture2D<float> g_pavVertices;
	
	// all.
	inline void pavGetVertexPNT(int vid, inout float4 position, inout float4 normal, inout float4 tangent)
	{
		// TODO: like following?
		uint vertIndex = i.vid;
		uint2 posUV = uint2((vertIndex % 1024) * 4, vertIndex / 1024);

#	ifdef PAV_NO_TANGENTS
		position = float4(g_pavVertices[posUV + uint2(0,0)], g_pavVertices[posUV + uint2(1,0)], g_pavVertices[posUV + uint2(2,0)], 1);
		normal = float4(g_pavVertices[posUV + uint2(3,0)], g_pavVertices[posUV + uint2(4,0)], g_pavVertices[posUV + uint2(5,0)], 1);
#	else
		position = float4(g_pavVertices[posUV + uint2(0,0)], g_pavVertices[posUV + uint2(1,0)], g_pavVertices[posUV + uint2(2,0)], 1);
		normal = float4(g_pavVertices[posUV + uint2(3,0)], g_pavVertices[posUV + uint2(4,0)], g_pavVertices[posUV + uint2(5,0)], 1);
		tangent = float4(g_pavVertices[posUV + uint2(6,0)], g_pavVertices[posUV + uint2(7,0)], g_pavVertices[posUV + uint2(8,0)], , g_pavVertices[posUV + uint2(9,0)]);
#	endif
	}
	// No tangents
	inline void pavGetVertexPN(int vid, inout float4 position, inout float4 normal)
	{
		uint vertIndex = i.vid;
		uint2 posUV = uint2((vertIndex % 1024) * 4, vertIndex / 1024);
		// TODO:.
	}
	// Only position..
	inline void pavGetVertexP(int vid, inout float4 position)
	{
		uint vertIndex = i.vid;
		uint2 posUV = uint2((vertIndex % 1024) * 4, vertIndex / 1024);
		// TODO.
	}

	// public macros.
	#define PAV_GET_VERTEX_PNT(vid, position, normal, tangent) pavGetVertexPNT(vid, position, normal, tangent);
	#define PAV_GET_VERTEX_PN(vid, position, normal) pavGetVertexPN(vid, position, normal);
	#define PAV_GET_VERTEX_P(vid, position)  pavGetVertexP(vid, position);

#elif defined(PAV_VERTEX_FROM_BUFFER)

	#include "../GpuSkinning/GPUSkin.cginc"

	// position/normal/tangent interpolaped data
	StructuredBuffer<uint> _outputBuffer;

	// all.
	inline void pavGetVertexPNT(StaticBufferDesc staticBufferDesc, int vid, inout float4 position, inout float3 normal, inout float4 tangent)
	{
#ifdef PAV_UNITY_SKIN

#else
        position.xyz = GetOutputPosition(_outputBuffer, staticBufferDesc, vid);
#	ifdef PAV_NO_TANGENTS
        normal = GetOutputNormal(_outputBuffer, staticBufferDesc, vid);
#	else
		normal = GetOutputNormal(_outputBuffer, staticBufferDesc, vid);
        tangent = GetOutputTangent(_outputBuffer, staticBufferDesc, vid);
#	endif
#endif
	}
	// No tangents
	inline void pavGetVertexPN(StaticBufferDesc staticBufferDesc, int vid, inout float4 position, inout float3 normal)
	{
#ifdef PAV_UNITY_SKIN

#else
        position.xyz = GetOutputPosition(_outputBuffer, staticBufferDesc, vid);
		normal = GetOutputNormal(_outputBuffer, staticBufferDesc, vid);
#endif
	}
	// Only position.
	inline void pavGetVertexP(StaticBufferDesc staticBufferDesc, int vid, inout float4 position)
	{
#ifdef PAV_UNITY_SKIN

#else
        position.xyz = GetOutputPosition(_outputBuffer, staticBufferDesc, vid);
#endif
	}

	inline void GetMaterialIndex(StaticBufferDesc staticBufferDesc, uint vid, inout float3 uv)
	{
#ifdef PAV_MERGED_TEXTURE
		uv.z = (float) GetMaterialIndex(staticBufferDesc, vid);
#else
		uv.z = 0;
#endif
	}

	// public macros..
	#define PAV_GET_DYNAMIC_BUFFER StaticBufferDesc staticBufferDesc = GetStaticBufferDesc((uint) _staticBufferOffset)
	#define PAV_GET_VERTEX_PNT(vid, position, normal, tangent) PAV_GET_DYNAMIC_BUFFER; pavGetVertexPNT(staticBufferDesc, vid, position, normal, tangent)
	#define PAV_GET_VERTEX_PN(vid, position, normal) PAV_GET_DYNAMIC_BUFFER; pavGetVertexPN(staticBufferDesc, vid, position, normal)
	#define PAV_GET_VERTEX_P(vid, position) PAV_GET_DYNAMIC_BUFFER; pavGetVertexP(staticBufferDesc, vid, position)
	#define PAV_GET_MATERIAL_INDEX(vid, uv) GetMaterialIndex(staticBufferDesc, vid, uv)
	#define PAV_FLIP_UV_Y(uv) uv.y = 1.0 - uv.y
#else

	#define PAV_GET_VERTEX_PNT(vid, position, normal, tangent)
	#define PAV_GET_VERTEX_PN(vid, position, normal)
	#define PAV_GET_VERTEX_P(vid, position)
	#define PAV_GET_MATERIAL_INDEX(vid, uv)
	#define PAV_FLIP_UV_Y(uv)
#endif


// Vertex ID attribute.
#if (defined(PAV_VERTEX_FROM_BUFFER) || defined(PAV_VERTEX_FROM_TEXTURE))
#	define PAV_VERTEX_ID  uint vid : SV_VertexID;
#else
#	define PAV_VERTEX_ID
#endif

#endif // PAV_BUILTIN_INCLUDED

