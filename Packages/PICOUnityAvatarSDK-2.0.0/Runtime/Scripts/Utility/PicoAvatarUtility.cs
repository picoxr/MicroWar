using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Pico
{
	namespace Avatar
	{
		public static class Utility
		{
			#region File Path

			[Tooltip(
				"desired avatar cache directory. Must be set before invoke Start and StartApp. e.g. Application.persistentDataPath + \"/PicoAvatarCache\". if is empty, \"/sdcard/AvatarCache\" will be used.")]
			public static string desiredAvatarCacheDirectory;

			/// <summary>
			/// Convert to unix path name and add '/' to end.
			/// e.g.  "dir\\file1\\file2" => "dir/file1/file2/"
			/// </summary>
			/// <param name="pathName"></param>
			/// <returns></returns>
			public static string GetPathNameWithEndSlash(string pathName)
			{
				var ret = pathName.Replace('\\', '/');
				if (!ret.EndsWith("/"))
				{
					ret += "/";
				}

				return ret;
			}

			#endregion


			#region Profiler

			public static bool EnableSDKUpdate = true;
			public static bool EnableRenderObject = true;

			#endregion


			#region IsNullOrEmpty for objects

			public static bool IsNullOrEmpty(string val)
			{
				return string.IsNullOrEmpty(val);
			}

			public static bool IsNullOrEmpty(MemoryStream val)
			{
				return val == null || val.Length == 0;
			}

			public static bool IsNullOrEmpty(byte[] bytes)
			{
				return bytes == null || bytes.Length == 0;
			}

			public static bool IsNullOrEmpty<T>(T[] array)
			{
				return array == null || array.Length == 0;
			}

			public static bool IsNullOrEmpty(MemoryView data)
			{
				return data == null || data.length == 0;
			}

			#endregion


			#region Miscs

			public static void Destroy<T>(T field) where T : UnityEngine.Object
			{
				if (field)
				{
					UnityEngine.Object.Destroy(field);
				}
			}

			public static void DestroyImmediate<T>(T field) where T : UnityEngine.Object
			{
				if (field)
				{
					UnityEngine.Object.DestroyImmediate(field);
				}
			}

			public static void LogMeshData(Mesh mesh, string logFileName)
			{
#if UNITY_ANDROID && !UNITY_EDITOR
                string logFileFullPath = Path.Combine(Application.persistentDataPath, logFileName);
#else
				string logFileFullPath = Path.Combine(Application.streamingAssetsPath, logFileName);
#endif
				FileStream file = new System.IO.FileStream(logFileFullPath, System.IO.FileMode.Create);
				StreamWriter sw = new System.IO.StreamWriter(file);

				sw.WriteLine(string.Format("Mesh: {0}", mesh.name));

				//Vertices
				sw.WriteLine(string.Format("-Vertices ({0})", mesh.vertexCount));

				var Vertices = mesh.vertices;
				for (int i = 0; i < Vertices.Length; ++i)
				{
					sw.WriteLine(string.Format(" （{0}, {1}, {2}）", Vertices[i].x, Vertices[i].y, Vertices[i].z));
				}

				sw.WriteLine();

				//Indices
				sw.WriteLine(string.Format("-SubMeshes ({0})", mesh.subMeshCount));

				for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; ++subMeshIdx)
				{
					var indices = mesh.GetIndices(subMeshIdx);
					int triangleCount = indices.Length / 3;

					sw.WriteLine(string.Format("-SubMesh {0} ({1} triangles)", subMeshIdx, triangleCount));

					for (int i = 0; i < triangleCount; ++i)
					{
						sw.WriteLine(" triangle {0}: ({1}, {2}, {3})", i, indices[3 * i], indices[3 * i + 1],
							indices[3 * i + 2]);
					}
				}

				sw.WriteLine();

				//BoneWeights
				var boneWeights = mesh.boneWeights;
				sw.WriteLine(string.Format("-BoneWeights ({0})", boneWeights.Length));

				for (int i = 0; i < boneWeights.Length; ++i)
				{
					string boneWrightInfo = " BoneWeight " + i;

					var boneWeight = boneWeights[i];

					//do
					//{
					//    if (boneWeight.boneIndex0 >= 0 && boneWeight.weight0 > 0)
					//        boneWrightInfo += string.Format(" ({0}:{1})", boneWeight.boneIndex0, boneWeight.weight0);
					//    else
					//        break;
					//    if (boneWeight.boneIndex1 >= 0 && boneWeight.weight1 > 0)
					//        boneWrightInfo += string.Format(" ({0}:{1})", boneWeight.boneIndex1, boneWeight.weight1);
					//    else
					//        break;
					//    if (boneWeight.boneIndex2 >= 0 && boneWeight.weight2 > 0)
					//        boneWrightInfo += string.Format(" ({0}:{1})", boneWeight.boneIndex2, boneWeight.weight2);
					//    else
					//        break;
					//    if (boneWeight.boneIndex3 >= 0 && boneWeight.weight3 > 0)
					//        boneWrightInfo += string.Format(" ({0}:{1})", boneWeight.boneIndex3, boneWeight.weight3);
					//    else
					//        break;
					//} while (false);

					boneWrightInfo += string.Format(" ({0}:{1})", boneWeight.boneIndex0, boneWeight.weight0);
					boneWrightInfo += string.Format(" ({0}:{1})", boneWeight.boneIndex1, boneWeight.weight1);
					boneWrightInfo += string.Format(" ({0}:{1})", boneWeight.boneIndex2, boneWeight.weight2);
					boneWrightInfo += string.Format(" ({0}:{1})", boneWeight.boneIndex3, boneWeight.weight3);

					sw.WriteLine(boneWrightInfo);
				}

				sw.WriteLine();

				//BindPoses
				var bindPoses = mesh.bindposes;
				sw.WriteLine(string.Format("-bindPoses ({0})", bindPoses.Length));

				for (int i = 0; i < bindPoses.Length; ++i)
				{
					var bindPose = bindPoses[i];
					Vector3 Position = bindPose.MultiplyPoint(Vector3.zero);
					Vector3 EularRotation = bindPose.rotation.eulerAngles;
					Vector3 LossyScale = bindPose.lossyScale;
					sw.WriteLine(" BindPose {0}: (Position: {1}, EularRotation: {2}, LossyScale: {3})",
						i, Position, EularRotation, LossyScale);
				}

				//Output
				sw.Flush();
				sw.Close();
				file.Close();
			}

			public static void LogArrayData<T>(T[] data, string logFileName, string prefix = "-")
			{
				if (data == null)
					return;

				string logFileFullPath = System.IO.Path.Combine(AvatarEnv.workSpacePath, logFileName);
				System.IO.FileStream file = new System.IO.FileStream(logFileFullPath, System.IO.FileMode.Create);
				System.IO.StreamWriter sw = new System.IO.StreamWriter(file);

				sw.WriteLine(string.Format("{0}: {1}", typeof(T).ToString(), data.Length));

				for (int i = 0; i < data.Length; ++i)
				{
					sw.WriteLine(string.Format(prefix + "{0}", data[i]));
				}

				sw.Flush();
				sw.Close();
				file.Close();
			}

			#endregion


			#region Format Helper

			/// <summary>
			/// Gets unity texture format from avatar texture pixel format.
			/// </summary>
			/// <param name="pixelFormat"></param>
			/// <returns></returns>
			public static TextureFormat GetUnityTextureFormat(Pico.Avatar.AvatarPixelFormat pixelFormat)
			{
				switch (pixelFormat)
				{
					case Pico.Avatar.AvatarPixelFormat.A8Unorm:
					case Pico.Avatar.AvatarPixelFormat.L8Unorm:
					case Pico.Avatar.AvatarPixelFormat.LA8Unorm:
					case Pico.Avatar.AvatarPixelFormat.R8Unorm:
					case Pico.Avatar.AvatarPixelFormat.R8Snorm:
					case Pico.Avatar.AvatarPixelFormat.R8Uscaleld:
					case Pico.Avatar.AvatarPixelFormat.R8Sscaled:
					case Pico.Avatar.AvatarPixelFormat.R8Uint:
					case Pico.Avatar.AvatarPixelFormat.R8Sint:
					case Pico.Avatar.AvatarPixelFormat.R8_sRGB:
						return TextureFormat.Alpha8;
					case Pico.Avatar.AvatarPixelFormat.RGBA8Sint:
					case Pico.Avatar.AvatarPixelFormat.RGBA8Snorm:
					case Pico.Avatar.AvatarPixelFormat.RGBA8Uint:
					case Pico.Avatar.AvatarPixelFormat.RGBA8Unorm:
					case Pico.Avatar.AvatarPixelFormat.RGBA8Uscaled:
					case Pico.Avatar.AvatarPixelFormat.RGBA8_sRGB:
						return TextureFormat.RGBA32;
					case Pico.Avatar.AvatarPixelFormat.ABGR8Sint:
					case Pico.Avatar.AvatarPixelFormat.ABGR8Snorm:
					case Pico.Avatar.AvatarPixelFormat.ABGR8Uint:
					case Pico.Avatar.AvatarPixelFormat.ABGR8Unorm:
					case Pico.Avatar.AvatarPixelFormat.ABGR8Uscaled:
					case Pico.Avatar.AvatarPixelFormat.ABGR8_sRGB:
						return TextureFormat.ARGB32;

					case Pico.Avatar.AvatarPixelFormat.RGB8Sint:
					case Pico.Avatar.AvatarPixelFormat.RGB8Uint:
					case Pico.Avatar.AvatarPixelFormat.RGB8Snorm:
					case Pico.Avatar.AvatarPixelFormat.RGB8Unorm:
					case Pico.Avatar.AvatarPixelFormat.RGB8Sscaled:
					case Pico.Avatar.AvatarPixelFormat.RGB8Uscaled:
					case Pico.Avatar.AvatarPixelFormat.RGB8_sRGB:
						return TextureFormat.RGB24;

					// TODO: RGB not implemented yet.
					case Pico.Avatar.AvatarPixelFormat.ASTC_4x4_sRGB:
						return TextureFormat.ASTC_4x4;
					case Pico.Avatar.AvatarPixelFormat.ASTC_5x5_sRGB:
						return TextureFormat.ASTC_5x5;
					case Pico.Avatar.AvatarPixelFormat.ASTC_6x6_sRGB:
						return TextureFormat.ASTC_6x6;
					case Pico.Avatar.AvatarPixelFormat.ASTC_8x8_sRGB:
						return TextureFormat.ASTC_8x8;
					case Pico.Avatar.AvatarPixelFormat.ASTC_10x10_sRGB:
						return TextureFormat.ASTC_10x10;
					case Pico.Avatar.AvatarPixelFormat.ASTC_12x12_sRGB:
						return TextureFormat.ASTC_12x12;
					//
					case Pico.Avatar.AvatarPixelFormat.ETC2_RGB8A1_sRGB:
						return TextureFormat.ETC2_RGBA1;
					case Pico.Avatar.AvatarPixelFormat.ETC2_RGBA8_sRGB:
						return TextureFormat.ETC2_RGBA8;
					case Pico.Avatar.AvatarPixelFormat.ETC2_RGB8_sRGB:
						return TextureFormat.ETC2_RGB;
					// 
					case Pico.Avatar.AvatarPixelFormat.BC4_RSnorm:
					case Pico.Avatar.AvatarPixelFormat.BC4_RUnorm:
						return TextureFormat.BC4;
					case Pico.Avatar.AvatarPixelFormat.BC5_RGSnorm:
					case Pico.Avatar.AvatarPixelFormat.BC5_RGUnorm:
						return TextureFormat.BC5;
					case Pico.Avatar.AvatarPixelFormat.BC6H_RGBSfloat:
					case Pico.Avatar.AvatarPixelFormat.BC6H_RGBUfloat:
						return TextureFormat.BC6H;
					case Pico.Avatar.AvatarPixelFormat.BC7_RGBAUnorm:
					case Pico.Avatar.AvatarPixelFormat.BC7_RGBAUnorm_sRGB:
						return TextureFormat.BC7;

					default:
						break;
				}

				;
				return TextureFormat.RGBA32;
			}

			#endregion


			#region Shared String Buffer

			// A 1MB sized string buffer. it can manage most case.
			public class StringCharBuffer
			{
				public StringCharBuffer(uint size = 1 << 20)
				{
					_SharedStringBuffer = new byte[size];
				}

				//
				~StringCharBuffer()
				{
					if (_SharedStringBufferHandle.IsAllocated)
					{
						_SharedStringBufferHandle.Free();
					}
				}

				// buffer size.
				public uint length
				{
					get => (_SharedStringBuffer == null ? 0 : (uint)_SharedStringBuffer.Length - 1);
				}

				// lock and get pinned address.
				public System.IntPtr Lock()
				{
					if (!_SharedStringBufferHandle.IsAllocated)
					{
						_SharedStringBufferHandle = GCHandle.Alloc(_SharedStringBuffer, GCHandleType.Pinned);
					}

					//
					return _SharedStringBufferHandle.AddrOfPinnedObject();
				}

				public void Unlock()
				{
					if (_SharedStringBufferHandle.IsAllocated)
					{
						_SharedStringBufferHandle.Free();
					}
				}

				public string GetANSIString(uint strLen)
				{
					if (_SharedStringBuffer == null)
					{
						return null;
					}

					if (_SharedStringBufferHandle.IsAllocated)
					{
						_SharedStringBufferHandle.Free();
					}

					//
					return System.Text.Encoding.ASCII.GetString(_SharedStringBuffer, 0,
						(int)Math.Min((int)strLen, _SharedStringBuffer.Length));
				}

				public string GetUTF8String(uint strLen)
				{
					if (_SharedStringBuffer == null)
					{
						return null;
					}

					if (_SharedStringBufferHandle.IsAllocated)
					{
						_SharedStringBufferHandle.Free();
					}

					//
					return System.Text.Encoding.UTF8.GetString(_SharedStringBuffer, 0,
						(int)Math.Min((int)strLen, _SharedStringBuffer.Length));
				}

				public string GetUTF16String(uint strLen)
				{
					if (_SharedStringBuffer == null)
					{
						return null;
					}

					if (_SharedStringBufferHandle.IsAllocated)
					{
						_SharedStringBufferHandle.Free();
					}

					//
					return System.Text.Encoding.Unicode.GetString(_SharedStringBuffer, 0,
						(int)Math.Min((int)strLen, _SharedStringBuffer.Length));
				}

				private byte[] _SharedStringBuffer;
				private GCHandle _SharedStringBufferHandle;
			}

			// shared char buffer.
			private static StringCharBuffer _SharedStringBuffer = new StringCharBuffer((uint)(1 << 20));

			// Gets shared string buffer. Only used in Main thread.
			public static StringCharBuffer sharedStringBuffer
			{
				get { return _SharedStringBuffer; }
			}

			#endregion


			#region Native Utilities

			/**
             * Adds a name to name table and get the returned id.
             * @note It is used to get properties from object such as native material.
             */
			public static uint AddNameToIDNameTable(string name)
			{
				if (!Pico.Avatar.PicoAvatarApp.isNativeEngineReady)
				{
					throw new System.Exception("GetNameHash Need AvatarApp Started.");
				}

				return pav_IDNameTable_AddName(name);
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;


			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_Utility_GetNameHash([MarshalAs(UnmanagedType.LPStr)] string name);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_IDNameTable_AddName([MarshalAs(UnmanagedType.LPStr)] string name);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_IDNameTable_GetNameWithID(uint nameID, ref byte[] chars, uint byteCount);

			#endregion


			#region System Check

			public static void PrintSystemInfos()
			{
				var sb = new System.Text.StringBuilder();
				{
					sb.Append("SystemInfo:");
					sb.Append("batteryLevel:");
					sb.Append(SystemInfo.batteryLevel.ToString());
					sb.Append(" | operatingSystem:");
					sb.Append(SystemInfo.operatingSystem.ToString());
					sb.Append(" | operatingSystemFamily:");
					sb.Append(SystemInfo.operatingSystemFamily.ToString());
					sb.Append(" | processorType:");
					sb.Append(SystemInfo.processorType.ToString());
					sb.Append(" | processorFrequency:");
					sb.Append(SystemInfo.processorFrequency.ToString());
					sb.Append(" | processorCount:");
					sb.Append(SystemInfo.processorCount.ToString());
					sb.Append(" | systemMemorySize:");
					sb.Append(SystemInfo.systemMemorySize.ToString());
					sb.Append(" | deviceUniqueIdentifier:");
					sb.Append(SystemInfo.deviceUniqueIdentifier.ToString());
					sb.Append(" | graphicsMemorySize:");
					sb.Append(SystemInfo.graphicsMemorySize.ToString());
					sb.Append(" | graphicsDeviceName:");
					sb.Append(SystemInfo.graphicsDeviceName.ToString());
					sb.Append(" | graphicsDeviceID:");
					sb.Append(SystemInfo.graphicsDeviceID.ToString());
					sb.Append(" | graphicsUVStartsAtTop:");
					sb.Append(SystemInfo.graphicsUVStartsAtTop.ToString());
					sb.Append(" | graphicsShaderLevel:");
					sb.Append(SystemInfo.graphicsShaderLevel.ToString());
					sb.Append(" | renderingThreadingMode:");
					sb.Append(SystemInfo.renderingThreadingMode.ToString());
					sb.Append(" | supportsShadows:");
					sb.Append(SystemInfo.supportsShadows.ToString());
					sb.Append(" | graphicsMultiThreaded:");
					sb.Append(SystemInfo.graphicsMultiThreaded.ToString());
					sb.Append(" | supportsRawShadowDepthSampling:");
					sb.Append(SystemInfo.supportsRawShadowDepthSampling.ToString());
					sb.Append(" | supportsMotionVectors:");
					sb.Append(SystemInfo.supportsMotionVectors.ToString());
					sb.Append(" | supports3DTextures:");
					sb.Append(SystemInfo.supports3DTextures.ToString());
					sb.Append(" | supportsComputeShaders:");
					sb.Append(SystemInfo.supportsComputeShaders.ToString());
					// sb.Append(" | supportsMultiview:"); sb.Append(SystemInfo.supportsMultiview.ToString());
					sb.Append(" | supportsGeometryShaders:");
					sb.Append(SystemInfo.supportsGeometryShaders.ToString());
					sb.Append(" | supportsTessellationShaders:");
					sb.Append(SystemInfo.supportsTessellationShaders.ToString());
					sb.Append(" | supportsInstancing:");
					sb.Append(SystemInfo.supportsInstancing.ToString());
					sb.Append(" | supportsHardwareQuadTopology:");
					sb.Append(SystemInfo.supportsHardwareQuadTopology.ToString());
					sb.Append(" | usesReversedZBuffer:");
					sb.Append(SystemInfo.usesReversedZBuffer.ToString());
					sb.Append(" | supportsGraphicsFence:");
					sb.Append(SystemInfo.supportsGraphicsFence.ToString());
				}
				UnityEngine.Debug.LogWarning(sb.ToString());
			}

			#endregion


			#region JSONUtil Helper

			public static Dictionary<string, object> JObjectToDictionary(JObject obj)
			{
				Dictionary<string, object> dict = new Dictionary<string, object>();
				foreach (JProperty property in obj.Properties())
				{
					if (property.Value.Type == JTokenType.Object)
					{
						dict[property.Name] = JObjectToDictionary(property.Value.ToObject<JObject>());
					}
					else if (property.Value.Type == JTokenType.Array)
					{
						dict[property.Name] = JArrayToList(property.Value.ToObject<JArray>());
					}
					else
					{
						dict[property.Name] = property.Value.ToObject<object>();
					}
				}

				return dict;
			}

			public static List<object> JArrayToList(JArray array)
			{
				List<object> list = new List<object>();
				foreach (JToken token in array)
				{
					if (token.Type == JTokenType.Object)
					{
						list.Add(JObjectToDictionary(token.ToObject<JObject>()));
					}
					else if (token.Type == JTokenType.Array)
					{
						list.Add(JArrayToList(token.ToObject<JArray>()));
					}
					else
					{
						list.Add(token.ToObject<object>());
					}
				}

				return list;
			}

			public static int GetInt(Dictionary<string, object> json, string key, int defaultValue = 0)
			{
				int result = defaultValue;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						result = System.Convert.ToInt32(json[key]);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static long GetLong(Dictionary<string, object> json, string key, long defaultValue = 0)
			{
				long result = defaultValue;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						long.TryParse(json[key].ToString(), out result);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static float GetFloat(Dictionary<string, object> json, string key, float defaultValue = 0f)
			{
				float result = defaultValue;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						result = System.Convert.ToSingle(json[key]);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static bool GetBool(Dictionary<string, object> json, string key, bool defaultValue = false)
			{
				bool result = defaultValue;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						result = System.Convert.ToBoolean(json[key]);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static string GetString(Dictionary<string, object> json, string key, string defaultValue = null)
			{
				string result = defaultValue;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						result = System.Convert.ToString(json[key]);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static Dictionary<string, object> GetDictionary(Dictionary<string, object> json, string key)
			{
				Dictionary<string, object> result = null;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						result = ((JObject)json[key]).ToObject<Dictionary<string, object>>();
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static Dictionary<TKey, object> GetDictionary<TKey>(Dictionary<string, object> json, string key)
			{
				Dictionary<TKey, object> result = null;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						result = ((JObject)json[key]).ToObject<Dictionary<TKey, object>>();
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static int[] GetIntList(Dictionary<string, object> json, string key)
			{
				var list = GetList(json, key);
				if (list == null || list.Count == 0)
				{
					return null;
				}

				try
				{
					var ret = new int[list.Count];
					for (int i = 0; i < ret.Length; ++i)
					{
						ret[i] = (int)(System.Int64)list[i];
					}

					return ret;
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return null;
			}

			public static List<object> GetList(Dictionary<string, object> json, string key)
			{
				List<object> result = null;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						result = ((JArray)json[key]).ToList<object>();
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static Dictionary<string, object> ToObject(Vector3 val)
			{
				var elements = new Dictionary<string, object>();
				elements["x"] = val.x;
				elements["y"] = val.y;
				elements["z"] = val.z;
				//
				return elements;
			}

			public static Dictionary<string, object> ToObject(Rect val)
			{
				var elements = new Dictionary<string, object>();
				elements["x"] = val.x;
				elements["y"] = val.y;
				elements["width"] = val.width;
				elements["height"] = val.height;
				//
				return elements;
			}

			public static object ToList(Vector3[] val)
			{
				var list = new List<object>();
				if (val != null)
				{
					for (int i = 0; i < val.Length; ++i)
					{
						list.Add(ToObject(val[i]));
					}
				}

				return list;
			}


			public static Vector3 GetVector3(Dictionary<string, object> json, string key)
			{
				Vector3 result = Vector3.zero;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						var elements = (Dictionary<string, object>)json[key];
						result.x = System.Convert.ToSingle(elements["x"]);
						result.y = System.Convert.ToSingle(elements["y"]);
						result.z = System.Convert.ToSingle(elements["z"]);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static Dictionary<string, object> ToObject(Vector2 val)
			{
				var elements = new Dictionary<string, object>();
				elements["x"] = val.x;
				elements["y"] = val.y;
				//
				return elements;
			}

			public static object ToList(Vector2[] val)
			{
				var list = new List<object>();
				for (int i = 0; i < val.Length; ++i)
				{
					list.Add(ToObject(val[i]));
				}

				return list;
			}

			public static Vector2 GetVector2(Dictionary<string, object> json, string key)
			{
				Vector3 result = Vector3.zero;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						var elements = (Dictionary<string, object>)json[key];
						result.x = System.Convert.ToSingle(elements["x"]);
						result.y = System.Convert.ToSingle(elements["y"]);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}

			public static Rect GetRect(Dictionary<string, object> json, string key)
			{
				Rect result = Rect.zero;
				try
				{
					if (json != null && json.ContainsKey(key))
					{
						var elements = (Dictionary<string, object>)json[key];
						result.x = System.Convert.ToSingle(elements["x"]);
						result.y = System.Convert.ToSingle(elements["y"]);
						result.width = System.Convert.ToSingle(elements["width"]);
						result.height = System.Convert.ToSingle(elements["height"]);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message + " stack:" + e.StackTrace);
				}

				return result;
			}
			
			public static object ToList(float[] val)
			{
				var list = new List<object>();
				if (val != null)
				{
					for (int i = 0; i < val.Length; ++i)
					{
						list.Add(val[i]);
					}
				}

				return list;
			}
			
			public static object ToList(int[] val)
			{
				var list = new List<object>();
				if (val != null)
				{
					for (int i = 0; i < val.Length; ++i)
					{
						list.Add((System.Int64)val[i]);
					}
				}

				return list;
			}
			#endregion
			
			

#if UNITY_EDITOR
			#region nation utility
			[MenuItem("AvatarSDK/NationType/CN")]
			public static void SetChina()
			{
				PlayerPrefs.SetInt("NationSelect", (int)NationType.China);
			}

			[MenuItem("AvatarSDK/NationType/OverSea")]
			public static void SetOverSea()
			{
				PlayerPrefs.SetInt("NationSelect", (int)NationType.Oversea);
			}
			[MenuItem("AvatarSDK/NationType/OverSea", true)]
			public static bool CheckCN()
			{
				Menu.SetChecked("AvatarSDK/NationType/CN", GetPCNation() == NationType.China);
				Menu.SetChecked("AvatarSDK/NationType/OverSea", GetPCNation() == NationType.Oversea);
				return true;
			}
			public static NationType GetPCNation()
			{
				return (NationType)PlayerPrefs.GetInt("NationSelect", 1);;
			}
			#endregion
#endif
		}
	}
}