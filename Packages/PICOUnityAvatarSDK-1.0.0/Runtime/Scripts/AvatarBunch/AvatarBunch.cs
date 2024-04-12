using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Jobs;


namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// AvatarBunch used to render many avatars that using same avatar model by gpu instance.
		/// </summary>
		public class AvatarBunch : MonoBehaviour
		{
			#region Public Types

			// The value MUST be equal to _PAV_MaxInstanceCount defined in PavConfig.hlsl
			public const int MaxInstanceCount = 200;

			/// <summary>
			/// material provider for the avatar bunch.
			/// </summary>
			public interface MaterialProvider
			{
				// get material for bunch primitive. the returned material MUST has perperty of "_PAV_PerInstanceBufferSize"
				Material GetBunchPrimitiveMaterial(AvatarBunch bunch, AvatarPrimitive avatarPrimitive,
					Material srcMaterial);

				// fill per instance material data.
				void UpdateMaterialProperties(AvatarPrimitive avatarPrimitive,
					List<AvatarBunchItem> renderableBunchItems,
					MaterialPropertyBlock materialPropertyBlock, Material material);
			}

			/// <summary>
			/// Stage
			/// </summary>
			enum Stage
			{
				/// <summary>
				/// None
				/// </summary>
				None = 0,
				/// <summary>
				/// Loading
				/// </summary>
				Loading = 1,
				/// <summary>
				/// Working
				/// </summary>
				Working = 2,
				/// <summary>
				/// Destroyed
				/// </summary>
				Destroyed = 3,
			}

			// primitive bunch. one avatar may has one more primitives.
			private class AvatarPrimitiveBunch
			{
				//  primitive of source avatar entity lod.
				public AvatarPrimitive primitive;

				// mesh in primitive.
				public Mesh mesh;

				// material in primitive.
				public Material material;


				// cached bone count the mesh needed.
				internal int boneCount;

				// instance datas, elements are line of the avatar instance in bonesTex.
				//public List<float> perInstanceBoneStart;
				// dynamic data of bones of all avatar element in _avatarBunchClients
				internal Texture2D bonesTex;

				// mesh bone weights texture.
				internal Texture2D boneWeightsTex;

				// material property block.
				internal MaterialPropertyBlock materialPropertyBlock;

				// runtime data.
				internal System.IntPtr boneTexPixelsBuffer; // bonesTex
				internal int boneTexWidth;
				internal int boneTexHeight;
			}

			#endregion


			#region Public Properties

			/// <summary>
			/// Gets native handle.
			/// </summary>
			public System.IntPtr nativeHandle
			{
				get => _nativeHandle;
			}

			// Whether has been destoryed.
			public bool isDestroyed
			{
				get => _stage == Stage.Destroyed;
			}

			// material provider.MUST be set before any source avatar data loaded.
			public MaterialProvider materialProvider;

			#endregion


			#region Public Methods

			
            //@brief Adds avatar bunch reference. 
            //@param targetEntity avatar entity who want to be replaced with the avatar bunch.
            internal AvatarBunchItem AddAvatarBunchItem(AvatarEntity targetEntity)
			{
				if (isDestroyed)
				{
					return null;
				}

				var bunchItem = new AvatarBunchItem(this);
				_avatarBunchItems.Add(bunchItem);

				// initialize the client if _bunchPrimitives has been created.
				bunchItem.Initialize(targetEntity);
				return bunchItem;
			}

			// Remove avatar bunch reference. Invoked from AvatarBunchClient when a avatar need not show with the avatar bunch.
			internal void RemoveAvatarBunchItem(AvatarBunchItem bunchItem)
			{
				if (_avatarBunchItems.Remove(bunchItem))
				{
					bunchItem.OnRemovedFromAvatarBunch();
				}
			}

			/// <summary>
			/// Load avatar with this AvatarBunch.
			/// </summary>
			/// <param name="loadContext">Load configuration</param>
			public void LoadAvatar(AvatarLoadContext loadContext)
			{
				// label as loading stage.
				_stage = Stage.Loading;

				_avatar = PicoAvatarManager.instance.LoadAvatar(loadContext, (avatar, avatarEntity) =>
				{
					if (avatarEntity != null && _stage == Stage.Loading)
					{
						InitializeBunchPrimitives();
						// label as loading stage.
						_stage = Stage.Working;
					}
					else
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, "Failed to load bunch avatar.");
					}
				});

				// no need to show the source avatar.
				_avatar.gameObject.SetActive(false);
			}


			/// <summary>
			/// Sets need udpate material properties. if developer need update material properties should invoke the method.
			/// </summary>
			/// @note
			/// It require materialProvider set before any render.
			public void SetNeedUpdateMaterialProperties()
			{
				_needUpdateMaterialProperties = true;
			}

			#endregion


			#region Private Properties

			// native handle.
			System.IntPtr _nativeHandle;

			// current object stage.
			Stage _stage = Stage.None;

			// list of avatar bunch clients.
			List<AvatarBunchItem> _avatarBunchItems = new List<AvatarBunchItem>();

			// avatar that provide render data.
			PicoAvatar _avatar;

			// avatar entity lod that provide render data.
			AvatarLod _avatarLod;

			// primitive bunch list. head/body may need splitted to standalone primitive.
			List<AvatarPrimitiveBunch> _bunchPrimitives = new List<AvatarPrimitiveBunch>();

			// bunch primitives that need rendered this frame.
			List<AvatarBunchItem> _renderableBunchItems = new List<AvatarBunchItem>();

			// matrices of all _avatarBunchClients
			Matrix4x4[] _clientMatrices;

			// global avatar bunch list for job works.
			static List<AvatarBunch> _jobAvatarBunchList = new List<AvatarBunch>();

			// visible instance count.
			int visibleCount = 0;

			// hash code for last visible bunch list.
			long _lastVisibleBunchListHashCode = 0;

			// whether need update material properties
			bool _needUpdateMaterialProperties = true;
			//

			#endregion


			#region Protected / Private Methods

			internal AvatarBunch()
			{
			}

			// Destroy the object.
			internal void OnDestroy()
			{
				Destroy();
			}

			// Destroy the object.
			private void Destroy()
			{
				// label as destroyed stage.
				_stage = Stage.Destroyed;

				_renderableBunchItems.Clear();
				//
				foreach (var x in _bunchPrimitives)
				{
					if (x.mesh != null)
					{
						UnityEngine.Object.DestroyImmediate(x.mesh);
					}

					if (x.material != null)
					{
						UnityEngine.Object.DestroyImmediate(x.material);
					}

					if (x.bonesTex != null)
					{
						UnityEngine.Object.DestroyImmediate(x.bonesTex);
					}
				}

				_bunchPrimitives.Clear();
				//
				foreach (var x in _avatarBunchItems)
				{
					x.OnRemovedFromAvatarBunch();
				}

				_avatarBunchItems.Clear();

				// release native handle.
				if (_nativeHandle != System.IntPtr.Zero)
				{
					NativeObject.ReleaseNative(ref _nativeHandle);
				}

				if (_avatar != null)
				{
					if (PicoAvatarManager.instance != null)
					{
						PicoAvatarManager.instance.UnloadAvatar(_avatar);
					}

					_avatar = null;
				}

				//
				_avatarLod = null;
			}

			// Job to pre-update render data. currently bone matrices updated from native part.
			private struct PreUpdateRenderDataTJob : IJob
			{
				// By default containers are assumed to be read & write
				[ReadOnly] public int avatarBunchIndex;

				[ReadOnly] public int primitiveIndex;

				// The code actually running on the job
				public void Execute()
				{
					var avatarBunch = _jobAvatarBunchList[avatarBunchIndex];
					avatarBunch.PreUpdateRenderDataT(primitiveIndex);
				}
			}

			/// <summary>
			/// Clear job system context for updating render data.
			/// </summary>
			static internal void ClearUpdateRenderDataJobContext()
			{
				_jobAvatarBunchList.Clear();
			}

			/// <summary>
			/// schedule pre-update for render data.get bone data from c++.
			/// </summary>
			/// <param name="jobHandles"></param>
			/// <param name="jobIndex"></param>
			internal void SchedulePreUpdateRenderData(NativeArray<JobHandle> jobHandles, ref int jobIndex)
			{
				// always clear and rebuild renderable bunch item list each frame.
				_renderableBunchItems.Clear();

				// if no avatar refer the avatar bunch, skip render.
				if (_avatarBunchItems.Count == 0 || _bunchPrimitives.Count == 0)
				{
					return;
				}

				//
				if (_clientMatrices == null || _clientMatrices.Length < _avatarBunchItems.Count)
				{
					Array.Resize<Matrix4x4>(ref _clientMatrices, _avatarBunchItems.Count);
				}

				// reset visible count and update now.
				int visibleCount = 0;
				long visibleListHashCode = 0;
				if (_renderableBunchItems.Capacity < _avatarBunchItems.Count)
				{
					_renderableBunchItems.Capacity = _avatarBunchItems.Count + 10;
				}

				// collect matrices that must be done in main thread.
				foreach (var x in _avatarBunchItems)
				{
					if (x.visibleThisFrame)
					{
						// gen list that visible this frame.
						_renderableBunchItems.Add(x);
						visibleListHashCode =
							(visibleListHashCode << 5) + visibleListHashCode + x.nativeHandle.ToInt64();

						_clientMatrices[visibleCount++] = x.localToWorldMatrix;
					}
				}

				// if no avatar visible, do nothing.
				if (visibleCount == 0)
				{
					return;
				}

				// check material.
				if (_lastVisibleBunchListHashCode != visibleListHashCode)
				{
					_lastVisibleBunchListHashCode = visibleListHashCode;
					_needUpdateMaterialProperties = true;
				}

				// pre-allocate memory.
				if (_jobAvatarBunchList.Capacity <= _jobAvatarBunchList.Count)
				{
					_jobAvatarBunchList.Capacity = _jobAvatarBunchList.Count + 10;
				}

				var avatarBunchIndex = _jobAvatarBunchList.Count;
				_jobAvatarBunchList.Add(this);

				int primitiveIndex = 0;

				// update and set data.
				foreach (var x in _bunchPrimitives)
				{
					// check update material properties.
					if (_needUpdateMaterialProperties && materialProvider != null)
					{
						x.materialPropertyBlock.Clear();
						//
						materialProvider.UpdateMaterialProperties(x.primitive, _renderableBunchItems,
							x.materialPropertyBlock, x.material);
					}

					var boneTexWidth = x.boneCount * 4;
					var boneTexHeight = (visibleCount + 3) & ~3;

					// check bone texture size. half float.
					// each instance occupy one line.  commonly boneCount of a mesh less than 200, and texture width of 800 are enough.
					// normaly we do not support more than 1024 instance per avatar bunch.
					{
						if (x.bonesTex == null || x.bonesTex.height < boneTexHeight)
						{
							if (x.bonesTex == null)
							{
								// use half4 piexls to store half4x4 matrices
								x.bonesTex = new Texture2D(boneTexWidth, boneTexHeight, TextureFormat.RGBAHalf, false);

								if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoadTrivial))
								{
									AvatarEnv.Log(DebugLogMask.AvatarLoadTrivial,
										string.Format("Create AvatarBunch BoneTex. width:{0} height:{1}", boneTexWidth,
											boneTexHeight));
								}
							}
							else
							{
#if UNITY_2021_1_OR_NEWER
								x.bonesTex.Reinitialize(boneTexWidth, boneTexHeight);
#else
                                x.bonesTex.Resize(boneTexWidth, boneTexHeight);
#endif
							}

							// set to material.
							x.material.SetTexture("g_pavInstanceBonesTex", x.bonesTex);
							x.material.SetInt("g_pavInstanceBonesTexWidth", boneTexWidth);
							x.material.SetInt("g_pavInstanceBonesTexHeight", boneTexHeight);
						}
					}

					//
					x.boneTexWidth = boneTexWidth;
					x.boneTexHeight = boneTexHeight;
					//
					unsafe
					{
						var rawTexData = x.bonesTex.GetRawTextureData<Vector4>();
						x.boneTexPixelsBuffer = (System.IntPtr)rawTexData.GetUnsafePtr();
					}

					//perInstanceBytes

					// schedule job.
					var jobHandle = new PreUpdateRenderDataTJob()
					{
						avatarBunchIndex = avatarBunchIndex,
						primitiveIndex = primitiveIndex++
					};
					jobHandles[jobIndex++] = jobHandle.Schedule();

					//PreUpdateRenderDataT(primitiveIndex++);
				}

				// clear material dirty.
				_needUpdateMaterialProperties = false;
			}

			// do thread work to copy bone data to texture pixel buffer.
			private void PreUpdateRenderDataT(int primitiveIndex)
			{
				var texInfo = _bunchPrimitives[primitiveIndex];
				// each bone matrix occupy 4 pixel in g_pavInstanceBonesTex, and each avatar skeleton occupy one line in g_pavInstanceBonesTex
				// use half4 piexls to store half4x4 matrices
				int curInstance = 0;
				int lineStrip =
					texInfo.boneTexWidth * 4 * sizeof(short); // (bone count * 4 colum) * 4 elements * sizeof(half)
				var curDataBase = texInfo.boneTexPixelsBuffer;
				uint flags = 1; // 0: float4x4 matrices,  1: half4x4 matrices

				// copy data.
				foreach (var item in _renderableBunchItems)
				{
					// fill instance data in AvatarBunchClient.
					item.FillInstanceBoneData(texInfo.primitive, curDataBase, lineStrip, flags);

					// advance base address
					curDataBase += lineStrip;
					++curInstance;
				}
			}

			// Update render data. invoked from AvatarBunchGroup to update all instances.
			internal void UpdateRenderData()
			{
				// if no avatar refer the avatar bunch, skip render.
				if (_renderableBunchItems.Count == 0)
				{
					return;
				}

				// update and set data.
				foreach (var x in _bunchPrimitives)
				{
					x.bonesTex.Apply(false, false);
				}

				DoRender();
			}

			// Do gpu instance render work.
			private void DoRender()
			{
				// if no avatar refer the avatar bunch, skip render.
				if (_renderableBunchItems.Count == 0)
				{
					return;
				}

				foreach (var x in _bunchPrimitives)
				{
					if (x.mesh != null && x.material != null)
					{
						Graphics.DrawMeshInstanced(x.mesh, 0, x.material, _clientMatrices, _renderableBunchItems.Count,
							x.materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false,
							this.gameObject.layer);
						//Graphics.DrawMeshInstanced(x.mesh, 0, x.material, _clientMatrices, _renderableBunchItems.Count,
						//    null, UnityEngine.Rendering.ShadowCastingMode.Off, false, this.gameObject.layer);
					}
				}
			}

			// Initialize bunch primitives.
			private void InitializeBunchPrimitives()
			{
				_avatarLod = _avatar.entity.GetCurrentAvatarLod();
				if (_avatarLod == null || _avatarLod.primitives == null)
				{
					return;
				}

				//
				var avatarBunchShader = PicoAvatarApp.instance.renderSettings.materialConfiguration.avatarBunchShader;
				if (avatarBunchShader == null && materialProvider == null)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						"avatarBunchShader in materialConfiguration should be set, or materialProvider in AvatarBunch should be set!");
					return;
				}

				if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
				{
					AvatarEnv.Log(DebugLogMask.AvatarLoad,
						string.Format("Initialize AvatarBunch. supportsInstancing:{0}", SystemInfo.supportsInstancing));
				}

				//
				var primitives = _avatarLod.primitives;
				foreach (var x in primitives)
                {
                    var primitiveRenderMesh = x.Value.primitiveRenderMesh;
                    if(primitiveRenderMesh == null || primitiveRenderMesh.avatarMeshBuffer == null ||
                        primitiveRenderMesh.renderMaterials == null ||
                        primitiveRenderMesh.renderMaterials.Length == 0 ||
                        primitiveRenderMesh.avatarMeshBuffer.meshInfo.positionCount == 0 ||
                        primitiveRenderMesh.avatarMeshBuffer.meshInfo.boneNameCount == 0)
                    {
                        continue;
                    }

					var primitiveBunch = new AvatarPrimitiveBunch();
					primitiveBunch.primitive = x.Value;
					primitiveBunch.boneCount = (int)primitiveRenderMesh.avatarMeshBuffer.boneNameHashes.Length;
					primitiveBunch.mesh = primitiveRenderMesh.avatarMeshBuffer.mesh;

					// create gpu-instance material for the primitive.
					if (materialProvider != null)
					{
						primitiveBunch.material = materialProvider.GetBunchPrimitiveMaterial(this, x.Value,
							primitiveRenderMesh.renderMaterials[0].GetRuntimeMaterial(primitiveRenderMesh));
					}

					// if failed to create a material, use default one.
					if (primitiveBunch.material == null)
					{
						primitiveBunch.material = primitiveRenderMesh.renderMaterials[0]
							.GetRuntimeMaterial(primitiveRenderMesh);
						primitiveBunch.material.shader = avatarBunchShader;
					}

					primitiveBunch.material.enableInstancing = true;

					// material property block.
					primitiveBunch.materialPropertyBlock = new MaterialPropertyBlock();

					{
						primitiveBunch.boneWeightsTex =
							primitiveRenderMesh.avatarMeshBuffer.CreateSkinDataAsTexture2D(primitiveRenderMesh
								.nativeRenderMeshHandler);
						primitiveBunch.material.SetTexture("g_pavBoneWeightsTex", primitiveBunch.boneWeightsTex);
						primitiveBunch.material.SetInt("g_pavBoneWeightsTexWidth", primitiveBunch.boneWeightsTex.width);
						primitiveBunch.material.SetInt("g_pavBoneWeightsTexHeight",
							primitiveBunch.boneWeightsTex.height);
					}

					primitiveBunch.mesh.boneWeights = null;
					primitiveBunch.mesh.bindposes = null;
					//
					_bunchPrimitives.Add(primitiveBunch);

					//if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
					//{
					//    AvatarEnv.Log(DebugLogMask.AvatarLoad, String.Format("avatarBunchID {0} Shader name:{1} tex width:{2} tex height:{3}",
					//        primitiveBunch.primitive.nodeId,  avatarBunchShader.name, primitiveBunch.boneWeightsTex.width, primitiveBunch.boneWeightsTex.height) );
					//}
				}
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBunch_New(System.IntPtr avatarBunchHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBunch_Initialize(System.IntPtr nativeHandle,
				System.IntPtr avatarEntityLodHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBunch_AddAvatarPrimitive(System.IntPtr nativeHandle,
				System.IntPtr avatarPrimitiveHandle);

			#endregion
		}
	}
}