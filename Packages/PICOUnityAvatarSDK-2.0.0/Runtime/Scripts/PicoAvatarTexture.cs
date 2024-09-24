using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		#region GraphicTypes

		// Image pixel format supported by Avatar SDK.
		public enum AvatarPixelFormat
		{
			Invalid = 0,
			A8Unorm,
			L8Unorm,
			LA8Unorm,
			GR4Unorm,
			ABGR4Unorm,
			ARGB4Unorm,
			RGBA4Unorm,
			BGRA4Unorm,
			B5G6R5Unorm,
			R5G6B5Unorm,
			A1BGR5Unorm,
			A1RGB5Unorm,
			RGB5A1Unorm,
			BGR5A1Unorm,

			R8Unorm,
			R8Snorm,
			R8Uscaleld,
			R8Sscaled,
			R8Uint,
			R8Sint,
			R8_sRGB,
			RG8Unorm,
			RG8Snorm,
			RG8Uscaled,
			RG8Sscaled,
			RG8Uint,
			RG8Sint,
			RG8_sRGB,

			RGB8Unorm,
			RGB8Snorm,
			RGB8Uscaled,
			RGB8Sscaled,
			RGB8Uint,
			RGB8Sint,
			RGB8_sRGB,
			BGR8Unorm,
			BGR8Snorm,
			BGR8Uscaled,
			BGR8Sscaled,
			BGR8Uint,
			BGR8Sint,
			BGR8_sRGB,

			RGBA8Unorm,
			RGBA8Snorm,
			RGBA8Uscaled,
			RGBA8Sscaled,
			RGBA8Uint,
			RGBA8Sint,
			RGBA8_sRGB,
			BGRA8Unorm,
			BGRA8Snorm,
			BGRA8Uscaled,
			BGRA8Sscaled,
			BGRA8Uint,
			BGRA8Sint,
			BGRA8_sRGB,
			ABGR8Unorm,
			ABGR8Snorm,
			ABGR8Uscaled,
			ABGR8Sscaled,
			ABGR8Uint,
			ABGR8Sint,
			ABGR8_sRGB,

			BGR10A2Unorm,
			BGR10A2Snorm,
			BGR10A2Uscaled,
			BGR10A2Sscaled,
			BGR10A2Uint,
			BGR10A2Sint,
			RGB10A2Unorm,
			RGB10A2Snorm,
			RGB10A2Uscaled,
			RGB10A2Sscaled,
			RGB10A2Uint,
			RGB10A2Sint,

			R16Unorm,
			R16Snorm,
			R16Uscaleld,
			R16Sscaled,
			R16Uint,
			R16Sint,
			R16Sfloat,
			RG16Unorm,
			RG16Snorm,
			RG16Uscaled,
			RG16Sscaled,
			RG16Uint,
			RG16Sint,
			RG16Sfloat,
			RGB16Unorm,
			RGB16Snorm,
			RGB16Uscaled,
			RGB16Sscaled,
			RGB16Uint,
			RGB16Sint,
			RGB16Sfloat,
			RGBA16Unorm,
			RGBA16Snorm,
			RGBA16Uscaled,
			RGBA16Sscaled,
			RGBA16Uint,
			RGBA16Sint,
			RGBA16Sfloat,

			R32Uint,
			R32Sint,
			R32Sfloat,
			RG32Uint,
			RG32Sint,
			RG32Sfloat,
			RGB32Uint,
			RGB32Sint,
			RGB32Sfloat,
			RGBA32Uint,
			RGBA32Sint,
			RGBA32Sfloat,

			R64Uint,
			R64Sint,
			R64Sfloat,
			RG64Uint,
			RG64Sint,
			RG64Sfloat,
			RGB64Uint,
			RGB64Sint,
			RGB64Sfloat,
			RGBA64Uint,
			RGBA64Sint,
			RGBA64Sfloat,

			RG11B10Ufloat,
			RGB9E5Ufloat,

			D16Unorm,
			D24X8Unorm,
			D32Sfloat,
			S8Uint,
			D16UnormS8Uint,
			D24UnormS8Uint,
			D32SfloatS8Uint,

			BC1_RGBUnorm,
			BC1_RGB_sRGB,
			BC1_RGBAUnorm,
			BC1_RGBA_sRGB,
			BC2_RGBAUnorm,
			BC2_RGBA_sRGB,
			BC3_RGBAUnorm,
			BC3_RGBA_sRGB,
			BC4_RUnorm,
			BC4_RSnorm,
			BC5_RGUnorm,
			BC5_RGSnorm,
			BC6H_RGBUfloat,
			BC6H_RGBSfloat,
			BC7_RGBAUnorm,
			BC7_RGBAUnorm_sRGB,

			ETC1_RGB8Unorm,
			ETC2_RGB8Unorm,
			ETC2_RGB8_sRGB,
			ETC2_RGB8A1Unorm,
			ETC2_RGB8A1_sRGB,
			ETC2_RGBA8Unorm,
			ETC2_RGBA8_sRGB,
			EAC_R11Unorm,
			EAC_R11Snorm,
			EAC_RG11Unorm,
			EAC_RG11Snorm,

			ASTC_4x4_LDR,
			ASTC_4x4_sRGB,
			ASTC_5x4_LDR,
			ASTC_5x4_sRGB,
			ASTC_5x5_LDR,
			ASTC_5x5_sRGB,
			ASTC_6x5_LDR,
			ASTC_6x5_sRGB,
			ASTC_6x6_LDR,
			ASTC_6x6_sRGB,
			ASTC_8x5_LDR,
			ASTC_8x5_sRGB,
			ASTC_8x6_LDR,
			ASTC_8x6_sRGB,
			ASTC_8x8_LDR,
			ASTC_8x8_sRGB,
			ASTC_10x5_LDR,
			ASTC_10x5_sRGB,
			ASTC_10x6_LDR,
			ASTC_10x6_sRGB,
			ASTC_10x8_LDR,
			ASTC_10x8_sRGB,
			ASTC_10x10_LDR,
			ASTC_10x10_sRGB,
			ASTC_12x10_LDR,
			ASTC_12x10_sRGB,
			ASTC_12x12_LDR,
			ASTC_12x12_sRGB,

			PVRTC1_RGB_2BPP,
			PVRTC1_RGB_4BPP,
			PVRTC1_RGBA_2BPP,
			PVRTC1_RGBA_4BPP,
			PVRTC2_RGBA_2BPP,
			PVRTC2_RGBA_4BPP,
			PVRTC1_RGB_2BPP_sRGB,
			PVRTC1_RGB_4BPP_sRGB,
			PVRTC1_RGBA_2BPP_sRGB,
			PVRTC1_RGBA_4BPP_sRGB,
			PVRTC2_RGBA_2BPP_sRGB,
			PVRTC2_RGBA_4BPP_sRGB,

			GBGR8_422_Unorm,
			BGRG8_422_Unorm,
			G8_B8_R8_3PLANE_420_Unorm,
			G8_BR8_2PLANE_420_Unorm,
			G8_B8_R8_3PLANE_422_Unorm,
			G8_BR8_2PLANE_422_UNnorm,
			G8_B8_R8_3PLANE_444_Unorm,

			// Only For GL
			RGB16Sfloat_GL_FLOAT,
			RGBA16Sfloat_GL_FLOAT,
		};

		// Data element type in Avatar SDK.
		public enum AvatarDataType
		{
			Invalid = 0,
			U8norm,
			S8norm,
			U8,
			S8,
			U16norm,
			S16norm,
			U16,
			S16,

			//    U32norm,
			//    S32norm,
			U32,
			S32,

			//    U64,
			//    S64,
			//    X32,
			F16,
			F32,
			//    F64,
		};

		// Image type in Avatar SDK.
		public enum ImageType
		{
			RENDERBUFFER,
			RENDERBUFFER_MS,
			TEXTURE_1D,
			TEXTURE_2D,
			TEXTURE_3D,
			TEXTURE_CUBE,
			TEXTURE_BUFFER,
			TEXTURE_2D_MS,
			TEXTURE_1D_ARRAY,
			TEXTURE_2D_ARRAY,
			TEXTURE_CUBE_ARRAY,
			TEXTURE_2D_MS_ARRAY,
			TEXTURE_EXTERNAL,
			TEXTURE_RECTANGLE,
			ImageTypeCount,
			RENDERBUFFER_MS_ATTACH,
			INVALID,
		};

		#endregion


		/// <summary>
		/// Texture is bound to a texture asset. The content is filled by native part.
		/// </summary>
		public class AvatarTexture : ReferencedObject
		{
			#region Public Properties

			public Texture runtimeTexture
			{
				get => _texture;
			}

			public Texture runtimeMergedTexture
			{
				get => _mergedTexture;
			}

			// memory data size.
			public uint dataSize
			{
				get => _dataByteSize;
			}

			#endregion


			#region Public Methods

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="texCacheKey">Cache key</param>
			public AvatarTexture(long texCacheKey)
			{
				_textureCacheKey = texCacheKey;
			}

			/// <summary>
			/// Create texture with texture information.
			/// @note
			/// Retain will be invoked for retained object.
			/// </summary>
			/// <param name="textureInfo">Configuration of texture</param>
			/// <param name="isLinear">Is in linear space</param>
			/// <returns>AvatarTexture created</returns>
			public static AvatarTexture CreateAndRefTexture(ref TextureInfo textureInfo, bool isLinear)
			{
				if (textureInfo.texObject == System.IntPtr.Zero)
				{
					return null;
				}

				long textureCacheKey = (textureInfo.texObject.ToInt64() << 16) + textureInfo.instanceKey;
				AvatarTexture avatarTex;
				if (_textures.TryGetValue(textureCacheKey, out avatarTex))
				{
					avatarTex.Retain();
					//
					return avatarTex;
				}

				// if not exist in global table, try create and load.
				avatarTex = new AvatarTexture(textureCacheKey);

				if (!avatarTex.Create(textureInfo, isLinear))
				{
					avatarTex.CheckDelete();
					return null;
				}

				// add to global map.
				_textures.Add(textureCacheKey, avatarTex);
				//
				avatarTex.Retain();
				//
				return avatarTex;
			}

			#endregion


			#region Protected Methods

			// Derived class can override the method to release resources when the object will be destroyed.
			protected override void OnDestroy()
			{
				if (_texture)
				{
					_textureCount -= 1;
					//if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
					//{
					//    AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format("destroy texture, totalAlive: {0}", _textureCount));
					//}
				}

				if (_mergedTexture)
				{
					_textureCount -= 1;
					//if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
					//{
					//    AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format("destroy texture, totalAlive: {0}", _textureCount));
					//}
				}

				if (_texture != null)
				{
					Utility.DestroyImmediate(_texture);
					_texture = null;
				}

				if (_mergedTexture != null)
				{
					Utility.DestroyImmediate(_mergedTexture);
					_mergedTexture = null;
				}

				// remove from cache.
				if (_textureCacheKey != 0)
				{
					_textures.Remove(_textureCacheKey);
					_textureCacheKey = 0;
				}

				//
				base.OnDestroy();
			}

			#endregion


			#region Private Fields

			//
			private TextureInfo _textureInfo;

			// key of texture in cache.
			private long _textureCacheKey = 0;

			// unity texturedsa
			private Texture _texture;
			private Texture _mergedTexture;

			// data size
			private uint _dataByteSize = 0;

			private static Dictionary<long, AvatarTexture> _textures = new Dictionary<long, AvatarTexture>();
			private static int _textureCount = 0;

			#endregion


			#region Private Methods

			private bool Create(TextureInfo textureInfo, bool isLinear)
			{
				_textureInfo = textureInfo;
				if (textureInfo.version > 0)
				{
					isLinear = textureInfo.sRGB == 0 ? true : false;
				}
				// check size.
				if (textureInfo.width == 0 || textureInfo.height == 0)
				{
					return false;
				}

				if (!Utility.EnableRenderObject)
				{
					return true;
				}

				try
				{
					var format = Pico.Avatar.Utility.GetUnityTextureFormat((AvatarPixelFormat)textureInfo.format);
					if (textureInfo.imageType == (ushort)ImageType.TEXTURE_2D)
					{
						//
						//if (AvatarEnv.NeedLog(DebugLogMask.AssetTrivial))
						//{
						//    AvatarEnv.Log(DebugLogMask.AssetTrivial, "start buildTexture");
						//}

						var tex = new Texture2D((int)textureInfo.width, (int)textureInfo.height, format,
							textureInfo.mipsCount > 1, isLinear);
						if (tex == null)
						{
							if (AvatarEnv.NeedLog(DebugLogMask.GeneralWarn))
							{
								AvatarEnv.Log(DebugLogMask.GeneralWarn, "new Texture2D error. format maybe wrong!");
							}

							return false;
						}

						var texData = tex.GetRawTextureData<byte>();

						var data = new TextureSliceData();
						data.version = 0;
						data.width = textureInfo.width;
						data.height = textureInfo.height;
						data.format = textureInfo.format;
						data.mips = textureInfo.mipsCount;
						data.face = 0;
						data.dataByteSize = (uint)texData.Length;
						// cache data size.
						_dataByteSize = (uint)texData.Length;

						unsafe
						{
							data.data = (System.IntPtr)texData.GetUnsafePtr();
						}

						if (pav_AvatarTexture_GetSliceFaceData(textureInfo.texObject, ref data) != NativeResult.Success)
						{
							Object.DestroyImmediate(tex);
							return false;
						}
						// upload texture data to gpu.
#if UNITY_EDITOR
						tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
#else
                        tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
#endif
						//
						_texture = tex;

						_textureCount += 1;

						//if (AvatarEnv.NeedLog(DebugLogMask.AssetTrivial))
						//{
						//    AvatarEnv.Log(DebugLogMask.AssetTrivial, string.Format("end buildTexture. w:{0} h:{1} totalAlive:{2}", tex.width, tex.height, _textureCount));
						//}
					}
					else if (textureInfo.imageType == (ushort)ImageType.TEXTURE_2D_ARRAY)
					{
						var tex2dArray = new Texture2DArray((int)textureInfo.width, (int)textureInfo.height,
							(int)textureInfo.slicesCount, format, textureInfo.mipsCount > 1, isLinear);
						if (tex2dArray == null)
						{
							Debug.LogError("new Texture2DArray error. format maybe wrong!");
							return false;
						}

						//
						//if(AvatarEnv.NeedLog(DebugLogMask.AssetTrivial))
						//{
						//    AvatarEnv.Log(DebugLogMask.AssetTrivial, "start buildTextureArray");
						//}

						var texData = new NativeArray<byte>((int)(textureInfo.width * textureInfo.height * 4),
							Allocator.Temp, NativeArrayOptions.UninitializedMemory);
						bool error = false;

						var data = new TextureSliceData();
						data.version = 0;
						data.width = textureInfo.width;
						data.height = textureInfo.height;
						data.format = textureInfo.format;
						data.face = 0;
						// cache data size.
						_dataByteSize = (uint)0;
						//
						for (int i = 0; i < textureInfo.mipsCount; ++i)
						{
							data.mips = (ushort)i;
							for (int j = 0; j < textureInfo.slicesCount; ++j)
							{
								data.slice = (ushort)j;
								unsafe
								{
									data.data = (System.IntPtr)texData.GetUnsafePtr();
								}

								if (pav_AvatarTexture_GetSliceFaceData(textureInfo.texObject, ref data) !=
								    NativeResult.Success)
								{
									error = true;
									break;
								}

								//
								_dataByteSize += data.dataByteSize;
								//
								tex2dArray.SetPixelData(texData, i, j, 0);
							}

							if (error)
							{
								break;
							}
						}

						texData.Dispose();

						if (error)
						{
							Object.DestroyImmediate(tex2dArray);
							return false;
						}

#if UNITY_EDITOR
						tex2dArray.Apply(updateMipmaps: false, makeNoLongerReadable: false);
#else
                        tex2dArray.Apply(updateMipmaps: false, makeNoLongerReadable: true);
#endif

						_mergedTexture = tex2dArray;

						_textureCount += 1;

						//
						//if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
						//{
						//    AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format("end buildTextureArray w:{0} h:{1} d:{2} totalAlive:{3}", tex2dArray.width, tex2dArray.height, tex2dArray.depth, _textureCount));
						//}
					}
				}
				catch (System.Exception e)
				{
					if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
					{
						AvatarEnv.Log(DebugLogMask.AvatarLoad, e.Message);
					}

					return false;
				}

				{
					var tex = _texture ? _texture : (_mergedTexture ? _mergedTexture : null);
					tex.wrapModeU = ConvertAvatarTextureWrapMode(textureInfo.wrapModeS);
					tex.wrapModeV = ConvertAvatarTextureWrapMode(textureInfo.wrapModeR);
					tex.filterMode = FilterMode.Bilinear;
				}

				return true;
			}

			private static TextureWrapMode ConvertAvatarTextureWrapMode(sbyte mode)
			{
				AvatarTextureWrapMode wrapMode = (AvatarTextureWrapMode)mode;
				switch (wrapMode)
				{
					case AvatarTextureWrapMode.REPEAT:
						return TextureWrapMode.Repeat;
					case AvatarTextureWrapMode.CLAMP:
					case AvatarTextureWrapMode.BORDER:
						return TextureWrapMode.Clamp;
					case AvatarTextureWrapMode.MIRROR:
						return TextureWrapMode.Mirror;
					default:
						return TextureWrapMode.Clamp;
				}
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;


			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarTexture_GetSliceFaceData(System.IntPtr nativeHandle,
				ref TextureSliceData sliceFaceData);

			#endregion
		}
	}
}