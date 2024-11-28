using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Text;
using System.IO.Compression;
using System;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.PlayerLoop;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// SDK logic unit of a single avatar object
		/// </summary>
		/// <remarks>
		/// Here is a list of PicoAvatar image loading and display related modules
		/// You can learn through these classes, successfully create and load images in the application,
		/// and know how to access Avatar capabilities
		/// You can only create images after PicoAvatarApp and PicoAvatarManager are initialized
		/// A pico user may has one more avatars with different appearance for different occasions
		/// An avatar encapsulates all aspects of a human or creature
		/// </remarks>
		public partial class PicoAvatar : MonoBehaviour
		{
			#region Public Types

			/// <summary>
			/// Avatar state
			/// </summary>
			public enum State
			{
				None,

				/// <summary>
				/// Avatar is loading
				/// </summary>
				Loading,

				/// <summary>
				/// Avatar is running
				/// </summary>
				Running,

				/// <summary>
				/// Avatar is Dead
				/// </summary>
				Dead,
			}

			/// <summary>
			/// Runtime material override delegate
			/// </summary>
			public delegate Material MaterialProvider(AvatarLodLevel lodLevel, AvatarShaderType shaderType,
				PicoAvatarRenderMesh renderMesh = null);

			#endregion


			#region Public Fiedls/Properties

			/// <summary>
			/// Get avatar current state
			/// </summary>
			public State curState
			{
				get => _curState;
			}

			/// <summary>
			/// If materialProvider is not null, it will override avatar runtime material 
			/// </summary>
			public MaterialProvider materialProvider { get; set; } = null;

			/// <summary>
			/// Whether any entity of the avatar ready. generally only one entity created for an avatar
			/// </summary>
			public bool isAnyEntityReady { get; private set; } = false;

			/// <summary>
			/// Active entit of the avatar. generally only one entity created
			/// </summary>
			public AvatarEntity entity { get; private set; } = null;

			/// <summary>
			/// Place holder avatar from which copy AvatarEntity instance and with which share mesh/material/animation
			/// </summary>
			public PicoAvatarPlaceholderRef placeHolderRef { get; private set; } = null;

			/// <summary>
			/// Cached transform of the GameObject
			/// </summary>
			public Transform cachedTransform { get; private set; } = null;

			/// <summary>
			/// Get the unique ID when creating the avatar
			/// </summary>
			/// <remarks>
			/// This ID is stored in the PicoAvatarManager Dictionary and must be globally unique
			/// Usually, the User Identification obtained by the PlatformSDK is used as the userId of the avatar
			/// </remarks>
			public string userId { get; private set; } = "0";

			// Min interval for native part to update
			public float nativeUpdateMinInterval = 0.1f;

			/// <summary>
			/// Get the avatarId of the current Avatar image, this id is associated with the user's backpack
			/// </summary>
			public string avatarId
			{
				get => _avatarId;
			}

			/// <summary>
			/// Get the capability configuration data of the current Avatar
			/// Specified by the external AvatarLoadContext when Avatar is loaded
			/// </summary>
			public AvatarCapabilities capabilities { get; private set; } = null;

			/// <summary>
			/// Primitive node types that should depress merge. bits from AvatarNodeTypes
			/// </summary>
			public uint depressMergeNodeTypes { get; private set; } = 0;

			/// <summary>
			/// Avatar show effect kind. Currently not supported
			/// </summary>
			internal AvatarEffectKind avatarEffectKind
			{
				get => _avatarEffectKind;
			}

			/// <summary>
			/// Whether is playing animation
			/// </summary>
			public bool isPlayingAnimation { get; private set; } = false;

			// Whether  
			public bool forceUpdateWhenInactive { get; set; } = false;

			/// <summary>
			/// Avatar sex type
			/// </summary>
			public AvatarSexType sex { get; private set; } = AvatarSexType.Invalid;

			/// <summary>
			/// Native avatar id
			/// </summary>
			public uint nativeAvatarId
			{
				get => _nativeAvatarId;
			}

			/// <summary>
			/// Avatar style name
			/// </summary>
			public string avatarStyleName { get; private set; }

			// If is controlled by RemoteServer, the property specify whether to update avatar position/orientation with
			// animation playback. SDK application can control move actively.
			// @Note : not implement
			public bool isMovedWithRemotePlaybackAnimation = false;

			/// <summary>
			/// Sets the skeleton mount node array
			/// You can set JointType array to create avatar corresponding skeleton Step in Unity and render it as GameObject.
			/// You can bind items under Step
			/// Object loading and destruction under Step need to be controlled by yourself
			/// Be sure to destroy the object before destroying the avatar to prevent abnormalities or resource leakage 
			/// </summary>
			public JointType[] criticalJoints
			{
				get=>_criticalJoints;
				set
				{
					_criticalJoints = value;
					_criticalJointsDirty = true;
				}
			}

			/// <summary>
			/// Whether enable block frame when build all avatar primitives in unity
			/// </summary>
			public bool allowBlockFrameWhenLoading = false;

			/// <summary>
			/// Whether native object is active.default is visible. Update each frame with gameObject.isActiveAndEnabled
			/// </summary>
			public bool isVisible
			{
				get => _isNativeVisible;
			}

			/// <summary>
			/// Whether depress updation for simulation render data. used to takeover render data
			/// </summary>
			public bool depressUpdateSimulationRenderData = false;

			/// <summary>
			/// Whether force update skeleton even when no primitive visible. for avatar bunch item, maybe no primitive created
			/// </summary>
			public bool forceUpdateSkeleton = false;

			/// <summary>
			/// Avatar lod dirty flag. Will be set to true when avatar change lod level.
			/// </summary>
			internal bool isAvatarLodDirty = false;

			#endregion


			#region Public Methods

			/// <summary>
			/// Set unity scene layer for the avatar
			/// </summary>
			/// <param name="layer">unity scene layer</param>
			public void SetLayer(int layer)
			{
				foreach (var trans in GetComponentsInChildren<Transform>(true))
				{
					trans.gameObject.layer = layer;
				}
			}


#if NO_XR
            /// <summary>
            /// start face track for the avatar
            /// </summary>
            /// <param name="useFT">useFT whether use FT</param>
            /// <param name="useLipSync">useLipSync whether use LipSync</param>
            /// <param name="useXR">useXR whether use xr-sdk to use FT&Lipsync functionality</param>
            public void StartFaceTrack(bool useFT, bool useLipSync, bool useXR = false)
            {
                var bodyAnimController = this.entity.bodyAnimController;
                if(bodyAnimController != null){
                    bodyAnimController.StartFaceTrack(useLipSync, useFT, useXR);
                }
            }
#else

			/// <summary>
			/// Start face track for the avatar
			/// </summary>
			/// <param name="useFT">Whether use FT</param>
			/// <param name="useLipSync">Whether use LipSync</param>
			public void StartFaceTrack(bool useFT, bool useLipSync)
			{
				var bodyAnimController = this.entity.bodyAnimController;
				if (bodyAnimController != null)
				{
					bodyAnimController.StartFaceTrack(useLipSync, useFT);
				}
			}
#endif
			/// <summary>
			/// Stop face track for the avatar
			/// </summary>
			public void StopFaceTrack()
			{
				var bodyAnimController = this.entity.bodyAnimController;
				if (bodyAnimController != null)
				{
					bodyAnimController.StopFaceTracker();
				}
			}

			/// <summary>
			/// Allow to play idle animation for the avatar
			/// </summary>
			/// <param name="useIdle">UseIdle whether use normal idle animation</param>
			public void SetIdleEnable(bool useIdle)
			{
				var bodyAnimController = this.entity.bodyAnimController;
				if (bodyAnimController != null)
				{
					bodyAnimController.SetIdleEnable(useIdle);
				}
			}

			/// <summary>
			/// Sets lod outline
			/// </summary>
			/// <param name="avatarEffectKind_">System level avatar display effects</param>
			public void SetAvatarEffectKind(AvatarEffectKind avatarEffectKind_)
			{
				if (_avatarEffectKind != avatarEffectKind_)
				{
					_avatarEffectKind = avatarEffectKind_;
					//
					if (entity != null)
					{
						var avatarLod = entity.GetCurrentAvatarLod();
						if (avatarLod != null)
						{
							avatarLod.SetAvatarEffectKind(avatarEffectKind_);
						}
					}
				}
			}

			/// <summary>
			/// Set avatar height, scale avatar to a specific height
			/// </summary>
			/// <param name="height">Avatar eye height or head bone height</param>
			/// <param name="isEyeHeight">Flag indicating whether the height param is avatar eye height</param>
			internal void SetAvatarHeight(float height, bool isEyeHeight = true)
			{
				if (entity != null && entity.bodyAnimController != null && isEyeHeight)
				{
					//convert eye height to avatar head bone height
					height -= entity.avatarIKTargetsConfig.eyePositionOffset.y;
					height *= 1.0f / (1.0f + entity.headToBodyRatio);
				}

				if (_rmiObject != null)
					_rmiObject.SetAvatarHeight(height);
			}

			/// <summary>
			/// Get GetJointObject  by JointType
			/// </summary>
			/// <param name="jointType">Skeleton mount node type</param>
			/// <returns>GameObject of joint</returns>
			public GameObject GetJointObject(JointType jointType)
			{
				if (criticalJointObjects == null || _criticalJoints == null)
					return null;
				for (int i = 0; i < _criticalJoints.Length; i++)
				{
					if (jointType == _criticalJoints[i])
					{
						if (criticalJointObjects.Count > i)
							return criticalJointObjects[i];
						break;
					}
				}

				return null;
			}

			#region Avatar Animation

			/// <summary>
			/// Play simple animation, temporary helper method. More animation methods will be implemented in AvatarBodyAnimController
			/// </summary>
			/// <remarks>
			/// For local avatar, animation will be played in bottom animation layer (will not be override by default IK,
			/// gesture animation, etc). For remote avatar, animation will be played normally
			/// </remarks>
			/// <param name="name">Play animation name</param>
			/// <param name="loopTime">LoopTime how much loops to play, cycle percentage used.</param>
			/// <param name="layerName">Current animation in layer</param>
			public void PlayAnimation(string name, float loopTime = 0.0f, string layerName = "ActionLayer")
			{
				// If is local avatar, and use place holder avatar, forward animation to place holder avatar.
				if (_placeholderRef != null)
				{
					if (_placeholderRef.isReady)
					{
						var placeholderAvatar = _placeholderRef.GetPlaceholderAvatar();
						if (placeholderAvatar != null)
						{
							isPlayingAnimation = true;
							placeholderAvatar.PlayAnimation(name, loopTime);
						}
					}
				}
				else
				{
					// Forward to self avatar.
					if (_rmiObject != null)
					{
						isPlayingAnimation = true;

						if (this.entity != null)
						{
							var bodyAnimController = this.entity.bodyAnimController;
							if (bodyAnimController != null)
							{
								var animLayer = bodyAnimController.GetAnimationLayerByName(layerName);
								if (animLayer != null)
								{
									animLayer.PlayAnimationClip(name, loopTime, 1.0f, 0f);
									return;
								}
							}
						}
					}
					else
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, "PicoAvatar.PlayAnimation need lod ready!");
					}
				}
			}

			/// <summary>
			/// Helper method to stop animation in default animation layer of "ActionLayer".
			/// </summary>
			/// <param name="immediately">Whether to play immediately</param>
			/// <param name="layerName">Layer name</param>
			public void StopAnimation(bool immediately = true, string layerName = "ActionLayer")
			{
				if (isPlayingAnimation)
				{
					isPlayingAnimation = false;

					// Forward to self avatar.
					if (this.entity != null)
					{
						var bodyAnimController = this.entity.bodyAnimController;
						if (bodyAnimController != null)
						{
							var animLayer = bodyAnimController.GetAnimationLayerByName(layerName);
							if (animLayer != null)
							{
								animLayer.StopAnimation(0.0f);
							}
						}
					}
				}
			}

			/// <summary>
			/// Get all animation names in a json string
			/// </summary>
			/// <returns>json string</returns>
			public string GetAnimationNames()
			{
				if (_rmiObject != null)
				{
					return _rmiObject.GetAnimationNames();
				}

				return "";
			}

			/// <summary>
			/// Loading the external zip file exported through the "Animation Editor" is the premise of playing the external skeleton animation
			/// </summary>
			/// <remarks>
			/// Usually the android side often uses Application.persistentDataPath
			/// Application.persistentDataPath points to /storage/emulated/0/Android/data/<packagename>/files
			/// on most devices (some older phones might point to location on SD card if present),
			/// the path is resolved using android.content.Context.getExternalFilesDir.
			/// </remarks>
			/// <param name="folderPath">Animation zip path(IO Readable Absolute Path)</param>
			public void LoadAllAnimationsExtern(string folderPath)
			{
				//List<string> filePaths = new List<string>();
				string fileType = "*.zip";
				string[] filePaths = Directory.GetFiles(folderPath, fileType);
				for (int i = 0; i < filePaths.Length; i++)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append("[");

					using (ZipArchive archive = new ZipArchive(File.OpenRead(filePaths[i]), ZipArchiveMode.Read))
					{
						int entryCount = 0;
						foreach (ZipArchiveEntry entry in archive.Entries)
						{
							if (entryCount > 0)
							{
								sb.Append(",");
							}

							sb.Append("\"");
							sb.Append(entry.FullName);
							sb.Append("\"");

							entryCount += 1;
						}
					}

					sb.Append("]");
					string animationPathsJson = sb.ToString();
					LoadAnimationsExtern(filePaths[i], animationPathsJson);
				}
			}

			/// <summary>
			/// Add animation set by its id
			/// </summary>
			/// <param name="animationSetId">animationSetId</param>
			public void AddAnimationSet(string animationSetId)
			{
				if (_rmiObject != null)
				{
					_rmiObject.AddAnimationSet(animationSetId);
				}
			}

			/// <summary>
			/// Remove animation set by its id
			/// </summary>
			/// <param name="animationSetId">animationSetId</param>
			/// <returns>true if remove</returns>
			public bool RemoveAnimationSet(string animationSetId)
			{
				if (_rmiObject != null)
				{
					return _rmiObject.RemoveAnimationSet(animationSetId);
				}

				return false;
			}

			/// <summary>
			/// Load animation extern
			/// </summary>
			/// <param name="assetBundlePath">assetBundle Path</param>
			/// <param name="animationPathsJson">animation Paths</param>
			public void LoadAnimationsExtern(string assetBundlePath, string animationPathsJson)
			{
				if (_rmiObject != null)
				{
					_rmiObject.LoadAnimationsExtern(assetBundlePath, animationPathsJson);
				}
			}

			public UnityAction<string, string> OnLoadAnimationsExternComplete;

			/// <summary>
			/// Use LoadAllAnimationsExtern to load external animation callback events asynchronously,
			/// which is used to initialize animation related after loading is completed
			/// </summary>
			/// <param name="ab">AvatarAssetBundle</param>
			/// <param name="animationPathsJson">animation Paths</param>
			public void LoadAnimationsFromAssetBundle(AvatarAssetBundle ab, string animationPathsJson)
			{
				if (_rmiObject != null)
				{
					_rmiObject.LoadAnimationsFromAssetBundle(ab, animationPathsJson);
				}
			}

			/// <summary>
			/// Remove animtion by name
			/// </summary>
			/// <param name="name">Animation name to remove</param>
			public void RemoveAnimation(string name)
			{
				if (_rmiObject != null)
				{
					_rmiObject.RemoveAnimation(name);
				}
			}

			#endregion


			/// <summary>
			/// Force update lod
			/// </summary>
			/// <remarks>
			/// Invoked by sdk user to force update lod when teleport happens
			/// </remarks>
			public void ForceUpdateLodLevels()
			{
				// If force lod level, no need to update.
				if (PicoAvatarApp.instance.lodSettings.forceLodLevel == AvatarLodLevel.Invalid && isAnyEntityReady)
				{
					// Forward to self avatar.
					if (_rmiObject != null)
					{
						_rmiObject.ForceUpdateLod();
					}
				}
			}

			/// <summary>
			/// Force update the native states. normally when animation played and want to effective immediately,
			/// should invoke the method since it is hard to assure update orders among Unity objects.
			/// </summary>
			public void ForceUpdate()
			{
				// 1 millsecond after last frame.
				var newTime = PicoAvatarApp.instance.lastUpdateTime + 0.001f;
				PreUpdateFrame(newTime);

				// Notify js to update.
				if (_rmiObject != null)
				{
					_rmiObject.ForceUpdate(0);
				}

				// Post update.
				PostUpdateFrame(newTime);

				// Update render data from native part.
				if (entity != null)
				{
					// Update simulation render data such as bs weights.
					entity.UpdateSimulationRenderData(0.001f);

					// Update skeleton data.
					var activeLod = entity.GetCurrentAvatarLod();
					if (activeLod != null && activeLod.avatarSkeleton != null)
					{
						activeLod.avatarSkeleton.UpdateUnityTransformsFromNative();
					}
				}
			}

			/// <summary>
			/// Adds entity ready callback
			/// </summary>
			/// <remarks>
			/// If entity is ready, callback will be inovked immediately, otherwise will be queued and invoked later
			/// </remarks>
			/// <param name="entityReadyCallback">avatar entity loaded callback</param>
			public void AddFirstEntityReadyCallback(System.Action<PicoAvatar, AvatarEntity> entityReadyCallback)
			{
				if (entity != null && this.isAnyEntityReady)
				{
					entityReadyCallback(this, entity);
				}
				else
				{
					if (_entityReadyCallbacks == null)
					{
						_entityReadyCallbacks = new List<System.Action<PicoAvatar, AvatarEntity>>();
					}

					// Fixed by Yagiz Hatay, thanks!
					_entityReadyCallbacks.Add(entityReadyCallback);
				}
			}

			/// <summary>
			/// Compact memory with flags
			/// </summary>
			/// <param name="flags">Bits from AvatarMemoryCompactFlags</param>
			public void CompactMemory(ulong flags)
			{
				pav_AvatarInstance_CompactMemory(_nativeHandle, flags);
			}

			//do not use
			public AvatarEditState GetAvatarEditState()
			{
				if (_avatarEditState == null && isAnyEntityReady && _rmiObject != null)
				{
					//
					_rmiObject.InitializeEdit();
					//
					_avatarEditState = new AvatarEditState(this);
				}

				return _avatarEditState;
			}


			/// <summary>
			/// Gets avatar specification from native object
			/// </summary>
			/// <returns> avatar specification json</returns>
			public string GetAvatarSpecification()
			{
				if (_rmiObject != null)
				{
					return _rmiObject.GetAvatarSpecText();
				}

				return "";
			}

			#endregion


			#region Private Fields

			// Avatar id.
			private string _avatarId;

			//
			private NativeCall_Avatar _rmiObject;

			// Current state.
			private State _curState = State.None;

			// Whether native object is active.default is visible.
			private bool _isNativeVisible = true;

			private Vector3 _lastNativePosition = Vector3.zero;
			private Quaternion _lastNativeRotation = Quaternion.identity;

			// Effect kind lables which system effect to employ. outline/transparent fade/...
			private AvatarEffectKind _avatarEffectKind = AvatarEffectKind.None;

			// Copied place holde enity. share mesh/material/animation with place holder.
			private PicoAvatarPlaceholderRef _placeholderRef = null;

			// Callback list for first entity ready event. it will be set to null when any entity lod ready.
			private List<System.Action<PicoAvatar, AvatarEntity>> _entityReadyCallbacks = null;

			//
			private AvatarEditState _avatarEditState = null;

            [HideInInspector]
            // public GameObject[] criticalJointObjects;
			public List<GameObject> criticalJointObjects = new List<GameObject>();

            private XForm[] _jointXForms;

			private JointType[] _criticalJoints;

			private bool _criticalJointsDirty = true;

			// Native handle.
			private System.IntPtr _nativeHandle;
			private uint _nativeAvatarId = 0;

			// Avatar manager that create the object.
			private PicoAvatarManager _avatarManager = null;

			#endregion


			#region Framework Methods

			internal void Notify_Initialized(string userId, string avatarId_, AvatarLoadContext loadCtx)
			{
				this.userId = userId;
				// AvatarId_ is the id passed to js from AvatarLoadContext by developer. Later will be updated in Notify_SpecUpdated when actual id of avatar is determined.
				_avatarId = avatarId_;
				//
				capabilities = loadCtx.capabilities;
				depressMergeNodeTypes = loadCtx.depressMergeNodeTypes;

				//Compact memory in default.
				{
					ulong memoryCompactFlags = this.capabilities.memoryCompactFlags;
					if (PicoAvatarApp.instance.renderSettings.materialConfiguration != null
					    && !PicoAvatarApp.instance.renderSettings.materialConfiguration.need_BumpMap
					    && !PicoAvatarApp.instance.renderSettings.materialConfiguration.need_MetallicGlossMap)
					{
						memoryCompactFlags |= (ulong)AvatarMemoryCompactFlags.NoTex_NormalAndMetal;
					}

					this.CompactMemory((ulong)memoryCompactFlags);
				}
				//
				if (cachedTransform == null)
				{
					// Dont destory on load.
					GameObject.DontDestroyOnLoad(this.gameObject);
					//
					cachedTransform = this.transform;
					this.gameObject.layer = gameObject.layer;

					var go = new GameObject("Entity");
					entity = go.AddComponent<AvatarEntity>();

					var entityTransform = go.transform;
					entityTransform.SetParent(cachedTransform);
					entityTransform.localPosition = Vector3.zero;
					entityTransform.localRotation = Quaternion.identity;
					entityTransform.localScale = Vector3.one;
					entityTransform.gameObject.layer = gameObject.layer;

					var avatarManager = PicoAvatarManager.instance;

					// Use place holder.
					if (capabilities.enablePlaceHolder)
					{
						PicoPlaceholderAvatar placeHolderAvatar;

						//Local Mode Place Holder Avatar.
						if (capabilities.isMainAvatar)
						{
							placeHolderAvatar = loadCtx.placeHolderAvatar != null
								? loadCtx.placeHolderAvatar
								: avatarManager.builtinPlaceHolderLocalAvatar;
						}
						//Other Mode Place Holder Avatar.
						else
						{
							placeHolderAvatar = loadCtx.placeHolderAvatar != null
								? loadCtx.placeHolderAvatar
								: avatarManager.builtinPlaceHolderOtherAvatar;
						}

						if (placeHolderAvatar != null)
						{
							_placeholderRef = new PicoAvatarPlaceholderRef(userId, cachedTransform, placeHolderAvatar);
						}
					}
					else
					{
						// If main avatar need not place holder,just hide it.
						if (capabilities.isMainAvatar &&
						    avatarManager.builtinPlaceHolderLocalAvatar != null)
						{
							avatarManager.builtinPlaceHolderLocalAvatar.gameObject.SetActive(false);
						}
					}
				}

				entity.SetOwner(this);
			}

			/// <summary>
			/// Invoked from PicoAvatarManager when native avatar attached
			/// </summary>
			/// <param name="avatarId"></param>
			/// <param name="nativeAvatarId"></param>
			/// <exception cref="Exception"></exception>
			internal void Notify_AttachNativeAvatar(string avatarId, uint nativeAvatarId)
			{
				if (_nativeHandle != System.IntPtr.Zero)
				{
					NativeObject.ReleaseNative(ref _nativeHandle);
				}

				//
				_nativeHandle = pav_AvatarInstance_GetObject(nativeAvatarId);
				if (_nativeHandle == System.IntPtr.Zero)
				{
					throw new System.Exception(
						"Native AvatarInstance was not created when notify C# OnAttachNativeAvatar!");
				}

				_nativeAvatarId = nativeAvatarId;
				//
				_curState = State.Loading;

				if (_rmiObject != null)
				{
					throw new System.Exception("PicoAvatar has attached.");
				}

				//
				_rmiObject = new NativeCall_Avatar(this, nativeAvatarId);
				_rmiObject.Retain();

				//if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
				//{
				//    AvatarEnv.Log(string.Format("PicoAvatar attached to native avatar. userId: {0}, nativeAvatarId: {1}", userId, nativeAvatarId));
				//}
			}

			internal void Destroy()
			{
				if (_curState == State.Dead)
				{
					return;
				}

				// Set invisible flag.
				_isNativeVisible = false;
				//
				_curState = State.Dead;

				// Stop editor
				if (_avatarEditState != null)
				{
					_avatarEditState.Destroy();
					_avatarEditState = null;
				}

				// Destroy entity.
				if (entity != null)
				{
					entity.Destroy();
					entity = null;
				}

				// Replace 
				if (_placeholderRef != null)
				{
					_placeholderRef.Release();
					_placeholderRef = null;
				}

				// Notify callbacks.
				if (_entityReadyCallbacks != null)
				{
					var callbacks = _entityReadyCallbacks;
					_entityReadyCallbacks = null;
					foreach (var callback in callbacks)
					{
						callback(null, null);
					}
				}

				//
				if (_rmiObject != null)
				{
					_rmiObject.Release();
					_rmiObject = null;
				}

				//
				if (_nativeHandle != System.IntPtr.Zero)
				{
					NativeObject.ReleaseNative(ref _nativeHandle);
				}
			}

			private void OnDestroy()
			{
				// Avatar manager MUST be cleared before here.
				if (_avatarManager != null)
				{
					AvatarEnv.Log(DebugLogMask.GeneralWarn,
						String.Format(
							"PicoAvatar should be destroy and unloaded with AvatarManager.UnloadAvatar! userId: {0}",
							this.userId));

					_avatarManager.UnloadAvatar(this.userId);
				}

				Destroy();
			}

			/// <summary>
			/// When first AvatarEntity finished loading, native notify the PicoAvatar to update the AvatarEntity
			/// Each PicoAvatar may holds one more AvatarEntity at the same time
			/// </summary>
			/// <param name="nativeEntityId"></param>
			/// <param name="curLodLevel"></param>
			/// <param name="avatarSex"></param>
			/// <param name="styleName"></param>
			internal void Notify_AvatarEntityLoaded(uint nativeEntityId, AvatarLodLevel curLodLevel,
				AvatarSexType avatarSex, string styleName)
			{
				sex = avatarSex;
				this.avatarStyleName = styleName;

				// If previous avatar entity created, need destroy first.
				if (entity != null)
				{
					//
					if (entity.isAnyLodReady)
					{
						entity.Destroy();
					}

					//
					entity.CheckBuildFromNative(nativeEntityId, curLodLevel);
				}

				//
				_curState = State.Running;

				//
				UpdateNativeAvatarMovementWithUnityXForm();
			}

			internal void Notify_LoadAnimationsExternComplete(string assetBundlePath, string animationPathsJson)
			{
				OnLoadAnimationsExternComplete?.Invoke(assetBundlePath, animationPathsJson);
			}

			/// <summary>
			/// When Unity AvatarEntity and AvatarLod loaded and visible, notify the avatar
			/// </summary>
			/// <param name="avatarEntity"></param>
			/// <param name="curLodLevel"></param>
			internal virtual void Notify_AvatarEntityLodReadyToShow(AvatarEntity avatarEntity,
				AvatarLodLevel curLodLevel)
			{
				//
				isAnyEntityReady = true;

				// Replace 
				if (_placeholderRef != null)
				{
					_placeholderRef.Release();
					_placeholderRef = null;
					//
					isPlayingAnimation = false;
				}

				if (entity.bodyAnimController != null)
				{
					entity.bodyAnimController.Initialize(entity.nativeEntityId);
					entity.CalcuateHeadToBodyRatio();
				}

				/*// Force use set avatar height. in video camera.
				if (PicoAvatarApp.instance.IKSettings.forceAvatarHeight > 0.01)
				{
				    this.SetAvatarHeight(PicoAvatarApp.instance.IKSettings.forceAvatarHeight);
				}
				*/

				//
				UpdateNativeAvatarMovementWithUnityXForm();

				//build initial critical joints
                var length = 0;
				if (_criticalJoints != null) 
				{
					length = _criticalJoints.Length;
				}
				
                if (length > 0 && _criticalJointsDirty)
                {
					foreach(var go in criticalJointObjects)
					{
						Destroy(go);
					}
					criticalJointObjects.Clear();
					var rootTran = entity.transform;
					for (var i = 0; i < length; ++i)
					{
						GameObject go = new GameObject(_criticalJoints[i].ToString());
						criticalJointObjects.Add(go);
						var curTran = go.transform;
						curTran.parent = rootTran;
						curTran.localScale = Vector3.one;
					}						
                }
				_criticalJointsDirty = false;
				//force update skeleton from native
                isAvatarLodDirty = true;


				//
				if (_entityReadyCallbacks != null)
				{
					var callbacks = _entityReadyCallbacks;
					_entityReadyCallbacks = null;
					foreach (var callback in callbacks)
					{
						callback(this, entity);
					}
				}

				// Update immediately.
				if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
				{
					var pos = this.transform.position;
					AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format(
						"C# AvatarEntity Ready. position:({0},{1},{2}) active:{3}",
						pos.x, pos.y, pos.z, this.gameObject.activeInHierarchy));
				}
			}


            public void AddCriticalJoint(JointType jointType)
            {
                GameObject go = GetJointObject(jointType);
                if (!go)
                {
                    List<JointType> joints = _criticalJoints.ToList();
                    joints.Add(jointType);
                    _criticalJoints = joints.ToArray();

                    GameObject jointObject = new GameObject(jointType.ToString());
                    jointObject.transform.parent = entity.transform;
                    jointObject.transform.localScale = Vector3.one;

                    criticalJointObjects.Add(jointObject);
                }
            }

            /// <summary>
            /// Notification from js when failed to load avatar entity lod level
            /// </summary>
            /// <param name="nativeEntityId"></param>
            /// <param name="lodLevel"></param>
            internal void Notify_OnEntityLodLevelLoadFailed(uint nativeEntityId, AvatarLodLevel lodLevel)
            {
                // Log the failed message.
				if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
				{
					AvatarEnv.Log(DebugLogMask.AvatarLoad,
						String.Format("Failed to load avatar. userId:{0} lodLevel:{1}", this.userId, lodLevel));
				}

				// Notify callbacks.
				if (_entityReadyCallbacks != null)
				{
					var callbacks = _entityReadyCallbacks;
					_entityReadyCallbacks = null;
					foreach (var callback in callbacks)
					{
						callback(null, null);
					}
				}
			}

			/// <summary>
			/// Pre-update for this frame. mainly update data of native part with unity objects
			/// </summary>
			/// <param name="gameTime"></param>
			/// <returns>true if render data of the avatar should be updated</returns>
			internal bool PreUpdateFrame(float gameTime)
			{
				// Update move.
				UpdateNativeAvatarMovementWithUnityXForm();

				// If nativeEntityId is zero, means has been destroyed and no need to update more.
				if (entity != null && entity.nativeEntityId != 0)
				{
					return entity.PreUpdateFrame() && !depressUpdateSimulationRenderData;
				}

				return false;
			}

			/// <summary>
			/// Post-update for this frame. mainly update data of unity object with native part after core calculation finished
			/// </summary>
			/// <param name="gameTime"></param>
			internal void PostUpdateFrame(float gameTime)
			{
				if (entity != null && entity.nativeEntityId != 0 && entity.isAnyLodReady) //这个为0导致不更新？
				{
					entity.PostUpdateFrame(gameTime);

                    if (this._criticalJoints != null && this._criticalJoints.Length > 0 && entity.bodyAnimController != null)
                    {
						//update critical joint objects
	                    if (_jointXForms == null || _jointXForms.Length != _criticalJoints.Length)
	                    {
		                    _jointXForms = new XForm[_criticalJoints.Length];
		                    
		                    if (_criticalJointsDirty)
		                    {
								//remove game objects
								foreach(var go in criticalJointObjects)
								{
									Destroy(go);
								}
			                    criticalJointObjects.Clear();
			                    for (var i = 0; i < _criticalJoints.Length; ++i)
			                    {
									GameObject go =  new GameObject(_criticalJoints[i].ToString());
				                    criticalJointObjects.Add(go);
				                    go.transform.parent = entity.transform;
			                    }
								_criticalJointsDirty = false;
		                    }
	                    }

						//update transform
	                    entity.bodyAnimController.GetJointXForms(_criticalJoints, ref _jointXForms);
                        for (var i = 0; i < _criticalJoints.Length; ++i)
                        {
	                        criticalJointObjects[i].transform.localPosition = _jointXForms[i].position;
	                        criticalJointObjects[i].transform.localRotation = _jointXForms[i].orientation;
	                        criticalJointObjects[i].transform.localScale = _jointXForms[i].scale;
                        }
                    }
                }
            }

			/// <summary>
			/// Notification from js when AvatarSpecification initialized
			/// </summary>
			/// <param name="avatarId"></param>
			/// <param name="jsonData"></param>
			internal void Notify_SpecUpdated(string avatarId, string jsonData)
			{
				// Actual avatar id.
				_avatarId = avatarId;
				//_jsonSpecData = jsonData;
			}

			/// <summary>
			/// Check whether gameObject is active. update native state
			/// </summary>
			/// <returns></returns>
			internal bool CheckNativeVisible()
			{
				if (this.isActiveAndEnabled != _isNativeVisible)
				{
					_isNativeVisible = this.isActiveAndEnabled;

					if (_rmiObject != null)
					{
						_rmiObject.SetVisible(_isNativeVisible);
					}

					// Notify entity that invisible
					entity.OnParentVisiblilityChanged(_isNativeVisible);
				}

				return _isNativeVisible;
			}

			internal bool CheckNeedUpdateSimulationDataThisFrame()
            {
                return CheckNativeVisible() || isAvatarLodDirty;
            }

			/// <summary>
			/// Sets avatar manager
			/// </summary>
			/// <param name="avatarManager"></param>
			internal void SetAvatarManager(PicoAvatarManager avatarManager)
			{
				_avatarManager = avatarManager;
			}

			/// <summary>
			/// Update avatar native part movement with unity transform xfrom. check whether avatar moved, if moved should notify native part
			/// </summary>
			private void UpdateNativeAvatarMovementWithUnityXForm()
			{
				if (cachedTransform != null)
				{
					bool dirty = false;
					{
						var newPos = cachedTransform.position;
						var newOrientation = cachedTransform.rotation;

						//Check changed
						if (Vector3.SqrMagnitude(newPos - _lastNativePosition) >
						    PicoAvatarApp.instance.squaredPositionErrorThreshold ||
						    (Quaternion.Dot(newOrientation, _lastNativeRotation) < (1.0f -
							    PicoAvatarApp.instance.optimizationSettings.orientationErrorThreshold)))
						{
							dirty = true;
						}

						if (dirty)
						{
							//
							_lastNativePosition = newPos;
							_lastNativeRotation = newOrientation;
							//
							if (_nativeHandle != System.IntPtr.Zero)
							{
								XForm xform;
								xform.position = newPos;
								xform.orientation = newOrientation;
								xform.scale = cachedTransform.localScale;
								//
								pav_AvatarInstance_SetXForm(_nativeHandle, ref xform);
							}
						}
					}

					// Update place holder.
					if (_placeholderRef != null)
					{
						_placeholderRef.UpdateMovement();
					}
				}
			}

			/// <summary>
			/// Gets native transform xform
			/// </summary>
			/// <returns></returns>
			private XForm GetNativeXForm()
			{
				var ret = new XForm();
				pav_AvatarInstance_GetXForm(_nativeHandle, ref ret);
				return ret;
			}

			#endregion
		}
	}
}
