using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Runtime.InteropServices;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Shared mesh resource buffer object. A unity mesh and a static buffer for mesh data created here.
		/// </summary>
		public class AvatarMeshBuffer : ReferencedObject
		{
			#region Public Properties

			/// <summary>
			/// mesh info get from native engine. never to modify it outside the class.
			/// </summary>
			public MeshInfo meshInfo;

			/// <summary>
			/// morph and skin data info. Never to modify the field outside.
			/// </summary>
			public MorphAndSkinDataInfo morphAndSkinDataInfo;

			/// <summary>
			/// morph channel names. never to modify it outside the class
			/// </summary>
			public string[] blendShapeNames;

			/// <summary>
			/// Unity static mesh for an avatar node.
			/// </summary>
			public Mesh mesh
			{
				get => _mesh;
			}

			/// <summary>
			/// bone name hashes.
			/// </summary>
			public int[] boneNameHashes
			{
				get => _boneNameHashes;
			}

			/// <summary>
			/// hash id of root bone name calcualted in native part.
			/// </summary>
			public int rootBoneNameHash { get; private set; }

			/// <summary>
			/// Whether has tangent data.
			/// </summary>
			public bool hasTangent
			{
				get => _hasTangents;
			}

			#endregion


			#region Public Methods

			public AvatarMeshBuffer(long meshCacheKey)
			{
				_meshCacheKey = meshCacheKey;
			}

			#endregion


			#region Protected Methods

			/// <summary>
			/// Derived class can override the method to release resources when the object will be destroyed.
			/// The method should be invoked by override method.
			/// </summary>
			protected override void OnDestroy()
			{
				// remove from global table.
				if (_meshCacheKey != 0)
				{
					_meshBuffers.Remove(_meshCacheKey);
					_meshCacheKey = 0;
				}

				//
				if (_mesh != null)
				{
					UnityEngine.Object.Destroy(_mesh);
				}

				// used in avatar edit mode.
				if (this._meshPositions.Length > 0)
				{
					this._meshPositions.Dispose();
					//
					if (this._meshNormals.Length > 0)
						this._meshNormals.Dispose();
					if (this._meshTangents.Length > 0)
						this._meshTangents.Dispose();
				}

				//
				base.OnDestroy();
			}

			#endregion


			#region Private Fields

			// 
			private long _meshCacheKey = 0;

			// Unity static mesh.
			private Mesh _mesh;

			private int[] _boneNameHashes;

			// Whether has tangent.
			private bool _hasTangents = false;

			// global object table for AvatarMeshBuffers. key is native mesh object.
			private static Dictionary<long, AvatarMeshBuffer> _meshBuffers = new Dictionary<long, AvatarMeshBuffer>();

			// shaping mesh updating cache.
			private NativeArray<Vector3> _meshPositions;
			private NativeArray<Vector3> _meshNormals;
			private NativeArray<Vector4> _meshTangents;

			#endregion


			#region Private/Friend Methods

			/// <summary>
			/// Get or create shared mesh buffer object.
			/// </summary>
			/// <param name="nativeRenderMeshHandle"></param>
			/// <param name="needTangent"></param>
			/// <param name="allowGpuDataCompressed">allowGpuDataCompressd whether allow compress gpu data.</param>
			/// <param name="depressSkin"></param>
			/// /// <param name="useCustomMaterial"></param>
			/// <returns>object has been reference increased.</returns>
			internal static AvatarMeshBuffer CreateAndRefMeshBuffer(System.IntPtr nativeRenderMeshHandle,
				bool needTangent,
				bool allowGpuDataCompressed, bool depressSkin, bool useCustomMaterial)
			{
				if (nativeRenderMeshHandle == System.IntPtr.Zero)
				{
					return null;
				}

				// get mesh information.
				MeshInfo meshInfo = new MeshInfo();
				meshInfo.version = 0;
				if (pav_AvatarRenderMesh_GetMeshInfo(nativeRenderMeshHandle, ref meshInfo) !=
				    NativeResult.Success
				    || meshInfo.positionCount == 0)
				{
					return null;
				}

				long meshCacheKey = (meshInfo.meshObject.ToInt64() << 16) + meshInfo.instanceKey;
				// find exist one. using data address is not rigorous, the key is error, we may get the history data. Todo: wenhao.
				if (_meshBuffers.TryGetValue(meshCacheKey, out var avatarMeshBuffer))
				{
					// for init native buffer descs
					avatarMeshBuffer.GetMorphAndSkinGpuDataInfo(nativeRenderMeshHandle, allowGpuDataCompressed);
					//
					avatarMeshBuffer.Retain();
					//
					return avatarMeshBuffer;
				}

				// if not exist, try create new one.
				avatarMeshBuffer = new AvatarMeshBuffer(meshCacheKey);
				//
				if (!avatarMeshBuffer.Create(nativeRenderMeshHandle, meshInfo, needTangent, allowGpuDataCompressed,
					    depressSkin, useCustomMaterial))
				{
					avatarMeshBuffer.CheckDelete();
					return null;
				}

				// retain mesh buffer.
				avatarMeshBuffer.Retain();

				//add to global lookup table.
				_meshBuffers.Add(meshCacheKey, avatarMeshBuffer);
				//
				return avatarMeshBuffer;
			}

			/// <summary>
			/// Create mesh buffer with native render mesh object.
			/// </summary>
			/// <param name="nativeRenderMeshHandle"></param>
			/// <param name="meshInfo_"></param>
			/// <param name="needTangent"></param>
			/// <param name="allowGpuDataCompressed">allowGpuDataCompressd whether allow compress gpu data.</param>
			/// <param name="depressSkin"></param>
			/// <returns></returns>
			private bool Create(System.IntPtr nativeRenderMeshHandle, MeshInfo meshInfo_, bool needTangent,
				bool allowGpuDataCompressed, bool depressSkin, bool useCustomMaterial)
			{
				// keep a meshInfo data.
				meshInfo = meshInfo_;

				// create unity mesh.
				CreateMesh(nativeRenderMeshHandle, needTangent, depressSkin, useCustomMaterial);

				// initialize morph and skin buffer.
				if (_mesh != null)
				{
					InitializeGpuData(nativeRenderMeshHandle, needTangent, allowGpuDataCompressed);
				}

				return true;
			}

			// shared handles used in CreateMesh. All used in main thread blockly.
			static private NativeArray<int>[] s_tmpIndicesNativeArrays =
				new NativeArray<int>[(int)Consts.kMaxSubmeshCount];

			static private ulong[] s_tmpIndicePointers = new ulong[(int)Consts.kMaxSubmeshCount];

			/// <summary>
			/// Create unity mesh object from native render mesh object.
			/// </summary>
			/// <param name="nativeRenderMeshHandle"></param>
			/// <param name="needTangent"></param>
			/// <param name="depressSkin"></param>
			/// /// <param name="useCustomMaterial"></param>
			protected void CreateMesh(System.IntPtr nativeRenderMeshHandle, bool needTangent, bool depressSkin, bool useCustomMaterial)
			{
				if (_mesh != null)
				{
					Object.DestroyImmediate(_mesh);
					_mesh = null;
				}

				// clear tangent flag first. check whether has tangent in following.
				_hasTangents = false;

				// whether has skin
				var needSkin = meshInfo.boneNameCount > 0 && meshInfo.boneWeightCount == meshInfo.positionCount
				                                          && meshInfo.bindPoseBoneCount == meshInfo.boneNameCount;

				var need4_8BoneSkin =
					meshInfo.boneNameCount > 0 && meshInfo.boneWeight4_8Count == meshInfo.positionCount
					                           && meshInfo.bindPoseBoneCount == meshInfo.boneNameCount;

				//
				var positions = new NativeArray<Vector3>((int)meshInfo.positionCount, Allocator.Temp,
					NativeArrayOptions.UninitializedMemory);
				var normals = new NativeArray<Vector3>((int)meshInfo.normalCount, Allocator.Temp,
					NativeArrayOptions.UninitializedMemory);
				var tangents = new NativeArray<Vector4>((int)meshInfo.tangentCount, Allocator.Temp,
					NativeArrayOptions.UninitializedMemory);
				var colors = new NativeArray<Color32>((int)meshInfo.colorCount, Allocator.Temp,
					NativeArrayOptions.UninitializedMemory);
				var uv1 = new NativeArray<Vector2>((int)meshInfo.uvCount, Allocator.Temp,
					NativeArrayOptions.UninitializedMemory);
				var uv2 = new NativeArray<Vector2>((int)meshInfo.uv2Count, Allocator.Temp,
					NativeArrayOptions.UninitializedMemory);
				var uv3 = new NativeArray<Vector2>((int)meshInfo.uv3Count, Allocator.Temp,
					NativeArrayOptions.UninitializedMemory);
				var uv4 = new NativeArray<Vector2>((int)meshInfo.uv4Count, Allocator.Temp,
					NativeArrayOptions.UninitializedMemory);

				NativeArray<Byte> bonesPerVertexList = needSkin
					? new NativeArray<Byte>((int)meshInfo.boneWeightCount, Allocator.Temp,
						NativeArrayOptions.UninitializedMemory)
					: new NativeArray<Byte>();

				NativeArray<BoneWeight1> bonesPerVertexWeight = needSkin
					? new NativeArray<BoneWeight1>((int)meshInfo.bonesPerVertexWeightCount, Allocator.Temp,
						NativeArrayOptions.UninitializedMemory)
					: new NativeArray<BoneWeight1>();

				NativeArray<BoneWeight> boneWeights = needSkin
					? new NativeArray<BoneWeight>((int)meshInfo.boneWeightCount, Allocator.Temp,
						NativeArrayOptions.UninitializedMemory)
					: new NativeArray<BoneWeight>();
				NativeArray<BoneWeight> boneWeight4_8s = need4_8BoneSkin
					? new NativeArray<BoneWeight>((int)meshInfo.boneWeightCount, Allocator.Temp,
						NativeArrayOptions.UninitializedMemory)
					: new NativeArray<BoneWeight>();
				NativeArray<Matrix4x4> bindPoses = (needSkin || need4_8BoneSkin)
					? new NativeArray<Matrix4x4>((int)meshInfo.bindPoseBoneCount, Allocator.Temp,
						NativeArrayOptions.UninitializedMemory)
					: new NativeArray<Matrix4x4>();
				_boneNameHashes = new int[meshInfo.boneNameCount];
				GCHandle boneNameHashesHandle = GCHandle.Alloc(_boneNameHashes, GCHandleType.Pinned);

				int subMeshCount = Mathf.Min((int)meshInfo.subMeshCount, (int)Consts.kMaxSubmeshCount);
				//
				var nativeRenderMeshAbstract = new MeshData();
				nativeRenderMeshAbstract.indices = s_tmpIndicePointers;

				// get info from material, current mesh use custom or official?
                nativeRenderMeshAbstract.useCustomMaterial = useCustomMaterial ? (byte)1 : (byte)0;

				unsafe
				{
					nativeRenderMeshAbstract.positions = (System.IntPtr)positions.GetUnsafePtr();
					nativeRenderMeshAbstract.normals = meshInfo.normalCount == meshInfo.positionCount
						? (System.IntPtr)normals.GetUnsafePtr()
						: System.IntPtr.Zero;
					nativeRenderMeshAbstract.tangents = meshInfo.tangentCount == meshInfo.positionCount
						? (System.IntPtr)tangents.GetUnsafePtr()
						: System.IntPtr.Zero;
					nativeRenderMeshAbstract.colors = meshInfo.colorCount == meshInfo.positionCount
						? (System.IntPtr)colors.GetUnsafePtr()
						: System.IntPtr.Zero;
					nativeRenderMeshAbstract.uv1 = meshInfo.uvCount == meshInfo.positionCount
						? (System.IntPtr)uv1.GetUnsafePtr()
						: System.IntPtr.Zero;
					nativeRenderMeshAbstract.uv2 = meshInfo.uv2Count == meshInfo.positionCount
						? (System.IntPtr)uv2.GetUnsafePtr()
						: System.IntPtr.Zero;
					nativeRenderMeshAbstract.uv3 = meshInfo.uv3Count == meshInfo.positionCount
						? (System.IntPtr)uv3.GetUnsafePtr()
						: System.IntPtr.Zero;
					nativeRenderMeshAbstract.uv4 = meshInfo.uv4Count == meshInfo.positionCount
						? (System.IntPtr)uv4.GetUnsafePtr()
						: System.IntPtr.Zero;
					//
					nativeRenderMeshAbstract.bonesPerVertexList =
						needSkin ? (System.IntPtr)bonesPerVertexList.GetUnsafePtr() : System.IntPtr.Zero;

					nativeRenderMeshAbstract.bonesPerVertexWeight = needSkin
						? (System.IntPtr)bonesPerVertexWeight.GetUnsafePtr()
						: System.IntPtr.Zero;

					nativeRenderMeshAbstract.boneWeights =
						needSkin ? (System.IntPtr)boneWeights.GetUnsafePtr() : System.IntPtr.Zero;
					nativeRenderMeshAbstract.boneWeight4_8s =
						need4_8BoneSkin ? (System.IntPtr)boneWeight4_8s.GetUnsafePtr() : System.IntPtr.Zero;
					nativeRenderMeshAbstract.bindPoses =
						(needSkin || need4_8BoneSkin)
							? (System.IntPtr)bindPoses.GetUnsafePtr()
							: System.IntPtr.Zero;
					nativeRenderMeshAbstract.boneNameHashes =
						(needSkin || need4_8BoneSkin)
							? boneNameHashesHandle.AddrOfPinnedObject()
							: System.IntPtr.Zero;
					nativeRenderMeshAbstract.boneNames = System.IntPtr.Zero;

					// no need for weights.
					for (int i = 0; i < subMeshCount; ++i)
					{
						if (meshInfo.indicesCount[i] == 0)
						{
							subMeshCount = i;
							break;
						}

						s_tmpIndicesNativeArrays[i] = new NativeArray<int>((int)meshInfo.indicesCount[i],
							Allocator.Temp, NativeArrayOptions.UninitializedMemory);
						// triangles.
						nativeRenderMeshAbstract.indices[i] = (ulong)s_tmpIndicesNativeArrays[i].GetUnsafePtr();
					}
				}

				// blend shapes
				var deltaVertices = new NativeArray<Vector3>();
				var deltaNormals = new NativeArray<Vector3>();
				var deltaTangents = new NativeArray<Vector3>();
				unsafe
				{
					nativeRenderMeshAbstract.blendShapeDeltaVertices = System.IntPtr.Zero;
					nativeRenderMeshAbstract.blendShapeDeltaNormals = System.IntPtr.Zero;
					nativeRenderMeshAbstract.blendShapeDeltaTangents = System.IntPtr.Zero;
					if ((needSkin || need4_8BoneSkin) && meshInfo.blendShapeCount > 0)
					{
						int deltaBufferCount = (int)meshInfo.positionCount * meshInfo.blendShapeCount;
						if (meshInfo.blendShapeHasVertex != 0)
						{
							deltaVertices = new NativeArray<Vector3>(deltaBufferCount, Allocator.Temp,
								NativeArrayOptions.ClearMemory);
							nativeRenderMeshAbstract.blendShapeDeltaVertices =
								(System.IntPtr)deltaVertices.GetUnsafePtr();
						}

						if (meshInfo.blendShapeHasNormal != 0)
						{
							deltaNormals = new NativeArray<Vector3>(deltaBufferCount, Allocator.Temp,
								NativeArrayOptions.ClearMemory);
							nativeRenderMeshAbstract.blendShapeDeltaNormals =
								(System.IntPtr)deltaNormals.GetUnsafePtr();
						}

						if (meshInfo.blendShapeHasTangent != 0)
						{
							deltaTangents = new NativeArray<Vector3>(deltaBufferCount, Allocator.Temp,
								NativeArrayOptions.ClearMemory);
							nativeRenderMeshAbstract.blendShapeDeltaTangents =
								(System.IntPtr)deltaTangents.GetUnsafePtr();
						}
					}
				}

				//
				var result = pav_AvatarRenderMesh_GetMeshData(nativeRenderMeshHandle, ref nativeRenderMeshAbstract);

				boneNameHashesHandle.Free();

				// get root bone name hash. some asset may has different root bone. bindpose depend on root bone.
				rootBoneNameHash = (int)meshInfo.rootBoneNameHash;

				// create unity mesh and fill data.
				if (result == NativeResult.Success && Utility.EnableRenderObject)
				{
					_mesh = new Mesh();
					_mesh.SetVertices(positions);
					//
					if (meshInfo.normalCount == meshInfo.positionCount)
					{
						_mesh.SetNormals(normals);
					}

					//
					if (needTangent && meshInfo.tangentCount == meshInfo.positionCount)
					{
						_hasTangents = true;
						//
						_mesh.SetTangents(tangents);
					}

					//
					if (meshInfo.colorCount == meshInfo.positionCount)
					{
						_mesh.SetColors(colors);
					}

					//
					if (meshInfo.uvCount == meshInfo.positionCount)
					{
						_mesh.SetUVs(0, uv1);
					}

					//
					if (meshInfo.uv2Count == meshInfo.positionCount)
					{
						_mesh.SetUVs(1, uv2);
					}

					//
					if (meshInfo.uv3Count == meshInfo.positionCount)
					{
						_mesh.SetUVs(2, uv3);
					}

					//
					if (meshInfo.uv4Count == meshInfo.positionCount)
					{
						_mesh.SetUVs(3, uv4);
					}

					//
					if (needSkin && need4_8BoneSkin == false && !depressSkin)
					{
						_mesh.boneWeights = boneWeights.ToArray();
					}

					if (need4_8BoneSkin && !depressSkin)
					{
						_mesh.SetBoneWeights(bonesPerVertexList, bonesPerVertexWeight);
					}

					if ((needSkin || need4_8BoneSkin) && !depressSkin)
					{
						_mesh.bindposes = bindPoses.ToArray();
					}
					else
					{
					}

					if (meshInfo.blendShapeCount > 0)
					{
						NativeArray<System.IntPtr> bsNames = new NativeArray<System.IntPtr>();
						bsNames = new NativeArray<System.IntPtr>((int)meshInfo.blendShapeCount, Allocator.Temp,
							NativeArrayOptions.ClearMemory);

						// get mesh extra information
						MeshExtraInfo meshExtraInfo = new MeshExtraInfo();
						meshExtraInfo.version = 0;
						unsafe
						{
							meshExtraInfo.blendShapeNames = (System.IntPtr)bsNames.GetUnsafePtr();
						}

						pav_AvatarRenderMesh_GetMeshExtraInfo(nativeRenderMeshHandle, ref meshExtraInfo);

						blendShapeNames = new string[meshInfo.blendShapeCount];
						unsafe
						{
							for (int i = 0; i < meshInfo.blendShapeCount; ++i)
							{
								int strlen = 0;
								byte* cstr = (byte*)bsNames[i];
								if (cstr != null)
								{
									while (cstr[strlen] != 0)
									{
										strlen += 1;
									}

									if (strlen > 0)
									{
										blendShapeNames[i] = System.Text.Encoding.UTF8.GetString(cstr, strlen);
									}
								}

								// fallback to bs index
								if (Utility.IsNullOrEmpty(blendShapeNames[i]))
								{
									blendShapeNames[i] = i.ToString();
								}
							}
						}

						if (bsNames.IsCreated)
						{
							bsNames.Dispose();
						}

						for (int i = 0; i < meshInfo.blendShapeCount; ++i)
						{
							Vector3[] verticesSlice = null;
							Vector3[] normalsSlice = null;
							Vector3[] tangentsSlice = null;
							if (meshInfo.blendShapeHasVertex != 0)
							{
								verticesSlice = deltaVertices.GetSubArray((int)meshInfo.positionCount * i,
									(int)meshInfo.positionCount).ToArray();
							}

							if (meshInfo.blendShapeHasNormal != 0)
							{
								normalsSlice = deltaNormals.GetSubArray((int)meshInfo.positionCount * i,
									(int)meshInfo.positionCount).ToArray();
							}

							if (meshInfo.blendShapeHasTangent != 0)
							{
								tangentsSlice = deltaTangents.GetSubArray((int)meshInfo.positionCount * i,
									(int)meshInfo.positionCount).ToArray();
							}

							// exception case process: bs has no delta vertices
							if (verticesSlice == null && normalsSlice == null && tangentsSlice == null)
							{
								verticesSlice = new Vector3[meshInfo.positionCount];
							}

							_mesh.AddBlendShapeFrame(blendShapeNames[i], 100, verticesSlice, normalsSlice,
								tangentsSlice);
						}
					}

					//
					_mesh.subMeshCount = subMeshCount;
					for (int i = 0; i < subMeshCount; ++i)
					{
						if (s_tmpIndicesNativeArrays[i].IsCreated)
						{
							_mesh.SetIndices(s_tmpIndicesNativeArrays[i], MeshTopology.Triangles, i, true);
							s_tmpIndicesNativeArrays[i].Dispose();
						}
					}
					//_mesh.bounds = new Bounds(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.6f, 2.0f, 1.2f) * 1.0f);
					//_mesh.RecalculateBounds();

					//TODO: get from c++;
					_mesh.bounds = new Bounds(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.5f, 2.0f, 1.5f));
				}

				// clear native array.
				{
					positions.Dispose();
					normals.Dispose();
					tangents.Dispose();
					colors.Dispose();
					uv1.Dispose();
					uv2.Dispose();
					uv3.Dispose();
					uv4.Dispose();
					//
					if (boneWeights.IsCreated)
					{
						boneWeights.Dispose();
					}

					if (boneWeight4_8s.IsCreated)
					{
						boneWeight4_8s.Dispose();
					}

					if (bonesPerVertexList.IsCreated)
					{
						bonesPerVertexList.Dispose();
					}

					if (bonesPerVertexWeight.IsCreated)
					{
						bonesPerVertexWeight.Dispose();
					}

					if (bindPoses.IsCreated)
					{
						bindPoses.Dispose();
					}

					if (deltaVertices.IsCreated) deltaVertices.Dispose();
					if (deltaNormals.IsCreated) deltaNormals.Dispose();
					if (deltaTangents.IsCreated) deltaTangents.Dispose();
				}
			}

			/// <summary>
			/// GetMorphAndSkinGpuDataInfo
			/// </summary>
			/// <param name="renderMeshHandle"></param>
			/// <param name="allowGpuDataCompressed">allowGpuDataCompressd whether allow compress gpu data.</param>
			/// <returns></returns>
			private NativeResult GetMorphAndSkinGpuDataInfo(System.IntPtr renderMeshHandle, bool allowGpuDataCompressed)
			{
				// WARNING: currently only Float format wholely supported, modify Internal_UpdateMeshPNTData if compress format changed.
				MorphAndSkinDataRequiredInfo requiredInfo = new MorphAndSkinDataRequiredInfo();

				requiredInfo.version = 0;
				requiredInfo.flags = (uint)RenderResourceDataFlags.Dynamic_HasMorphWeights;
				requiredInfo.meshVertexDataFormat = (uint)DataFormat.Float;
				requiredInfo.morphVertexDataFormat = (uint)DataFormat.Float;
				//
				if (allowGpuDataCompressed)
				{
					requiredInfo.meshVertexDataFormat =
						(uint)PicoAvatarApp.instance.renderSettings.meshVertexDataFormat;
					requiredInfo.morphVertexDataFormat =
						(uint)PicoAvatarApp.instance.renderSettings.morphVertexDataFormat;
				}

				morphAndSkinDataInfo.version = 0;
				return pav_AvatarRenderMesh_GetMorphAndSkinGpuDataInfo(renderMeshHandle, ref requiredInfo,
					ref morphAndSkinDataInfo);
			}

			/// <summary>
			/// Initialize gpu data.
			/// </summary>
			/// <param name="renderMeshHandle"></param>
			/// <param name="needTangent"></param>
			/// <param name="allowGpuDataCompressed">allowGpuDataCompressed</param>
			protected void InitializeGpuData(System.IntPtr renderMeshHandle, bool needTangent,
				bool allowGpuDataCompressed)
			{
				var result = GetMorphAndSkinGpuDataInfo(renderMeshHandle, allowGpuDataCompressed);
				if (result != NativeResult.Success
				    || morphAndSkinDataInfo.staticBufferByteSize == 0
				    || morphAndSkinDataInfo.dynamicBufferByteSize == 0
				    || morphAndSkinDataInfo.outputBufferByteSize == 0)

				{
					return;
				}

#if DEBUG
				if (AvatarEnv.NeedLog(DebugLogMask.GeneralInfo))
				{
					AvatarEnv.Log(DebugLogMask.GeneralInfo, string.Format(
						"static buffer size: {0} dynamic buffer size: {1} output buffer size: {2}",
						morphAndSkinDataInfo.staticBufferByteSize,
						morphAndSkinDataInfo.dynamicBufferByteSize,
						morphAndSkinDataInfo.outputBufferByteSize));
				}
#endif
			}

			/// <summary>
			/// Gets skin data as texture2d, used in shader to get only skin info.
			/// </summary>
			/// <remarks>
			/// Since the texture ONLY used by gpu instance rendering, 4 bone weighs are enough.
			/// </remarks>
			/// <param name="renderMeshHandle"></param>
			/// <returns></returns>
			internal Texture2D CreateSkinDataAsTexture2D(System.IntPtr renderMeshHandle)
			{
				if (meshInfo.positionCount == 0 || meshInfo.boneNameCount == 0)
				{
					return null;
				}

				// each vertex occupy 2 pixel in g_pavBoneWeightsTex.
				int texWidth = (int)meshInfo.positionCount * 2;
				if (texWidth <= 512)
				{
					texWidth = 512;
				}
				else // its enough for most cases. vertice count can be up to millions.
				{
					texWidth = 1024;
				}

				// each vertex occupy 2 pixel in g_pavBoneWeightsTex.
				int texHeight =
					((int)meshInfo.positionCount * 2 + texWidth - 1) /
					texWidth; // 4 weights * (1 index + 1 weight)   element type is half.
				//
				Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false);

				unsafe
				{
					MorphAndSkinResourceGpuData gpuData = new MorphAndSkinResourceGpuData();
					gpuData.version = 0;
					gpuData.staticBufferByteSize = (uint)texWidth * 4 * 2; // 4 channels * sizeof(half)
					gpuData.flags = (uint)MorphAndSkinResourceGpuDataFlags.SkinAsTextureRGBAHalf;
					gpuData.dataBuffer = (System.IntPtr)tex.GetRawTextureData<ushort>().GetUnsafePtr();
					//
					if (pav_AvatarRenderMesh_FillMorphAndSkinResourceGpuData(renderMeshHandle, ref gpuData) !=
					    NativeResult.Success)
					{
						Object.DestroyImmediate(tex);
						return null;
					}

					tex.Apply();
				}

				return tex;
			}

			#endregion


			#region Update PNT Data

			/// <summary>
			/// If in avatar edit mode, when 
			/// </summary>
			/// <param name="nativeRenderMeshHandle"></param>
			/// <param name="materialConfiguration"></param>
			/// <param name="needTangents"></param>
			internal void UpdateMeshPNTData(System.IntPtr nativeRenderMeshHandle
				, PicoMaterialConfiguration materialConfiguration, bool needTangents)
			{
				// turn of tangents.
				needTangents = false;
				//
				if (meshInfo.positionCount == 0 || meshInfo.normalCount != meshInfo.positionCount ||
				    materialConfiguration.transferShapingMeshShader == null)
				{
					return;
				}

				// allocate Unity native buffer for performance purpose.
				if (this._meshPositions.Length == 0 && meshInfo.positionCount > 0)
				{
					this._meshPositions = new NativeArray<Vector3>((int)meshInfo.positionCount, Allocator.Persistent,
						NativeArrayOptions.UninitializedMemory);
				}

				if (this._meshNormals.Length == 0 && meshInfo.normalCount > 0)
				{
					this._meshNormals = new NativeArray<Vector3>((int)meshInfo.normalCount, Allocator.Persistent,
						NativeArrayOptions.UninitializedMemory);
				}

				if (needTangents && this._meshTangents.Length == 0 && meshInfo.tangentCount > 0)
				{
					this._meshTangents = new NativeArray<Vector4>((int)meshInfo.tangentCount, Allocator.Persistent,
						NativeArrayOptions.UninitializedMemory);
				}

				var nativeRenderMeshAbstract = new MeshData();
				unsafe
				{
					nativeRenderMeshAbstract.positions = (System.IntPtr)this._meshPositions.GetUnsafePtr();
					nativeRenderMeshAbstract.normals = meshInfo.normalCount == meshInfo.positionCount
						? (System.IntPtr)this._meshNormals.GetUnsafePtr()
						: System.IntPtr.Zero;
					nativeRenderMeshAbstract.tangents =
						(needTangents && meshInfo.tangentCount == meshInfo.positionCount)
							? (System.IntPtr)this._meshTangents.GetUnsafePtr()
							: System.IntPtr.Zero;
				}

				// get mesh position / norma. tangents from native object.
				var result = pav_AvatarRenderMesh_GetMeshPNTData(nativeRenderMeshHandle, ref nativeRenderMeshAbstract);

				if (result == NativeResult.Success)
				{
					if (this._meshPositions.IsCreated)
					{
						_mesh.SetVertices(this._meshPositions);
					}

					if (this._meshNormals.IsCreated)
					{
						_mesh.SetNormals(this._meshNormals);
					}

					if (this._meshTangents.IsCreated)
					{
						_mesh.SetTangents(this._meshTangents);
					}
				}
			}

			/// <summary>
			/// Update gpu skin and morph buffer.
			/// </summary>
			/// <param name="renderMeshHandle"></param>
			/// <returns>true if UnityEngine.Mesh recreated.</returns>
			internal bool UpdateMorphAndSkinResourceGpuData(System.IntPtr renderMeshHandle)
			{
				if (_mesh != null && meshInfo.blendShapeCount > 0 && blendShapeNames != null &&
				    blendShapeNames.Length > 0)
				{
					_mesh.ClearBlendShapes();
					//
					var nativeRenderMeshAbstract = new MeshData();

					// blend shapes
					var deltaVertices = new NativeArray<Vector3>();
					var deltaNormals = new NativeArray<Vector3>();
					var deltaTangents = new NativeArray<Vector3>();
					unsafe
					{
						nativeRenderMeshAbstract.blendShapeDeltaVertices = System.IntPtr.Zero;
						nativeRenderMeshAbstract.blendShapeDeltaNormals = System.IntPtr.Zero;
						nativeRenderMeshAbstract.blendShapeDeltaTangents = System.IntPtr.Zero;
						{
							int deltaBufferCount = (int)meshInfo.positionCount * meshInfo.blendShapeCount;
							if (meshInfo.blendShapeHasVertex != 0)
							{
								deltaVertices = new NativeArray<Vector3>(deltaBufferCount, Allocator.Temp,
									NativeArrayOptions.ClearMemory);
								nativeRenderMeshAbstract.blendShapeDeltaVertices =
									(System.IntPtr)deltaVertices.GetUnsafePtr();
							}

							if (meshInfo.blendShapeHasNormal != 0)
							{
								deltaNormals = new NativeArray<Vector3>(deltaBufferCount, Allocator.Temp,
									NativeArrayOptions.ClearMemory);
								nativeRenderMeshAbstract.blendShapeDeltaNormals =
									(System.IntPtr)deltaNormals.GetUnsafePtr();
							}

							if (meshInfo.blendShapeHasTangent != 0)
							{
								deltaTangents = new NativeArray<Vector3>(deltaBufferCount, Allocator.Temp,
									NativeArrayOptions.ClearMemory);
								nativeRenderMeshAbstract.blendShapeDeltaTangents =
									(System.IntPtr)deltaTangents.GetUnsafePtr();
							}
						}
					}

					//
					var result = pav_AvatarRenderMesh_GetMeshData(renderMeshHandle, ref nativeRenderMeshAbstract);

					// create unity mesh and fill data.
					if (result == NativeResult.Success)
					{
						for (int i = 0; i < meshInfo.blendShapeCount; ++i)
						{
							Vector3[] verticesSlice = null;
							Vector3[] normalsSlice = null;
							Vector3[] tangentsSlice = null;
							if (meshInfo.blendShapeHasVertex != 0)
							{
								verticesSlice = deltaVertices.GetSubArray((int)meshInfo.positionCount * i,
									(int)meshInfo.positionCount).ToArray();
							}

							if (meshInfo.blendShapeHasNormal != 0)
							{
								normalsSlice = deltaNormals.GetSubArray((int)meshInfo.positionCount * i,
									(int)meshInfo.positionCount).ToArray();
							}

							if (meshInfo.blendShapeHasTangent != 0)
							{
								tangentsSlice = deltaTangents.GetSubArray((int)meshInfo.positionCount * i,
									(int)meshInfo.positionCount).ToArray();
							}

							// exception case process: bs has no delta vertices
							if (verticesSlice == null && normalsSlice == null && tangentsSlice == null)
							{
								verticesSlice = new Vector3[meshInfo.positionCount];
							}

							_mesh.AddBlendShapeFrame(blendShapeNames[i], 100, verticesSlice, normalsSlice,
								tangentsSlice);
						}
					}

					// clear native array.
					{
						if (deltaVertices.IsCreated) deltaVertices.Dispose();
						if (deltaNormals.IsCreated) deltaNormals.Dispose();
						if (deltaTangents.IsCreated) deltaTangents.Dispose();
					}
				}

				return true;
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_GetMeshInfo(System.IntPtr nativeHandle,
				ref MeshInfo meshInfo);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_GetMeshExtraInfo(System.IntPtr nativeHandle,
				ref MeshExtraInfo meshExtraInfo);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_GetMeshData(System.IntPtr nativeHandle,
				ref MeshData meshData);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_GetMorphAndSkinGpuDataInfo(
				System.IntPtr nativeHandle, ref MorphAndSkinDataRequiredInfo requiredInfo,
				ref MorphAndSkinDataInfo gpuDataInfo);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_FillMorphAndSkinResourceGpuData(
				System.IntPtr nativeHandle, ref MorphAndSkinResourceGpuData gpuData);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_GetMeshPNTData(System.IntPtr nativeHandle,
				ref MeshData meshData);

			#endregion
		}

		public class AvatarMergedMeshBuffer : ReferencedObject
		{
			private Mesh _mesh;
			public Mesh mesh { get => _mesh; }

			private int[] _boneNameHashes;
			public int[] boneNameHashes { get => _boneNameHashes; }

			private uint _rootBoneNameHash;
			internal uint rootBoneNameHash { get => _rootBoneNameHash; }

			private System.IntPtr _nativeHandle;
			private int _hashCode;

            protected override void OnDestroy()
            {
                if (_mesh != null)
                {
                    UnityEngine.Object.Destroy(_mesh);
                }
                _meshBuffers.Remove(_hashCode); 
				_hashCode = 0;
                base.OnDestroy();
            }

            private static Dictionary<int, AvatarMergedMeshBuffer> _meshBuffers = new Dictionary<int, AvatarMergedMeshBuffer>();
			internal static AvatarMergedMeshBuffer TryGetMergedMeshBuffer(int hashCode)
			{
				AvatarMergedMeshBuffer buffer;
				if (_meshBuffers.TryGetValue(hashCode, out buffer))
					return buffer;
				else
					return null;
			}
			internal static AvatarMergedMeshBuffer Create(int hashCode, System.IntPtr nativeHandle, ref MergedMeshInfo meshInfo, ref MergedMeshData meshData)
			{
				var buffer = new AvatarMergedMeshBuffer();
				buffer.CreateMesh(ref meshInfo, ref meshData);
				buffer._boneNameHashes = meshData.boneNameHashes.ToArray();
				buffer._rootBoneNameHash = meshInfo.rootBoneNameHash;
				buffer._nativeHandle = nativeHandle;
				buffer._hashCode = hashCode;
				buffer.Retain();
				_meshBuffers.Add(hashCode, buffer);
				return buffer;
			}
			private void CreateMesh(ref MergedMeshInfo meshInfo, ref MergedMeshData meshData)
			{
				_mesh = new Mesh();
                _mesh.SetVertices(meshData.positions);
				_mesh.SetNormals(meshData.normals);
				if (meshInfo.tangentCount > 0)
					_mesh.SetTangents(meshData.tangents);
				if (meshInfo.colorCount > 0)
					_mesh.SetColors(meshData.colors);
				if (meshInfo.uv1Count > 0)
					_mesh.SetUVs(0, meshData.uv1);
				if (meshInfo.uv2Count > 0)
					_mesh.SetUVs(1, meshData.uv2);
				if (meshInfo.uv3Count > 0)
					_mesh.SetUVs(2, meshData.uv3);
				if (meshInfo.uv4Count > 0)
					_mesh.SetUVs(3, meshData.uv4);
				// Using uv channel 4 as material index
				_mesh.SetUVs(4, meshData.materialIndices);
				if (meshInfo.weight8 > 0)
				{
					// 8 weights
					byte[] boneWeightsPerVertex = new byte[meshInfo.positionCount];
					for (int idx = 0; idx < boneWeightsPerVertex.Length; ++idx)
						boneWeightsPerVertex[idx] = 8;
					BoneWeight1[] boneWeights = new BoneWeight1[meshInfo.positionCount * 8];
					for (int idx = 0; idx < meshInfo.positionCount; ++idx)
					{
						var weight0 = meshData.boneWeights[idx * 2];
						boneWeights[idx * 8].boneIndex = weight0.boneIndex0;
						boneWeights[idx * 8].weight = weight0.weight0;
						boneWeights[idx * 8 + 1].boneIndex = weight0.boneIndex1;
						boneWeights[idx * 8 + 1].weight = weight0.weight1;
						boneWeights[idx * 8 + 2].boneIndex = weight0.boneIndex2;
						boneWeights[idx * 8 + 2].weight = weight0.weight2;
						boneWeights[idx * 8 + 3].boneIndex = weight0.boneIndex3;
						boneWeights[idx * 8 + 3].weight = weight0.weight3;

						var weight1 = meshData.boneWeights[idx * 2 + 1];
						boneWeights[idx * 8 + 4].boneIndex = weight1.boneIndex0;
						boneWeights[idx * 8 + 4].weight = weight1.weight0;
						boneWeights[idx * 8 + 5].boneIndex = weight1.boneIndex1;
						boneWeights[idx * 8 + 5].weight = weight1.weight1;
						boneWeights[idx * 8 + 6].boneIndex = weight1.boneIndex2;
						boneWeights[idx * 8 + 6].weight = weight1.weight2;
						boneWeights[idx * 8 + 7].boneIndex = weight1.boneIndex3;
						boneWeights[idx * 8 + 7].weight = weight1.weight3;
					}

					var boneWeightsPerVertexArray = new NativeArray<byte>(boneWeightsPerVertex, Allocator.Temp);
					var boneWeightsArray = new NativeArray<BoneWeight1>(boneWeights, Allocator.Temp);
					_mesh.SetBoneWeights(boneWeightsPerVertexArray, boneWeightsArray);
					boneWeightsPerVertexArray.Dispose();
					boneWeightsArray.Dispose();
				}
				else
				{
					// Regular 4 weights
                    _mesh.boneWeights = meshData.boneWeights.ToArray();
				}
				_mesh.bindposes = meshData.invBindPoses.ToArray();
				if (meshInfo.positionCount > 65535)
					_mesh.indexFormat = IndexFormat.UInt32;
				else
					_mesh.indexFormat = IndexFormat.UInt16;
				_mesh.SetIndices<uint>(meshData.indices, MeshTopology.Triangles, 0, true);
				//_mesh.bounds = new Bounds(meshInfo.boundCenter, meshInfo.boundSize);
				_mesh.bounds = new Bounds(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.5f, 2.0f, 1.5f));
			}
		}
	}
}