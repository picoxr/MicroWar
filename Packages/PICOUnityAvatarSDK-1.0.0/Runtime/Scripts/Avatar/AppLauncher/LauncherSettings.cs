using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Pico.Avatar
{
	/// <summary>
	///App Settings
	/// </summary>
	[Serializable]
	public class AppSettings
	{
		/// <summary>
		/// Whether to use Matrix avatarSDK service(We recommend you to false when build apk, use true in PC)
		/// </summary>
		/// <remarks>
		/// If checked, the libeffect.so and AvatarSDKScript.bytes in your project will be disabled.
		/// Your application will use system services (your application will be hot updated by Matrix for the underlying sdk libraries).
		/// If unchecked, your application will need to be updated manually with the release of the sdk version,
		/// and unknown problems may occur if this is not done
		/// </remarks>
		public bool localMode = true;

		/// <summary>
		/// Whether do garbage collection automatically
		/// </summary>
		public bool autoGarbageCollection;

		/// <summary>
		/// Server type
		/// </summary>
		public ServerType serverType;

		/// <summary>
		/// Main camera used to calcuate lod, cull animation ...
		/// </summary>
		public Camera mainCamera;

		/// <summary>
		/// Additive gi for dark scene to raise base lighting of avatar for simple light mode
		/// </summary>
		/// <remarks>
		/// You must set before load any avatar.
		/// </remarks>
		[Range(0, 1)] public float additiveGI;

		/// <summary>
		/// Enabled full screen features. bit flags of CommonFullscreenFeatureType
		/// </summary>
#if UNITY_EDITOR
		[EnumFlagsAttribute]
#endif
		public CommonFullscreenFeatureType enabledFullscreenFeatures;

		/// <summary>
		/// Local Path
		/// </summary>
		[HideInInspector] public string localResourcePath;

		/// <summary>
		/// avatarSdkVersion
		/// V1 Avatar no version
		/// V2 Avatar >= 1.3.0  < 2.0.0
		/// V3 Avatar >= 2.0.0
		/// CustomAvatar & Character >= 2.5.0
		/// BodyTracking >= 2.6.0
		/// Avatar Bake >= 2.7.0
		/// Standardize & reform >= 2.8.0
		/// </summary>
		[NonSerialized] public const string avatarSdkVersion = "2.8.0";

		/// <summary>
		/// the unity plugin commit.
		/// </summary>
		[NonSerialized] public const string unityPluginCommit = "7c5b6bac";

		/// <summary>
		/// Version of under level avatar sdk. It should be get from native library.
		/// </summary>
		[NonSerialized] public string avatarCoreVersion;
	}

	/// <summary>
	///Login Settings
	/// </summary>
	[Serializable]
	public class LoginSettings
	{
		/// <summary>
		/// for internal test
		/// </summary>
		public string userID;

		/// <summary>
		/// The user's unique identifier, which belongs to the SDK initialization parameter,
		/// which can be obtained through PlatformSDK
		/// </summary>
		public string accessToken;

		/// <summary>
		/// The StoreRegion when the user registers belongs to the SDK initialization parameter,
		/// which can be obtained through PlatformSDK
		/// </summary>
		[Obsolete]
		private string nationType;
	}

	/// <summary>
	///Log Settings
	/// </summary>
	[Serializable]
	public class LogSettings
	{
		/// <summary>
		/// Debug log masks. bits from DebugLogMask
		/// </summary>
#if UNITY_EDITOR
		[EnumFlagsAttribute]
#endif
		public DebugLogMask debugLogMasks;

		/// <summary>
		/// Enable add time to log item to helper analyse load performance.
		/// </summary>
		public bool enableLogTime;
	}

	/// <summary>
	/// sdk avatar render module related Settings
	/// </summary>
	[Serializable]
	public class AvatarRenderSettings
	{
		/// <summary>
		/// Whether to turn off image mesh batch processing
		/// </summary>
		[HideInInspector] public bool disablePrimitiveMerge;

		/// <summary>
		/// whether force disable casting shadow..
		/// </summary>
		public bool forceDisableCastShadow;

		/// <summary>
		/// whether force disable receive shadow.
		/// </summary>
		public bool forceDisableReceiveShadow;

		/// <summary>
		/// meshVertex compression format 
		/// </summary>
		[HideInInspector] public DataFormat meshVertexDataFormat;

		/// <summary>
		/// meshVertex compression format
		/// </summary>
		[HideInInspector] public DataFormat morphVertexDataFormat;

		/// <summary>
		/// Material.renderQueue for avatar transparent object.
		/// </summary>
		[Range(3000, 3999)] public int avatarTranspQueueStart;

		/// <summary>
		/// Material.renderQueue for avatar opaque object.
		/// </summary>
		[Range(2000, 3999)] 
		public int avatarOpaqueQueueStart;

		/// <summary>
		/// whether show rim profile.
		/// </summary>
		[HideInInspector] public bool enableRimProfile;

		/// <summary>
		/// distance to switch to line profile mode.
		/// </summary>
		[HideInInspector] public float lineProfileDistance;

		/// <summary>
		/// sdk material configuration, you can switch the built-in and urp materials
		/// </summary>
		[SerializeField] public Pico.Avatar.PicoMaterialConfiguration materialConfiguration;
	}

	/// <summary>
	///Lod module related Settings
	/// </summary>
	[Serializable]
	public class AvatarLodSettings
	{
		[HideInInspector] public AvatarLodLevel maxLodLevel;

		/// <summary>
		/// Force lod level. if is a valid lod level, lod level will be fixed
		/// </summary>
		public AvatarLodLevel forceLodLevel;

		/// <summary>
		/// lod0 screen percentage that a lod occupy.
		/// </summary>
		[Range(0, 1)] [HideInInspector] public float lod0ScreenPercentage;

		/// <summary>
		/// lod1 screen percentage that a lod occupy.
		/// </summary>
		[Range(0, 1)] [HideInInspector] public float lod1ScreenPercentage;

		/// <summary>
		/// lod2 screen percentage that a lod occupy.
		/// </summary>
		[Range(0, 1)] [HideInInspector] public float lod2ScreenPercentage;

		/// <summary>
		/// lod3 screen percentage that a lod occupy.
		/// </summary>
		[Range(0, 1)] [HideInInspector] public float lod3ScreenPercentage;

		/// <summary>
		/// lod4 screen percentage that a lod occupy.
		/// </summary>
		[Range(0, 1)] [HideInInspector] public float lod4ScreenPercentage;
	}

	/// <summary>
	///Animation Settings
	/// </summary>
	[Serializable]
	public class AvatarAnimationSettings
	{
		/// <summary>
		/// Bit flags from AvatarAnimationFlags
		/// </summary>
#if UNITY_EDITOR
		[EnumFlagsAttribute]
#endif
		[Tooltip("Bit flags from AvatarAnimationFlags.")]
		public AvatarAnimationFlags animationFlags;
	}

	/// <summary>
	/// avatarOptimization module related Settings
	/// </summary>
	[Serializable]
	public class AvatarOptimizationSettings
	{
		/// <summary>
		/// position sync threshold for none-animation calculation.
		/// </summary>
		[Range(0.0f, 0.1f)] [HideInInspector] public float positionErrorThreshold;

		/// <summary>
		/// orientation sync threshold for non-animation calculation.
		/// </summary>
		[Range(0.0f, 0.1f)] [HideInInspector] public float orientationErrorThreshold;

		/// <summary>
		/// scale sync threshold for non-animation calculation.
		/// </summary>
		[Range(0.0f, 0.1f)] [HideInInspector] public float scaleErrorThreshold;

		/// <summary>
		/// skip updation if invisible this frame.
		/// </summary>
		public bool skipUpdateIfInvisibleThisFrame;

		/// <summary>
		/// dist in metre out side which will reduce update time by skipUpdateInterval even the avatar is visible
		/// </summary>
		[Range(-0.1f, 50.0f)] public float skipUpdateEvenVisibleDistance;

		/// <summary>
		/// time interval
		/// </summary>
		[Range(0.0f, 1.0f)] public float skipUpdateEvenVisibleInterval;

		/// <summary>
		/// distance to skip face expression.
		/// </summary>
		[Range(0.0f, 50.0f)] public float skipFaceExpressionPlaybackDist;

		[Tooltip(
			"Whether enable weak network mode. if true, avatar meta and asset package meta will be cached and can be loaded from local cache to avoid accessing server.")]
		public bool enableWeakNetworkMode;

		/// <summary>
		/// Set whether the IK driver is updated by ikInterpDelayTime
		/// </summary>
		[HideInInspector] public bool intermitentMode;

		/// <summary>
		/// ik interplation delay time. When using intermitentMode, self update will delay to avoid jittering
		/// </summary>
		[Range(0.0f, 1.0f)] [HideInInspector] public float IKInterpDelayTime;
	}

	/// <summary>
	///  AvatarExtraSettings
	/// </summary>
	[Serializable]		
	public class AvatarExtraSettings
	{
		/// <summary>
		/// internal develop information
		/// </summary>
		public string configString;

		/// <summary>
		/// whether Display a placeholder with builtin avatar when the image is not loaded. default avatar used.
		/// </summary>
		public bool enableBuiltinAvatarPlaceHolder;

		/// <summary>
		/// Avatar unity scene layer configuration. if not -1,
		/// set the value for GameObject of newly created avatar
		/// </summary>
		public int avatarSceneLayer;

		/// <summary>
		/// Unity scene layer to hide avatar heads(bit index to label the layer)
		/// </summary>
		public int mainAvatarHeadSceneLayer;
	}

	/// <summary>
	/// netPlayback module related Settings
	/// </summary>
	[Serializable]
	public class AvatarNetBodyPlaybackSettings
	{
		/// <summary>
		/// max packet count cached per frame.
		/// </summary>
		[Range(1, 10)] public int maxPacketCountPerFrame;

		/// <summary>
		/// average net delay time. 
		/// </summary>
		[Range(0.01f, 2.0f)] public float avgPlaybackDelayTime;

		/// <summary>
		/// maximum speed ratio when run after during playback.
		/// </summary>
		[Range(1.0f, 2.0f)] public float maxPlaybackSpeedRatio;

		/// <summary>
		/// minimum speed ratio when run before during playback.
		/// </summary>
		[Range(0.0f, 1.0f)] public float minPlaybackSpeedRatio;

		/// <summary>
		/// interval to playback. if 0, disable record.
		/// </summary>
		[Range(0.0f, 1.0f)] public float playbackInterval;

		/// <summary>
		/// Default playback snapshot level
		/// </summary>
		public RecordBodyAnimLevel defaultPlaybackLevel;

		/// <summary>
		/// version of packet to record. For new application, should always be the largest value.
		/// </summary>
		[Range(1, 5)] public int recordVersion;

		/// <summary>
		/// interval to record packet. if 0, disable record.
		/// </summary>
		[Range(0.0f, 1.0f)] public float recordInterval;

		/// <summary>
		/// record packet apply mode
		/// </summary>
		public RecordPacketApplyMode packetApplyMode;
	}

	/// <summary>
	/// netPlayback module related Settings
	/// </summary>
	[Serializable]
	public class AvatarFaceExpressionNetPlaybackSettings
	{
		[Tooltip("max packet count cached per frame.")] [Range(1, 10)]
		public int maxPacketCountPerFrame;

		[Tooltip("average net delay time..")] [Range(0.01f, 2.0f)]
		public float avgPlaybackDelayTime;

		[Tooltip("maximum speed ratio when run after during playback..")] [Range(1.0f, 2.0f)]
		public float maxPlaybackSpeedRatio;

		[Tooltip("minum speed ratio when run before during playback.")] [Range(0.0f, 1.0f)]
		public float minPlaybackSpeedRatio;

		[Tooltip("version of packet to record. latest app should use 4 as suggested.")] [Range(1, 4)]
		public int recordVersion;
	}

	/// <summary>
	/// debug module related Settings
	/// </summary>
	[Serializable]
	public class AvatarLocalDebugSettings
	{
		/// <summary>
		/// config Json
		/// </summary>
		public String debugConfigText;

		/// <summary>
		/// whether eanble debug pause resume.  if enabled, Space bar
		/// </summary>
		/// <remarks>
		/// if enabled, Press Spacebar to Pause/Resume the time. Right Arrow to run a time step.
		/// </remarks>
		public bool enableDebugPauseResume;

		/// <summary>
		/// Max fps to render over which will sleep for a while.
		/// </summary>
		public bool enableStats;

		/// <summary>
		/// whether print system infos.
		/// </summary>
		public bool enableSystemInfo;

		/// <summary>
		/// Whether _traceNativeCaller. Only work in debug mode.
		/// </summary>
		public bool traceNativeCaller;
	}
}