using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#if !NO_XR
	using Pico.Platform;
#endif

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar loaded or updated delegate.
		/// </summary>
		public delegate void AvatarSpecificationUpdated(PicoAvatar avatar, int errorCode,
			string message);

		/// <summary>
		/// Notification that failed to load avatar.
		/// </summary>
		public delegate void AvatarLoadFailed(string userId, string avatarId, NativeResult errorCode, string errorDesc);

		/// <summary>
		/// The class manages avatars
		/// Sdk unified management module, providing major Avatar related interfaces. Including important load and destroy interfaces
		/// </summary>
		/// <remarks>
		/// The singleton instance MUST be created and maintained by PicoAvatarApp
		/// </remarks>
		public partial class PicoAvatarManager
		{
			#region Public Events

			/// <summary>
			/// Events that avatar loaded or updated
			/// </summary>
			/// <remarks>
			/// Only avatar spec data arrived, avatar grpahics data has not been ready!
			/// </remarks>
			public event AvatarSpecificationUpdated OnAvatarSpecificationUpdated;

			/// <summary>
			/// Events that failed to load avatar.
			/// </summary>
			public event AvatarLoadFailed OnAvatarLoadFailed;
			
			#endregion


			#region Public Properties

			/// <summary>
			/// Singleton instance.
			/// </summary>
			public static PicoAvatarManager instance
			{
				get => _instance;
			}

			/// <summary>
			/// Whether the initialization is completed, the image can be loaded after the initialization is completed
			/// </summary>
			public static bool isWorking
			{
				get { return _instance != null && _instance.isReady; }
			}

			/// <summary>
			/// Equivalent to isWorking，whether avatar sdk has been successfully logged in
			/// </summary>
			public bool isReady { get; private set; } = false;

			/// <summary>
			/// Whether the avatar image can be loaded,should wait and load avatar
			/// </summary>
			/// <remarks>
			/// The isWorking condition must be met, and if you PicoAvatarApp.enablePlaceHolder = true,
			/// You need to wait for the placeholder to load
			/// </remarks>

			public static bool canLoadAvatar
			{
				get
				{
					if (PicoAvatarApp.instance == null || !isWorking)
					{
						return false;
					}
					else if (PicoAvatarApp.instance.extraSettings.enableBuiltinAvatarPlaceHolder)
					{
						return _instance.builtinPlaceHolderOtherAvatar != null
						       && _instance.builtinPlaceHolderOtherAvatar.isAnyEntityReady;
					}

					return true;
				}
			}

			/// <summary>
			/// Avatar count added
			/// </summary>
			public int avatarCount
			{
				get => _avatarDict.Count;
			}

			/// <summary>
			/// Avatar camera.
			/// </summary>
			public PicoAvatarCamera avatarCamera { get; private set; } = new PicoAvatarCamera();

			/// <summary>
			/// Placeholder PicoAvatar for local avatar. Mesh/material/animation are shared among avatars that use the placeholder.
			/// </summary>
			public PicoPlaceholderAvatar builtinPlaceHolderLocalAvatar { get; private set; } = null;

			/// <summary>
			/// Placeholder PicoAvatar for none local avatar. Mesh/material/animation are shared among avatars that use the placeholder.
			/// </summary>
			public PicoPlaceholderAvatar builtinPlaceHolderOtherAvatar { get; private set; } = null;

			/// <summary>
			/// Main avatar which is created with AvatarCapabilities.Flags.MainAvatar is usually avatar of the local player.  Only one main avatar allowed.
			/// </summary>
			public PicoAvatar mainAvatar { get; private set; } = null;

			// Visible avatar entities this frame. updated from 
			internal List<AvatarEntity> avatarEntitiesToUpdateSimulationDataThisFrame { get; private set; } =
				new List<AvatarEntity>();

			#endregion


			#region Public Methods

			// Constructor that ONLY BE invoked from PicoAvatarApp.
			internal PicoAvatarManager()
			{
				if (_instance == null)
				{
					_instance = this;
				}
			}

			/// <summary>
			/// Sets whether enable weak network
			/// If enabled, when loaded avatar changed, maybe old avatar will be loaded and animation playback will fail in remote peers.
			/// It is developer's responsibility to notify all remote peers to unload/reload avatars when avatar changed
			/// PicoAvatar.avatarId can be used to test whether compatible avatar loaded
			/// </summary>
			/// <param name="isEnabled">If true, avatar meta / asset lod meta will also be cached and used when no network available</param>
			internal void SetEnableWeakNetworkMode(bool isEnabled)
			{
				if (this._rmiObject != null)
				{
					this._rmiObject.SetEnableWeakNetworkMode(isEnabled);
				}
			}

			/// <summary>
			/// Gets avatar with user id
			/// </summary>
			/// <param name="userId">Avatar userid</param>
			/// <returns>Return null if can not find the avatar</returns>
			public PicoAvatar GetAvatar(string userId)
			{
				if (_avatarDict.TryGetValue(userId, out PicoAvatar avatarBase))
				{
					if (avatarBase == null)
					{
						AvatarEnv.Log(DebugLogMask.GeneralError,
							"PicoAvatar CAN ONLY be DESTROYED by invoke UnloadAvatar!");
						_avatarDict.Remove(userId);
					}

					return avatarBase;
				}
				else
				{
					return null;
				}
			}

			/// <summary>
			/// Create a new PicoAvatar and initiate an asynchronous loading avatar image
			/// If the user Id in the parameter has been loaded, the PicoAvatar object currently created by the user will be returned
			/// Since the loading of Avatar is an asynchronous process, the current resources of the object returned by this interface are not loaded,
			/// and you need to wait for the subsequent process to complete the loading of Avatar
			/// </summary>
			/// <param name="loadContext">Avatar image configuration</param>
			/// <param name="responsed">Image data return callback</param>
			/// <returns>Return null if avatar with same user id</returns>
			public PicoAvatar LoadAvatar(AvatarLoadContext loadContext,
				Action<PicoAvatar, AvatarEntity> callback = null, string characterType = "", string characterVersion = "")
			{
				// If AvatarManager has not been initialized and do not allow load avatar from cache, just return.
				if (!isReady)
				{
					callback?.Invoke(null, null);
					return null;
				}

				// Check for main avatar.
				if (loadContext.capabilities.isMainAvatar)
				{
					// not allowed.
					if (mainAvatar != null)
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, "Only one main avatar allowed!");
						callback?.Invoke(null, null);
						return null;
					}
				}

				// Clamp max lod level.
				{
					if (loadContext.capabilities.maxLodLevel < PicoAvatarApp.instance.lodSettings.maxLodLevel)
					{
						loadContext.capabilities.maxLodLevel = PicoAvatarApp.instance.lodSettings.maxLodLevel;
					}

					if (loadContext.capabilities.forceLodLevel >= AvatarLodLevel.Lod0)
					{
						loadContext.capabilities.maxLodLevel = loadContext.capabilities.forceLodLevel;
					}
				}

				if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
				{
					AvatarEnv.Log(DebugLogMask.AvatarLoad,
						string.Format("Start LoadAvatar. userId:{0}", loadContext.userId));
				}

				// Use global animations.
				loadContext.capabilities.animationFlags = (uint)PicoAvatarApp.instance.animationSettings.animationFlags;

				// If the avatar with same user id exists, unload first.
				if (_avatarDict.TryGetValue(loadContext.userId, out var avatar) && avatar != null)
				{
					AvatarEnv.Log(DebugLogMask.GeneralWarn,
						string.Format("LoadAvatar will unload existing avatar. userId:{0}", loadContext.userId));
					//
					UnloadAvatar(loadContext.userId);
				}

				// Remove last one.
				{
					if (_avatarLoadContexts.ContainsKey(loadContext.userId))
					{
						_avatarLoadContexts.Remove(loadContext.userId);
					}

					// Cache capabilityes
					_avatarLoadContexts.Add(loadContext.userId, loadContext);
				}

				// Load request will create avatar be invoke 
				if (!loadContext.DoRequest(characterType, characterVersion))
				{
					callback?.Invoke(null, null);
					AvatarEnv.Log(DebugLogMask.GeneralError, "LoadAvatar failed for bad parameters! ");
					return null;
				}

				// Set capabilities.
				_avatarDict.TryGetValue(loadContext.userId, out avatar);
				//
				if (avatar == null)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						"LoadAvatar failed since js does not notify c# the to create new avatar! ");

					// Assure remove from js in case previous c# did not unload.
					if (this._rmiObject != null)
					{
						this._rmiObject.UnloadAvatar(loadContext.userId);

						//Try load again.
						{
							// Load request will create avatar be invoke 
							loadContext.DoRequest(characterType, characterVersion);
							// Set capabilities.
							_avatarDict.TryGetValue(loadContext.userId, out avatar);
						}
					}
				}

				// Sets main avatar.
				if (loadContext.capabilities.isMainAvatar)
				{
					this.mainAvatar = avatar;
				}

				if (avatar != null && callback != null)
				{
					avatar.AddFirstEntityReadyCallback(callback);
				}

				return avatar;
			}

			/// <summary>
			/// Remove a picoavatar
			/// </summary>
			/// <param name="avatar">picoavatar target</param>
			/// <returns>Is remove successed</returns>
			public bool UnloadAvatar(PicoAvatar avatar)
			{
				if (avatar == null)
					return true;
				//
				return UnloadAvatar(avatar.userId);
			}

			/// <summary>
			/// Remove an avatar with userid
			/// </summary>
			/// <param name="userId">Avatar userId</param>
			/// <returns>Is remove successed</returns>
			public bool UnloadAvatar(string userId)
			{
				// Try unload from lod context cache.
				_avatarLoadContexts.Remove(userId);

				// Destroy Avatar Entity
				PicoAvatar avatar;
				if (_avatarDict.TryGetValue(userId, out avatar))
				{
					_avatarDict.Remove(userId);
					//
					if (avatar != null)
					{
						UnloadTheAvatar(avatar);
					}

					return true;
				}
				else
				{
					AvatarEnv.Log(DebugLogMask.GeneralWarn, "Avatar with the id has been unloaded.");
					return false;
				}
			}

			/// <summary>
			/// Remove all avatars
			/// </summary>
			public void UnloadAllAvatars()
			{
				if (AvatarEnv.NeedLog(DebugLogMask.Framework))
				{
					AvatarEnv.Log(DebugLogMask.Framework, "Pico AvatarManager.UnloadAllAvatars Start.");
				}

				// Clear ready avatar lods.
				_readyAvatarLods.Clear();

				var avatarDict = _avatarDict;
				_avatarDict = new Dictionary<string, PicoAvatar>();
				//
				foreach (var x in avatarDict)
				{
					if (x.Value != null)
					{
						UnloadTheAvatar(x.Value);
					}
				}
			}

			/// <summary>
			/// Visitor of avatar entities
			/// </summary>
			/// <remarks>
			/// If visitor return false, stop loop
			/// </remarks>
			public delegate bool AvatarEntityVisitor(AvatarEntity avatarEntity);

			/// <summary>
			/// Loop all avatar entity by the given visitor
			/// </summary>
			/// <param name="visitor">Visitor to use</param>
			public void VisitAvatarEntities(AvatarEntityVisitor visitor)
			{
				foreach (var x in _avatarDict)
				{
					if (!visitor(x.Value.entity))
					{
						return;
					}
				}
			}

			/// <summary>
			/// Visitor of avatar
			/// </summary>
			/// <remarks>
			/// If visitor return false, stop loop
			/// </remarks>
			public delegate bool AvatarVisitor(PicoAvatar avatar);

			/// <summary>
			/// Loop all avatar by the given visitor
			/// </summary>
			/// <param name="visitor">Visitor to use</param>
			public void VisitAvatars(AvatarVisitor visitor)
			{
				foreach (var x in _avatarDict)
				{
					if (!visitor(x.Value))
					{
						return;
					}
				}
			}

			/// <summary>
			/// Force update lod of all the avatars
			/// </summary>
			/// <remarks>
			/// If avatar transfered to new place, can force update lod levels
			/// </remarks>
			public void ForceUpdateAvatarLodLevels()
			{
				// If force lod level, no need to update.
				if (PicoAvatarApp.instance.lodSettings.forceLodLevel == AvatarLodLevel.Invalid)
				{
					// Update camera first
					avatarCamera?.PreUpdateFrame();

					foreach (var x in _avatarDict)
					{
						x.Value.ForceUpdateLodLevels();
					}
				}
			}

			/// <summary>
			/// Sets whether skip avatar updation if invisible this frame
			/// </summary>
			/// <param name="skip">Skip flag</param>
			public void SetSkipUpdateWhenInvisibleThisFrame(bool skip)
			{
				pav_AvatarManager_SetSkipUpdateWhenInvisibleThisFrame(_nativeHandle, skip);
			}

			/// <summary>
			/// Add a new AvatarBunchGroup which used to holds a group of avatar bunches
			/// </summary>
			/// <returns>New AvatarBunchGroup</returns>
			public AvatarBunchGroup AddAvatarBunchGroup()
			{
				if (_avatarBunchGroups == null)
				{
					_avatarBunchGroups = new List<AvatarBunchGroup>();
				}

				//
				var bunchGroup = new AvatarBunchGroup();
				_avatarBunchGroups.Add(bunchGroup);
				return bunchGroup;
			}

			/// <summary>
			/// Remove avatar bunch group
			/// </summary>
			/// <param name="bunchGroup">AvatarBunchGroup to remove</param>
			public void RemoveAvatarBunchGroup(AvatarBunchGroup bunchGroup)
			{
				if (_avatarBunchGroups != null)
				{
					_avatarBunchGroups.Remove(bunchGroup);
					// Assure to destroy the group.
					bunchGroup.Destroy();
				}
			}

			/// <summary>
			/// Start avatar editor app
			/// </summary>
			/// <param name="packageName">application package name</param>
			/// <param name="callback"></param>
			/// <returns>true if start</returns>
			public void StartAvatarEditor(string packageName, Action<bool> callback)
			{
#if !NO_XR
				VerifyAppModeRequest.DoRequest(CoreService.GetAppID(), (errorCode, type) =>
				{
					if (errorCode != 0 || type == 3 || PicoAvatarApp.instance == null) //private mode do nothing
					{
						callback(false);
						return;
					}

				
					PicoAvatarApp.instance.appMode = type == 2 ? AppModeType.Single : AppModeType.Public;
#if UNITY_ANDROID && !UNITY_EDITOR
					try
					{
						SetAvatarEditorJavaObject();
						var context = _unityPlayerJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
						_avatarEditorJavaObject.Call<bool>("startAvatarEditor", context, packageName, "near", type);
						callback(true);
						Debug.Log("start avatar editor succeed!");
						return;
					}
					catch (System.Exception ex)
					{
						Debug.Log(ex.ToString());
						Debug.Log("start avatar editor failure!");
						callback(false);
					}
#else
					UnityEngine.Debug.LogError("StartAvatarEditor DOES NOT WORK in current platform!");
#endif
					Debug.Log("start avatar editor failure!");
					callback(false);
				});
#endif
			}

			/// <summary>
			/// Register avatar changed broadcast
			/// </summary>
			/// <param name="gameObjectName">The target gameobject that needs to listen</param>
			/// <param name="methodName">The method that needs to be executed</param>
			public void RegisterAvatarChangedReceiver(string gameObjectName, string methodName)
			{
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    SetAvatarEditorJavaObject();
                    var context = _unityPlayerJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
                    _avatarEditorJavaObject.Call("registerAvatarReceiver", context, gameObjectName, methodName);
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
#else
				UnityEngine.Debug.LogWarning("RegisterAvatarChangedReceiver DOES NOT WORK in current platform!");
#endif
			}

			/// <summary>
			/// Unregister avatar changed broadcast
			/// </summary>
			public void UnregisterAvatarChangedReceiver()
			{
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    SetAvatarEditorJavaObject();
                    var context = _unityPlayerJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
                    _avatarEditorJavaObject.Call("unregisterAvatarReceiver", context);
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
#else
				UnityEngine.Debug.LogError("UnregisterAvatarChangedReceiver DOES NOT WORK in current platform!");
#endif
			}

			/// <summary>
			/// Init avatar editor interaction object and context
			/// </summary>
			internal void SetAvatarEditorJavaObject()
			{
#if UNITY_ANDROID && !UNITY_EDITOR
                if (_avatarEditorJavaObject == null)
                {
                    _avatarEditorJavaObject = new AndroidJavaObject("com.pvr.avatarjar.AvatarEditorInteraction");
                }
                if (_unityPlayerJavaClass == null)
                {
                    _unityPlayerJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                }
#else
				UnityEngine.Debug.LogError("SetAvatarEditorJavaObject DOES NOT WORK in current platform!");
#endif
			}

			#endregion


			#region Private Fields

			// Singleton instance.
			private static PicoAvatarManager _instance;

			// Native handle.
			private IntPtr _nativeHandle = IntPtr.Zero;

			// Avatar entity dictionary. key is user id.  TODO: key should be changed to avatar id.
			private Dictionary<string, PicoAvatar> _avatarDict = new Dictionary<string, PicoAvatar>();

			// Current loading avatars
			private Dictionary<string, AvatarLoadContext> _avatarLoadContexts =
				new Dictionary<string, AvatarLoadContext>();

			// Remote object.
			private NativeCall_AvatarManager _rmiObject;

			// Ready avatar lod list.
			private List<AvatarLod> _readyAvatarLods = new List<AvatarLod>();

			// List of AvatarBunchGroup.
			private List<AvatarBunchGroup> _avatarBunchGroups = null;

			// Job handles.
			NativeArray<JobHandle> _jobHandles;

			// Each avatar bunch primitive occupy a job task.
			const int MaxAvatarBunchPrimitiveCount = 200;

			// Avatar editor interaction java object
			private AndroidJavaObject _avatarEditorJavaObject = null;

			// Avatar editor interaction context
			private AndroidJavaClass _unityPlayerJavaClass = null;

			// handle sendmessage
			private PicoAvatarManagerEventReceiver _eventReciever;

			#endregion


			#region Unity Framework Methods

			/// <summary>
			/// Update is called once per frame. Later will be removed. mainly update data of native part with unity objects
			/// </summary>
			/// <param name="gameTime"></param>
			internal void PreUpdateFrame(float gameTime)
			{
				if (isReady)
				{
					// Check update ready avatar lod.
					if (_readyAvatarLods.Count > 0)
					{
						for (int i = 0; i < _readyAvatarLods.Count; i++)
						{
							var avatarLod = _readyAvatarLods[i];
							if (avatarLod != null && !avatarLod.isDestroyed && avatarLod.owner != null)
							{
								avatarLod.owner.Notify_AvatarLodBuildFinished(avatarLod.lodLevel);
							}
						}

						_readyAvatarLods.Clear();
					}

					avatarCamera?.PreUpdateFrame();

					// Clear avatar entities visible this frame and rebuild here.
					if (avatarEntitiesToUpdateSimulationDataThisFrame.Capacity < _avatarDict.Count)
					{
						avatarEntitiesToUpdateSimulationDataThisFrame.Capacity = _avatarDict.Count + 10;
					}

					avatarEntitiesToUpdateSimulationDataThisFrame.Clear();

					// Update avatar entities before amaz update
					foreach (var x in _avatarDict)
					{
						// If the avatar has been destroyed outside the manager, just remove and skip the following updation this frame.
						if (x.Value == null)
						{
							AvatarEnv.Log(DebugLogMask.GeneralError,
								"PicoAvatar CAN ONLY be DESTRYOYED be by invoke UnloadAvatar!");
							_avatarDict.Remove(x.Key);
							break;
						}

						//
						if (x.Value.CheckNeedUpdateSimulationDataThisFrame() && x.Value.PreUpdateFrame(gameTime))
						{
							avatarEntitiesToUpdateSimulationDataThisFrame.Add(x.Value.entity);
							x.Value.forceUpdateSkeletonFromNative = false;
						}
					}
				}
			}

			/// <summary>
			/// Post-update this frame. mainly update data of unity object with native part after core calculation finished
			/// </summary>
			/// <param name="gameTime"></param>
			internal void PostUpdateFrame(float gameTime)
			{
				avatarCamera?.PostUpdateFrame();

				// Update avatar entities after amaz update
				{
					UnityEngine.Profiling.Profiler.BeginSample("AvatarManager.Avatar.PostUpdateFrame");
					foreach (var x in _avatarDict)
					{
						if (x.Value != null && x.Value.isVisible)
						{
							x.Value.PostUpdateFrame(gameTime);
						}
					}

					UnityEngine.Profiling.Profiler.EndSample();
				}

				var countOfEntitiesToUpdateRenderData = avatarEntitiesToUpdateSimulationDataThisFrame.Count;

				// Update simulation render data
				if (countOfEntitiesToUpdateRenderData > 0)
				{
					int jobIndex = 0;
					if (_jobHandles.Length < countOfEntitiesToUpdateRenderData)
					{
						if (_jobHandles.IsCreated)
							_jobHandles.Dispose();

						_jobHandles = new NativeArray<JobHandle>(
							Mathf.Max(MaxAvatarBunchPrimitiveCount, countOfEntitiesToUpdateRenderData),
							Allocator.Persistent,
							NativeArrayOptions.UninitializedMemory);
					}

					// Pre-update simulation render data
					{
						UnityEngine.Profiling.Profiler.BeginSample("C#.SchedulePreUpdateSimulationRenderDataJobs");

						for (int i = 0; i < countOfEntitiesToUpdateRenderData; ++i)
						{
							avatarEntitiesToUpdateSimulationDataThisFrame[i]
								.SchedulePreUpdateSimulationRenderDataJobs(i, _jobHandles, ref jobIndex);
						}

						// Combine jobs and complete all
						JobHandle.CombineDependencies(_jobHandles.GetSubArray(0, countOfEntitiesToUpdateRenderData))
							.Complete();

						UnityEngine.Profiling.Profiler.EndSample();
					}


					//UnityEngine.Profiling.Profiler.BeginSample("AvatarManager.Native.UpdateAvatarSimulationRenderDatas");
					//Pav_AvatarManager_UpdateAvatarSimulationRenderDatas(_nativeHandle);
					//UnityEngine.Profiling.Profiler.EndSample();
					{
						UnityEngine.Profiling.Profiler.BeginSample("C#.UpdateSimulationRenderData");

						for (int i = 0; i < countOfEntitiesToUpdateRenderData; ++i)
						{
							avatarEntitiesToUpdateSimulationDataThisFrame[i].UpdateSimulationRenderData(gameTime);
						}

						UnityEngine.Profiling.Profiler.EndSample();
					}

					{
						UnityEngine.Profiling.Profiler.BeginSample("C#.SchedulePreUpdateSimulationRenderDataJobs");

						// Reset job index
						jobIndex = 0;
						for (int i = 0; i < countOfEntitiesToUpdateRenderData; ++i)
						{
							avatarEntitiesToUpdateSimulationDataThisFrame[i]
								.SchedulePostUpdateSimulationRenderDataJobs(i, _jobHandles, ref jobIndex);
						}

						if (jobIndex > 0)
						{
							var jobHandle = JobHandle.CombineDependencies(_jobHandles.GetSubArray(0, jobIndex));
							jobHandle.Complete();
						}

						UnityEngine.Profiling.Profiler.EndSample();
					}
				}

				// Update avatar bunch groups.
				if (_avatarBunchGroups != null)
				{
					if (_jobHandles.Length < MaxAvatarBunchPrimitiveCount)
					{
						if (_jobHandles.IsCreated)
							_jobHandles.Dispose();
						_jobHandles = new NativeArray<JobHandle>(MaxAvatarBunchPrimitiveCount, Allocator.Persistent,
							NativeArrayOptions.UninitializedMemory);
					}

					{
						UnityEngine.Profiling.Profiler.BeginSample("AvatarBunch.SchedulePreUpdateRenderData");

						// Reset job index
						int jobIndex = 0;
						foreach (var x in _avatarBunchGroups)
						{
							x.SchedulePreUpdateRenderData(_jobHandles, ref jobIndex);
						}

						if (jobIndex > 0)
						{
							var jobHandle = JobHandle.CombineDependencies(_jobHandles.GetSubArray(0, jobIndex));
							jobHandle.Complete();
						}

						// Clear job context.
						AvatarBunch.ClearUpdateRenderDataJobContext();

						UnityEngine.Profiling.Profiler.EndSample();
					}

					{
						UnityEngine.Profiling.Profiler.BeginSample("AvatarBunch.UpdateRenderDataForAvatarBunches");

						foreach (var x in _avatarBunchGroups)
						{
							x.UpdateRenderData();
						}

						UnityEngine.Profiling.Profiler.EndSample();
					}
				}
			}

			/// <summary>
			/// Network synchronize simulation
			/// <remarks>
			///     Queue the sync request. actual synchronization will be done during PicoAvatarApp.Update(...)
			/// </remarks>
			/// </summary>
			/// <param name="timeStamp">Current time to record packet. seconds with precision of millisecond</param>
			public void SyncNetSimulation(double timeStamp)
			{
				pav_AvatarManager_SyncNetSimulation(_nativeHandle, timeStamp);
			}

			/// <summary>
			/// Initialize the avatr system. asynchronously log in avatar service
			/// </summary>
			/// <remarks>
			/// Invoked by PicoAvatarApp
			/// </remarks>>
			/// <param name="avatarAppId"></param>
			/// <param name="avatarAppToken"></param>
			/// <param name="userToken"></param>
			/// <param name="startGameTime"></param>
			/// <param name="serverType"></param>
			/// <param name="accessType"></param>
			/// <param name="nationType"></param>
			/// <param name="configString"></param>
			/// <param name="sceneData"></param>
			internal void Initialize(string userToken = "", double startGameTime = 0,
				ServerType serverType = ServerType.ProductionEnv, AccessType accessType = AccessType.ThirdApp,
				string nationType = "", string configString = "", System.IntPtr sceneData = default(System.IntPtr))
			{
				// Register delegates
				if (_nativeHandle != IntPtr.Zero)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						"AvatrManager initialization failed. nativeHandle is null.");
					return;
				}

				//
				if (_rmiObject != null)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "Duplicated invoke to initialize.");
					return;
				}

				// Add stats.
				if (PicoAvatarStats.instance != null)
				{
					PicoAvatarStats.instance.AvatarManagerLoginStart();
				}

				// Create native invoke proxy first.
				if (_rmiObject == null)
				{
					_rmiObject = new NativeCall_AvatarManager(this, 0);
					_rmiObject.Retain();
				}

				// Initialize AvatarPreloadAssetPackageSet
				AvatarPreloadAssetPackageSet.intance.Initialize();

				{
					_nativeHandle = pav_AvatarManager_New();
#if UNITY_EDITOR
					if (_nativeHandle == System.IntPtr.Zero)
					{
						UnityEditor.EditorUtility.DisplayDialog("",
							"Please restart Unity, AvatarMananger not uninitialized at previous stop.", "OK");
					}
#endif
					// Add developer real appid
					if (string.IsNullOrEmpty(configString))
					{
#if !NO_XR
						string appId = Pico.Platform.CoreService.GetAppID();
						AppConfigData configData = new AppConfigData();
						configData.PicoDevelopAppId = appId;
						configString = JsonUtility.ToJson(configData);
						
						AvatarEnv.Log(DebugLogMask.GeneralInfo, "configString= " + configString);
						PicoAvatarApp.instance.extraSettings.configString = configString;
#endif
					}
					//
					if (pav_AvatarManager_Initialize(_nativeHandle, "", "", userToken,
						    startGameTime, serverType, accessType, nationType, configString, sceneData) !=
					    NativeResult.Success)
					{
						// Notify that initialization failed .
						PicoAvatarApp.instance.Notify_AvatarManagerInitialized(false);
					}
				}
			}

			/// <summary>
			/// Invoked by script end when successfully initialized
			/// </summary>
			/// <param name="success"></param>
			internal void OnInitialized(bool success)
			{
				if (success)
				{
					if (isReady)
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, "AvatarManager has already been initialized");
					}

					isReady = true;

					CreateAvatarManagerEventReceiver();

					// pass extra parameters to native part.
					{
						Dictionary<string, object> configParams = new Dictionary<string, object>();

						if (PicoAvatarApp.instance.renderSettings.materialConfiguration != null)
						{
							configParams["renderPipeline"] = (int)PicoAvatarApp.instance.renderSettings
								.materialConfiguration.renderPipelineType;
						}


#if UNITY_EDITOR_64
						configParams["runInEditor"] = true;
#else
                        configParams["runInEditor"] = false;
#endif

						configParams["disablePrimitiveMerge"] =
							(bool)PicoAvatarApp.instance.renderSettings.disablePrimitiveMerge;

						string paramJsonText = JsonConvert.SerializeObject(configParams);
						PostInitialize(AppSettings.avatarSdkVersion, paramJsonText);
					}


					//
					avatarCamera?.Initialize();

					//
					PicoAvatarLodManager.Initialize();

					//
					if (AvatarEnv.NeedLog(DebugLogMask.Framework))
					{
						AvatarEnv.Log(DebugLogMask.Framework, "PicoAvatarManager Initialized.");
					}

					//
					if (PicoAvatarApp.instance.extraSettings.enableBuiltinAvatarPlaceHolder)
					{
						// User or local avatar that ik enabled.
						var capability = new AvatarCapabilities();
						capability.manifestationType = AvatarManifestationType.Full;
						capability.bodyCulling = false;
						capability.enablePlaceHolder = false;
						capability.maxLodLevel = AvatarLodLevel.Count - 1;
						capability.controlSourceType = ControlSourceType.MainPlayer;
						capability.usage = AvatarCapabilities.Usage.AsPlaceHolder;
						capability.forceLodLevel = AvatarLodLevel.Lod0;
						capability.headShowType = AvatarHeadShowType.Hide;
						//
						if (PicoAvatarApp.instance.GetNationType().Equals("cn"))
							builtinPlaceHolderLocalAvatar =
								this.LoadAvatar(AvatarLoadContext.CreateByJsonConfig(AvatarSpecConfig.userIdPlaceHolder,
									AvatarSpecConfig.specJsonStrPlaceHolder, capability)) as PicoPlaceholderAvatar;
						else
							builtinPlaceHolderLocalAvatar =
								this.LoadAvatar(AvatarLoadContext.CreateByJsonConfig(AvatarSpecConfig.userIdPlaceHolder,
										AvatarSpecConfig.specJsonStrPlaceHolderOverSea, capability)) as
									PicoPlaceholderAvatar;
						builtinPlaceHolderLocalAvatar.transform.localPosition = Vector3.zero;
						builtinPlaceHolderLocalAvatar.transform.localScale = Vector3.zero;

						// Other avatar
						var capabilityOther = new AvatarCapabilities();
						capabilityOther.manifestationType = AvatarManifestationType.Full;
						capabilityOther.bodyCulling = false;
						capabilityOther.enablePlaceHolder = false;
						capabilityOther.maxLodLevel = AvatarLodLevel.Count - 1;
						capabilityOther.controlSourceType = ControlSourceType.OtherPlayer;
						capability.usage = AvatarCapabilities.Usage.AsPlaceHolder;
						capability.forceLodLevel = AvatarLodLevel.Lod0;
						capability.headShowType = AvatarHeadShowType.Normal;
						//
						if (PicoAvatarApp.instance.GetNationType().Equals("cn"))
							builtinPlaceHolderOtherAvatar =
								this.LoadAvatar(AvatarLoadContext.CreateByJsonConfig(
									AvatarSpecConfig.userIdPlaceHolderOther, AvatarSpecConfig.specJsonStrPlaceHolder,
									capabilityOther)) as PicoPlaceholderAvatar;
						else
							builtinPlaceHolderOtherAvatar =
								this.LoadAvatar(AvatarLoadContext.CreateByJsonConfig(
										AvatarSpecConfig.userIdPlaceHolderOther,
										AvatarSpecConfig.specJsonStrPlaceHolderOverSea, capabilityOther)) as
									PicoPlaceholderAvatar;
						builtinPlaceHolderOtherAvatar.transform.localPosition = Vector3.zero;
						builtinPlaceHolderOtherAvatar.transform.localScale = Vector3.zero;
					}

					// Add stats.
					if (PicoAvatarStats.instance != null)
					{
						PicoAvatarStats.instance.AvatarManagerLoginFinished();
					}
				}
				else
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "Pico AvatarManager failed to initialize.");
				}

				//
				PicoAvatarApp.instance.Notify_AvatarManagerInitialized(success);
			}

			/// <summary>
			/// Invoked from PicoAvatarApp to uninitialize avatar manager
			/// </summary>
			internal void Unitialize()
			{
				//
				if (AvatarEnv.NeedLog(DebugLogMask.Framework))
				{
					AvatarEnv.Log(DebugLogMask.Framework, "Pico AvatarManager Unitialize Start.");
				}

				//
				if (_nativeHandle == System.IntPtr.Zero)
				{
					return;
				}

				//
				UnloadAllAvatars();
				DestroyAvatarManagerEventReceiver();

				// dispose job handles
				if (_jobHandles.IsCreated)
				{
					_jobHandles.Dispose();
				}

				//
				PicoAvatarLodManager.Unitialize();

				if (_avatarBunchGroups != null)
				{
					foreach (var x in _avatarBunchGroups)
					{
						x.Destroy();
					}

					_avatarBunchGroups = null;
				}

				//
				avatarCamera?.Destroy();

				// Un-initialize AvatarPreloadAssetPackageSet
				AvatarPreloadAssetPackageSet.intance.Uninitialize();

				if (_rmiObject != null)
				{
					_rmiObject.Uninitialize();
					_rmiObject.Release();
					_rmiObject = null;
				}

				//
				if (_nativeHandle != System.IntPtr.Zero)
				{
					pav_AvatarManager_Shutdown(_nativeHandle);
					//
					NativeObject.ReleaseNative(ref _nativeHandle);
				}

				if (_jobHandles.IsCreated)
				{
					_jobHandles.Dispose();
				}

				//
				_instance = null;
			}

			/// <summary>
			/// Update light env
			/// </summary>
			/// <param name="lightEnv"></param>
			internal void OnAvatarSceneLightEnvChanged(PicoAvatarSceneLightEnv lightEnv)
			{
				if (_avatarDict != null)
				{
					//
					foreach (var x in _avatarDict)
					{
						if (x.Value != null)
						{
							x.Value.entity?.Notify_AvatarSceneLightEnvChanged(lightEnv);
						}
					}
				}
			}

			#endregion


			#region Private Methods

			/// <summary>
			/// Disable primitive merge
			/// </summary>
			/// <param name="version"></param>
			/// <param name="paramsJsonText"></param>
			private void PostInitialize(string version, string paramsJsonText)
			{
				if (_rmiObject != null)
				{
					_rmiObject.PostInitialize(version, paramsJsonText);
				}
			}

			/// <summary>
			/// Destroy avatar
			/// </summary>
			/// <param name="avatarBase"></param>
			private void UnloadTheAvatar(PicoAvatar avatarBase)
			{
				if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
				{
					AvatarEnv.Log(DebugLogMask.AvatarLoad,
						string.Format("UnloadTheAvatar. userId:{0}", avatarBase.userId));
				}

				// Clear avatar manager.
				avatarBase.SetAvatarManager(null);

				// Clear place holder avatar.
				if (avatarBase == builtinPlaceHolderLocalAvatar)
				{
					builtinPlaceHolderLocalAvatar = null;
				}

				if (avatarBase == builtinPlaceHolderOtherAvatar)
				{
					builtinPlaceHolderOtherAvatar = null;
				}

				//
				if (avatarBase == mainAvatar)
				{
					this.mainAvatar = null;
				}

				// Clear camera tracking avatar.
				if (avatarCamera != null && avatarBase == avatarCamera.trakingAvatar)
				{
					avatarCamera.trakingAvatar = null;
				}

				// Remove from _readyAvatarLods
				if (_readyAvatarLods.Count > 0)
				{
					for (int i = 0; i < _readyAvatarLods.Count;)
					{
						var avatarLod = _readyAvatarLods[i];
						if (avatarLod != null && avatarLod.owner != null && avatarLod.owner.owner == avatarBase)
						{
							_readyAvatarLods.RemoveAt(i);
						}
						else
						{
							++i;
						}
					}
				}

				// Destroy c# object first.
				avatarBase.Destroy();

				//
				if (this._rmiObject != null)
				{
					this._rmiObject.UnloadAvatar(avatarBase.userId.ToString());
				}
				else
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "Wrong, UnloadTheAvatar MUST before delete rmiObject!");
				}

				//
				UnityEngine.GameObject.Destroy(avatarBase.gameObject);
			}


			/// <summary>
			/// Only avatar spec data arrived, avatar graphics data has not been ready!
			/// </summary>
			/// <param name="requestId"></param>
			/// <param name="userId"></param>
			/// <param name="errorCode"></param>
			/// <param name="msg"></param>
			internal void ProcessAvatarLoadRequest(string userId, int errorCode, string msg)
			{
				var avatarBase = GetAvatar(userId);
				if (avatarBase != null)
				{
					if (errorCode == 0)
					{
						//这里是加载资产完成的回调
						OnAvatarSpecificationUpdated?.Invoke(avatarBase, (int)errorCode, msg);
					}
					else
					{
						//TODO: error code.
						OnAvatarSpecificationUpdated?.Invoke(avatarBase, (int)errorCode, msg);
					}
				}
			}

			/// <summary>
			/// Avatar message arrived
			/// </summary>
			/// <param name="messageType"></param>
			/// <param name="content"></param>
			internal void Notify_Message(uint messageType, string content)
			{
				// Not implemented yet.
			}

			/// <summary>
			/// Invoked from native part when an avatar instance created
			/// </summary>
			/// <param name="userId"></param>
			/// <param name="avatarId"></param>
			/// <param name="nativeAvatarId"></param>
			internal void OnAttachNativeAvatar(string userId, string avatarId, uint nativeAvatarId)
			{
				// Has been removed.
				if (!_avatarLoadContexts.TryGetValue(userId, out AvatarLoadContext loadCtx))
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						"Wrong, OnAttachNativeAvatar find exiting avatar load context!");
					return;
				}

				if (!_avatarDict.TryGetValue(userId, out var avatar))
				{
					var go = new GameObject(string.Format("PicoAvatar{0}", userId));
					//
					if (PicoAvatarApp.instance.extraSettings.avatarSceneLayer > 0)
					{
						go.layer = (int)PicoAvatarApp.instance.extraSettings.avatarSceneLayer;
					}

					if (loadCtx.capabilities.isAsPlaceholder)
					{
						// Force disable enablePlaceHolder for a place holder avatar.
						loadCtx.capabilities.enablePlaceHolder = false;

						avatar = go.AddComponent<Pico.Avatar.PicoPlaceholderAvatar>();

						if (userId == AvatarSpecConfig.userIdPlaceHolder)
						{
							builtinPlaceHolderLocalAvatar = (PicoPlaceholderAvatar)avatar;
							builtinPlaceHolderLocalAvatar.SetForLocalAvatar(true);
						}
						else if (userId == AvatarSpecConfig.userIdPlaceHolderOther)
						{
							builtinPlaceHolderOtherAvatar = (PicoPlaceholderAvatar)avatar;
							builtinPlaceHolderOtherAvatar.SetForLocalAvatar(false);
						}
					}
					else
					{
						avatar = go.AddComponent<Pico.Avatar.PicoAvatar>();
					}

					// Set avatar manager.
					avatar.SetAvatarManager(this);
					//
					avatar.Notify_AttachNativeAvatar(avatarId, nativeAvatarId);
					//
					avatar.Notify_Initialized(userId, avatarId, loadCtx);
					//
					_avatarDict[userId] = avatar;
				}
				else
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "Duplicated Avatar detected!!!");
				}
			}

			/// <summary>
			/// Notification from native layer
			/// </summary>
			/// <param name="userId"></param>
			/// <param name="avatarId"></param>
			/// <param name="errorCode"></param>
			/// <param name="errorDesc"></param>
			internal void Notify_AvatarLoadFailed(string userId, string avatarId, NativeResult errorCode,
				string errorDesc)
			{
				if (OnAvatarLoadFailed != null)
				{
					OnAvatarLoadFailed(userId, avatarId, errorCode, errorDesc);
				}
				else
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, string.Format(
						"OnAvatarLoadFailed. userId:{0} avatarId:{1} reason:{2} desc:{3}",
						userId, avatarId, errorCode, errorDesc));
				}
			}

			/// <summary>
			/// Notification from PicoAvatarApp that application paused/resumed
			/// </summary>
			/// <param name="paused"></param>
			internal void Notify_ApplicationPaused(bool paused)
			{
				if (this._rmiObject != null)
				{
					this._rmiObject.OnApplicationPause(paused);
				}
			}

			/// <summary>
			/// When an avatar lod loaded, should queue it here and wait to be
			/// set actived after ready callback processed
			/// </summary>
			/// <param name="avatarLod"></param>
			internal void QueueReadyAvatarLod(AvatarLod avatarLod)
			{
				_readyAvatarLods.Add(avatarLod);
			}

			private void CreateAvatarManagerEventReceiver()
			{
				if (PicoAvatarApp.instance == null)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "Create AvatarManagerEventReceiver failed");
					return;
				}

				DestroyAvatarManagerEventReceiver();

				// used for receive notification that avatar changed with inter-process editor
				GameObject go = new GameObject("AvatarManagerEventReceiver");
				go.transform.parent = PicoAvatarApp.instance.transform;
				_eventReciever = go.AddComponent<PicoAvatarManagerEventReceiver>();
				RegisterAvatarChangedReceiver("AvatarManagerEventReceiver", "OnRecvSystemAvatarChange");
			}

			private void DestroyAvatarManagerEventReceiver()
			{
				if (_eventReciever != null)
					GameObject.DestroyImmediate(_eventReciever);

				_eventReciever = null;
			}
			
			private class AppConfigData
			{
				public string channel;
				public string PicoDevelopAppId;
			}

			/// <summary>
			/// Monitor status changes, such as Avatar switching on the AvatarEditor side, saving after editing, etc.
			/// </summary>
			/// <param name="callback">when avatar change, do callback</param>
			public void AddAvatarChangeListener(Action<string> callback)
			{
				if (_eventReciever == null || callback == null)
					return;
				_eventReciever.OnRecvSystemAvatarChangeHandler += callback;
			}
			/// <summary>
			/// Cancel monitoring of Avatar changes on the AvatarEditor side
			/// </summary>
			/// <param name="callback">when avatar change, do callback</param>
			public void RemoveAvatarChangeListener(Action<string> callback)
			{
				if (_eventReciever == null || callback == null)
					return;
				_eventReciever.OnRecvSystemAvatarChangeHandler -= callback;
			}
			
			#endregion
		}
	}
}