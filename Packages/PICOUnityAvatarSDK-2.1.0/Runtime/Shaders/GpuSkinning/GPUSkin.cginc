#ifndef __GPU_SKIN_INC__
#define __GPU_SKIN_INC__

static const uint PAV_DATA_FORMAT_FLOAT                     = 0;
static const uint PAV_DATA_FORMAT_HALF                      = 1;
static const uint PAV_DATA_FORMAT_PACKED_10_10_10_2_SNORM   = 2;

static const uint PAV_TEXTURE_KEY_COUNT              = 32;
static const uint PAV_CUSTOM_TEXTURE_KEY_COUNT       = 17;
static const uint PAV_CUSTOM_PROPERTY_KEY_COUNT      = 9;

static const uint PAV_TEXTURE_KEY_BASE_MAP           = 0;
static const uint PAV_TEXTURE_KEY_METALLIC_GLOSS_MAP = 1;
static const uint PAV_TEXTURE_KEY_SPEC_GLOSS_MAP     = 2;
static const uint PAV_TEXTURE_KEY_BUMP_MAP           = 3;
static const uint PAV_TEXTURE_KEY_PARALLAX_MAP       = 4;
static const uint PAV_TEXTURE_KEY_OCCLUSION_MAP      = 5;
static const uint PAV_TEXTURE_KEY_EMISSION_MAP       = 6;
static const uint PAV_TEXTURE_KEY_DETAIL_MASK        = 7;
static const uint PAV_TEXTURE_KEY_DETAIL_ALBEDO_MAP  = 8;
static const uint PAV_TEXTURE_KEY_DETAIL_NORMAL_MAP  = 9;
static const uint PAV_TEXTURE_KEY_TOON_SHADOW_MAP    = 10;
static const uint PAV_TEXTURE_KEY_SECOND_BASE_MAP    = 11;
static const uint PAV_TEXTURE_KEY_SECOND_BUMP_MAP    = 12;
static const uint PAV_TEXTURE_KEY_SECOND_METALLIC_SPEC_GLOSS_MAP    = 13;
static const uint PAV_TEXTURE_KEY_COLOR_REGION_MAP   = 14;
static const uint PAV_TEXTURE_KEY_CUSTOM_MAP_0       = 15;

static const uint PAV_SHADER_TYPE_BODY_BASE     = 0;
static const uint PAV_SHADER_TYPE_BODY_TOON     = 1;
static const uint PAV_SHADER_TYPE_EYELASH_BASE  = 200;
static const uint PAV_SHADER_TYPE_HAIR_BASE     = 300;
static const uint PAV_SHADER_TYPE_EYE_BASE      = 400;
static const uint PAV_SHADER_TYPE_CLOTH_BASE    = 500;
static const uint PAV_SHADER_TYPE_CLOTH_LASER   = 501;

static const uint PAV_MERGED_MATERIAL_DATA_SIZE = 1376; // (8 * PAV_TEXTURE_KEY_COUNT + (13 + PAV_CUSTOM_PROPERTY_KEY_COUNT) * 4) * 4

#ifdef PAV_TRANSFER_SHAPING_MESH_TO_STATIC_BUFFR
#   define BufferType RWByteAddressBuffer
#else
#   define BufferType ByteAddressBuffer
#endif

#define PAV_DATA_COMPRESS
//#define PAV_OUT_STRUCTURED_BUFFER
#define PAV_OUT_RAW_BUFFER
//#define PAV_MATERIAL_DATA_TEXTURE
#define PAV_MATERIAL_DATA_BUFFER
//#define PAV_UNITY_SKIN

BufferType _staticBuffer;
BufferType _dynamicBuffer;

int _staticBufferOffset;
int _dynamicBufferOffset;
int _vertexIndexOffset;

struct MergedTextureInfo
{
    float4 uvScaleOffset;
    int textureArrayIndex;
    float reserved[3];
};

struct MergedMaterialData
{
    MergedTextureInfo textureInfos[PAV_TEXTURE_KEY_COUNT];

    float outline;
    float cutoff;
    float smoothness;
    float glossMapScale;

    float smoothnessTextureChannel;
    float metallic;
    float bumpScale;
    float parallax;

    float occlusionStrength;
    float detailAlbedoMapScale;
    float detailNormalMapScale;
    float clearCoatMask;

    float clearCoatSmoothness;
    float shaderType;
    float usingAlbedoHue;
    float reserved;

    float4 outlineColor;
    float4 baseColor;
    float4 specColor;
    float4 emissionColor;

    float4 baseMap_ST;
    float4 colorRegion1;
    float4 colorRegion2;
    float4 colorRegion3;
    float4 colorRegion4;

    float4 customVecs[PAV_CUSTOM_PROPERTY_KEY_COUNT];
};

struct MeshInstanceDesc
{
    uint staticBufferOffset;
    uint dynamicBufferOffset;
    uint vertexIndexOffset;
    uint vertexID;
};

struct StaticBufferDesc
{
    // data formats
    uint meshVertexDataFormat;
    uint morphVertexDataFormat;
    uint reserved0;
    uint reserved1;
    // mesh data offsets
    uint meshPositionDataOffset;
    uint meshNormalDataOffset;
    uint meshTangentDataOffset;
    uint reserved2;
    // skin data offsets
    uint meshBoneIndicesDataOffset;
    uint meshBoneWeightsDataOffset;
    uint meshBindposesDataOffset;
    uint reserved3;
    // mesh position scale offset
    float4 meshPositionOffset;
    float4 meshPositionScale;
    // morph data offsets
    uint morphPositionDataOffset;
    uint morphNormalDataOffset;
    uint morphTangentDataOffset;
    uint morphStride;
    // morph data scale
    float4 morphPositionScale;
    float4 morphNormalScale;
    float4 morphTangentScale;
    // packed morph
    uint morphPackedVectorOffsetsDataOffset;
    uint morphPackedVectorDataOffset;
    uint morphCount;
    uint reserved4;
    // output
    uint outputPositionDataOffset;
    uint outputNormalDataOffset;
    uint outputTangentDataOffset;
    uint reserved5;
    // merged
    uint mergedMaterialIndexDataOffset;
    uint mergedContinueLayoutVerticesDataOffset;
    uint mergedContinueLayoutInfoDataOffset;
    uint reserved6;
};

struct DynamicBufferDesc
{
    uint morphCount;
    uint morphIndexDataOffset;
    uint morphWeightDataOffset;
    uint boneMatrixDataOffset;
};

struct VertexContinueLayoutInfo
{
    uint meshDataOffset;
    uint attributeBits;
    uint vertexStride;
    uint morphVertexOffset;
    uint morphVertexStride;
};

MeshInstanceDesc GetMeshInstanceDesc(uint vertexIndex)
{
    MeshInstanceDesc desc = (MeshInstanceDesc) 0;

    const uint stride = 16;
    uint addr = stride * vertexIndex;
    uint4 u4 = _staticBuffer.Load4(addr);
    desc.staticBufferOffset = u4.x;
    desc.dynamicBufferOffset = u4.y;
    desc.vertexIndexOffset = u4.z;
    desc.vertexID = u4.w;

    return desc;
}

StaticBufferDesc GetStaticBufferDesc(uint staticBufferOffset)
{
    StaticBufferDesc desc = (StaticBufferDesc) 0;
    uint offset = staticBufferOffset;

    uint4 u4 = _staticBuffer.Load4(offset + 16 * 0);
    desc.meshVertexDataFormat = u4.x;
    desc.morphVertexDataFormat = u4.y;

    u4 = _staticBuffer.Load4(offset + 16 * 1);
    desc.meshPositionDataOffset = u4.x;
    desc.meshNormalDataOffset = u4.y;
    desc.meshTangentDataOffset = u4.z;

    u4 = _staticBuffer.Load4(offset + 16 * 2);
    desc.meshBoneIndicesDataOffset = u4.x;
    desc.meshBoneWeightsDataOffset = u4.y;
    desc.meshBindposesDataOffset = u4.z;

    u4 = _staticBuffer.Load4(offset + 16 * 3);
    desc.meshPositionOffset = asfloat(u4);

    u4 = _staticBuffer.Load4(offset + 16 * 4);
    desc.meshPositionScale = asfloat(u4);

    u4 = _staticBuffer.Load4(offset + 16 * 5);
    desc.morphPositionDataOffset = u4.x;
    desc.morphNormalDataOffset = u4.y;
    desc.morphTangentDataOffset = u4.z;
    desc.morphStride = u4.w;

    u4 = _staticBuffer.Load4(offset + 16 * 6);
    desc.morphPositionScale = asfloat(u4);

    u4 = _staticBuffer.Load4(offset + 16 * 7);
    desc.morphNormalScale = asfloat(u4);

    u4 = _staticBuffer.Load4(offset + 16 * 8);
    desc.morphTangentScale = asfloat(u4);

    u4 = _staticBuffer.Load4(offset + 16 * 9);
    desc.morphPackedVectorOffsetsDataOffset = u4.x;
    desc.morphPackedVectorDataOffset = u4.y;
    desc.morphCount = u4.z;

    u4 = _staticBuffer.Load4(offset + 16 * 10);
    desc.outputPositionDataOffset = u4.x;
    desc.outputNormalDataOffset = u4.y;
    desc.outputTangentDataOffset = u4.z;

    u4 = _staticBuffer.Load4(offset + 16 * 11);
    desc.mergedMaterialIndexDataOffset = u4.x;
    desc.mergedContinueLayoutVerticesDataOffset = u4.y;
    desc.mergedContinueLayoutInfoDataOffset = u4.z;

    return desc;
}

DynamicBufferDesc GetDynamicBufferDesc(uint dynamicBufferOffset)
{
    DynamicBufferDesc desc = (DynamicBufferDesc) 0;
    uint offset = dynamicBufferOffset;

    uint4 u4 = _dynamicBuffer.Load4(offset + 16 * 0);
    desc.morphCount = u4.x;
    desc.morphIndexDataOffset = u4.y;
    desc.morphWeightDataOffset = u4.z;
    desc.boneMatrixDataOffset = u4.w;

    return desc;
}

void GetMeshIndexAndLocalVertexID(StaticBufferDesc desc, uint vid, out uint meshIndex, out uint localVertexID)
{
    const uint stride = 4;
    uint addr = desc.mergedMaterialIndexDataOffset + stride * vid;
    uint u = _staticBuffer.Load(addr);
    localVertexID = (u & 0xffffff00u) >> 8;
    meshIndex = u & 0xffu;
}

void GetVertexContinueLayoutInfo(StaticBufferDesc desc, uint vid, out uint meshIndex, out uint localVertexID, out VertexContinueLayoutInfo layoutInfo)
{
    GetMeshIndexAndLocalVertexID(desc, vid, meshIndex, localVertexID);
    const uint stride = 20;
    uint addr = desc.mergedContinueLayoutInfoDataOffset + stride * meshIndex;
    uint4 u4 = _staticBuffer.Load4(addr);

    layoutInfo.meshDataOffset = u4.x;
    layoutInfo.attributeBits = u4.y;
    layoutInfo.vertexStride = u4.z;
    layoutInfo.morphVertexOffset = u4.w;
    layoutInfo.morphVertexStride = _staticBuffer.Load(addr + 16);
}

int UnpackBits(int packed, int offset, int bits)
{
    int shifted = packed >> offset;
    int mask = (1 << bits) - 1;
    int signBit = shifted & (1 << (bits - 1));
    return (-signBit) | (shifted & mask);
}

float4 Unpack4_10_10_10_2_snorm(int packed)
{
    int4 i4;
    i4.x = UnpackBits(packed, 0, 10);
    i4.y = UnpackBits(packed, 10, 10);
    i4.z = UnpackBits(packed, 20, 10);
    i4.w = UnpackBits(packed, 30, 2);
    float4 f4 = float4(i4);
    f4.xyz *= 1.0 / 511.0;
    return f4;
}

#ifdef PAV_TRANSFER_SHAPING_MESH_TO_STATIC_BUFFR

void SetFloat3(BufferType buffer, uint format, uint offset, uint vid, float3 val)
{
#ifndef PAV_DATA_COMPRESS
    const uint stride = 12;
    uint addr = offset + stride * vid; 
    buffer.Store3(addr, asuint(val));
#else
    if (format == PAV_DATA_FORMAT_FLOAT)
    {
        const uint stride = 12;
        uint addr = offset + stride * vid;
        buffer.Store3(addr, asuint(val));
    }
    else
    {
        #pragma error(" not implemented")
    }
    
#endif
}

#endif

uint GetFormatSize(uint format, int componentCount)
{
    switch (format)
    {
        case PAV_DATA_FORMAT_FLOAT:
            return 4 * componentCount;
        case PAV_DATA_FORMAT_HALF:
            return 2 * componentCount;
        case PAV_DATA_FORMAT_PACKED_10_10_10_2_SNORM:
            return 4;
        default:
            return 0;
    }
}

float3 GetFormatFloat3(BufferType buffer, uint format, uint offset, uint vid)
{
#ifndef PAV_DATA_COMPRESS
    const uint stride = 12;
    uint addr = offset + stride * vid;
    uint3 u3 = buffer.Load3(addr);
    return asfloat(u3);
#else
    switch (format)
    {
        case PAV_DATA_FORMAT_FLOAT:
        {
            const uint stride = 12;
            uint addr = offset + stride * vid;
            uint3 u3 = buffer.Load3(addr);
            return asfloat(u3);
        }
        case PAV_DATA_FORMAT_HALF:
        {
            const uint stride = 6;
            uint addr = offset + stride * vid;
            float3 f3;
            if (addr % 4 == 0)
            {
                uint2 u2 = buffer.Load2(addr);
                f3.x = f16tof32(u2.x & 0xffffu);
                f3.y = f16tof32((u2.x >> 16) & 0xffffu);
                f3.z = f16tof32(u2.y & 0xffffu);
            }
            else
            {
                // address must be a multiple of 4
                uint2 u2 = buffer.Load2(addr - 2);
                f3.x = f16tof32((u2.x >> 16) & 0xffffu);
                f3.y = f16tof32(u2.y & 0xffffu);
                f3.z = f16tof32((u2.y >> 16) & 0xffffu);
            }
            return f3;
        }
        case PAV_DATA_FORMAT_PACKED_10_10_10_2_SNORM:
        {
            const uint stride = 4;
            uint addr = offset + stride * vid;
            uint u = buffer.Load(addr);
            return Unpack4_10_10_10_2_snorm(asint(u)).xyz;
        }
        default:
        {
            return 0;
        }
    }
#endif
}

#ifdef PAV_TRANSFER_SHAPING_MESH_TO_STATIC_BUFFR

void SetFloat4(BufferType buffer, uint format, uint offset, uint vid, float4 val)
{
#ifndef PAV_DATA_COMPRESS
    const uint stride = 16;
    uint addr = offset + stride * vid; 
    buffer.Store4(addr, asuint(val));
#else
    if (format == PAV_DATA_FORMAT_FLOAT)
    {
        const uint stride = 16;
        uint addr = offset + stride * vid;
        buffer.Store4(addr, asuint(val));
    }
    else
    {
#pragma error(" not implemented")
    }
#endif
}

#endif

float4 GetFormatFloat4(BufferType buffer, uint format, uint offset, uint vid)
{
#ifndef PAV_DATA_COMPRESS
    const uint stride = 16;
    uint addr = offset + stride * vid;
    uint4 u4 = buffer.Load4(addr);
    return asfloat(u4);
#else
    switch (format)
    {
        case PAV_DATA_FORMAT_FLOAT:
        {
            const uint stride = 16;
            uint addr = offset + stride * vid;
            uint4 u4 = buffer.Load4(addr);
            return asfloat(u4);
        }
        case PAV_DATA_FORMAT_HALF:
        {
            const uint stride = 8;
            uint addr = offset + stride * vid;
            uint2 u2 = buffer.Load2(addr);
            float4 f4;
            f4.x = f16tof32(u2.x & 0xffffu);
            f4.y = f16tof32((u2.x >> 16) & 0xffffu);
            f4.z = f16tof32(u2.y & 0xffffu);
            f4.w = f16tof32((u2.y >> 16) & 0xffffu);
            return f4;
        }
        case PAV_DATA_FORMAT_PACKED_10_10_10_2_SNORM:
        {
            const uint stride = 4;
            uint addr = offset + stride * vid;
            uint u = buffer.Load(addr);
            return Unpack4_10_10_10_2_snorm(asint(u));
        }
        default:
        {
            return 0;
        }
    }
#endif
}

float3 GetPosition(StaticBufferDesc desc, uint vid)
{
    float3 position = GetFormatFloat3(_staticBuffer, desc.meshVertexDataFormat, desc.meshPositionDataOffset, vid);

#ifdef PAV_DATA_COMPRESS
    if (desc.meshVertexDataFormat == PAV_DATA_FORMAT_PACKED_10_10_10_2_SNORM)
    {
        // snorm [-1.0, 1.0] map back to [0.0, 1.0]
        position = (position + 1.0) * 0.5;
    }

    // apply scale and offset to get raw value
    position = position * desc.meshPositionScale.xyz + desc.meshPositionOffset.xyz;
#endif
    //
    return position;
}

float3 GetNormal(StaticBufferDesc desc, uint vid)
{
    return GetFormatFloat3(_staticBuffer, desc.meshVertexDataFormat, desc.meshNormalDataOffset, vid);
}

float4 GetTangent(StaticBufferDesc desc, uint vid)
{
    return GetFormatFloat4(_staticBuffer, desc.meshVertexDataFormat, desc.meshTangentDataOffset, vid);
}

void GetBaseVertex(StaticBufferDesc desc, uint vid, inout float3 position, inout float3 normal, inout float4 tangent)
{
    position = GetPosition(desc, vid);
    if (desc.meshNormalDataOffset != 0)
    {
        normal = GetNormal(desc, vid);
    }
    if (desc.meshTangentDataOffset != 0)
    {
        tangent = GetTangent(desc, vid);
    }
}

uint4 GetBoneIndices(StaticBufferDesc desc, uint vid)
{
    const uint stride = 8;
    uint addr = desc.meshBoneIndicesDataOffset + stride * vid;
    uint2 u2 = _staticBuffer.Load2(addr);
    uint4 indices;
    indices.x = u2.x & 0xffffu;
    indices.y = (u2.x >> 16) & 0xffffu;
    indices.z = u2.y & 0xffffu;
    indices.w = (u2.y >> 16) & 0xffffu;
    return indices;
}

float4 GetBoneWeights(StaticBufferDesc desc, uint vid)
{
    const uint stride = 16;
    uint addr = desc.meshBoneWeightsDataOffset + stride * vid;
    uint4 u4 = _staticBuffer.Load4(addr);
    return asfloat(u4);
}

float4x4 GetBindposeMatrix(StaticBufferDesc desc, uint index)
{
    const uint stride = 64;
    uint addr = desc.meshBindposesDataOffset + stride * index;
    uint4 row0 = _staticBuffer.Load4(addr + 0);
    uint4 row1 = _staticBuffer.Load4(addr + 16);
    uint4 row2 = _staticBuffer.Load4(addr + 32);
    uint4 row3 = _staticBuffer.Load4(addr + 48);
    return float4x4(asfloat(row0), asfloat(row1), asfloat(row2), asfloat(row3));
}

float3 GetMorphPosition(StaticBufferDesc desc, uint vid, uint morphIndex)
{
    uint offset = desc.morphPositionDataOffset + desc.morphStride * morphIndex;
    float3 position = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, offset, vid);
    return position * desc.morphPositionScale.xyz;
}

float3 GetMorphNormal(StaticBufferDesc desc, uint vid, uint morphIndex)
{
    uint offset = desc.morphNormalDataOffset + desc.morphStride * morphIndex;
    float3 normal = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, offset, vid);
    return normal * desc.morphNormalScale.xyz;
}

float3 GetMorphTangent(StaticBufferDesc desc, uint vid, uint morphIndex)
{
    uint offset = desc.morphTangentDataOffset + desc.morphStride * morphIndex;
    float3 tangent = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, offset, vid);
    return tangent * desc.morphTangentScale.xyz;
}

void GetMorphPackedVertex_PNT(StaticBufferDesc desc, uint vid, uint morphIndex, out float3 position, out float3 normal, out float3 tangent)
{
    position = 0;
    normal = 0;
    tangent = 0;

    const uint stride = 4;
    uint addr = desc.morphPackedVectorOffsetsDataOffset + stride * (desc.morphCount * vid + morphIndex);
    uint offset = _staticBuffer.Load(addr);
    if (offset > 0)
    {
        position = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, offset, 0);
        position *= desc.morphPositionScale.xyz;

        normal = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, offset, 1);
        normal *= desc.morphNormalScale.xyz;

        tangent = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, offset, 2);
        tangent *= desc.morphTangentScale.xyz;
    }
}

void GetMorphPackedVertex_PN(StaticBufferDesc desc, uint vid, uint morphIndex, out float3 position, out float3 normal)
{
    position = 0;
    normal = 0;

    const uint stride = 4;
    uint addr = desc.morphPackedVectorOffsetsDataOffset + stride * (desc.morphCount * vid + morphIndex);
    uint offset = _staticBuffer.Load(addr);
    if (offset > 0)
    {
        position = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, offset, 0);
        position *= desc.morphPositionScale.xyz;

        normal = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, offset, 1);
        normal *= desc.morphNormalScale.xyz;
    }
}

#ifdef PAV_MERGED_TEXTURE

static float4 _mergedTextureSizes[PAV_TEXTURE_KEY_COUNT];
static MergedMaterialData _mergedMaterialData;

#ifdef PAV_MATERIAL_DATA_TEXTURE

Texture2D<float4> _materialDataTexture;
float4 _materialDataTextureSize;

float4 LoadNextDataTexturePixel(inout uint pixelX, inout uint pixelY, uint width)
{
    pixelX += 1;
    if (pixelX == width)
    {
        pixelX = 0;
        pixelY += 1;
    }
    return _materialDataTexture.Load(int3(pixelX, pixelY, 0));
}

void GetTextureSizes()
{
    const uint pixelByteSize = 4 * 4;
    uint addr = 0;
    uint pixelIndex = addr / pixelByteSize;
    uint materialDataTextureWidth = (uint) round(_materialDataTextureSize.x);

    uint pixelX = pixelIndex % materialDataTextureWidth;
    uint pixelY = pixelIndex / materialDataTextureWidth;

    for (uint i = 0u; i < PAV_TEXTURE_KEY_COUNT; ++i)
    {
        float4 f4;

        if (i == 0u)
        {
            f4 = _materialDataTexture.Load(int3(pixelX, pixelY, 0));
        }
        else
        {
            f4 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        }
        _mergedTextureSizes[i] = f4;
    }
}

MergedMaterialData GetMaterialData(uint materialIndex)
{
    const uint pixelByteSize = 4 * 4;
    const uint materialStride = PAV_MERGED_MATERIAL_DATA_SIZE; // align with pixelByteSize
    uint offset = 16 * PAV_TEXTURE_KEY_COUNT; // texture sizes
    uint addr = offset + materialStride * materialIndex;
    uint pixelIndex = addr / pixelByteSize;
    uint materialDataTextureWidth = (uint) round(_materialDataTextureSize.x);

    uint pixelX = pixelIndex % materialDataTextureWidth;
    uint pixelY = pixelIndex / materialDataTextureWidth;

    MergedMaterialData materialData = (MergedMaterialData) 0;

    for (uint i = 0u; i < PAV_TEXTURE_KEY_COUNT; ++i)
    {
        float4 f4;
        
        if (i == 0u)
        {
            f4 = _materialDataTexture.Load(int3(pixelX, pixelY, 0));
        }
        else
        {
            f4 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        }
        materialData.textureInfos[i].uvScaleOffset = f4;

        f4 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.textureInfos[i].textureArrayIndex = asint(f4.x);
    }

    {
        float4 f4 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.outline = f4.x;
        materialData.cutoff = f4.y;
        materialData.smoothness = f4.z;
        materialData.glossMapScale = f4.w;

        f4 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.smoothnessTextureChannel = f4.x;
        materialData.metallic = f4.y;
        materialData.bumpScale = f4.z;
        materialData.parallax = f4.w;

        f4 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.occlusionStrength = f4.x;
        materialData.detailAlbedoMapScale = f4.y;
        materialData.detailNormalMapScale = f4.z;
        materialData.clearCoatMask = f4.w;

        f4 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.clearCoatSmoothness = f4.x;
        materialData.shaderType = f4.y;
        materialData.usingAlbedoHue = f4.z;

        materialData.outlineColor = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.baseColor = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.specColor = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.emissionColor = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);

        materialData.baseMap_ST = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);

        materialData.colorRegion1 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.colorRegion2 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.colorRegion3 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        materialData.colorRegion4 = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);

        for (uint i = 0u; i < PAV_CUSTOM_PROPERTY_KEY_COUNT; ++i)
        {
            materialData.customVecs[i] = LoadNextDataTexturePixel(pixelX, pixelY, materialDataTextureWidth);
        }
    }

    return materialData;
}

#endif

#ifdef PAV_MATERIAL_DATA_BUFFER

ByteAddressBuffer _materialDataBuffer;

float4 LoadMaterialDataBuffer(uint addr)
{
    return asfloat(_materialDataBuffer.Load4(addr));
}

void GetTextureSizes()
{
    const uint stride = 16;
    for (uint i = 0u; i < PAV_TEXTURE_KEY_COUNT; ++i)
    {
        uint addr = stride * i;
        uint4 u4 = _materialDataBuffer.Load4(addr);
        _mergedTextureSizes[i] = asfloat(u4);
    }
}

MergedMaterialData GetMaterialData(uint materialIndex)
{
    const uint materialStride = PAV_MERGED_MATERIAL_DATA_SIZE; // align with pixelByteSize
    const uint textureInfoStride = 32;
    uint offset = 16 * PAV_TEXTURE_KEY_COUNT; // texture sizes
    offset += materialStride * materialIndex;

    MergedMaterialData materialData = (MergedMaterialData) 0;

    for (uint i = 0u; i < PAV_TEXTURE_KEY_COUNT; ++i)
    {
        uint addr = offset + textureInfoStride * i;
        materialData.textureInfos[i].uvScaleOffset = LoadMaterialDataBuffer(addr);
        materialData.textureInfos[i].textureArrayIndex = asuint(LoadMaterialDataBuffer(addr + 16).x);
    }

    offset += textureInfoStride * PAV_TEXTURE_KEY_COUNT;

    {
        uint addr = offset;
        float4 f4 = LoadMaterialDataBuffer(addr + 16 * 0);
        materialData.outline = f4.x;
        materialData.cutoff = f4.y;
        materialData.smoothness = f4.z;
        materialData.glossMapScale = f4.w;

        f4 = LoadMaterialDataBuffer(addr + 16 * 1);
        materialData.smoothnessTextureChannel = f4.x;
        materialData.metallic = f4.y;
        materialData.bumpScale = f4.z;
        materialData.parallax = f4.w;

        f4 = LoadMaterialDataBuffer(addr + 16 * 2);
        materialData.occlusionStrength = f4.x;
        materialData.detailAlbedoMapScale = f4.y;
        materialData.detailNormalMapScale = f4.z;
        materialData.clearCoatMask = f4.w;

        f4 = LoadMaterialDataBuffer(addr + 16 * 3);
        materialData.clearCoatSmoothness = f4.x;
        materialData.shaderType = f4.y;
        materialData.usingAlbedoHue = f4.z;

        materialData.outlineColor = LoadMaterialDataBuffer(addr + 16 * 4);
        materialData.baseColor = LoadMaterialDataBuffer(addr + 16 * 5);
        materialData.specColor = LoadMaterialDataBuffer(addr + 16 * 6);
        materialData.emissionColor = LoadMaterialDataBuffer(addr + 16 * 7);

        materialData.baseMap_ST = LoadMaterialDataBuffer(addr + 16 * 8);

        materialData.colorRegion1 = LoadMaterialDataBuffer(addr + 16 * 9);
        materialData.colorRegion2 = LoadMaterialDataBuffer(addr + 16 * 10);
        materialData.colorRegion3 = LoadMaterialDataBuffer(addr + 16 * 11);
        materialData.colorRegion4 = LoadMaterialDataBuffer(addr + 16 * 12);

        addr += 16 * 13;
        for (uint i = 0u; i < PAV_CUSTOM_PROPERTY_KEY_COUNT; ++i)
        {
            materialData.customVecs[i] = LoadMaterialDataBuffer(addr + 16 * i);
        }
    }

    return materialData;
}

#endif

uint GetMaterialIndex(StaticBufferDesc desc, uint vid)
{
    const uint stride = 4;
    uint addr = desc.mergedMaterialIndexDataOffset + stride * vid;
    uint u = _staticBuffer.Load(addr);
    return u & 0xffu;
}

void GetTextureInfo(uint textureIndex, uint mtlIndex, out float textureArrayIndex, out float4 uvScaleOffset)
{
    uvScaleOffset = _mergedMaterialData.textureInfos[textureIndex].uvScaleOffset;
    textureArrayIndex = _mergedMaterialData.textureInfos[textureIndex].textureArrayIndex;
}

void GetTextureSize(uint textureIndex, out float2 textureSize)
{
    textureSize = _mergedTextureSizes[textureIndex].xy;
}

#endif

uint2 GetMorphIndex2Merged(DynamicBufferDesc desc, uint activeIndex)
{
    const uint stride = 4;
    uint addr = desc.morphIndexDataOffset + stride * activeIndex;
    return _dynamicBuffer.Load2(addr);
}

uint2 GetMorphIndex2(DynamicBufferDesc desc, uint activeIndex)
{
    const uint stride = 4;
    uint addr = desc.morphIndexDataOffset + stride * activeIndex;
    uint2 u2 = _dynamicBuffer.Load2(addr);
    uint2 index2;
    index2.x = u2.x & 0xffffu;
    index2.y = u2.y & 0xffffu;
    return index2;
}

float2 GetMorphWeight2(DynamicBufferDesc desc, uint activeIndex)
{
    const uint stride = 4;
    uint addr = desc.morphWeightDataOffset + stride * activeIndex;
    uint2 u2 = _dynamicBuffer.Load2(addr);
    return asfloat(u2);
}

float4x4 GetBoneMatrix(DynamicBufferDesc desc, uint index)
{
    const uint stride = 64;
    uint addr = desc.boneMatrixDataOffset + stride * index;
    uint4 row0 = _dynamicBuffer.Load4(addr + 0);
    uint4 row1 = _dynamicBuffer.Load4(addr + 16);
    uint4 row2 = _dynamicBuffer.Load4(addr + 32);
    uint4 row3 = _dynamicBuffer.Load4(addr + 48);
    return float4x4(asfloat(row0), asfloat(row1), asfloat(row2), asfloat(row3));
}

void GetMorphVertexContinueLayout_PNT(StaticBufferDesc desc, VertexContinueLayoutInfo layoutInfo, uint vertexOffset, uint morphIndex, out float3 position, out float3 normal, out float3 tangent)
{
    position = 0;
    normal = 0;
    tangent = 0;

    const uint HasMorphNormal = 4;
    const uint HasMorphTangent = 8;
    const uint HasMorphPosition = 16;

    uint addr = vertexOffset + layoutInfo.morphVertexOffset + layoutInfo.morphVertexStride * morphIndex;
    uint offset = 0;
    uint morphAttributeBits = _staticBuffer.Load(addr + offset);
    offset += 4;

    {
        if ((morphAttributeBits & HasMorphPosition) != 0)
        {
            position = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, addr + offset, 0);
            position *= desc.morphPositionScale.xyz;
        }
        offset += GetFormatSize(desc.morphVertexDataFormat, 3);
    }

    if ((layoutInfo.attributeBits & HasMorphNormal) != 0)
    {
        if ((morphAttributeBits & HasMorphNormal) != 0)
        {
            normal = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, addr + offset, 0);
            normal *= desc.morphNormalScale.xyz;
        }
        offset += GetFormatSize(desc.morphVertexDataFormat, 3);
    }

    if ((layoutInfo.attributeBits & HasMorphTangent) != 0)
    {
        if ((morphAttributeBits & HasMorphTangent) != 0)
        {
            tangent = GetFormatFloat3(_staticBuffer, desc.morphVertexDataFormat, addr + offset, 0);
            tangent *= desc.morphTangentScale.xyz;
        }
        offset += GetFormatSize(desc.morphVertexDataFormat, 3);
    }
}

void GetBaseVertexContinueLayout_ApplyMorph_PNT(
    StaticBufferDesc staticBufferDesc,
    DynamicBufferDesc dynamicBufferDesc,
    uint vid,
    inout float3 position,
    inout float3 normal,
    inout float4 tangent)
{
    const uint HasNormal = 1;
    const uint HasTangent = 2;

    if (staticBufferDesc.mergedContinueLayoutInfoDataOffset > 0)
    {
        uint meshIndex;
        uint localVertexID;
        VertexContinueLayoutInfo layoutInfo;
        GetVertexContinueLayoutInfo(staticBufferDesc, vid, meshIndex, localVertexID, layoutInfo);

        uint addr = staticBufferDesc.mergedContinueLayoutVerticesDataOffset + layoutInfo.meshDataOffset + layoutInfo.vertexStride * localVertexID;
        uint offset = 0;
        {
            position = GetFormatFloat3(_staticBuffer, staticBufferDesc.meshVertexDataFormat, addr + offset, 0);
            offset += GetFormatSize(staticBufferDesc.meshVertexDataFormat, 3);

#ifdef PAV_DATA_COMPRESS
            if (staticBufferDesc.meshVertexDataFormat == PAV_DATA_FORMAT_PACKED_10_10_10_2_SNORM)
            {
                // snorm [-1.0, 1.0] map back to [0.0, 1.0]
                position = (position + 1.0) * 0.5;
            }

            // apply scale and offset to get raw value
            position = position * staticBufferDesc.meshPositionScale.xyz + staticBufferDesc.meshPositionOffset.xyz;
#endif
        }

        if ((layoutInfo.attributeBits & HasNormal) != 0)
        {
            normal = GetFormatFloat3(_staticBuffer, staticBufferDesc.meshVertexDataFormat, addr + offset, 0);
            offset += GetFormatSize(staticBufferDesc.meshVertexDataFormat, 3);
        }

        if ((layoutInfo.attributeBits & HasTangent) != 0)
        {
            tangent = GetFormatFloat4(_staticBuffer, staticBufferDesc.meshVertexDataFormat, addr + offset, 0);
            offset += GetFormatSize(staticBufferDesc.meshVertexDataFormat, 4);
        }

        for (uint i = 0; i < dynamicBufferDesc.morphCount; i += 2)
        {
            uint2 index2 = GetMorphIndex2Merged(dynamicBufferDesc, i);
            float2 weight2 = GetMorphWeight2(dynamicBufferDesc, i);
            uint morphMeshIndex = (index2.x & 0xffff0000u) >> 16;
            
            if (morphMeshIndex == meshIndex)
            {
                index2.x = index2.x & 0xffffu;

                float3 deltaPosition;
                float3 deltaNormal;
                float3 deltaTangent;
                GetMorphVertexContinueLayout_PNT(staticBufferDesc, layoutInfo, addr, index2.x, deltaPosition, deltaNormal, deltaTangent);
                position += deltaPosition * weight2.x;
                normal += deltaNormal * weight2.x;
                tangent.xyz += deltaTangent * weight2.x;
            }

            if (i + 1 < dynamicBufferDesc.morphCount)
            {
                morphMeshIndex = (index2.y & 0xffff0000u) >> 16;

                if (morphMeshIndex == meshIndex)
                {
                    index2.y = index2.y & 0xffffu;

                    float3 deltaPosition;
                    float3 deltaNormal;
                    float3 deltaTangent;
                    GetMorphVertexContinueLayout_PNT(staticBufferDesc, layoutInfo, addr, index2.y, deltaPosition, deltaNormal, deltaTangent);
                    position += deltaPosition * weight2.y;
                    normal += deltaNormal * weight2.y;
                    tangent.xyz += deltaTangent * weight2.y;
                }
            }
        }
    }
}

void ApplyMorph_PNT(
    StaticBufferDesc staticBufferDesc,
    DynamicBufferDesc dynamicBufferDesc,
    uint vid,
    inout float3 position,
    inout float3 normal,
    inout float4 tangent)
{
    for (uint i = 0; i < dynamicBufferDesc.morphCount; i += 2)
    {
        uint2 index2 = GetMorphIndex2(dynamicBufferDesc, i);
        float2 weight2 = saturate(GetMorphWeight2(dynamicBufferDesc, i));

        if (staticBufferDesc.morphPackedVectorOffsetsDataOffset > 0)
        {
            float3 deltaPosition;
            float3 deltaNormal;
            float3 deltaTangent;
            GetMorphPackedVertex_PNT(staticBufferDesc, vid, index2.x, deltaPosition, deltaNormal, deltaTangent);
            position += deltaPosition * weight2.x;
            normal += deltaNormal * weight2.x;
            tangent.xyz += deltaTangent * weight2.x;

            if (i + 1 < dynamicBufferDesc.morphCount)
            {
                GetMorphPackedVertex_PNT(staticBufferDesc, vid, index2.y, deltaPosition, deltaNormal, deltaTangent);
                position += deltaPosition * weight2.y;
                normal += deltaNormal * weight2.y;
                tangent.xyz += deltaTangent * weight2.y;
            }
        }
        else
        {
            position += GetMorphPosition(staticBufferDesc, vid, index2.x) * weight2.x;

            if (staticBufferDesc.morphNormalDataOffset != 0)
            {
                normal += GetMorphNormal(staticBufferDesc, vid, index2.x) * weight2.x;
            }

            if (staticBufferDesc.morphTangentDataOffset != 0)
            {
                tangent.xyz += GetMorphTangent(staticBufferDesc, vid, index2.x) * weight2.x;
            }

            if (i + 1 < dynamicBufferDesc.morphCount)
            {
                position += GetMorphPosition(staticBufferDesc, vid, index2.y) * weight2.y;

                if (staticBufferDesc.morphNormalDataOffset != 0)
                {
                    normal += GetMorphNormal(staticBufferDesc, vid, index2.y) * weight2.y;
                }

                if (staticBufferDesc.morphTangentDataOffset != 0)
                {
                    tangent.xyz += GetMorphTangent(staticBufferDesc, vid, index2.y) * weight2.y;
                }
            }
        }
    }
}

void ApplyMorph_PN(
    StaticBufferDesc staticBufferDesc,
    DynamicBufferDesc dynamicBufferDesc,
    uint vid,
    inout float3 position,
    inout float3 normal)
{
    for (uint i = 0; i < dynamicBufferDesc.morphCount; i += 2)
    {
        uint2 index2 = GetMorphIndex2(dynamicBufferDesc, i);
        float2 weight2 = saturate(GetMorphWeight2(dynamicBufferDesc, i));

        if (staticBufferDesc.morphPackedVectorOffsetsDataOffset > 0)
        {
            float3 deltaPosition;
            float3 deltaNormal;
            GetMorphPackedVertex_PN(staticBufferDesc, vid, index2.x, deltaPosition, deltaNormal);
            position += deltaPosition * weight2.x;
            normal += deltaNormal * weight2.x;

            if (i + 1 < dynamicBufferDesc.morphCount)
            {
                GetMorphPackedVertex_PN(staticBufferDesc, vid, index2.y, deltaPosition, deltaNormal);
                position += deltaPosition * weight2.y;
                normal += deltaNormal * weight2.y;
            }
        }
        else
        {
            position += GetMorphPosition(staticBufferDesc, vid, index2.x) * weight2.x;

            if (staticBufferDesc.morphNormalDataOffset != 0)
            {
                normal += GetMorphNormal(staticBufferDesc, vid, index2.x) * weight2.x;
            }
            if (i + 1 < dynamicBufferDesc.morphCount)
            {
                position += GetMorphPosition(staticBufferDesc, vid, index2.y) * weight2.y;

                if (staticBufferDesc.morphNormalDataOffset != 0)
                {
                    normal += GetMorphNormal(staticBufferDesc, vid, index2.y) * weight2.y;
                }
            }
        }
    }
}

void ApplySkin_PNT(
    StaticBufferDesc staticBufferDesc,
    DynamicBufferDesc dynamicBufferDesc,
    uint4 boneIndices,
    float4 boneWeights,
    inout float3 position,
    inout float3 normal,
    inout float4 tangent)
{
    float4 srcPos = float4(position, 1.0);
    position = 0.0;

    float4 srcNormal = float4(normal, 0.0);
    normal = 0.0;

    float4 srcTangent = float4(tangent.xyz, 0.0);
    tangent.xyz = 0.0;

    for (int i = 0; i < 4; ++i)
    {
        float weight = boneWeights[i];
        if (weight < 0.001)
        {
            continue;
        }
        float4x4 bone = GetBoneMatrix(dynamicBufferDesc, boneIndices[i]);
        position += mul(srcPos, bone).xyz * weight;
        normal.xyz += mul(srcNormal, bone).xyz * weight;
        tangent.xyz += mul(srcTangent, bone).xyz * weight;
    }
}

void ApplySkin_PN(
    StaticBufferDesc staticBufferDesc,
    DynamicBufferDesc dynamicBufferDesc,
    uint4 boneIndices,
    float4 boneWeights,
    inout float3 position,
    inout float3 normal)
{
    float4 srcPos = float4(position, 1.0);
    position = 0.0;
    float4 srcNormal = float4(normal, 0.0);
    normal.xyz = 0.0;

    for (int i = 0; i < 4; ++i)
    {
        float weight = boneWeights[i];
        if (weight < 0.001)
        {
            continue;
        }
        float4x4 bone = GetBoneMatrix(dynamicBufferDesc, boneIndices[i]);
        position += mul(srcPos, bone).xyz * weight;
        normal += mul(srcNormal, bone).xyz * weight;
    }
}

#ifdef PAV_OUT_STRUCTURED_BUFFER

float3 GetOutputPosition(StructuredBuffer<uint> outputBuffer, StaticBufferDesc desc, uint vid)
{
    const uint stride = 12;
    uint addr = desc.outputPositionDataOffset + stride * vid;
    uint3 u3;
    u3.x = outputBuffer[addr / 4];
    u3.y = outputBuffer[addr / 4 + 1];
    u3.z = outputBuffer[addr / 4 + 2];
    return asfloat(u3);
}

float3 GetOutputNormal(StructuredBuffer<uint> outputBuffer, StaticBufferDesc desc, uint vid)
{
    const uint stride = 12;
    uint addr = desc.outputNormalDataOffset + stride * vid;
    uint3 u3;
    u3.x = outputBuffer[addr / 4];
    u3.y = outputBuffer[addr / 4 + 1];
    u3.z = outputBuffer[addr / 4 + 2];
    return asfloat(u3);
}

float4 GetOutputTangent(StructuredBuffer<uint> outputBuffer, StaticBufferDesc desc, uint vid)
{
    const uint stride = 16;
    uint addr = desc.outputTangentDataOffset + stride * vid;
    uint4 u4;
    u4.x = outputBuffer[addr / 4];
    u4.y = outputBuffer[addr / 4 + 1];
    u4.z = outputBuffer[addr / 4 + 2];
    u4.w = outputBuffer[addr / 4 + 3];
    return asfloat(u4);
}

void WriteOutputPosition(RWStructuredBuffer<uint> outputBuffer, StaticBufferDesc desc, uint vid, float3 position)
{
    const uint stride = 12;
    uint addr = desc.outputPositionDataOffset + stride * vid;
#ifndef PAV_LEFT_HAND_SPACE
    position.z *= -1;
#endif
    uint3 u3 = asuint(position);
    outputBuffer[addr / 4] = u3.x;
    outputBuffer[addr / 4 + 1] = u3.y;
    outputBuffer[addr / 4 + 2] = u3.z;
}

void WriteOutputNormal(RWStructuredBuffer<uint> outputBuffer, StaticBufferDesc desc, uint vid, float3 normal)
{
    const uint stride = 12;
    uint addr = desc.outputNormalDataOffset + stride * vid;
#ifndef PAV_LEFT_HAND_SPACE
    normal.z *= -1;
#endif
    uint3 u3 = asuint(normal);
    outputBuffer[addr / 4] = u3.x;
    outputBuffer[addr / 4 + 1] = u3.y;
    outputBuffer[addr / 4 + 2] = u3.z;
}

void WriteOutputTangent(RWStructuredBuffer<uint> outputBuffer, StaticBufferDesc desc, uint vid, float4 tangent)
{
    const uint stride = 16;
    uint addr = desc.outputTangentDataOffset + stride * vid;
#ifndef PAV_LEFT_HAND_SPACE
    tangent.z *= -1;
#endif
    uint4 u4 = asuint(tangent);
    outputBuffer[addr / 4] = u4.x;
    outputBuffer[addr / 4 + 1] = u4.y;
    outputBuffer[addr / 4 + 2] = u4.z;
    outputBuffer[addr / 4 + 3] = u4.w;
}

#endif

#ifdef PAV_OUT_RAW_BUFFER

float3 GetOutputPosition(ByteAddressBuffer outputBuffer, StaticBufferDesc desc, uint vid)
{
    const uint stride = 12;
    uint addr = desc.outputPositionDataOffset + stride * vid;
    uint3 u3 = outputBuffer.Load3(addr);
    return asfloat(u3);
}

float3 GetOutputNormal(ByteAddressBuffer outputBuffer, StaticBufferDesc desc, uint vid)
{
    const uint stride = 12;
    uint addr = desc.outputNormalDataOffset + stride * vid;
    uint3 u3 = outputBuffer.Load3(addr);
    return asfloat(u3);
}

float4 GetOutputTangent(ByteAddressBuffer outputBuffer, StaticBufferDesc desc, uint vid)
{
    const uint stride = 16;
    uint addr = desc.outputTangentDataOffset + stride * vid;
    uint4 u4 = outputBuffer.Load4(addr);
    return asfloat(u4);
}

void WriteOutputPosition(RWByteAddressBuffer outputBuffer, StaticBufferDesc desc, uint vid, float3 position)
{
    const uint stride = 12;
    uint addr = desc.outputPositionDataOffset + stride * vid;
#ifndef PAV_LEFT_HAND_SPACE
    position.z *= -1;
#endif
    uint3 u3 = asuint(position);
    outputBuffer.Store3(addr, u3);
}

void WriteOutputNormal(RWByteAddressBuffer outputBuffer, StaticBufferDesc desc, uint vid, float3 normal)
{
    const uint stride = 12;
    uint addr = desc.outputNormalDataOffset + stride * vid;
#ifndef PAV_LEFT_HAND_SPACE
    normal.z *= -1;
#endif
    uint3 u3 = asuint(normal);
    outputBuffer.Store3(addr, u3);
}

void WriteOutputTangent(RWByteAddressBuffer outputBuffer, StaticBufferDesc desc, uint vid, float4 tangent)
{
    const uint stride = 16;
    uint addr = desc.outputTangentDataOffset + stride * vid;
#ifndef PAV_LEFT_HAND_SPACE
    tangent.z *= -1;
#endif
    uint4 u4 = asuint(tangent);
    outputBuffer.Store4(addr, u4);
}

#endif

#endif
