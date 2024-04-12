using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using Pico.Platform;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Notification that AvatarManager is initialized.
		/// </summary>
		public delegate void AvatarManagerInitialized(bool success);

		/// <summary>
		/// Notification that AvatarManager is stopping.
		/// </summary>
		public delegate void AvatarManagerStopping();

		/// <summary>
		/// Manage the configuration and life cycle of AvatarSDK.
		/// It inherits from MonoBehaviour and is not destroyed in the scene. At the same time, PicoAvatarApp provides many SDK global settings
		/// </summary>
		/// <remarks>
		/// Top level framework class of the sdk
		/// Initialize native sdk
		/// Implements main loop pipeline
		/// Global configurations(static and runtime) are here
		/// </remarks>
		public partial class PicoAvatarApp : MonoBehaviour
		{
			#region types definition

			// Avatar sdk application stages.
			enum Stage
			{
				None,
				PreparingFirmware,
				PreInitialize,
				Running,
				PreUninitialize,
				Uninitialized,
			}

			// Bit flags for AvatarAppConfig.flags
			private enum AvatarAppConfigFlags
			{
				JsRuntimeDebugMode = 1 << 1,
				TTNetInjectMode = 1 << 2,
				AnimationIntermitentMode = 1 << 3,
			}

			/// <summary>
			/// Configurations to initialize basic avatar environment.
			/// </summary>
			private struct AvatarAppConfig
			{
				public uint version;
				public uint flags; // bits combination of AvatarAppConfigFlags
				public string avatarPath;
				public string cachePath;
			}

			#endregion


			#region Public Fields

			/// <summary>
			/// App related settings
			/// </summary>
			public AppSettings appSettings;

			/// <summary>
			/// Login related settings
			/// </summary>
			public LoginSettings loginSettings;

			/// <summary>
			/// Log related settings
			/// </summary>
			public LogSettings logSettings;

			/// <summary>
			/// Sdk avatar render module related Settings
			/// </summary>
			public AvatarRenderSettings renderSettings;

			/// <summary>
			/// Lod related settings
			/// </summary>
			public AvatarLodSettings lodSettings;

			/// <summary>
			/// Animation related settings
			/// </summary>
			public AvatarAnimationSettings animationSettings;

			/// <summary>
			/// Optimization module related Settings
			/// </summary>
			public AvatarOptimizationSettings optimizationSettings;

			/// <summary>
			/// Some stand-alone settings
			/// note:not open in 281 version
			/// </summary>
			[HideInInspector]
			public AvatarExtraSettings extraSettings;

			/// <summary>
			/// NetPlayback module related Settings
			/// </summary>
			public AvatarNetBodyPlaybackSettings netBodyPlaybackSettings;

			/// <summary>
			/// NetPlayback module related Settings
			/// </summary>
			public AvatarFaceExpressionNetPlaybackSettings netFaceExpressionPlaybackSettings;

			/// <summary>
			/// LocalDebug module related Settings
			/// </summary>
#if !PAV_INTERN
			[HideInInspector]
#endif 
			public AvatarLocalDebugSettings localDebugSettings;

			/// <summary>
			/// Set the account system, normal use without setting
			/// </summary>
			/// <remarks>
			/// One application accesses the user image through the encrypted uid , and the third-party application
			/// obtains the current user through the platform.
			/// </remarks>
			[HideInInspector] public AccessType accessType = AccessType.ThirdApp;

			/// <summary>
			/// Notification that AvatarManager is initialized.
			/// </summary>
			public event AvatarManagerInitialized OnAvatarManagerInitialized;

			/// <summary>
			/// Notification when AvatarManager is stopping
			/// </summary>
			public event AvatarManagerStopping OnAvatarManagerStopping;

			// Last game time used to update avatars
			internal float lastUpdateTime
			{
				get => _lastUpdateTime;
			}

			#endregion


			#region Public Properties

			/// <summary>
			/// Singleton of the class object
			/// </summary>
			public static PicoAvatarApp instance
			{
				get => _instance;
			}

			/// <summary>
			/// Whether avatar app is working. Both editor mode and play mode, it should work.
			/// </summary>
			public static bool isWorking
			{
				get => _instance != null && _instance._curStage == Stage.Running;
			}

			// Whether avatar engine is working. Both editor mode and play mode, it should work.
			internal static bool isNativeEngineReady
			{
				get => _instance != null && _instance._curStage != Stage.None &&
				       _instance._curStage != Stage.Uninitialized;
			}

			// Current detected main camera used to calcuate lod, cull animation ... updated each frame.
			internal Camera currentCullingCamera { get; private set; } = null;

			/// <summary>
			/// Main camera frustum planes.
			/// </summary>
			public Plane[] frustumPlanes
			{
				get => _frustumPlanes;
			}


#if PAV_INTERNAL_DEV
			// Avatar tracer.
			public AvatarTracer avatarTracer
			{
				get => _avatarTracer;
			}
#endif

			// Sets/gets avatar scene light env
			public PicoAvatarSceneLightEnv curAvatarSceneLightEnv { get; private set; }

			#endregion


			#region Framework Methods

			void Awake()
			{
				_playingMode = Application.isPlaying;
			}

			// Use this for initialization
			void Start()
			{
				// Do not destroy when load new scene.
				if (dontDestroyOnLoad)
					GameObject.DontDestroyOnLoad(this.gameObject);

#if UNITY_EDITOR
				// Currently if editor mode
				if (!Application.isPlaying)
				{
					// Check default configuration.
					if (renderSettings.materialConfiguration == null)
					{
						// Builtin
						if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null)
						{
							renderSettings.materialConfiguration =
								UnityEditor.AssetDatabase.LoadAssetAtPath<PicoMaterialConfiguration>(
									"Packages/org.byted.avatar.sdk/Settings/BuiltInMaterialConfiguration.asset");
						}
						else // Srp.
						{
							renderSettings.materialConfiguration =
								UnityEditor.AssetDatabase.LoadAssetAtPath<PicoMaterialConfiguration>(
									"Packages/org.byted.avatar.sdk/Settings/URPMaterialConfiguration.asset");
						}

						UnityEditor.EditorUtility.SetDirty(this);
					}

					return;
				}
#endif
				// Set global coroutine holder as this.
				CoroutineExecutor.Start(this);

				if (_instance == null)
				{
					_instance = this;
					//
					this.StartCoroutine(Coroutine_Initialize());
				}
				else if (_instance != this)
				{
					UnityEngine.Debug.LogWarning("Only one PicoAvatarSDK instance enabled.");
				}
			}

			/// <summary>
			/// Main frame update. Usually invoked from Unity framework.
			/// at several case, may need manually invoke update.
			/// </summary>
			void Update()
			{
#if UNITY_EDITOR
				// Currently if editor mode
				if (!Application.isPlaying)
				{
					UpdateNecessaryFieldsInEditorMode();
				}
#endif

				if (_instance == null)
				{
					return;
				}


				// Update cached values.
				try
				{
					// update cached values.
					{
						squaredSkipFaceExpressionPlaybackDist = optimizationSettings.skipFaceExpressionPlaybackDist *
						                                        optimizationSettings.skipFaceExpressionPlaybackDist;
						squaredSkipUpdateEvenVisibleDistance = optimizationSettings.skipUpdateEvenVisibleDistance *
						                                       optimizationSettings.skipUpdateEvenVisibleDistance;
						squaredPositionErrorThreshold = optimizationSettings.positionErrorThreshold *
						                                optimizationSettings.positionErrorThreshold;
					}

					// Scale time.
					var delta = (Time.time - _lastUpdateTime) * _timeScale;
					_lastUpdateTime += delta;

					//
					{
						currentCullingCamera = appSettings.mainCamera ? appSettings.mainCamera : Camera.main;
						if (currentCullingCamera != null)
						{
							_frustumPlanes = GeometryUtility.CalculateFrustumPlanes(currentCullingCamera);
						}
					}

					//
					if (Utility.EnableSDKUpdate)
					{
						if (_nativeHandle != System.IntPtr.Zero)
						{
							//
							if (PicoAvatarStats.instance != null)
							{
								PicoAvatarStats.instance.EmitStart(PicoAvatarStats.StatsType.AvatarTotalUpdate);
							}


							UnityEngine.Profiling.Profiler.BeginSample("NativeCallMarshal.Update");

							if (NativeCallMarshal.IsInitialized())
								NativeCallMarshal.Update();

							UnityEngine.Profiling.Profiler.EndSample();


							UnityEngine.Profiling.Profiler.BeginSample("AvatarManager.PreUpdateFrame");

							{
								if (_avatarManager != null)
								{
									if (PicoAvatarStats.instance != null)
										PicoAvatarStats.instance.EmitStart(PicoAvatarStats.StatsType.AvatarPreUpdate);
									_avatarManager.PreUpdateFrame(_lastUpdateTime);
									if (PicoAvatarStats.instance != null)
										PicoAvatarStats.instance.EmitFinish(PicoAvatarStats.StatsType.AvatarPreUpdate);
								}
							}


							UnityEngine.Profiling.Profiler.EndSample();
							//
							{
								if (PicoAvatarStats.instance != null)
									PicoAvatarStats.instance.EmitStart(PicoAvatarStats.StatsType.AvatarCoreUpdate);

								UnityEngine.Profiling.Profiler.BeginSample("pav_AvatarApp_Update");
								pav_AvatarApp_Update(_nativeHandle, _lastUpdateTime);
								UnityEngine.Profiling.Profiler.EndSample();

								if (PicoAvatarStats.instance != null)
									PicoAvatarStats.instance.EmitFinish(PicoAvatarStats.StatsType.AvatarCoreUpdate);
							}


							UnityEngine.Profiling.Profiler.BeginSample("AvatarManager.PostUpdateFrame");
							if (_avatarManager != null)
							{
								if (PicoAvatarStats.instance != null)
									PicoAvatarStats.instance.EmitStart(PicoAvatarStats.StatsType.AvatarRenderUpdate);

								_avatarManager.PostUpdateFrame(_lastUpdateTime);

								if (PicoAvatarStats.instance != null)
									PicoAvatarStats.instance.EmitFinish(PicoAvatarStats.StatsType.AvatarRenderUpdate);
							}

							UnityEngine.Profiling.Profiler.EndSample();

							//
							if (PicoAvatarStats.instance != null)
							{
								PicoAvatarStats.instance.EmitFinish(PicoAvatarStats.StatsType.AvatarTotalUpdate);
							}
						}
					}

					// Auto garbage collection.
					if (appSettings.autoGarbageCollection)
					{
						if (Time.time - _lastGCTime > timeSpanToGC)
						{
							/*// pre gc.
							{
							    var stats = AvatarEnv.GetAvatarStats();
							    UnityEngine.Debug.Log(stats);
							    //
							    if (PicoAvatarStats.instance != null)
							    {
							        PicoAvatarStats.instance.LogStats();
							    }
							}*/

							_lastGCTime = Time.time;
							this.GarbageCollection(
#if PAV_INTERNAL_DEV
								11
#endif
							);

							/*// after gc.
							{
							    var stats = AvatarEnv.GetAvatarStats();
							    UnityEngine.Debug.Log(stats);
							    //
							    if (PicoAvatarStats.instance != null)
							    {
							        PicoAvatarStats.instance.LogStats();
							    }
							}*/
						}
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogError(string.Format("PicoAvatarApp.Update wrong. message:{0} stack:{1}", ex.Message,
						ex.StackTrace));
				}
			}

			/// <summary>
			/// Unity late update chance to do some check.
			/// </summary>
			private void LateUpdate()
			{
				// do some finishe work
				AvatarEnv.OnFrameEnd();
			}


			private void OnApplicationQuit()
			{
				UnityEngine.Debug.Log("pav: PicoAvatarApp.OnApplicationQuit");
				Uninitialize();
			}

			private void OnApplicationPause(bool pause)
			{
				if (pause)
				{
					//
					UnityEngine.Debug.Log("pav: PicoAvatarApp.OnApplicationPause true");
				}
				else
				{
					UnityEngine.Debug.Log("pav: PicoAvatarApp.OnApplicationPause false");
				}

				//
				if (_avatarManager != null)
				{
					_avatarManager.Notify_ApplicationPaused(pause);
				}
			}

			// For stop to play
			private void OnDestroy()
			{
				UnityEngine.Debug.Log("pav: PicoAvatarApp.OnDestroy");
				Uninitialize();
			}

			#endregion


			#region Public Methods

			public PicoAvatarApp()
			{
				/*netBodyPlaybackSettings.SetDefault();
				netFaceExpressionPlaybackSettings.SetDefault();
				lodSettings.SetDefault();
				optimizationSettings.SetDefault();
				loginSettings.SetDefault();
				extraSettings.SetDefault();
				appSettings.SetDefault();
				logSettings.SetDefault();
				localDebugSettings.SetDefault();
				animationSettings.SetDefault();*/
			}

			/// <summary>
			/// Start avatar application manually
			/// </summary>
			/// <remarks>
			/// Normal application need not invoke the method.
			/// Usually PicoAvatarApp will initialize when Start() invoked, but sometime applcation may manally stop
			/// avatar module without destroy PicoAvatarApp object and restart avatar module later. e.g. In Editor mode
			/// may need manually start/stop application.
			/// </remarks>
			public void StartApp()
			{
				// Check stage 
				if (_curStage != Stage.None && _curStage != Stage.Uninitialized)
				{
					return;
				}

				if (_instance == null)
				{
					_instance = this;
					//
					_lastUpdateTime = Time.time;
					StartCoroutine(Coroutine_Initialize());
				}
				else if (_instance != this)
				{
					UnityEngine.Debug.LogWarning("Only one PicoAvatarSDK instance enabled.");
				}
			}

			/// <summary>
			/// Stop avatar application
			/// </summary>
			/// <remarks>
			/// Normal application need not invoke the method
			/// </remarks>
			public void StopApp()
			{
				Uninitialize();

				// Clear instance
				if (_instance == this)
				{
					_instance = null;
				}
			}

			/// <summary>
			/// Start Launch AvatarSDK
			/// </summary>
			/// <remarks>
			/// Initialize the SDK according to the set parameters.
			/// If the parameters are wrong or the network is abnormal, the initialization lecture will fail
			/// </remarks>
			public void StartAvatarManager()
			{
				// Currently stop/start in sync mode.
				/*if (_playingMode && isWorking && _avatarManager != null)
				{
				    StopAvatarManager();// bugfix by jmf, maybe not init completely before stop
				}*/

				// Add log.
				if (AvatarEnv.NeedLog(DebugLogMask.Framework))
				{
					AvatarEnv.Log(DebugLogMask.Framework, "PicoAvatarApp.StartAvatarManager");
				}

				//
				if (_playingMode && isWorking
				                 && _avatarManager == null)
				{
					_avatarManager = new PicoAvatarManager();
					// Debug in Development Env.
					bool thirdPackage = !System.IO.Directory.Exists(AvatarEnv.avatarPath);
					if (AvatarEnv.avatarPackedPathFirst || thirdPackage)
					{
						MemoryView jsData = null;
						byte[] matrixJS = DllLoaderHelper.GetMatrixJSBytes(!appSettings.localMode);
						if (matrixJS != null)
						{
							jsData = new MemoryView(matrixJS, false);
						}
						else
						{
							jsData = new MemoryView(Resources.Load<TextAsset>("AvatarSDKScript").bytes, false);
						}

						if (jsData != null)
						{
							_avatarManager.Initialize(loginSettings.accessToken, Time.time, appSettings.serverType,
								accessType,
								GetNationType(), extraSettings.configString, jsData.nativeHandle);
						}
						else
						{
							Debug.LogError("Packed AvatarSDKScript load failed.");
						}
					}
					else
					{
						_avatarManager.Initialize(loginSettings.accessToken, Time.time, appSettings.serverType,
							accessType,
							GetNationType(), extraSettings.configString);
					}

					// Skip updation if invisible this frame.
					_avatarManager.SetSkipUpdateWhenInvisibleThisFrame(this.optimizationSettings
						.skipUpdateIfInvisibleThisFrame);
				}
			}

			internal string GetNationType()
			{
				return IsCnDevice() ? "cn": "sg";
			}

			internal bool IsCnDevice()
			{
#if UNITY_EDITOR || UNITY_STANDALONE
				return true;
#else
				return ApplicationService.GetSystemInfo().IsCnDevice;
#endif
			}

			/// <summary>
			/// Invoked by AvatarManager to notify
			/// </summary>
			/// <param name="success"></param>
			internal void Notify_AvatarManagerInitialized(bool success)
			{
				// Sets whether enable weak network mode.
				if (optimizationSettings.enableWeakNetworkMode)
				{
					_avatarManager.SetEnableWeakNetworkMode(optimizationSettings.enableWeakNetworkMode);
				}

				// Send notification that avatar manager is stopping, listener may need clear some object.
				this.OnAvatarManagerInitialized?.Invoke(success);
				//
				if (success && !string.IsNullOrEmpty(localDebugSettings.debugConfigText))
				{
					var unescapedJsonText = localDebugSettings.debugConfigText.Replace("\\\"", "\"");
                    _rmiObject.SetDebugConfig(unescapedJsonText);
				}
			}

			/// <summary>
			/// Stop AvatarSDK
			/// </summary>
			public void StopAvatarManager()
			{
				// Add log.
				if (AvatarEnv.NeedLog(DebugLogMask.Framework))
				{
					AvatarEnv.Log(DebugLogMask.Framework, "PicoAvatarApp.StopAvatarManager start...");
				}

				// Send notification that avatar manager is stopping, listener may need clear some object.
				this.OnAvatarManagerStopping?.Invoke();
				//
				if (_playingMode && _avatarManager != null)
				{
					_avatarManager.Unitialize();
					_avatarManager = null;
				}
			}

            /// <summary>
            /// Sets lod range ratio, used to control lod switch range.
            /// Different scene need different ratio, generally is 1.0
            /// </summary>
            /// <param name="lod0ScreenPercentage">lod0 percentage</param>
            /// <param name="lod1ScreenPercentage">lod1 percentage</param>
            /// <param name="lod2ScreenPercentage">lod2 percentage</param>
            /// <param name="lod3ScreenPercentage">lod3 percentage</param>
            public void SetLodScreenPercentages(float lod0ScreenPercentage, float lod1ScreenPercentage
                , float lod2ScreenPercentage, float lod3ScreenPercentage, float lod4ScreenPercentage)
            {
                this.lodSettings.lod0ScreenPercentage = lod0ScreenPercentage;
                this.lodSettings.lod1ScreenPercentage = lod1ScreenPercentage;
                this.lodSettings.lod2ScreenPercentage = lod2ScreenPercentage;
                this.lodSettings.lod3ScreenPercentage = lod3ScreenPercentage;
                this.lodSettings.lod4ScreenPercentage = lod4ScreenPercentage;
                //
                if (PicoAvatarLodManager.instance != null)
                {
                    PicoAvatarLodManager.instance.SetLodScreenPercentages(lod0ScreenPercentage, 
                        lod1ScreenPercentage, 
                        lod2ScreenPercentage, 
                        lod3ScreenPercentage,
                        lod4ScreenPercentage);
                }
            }
			/// <summary>
			/// EditorModel:Sets burden level
			/// </summary>
			/// <param name="burdenLevel">larger level means more heavy burden</param>
			public void SetBurdenLevel(uint burdenLevel)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetBurden(burdenLevel);
				}
				//Caller_System_BurdenChanged.Invoke(burdenLevel);
			}

			/// <summary>
			/// Sets force lod level
			/// </summary>
			/// <param name="forceLodLevel">if is AvatarLodLevel.Invalid do not force lod level</param>
			public void SetForceLodLevel(AvatarLodLevel forceLodLevel)
			{
				lodSettings.forceLodLevel = forceLodLevel;
				//
				if (PicoAvatarLodManager.instance != null)
				{
					PicoAvatarLodManager.instance.SetForceAndMaxLodLevel(forceLodLevel, lodSettings.maxLodLevel);
				}
			}

			/// <summary>
			/// Sets max lod level
			/// </summary>
			/// <param name="maxLodLevel">if is AvatarLodLevel.Invalid do not force lod level</param>
			public void SetMaxLodLevel(AvatarLodLevel maxLodLevel)
			{
				lodSettings.maxLodLevel = maxLodLevel;
				//
				if (PicoAvatarLodManager.instance != null)
				{
					PicoAvatarLodManager.instance.SetForceAndMaxLodLevel(lodSettings.forceLodLevel,
						lodSettings.maxLodLevel);
				}
			}


			/// <summary>
			/// Sets additive gi amounts to raise base luminance for avatars in dark scene
			/// </summary>
			/// <param name="additiveGI_"></param>
			private void SetAdditiveGI(float additiveGI_)
			{
				appSettings.additiveGI = additiveGI_;

				// Change for avatar materials.
				{
					//TODO:
				}
			}

			/// <summary>
			/// Initialize avatar manager
			/// </summary>
			private IEnumerator Coroutine_Initialize()
			{
				// Test stage.
				if (_curStage != Stage.None && _curStage != Stage.Uninitialized)
				{
					yield break;
				}

				DllLoaderHelper.InitDllLoader(!appSettings.localMode);

				// Wait debugger when need debug c# on vr device.
				if (false)
				{
					int i = 0;
					while (i++ < 5)
					{
						yield return new WaitForSeconds(1.0f);
					}
				}

				var Path = "false";
				if (appSettings.localResourcePath != "")
				{
					Path = appSettings.localResourcePath;
				}

				//
				{
					// Log version
					IntPtr versionChar = pav_AvatarApp_GetVersion();
					appSettings.avatarCoreVersion = Marshal.PtrToStringAnsi(versionChar);

					// Output plugin and native sdk version.
					UnityEngine.Debug.Log(string.Format(
						"pav: PicoAvatarSdk Starting. avatarSdkVersion:{0}  avatarCoreVersion:{1}",
						AppSettings.avatarSdkVersion + "-" + AppSettings.unityPluginCommit,
						appSettings.avatarCoreVersion));

					// Initialize avatar env.
					var result = AvatarEnv.Initialize(AppSettings.avatarSdkVersion, (uint)logSettings.debugLogMasks,
						Path);
					//
					if (result != NativeResult.Success)
					{
						UnityEngine.Debug.Log(string.Format("pav: Failed to initialize Pico Avatar. result: {0}",
							result.ToString()));
						yield break;
					}

					// Initialize avatar environment first.
					if (AvatarEnv.NeedLog(DebugLogMask.Framework))
					{
						AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp AvatarEnv Initialize");
					}
				}


				// Whether enable stats.
				// Debug.LogError($"setCXXObjectStatsEnabled = {_enableStats}");
				AvatarEnv.SetCXXObjectStatsEnabled(localDebugSettings.enableStats);
				if (localDebugSettings.enableStats)
				{
					PicoAvatarStats.instance = new PicoAvatarStats();
				}

				//
				if (AvatarEnv.NeedLog(DebugLogMask.Framework))
				{
					AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp Start Initialize");
				}

				//
				_lastUpdateTime = Time.time;

				// Whether enable log time.
				SetEnableLogTime(logSettings.enableLogTime);

				// Whether print system infos
				if (localDebugSettings.enableSystemInfo)
				{
					Pico.Avatar.Utility.PrintSystemInfos();
				}

				//
				if (_nativeHandle == System.IntPtr.Zero)
				{
					_nativeHandle = pav_AvatarApp_New(true);
				}

				//
				_curStage = Stage.PreInitialize;


				// Add stats.
				if (PicoAvatarStats.instance != null)
				{
					PicoAvatarStats.instance.AppBootStart();
				}

				// Set config information.
				{
					if (AvatarEnv.NeedLog(DebugLogMask.Framework))
					{
						AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp Native SetConfig");
					}

#if AVATARSDK_TTNET_INJECTMODE && !UNITY_EDITOR
                    bool ttnInjectMode = true;
#else
					bool ttnInjectMode = false;
					Pico.Avatar.TTNetUtility.InitTTNet();
#endif
					AvatarAppConfig config = new AvatarAppConfig();
					config.version = 0;
					config.avatarPath = AvatarEnv.avatarPath;

					// use application cache first.
					String avatarCachePath = AvatarEnv.avatarCachePath;
					if (!string.IsNullOrEmpty(Utility.desiredAvatarCacheDirectory))
					{
						if (System.IO.Path.IsPathRooted(Utility.desiredAvatarCacheDirectory))
						{
							avatarCachePath = Utility.desiredAvatarCacheDirectory;
						}
						else
						{
							avatarCachePath = Application.persistentDataPath + "/" +
							                  Utility.desiredAvatarCacheDirectory;
						}

						try
						{
							System.IO.Directory.CreateDirectory(avatarCachePath);
						}
						catch (System.Exception ex)
						{
							this.StopApp();

							AvatarEnv.Log(DebugLogMask.GeneralError,
								"Failed to create avatar cahe directory: " + avatarCachePath);
						}
					}

					config.cachePath = avatarCachePath;
					config.flags = 0;
					if (ttnInjectMode)
					{
						config.flags |= (uint)AvatarAppConfigFlags.TTNetInjectMode;
					}

					if (optimizationSettings.intermitentMode)
					{
						config.flags |= (uint)AvatarAppConfigFlags.AnimationIntermitentMode;
					}
#if PAV_INTERNAL_DEV
					// set JsRuntimeMode to debug
					config.flags |= (uint)AvatarAppConfigFlags.JsRuntimeDebugMode;
#endif
					if (AvatarEnv.avatarPackedPathFirst || !System.IO.Directory.Exists(AvatarEnv.avatarPath))
					{
						config.avatarPath = AvatarEnv.avatarPackedPath;
					}

					// Log avatar boot package path and avatar cache path.
					if (AvatarEnv.NeedLog(DebugLogMask.Framework))
					{
						var sb = new System.Text.StringBuilder();
						sb.Append("avatarPath: ");
						sb.AppendLine(config.avatarPath);
						sb.Append("cachePath: ");
						sb.AppendLine(config.cachePath);
						AvatarEnv.Log(DebugLogMask.Framework, sb.ToString());
					}

					pav_AvatarApp_SetConfig(_nativeHandle, ref config);
				}

				// Pre initialize.
				if (AvatarEnv.NeedLog(DebugLogMask.Framework))
				{
					AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp Native PreInitialize");
				}

				pav_AvatarApp_PreInitialize(_nativeHandle, Time.time);

				//
				if (_playingMode)
				{
					if (AvatarEnv.NeedLog(DebugLogMask.Framework))
					{
						AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp NativeCallMarshal Initialize");
					}

					// initialize native invoke marshal
					NativeCallMarshal.Initialize();

					//
					_rmiObject = new NativeCall_AvatarApp(this, 0);
					_rmiObject.Retain();
				}

				//
				if (AvatarEnv.NeedLog(DebugLogMask.Framework))
				{
					AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp NativeCallMarshal PostInitialize");
				}

				pav_AvatarApp_PostInitialize(_nativeHandle);

				//
				if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null)
				{
					Postprocess.PicoColorTemperature.CheckColorTemperature();
				}

				// Add to stats
				if (PicoAvatarStats.instance != null)
				{
					PicoAvatarStats.instance.AppBootFinished();
				}

				// Working.
				_curStage = Stage.Running;

				// Automatically start avatar manager with configurations in PicoAvatarApp.
				/*if (this.appSettings.autoStartAvatarManager)
				{
				    yield return null; //leave one frame to init app param
				    if (string.IsNullOrEmpty(loginSettings.accessToken))
				    {
				        UnityWebRequest webRequest = UnityWebRequest.Get(NetEnvHelper.GetFullRequestUrl(NetEnvHelper.SampleTokenApi));
				        webRequest.timeout = 10; // 设置超时时间为10秒
				        webRequest.SetRequestHeader("Content-Type", "application/json"); 
				        yield return webRequest.SendWebRequest();
				        if (webRequest.result != UnityWebRequest.Result.Success)
				        {
				            Debug.Log("Get Token Failed! Reason:" + webRequest.error);
				            yield break;
				        }
				        string responseText = webRequest.downloadHandler.text;
				        var token = JsonConvert.DeserializeObject<JObject>(responseText)?.Value<string>("data");
				        loginSettings.accessToken = token;
				        StartAvatarManager();
				    }
				    else
				    {
				        StartAvatarManager();
				    }
			   
				}*/
			}

			/// <summary>
			/// Uninitialize the sdk. 
			/// </summary>
			public void Uninitialize()
			{
				if (_curStage == Stage.None || _curStage == Stage.Uninitialized)
				{
					return;
				}

				try
				{
					_curStage = Stage.PreUninitialize;

					//
					if (AvatarEnv.NeedLog(DebugLogMask.Framework))
					{
						AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp NativeCallMarshal PreUninitialize");
					}

					//
					pav_AvatarApp_PreUninitialize(_nativeHandle);

					// Uninitialize avatar manager.
					StopAvatarManager();

#if PAV_INTERNAL_DEV
					// destroy avatar tracer.
					if (_avatarTracer != null)
					{
						_avatarTracer.Destroy();
						_avatarTracer = null;
					}
#endif
					// Notify script to shutdown.
					if (_rmiObject != null)
					{
						if (AvatarEnv.NeedLog(DebugLogMask.Framework))
						{
							AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp RmiObject Shutdown");
						}

						_rmiObject.Shutdown(0);
					}

					//
					if (_rmiObject != null)
					{
						if (AvatarEnv.NeedLog(DebugLogMask.Framework))
						{
							AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp RmiObject Destroy");
						}

						_rmiObject.Release();
						_rmiObject = null;
					}

					//
					if (_playingMode)
					{
						if (AvatarEnv.NeedLog(DebugLogMask.Framework))
						{
							AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp NativeCallMarshal Unitialize");
						}

						NativeCallMarshal.Unitialize();
					}


					//
					if (AvatarEnv.NeedLog(DebugLogMask.Framework))
					{
						AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp NativeCallMarshal PostUninitialize");
					}

					pav_AvatarApp_PostUninitialize(_nativeHandle);


					//
					if (AvatarEnv.NeedLog(DebugLogMask.Framework))
					{
						AvatarEnv.Log(DebugLogMask.Framework, "AvatarApp AvatarEnv UnInitialize");
					}

					AvatarEnv.UnInitialize();
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e.Message);
				}

				//
				NativeObject.ReleaseNative(ref _nativeHandle);

				// Set uninitiliazed stage.
				_curStage = Stage.Uninitialized;
			}

			// Update net stats.
			public void UpdateNetStats()
			{
				if (_rmiObject != null)
				{
					_rmiObject.GetNetStats();
				}
			}

			/// <summary>
			/// Sets debug log masks. bits from DebugLogMask
			/// </summary>
			/// <param name="logMasks"></param>
			private void SetDebugLogMasks(uint logMasks)
			{
				//
				logSettings.debugLogMasks = (DebugLogMask)logMasks;

				// Set debug log masks.
				AvatarEnv.SetDebugLogMasks((uint)logSettings.debugLogMasks);

				if (_rmiObject != null)
				{
					_rmiObject.SetDebugLogMasks((uint)logSettings.debugLogMasks);
				}
			}

			private void SetTraceNativeCaller(bool trace)
			{
				//
				_traceNativeCaller = trace;

				if (_rmiObject != null)
				{
					_rmiObject.SetTraceNativeCaller(trace);
				}
			}

			/// <summary>
			/// Sets whether enable add time to log item
			/// </summary>
			/// <param name="enableLogTime"></param>
			private void SetEnableLogTime(bool enableLogTime)
			{
				logSettings.enableLogTime = enableLogTime;
				AvatarEnv.SetEnableLogTime(enableLogTime);
			}

			/// <summary>
			/// Sets avatar light env
			/// </summary>
			/// <param name="curAvatarSceneLightEnv_">Light env to use</param>
			public void SetAvatarSceneLightEnv(PicoAvatarSceneLightEnv curAvatarSceneLightEnv_)
			{
				curAvatarSceneLightEnv = curAvatarSceneLightEnv_;
				//
				if (_avatarManager != null && curAvatarSceneLightEnv_ != null)
				{
					_avatarManager.OnAvatarSceneLightEnvChanged(curAvatarSceneLightEnv);
				}
			}

			/// <summary>
			/// Do garbage collection .it may stall the frame for hundreds of millseconds.
			/// Application layer MUST NOT invoke the method each frame
			/// </summary>
			/// <param name="gcLevel">gcLevel a level from 0 to 10. larger and try to collect more memory</param>
			public void GarbageCollection(uint gcLevel = 0)
			{
				if (_rmiObject != null)
				{
					_rmiObject.GarbageCollection(gcLevel);
				}

				//Resources.UnloadUnusedAssets();
				//Force 4 generation gc.
				//System.GC.Collect(4);

				//If larger than 10, output log.
				if (gcLevel >= 11)
				{
					if (PicoAvatarStats.instance != null)
					{
						// C# part instances stats.
						PicoAvatarStats.instance.LogStats();
						// Log referenced object.
						ReferencedObject.LogStats();


						// Output C++ part stats
						{
							var stats = AvatarEnv.GetAvatarStats().Split('\n');

							// Use unity log to avoid avatar log performance burden.
							foreach (var stat in stats)
							{
								UnityEngine.Debug.Log(stat);
							}
						}
					}
				}
			}
#if PAV_INTERNAL_DEV
			public new void SendMessage(string msg)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SendMessage(msg);
				}
			}


			/// <summary>
			/// Used by test framework to trace avatar logic points
			/// </summary>
			/// <param name="enableTracer"></param>
			public void SetEnableAvatarTracer(bool enableTracer)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetEnableAvatarTracer(enableTracer);
					//
					if (enableTracer && _avatarTracer == null)
					{
						_avatarTracer = new AvatarTracer();
					}
				}
			}
#endif

			#endregion

			#region Cache Frame Values

			// OptimizationSettings.skipFaceExpressionPlaybackDist * OptimizationSettings.skipFaceExpressionPlaybackDist;
			internal float squaredSkipFaceExpressionPlaybackDist { get; private set; } = 0.0f;

			// OptimizationSettings.skipUpdateEvenVisibleDistance * OptimizationSettings.skipUpdateEvenVisibleDistance;
			internal float squaredSkipUpdateEvenVisibleDistance { get; private set; } = 0.0f;

			// OptimizationSettings.positionErrorThreshold * OptimizationSettings.positionErrorThreshold;
			internal float squaredPositionErrorThreshold { get; private set; } = 0.0f;

			#endregion

			#region Private Fields

			// Singleton instance.
			private static PicoAvatarApp _instance;

			// Destroy or not.
			private bool dontDestroyOnLoad = true;

			// Whether is in playing mode.
			private bool _playingMode = false;

			// Current stage.
			private Stage _curStage = Stage.None;

			// NativeCall proxy object. a kind of RMI object.
			private NativeCall_AvatarApp _rmiObject;

			// Native handle for PicoAvatarApp object.
			private System.IntPtr _nativeHandle;

			// Avatar manager.
			private PicoAvatarManager _avatarManager;

			// Scene light env.
			private PicoAvatarSceneLightEnv _avatarSceneLightEnv;

			//
			private float _lastUpdateTime = 0.0f;
			private float _timeScale = 1.0f;

			// Frustum planes.
			private Plane[] _frustumPlanes;

			// Trace native caller.
			//[SerializeField, HideInInspector]
			private bool _traceNativeCaller = false;

#if PAV_INTERNAL_DEV
			// Avatar tracer.
			private AvatarTracer _avatarTracer = null;
#endif
			// Auto gc.
			private float _lastGCTime = 0.0f;
			private float timeSpanToGC = 15.0f;

			#endregion


			#region Debug Pause

#if (UNITY_EDITOR || PAV_SHOW_FPS) && DEBUG
#if PAV_SHOW_FPS
            private int fps = 0;
            private int displayFPS = 0;
#endif
			private bool[] keyDowns = new bool[1024];
			private float stopAtTime = 0.0f;
			private static bool _debugIsPaused = false;

			void OnGUI()
			{
				if (!Application.isPlaying) return;
#if PAV_SHOW_FPS
        // Space =>  Pause/Resume
        //
        if (Time.deltaTime > 0.001f)
        {
            fps = (fps + Mathf.FloorToInt(1.0f / Time.deltaTime)) >> 1;
        }
        // Stay for while.
        if(Mathf.Repeat(Time.time, 1.0f) < 0.5f)
        {
            displayFPS = fps;
        }
        //GUI.contentColor = new Color(0, 255, 0, 255);
        //GUI.skin.label.fontSize = 250;
        //GUI.Label(new Rect(0.0f, 10.0f, 100.0f, 20.0f), new GUIContent("fps:" + displayFPS.ToString()));

        var customStyle = GUIStyle.none;
        customStyle.fontSize = 60;
        customStyle.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(440.0f, 0, 200.0f, 200.0f), new GUIContent("fps:" + displayFPS.ToString()), customStyle);
#endif
				//
				if (localDebugSettings.enableDebugPauseResume)
				{
					//
					if (stopAtTime > 1e-5f && stopAtTime < Time.time)
					{
						Time.timeScale = 0.0f;
						stopAtTime = 0.0f;
						_debugIsPaused = true;
					}

					/*// space bar pause/resume
					{
					    if (Input.GetKeyDown(KeyCode.Space))
					    {
					        if (!keyDowns[(int)KeyCode.Space])
					        {
					            keyDowns[(int)KeyCode.Space] = true;
					            DebugPause();
					        }
					    }
					    else
					    {
					        keyDowns[(int)KeyCode.Space] = false;
					    }

					    // right arrow move on.
					    if (Input.GetKeyDown(KeyCode.RightArrow))
					    {
					        if (!keyDowns[(int)KeyCode.RightArrow])
					        {
					            keyDowns[(int)KeyCode.RightArrow] = true;
					            DebugResume();
					        }
					    }
					    else
					    {
					        keyDowns[(int)KeyCode.RightArrow] = false;
					    }
					}*/
				}
			}

			// Whether is paused.
			internal bool DebugIsPaused()

			{
				return _debugIsPaused;
			}

			// Debug pause/resume
			internal void DebugPause()
			{
				if (localDebugSettings.enableDebugPauseResume)
				{
					if (Time.timeScale < 0.5f)
					{
						Time.timeScale = 1.0f;
						_debugIsPaused = false;
					}
					else
					{
						Time.timeScale = 0.0f;
						_debugIsPaused = true;
					}

					stopAtTime = 0.0f;
				}
			}

			// Debug move a frame
			internal void DebugNextFrame()
			{
				if (localDebugSettings.enableDebugPauseResume)
				{
					stopAtTime = Time.time + 0.02f;
					Time.timeScale = 1.0f;
				}
			}
#endif

			#endregion


			#region Private Methods

#if UNITY_EDITOR
			private void UpdateNecessaryFieldsInEditorMode()
			{
				// Currently if editor mode
				if (!Application.isPlaying)
				{
					var builtinMatCfg =
						UnityEditor.AssetDatabase.LoadAssetAtPath<PicoMaterialConfiguration>(
							"Packages/org.byted.avatar.sdk/Settings/BuiltInMaterialConfiguration.asset");
					var urpMatCfg =
						UnityEditor.AssetDatabase.LoadAssetAtPath<PicoMaterialConfiguration>(
							"Packages/org.byted.avatar.sdk/Settings/URPMaterialConfiguration.asset");
					// Check default configuration.

					// Builtin
					if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null)
					{
						if (renderSettings.materialConfiguration == null ||
						    renderSettings.materialConfiguration == urpMatCfg)
						{
							renderSettings.materialConfiguration = builtinMatCfg;
							UnityEditor.EditorUtility.SetDirty(this);
						}
					}
					else // Srp.
					{
						if (renderSettings.materialConfiguration == null ||
						    renderSettings.materialConfiguration == builtinMatCfg)
						{
							renderSettings.materialConfiguration = urpMatCfg;
							UnityEditor.EditorUtility.SetDirty(this);
						}
					}
					//


					return;
				}
			}
#endif

			#endregion
		}
	}
}