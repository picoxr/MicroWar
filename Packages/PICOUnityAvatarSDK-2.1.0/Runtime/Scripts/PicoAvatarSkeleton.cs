using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using Unity.Jobs;
using UnityEngine.Jobs;
using UnityEngine.Experimental.Rendering;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Runtime skeleton of an avatar lod.
		/// </summary>
		public class AvatarSkeleton : NativeObject
		{
			#region Public Properties

			// Whether enable update from native skeleton.
			public bool enableUpdateFromNativeSkeleton = true;

			// used as default root to merge one more meshes. normally is bone that named as "Root".
			public Transform rootTransform
			{
				get => _rootTransform;
			}

			// Skeleton Data Entity that creates the skeleton.
			public RawSkeletonDataEntity skeletonDataEntity
			{
				get
				{
					if (_skeletonDataEntity == null && this.nativeHandle != System.IntPtr.Zero)
					{
						var nativeObj = pav_AvatarSkeleton_GetSkeletonDataEntity(this.nativeHandle);
						if (nativeObj != IntPtr.Zero)
						{
							_skeletonDataEntity = new RawSkeletonDataEntity(nativeObj, false);
							_skeletonDataEntity.Retain();
						}
					}

					return _skeletonDataEntity;
				}
			}

			#endregion


			#region Framework Methods

			[UnityEngine.Scripting.PreserveAttribute]
			internal AvatarSkeleton(System.IntPtr nativeHandle_)
			{
				// check out of memory exception.
				if (nativeHandle_ == System.IntPtr.Zero)
				{
					throw new System.OutOfMemoryException("IDParameterTable.New");
				}

				SetNativeHandle(nativeHandle_, false);
			}
			
            //@brief Destroy the object and release native reference count.
            //@note Derived class MUST invoke the method if override it.
            protected override void OnDestroy()
			{
				// Do Nothing.
				base.OnDestroy();

				if (_skeletonDataEntity != null)
				{
					_skeletonDataEntity.Release();
					_skeletonDataEntity = null;
				}

				// release native array memory.
				if (_boneNameHashes.IsCreated)
				{
					_boneNameHashes.Dispose();
				}

				if (_lastBoneXFroms.IsCreated)
				{
					_lastBoneXFroms.Dispose();
				}

				if (_boneXFroms.IsCreated)
				{
					_boneXFroms.Dispose();
				}

				if (_additiveSkeletons != null)
				{
					foreach (var x in _additiveSkeletons)
					{
						x.Value.Release();
					}

					_additiveSkeletons.Clear();
				}

				_rootTransform = null;
				//
				if (_boneTransformMap != null)
				{
					_boneTransformMap.Clear();
					_boneTransformMap = null;
				}

				if (_accessArray.isCreated)
				{
					_accessArray.Dispose();
				}
			}

			#endregion

			private struct TransformJob : IJobParallelForTransform
			{
				[ReadOnly] public NativeArray<XForm> boneXFroms;

				public void Execute(int index, TransformAccess transform)
				{
					unsafe
					{
						var xform = boneXFroms[index];
						transform.localScale = xform.scale;
						transform.localPosition = xform.position;
						transform.localRotation = xform.orientation;
					}
				}
			}

			#region Public Methods

			/// <summary>
			/// Create skeleton transform
			/// </summary>
			/// <param name="holdTrans">Root transform to hold the skeleton transforms</param>
			/// <param name="mainSkeleton">If not null, the skeleton is an additive skeleton to it</param>
			public void CreateTransforms(Transform holdTrans, AvatarSkeleton mainSkeleton = null)
			{
				if (_boneTransformMap != null)
				{
					return;
				}

				//
				_boneTransformMap = new Dictionary<int, Transform>();

				var skeletonData = this.skeletonDataEntity;
				var bones = skeletonData.bones;

				var boneCount = bones.Length;
				if (!_boneNameHashes.IsCreated)
				{
					_boneNameHashes = new NativeArray<int>((int)boneCount, Allocator.Persistent,
						NativeArrayOptions.UninitializedMemory);
				}

				if (!_lastBoneXFroms.IsCreated)
				{
					_lastBoneXFroms = new NativeArray<XForm>((int)boneCount, Allocator.Persistent,
						NativeArrayOptions.UninitializedMemory);
				}

				if (!_boneXFroms.IsCreated)
				{
					_boneXFroms = new NativeArray<XForm>((int)boneCount, Allocator.Persistent,
						NativeArrayOptions.UninitializedMemory);
					unsafe
					{
						_boneXFromsPtr = (System.IntPtr)_boneXFroms.GetUnsafePtr();
					}
				}

				_transforms = new Transform[boneCount];

				XForm defaultForm;
				defaultForm.orientation = Quaternion.identity;
				defaultForm.scale = Vector3.one;
				defaultForm.position = Vector3.zero;

				// find hold trans from main skeleton
				_mainSkeleton = mainSkeleton;

				if (mainSkeleton != null)
				{
					if (holdTrans == null && boneCount > 0)
					{
						holdTrans = mainSkeleton.GetTransform(bones[0].boneNameHash);
					}
				}

				if (holdTrans == null)
				{
					holdTrans = mainSkeleton.rootTransform;
				}

				for (int i = 0; i < boneCount; i++)
				{
					var bone = bones[i];

					// try get bone from main skeleton.
					if (mainSkeleton != null)
					{
						var mainBone = mainSkeleton.GetTransform(bone.boneNameHash);
						if (mainBone != null)
						{
							//_boneTransformMap.Add(bone.boneNameHash, mainBone);
							//_transforms[i] = mainBone;
							continue;
						}
					}
						var go = new GameObject(bone.boneName);
					var boneTrans = go.transform;
					//
					_boneNameHashes[i] = bone.boneNameHash;

					// create transform and add to _boneTransformMap
					{
						// if failed to find parent bone, add to root
						Transform parentTrans;
						if (_boneTransformMap.TryGetValue(bone.parentBoneNameHash, out parentTrans))
						{
							boneTrans.parent = parentTrans;
						}
						else if (mainSkeleton != null)
						{
							boneTrans.parent = mainSkeleton.GetTransform(bone.parentBoneNameHash);
						}
						else
						{
							boneTrans.parent = holdTrans;
						}

						// track root transform.
						if (_rootTransform == null)
						{
							_rootTransform = boneTrans;
							_rootTransformNameHash = (uint)bone.boneNameHash;
						}

						//
						_boneTransformMap.Add(bone.boneNameHash, boneTrans);
						_transforms[i] = boneTrans;
					}

					//
					boneTrans.localPosition = bone.position;
					boneTrans.localRotation = bone.rotation;
					boneTrans.localScale = bone.scale;

					_lastBoneXFroms[i] = defaultForm;
				}

				if (!_accessArray.isCreated)
				{
					_accessArray = new TransformAccessArray(_transforms, _transforms.Length);
				}

				unsafe
				{
					pav_AvatarSkeleton_GetLocalJointXForms(this.nativeHandle, (uint)_boneNameHashes.Length,
						(System.IntPtr)_boneNameHashes.GetUnsafePtr(), (System.IntPtr)_boneXFroms.GetUnsafePtr());
				}

				// create additive skeleton transforms
				CreateAdditiveSkeletonsTransforms();
			}

            /**
             * Check whether the skeleton is dirty and need be modified.
             */ 
            internal void CheckDirtyState()
            {
                // need remove transforms that maybe removed from native part.
                if(skeletonDataEntity.boneCount != skeletonDataEntity.bones.Length)
                {
                    var bones = skeletonDataEntity.GetBones(true);
                    var newBones = new HashSet<int>();
                    foreach(var x in bones)
                    {
                        newBones.Add(x.boneNameHash);
                    }

                    var bonesToRemove = new List<int>();
                    // remove not used transform.
                    foreach(var x in _boneTransformMap)
                    {
                        if(!newBones.Contains(x.Key))
                        {
                            bonesToRemove.Add(x.Key);
                            GameObject.Destroy(x.Value.gameObject);
                        }
                    }
                    // remove from _boneTransformMap
                    foreach (var x in bonesToRemove)
                    {
                        _boneTransformMap.Remove(x);
                    }

                    //TODO: need to create new transforms maybe added in native part?
                    if(_boneTransformMap.Count != bones.Length)
                    {
                        throw new Exception("To fix CheckDirtyState and add new transform operation!");
                    }

                    XForm defaultForm;
                    defaultForm.orientation = Quaternion.identity;
                    defaultForm.scale = Vector3.one;
                    defaultForm.position = Vector3.zero;

                    // rebuild structures
                    {
                        var boneCount = bones.Length;
                        _boneNameHashes.Dispose();
                        _lastBoneXFroms.Dispose();
                        _boneXFroms.Dispose();
                        _accessArray.Dispose();

                        
                        _boneNameHashes = new NativeArray<int>((int)boneCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                        _lastBoneXFroms = new NativeArray<XForm>((int)boneCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                        _boneXFroms = new NativeArray<XForm>((int)boneCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                        unsafe
                        {
                            _boneXFromsPtr = (System.IntPtr)_boneXFroms.GetUnsafePtr();
                        }

                        for (int i = 0; i < boneCount; i++)
                        {
                            var bone = bones[i];
                            if (!_boneTransformMap.TryGetValue(bone.boneNameHash, out Transform boneTrans))
                            {
                                _boneNameHashes[i] = (int)_rootTransformNameHash;
                                _transforms[i] = null;
                                continue;
                            }

                            //
                            _boneNameHashes[i] = bone.boneNameHash;
                            _transforms[i] = boneTrans;
                            //
                            boneTrans.localPosition = bone.position;
                            boneTrans.localRotation = bone.rotation;
                            boneTrans.localScale = bone.scale;
                            _lastBoneXFroms[i] = defaultForm;
                        }

                        // TransformAccessArray for _transforms
                        {
                            Array.Resize(ref _transforms, boneCount);
                            _accessArray = new TransformAccessArray(_transforms, _transforms.Length);
                        }

                        // get current value.
                        unsafe
                        {
                            pav_AvatarSkeleton_GetLocalJointXForms(this.nativeHandle, (uint)_boneNameHashes.Length, (System.IntPtr)_boneNameHashes.GetUnsafePtr(), _boneXFromsPtr);
                        }
                    }
                }
            }
			
            //@brief pre-update simulation render data. get xforms from c++ part.
            internal void PreUpdateSimulationRenderDataT()
			{
				// check whether has been destroyed.
				if (_boneTransformMap == null || this.nativeHandle == System.IntPtr.Zero ||
				    !enableUpdateFromNativeSkeleton || _boneXFromsPtr == System.IntPtr.Zero)
				{
					return;
				}

				unsafe
				{
					// get xform datas.
					_boneXFormsUpdated = NativeResult.Success == pav_AvatarSkeleton_GetLocalJointXForms(
						this.nativeHandle,
						(uint)_boneNameHashes.Length, (System.IntPtr)_boneNameHashes.GetUnsafePtr(), _boneXFromsPtr);
				}
			}
            
            //@brief post update simulation render data.
            //@note PreUpdateSimulationRenderDataT MUST be invoke before here.
            internal void SchedulePostUpdateSimulationRenderDataJobs(NativeArray<JobHandle> jobHandles,
				ref int jobIndex)
			{
				// check whether has been destroyed.
				if (_boneXFormsUpdated)
				{
					// TODO: update additive skeletons in work threads.
					if (_additiveSkeletons != null)
					{
						foreach (var x in _additiveSkeletons)
						{
							//TODO: only update additive joints and leave main skeleton joints to job.
							x.Value.UpdateUnityTransformsFromNative();
						}
					}

					var transformJob = new TransformJob()
					{
						boneXFroms = _boneXFroms,
					};
					jobHandles[jobIndex++] = transformJob.Schedule(_accessArray);
				}
			}

			/// <summary>
			/// Update unity transform from native transforms.
			/// </summary>
			public void UpdateUnityTransformsFromNative()
			{
				// check whether has been destroyed.
				if (_boneTransformMap == null || this.nativeHandle == System.IntPtr.Zero ||
				    !enableUpdateFromNativeSkeleton)
				{
					return;
				}

				UnityEngine.Profiling.Profiler.BeginSample("AvatarSkeleton.UpdateUnityTransformsFromNative");

				unsafe
				{
					UnityEngine.Profiling.Profiler.BeginSample("AvatarSkeleton.UpdateUnityTransformsFromNative Native");
					pav_AvatarSkeleton_GetLocalJointXForms(this.nativeHandle, (uint)_boneNameHashes.Length,
						(System.IntPtr)_boneNameHashes.GetUnsafePtr(), _boneXFromsPtr);
					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("AvatarSkeleton.UpdateUnityTransformsFromNative Unity");

					for (int i = 0; i < _boneNameHashes.Length; i++)
					{
						var trans = _transforms[i];
						if (trans == null)
						{
							continue;
						}

						var lastxform = _lastBoneXFroms[i];
						var xform = _boneXFroms[i];

						// FIXME: choose correct scaleErrorThreshold
						// bool updateScale = Vector3.SqrMagnitude(xform.scale - lastxform.scale) >= (PicoAvatarApp.instance.OptimizationSettings.scaleErrorThreshold) * (PicoAvatarApp.instance.OptimizationSettings.scaleErrorThreshold);
						bool updateScale = true;
						if (updateScale)
						{
							trans.localScale = xform.scale;
							lastxform.scale = xform.scale;
						}

						bool updatePosOrRot = (Vector3.SqrMagnitude(xform.position - lastxform.position) >=
						                       PicoAvatarApp.instance.optimizationSettings.positionErrorThreshold) ||
						                      (Quaternion.Dot(xform.orientation, lastxform.orientation) <
						                       (1.0 - PicoAvatarApp.instance.optimizationSettings
							                       .orientationErrorThreshold));
						if (updatePosOrRot)
						{
#if UNITY_2021_3_OR_NEWER || UNITY_2022_2_OR_NEWER
							trans.SetLocalPositionAndRotation(xform.position, xform.orientation);
#else
                            trans.localPosition = xform.position;
                            trans.localRotation = xform.orientation;
#endif
							lastxform.orientation = xform.orientation;
							lastxform.position = xform.position;
						}
					}

					UnityEngine.Profiling.Profiler.EndSample();
				}

				// TODO: update additive skeletons in work threads.
				if (_additiveSkeletons != null)
				{
					foreach (var x in _additiveSkeletons)
					{
						//TODO: only update additive joints and leave main skeleton joints to job.
						x.Value.UpdateUnityTransformsFromNative();
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();
			}


			/// <summary>
			/// Gets transform with bone name hash.
			/// </summary>
			/// <param name="boneNameHash">Native part name hash</param>
			/// <returns>Transform of target bone</returns>
			public Transform GetTransform(int boneNameHash)
			{
				Transform trans;
				if (_boneTransformMap != null && _boneTransformMap.TryGetValue(boneNameHash, out trans))
				{
					return trans;
				}

				// if not found in bone map, search in main skeleton.
				if (_mainSkeleton != null)
				{
					return _mainSkeleton.GetTransform(boneNameHash);
				}

				return null;
			}

			/// <summary>
			/// Clone skeleton instance
			/// </summary>
			/// <returns>Cloned object</returns>
			public AvatarSkeleton CloneAsRef()
			{
				NativeObject.RetainNative(this.nativeHandle);
				var clonedSkeleton = new AvatarSkeleton(this.nativeHandle);
				return clonedSkeleton;
			}

			/// <summary>
			/// Gets additive skeleton with native skeleton handle.
			/// </summary>
			/// <param name="skeletonHandle">Skeleton handle</param>
			/// <returns>Target AvatarSkeleton</returns>
			public AvatarSkeleton GetAdditiveSkeleton(System.IntPtr skeletonHandle)
			{
				if (skeletonHandle != System.IntPtr.Zero && _additiveSkeletons != null)
				{
					if (_additiveSkeletons.TryGetValue(skeletonHandle, out var skeleton))
					{
						return skeleton;
					}
				}

				return null;
			}

			/// <summary>
			/// Gets additive skeleton that contains the bone.
			/// </summary>
			/// <param name="boneNameHash"> Name hash of bone in native sdk.</param>
			/// <returns>Additive skeleton that contains the bone.</returns>            
			public AvatarSkeleton GetAdditiveSkeletonContainsTheJoint(uint boneNameHash)
			{
				if (_additiveSkeletons != null)
				{
					foreach (var x in _additiveSkeletons)
					{
						if (x.Value._boneTransformMap.ContainsKey((int)boneNameHash))
						{
							return x.Value;
						}
					}
				}

				return null;
			}

			#endregion


			#region Private Fields

			// skeleton data.
			private RawSkeletonDataEntity _skeletonDataEntity = null;

			// if additive skeleton, those shared bones in main skeleton are excluded.
			private Dictionary<int, Transform> _boneTransformMap = null;

			// bone name hashes. if additive skeleton, those shared bones in main skeleton are excluded.
			private NativeArray<int> _boneNameHashes;

			// xform cache.  if additive skeleton, those shared bones in main skeleton are excluded.
			private NativeArray<XForm> _lastBoneXFroms;
			private NativeArray<XForm> _boneXFroms;

			// HACK: pointer to _boneXFroms to avoid unity check for ReadOnly NativeArray in Job.
			private System.IntPtr _boneXFromsPtr;

			// whether _boneXFroms updated in job system. if not updated, no need to be synced to transforms.
			private bool _boneXFormsUpdated = false;

			// root transform.
			private Transform _rootTransform;

			// name hash for root transform
			private uint _rootTransformNameHash = 0;

			// main skeleton for the additive skeleton.
			AvatarSkeleton _mainSkeleton = null;

			// dictionary key is native handle of additive skeleton, value is unity avatar skeleton.
			private Dictionary<System.IntPtr, AvatarSkeleton> _additiveSkeletons = null;

			// transform list. if additive skeleton, those shared bones in main skeleton are excluded.
			private Transform[] _transforms;

			// for jobsystem updation.
			private TransformAccessArray _accessArray;

			#endregion


			#region Additive Skeletons

			// Create additive skeletons for assets like dynamic hair, swing.
			private void CreateAdditiveSkeletonsTransforms()
			{
				int skeletonCount = pav_AvatarSkeleton_GetAdditiveSkeletonCount(this.nativeHandle);
				if (skeletonCount > 0 && _additiveSkeletons == null)
				{
					_additiveSkeletons = new Dictionary<IntPtr, AvatarSkeleton>();
					unsafe
					{
						var skeletonHandles = new NativeArray<System.IntPtr>((int)skeletonCount, Allocator.Temp,
							NativeArrayOptions.UninitializedMemory);
						pav_AvatarSkeleton_GetAdditiveSkeletons(this.nativeHandle, (uint)skeletonCount,
							(System.IntPtr)skeletonHandles.GetUnsafePtr());

						for (int i = 0; i < skeletonCount; ++i)
						{
							AddAdditiveSkeletonTransforms(skeletonHandles[i]);
						}

						skeletonHandles.Dispose();
					}
				}
			}

			// Create additive skeleton an asset like dynamic hair, swing.
			private void AddAdditiveSkeletonTransforms(System.IntPtr skeletonHandle)
			{
				if (skeletonHandle == System.IntPtr.Zero)
				{
					return;
				}

				// if same skeleton has not been added, create the additive skeleton.
				if (!_additiveSkeletons.TryGetValue(skeletonHandle, out AvatarSkeleton skeleton))
				{
					var additiveSkeleton = new AvatarSkeleton(skeletonHandle);
					additiveSkeleton.Retain();
					_additiveSkeletons.Add(skeletonHandle, additiveSkeleton);
					//
					additiveSkeleton.CreateTransforms(null, this);
				}
			}

			// Invoked when avatar changed any assets, to check create transforms for new additive skeleton and remove transforms of not used skeleton .
			internal void PartialCreateAdditiveSkeletonTransforms()
			{
				int skeletonCount = pav_AvatarSkeleton_GetAdditiveSkeletonCount(this.nativeHandle);
				if (skeletonCount > 0)
				{
					if (_additiveSkeletons == null)
					{
						_additiveSkeletons = new Dictionary<IntPtr, AvatarSkeleton>();
					}

					unsafe
					{
						var skeletonHandles = new NativeArray<System.IntPtr>((int)skeletonCount, Allocator.Temp,
							NativeArrayOptions.UninitializedMemory);
						pav_AvatarSkeleton_GetAdditiveSkeletons(this.nativeHandle, (uint)skeletonCount,
							(System.IntPtr)skeletonHandles.GetUnsafePtr());

						HashSet<IntPtr> newSkeletonHandles = new HashSet<IntPtr>();

						// add new skeletons.
						for (int i = 0; i < skeletonCount; ++i)
						{
							var skeletonHandle = skeletonHandles[i];
							if (skeletonHandle != System.IntPtr.Zero)
							{
								newSkeletonHandles.Add(skeletonHandle);
								//
								if (_additiveSkeletons.ContainsKey(skeletonHandle))
								{
									NativeObject.ReleaseNative(ref skeletonHandle);
								}
								else
								{
									var additiveSkeleton = new AvatarSkeleton(skeletonHandle);
									additiveSkeleton.Retain();
									_additiveSkeletons.Add(skeletonHandle, additiveSkeleton);
									//
									additiveSkeleton.CreateTransforms(null, this);
								}
							}
						}

						// remove not used skeletons
						{
							var skeletonsToRemove = new List<System.IntPtr>();
							foreach (var x in _additiveSkeletons)
							{
								if (!newSkeletonHandles.Contains(x.Key))
								{
									skeletonsToRemove.Add(x.Key);
								}
							}

							//
							foreach (var x in skeletonsToRemove)
							{
								// remove additive transforms
								if (_additiveSkeletons.TryGetValue(x, out AvatarSkeleton skeleton))
								{
									// remove the skeleton.
									_additiveSkeletons.Remove(x);

									skeleton.RemoveAdditiveTransforms();

									skeleton.Release();
								}
							}
						}

						skeletonHandles.Dispose();
					}
				} // need destroy current additive skeletons
				else if (_additiveSkeletons != null)
				{
					foreach (var x in _additiveSkeletons)
					{
						x.Value.RemoveAdditiveTransforms();
						x.Value.Release();
					}

					_additiveSkeletons.Clear();
					_additiveSkeletons = null;
				}
			}

			// Remove additive transforms. when remove assets from the avatar, need remove additive transforms.
			private void RemoveAdditiveTransforms()
			{
				if (_mainSkeleton != null && _boneTransformMap != null)
				{
					foreach (var x in _boneTransformMap)
					{
						// if not in main skeleton, destroy the transform.
						if (_mainSkeleton.GetTransform(x.Key) == null)
						{
							GameObject.Destroy(x.Value.gameObject);
						}
					}

					_boneTransformMap.Clear();
				}
			}

			/// <summary>
			/// Notification from AvatarPrimitive when parent of the skeleton changed.
			/// </summary>
			internal void OnAdditiveSkeletonParentChanged()
			{
				if (_rootTransformNameHash != 0 && _mainSkeleton != null)
				{
					var localPos = _rootTransform.localPosition;
					var localScale = _rootTransform.localScale;
					var localRotation = _rootTransform.localRotation;

					var parentNameHash =
						pav_AvatarSkeleton_GetTransformParentNameHash(this.nativeHandle, _rootTransformNameHash);

					var trans = _mainSkeleton.GetTransform((int)parentNameHash);

					if (trans != null)
					{
						_rootTransform.parent = trans;
					}
					else
					{
						AvatarEnv.Log(DebugLogMask.GeneralError,
							String.Format("Failed find new parent transform in main skeleton. parentNameHash:{0}",
								parentNameHash));
					}
				}
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarSkeleton_GetSkeletonDataEntity(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarSkeleton_GetLocalJointXForms(System.IntPtr entityHandle,
				uint boneCount, System.IntPtr boneNameHashes, System.IntPtr xforms);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarSkeleton_GetWorldJointXForms(System.IntPtr entityHandle,
				uint boneCount, System.IntPtr boneNameHashes, System.IntPtr xforms);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarSkeleton_GetAdditiveSkeletonCount(System.IntPtr entityHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarSkeleton_GetAdditiveSkeletons(System.IntPtr entityHandle,
				uint skeletonCount, System.IntPtr skeletons);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarSkeleton_GetTransformParentNameHash(System.IntPtr entityHandle,
				uint boneNameHash);

			#endregion
		}
	}
}