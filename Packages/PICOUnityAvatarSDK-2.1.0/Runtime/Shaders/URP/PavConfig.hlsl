#ifndef PAV_BASE_INCLUDED
#define PAV_BASE_INCLUDED

/** SurfaceInput
always       TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
_NORMALMAP   TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
_EMISSION    TEXTURE2D(_EmissionMap);        SAMPLER(sampler_EmissionMap);
PAV_ToonShadowMap TEXTURE2D(_ToonShadowMap); SAMPLER(sampler_ToonShadowMap);
 */

/**  LitInput

_PARALLAXMAP  TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
_OCCLUSIONMAP TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
_DETAIL       TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
_DETAIL       TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
_DETAIL       TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);
!_SPECULAR_SETUP && _METALLICSPECGLOSSMAP     TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
 _SPECULAR_SETUP                              TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
(_CLEARCOAT || _CLEARCOATMAP)                 TEXTURE2D(_ClearCoatMap);       SAMPLER(sampler_ClearCoatMap);
*/

// has specular in default.
#define PAV_HAS_SPECULAR
// has main light in default.
#define PAV_HAS_MAINLIGHT
//
//#define PAV_HAS_OCCLUSION

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
	inline void pavGetVertexPNT(int vid, inout float4 position, inout float3 normal, inout float4 tangent)
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
	inline void pavGetVertexPN(int vid, inout float4 position, inout float3 normal)
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

#ifdef PAV_OUT_STRUCTURED_BUFFER
StructuredBuffer<uint> _outputBuffer;
#endif

#ifdef PAV_OUT_RAW_BUFFER
ByteAddressBuffer _outputBuffer;
#endif

	// all.
	inline void pavGetVertexPNT(StaticBufferDesc staticBufferDesc, int vid, inout float4 position, inout float3 normal, inout float4 tangent)
	{
#ifdef PAV_UNITY_SKIN
		//position.z = -position.z;
		//normal.z = -normal.z;
		//tangent.z = -tangent.z;
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
		//position.z = -position.z;
		//normal.z = -normal.z;
#else
        position.xyz = GetOutputPosition(_outputBuffer, staticBufferDesc, vid);
		normal = GetOutputNormal(_outputBuffer, staticBufferDesc, vid);
#endif
	}
	// Only position.
	inline void pavGetVertexP(StaticBufferDesc staticBufferDesc, int vid, inout float4 position)
	{
#ifdef PAV_UNITY_SKIN
		//position.z = -position.z;
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
#if (defined(PAV_VERTEX_FROM_BUFFER) || defined(PAV_VERTEX_FROM_TEXTURE) || defined(PAV_AVATAR_BUNCH))
#	define PAV_VERTEX_ID  uint vid : SV_VertexID;
#else
#	define PAV_VERTEX_ID
#endif


//////////////////////////////////// AvatarBunch 顶点数据替换 /////////////////////////////////////////////////////////////
#if defined(PAV_AVATAR_BUNCH) && defined(INSTANCING_ON)

// position/normal/tangent interpolaped data
uniform Texture2D<half4> g_pavInstanceBonesTex;
// position/normal/tangent interpolaped data width is 1024.
uniform uint g_pavInstanceBonesTexWidth;
uniform uint g_pavInstanceBonesTexHeight;
// first line is bind poses,  bone weights from line 2.   boneCount(2byte) | index0(int as half) weight0(half) | ...  | indexLastBone(int as half) weightLastBone(half) | 
uniform Texture2D<half4> g_pavBoneWeightsTex;
uniform uint g_pavBoneWeightsTexWidth;
uniform uint g_pavBoneWeightsTexHeight;

// all.
inline void pavGpuInstanceSkinVertexPNT(int instanceId, int vid, inout float4 position, inout float3 normal, inout float4 tangent)
{
	// each vertex occupy 2 pixel in g_pavBoneWeightsTex.
	uint2 bwUV = uint2((vid * 2) % g_pavBoneWeightsTexWidth, (vid * 2) / g_pavBoneWeightsTexWidth);
	int4 indices = floor(g_pavBoneWeightsTex[bwUV]);
	half4 weights = g_pavBoneWeightsTex[bwUV + uint2(1, 0)];

	half4 srcPos = position;
	position = half4(0, 0, 0, 1);
	half4 srcNormal = half4(normal, 1.0);
	normal = 0.0;
	half4 srcTangent = half4(tangent.xyz, 0.0);
	tangent.xyz = 0.0;

	for (int i = 0; i < 4;)
	{
		uint bonePixelBase = indices[i] * 4;
		// each bone matrix occupy 4 pixel in g_pavInstanceBonesTex, and each avatar skeleton occupy one line in g_pavInstanceBonesTex
		half4 c0 = g_pavInstanceBonesTex[uint2(bonePixelBase, instanceId)];
		half4 c1 = g_pavInstanceBonesTex[uint2(bonePixelBase + 1, instanceId)];
		half4 c2 = g_pavInstanceBonesTex[uint2(bonePixelBase + 2, instanceId)];
		half4 c3 = g_pavInstanceBonesTex[uint2(bonePixelBase + 3, instanceId)];
		half4x4 boneMat = { c0, c1, c2, c3 };

		position.xyz += mul(srcPos, boneMat).xyz * weights[i];
		normal.xyz += mul(srcNormal, boneMat).xyz * weights[i];
		tangent.xyz += mul(srcTangent, boneMat).xyz * weights[i];

		if (weights[i++] < 0.001)
		{
			break;
		}
	}
}
// No tangents
inline void pavGpuInstanceSkinVertexPN(int instanceId, int vid, inout float4 position, inout float3 normal)
{
	// each vertex occupy 2 pixel in g_pavBoneWeightsTex.
	uint2 bwUV = uint2((vid * 2) % g_pavBoneWeightsTexWidth, (vid * 2)/ g_pavBoneWeightsTexWidth);
	int4 indices = floor(g_pavBoneWeightsTex[bwUV]);
	half4 weights = g_pavBoneWeightsTex[bwUV + uint2(1, 0)];
	
	half4 srcPos = position;
	position = half4(0, 0, 0, 1);
	half4 srcNormal = half4(normal, 1.0);
	normal = 0.0;

	for (int i = 0; i < 4;)
	{
		uint bonePixelBase = indices[i] * 4;
		// each bone matrix occupy 4 pixel in g_pavInstanceBonesTex, and each avatar skeleton occupy one line in g_pavInstanceBonesTex
		half4 c0 = g_pavInstanceBonesTex[uint2(bonePixelBase, instanceId)];
		half4 c1 = g_pavInstanceBonesTex[uint2(bonePixelBase + 1, instanceId)];
		half4 c2 = g_pavInstanceBonesTex[uint2(bonePixelBase + 2, instanceId)];
		half4 c3 = g_pavInstanceBonesTex[uint2(bonePixelBase + 3, instanceId)];
		half4x4 boneMat = { c0, c1, c2, c3 };

		position.xyz += mul(srcPos, boneMat).xyz * weights[i];
		normal.xyz += mul(srcNormal, boneMat).xyz * weights[i];

		if (weights[i++] < 0.001)
		{
			break;
		}
	}
}
// Only position..
inline void pavGpuInstanceSkinVertexP(int instanceId, int vid, inout float4 position)
{
	// each vertex occupy 2 pixel in g_pavBoneWeightsTex.
	uint2 bwUV = uint2((vid * 2) % g_pavBoneWeightsTexWidth, (vid * 2) / g_pavBoneWeightsTexWidth);
	int4 indices = floor(g_pavBoneWeightsTex[bwUV]);
	half4 weights = g_pavBoneWeightsTex[bwUV + uint2(1, 0)];

	half4 srcPos = position;
	position = half4(0, 0, 0, 1);

	for (int i = 0; i < 4;)
	{
		uint bonePixelBase = indices[i] * 4;
		// each bone matrix occupy 4 pixel in g_pavInstanceBonesTex, and each avatar skeleton occupy one line in g_pavInstanceBonesTex
		half4 c0 = g_pavInstanceBonesTex[uint2(bonePixelBase, instanceId)];
		half4 c1 = g_pavInstanceBonesTex[uint2(bonePixelBase + 1, instanceId)];
		half4 c2 = g_pavInstanceBonesTex[uint2(bonePixelBase + 2, instanceId)];
		half4 c3 = g_pavInstanceBonesTex[uint2(bonePixelBase + 3, instanceId)];
		half4x4 boneMat = { c0, c1, c2, c3 };

		position.xyz += mul(srcPos, boneMat).xyz * weights[i];

		if (weights[i++] < 0.001)
		{
			break;
		}
	}
}

// public macros.
#define PAV_GPUINSTANCE_SKIN_PNT(instanceId, vid, position, normal, tangent) pavGpuInstanceSkinVertexPNT(instanceId, vid, position, normal, tangent);
#define PAV_GPUINSTANCE_SKIN_PN(instanceId, vid, position, normal) pavGpuInstanceSkinVertexPN(instanceId, vid, position, normal);
#define PAV_GPUINSTANCE_SKIN_P(instanceId, vid, position)  pavGpuInstanceSkinVertexP(instanceId, vid, position);

#else

// public macros.
#define PAV_GPUINSTANCE_SKIN_PNT(instanceId, vid, position, normal, tangent)
#define PAV_GPUINSTANCE_SKIN_PN(instanceId, vid, position, normal)
#define PAV_GPUINSTANCE_SKIN_P(instanceId, vid, position)

#endif


// extra gi for dark scene to lighting avatar.
#ifndef PAV_ADDITIVE_GI
#	define PAV_ADDITIVE_GI
#endif

#ifndef PAV_HAS_ADDITIVE_GI
#	define PAV_HAS_ADDITIVE_GI
#endif

#endif // file

