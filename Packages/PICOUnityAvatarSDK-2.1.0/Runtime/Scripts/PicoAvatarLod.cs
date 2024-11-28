using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Runtime.CompilerServices;

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

			// merged render mesh
			private PicoAvatarMergedRenderMesh _mergedRenderMesh;

			public PicoAvatarMergedRenderMesh mergedRenderMesh { get => _mergedRenderMesh; }

			private uint _mergedPrimitiveCount = 0;

			public uint mergedPrimitiveCount { get => _mergedPrimitiveCount; }

			// merged render material
			private PicoAvatarMergedRenderMaterial _mergedRenderMaterial;

			public PicoAvatarMergedRenderMaterial mergedRenderMaterial { get => _mergedRenderMaterial; }

			#endregion


			#region Private/Friend Methods

			
             //Build primitives asynchronously.
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

			public Bounds firstSuggestedBounds = new Bounds();
			public Bounds suggestedBounds = new Bounds();
			public Bounds firstRawBounds = new Bounds();
			public Bounds rawBounds = new Bounds();
#if PAV_INTERNAL_DEV
			public GameObject boundsCube;
			public bool UpdateBoundsCube(Bounds _bounds)
            {
				if (boundsCube)
                {
                    boundsCube.transform.localPosition = _bounds.center;
                    boundsCube.transform.localScale = _bounds.size;
                }
				return boundsCube != null;
            }
#endif

			// Build primitive render meshes in coroutine.
			private IEnumerator Coroutine_BuildPrimitives()
			{
				// check destroyed.
				if (isDestroyed)
				{
					Destroy();
					yield break;
				}

				AvatarEnv.Log(DebugLogMask.GeneralInfo, string.Format("PicoAvatarApp.instance.renderSettings.useCustomMaterial is {0}.", PicoAvatarApp.instance.renderSettings.useCustomMaterial));

				int accumYieldCount = 0;
				int PrimLoadYieldCount = owner.owner.allowBlockFrameWhenLoading ? 100 : 4;

				//
				if (CreateAvatarPrimitives())
				{
					_mergedPrimitiveCount = 0;
					uint notMerged = 0;

					foreach (var primitive in _primitives.Values)
					{
						if (primitive != null)
						{
							if (primitive.isMergedToAvatarLod)
								++_mergedPrimitiveCount;
							else
								++notMerged;
						}
					}
					UnityEngine.Debug.Log(string.Format("pav: Coroutine_BuildPrimitives Merged primitive: {0}, Not merged primitive: {1}", _mergedPrimitiveCount, notMerged));

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

					if (PicoAvatarApp.instance != null && !this.owner.owner.capabilities.allowEdit && PicoAvatarApp.instance.appSettings.enableStaticMeshBatching)
					{
						bool needTangent = false;
                        if (BuildFromNativeMergedMesh(ref needTangent))
						{
							var nativeMergedMtl = BuildMaterialFromNativeMergedMesh();
							if (nativeMergedMtl != System.IntPtr.Zero)
							{
                                while (_mergedRenderMaterial != null && !_mergedRenderMaterial.ConditionalContinueMerging())
                                {
									// yield to wait for all compute shader finish executing
									// for 60fps about 2 frames
                                    yield return new WaitForSeconds(.03f);
                                }

                                if (pav_AvatarLod_ConditionalMergeMaterials(_nativeHandle) != NativeResult.Success)
                                {
                                    DestroyMergedData();
                                }

                                Material renderMtl = _mergedRenderMaterial.Build(nativeMergedMtl, this, needTangent);
                                if (renderMtl == null)
                                {
                                    DestroyMergedData();
                                }

                                int colorShiftID = Shader.PropertyToID("_ColorShift");
#if UNITY_2021_1_OR_NEWER
                                if (renderMtl.HasFloat(colorShiftID))
#else
                                if (renderMtl.HasProperty(colorShiftID))
#endif
                                {
                                    renderMtl.SetFloat(colorShiftID, 0.0f);
                                    renderMtl.DisableKeyword("_COLOR_SHIFT");
                                }
                                renderMtl.EnableKeyword("PAV_COLOR_REGION_BAKED");

                                AvatarEnv.Log(DebugLogMask.GeneralInfo, "PicoAvatarApp.Batching.GPU: All tasks finished");
                                _mergedRenderMesh.skinnedMeshRenderer.sharedMaterial = renderMtl;

                                // Hide merged mesh when switching to AvatarManifestationType.Head or AvatarManifestationType.HeadHands mode
                                // Because head and gloves will never be merged, so we don't need to check primitiveNodeTypes here
                                if (owner.owner.capabilities.manifestationType == AvatarManifestationType.Head ||
                                    owner.owner.capabilities.manifestationType == AvatarManifestationType.HeadHands)
                                {
                                    _mergedRenderMesh.gameObject.SetActive(false);
                                }
                                else
                                {
                                    _mergedRenderMesh.gameObject.SetActive(true);
                                }
							}
                        }
					}

                    yield return null;
                    var activeScale = this.owner.gameObject.transform.localScale;
                    if (activeScale == Vector3.zero)
                        this.owner.gameObject.transform.localScale = Vector3.one;
                    AccumulateBounds(true);
					if (activeScale == Vector3.zero)
                        this.owner.gameObject.transform.localScale = activeScale;
                    firstSuggestedBounds = suggestedBounds;
					firstRawBounds = rawBounds;

#if PAV_INTERNAL_DEV
					var go = GameObject.Find("PicoAvatarTestBoundsCube");
					if (go != null)
                    {
                        boundsCube = GameObject.Instantiate(go);
                        boundsCube.transform.parent = this.transform;
                        boundsCube.transform.localPosition = suggestedBounds.center;
                        boundsCube.transform.localScale = suggestedBounds.size;
                        boundsCube.transform.localRotation = Quaternion.identity;
                        boundsCube.SetActive(false);
                    }

#endif

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

			public bool AccumulateBounds(bool needUpdateRenderer)
			{
				//var activeBefore = this.gameObject.activeSelf;
				//if (!activeBefore)
				//	this.gameObject.SetActive(true);
				bool isFirst = true;
				foreach (var primitive in _primitives.Values)
				{
					if (primitive == null || primitive.primitiveRenderMesh == null || primitive.primitiveRenderMesh.skinMeshRenderer == null)
						continue;
                    var smr = primitive.primitiveRenderMesh.skinMeshRenderer;
                    var lastUpdateWhenOffscreen = smr.updateWhenOffscreen;
                    smr.rootBone = null;
                    smr.updateWhenOffscreen = true;
                    if (isFirst)
                    {
						isFirst = false;
						suggestedBounds = smr.localBounds;
					}
					else
                    {
						suggestedBounds.Encapsulate(smr.localBounds);
					}
                    smr.updateWhenOffscreen = lastUpdateWhenOffscreen;
                }
				rawBounds = suggestedBounds;
				suggestedBounds = AdjustBounds(suggestedBounds);
				if (needUpdateRenderer)
                {
                    foreach (var primitive in _primitives.Values)
                    {
                        if (primitive == null || primitive.primitiveRenderMesh == null || primitive.primitiveRenderMesh.skinMeshRenderer == null)
                            continue;
                        var smr = primitive.primitiveRenderMesh.skinMeshRenderer;
                        smr.localBounds = suggestedBounds;
                        smr.rootBone = smr.transform;
                    }
                }
				return !isFirst;
            }

			public bool AccumulateSMR(bool needUpdateRenderer)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Bounds:Whole");
				UnityEngine.Profiling.Profiler.BeginSample("Bounds:GetComponentsInChildren");
				var smrs = this.GetComponentsInChildren<SkinnedMeshRenderer>();
				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Bounds:RecalculateBounds");
				bool ret = AccumulateSMR(smrs, needUpdateRenderer);
				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.EndSample();
				return ret;
			}

			public bool AccumulateSMR(SkinnedMeshRenderer[] smrs, bool needUpdateRenderer)
			{
				//var activeBefore = this.gameObject.activeSelf;
				//if (!activeBefore)
				//	this.gameObject.SetActive(true);
				bool isFirst = true;
				foreach (var smr in smrs)
				{
                    //if (!smr.gameObject.name.Contains("Hair")) continue;
                    var lastUpdateWhenOffscreen = smr.updateWhenOffscreen;
                    smr.rootBone = null;
                    smr.updateWhenOffscreen = true;
                    UnityEngine.Profiling.Profiler.BeginSample("Bounds:GetLocalBounds");
					Bounds localBounds = smr.localBounds;
					UnityEngine.Profiling.Profiler.EndSample();
					UnityEngine.Profiling.Profiler.BeginSample("Bounds:Encapsulate");
                    if (isFirst)
                    {
                        isFirst = false;
                        suggestedBounds = smr.localBounds;
                    }
                    else
                    {
                        suggestedBounds.Encapsulate(smr.localBounds);
                    }
                    UnityEngine.Profiling.Profiler.EndSample();
                    smr.updateWhenOffscreen = lastUpdateWhenOffscreen;
                }
				rawBounds = suggestedBounds;
				suggestedBounds = AdjustBounds(suggestedBounds);
				if (needUpdateRenderer)
                {
                    foreach (var smr in smrs)
                    {
                        smr.localBounds = suggestedBounds;
                        smr.rootBone = smr.transform;
                    }
                }
				//if (!activeBefore)
				//	this.gameObject.SetActive(false);
				return !isFirst;
			}

			Bounds AdjustBounds(Bounds bounds)
			{
				float maxVal = Mathf.Max(bounds.extents.x, bounds.extents.z);
				float minVal = Mathf.Min(bounds.extents.x, bounds.extents.z);
				float lerpVal = Mathf.Lerp(minVal, maxVal, 0.8f);
				if (bounds.extents.x > bounds.extents.z)
				{
					bounds.extents = new Vector3(bounds.extents.x, bounds.extents.y, lerpVal);
				}
				else
				{
					bounds.extents = new Vector3(lerpVal, bounds.extents.y, bounds.extents.z);
				}
				return bounds;
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

				// TODO: Merged material ?

				return true;
			}

			private bool CreateAvatarPrimitives()
			{
				// if has created, do nothing.
				if (_primitives != null)
				{
					return true;
				}

				var primitiveMergeList = new PrimitiveMergeList();
				primitiveMergeList.count = 0;
				if (PicoAvatarApp.instance != null && !owner.owner.capabilities.allowEdit && PicoAvatarApp.instance.appSettings.enableStaticMeshBatching)
					pav_AvatarLod_TryMergePrimitives(_nativeHandle, ref primitiveMergeList);

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
							// UnityEngine.Debug.Log(string.Format("pav: Index: {0}, Merged: {1}", i, primitiveMergeList.mergedToLods[i]));
							uint nodeId = (uint)primitiveList.ids[i];
							bool isMerged = primitiveMergeList.count > 0 ? primitiveMergeList.mergedToLods[i] > 0 : false;
							var primitive = new AvatarPrimitive((System.IntPtr)primitiveList.pointers[i], nodeId, this, isMerged);
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

			private void DestroyMergedMesh()
			{
				if (_mergedRenderMesh != null)
				{
					Destroy(_mergedRenderMesh.gameObject);
					_mergedRenderMesh = null;
				}
			}

			private void DestroyMergedMaterial()
			{
				if (_mergedRenderMaterial != null)
				{
					_mergedRenderMaterial.Release();
					_mergedRenderMaterial = null;
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

			internal void DestroyMergedData()
			{
				// destroy merged mesh
				DestroyMergedMesh();
				// destroy merged material
				DestroyMergedMaterial();
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

				DestroyMergedData();

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

				// TODO: mergedRenderMesh����
				if (_stage == Stage.Working)
				{
					if (_mergedRenderMaterial != null)
						_mergedRenderMaterial.TryUpdateMergedMaterialInfo(this);
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

			internal bool BuildFromNativeMergedMesh(ref bool needTangent)
			{
				// AvatarEnv.Log(DebugLogMask.GeneralInfo, "AvatarLod: Start mesh merging");
				if (avatarSkeleton == null)
				{
					DestroyMergedData();
					return false;
				}

				if (mergedPrimitiveCount == 0)
				{
					// Destroy GameObject of mergedRenderMesh when no primitives are merged
					DestroyMergedData();
					return false;
				}

				// Begin render mesh processing
				if (_mergedRenderMesh == null)
				{
					var go = new GameObject(string.Format("MergedMesh{0}_{1}", owner.nativeEntityId, _lodLevel));
					go.hideFlags = HideFlags.DontSave;
					var goTransform = go.transform;
					goTransform.parent = transform;
					goTransform.localPosition = Vector3.zero;
					goTransform.localRotation = Quaternion.identity;
					goTransform.localScale = Vector3.one;
					go.layer = owner.gameObject.layer;
					System.IntPtr nativeMergedMesh = pav_AvatarLod_GetMergedRenderMesh(_nativeHandle);
					if (nativeMergedMesh == System.IntPtr.Zero)
					{
						DestroyMergedData();
						return false;
					}
					_mergedRenderMesh = go.AddComponent<PicoAvatarMergedRenderMesh>();
					_mergedRenderMesh.nativeHandle = nativeMergedMesh;
				}

				int mergedPrimMeshHashCode = (int)pav_AvatarLod_GetMergedHashCode(_nativeHandle);
				// Try get merged buffer from cache
				var mergedBuffer = AvatarMergedMeshBuffer.TryGetMergedMeshBuffer(mergedPrimMeshHashCode);
				if (mergedBuffer != null)
				{
					needTangent = mergedBuffer.mesh.tangents.Length > 0;
					if (!_mergedRenderMesh.Build(this, mergedBuffer))
					{
						// Destroy game object if merged render mesh failed to build
						DestroyMergedData();
						return false;
					}
				}
				else
				{
					if (pav_AvatarLod_MergePrimitives(_nativeHandle) != NativeResult.Success)
					{
						DestroyMergedData();
						return false;
					}

					MergedMeshInfo meshInfo = new MergedMeshInfo();
					if (pav_AvatarLod_GetMergedMeshInfo(_nativeHandle, ref meshInfo) != NativeResult.Success)
					{
						DestroyMergedData();
                        return false;
					}


					// UnityEngine.Debug.Log(string.Format("pav: Pos: {0}, Tan: {1}, uv1: {2}, uv4: {3}, index: {4}", meshInfo.positionCount, meshInfo.tangentCount, meshInfo.uv1Count, meshInfo.uv4Count, meshInfo.indexCount));

					needTangent = meshInfo.tangentCount > 0;
					int boneWeightMultiplier = meshInfo.weight8 > 0 ? 2 : 1;

                    MergedMeshData mergedMeshData	= new MergedMeshData();
                    mergedMeshData.positions		= new NativeArray<Vector3>((int)meshInfo.positionCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    mergedMeshData.normals			= new NativeArray<Vector3>((int)meshInfo.positionCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    mergedMeshData.tangents			= meshInfo.tangentCount > 0 ? new NativeArray<Vector4>((int)meshInfo.tangentCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : new NativeArray<Vector4>();
                    mergedMeshData.colors			= meshInfo.colorCount > 0 ? new NativeArray<Color32>((int)meshInfo.colorCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : new NativeArray<Color32>();
                    mergedMeshData.uv1				= meshInfo.uv1Count > 0 ? new NativeArray<Vector2>((int)meshInfo.uv1Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : new NativeArray<Vector2>();
                    mergedMeshData.uv2				= meshInfo.uv2Count > 0 ? new NativeArray<Vector2>((int)meshInfo.uv2Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : new NativeArray<Vector2>();
                    mergedMeshData.uv3				= meshInfo.uv3Count > 0 ? new NativeArray<Vector2>((int)meshInfo.uv3Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : new NativeArray<Vector2>();
                    mergedMeshData.uv4				= meshInfo.uv4Count > 0 ? new NativeArray<Vector2>((int)meshInfo.uv4Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : new NativeArray<Vector2>();
					mergedMeshData.materialIndices	= new NativeArray<Vector2>((int)meshInfo.positionCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    // Mesh with 8 skinning weights will not be merged, BoneWeight here is ok.
                    mergedMeshData.boneWeights		= new NativeArray<BoneWeight>((int)meshInfo.positionCount * boneWeightMultiplier, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    mergedMeshData.boneNameHashes	= new NativeArray<int>((int)meshInfo.boneCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    mergedMeshData.invBindPoses		= new NativeArray<Matrix4x4>((int)meshInfo.boneCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    mergedMeshData.indices			= new NativeArray<uint>((int)meshInfo.indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

					MergedMeshRawData meshRawData = new MergedMeshRawData();
					unsafe
					{
						meshRawData.positions = (System.IntPtr)mergedMeshData.positions.GetUnsafePtr();
						meshRawData.normals = (System.IntPtr)mergedMeshData.normals.GetUnsafePtr();
						meshRawData.tangents = meshInfo.tangentCount > 0 ? (System.IntPtr)mergedMeshData.tangents.GetUnsafePtr() : System.IntPtr.Zero;
						meshRawData.colors = meshInfo.colorCount > 0 ? (System.IntPtr)mergedMeshData.colors.GetUnsafePtr() : System.IntPtr.Zero;
						meshRawData.uv1 = meshInfo.uv1Count > 0 ? (System.IntPtr)mergedMeshData.uv1.GetUnsafePtr() : System.IntPtr.Zero;
						meshRawData.uv2 = meshInfo.uv2Count > 0 ? (System.IntPtr)mergedMeshData.uv2.GetUnsafePtr() : System.IntPtr.Zero;
						meshRawData.uv3 = meshInfo.uv3Count > 0 ? (System.IntPtr)mergedMeshData.uv3.GetUnsafePtr() : System.IntPtr.Zero;
						meshRawData.uv4 = meshInfo.uv4Count > 0 ? (System.IntPtr)mergedMeshData.uv4.GetUnsafePtr() : System.IntPtr.Zero;
						meshRawData.materialIndices = (System.IntPtr)mergedMeshData.materialIndices.GetUnsafePtr();
						meshRawData.boneWeights = (System.IntPtr)mergedMeshData.boneWeights.GetUnsafePtr();
						meshRawData.boneNameHashes = (System.IntPtr)mergedMeshData.boneNameHashes.GetUnsafePtr();
						meshRawData.invBindPoses = (System.IntPtr)mergedMeshData.invBindPoses.GetUnsafePtr();
						meshRawData.indices = (System.IntPtr)mergedMeshData.indices.GetUnsafePtr();
					}
					Action arrayCleanup = () =>
					{
						mergedMeshData.positions.Dispose();
						mergedMeshData.normals.Dispose();
						if (mergedMeshData.tangents.Length > 0)
							mergedMeshData.tangents.Dispose();
						if (mergedMeshData.colors.Length > 0)
							mergedMeshData.colors.Dispose();
						if (mergedMeshData.uv1.Length > 0)
							mergedMeshData.uv1.Dispose();
						if (mergedMeshData.uv2.Length > 0)
							mergedMeshData.uv2.Dispose();
						if (mergedMeshData.uv3.Length > 0)
							mergedMeshData.uv3.Dispose();
						if (mergedMeshData.uv4.Length > 0)
							mergedMeshData.uv4.Dispose();
						mergedMeshData.materialIndices.Dispose();
						mergedMeshData.boneWeights.Dispose();
						mergedMeshData.boneNameHashes.Dispose();
						mergedMeshData.invBindPoses.Dispose();
						mergedMeshData.indices.Dispose();
					};
					if (pav_AvatarLod_GetMergedMeshData(_nativeHandle, ref meshRawData) != NativeResult.Success)
					{
						arrayCleanup();
						DestroyMergedData();
                        return false;
					}

					if (!_mergedRenderMesh.Build(this, mergedPrimMeshHashCode, ref meshInfo, ref mergedMeshData))
					{
                        arrayCleanup();
						DestroyMergedData();
						return false;
					}

					arrayCleanup();
				}
				return true;
				// End merged mesh processing
			}

			internal System.IntPtr BuildMaterialFromNativeMergedMesh()
			{
                // Begin merged material processing
				System.IntPtr nativeMergedMaterial = IntPtr.Zero;
				if (_mergedRenderMaterial == null)
				{
					_mergedRenderMaterial = new PicoAvatarMergedRenderMaterial(true, this);
					_mergedRenderMaterial.Retain();
					nativeMergedMaterial = pav_AvatarLod_GetMergedRenderMaterial(_nativeHandle);
					if (nativeMergedMaterial == System.IntPtr.Zero)
					{
						DestroyMergedData();
						return System.IntPtr.Zero;
					}
				}
				_mergedRenderMaterial.ConditionalDispatchGPUTasks(this, _primitives, nativeMergedMaterial);
                // End merged material processing
				return nativeMergedMaterial;
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

					DestroyMergedMesh();
					DestroyMergedMaterial();

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
									primitiveId, this, false);
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

			[StructLayout(LayoutKind.Sequential, Pack = 4)]
			public struct PrimitiveMergeList
			{
				[MarshalAs(UnmanagedType.U8)] public ulong count;

				[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 100)]
				public ulong[] mergedToLods;
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

			// Begin new primitive batching 
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarLod_TryMergePrimitives(System.IntPtr nativeHandle, ref PrimitiveMergeList primitives);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarLod_MergePrimitives(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarLod_GetMergedHashCode(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarLod_GetMergedMeshInfo(System.IntPtr nativeHandle, ref MergedMeshInfo mergedMeshInfo);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			public static extern NativeResult pav_AvatarLod_GetMergedMeshData(System.IntPtr nativeHandle, ref MergedMeshRawData mergedMeshRawData);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			public static extern NativeResult pav_AvatarLod_ConditionalMergeMaterials(System.IntPtr nativeHandle);
			// End new primitive batching

			#endregion
		}
	}
}