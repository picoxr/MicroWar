using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Debug log type masks.
		/// </summary>
		public enum DebugLogMask
		{
			/// <summary>
			/// General info.
			/// </summary>
			GeneralInfo = 1 << 0,

			/// <summary>
			/// General warn.
			/// </summary>
			GeneralWarn = 1 << 1,

			/// <summary>
			/// General error.
			/// </summary>
			GeneralError = 1 << 2,

			/// <summary>
			/// General error.
			/// </summary>
			Framework = 1 << 3,

			/// <summary>
			/// about avatar load trivial
			/// </summary>
			AvatarLoadTrivial = 1 << 4,

			/// <summary>
			/// about avatar load
			/// </summary>
			AvatarLoad = 1 << 5,

			/// <summary>
			/// about avatar ik
			/// </summary>
			AvatarIK = 1 << 6,

			/// <summary>
			/// about avatar animation.
			/// </summary>
			AvatarAnimation = 1 << 7,

			/// <summary>
			/// about native invoke
			/// </summary>
			NativeCallTrivial = 1 << 10,

			/// <summary>
			/// about native invoke
			/// </summary>
			NativeCallImportant = 1 << 11,

			/// <summary>
			/// all about native invoke.
			/// </summary>
			NativeCallAll = (NativeCallTrivial | NativeCallImportant),

			/// <summary>
			/// about avatar edit trivial
			/// </summary>
			EditTrivial = 1 << 13,

			/// <summary>
			/// about avatar edit important
			/// </summary>
			EditImportant = 1 << 14,

			/// <summary>
			/// about avatar edit trivial
			/// </summary>
			AssetTrivial = 1 << 18,

			/// <summary>
			/// about avatar edit important
			/// </summary>
			AssetImportant = 1 << 19,

			/// <summary>
			/// about avatar edit trivial
			/// </summary>
			EngineTrivial = 1 << 21,

			/// <summary>
			/// about avatar edit important
			/// </summary>
			EngineImportant = 1 << 22,

			/// <summary>
			/// about net trivial
			/// </summary>
			NetTrivial = 1 << 23,

			/// <summary>
			/// profile start flag
			/// </summary>
			ProfileStatsStart = 1 << 25,

			/// <summary>
			/// profile end flag
			/// </summary>
			ProfileStatsEnd = 1 << 26,

			/// Temporarily disable all
			//WholelyDisableLog = 1 << 30,

			/// <summary>
			/// force to log.
			/// </summary>
			ForceLog = 1 << 30,
		}

		/// <summary>
		/// Flags to initialize environment.
		/// </summary>
		public enum EnvironmentFlags
		{
			/// <summary>
			/// if flag is set, playing mode, otherwise editor mode.
			/// </summary>
			Playing = 1 << 0,
		}

		// delegate invoked when log arrived. It is not thread safe.
		public delegate void AvatarLogAddedT(DebugLogMask logMask, string message);

		/// <summary>
		/// SDK Environment used to pass engine side runtime environment to native sdk library.
		/// </summary>
		public class AvatarEnv
		{
			#region Public Types

			/// <summary>
			/// Flags to initialize environment.
			/// </summary>
			public enum EnvironmentFlags
			{
				// if flag is set, playing mode, otherwise editor mode.
				Playing = 1 << 0,
			}

			#endregion


			#region Public Properties

			// events that avatar log added. It is not thread safe.
			public static event AvatarLogAddedT OnAvatarLogAddedT;

			// thread safe time getter.
			public static float realtimeSinceStartup
			{
				get { return pav_AvatarEnv_GetRealtimeSinceStartup(); }
			}

			// Path of root directory where to place readonly resource data.
			public static string resourceSpacePath
			{
				get => Application.streamingAssetsPath;
			}

			// Path of directory where to place AvatarSDKScript
			public static string scriptResourcePath 
			{ 
				get => System.IO.Path.Combine(Application.dataPath.Replace("Assets", string.Empty),
				"Packages/org.byted.avatar.sdk/Runtime/Resources"); 
			}

			// Path of root directory where to place persistent cache data.
			public static string cacheSpacePath
			{
				get
				{
#if UNITY_ANDROID && !UNITY_EDITOR
                    if (localModeOrPath == "false"){
                        if(string.IsNullOrEmpty(Utility.desiredAvatarCacheDirectory))
                        {
                            return "/sdcard";    
                        }
                        else
                        {
                            return Application.persistentDataPath;
                        }
                    }
                    else{
                        return localModeOrPath;
                    }
#else
					if (localModeOrPath == "false")
					{
						return Application.persistentDataPath;
					}
					else
					{
						return localModeOrPath;
					}
#endif
				}
			}

			public static string localModeOrPath = "false";

			// Path of root directory where to place persistent data other than cache data.
			public static string workSpacePath
			{
				get

#if UNITY_ANDROID && !UNITY_EDITOR
            => Application.persistentDataPath;
#else
					=> Application.streamingAssetsPath;
#endif
			}

			public const string AvatarRelativePath = "AvatarSDKScript/";

			public static string avatarPath
			{
				get => System.IO.Path.Combine(workSpacePath, AvatarRelativePath);
			}

			public const string AvatarPackedRelativePath = "AvatarSDKScript.bytes";

			public static string avatarPackedPath
			{
				get => System.IO.Path.Combine(scriptResourcePath, AvatarPackedRelativePath);
			}

#if UNITY_ANDROID && !UNITY_EDITOR
            public static bool avatarPackedPathFirst = true;
#else
			public static bool avatarPackedPathFirst = false;
#endif
			public const string CacheRelativePath = "AvatarCache";

			public static string avatarCachePath
			{
				get => System.IO.Path.Combine(cacheSpacePath, CacheRelativePath);
			}

			#endregion


			#region Public Methods

			/// <summary>
			/// Initialize.
			/// </summary>
			/// <param name="enginePluginVersion"></param>
			/// <param name="debugLogMasks"></param>
			/// <param name="path"></param>
			/// <returns>native result.</returns>
			internal static NativeResult Initialize(string enginePluginVersion, uint debugLogMasks, string path)
			{
				NativeResult result = NativeResult.Unsupported;

				if (!_initialized)
				{
					localModeOrPath = path;
					uint flags = 0;
					// whether is playing mode. 
					if (Application.isPlaying)
					{
						flags |= (uint)EnvironmentFlags.Playing;
					}

					//
					result = pav_AvatarEnv_Initialize(enginePluginVersion, flags, (uint)TargetEngineType.Unity);
					if (result != NativeResult.Success)
					{
						return result;
					}

					// set log callback.
					{
						if (!_logContentBuffer.IsCreated)
						{
							_logContentBuffer = new NativeArray<byte>((int)LogContentBufferSize, Allocator.Persistent,
								NativeArrayOptions.ClearMemory);
						}

						//
						_debugLogMasks = debugLogMasks;
						unsafe
						{
							pav_AvatarEnv_SetLogCallback(_debugLogMasks, OnLogCalback,
								(System.IntPtr)_logContentBuffer.GetUnsafePtr(), LogContentBufferSize - 1);
						}
					}

					// set initialized flag.
					_initialized = true;
				}

#if PAV_LOG_DIABLE_STACK_TRACE || true
				// disable trace.
				UnityEngine.Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
#endif

				return result;
			}

			// Initialize.
			internal static void UnInitialize()
			{
				if (_initialized)
				{
					_initialized = false;
					// 
					{
						// free log buffer.
						if (_logContentBuffer.IsCreated)
						{
							_logContentBuffer.Dispose();
						}

						pav_AvatarEnv_SetLogCallback(10000, null, System.IntPtr.Zero, 0);
					}
					//
					pav_AvatarEnv_Uninitialize();
				}
			}

			
            //Sets debug log masks. bits from DebugLogMask.
            //Invoked by PicoAvatarApp when config changed.
            internal static void SetDebugLogMasks(uint logMasks)
			{
				_debugLogMasks = logMasks;

				// if initialized, set debug log mask.
				if (_initialized)
				{
					pav_AvatarEnv_SetDebugLogMasks(logMasks);
				}
			}
            
            //Sets debug log masks. bits from DebugLogMask.
            //Invoked by PicoAvatarApp when config changed.
            internal static void SetEnableLogTime(bool enableLogTime)
			{
				_enableLogTime = enableLogTime;
			}

			/// <summary>
			/// Log avatar stats
			/// </summary>
			/// <returns>The string of avatar stats</returns>
			public static string GetAvatarStats()
			{
				string ret = "";
				if (_initialized)
				{
					unsafe
					{
						var len = pav_AvatarEnv_LogAvatarStats((System.IntPtr)_logContentBuffer.GetUnsafePtr(),
							LogContentBufferSize);
						ret = System.Text.Encoding.ASCII.GetString((byte*)_logContentBuffer.GetUnsafePtr(), (int)len);
					}
				}

				return ret;
			}

			// Sets whether enable native c++ stats.
			internal static void SetCXXObjectStatsEnabled(bool enabled)
			{
				pav_AvatarEnv_SetStatsEnabled(enabled, 0);
			}

			/// <summary>
			/// Is logMask need show
			/// </summary>
			/// <param name="logMask">DebugLogMask to check</param>
			/// <returns>Check result</returns>
			public static bool NeedLog(DebugLogMask logMask)
			{
				// filter log message.
				return ((uint)logMask & _debugLogMasks) != 0;
			}

			/// <summary>
			/// Log avatar messages.
			/// </summary>
			/// <param name="debugLogMask">Log mask</param>
			/// <param name="content">Log content</param>
			public static void Log(DebugLogMask debugLogMask, string content)
			{
				if (_debugLogMasks != 0)
				{
					// if the macro defined, skipp logs other than GeneralError.
#if PAV_DISABLE_DEBUG_LOG
                    if(debugLogMask != DebugLogMask.GeneralError && debugLogMask != DebugLogMask.ForceLog )
                    {
                        return;
                    }
#endif
					if (OnAvatarLogAddedT != null)
					{
						OnAvatarLogAddedT(debugLogMask, ConcatLogTime(content, LogSourceLangurage.CSharp));
					}
					else
					{
						switch (debugLogMask)
						{
							case DebugLogMask.GeneralError:
								UnityEngine.Debug.LogError(ConcatLogTime(content, LogSourceLangurage.CSharp));
								break;
							case DebugLogMask.GeneralWarn:
								UnityEngine.Debug.LogWarning(ConcatLogTime(content, LogSourceLangurage.CSharp));
								break;
							default:
								UnityEngine.Debug.Log(ConcatLogTime(content, LogSourceLangurage.CSharp));
								break;
						}
					}
				}
			}

			// Finish frame.
			internal static void OnFrameEnd()
			{
				// check all sample stats finished.
				while (_beginedSampleCount > 0)
				{
					_beginedSampleCount--;
					UnityEngine.Profiling.Profiler.EndSample();
				}
			}

			#endregion


			#region Private Methods

			private static bool _initialized = false;
			private static uint _debugLogMasks = 0;
			private static bool _enableLogTime = false;

			private const int LogContentBufferSize = 10240;
			private static NativeArray<byte> _logContentBuffer = new NativeArray<byte>();
			private static GCHandle _handleForLogContentBuffer;

			private static int _beginedSampleCount = 0;

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			// log source langurage.
			enum LogSourceLangurage
			{
				Unknown = 0,
				CSharp = 1,
				JS = 2,
				CPlusPlus = 3,
			}

			// log callback type.
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void LogCallback(int debugLogMask, uint contentLen);

			// PI callback.
			[MonoPInvokeCallback(typeof(LogCallback))]
			private static void OnLogCalback(int debugLogMask, uint contentLen)
			{
				if (contentLen < LogContentBufferSize && _debugLogMasks != 0)
				{
					// if the macro defined, skipp logs other than GeneralError.
#if PAV_DISABLE_DEBUG_LOG
                    if(debugLogMask != DebugLogMask.GeneralError && debugLogMask != DebugLogMask.ForceLog)
                    {
                        return;
                    }
#endif
					string logContent;
					unsafe
					{
						logContent = System.Text.Encoding.ASCII.GetString((byte*)_logContentBuffer.GetUnsafePtr(),
							(int)contentLen);
					}

					if (OnAvatarLogAddedT != null)
					{
						OnAvatarLogAddedT((DebugLogMask)debugLogMask, ConcatLogTime(logContent));
					}
					else
					{
						//
						switch ((DebugLogMask)debugLogMask)
						{
							case DebugLogMask.GeneralError:
								UnityEngine.Debug.LogError(ConcatLogTime(logContent));
								break;
							case DebugLogMask.GeneralWarn:
								UnityEngine.Debug.LogWarning(ConcatLogTime(logContent));
								break;
							case DebugLogMask.ProfileStatsStart:
							{
								++_beginedSampleCount;
								UnityEngine.Profiling.Profiler.BeginSample(logContent);

								// can not embed too many.
								if (_beginedSampleCount > 100)
								{
									// check all sample stats finished.
									while (_beginedSampleCount > 0)
									{
										_beginedSampleCount--;
										UnityEngine.Profiling.Profiler.EndSample();
									}
								}
							}
								break;
							case DebugLogMask.ProfileStatsEnd:
							{
								if (_beginedSampleCount > 0)
								{
									--_beginedSampleCount;
								}

								UnityEngine.Profiling.Profiler.EndSample();
							}
								break;
							default:
								UnityEngine.Debug.Log(ConcatLogTime(logContent));
								break;
						}
					}
				}
			}

			// Concate log time.
			static string ConcatLogTime(string content, LogSourceLangurage logLan = LogSourceLangurage.Unknown)
			{
				if (_enableLogTime)
				{
					var curTime = AvatarEnv.realtimeSinceStartup;
					var seconds = Mathf.FloorToInt(curTime);
					var millSeconds = (int)((curTime - seconds) * 1000.0f);
					var minutes = seconds / 60;
					var hour = minutes / 60;
					minutes = minutes % 60;
					seconds = seconds % 60;

					if (hour == 0)
					{
						content = string.Format("pav:{0} {1:D2}:{2:D2}:{3:D3}: ",
							          (logLan == LogSourceLangurage.CSharp ? "C#: " : ""), minutes, seconds,
							          millSeconds) +
						          content;
					}
					else
					{
						content = string.Format("pav:{0} {1:D2}:{2:D2}:{3:D2}:{4:D3}: ",
							          (logLan == LogSourceLangurage.CSharp ? "C#: " : ""), hour, minutes, seconds,
							          millSeconds) +
						          content;
					}
				}

				return content;
			}


			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarEnv_Initialize(string enginePluginVersion, uint flags,
				uint targetEngineType);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarEnv_Uninitialize();

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarEnv_SetLogCallback(uint debugLogMasks, LogCallback logcallback,
				System.IntPtr logContentBuffer, uint logContentBufferSize);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEnv_AddLog(int logLevel, string content, bool backEngine);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEnv_SetDebugLogMasks(uint logMasks);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern float pav_AvatarEnv_GetRealtimeSinceStartup();

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarEnv_LogAvatarStats(System.IntPtr contentBuffer, uint bufferSize);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEnv_SetStatsEnabled(bool statEnabled, int flags);

			#endregion
		}
	}
}