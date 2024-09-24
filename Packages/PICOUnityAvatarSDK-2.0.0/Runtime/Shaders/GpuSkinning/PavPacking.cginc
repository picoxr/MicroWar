#ifndef PAV_PACKING_INCLUDED
#define PAV_PACKING_INCLUDED

static const int OVR_FORMAT_FLOAT_32 = 0;
static const int OVR_FORMAT_HALF_16  = 1;
static const int OVR_FORMAT_UNORM_16 = 2;
static const int OVR_FORMAT_UINT_16 = 3;
static const int OVR_FORMAT_SNORM_10_10_10_2 = 4;
static const int OVR_FORMAT_UNORM_8 = 5;
static const int OVR_FORMAT_UINT_8 = 6;

int bitfieldExtract10(int value, int offset) {
  value = value >> offset;
  value &= 0x03ff;
  if ((value & 0x0200) != 0) {
    value |= 0xfffffc00;
  }
  return value;
}

uint BitfieldExtract(uint data, uint offset, uint numBits)
{
  const uint mask = (1u << numBits) - 1u;
  return (data >> offset) & mask;
}

// With sign extension
int BitfieldExtract(int data, uint offset, uint numBits)
{
  int  shifted = data >> offset;      // Sign-extending (arithmetic) shift
  int  signBit = shifted & (1u << (numBits - 1u));
  uint mask    = (1u << numBits) - 1u;

  return -signBit | (shifted & mask); // Use 2-complement for negation to replicate the sign bit
}

uint BitfieldInsert(uint base, uint insert, uint offset, uint numBits)
{
  uint mask = ~(0xffffffffu << numBits) << offset;
  mask = ~mask;
  base = base & mask;
  return base | (insert << offset);
}

// float * 
uint F32_To_U32_Scaled(float Value, float Scale)
{
	return (uint)floor(Value * Scale + 0.5f);
}

//---------------------------------
// float <=> uint..
uint Pack_F32_To_U32(float val)
{
    return asuint(val);
}
float Unpack_U32_To_F32(uint u)
{
    return asfloat(u);
}

//---------------------------------
// (F16,F16) <=> U32.  
uint Pack_F16x2_To_U32(float2 val)
{
    return f32tof16(val.x) | (f32tof16(val.y) << 16);
}
float2 Unpack_U32_To_F16x2(uint val)
{
    const uint x = val & 0xFFFFu;
    const uint y = (val >> 16) & 0xFFFFu;
    return float2(f16tof32(x), f16tof32(y));
}

//---------------------------------
// (U16, U16) <=> U32
uint Pack_U16x2_To_U32(uint2 val)
{
    return val.x | (val.y << 16);
}
uint2 Unpack_U32_To_U16x2(uint val)
{
    uint x = val & 0xFFFFu;
    uint y = (val >> 16) & 0xFFFFu;
    return uint2(x, y);
}

//---------------------------------
// (F16_UNorm, F16_UNorm) <=> U32.
uint Pack_F16x2_UNorm_To_U32(float2 val)
{
  const uint x = round(saturate(val.x) * 65535);
  const uint y = round(saturate(val.y) * 65535);
  return val.x | (val.y << 16);
}
float2 Unpack_U32_To_U16x2_UNorm(uint val)
{
    float2 retUNorm;
    retUNorm.x = (float)(val & 0xFFFFu) / 65535;
    retUNorm.y = (float)((val >> 16) & 0xFFFFu) / 65535;
    return retUNorm;
}

//---------------------------------
// (U8, U8, U8, U8) <=> U32.
uint Pack_U8x4_To_U32(uint4 val)
{
    return vals.x | (vals.y << 8) | (vals.z << 16) | (vals.w << 24);
}
uint4 Unpack_U32_To_U8x4(uint val)
{
    uint4 ret;
    ret.x = val & 0xFFu;
    ret.y = (val >> 8) & 0xFFu;
    ret.z = (val >> 16) & 0xFFu;
    ret.w = (val >> 24) & 0xFFu;
    return ret;
}

//---------------------------------
// (F32_UNorm, F32_UNorm, F32_UNorm, F32_UNorm) <=> U32.
uint Pack_F32x4_UNorm_To_U32(float4 val)
{
    uint4 uvals;
    uvals.x = round(saturate(val.x) * 255);
    uvals.y = round(saturate(val.y) * 255);
    uvals.z = round(saturate(val.z) * 255);
    uvals.w = round(saturate(val.w) * 255);
    
    return Pack_U8x4_To_U32(uvals);
}
float4 Unpack_U32_To_F32x4_UNorm(uint val)
{
    float4 ret;
    ret.x = (float)(val & 0xFFu) / 255;
    ret.y = (float)((val >> 8) & 0xFFu) / 255;
    ret.z = (float)((val >> 16) & 0xFFu) / 255;
    ret.w = (float)((val >> 24) & 0xFFu) / 255;
    return ret;
}

// Unpack UNorm [0, 1] 4 bytes (as a 32 bit uint)
float4 UnpackUnorm4x8(uint four_packed_values) {
  uint4 non_normalized = UnpackUint4x8(four_packed_values);

  // Convert from 0 -> 255 to 0 -> 1
  const float inv255 = 1.0 / 255.0;

  return float4(
    non_normalized.x * inv255,
    non_normalized.y * inv255,
    non_normalized.z * inv255,
    non_normalized.w * inv255);
}

uint PackUnorm4x8(float4 unorms) {
  const float factor = 255.0;
  const uint x = round(saturate(unorms.x) * factor);
  const uint y = round(saturate(unorms.y) * factor);
  const uint z = round(saturate(unorms.z) * factor);
  const uint w = round(saturate(unorms.w) * factor);

  return PackUint4x8(uint4(x, y, z, w));
}

float4 UnpackSnorm4x10_10_10_2(int four_packed_values) {
  int4 unpackedInt;
  unpackedInt.x = BitfieldExtract(four_packed_values, 0, 10);
  unpackedInt.y = BitfieldExtract(four_packed_values, 10, 10);
  unpackedInt.z = BitfieldExtract(four_packed_values, 20, 10);
  unpackedInt.w = BitfieldExtract(four_packed_values, 30, 2);

  // xyz is -511-511 w is -1-1
  float4 unpacked = float4(unpackedInt);
  // convert all to -1-1
  unpacked.xyz *= 1.0/511.0;

  return unpacked;
}

uint PackSnorm4x10_10_10_2(float4 snorms) {
  static const float3 range = 511.0;
  float4 scaled = 0.0;
  scaled.xyz = snorms.xyz * range; // Convert from -1.0 -> 1.0 to -511.0 -> 511.0
  scaled.xyz = clamp(scaled.xyz, -range, range);
  scaled.xyz = round(scaled.xyz); // Round to nearest int
  scaled.w = clamp(scaled.w, -1.0, 1.0);
  scaled.w = round(scaled.w);

  // now convert from 16 bit to 10 bits, and pack into 32 bits
  int4 integers = int4(scaled);
  uint result = 0;
  result = BitfieldInsert(result, uint(integers.x), 0, 10);
  result = BitfieldInsert(result, uint(integers.y), 10, 10);
  result = BitfieldInsert(result, uint(integers.z), 20, 10);
  result = BitfieldInsert(result, uint(integers.w), 30, 2);

  return result;
}

// Takes 4 "raw, packed" bytes in a 10/10/10/2 format as a signed 32 bit integer (4 bytes).
// The 2 bits is used as a "bonus scale".
// Returns a 3 component (x,y,z) float vector
float3 UnpackVector_10_10_10_2(int packed_value) {
  // bonus scale is still a unorm, if I convert it to an snorm, I lose one value.
  // that does mean I can't use the hardware to convert this format though, it has
  // to be unpacked by hand. If you do have hardware 10_10_10_2 conversion, it may
  // be better to just sample twice? once as unorm, once as snorm.
  uint bonusScaleIndex = uint(packed_value >> 30 & 0x03);

  const float bonus_scale_lookup[4] = {1.0f, 0.5f, 0.25f, 0.125f};
  const float bonus_scale = bonus_scale_lookup[bonusScaleIndex];

  int3 unpackedInt;
  unpackedInt.x = bitfieldExtract10(packed_value, 0);
  unpackedInt.y = bitfieldExtract10(packed_value, 10);
  unpackedInt.z = bitfieldExtract10(packed_value, 20);

  float3 unpacked = float3(unpackedInt);
  // convert all to -1 to 1
  const float inv511 = 1.0 / 511.0;
  unpacked *= float3(inv511, inv511, inv511);

  unpacked = unpacked * bonus_scale;

  return unpacked;
}

float3 UnpackVector_10_10_10_2(in ByteAddressBuffer data_buffer, int address) {
  return UnpackVector_10_10_10_2(data_buffer.Load(address));
}

float4 UnpackSnorm4x10_10_10_2(in ByteAddressBuffer data_buffer, int address) {
  const int packed_value = data_buffer.Load(address);
  return UnpackSnorm4x10_10_10_2(packed_value);
}

float3 UnpackSnorm3x10_10_10_2(in ByteAddressBuffer data_buffer, int address) {
  return UnpackSnorm4x10_10_10_2(data_buffer, address).xyz;
}

// 3x 32 bit uint -> 3x 32 bit float
float3 UnpackFloat3x32(in ByteAddressBuffer data_buffer, int address) {
  const uint3 packed_data = data_buffer.Load3(address);
  return asfloat(packed_data);
}

// 4x 32 bit uint -> 4x 32 bit float
float4 UnpackFloat4x32(in ByteAddressBuffer data_buffer, int address) {
  const uint4 packed_data = data_buffer.Load4(address);
  return asfloat(packed_data);
}

// 16x 32 bit uint -> 32 bit float4x4
float4x4 UnpackFloat16x32(in ByteAddressBuffer data_buffer, int address) {
  float4 r0 = UnpackFloat4x32(data_buffer, address);
  float4 r1 = UnpackFloat4x32(data_buffer, address + 16);
  float4 r2 = UnpackFloat4x32(data_buffer, address + 32);
  float4 r3 = UnpackFloat4x32(data_buffer, address + 48);

  return float4x4(r0, r1, r2, r3);
}

// 2x 32 bit uint -> 3x 16 bit "half floats"
float3 UnpackHalf3x16(in ByteAddressBuffer data_buffer, int address) {
  uint2 packed_data = data_buffer.Load2(address);
  float2 xy = UnpackHalf2x16(packed_data.x);
  float z = UnpackHalf2x16(packed_data.y).x;

  return float3(xy, z);
}

// 2x 32 bit uint -> 4x 16 bit "half floats"
float4 UnpackHalf4x16(in ByteAddressBuffer data_buffer, int address) {
  uint2 packed_data = data_buffer.Load2(address);
  float2 xy = UnpackHalf2x16(packed_data.x);
  float2 zw = UnpackHalf2x16(packed_data.y);

  return float4(xy, zw);
}

// 2x 32 bit uint -> 3x 16-bit unsigned int
uint3 UnpackUint3x16(in ByteAddressBuffer data_buffer, int address) {
  uint2 packed_data = data_buffer.Load2(address);
  float2 xy = UnpackUint2x16(packed_data.x);
  float z = UnpackUint2x16(packed_data.y).x;

  return float3(xy, z);
}

// 2x 32 bit uint -> 4x 16-bit unsigned int
uint4 UnpackUint4x16(in ByteAddressBuffer data_buffer, int address) {
  uint2 packed_data = data_buffer.Load2(address);
  float2 xy = UnpackUint2x16(packed_data.x);
  float2 zw = UnpackUint2x16(packed_data.y);

  return float4(xy, zw);
}

// 2x 32-bit uint -> 3x 16-bit unsigned normalized
float3 UnpackUnorm3x16(in ByteAddressBuffer data_buffer, int address) {
  uint2 packed_data = data_buffer.Load2(address);
  float2 xy = UnpackUnorm2x16(packed_data.x);
  float z = UnpackUnorm2x16(packed_data.y).x;

  return float3(xy, z);
}

// 2x 32-bit uint -> 4x 16-bit unsigned normalized
float4 UnpackUnorm4x16(in ByteAddressBuffer data_buffer, int address) {
  uint2 packed_data = data_buffer.Load2(address);
  float2 xy = UnpackUnorm2x16(packed_data.x);
  float2 zw = UnpackUnorm2x16(packed_data.y);

  return float4(xy, zw);
}

// 1x 32-bit uint -> 3x 8-bit unsigned int
uint3 UnpackUint3x8(in ByteAddressBuffer data_buffer, int address) {
  return UnpackUint4x8(data_buffer.Load(address)).xyz;
}

// 1x 32 bit uint -> 4x 8 bit unsigned normalized
float4 UnpackUint4x8(in ByteAddressBuffer data_buffer, int address) {
  return UnpackUint4x8(data_buffer.Load(address));
}


// 1x 32-bit uint -> 3x 8-bit unsigned normalized
float3 UnpackUnorm3x8(in ByteAddressBuffer data_buffer, int address) {
  return UnpackUnorm4x8(data_buffer.Load(address)).xyz;
}

// 1x 32-bit uint -> 4x 8-bit unsigned normalized
float4 UnpackUnorm4x8(in ByteAddressBuffer data_buffer, int address) {
  return UnpackUnorm4x8(data_buffer.Load(address));
}

#endif
