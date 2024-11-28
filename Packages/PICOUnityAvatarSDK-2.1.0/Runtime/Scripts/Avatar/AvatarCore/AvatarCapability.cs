using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Configuration when loading avatar 
		/// </summary>
		[System.Serializable]
		public class AvatarCapabilities
		{
			#region Types

			// Bits for flags.
			public enum Flags
			{
				//MainAvatar = 1 << 0,                        // Whether is main avatar. Usually player is the main avatar.
				EnableFaceExpressionTransfer = 1 << 2, // Whether enable Facial Expression Transfer

				FlipFollowMode =
					1 << 3, // Whether flip following mode.  left and right bs channel and ik should be reversed.

				ShowOnlyWithAvatarBunch =
					1 << 4, // Whether show with avatar bunch and depress building self render data
				AvatarBunchSource = 1 << 5, // Whether used as avatar bunch.

				AllowAvatarMetaFromCache =
					1 << 6, // Whether allow avatar meta from cache when network is very slow. It means maybe old avatar of the user maybe loaded.

				// PicoAvatarManager.SetEnableWeakNetwork(true) need be invoked before loading avatar or pico avatar app enableWeakNetworkMode enabled.
				UniqueAvatar =
					1 << 8, // is the unique avatar globally. only one avatar created. if set, asset resources will be unloaded immediately when the avatar loaded.
			}

			public enum Usage
			{
				Normal = 0, // load the avatar normally
				ForBake = 1, // load the avatar for bake

				AllowEdit =
					2, // load the avatar to allow edit in runtime. if allow edit, any bake for runtime optimization will be depressed.
				AsPlaceHolder = 3, // load the avatar as place holder.
			}

			public enum BakeType
			{
				GLTF = 0,
				GLB = 1,
				USD = 2,
			}

			[Flags]
			public enum BakeFlags
			{
				ExportAllAnimations = 1 << 0,
				ExportWithKtx = 1 << 1,
				ExportWithSparse = 1 << 2,
			}

			public struct BakeOptions
			{
				public BakeType bakeType; // bake type
				public int bakeFlags; // bake flags
				public string[] customAnimationSetIds; // custom animation set ids, if exist

				public string[] exportAnimations; // animation selected to export, is invalid if |exportAllAnimations| = true 

				public string exportFilePath; // exported file path, excluding the suffix
				public string dependResDir; // depends resource path, astcenc will be saved here
				public string[] bakedPartIds; // baked part ids, if exist
			}

			#endregion


			#region Public Fields

			[Tooltip("How to show avatar.")]
			public AvatarManifestationType manifestationType = AvatarManifestationType.Full;

			[Obsolete("Not support currently.")] public uint viewType = (uint)AvatarViewType.FirstPerson;

			[Tooltip(
				"Whether to enable mesh clipping. some mesh triangles in body mesh that overlapped with clothes may be removed.")]
			public bool bodyCulling = true;

			[Tooltip(
				"Whether update expression bs channel weights from native part.\r\n c++ part will sync face expression weights if developer invoke PicoAvatarEntity.ApplyFaceExpressionPacket even the field value is false.")]
			public bool enableExpression = true;
			
			[Obsolete("Not used anymore.")] public bool useFaceTrackor = false;

			[Tooltip("whether enable show place holder avatar before the avatar loaded.")]
			public bool enablePlaceHolder = false;

			[Tooltip("asset id of hand/gloves when show avatar with only hand mode.")]
			public string handAssetId;

            [Tooltip("initial lod level to load avatar. If invalid, usually load lod2 at first.")]
            public AvatarLodLevel initLodLevel = AvatarLodLevel.Invalid;

			[Tooltip("max lod level clamped to load avatar. if not invalid, prior to PicoAvatarApp.maxLodLevel.")]
			public AvatarLodLevel maxLodLevel = AvatarLodLevel.Invalid;

			[Tooltip("lod level forced to load avatar. if not invalid, prior to PicoAvatarApp.forceLodLevel.")]
			public AvatarLodLevel forceLodLevel = AvatarLodLevel.Invalid;

			[Tooltip("Animation packet record level.")]
			public RecordBodyAnimLevel recordBodyAnimLevel = RecordBodyAnimLevel.Invalid;

			[Tooltip("Default sex/gender if no avatar created for the user.")]
			public uint defaultAvatarGender = 1;

			[FormerlySerializedAs("autoStopAnimating")]
			[Tooltip("Whether auto stop animation when animation played to end.")]
			public bool autoStopIK = true;

			[Tooltip("Who control the avatar.")]
			public ControlSourceType controlSourceType = ControlSourceType.OtherPlayer;

			[Tooltip("Avatar ik inputType.")]
			public DeviceInputReaderBuilderInputType inputSourceType = DeviceInputReaderBuilderInputType.Invalid;

			[Tooltip("Mode of ik calculation.")] 
			public AvatarIKMode ikMode = AvatarIKMode.FullBody;

			[Tooltip("Bit flags from AvatarAnimationFlags.")]
			public uint animationFlags = 0;

			[Tooltip("Bit flags from AvatarCapabilities.Flags")]
			public uint flags = 0;

			[Tooltip("Exclusive flag for the usage of loading avatar")]
			public Usage usage = Usage.Normal;

			 [Tooltip("From which lod level to use avatar bunch. Currently should be same to maxLodLevel set if want to enable AvatarBunch, until skeleton retarget is supported.")]
			public AvatarLodLevel avatarBunchLodLevel = AvatarLodLevel.Invalid;

			[Tooltip("how to compact memory about avatar. Bit flags from AvatarMemoryCompactFlags.Flags")]
			public uint memoryCompactFlags = (uint)AvatarMemoryCompactFlags.ClearUsedAssetsInEditor;

			[Tooltip(
				"avatar style name to force to load. if null or empty, default one loaded. such as: \"PicoAvatar\" \"PicoAvatar2\" \"PicoAvatar3\"")]
			public string avatarStyleName;

			[Tooltip("Half body position in meters used when generating half body.")]
			public float halfBodyPos = 1.0f;

			
            //@brief Current avatar header display mode
            //@note Usually set to AvatarHeadShowType.Hide for yourself and AvatarHeadShowType.Normal for others 
            [Tooltip("whether hide avatar head and nodes on head.")]
			public AvatarHeadShowType headShowType = AvatarHeadShowType.Normal;
			
			[Tooltip("Bake Options when the usage is forBake.")]
			public BakeOptions bakeOptions;
			#endregion


			#region Public Properties / Methods

			public string ToJson()
			{
				// Whether to use body tracking
				bool isBodyTrackingMode = false;

                // check data.
                if (isMainAvatar)
                {
                    controlSourceType = ControlSourceType.MainPlayer;
					if (inputSourceType == DeviceInputReaderBuilderInputType.BodyTracking)
					{
						isBodyTrackingMode = true;
					}
				}

				if (recordBodyAnimLevel == RecordBodyAnimLevel.Invalid)
				{
					recordBodyAnimLevel = PicoAvatarApp.instance == null
						? RecordBodyAnimLevel.FullBone
						: PicoAvatarApp.instance.netBodyPlaybackSettings.defaultPlaybackLevel;
				}

				if (isBodyTrackingMode)
				{
					recordBodyAnimLevel = RecordBodyAnimLevel.FullBone;
				}

				Dictionary<string, object> result = new Dictionary<string, object>();
				result.Add("manifestationType", (int)manifestationType);
				result.Add("viewType", viewType);
				result.Add("bodyCulling", bodyCulling);
				//result.Add("isLocalAvatar", isLocalAvatar);
				result.Add("isBodyTrackingMode", isBodyTrackingMode);
				//result.Add("useFaceTrackor", useFaceTrackor);
				result.Add("enablePlaceHolder", enablePlaceHolder);
				result.Add("maxLodLevel", (int)maxLodLevel);
				result.Add("forceLodLevel", (int)forceLodLevel);
				result.Add("snapshotLevel", (int)recordBodyAnimLevel);
				result.Add("defaultAvatarGender", defaultAvatarGender);
				result.Add("autoStopAnimating", autoStopIK);
				result.Add("controlSourceType", (int)controlSourceType);
				result.Add("ikMode", (int)ikMode);
				result.Add("animationFlags", animationFlags);
				result.Add("flags", flags);
				result.Add("handAssetId", handAssetId);
				result.Add("avatarBunchLodLevel", (int)avatarBunchLodLevel);
				result.Add("memoryCompactFlags", (int)memoryCompactFlags);
				result.Add("avatarStyleName", avatarStyleName);
				result.Add("halfBodyPos", (float)halfBodyPos);
				result.Add("usage", (int)usage);

				if (usage == Usage.ForBake)
				{
					result.Add("bakeOptions", bakeOptions);
				}

				string resultJson = JsonConvert.SerializeObject(result);

				//resultJson = LitJson_Renamed.JsonMapper.ToJson(this);
				return resultJson;
			}

			// Whether allow edit
			public bool allowEdit
			{
				get => this.usage == Usage.AllowEdit;
			}

			// Whether for bake
			public bool forBake
			{
				get => this.usage == Usage.ForBake;
			}

			// Whether main avatar.
			public bool isMainAvatar
			{
				get => controlSourceType == ControlSourceType.MainPlayer;
			}

			// Whether enable facial expression transfer
			public bool enableFacialExpressionTransfer
			{
				get => (this.flags & (uint)Flags.EnableFaceExpressionTransfer) != 0;
			}

			// Whether enable flip follow mode. thus left and right bs channels are swapped.
			public bool isFlipFollowMode
			{
				get => (this.flags & (uint)Flags.FlipFollowMode) != 0;
			}

			// Whether is avatar bunch source.
			public bool isAvatarBunchSource
			{
				get => (this.flags & (uint)Flags.AvatarBunchSource) != 0;
			}

			// Whether allow avatar meta from cache when network is very slow. It means maybe old avatar of the user maybe loaded.
			public bool isAllowAvatarMetaFromCache
			{
				get => (this.flags & (uint)Flags.AllowAvatarMetaFromCache) != 0;
			}

			// whether use the avatar as placeholder avatar.
			public bool isAsPlaceholder
			{
				get => this.usage == Usage.AsPlaceHolder;
			}

			#endregion
		}
	}
}