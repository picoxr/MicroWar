using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;

namespace Pico
{
	namespace Avatar
	{
		public class AvatarLod : MonoBehaviour
		{
			#region Public Types

			// Avatar Lod Stage Type.
			public enum Stage
			{
				None,
				Attached,
				Building,
				Working,
				Destroyed,
			}

			#endregion


			#region Public Properties

			// Whether has been destoryed.
			public bool isDestroyed
			{
				get => _stage == Stage.Destroyed;
			}

			// owner who owns the object.
			public AvatarEntity owner { get; private set; } = null;

			// Whether primitives ready.
			public bool isPrimitivesReady { get; private set; } = false;

			// Primitives the lod owns.
			public Dictionary<uint, AvatarPrimitive> primitives
			{
				get => _primitives;
			}

			// Native handle.
			internal System.IntPtr nativeHandle
			{
				get => _nativeHandle;
			}

			/// <summary>
			/// Gets the lod level.
			/// </summary>
			public AvatarLodLevel lodLevel
			{
				get => _lodLevel;
			}

			// avatar skeleton
			public AvatarSkeleton avatarSkeleton
			{
				get => _avatarSkeleton;
			}

			#endregion


			#region Public Framework Methods

			public AvatarLod()
			{
				//
				if (PicoAvatarStats.instance != null)
				{
					PicoAvatarStats.instance.IncreaseInstanceCount(PicoAvatarStats.InstanceType.AvatarLod);
				}
			}

			#endregion


			#region Privatr Fields

			// native handle.
			private System.IntPtr _nativeHandle;

			// current stage.
			private Stage _stage = Stage.None;

			// working thread count.
			private volatile int _workingThreadCount = 0;

			// the lod level
			private AvatarLodLevel _lodLevel;

			// all primitives in the lod.
			private Dictionary<uint, AvatarPrimitive> _primitives;

			// all primitives that need update render data in the lod.
			private List<AvatarPrimitive> _simulationNeededPrimitives = null;

			// all primitives that need update frame in the lod.
			private List<AvatarPrimitive> _updationNeededPrimitives = null;

			// runtime skeleton.
			private AvatarSkeleton _avatarSkeleton;

			#endregion


			#region Private/Friend Methods

            /**
             * Build primitives asynchronously.
             */
            internal void AsyncBuildPrimitives()
            {
                if (isPrimitivesReady)
                {
                    // check dirty state
                    _avatarSkeleton.CheckDirtyState();
                    //
                    owner.Notify_AvatarLodBuildFinished(_lodLevel);
                    return;
                }

				// can only build when attached stage.
				if (_stage != Stage.Attached)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						"C# AvatarLod can only build primitives when newly attached and native data ready.");
					return;
				}

				// set current building stage.
				_stage = Stage.Building;

				// build transforms first.
				{
					var nativeSkeleton = pav_AvatarLod_GetSkeleton(this.nativeHandle);
					if (nativeSkeleton == System.IntPtr.Zero)
					{
						AvatarEnv.Log(DebugLogMask.GeneralError,
							"C# AvatarLod can only build primitives when native avatar skeleton ready.");
						return;
					}

					_avatarSkeleton = new AvatarSkeleton(nativeSkeleton);
					_avatarSkeleton.Retain();
					_avatarSkeleton.CreateTransforms(this.transform);
				}

                // if avatar bunch lod level less than the lod level, create and load primitives.
                //if (owner.owner.capabilities.avatarBunchLodLevel < 0 || owner.owner.capabilities.avatarBunchLodLevel > _lodLevel)
                // Currently if avatar bunch need, should not load self data.
                if ((int)owner.owner.capabilities.avatarBunchLodLevel < 0)
                {
                    PicoAvatarApp.instance.StartCoroutine(Coroutine_BuildPrimitives());
                }
				else
				{
					//
					_stage = Stage.Working;

					// set primitives ready flag.
					isPrimitivesReady = true;

					// Need queue the ready entity lod and set visible after application developer process the ready event.
					if (PicoAvatarManager.instance != null)
					{
						PicoAvatarManager.instance.QueueReadyAvatarLod(this);
					}
				}
			}

			// Build primitive render meshes in coroutine.
			private IEnumerator Coroutine_BuildPrimitives()
			{
				// check destroyed.
				if (isDestroyed)
				{
					Destroy();
					yield break;
				}
				
				AvatarEnv.Log(DebugLogMask.GeneralInfo, string.Format("PicoAvatarApp.instance.renderSettings.useCustomMaterial is {0}.",PicoAvatarApp.instance.renderSettings.useCustomMaterial));
				
				int accumYieldCount = 0;
				int PrimLoadYieldCount = owner.owner.allowBlockFrameWhenLoading ? 100 : 4;

				//
				if (CreateAvatarPrimitives())
				{
					// build render mesh / materials for un-merged primitives.
					foreach (var primitive in _primitives.Values)
					{
						if (primitive != null && !primitive.isMergedToAvatarLod)
						{
							//
							if (!primitive.BuildFromNativeRenderMeshAndMaterial())
							{
								Destroy();
								yield break;
							}

							// wait a frame.
							if (accumYieldCount++ > PrimLoadYieldCount)
							{
								accumYieldCount = 0;
								yield return null;
							}

							// check destroyed.
							if (_stage != Stage.Building)
							{
								Destroy();
								yield break;
							}
						}
					}

					// check whether destroyed.
					if (_stage != Stage.Building)
					{
						yield break;
					}
				}

				//
				_stage = Stage.Working;

				// set primitives ready flag.
				isPrimitivesReady = true;

				// Need queue the ready entity lod and set visible after application developer process the ready event.
				if (PicoAvatarManager.instance != null)
				{
					PicoAvatarManager.instance.QueueReadyAvatarLod(this);
				}
			}

			internal bool RebuildMaterials()
			{
				if (_primitives == null)
				{
					return false;
				}

				foreach (var primitive in _primitives.Values)
				{
					if (primitive != null && !primitive.isMergedToAvatarLod)
					{
						if (!primitive.RebuildMaterialsFromNative())
						{
							return false;
						}
					}
				}

				return true;
			}

			private bool CreateAvatarPrimitives()
			{
				// if has created, do nothing.
				if (_primitives != null)
				{
					return true;
				}

				// log
				//if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
				//{
				//    AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format("AvatarLod {0}.CreateAvatarPrimitives", this._lodLevel));
				//}

				var primitiveList = new PrimitiveList();
				// a very big count.
				primitiveList.count = 100;
				if (pav_AvatarLod_GetPrimitives(_nativeHandle, ref primitiveList) != NativeResult.Success)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						"avatar lod has now primitives! avatarId:" + this.owner.owner.avatarId);
					return false;
				}

				var newPrimitives = new Dictionary<uint, AvatarPrimitive>();

				unsafe
				{
					if (!owner.owner.capabilities.forBake)
					{
						for (uint i = 0; i < primitiveList.count; ++i)
						{
							uint nodeId = (uint)primitiveList.ids[i];
							var primitive = new AvatarPrimitive((System.IntPtr)primitiveList.pointers[i], nodeId, this);
							primitive.Retain();

							// disable native bone accumulation in unity.
							primitive.SetControlFlags((uint)AvatarPrimitiveControlFlags.DisableNativeBoneAccumulation);
							//
							newPrimitives.Add(nodeId, primitive);
						}
					}
				}

				// remove old primitives.
				_primitives = newPrimitives;
				//
				return true;
			}

			internal bool IsAllowGpuDataCompress(bool merged)
			{
				if (merged)
				{
					return true;
				}

				// if editor mode, disable compress.
				return !owner.owner.capabilities.allowEdit;
			}

			// Destroy old primitives.
			private void DestoryPrimitives()
			{
				if (_primitives != null)
				{
					foreach (var primitive in _primitives)
					{
						primitive.Value.Destory();
						//
						primitive.Value.Release();
					}

					_primitives = null;
				}

				//
				if (_simulationNeededPrimitives != null)
				{
					_simulationNeededPrimitives.Clear();
				}
			}
			
            //Attach to native object handle.
            //@note reference count of nativeAvatarLod should be increased by invoker, here simply store the handle.
            internal void AttachNative(AvatarEntity owner, System.IntPtr nativeAvatarLod, AvatarLodLevel lodLevel)
			{
				if (_stage == Stage.Destroyed)
				{
					return;
				}

				if (_nativeHandle != System.IntPtr.Zero)
				{
					throw new System.ArgumentException("the PicoAvatarLod has attached to native object.");
				}

				this.owner = owner;
				_lodLevel = lodLevel;
				_nativeHandle = nativeAvatarLod;
				//
				_stage = Stage.Attached;
			}

			// Definitely destroy the object.
			internal void Destroy()
			{
				//
				if (PicoAvatarStats.instance != null && _stage != Stage.Destroyed)
				{
					PicoAvatarStats.instance.DecreaseInstanceCount(PicoAvatarStats.InstanceType.AvatarLod);
				}

				_stage = Stage.Destroyed;

				// if in working thread, wait finish.

				while (Interlocked.CompareExchange(ref _workingThreadCount, 0, 0) != 0)
				{
					AvatarEnv.Log(DebugLogMask.GeneralInfo,
						"Wait some ms when loading thread finish before detroy AvatarLod.");
					//
					System.Threading.Thread.Sleep(5);
				}

				// destroy primitives.
				DestoryPrimitives();
				//
				if (_avatarSkeleton != null)
				{
					_avatarSkeleton.Release();
					_avatarSkeleton = null;
				}

				// destroy native objects.
				NativeObject.ReleaseNative(ref _nativeHandle);
			}

			// Invoked by Unity framework to destory the object.
			internal void OnDestroy()
			{
				Destroy();
			}

			// @brief do pre-update simulation render data in work thread.
			internal void PreUpdateSimulationRenderDataT()
			{
				if (_stage == Stage.Working && isPrimitivesReady &&
				    (_primitives != null || owner.owner.forceUpdateSkeleton))
				{
					if (_avatarSkeleton != null)
					{
						_avatarSkeleton.PreUpdateSimulationRenderDataT();
					}
				}
			}

			// schedule post update simulation render data jobs.
			internal void SchedulePostUpdateSimulationRenderDataJobs(int avatarEntityIndex,
				NativeArray<Unity.Jobs.JobHandle> jobHandles, ref int jobIndex)
			{
				if (_stage == Stage.Working && isPrimitivesReady &&
				    (_primitives != null || owner.owner.forceUpdateSkeleton))
				{
					if (_updationNeededPrimitives != null)
					{
						foreach (var primitive in _updationNeededPrimitives)
						{
							primitive.UpdateFrame();
						}
					}

					if (_avatarSkeleton != null)
					{
						_avatarSkeleton.SchedulePostUpdateSimulationRenderDataJobs(jobHandles, ref jobIndex);
					}
				}
			}

			// Update after amaz
			internal void UpdateSimulationRenderData(float elapsedTime)
			{
				if (_stage == Stage.Working && isPrimitivesReady && _primitives != null)
				{
					uint mergedMeshDirtyFlags = 0;
					// check dirty.
					if (pav_AvatarLod_GetAndClearAnyPrimitiveDirtyFlag(_nativeHandle))
					{
						foreach (var primitive in _primitives.Values)
						{
							var dirtyFlags = primitive.CheckUpdatePrimitiveDrityData();
							if (dirtyFlags != 0 && primitive.isMergedToAvatarLod)
							{
								mergedMeshDirtyFlags |= dirtyFlags;
							}
						}
					}

					// update simulation data. CURRENTLY ONLY face expression blendshape need updated in simulation render data.
					if (this.owner.expressionPlaybackEnabled && _simulationNeededPrimitives != null)
					{
						foreach (var primitive in _simulationNeededPrimitives)
						{
							if (!primitive.isMergedToAvatarLod && primitive.needUpdateSimulation)
							{
								primitive.UpdateSimulationRenderData();
							}
						}
					}
				}
			}

			internal void SetAvatarEffectKind(AvatarEffectKind avatarEffectKind)
			{
				//
				foreach (var j in primitives)
				{
					if (j.Value.primitiveRenderMesh)
					{
						j.Value.primitiveRenderMesh.SetAvatarEffectKind(avatarEffectKind);
					}
				}
			}

			// Update light env.
			internal void OnAvatarSceneLightEnvChanged(PicoAvatarSceneLightEnv lightEnv)
			{
				foreach (var j in primitives)
				{
					if (j.Value.primitiveRenderMesh)
					{
						j.Value.primitiveRenderMesh.Notify_AvatarSceneLightEnvChanged(lightEnv);
					}
				}
			}

			// @brief Add primitive whose render data need be updated each frame. Invoked from PicoAvatarRenderMesh
			internal void AddSimulationNeededAvatarPrimitive(AvatarPrimitive primitive)
			{
				if (_simulationNeededPrimitives == null)
				{
					_simulationNeededPrimitives = new List<AvatarPrimitive>();
				}

				_simulationNeededPrimitives.Add(primitive);
			}

			// @brief Add primitive that need be updated each frame. Invoked from AvatarPrimitive.
			internal void AddUpdationNeededAvatarPrimitive(AvatarPrimitive primitive)
			{
				if (_updationNeededPrimitives == null)
				{
					_updationNeededPrimitives = new List<AvatarPrimitive>();
				}

				_updationNeededPrimitives.Add(primitive);
			}

			#endregion


			#region For Editor

			internal void PartialRebuild(string modificationJsonText)
			{
				try
				{
					// add/remove additive skeleton from mesh assets.
					if (avatarSkeleton != null)
					{
						avatarSkeleton.PartialCreateAdditiveSkeletonTransforms();
					}

					var primitiveList = new PrimitiveList();
					// a very big count.
					primitiveList.count = 100;
					if (pav_AvatarLod_GetPrimitives(_nativeHandle, ref primitiveList) != NativeResult.Success)
					{
						AvatarEnv.Log(DebugLogMask.GeneralError,
							"avatar lod has now primitives! avatarId:" + this.owner.owner.avatarId);
						return;
					}

					//
					var newAllPrimitives = new HashSet<uint>();

					// add new and collect active primitives
					unsafe
					{
						for (uint i = 0; i < primitiveList.count; ++i)
						{
							uint primitiveId = (uint)primitiveList.ids[i];
							if (!_primitives.ContainsKey(primitiveId))
							{
								var primitive = new AvatarPrimitive((System.IntPtr)primitiveList.pointers[i],
									primitiveId, this);
								//if ((primitive.nodeTypes & ((int)AvatarNodeTypes.Body)) != 0)
								//{
								//    AvatarEnv.Log(DebugLogMask.GeneralError, "body rebuild success...");
								//}
								primitive.Retain();
								//
								_primitives.Add(primitiveId, primitive);
								//
								primitive.BuildFromNativeRenderMeshAndMaterial();
							}
							else
							{
								// need release primitve 
								var ptr = (System.IntPtr)primitiveList.pointers[i];
								NativeObject.ReleaseNative(ref ptr);
							}

							newAllPrimitives.Add(primitiveId);
						}
					}

					// find and delete deactive primitives.
					{
						var deletedPrimitiveIds = new List<uint>();
						foreach (var primitivePair in _primitives)
						{
							if (!newAllPrimitives.Contains(primitivePair.Key))
							{
								deletedPrimitiveIds.Add(primitivePair.Key);
							}
						}

						// delete AvatarPrimitives
						foreach (var primitiveId in deletedPrimitiveIds)
						{
							AvatarPrimitive primitive;
							if (_primitives.TryGetValue(primitiveId, out primitive))
							{
								// for debug
								//AvatarEnv.Log(DebugLogMask.GeneralError, "c#: remove primitive id: " + primitiveId + " name:" + primitive.primitiveRenderMesh.name);
								_primitives.Remove(primitiveId);

								if (_simulationNeededPrimitives != null)
								{
									_simulationNeededPrimitives.Remove(primitive);
								}

								if (primitive.needUpdateFrame && _updationNeededPrimitives != null)
								{
									_updationNeededPrimitives.Remove(primitive);
								}

								//
								primitive.Destory();
								primitive.Release();
							}
						}
					}
				}
				catch (System.Exception e)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						string.Format("PartialRebuild error. message: {0} stack: {1}", e.Message, e.StackTrace));
				}
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			// primitive pointer list.
			[StructLayout(LayoutKind.Sequential, Pack = 4)]
			public struct PrimitiveList
			{
				[MarshalAs(UnmanagedType.U8)] public ulong count;

				[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 100)]
				public long[] pointers;

				[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 100)]
				public ulong[] ids;
			}

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarLod_GetLodLevel(System.IntPtr avatarLodHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarLod_GetSkeleton(System.IntPtr avatarLodHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarLod_GetPrimitives(System.IntPtr avatarLodHandle,
				ref PrimitiveList primitives);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarLod_GetPrimitive(System.IntPtr avatarLodHandle, uint nodeId);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarLod_TryMerge(System.IntPtr nativeHandle, System.IntPtr nodeIds,
				int nodeIdCount, uint meshVertexDataFormat, uint morphVertexDataFormat);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarLod_GetMergedRenderMaterial(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarLod_GetMergedRenderMesh(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_AvatarLod_GetAndClearAnyPrimitiveDirtyFlag(System.IntPtr nativeHandle);

			#endregion
		}
	}
}