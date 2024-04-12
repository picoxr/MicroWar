using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// AvatarEntity encapsulates whole data of a character, e.g. skeleton,blendshapes,animation,render primitives etc
		/// The xform of AvatarEntity ONLY managed by SDK, application MUST NOT change it
		/// </summary>
		public partial class AvatarEntity : MonoBehaviour
		{
			#region Public Types

			#endregion


			#region Public Properties

			// Gets native handle.
			internal System.IntPtr nativeHandle
			{
				get => _nativeHandle;
			}

			/// <summary>
			/// Whether enity is destroyed.
			/// </summary>
			public bool isDestroyed
			{
				get => _isDestroyed;
			}

			/// <summary>
			/// User id of owner avatar.
			/// </summary>
			public string userId
			{
				get => _userId;
			}

			/// <summary>
			/// Entity id which is a local identification of avatar and is allocated by native system.
			/// </summary>
			public uint nativeEntityId
			{
				get => _nativeEntityId;
			}

			/// <summary>
			/// Owner avatar
			/// </summary>
			public PicoAvatar owner
			{
				get => _owner;
			}

			/// <summary>
			/// Current lod level.
			/// </summary>
			public AvatarLodLevel currentLodLevel
			{
				get => _activeLodLevel;
			}

			/// <summary>
			/// Whether any lod ready and can be rendered now.
			/// </summary>
			public bool isAnyLodReady { get; set; } = false;

			/// <summary>
			/// Whether is in camera frustum.
			/// </summary>
			public bool inCameraFrustum
			{
				get => _inCameraFrustum;
			}

			/// <summary>
			/// Avatar input targets setting for avatar ik and other usage. 
			/// Target values will be passed to avatar ik controller through device input reader.
			/// </summary>
			public AvatarIKTargetsConfig avatarIKTargetsConfig = new AvatarIKTargetsConfig();

			/// <summary>
			/// Left hand custom pose target.
			/// </summary>
			public AvatarCustomHandPose leftCustomHandPose = null;

			/// <summary>
			/// Right hand custom pose target.
			/// </summary>
			public AvatarCustomHandPose rightCustomHandPose = null;

			/// <summary>
			/// Whether to use custom InputActionProperty.
			/// </summary>
			public bool actionBasedControl = false;

			/// <summary>
			/// Input actions of position.
			/// </summary>
			public InputActionProperty[] positionActions;

			/// <summary>
			/// Input actions of rotation.
			/// </summary>
			public InputActionProperty[] rotationActions;

			/// <summary>
			/// Use XR inputAction to configure input instead XR device input.
			/// </summary>
			public InputActionProperty[] buttonActions;

			/// <summary>
			/// Gets body animation controller.
			/// </summary>
			public AvatarBodyAnimController bodyAnimController
			{
				get
				{
					if (_bodyAnimController == null)
					{
						// Only when lod ready
						if (!isAnyLodReady)
						{
							return null;
						}

						//
						var animHandler = pav_AvatarEntity_GetBodyAnimController(_nativeHandle);
						if (animHandler != System.IntPtr.Zero)
						{
							_bodyAnimController = new AvatarBodyAnimController(animHandler, this);
							_bodyAnimController.Retain();
						}
					}

					return _bodyAnimController;
				}
			}

			/// <summary>
			/// Gets device input reader.
			/// </summary>
			public IDeviceInputReader deviceInputReader
			{
				get { return _deviceInputReader; }
			}

			public PicoPlaybackPacketRecorder debugPlaybackRecorder;

			/// <summary>
			/// Last animation playback level.
			/// </summary>
			public RecordBodyAnimLevel lastAnimationPlaybackLevel { get; private set; } = RecordBodyAnimLevel.Count;

			/// <summary>
			/// Expression playback enable status.
			/// </summary>
			public bool expressionPlaybackEnabled { get; private set; } = false;

			/// <summary>
			/// Whether native updates have skipped this frame.
			/// </summary>
			public bool isNativeUpdateSkippedThisFrame { get; private set; } = false;

			/// <summary>
			/// Whether the animation of avatar is calculation by IK algorithm.
			/// </summary>
			public bool needIK { get; private set; } = false;


			public float headToBodyRatio { get; private set; } = 0.15f;

			#region Stats

			//
			private float _statsStartLoadTime;
			private float _statsFirstLodReadyTime = 0.0f;

			#endregion

			#endregion


			#region Public Events

			/// <summary>
			/// Event that avatar lod is ready.
			/// </summary>
			public UnityEvent OnAvatarLodReady { get; private set; } = new UnityEvent();

			/// <summary>
			/// Event that avatar entity will destroy.
			/// </summary>
			public UnityEvent OnAvatarWillUnBind { get; private set; } = new UnityEvent();

			#endregion


			#region Public Methods

			public AvatarEntity()
			{
				//
				if (PicoAvatarStats.instance != null)
				{
					PicoAvatarStats.instance.IncreaseInstanceCount(PicoAvatarStats.InstanceType.AvatarEnity);
				}
			}

			/// <summary>
			/// Current avatar lod. Maybe null
			/// </summary>
			/// <returns>avatar lod</returns>
			public AvatarLod GetCurrentAvatarLod()
			{
				return (uint)_activeLodLevel < (uint)AvatarLodLevel.Count ? _avatarLods[(int)_activeLodLevel] : null;
			}

			/// <summary>
			/// Current playback delay time of remote animation stream. It is used to show network quality
			/// Get avatar delay time from c++.
			/// </summary>
			/// <returns>Animation PlaybackDelayTime</returns>
			public double GetAnimationPlaybackDelayTime()
			{
				return pav_AvatarEntity_GetPing(_nativeHandle);
			}

			// Try get avatar entity with name id.
			internal static AvatarEntity GetAvatarEntityWithNativeID(uint nativeEntityId)
			{
				if (_allAvatarEntities.TryGetValue(nativeEntityId, out AvatarEntity avatarEntity))
				{
					return avatarEntity;
				}

				return null;
			}

			// Create entity instance with entity id which is allocated by native module.
			internal bool CheckBuildFromNative(uint nativeEntityId, AvatarLodLevel lodLevel)
			{
				if (_nativeEntityId != 0 && _nativeEntityId != nativeEntityId)
				{
					throw new System.ArgumentException("Native entity id conflicts!");
				}

				//
				if (_rmiObject == null)
				{
					_rmiObject = new NativeCall_AvatarEntity(this, nativeEntityId);
					_rmiObject.Retain();
				}

				//
				if (_nativeHandle == System.IntPtr.Zero)
				{
					_nativeHandle = pav_AvatarEntity_GetObject(nativeEntityId);
				}
				else
				{
					// Try load more lods.
					Notify_LodChanged(lodLevel);
					return true;
				}

				if (_nativeHandle != System.IntPtr.Zero && !_allAvatarEntities.ContainsKey(nativeEntityId))
				{
					// Keep store the new native handle.
					_nativeEntityId = nativeEntityId;

					// Add to global table.
					_allAvatarEntities.Add(nativeEntityId, this);

					//
					Notify_LodChanged(lodLevel);
					return true;
				}
				else
				{
					Debug.LogError("BindEntity failed. NativeHandle is invalid or allready been used.");
					return false;
				}
			}

			/// <summary>
			/// Force avatar entity to target lodLevel
			/// </summary>
			/// <param name="lodLevel">Target lodLevel</param>
			public void ForceLod(AvatarLodLevel lodLevel)
			{
				_forceLodLevel = AvatarLodLevel.Invisible;
				//
				if (_rmiObject != null)
				{
					_rmiObject.SetForceLodLevel(lodLevel);
				}
			}

			/// <summary>
			/// Set material property value.
			/// </summary>
			/// <param name="primitiveId">AvatarPrimitive with nodeId primitiveId</param>
			/// <param name="name">Material property name</param>
			/// <param name="value">Vector4 value</param>
			internal void SetMaterialVec4(uint primitiveId, string name, Vector4 value)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetMaterialVec4(primitiveId, name, value);
				}
			}

			/// <summary>
			/// Gets native transform xform
			/// </summary>
			/// <returns>Xform of transform</returns>
			public XForm GetNativeXForm()
			{
				var ret = new XForm();
				if (pav_AvatarEntity_GetXForm(_nativeHandle, ref ret) != NativeResult.Success)
				{
					ret.position = Vector3.zero;
					ret.scale = Vector3.one;
					ret.orientation = Quaternion.identity;
				}

				return ret;
			}

			public bool SetCustomHand(HandSide side, GameObject handSkeleton, GameObject handPose, Vector3 handUp,
				Vector3 handForward, Vector3 wristOffset)
			{
				if (_nativeHandle == System.IntPtr.Zero || side == HandSide.Invalid || _owner == null)
				{
					return false;
				}

				pav_AvatarEntity_SetCustomHandSide(_nativeHandle, (uint)side);
				if (side == HandSide.Left)
				{
					leftCustomHand = true;
					leftCustomHandPose = new AvatarCustomHandPose();
					leftCustomHandPose.Initialize(side, handSkeleton, handPose, handUp, handForward, wristOffset,
						_owner.cachedTransform);
				}
				else if (side == HandSide.Right)
				{
					rightCustomHand = true;
					rightCustomHandPose = new AvatarCustomHandPose();
					rightCustomHandPose.Initialize(side, handSkeleton, handPose, handUp, handForward, wristOffset,
						_owner.cachedTransform);
				}

				return true;
			}

			/// <summary>
			/// Clear custom hand already set.
			/// </summary>
			/// <param name="side">Which hand to clear</param>
			public void ClearCustomHand(HandSide side)
			{
				if (side == HandSide.Left)
				{
					leftCustomHand = false;
					if (leftCustomHandPose != null)
					{
						leftCustomHandPose.StopCustomHand(bodyAnimController);
					}

					leftCustomHandPose = null;
				}
				else if (side == HandSide.Right)
				{
					rightCustomHand = false;
					if (rightCustomHandPose != null)
					{
						rightCustomHandPose.StopCustomHand(bodyAnimController);
					}

					rightCustomHandPose = null;
				}

				pav_AvatarEntity_ClearCustomHandSide(_nativeHandle, (uint)side);
			}


			/// <summary>
			/// Sets avatar bunch to replace the avatar.  The lod level to replace is set in AvatarCapabilities.avatarBunchLodLevel
			/// </summary>
			/// <param name="avatarBunch">AvatarBunch to use</param>
			public void SetAvatarBunch(AvatarBunch avatarBunch)
			{
				if (_avatarBunchItem != null)
				{
					if (_avatarBunchItem.avatarBunch == avatarBunch)
					{
						return;
					}

					_avatarBunchItem.RemoveFromAvatarBunch();
					_avatarBunchItem = null;
				}

				// Sets new avatar bunch.
				_avatarBunch = avatarBunch;

				if (avatarBunch != null &&
				    this._activeLodLevel >= AvatarLodLevel.Lod0 &&
				    this._activeLodLevel >= this.owner.capabilities.avatarBunchLodLevel)
				{
					_avatarBunchItem = avatarBunch.AddAvatarBunchItem(this);
				}
			}

			/// <summary>
			/// Gets avatar runtime scale caused by changing avatar height.
			/// </summary>
			/// <returns>Avatar scale</returns>
			public Vector3 GetAvatarScale()
			{
				if (_bodyAnimController != null)
				{
					return _bodyAnimController.avatarScale;
				}

				return Vector3.one;
			}

			/// <summary>
			/// Scale xr root to align user arm span with avtar arm span. 
			/// Only applicable when VR controllers are used as the IK target.
			public void AlignAvatarArmSpan()
			{
				if (_bodyAnimController != null)
				{
					//avatar arm span
					float avatarArmSpan = _bodyAnimController.bipedIKController.GetAvatarArmSpan();

					//trick: approximate avatar arm span with offset
					float leftOffsetLength = GetAvatarScale().x * avatarIKTargetsConfig.leftHandPositionOffset.z;
					float rightOffsetLength = GetAvatarScale().x * avatarIKTargetsConfig.rightHandPositionOffset.z;
					avatarArmSpan += leftOffsetLength;
					avatarArmSpan += rightOffsetLength;

					//real user arm span
					float realUserArmSpan = deviceInputReader.deviceArmSpan;

					Transform xrRoot = avatarIKTargetsConfig.xrRoot;
					if (xrRoot != null && realUserArmSpan > 0.1f * xrRoot.localScale.x && avatarArmSpan > 1e-6f)
					{
						xrRoot.localScale *= avatarArmSpan / realUserArmSpan;
						var bipedIKController = _bodyAnimController.bipedIKController;
						if (bipedIKController != null)
						{
							bipedIKController.controllerScale = xrRoot.localScale.x;
						}
					}
				}
			}

			#endregion


			#region Public Skeleton Record/Playback

			/// <summary>
			/// Gets the avatar data packet of the current action synchronization, which is used for network action data transmission.
			/// Used in conjunction with ApplyPacket
			/// </summary>
			/// <returns>Data packet</returns>
			public MemoryView GetFixedPacketMemoryView()
			{
				if (_shotPacketMemoryView == null && _nativeHandle != System.IntPtr.Zero)
				{
					_shotPacketMemoryView = new MemoryView(pav_AvatarEntity_GetFixedPacketMemoryView(_nativeHandle));
					_shotPacketMemoryView.Retain();
				}

				return _shotPacketMemoryView;
			}

			/// <summary>
			/// Build a animation playback packet. Data will be wrote to _shotPacketMemoryView which is a shared memory
			/// Need not be invoked any more, directly access  GetFixedPacketMemoryView to received cached data.
			/// </summary>
			/// <param name="timestamp">Timestamp to record data</param>
			/// <exception cref="NullReferenceException">native AvatarEntity destroyed</exception>
			public void RecordPacket(double timestamp)
			{
				if (_nativeHandle == System.IntPtr.Zero)
				{
					throw new NullReferenceException("native AvatarEntity destroyed.");
				}

				if (_animationRecordPacketInterval < 0.0001f)
				{
					pav_AvatarEntity_RecordAnimationPacket(_nativeHandle, timestamp);
				}

				//
				if (debugPlaybackRecorder != null)
				{
					debugPlaybackRecorder.RecordPacket(GetFixedPacketMemoryView());
				}
			}

			/// <summary>
			/// Set packet apply mode, 0 means write to animation system, 1 means update directly to skeleton transform or bs weights table
			/// </summary>
			/// <param name="packetApplyMode">Packet apply mode</param>
			public void SetPacketApplyMode(RecordPacketApplyMode packetApplyMode = RecordPacketApplyMode.Invalid)
			{
				_packetApplyMode = packetApplyMode;
				// Fallback to app value is invalid is set
				if (_packetApplyMode == RecordPacketApplyMode.Invalid)
				{
					_packetApplyMode = PicoAvatarApp.instance.netBodyPlaybackSettings.packetApplyMode;
				}

				Debug.Assert(_packetApplyMode != RecordPacketApplyMode.Invalid,
					"Record packet apply mode cannot be invalid");
				pav_AvatarEntity_SetPacketApplyMode(nativeHandle, (int)_packetApplyMode);
			}

			/// <summary>
			/// Queue remote animation playback packet. It will be processed during SyncNet....
			/// </summary>
			/// <param name="newPacketData">get value from GetFixedPacketMemoryView</param>
			public int ApplyPacket(MemoryView newPacketData)
			{
				//
				if (debugPlaybackRecorder != null)
				{
					debugPlaybackRecorder.RecordPacket(GetFixedPacketMemoryView());
				}

				//
				return pav_AvatarEntity_ApplyAnimationPacket(_nativeHandle, newPacketData.nativeHandle);
			}

			/// <summary>
			/// Set record packet config. Should be invoked before RecordPacket.
			/// </summary>
			internal void SetAnimationRecordPacketConfig()
			{
				var app = PicoAvatarApp.instance;
				_animationRecordPacketInterval = app.netBodyPlaybackSettings.recordInterval;

				var config = new AnimationRecordConfig();
				config.version = 0;
				config.recordVersion = app.netBodyPlaybackSettings.recordVersion;
				config.recordInterval = _animationRecordPacketInterval;

				pav_AvatarEntity_SetAnimationRecordConfig(_nativeHandle, ref config);
			}

			// Sets animation playback configuration.
			private void SetAnimationPlaybackConfig()
			{
				var app = PicoAvatarApp.instance;

				var config = new AnimationPlaybackConfig();
				config.version = 0;
				config.maxPacketCountPerFrame = app.netBodyPlaybackSettings.maxPacketCountPerFrame;
				config.avgDelayTime = app.netBodyPlaybackSettings.avgPlaybackDelayTime;
				config.maxPlaybackSpeedRatio = app.netBodyPlaybackSettings.maxPlaybackSpeedRatio;
				config.minPlaybackSpeedRatio = app.netBodyPlaybackSettings.minPlaybackSpeedRatio;
				config.playbackInterval = app.netBodyPlaybackSettings.playbackInterval;

				pav_AvatarEntity_SetAnimationPlaybackConfig(_nativeHandle, ref config);
			}

			/// <summary>
			/// Sets local playback configuration
			/// </summary>
			private void SetLocalAnimationConfig()
			{
				var app = PicoAvatarApp.instance;
				var config = new LocalAnimationConfig();
				config.version = 0;
				config.ikInterpDelayTime = app.optimizationSettings.IKInterpDelayTime;

				pav_AvatarEntity_SetLocalAnimationConfig(_nativeHandle, ref config);
			}

			/// <summary>
			/// Sets face expression packet recording configuration
			/// </summary>
			private void SetFaceExpressionRecordConfig()
			{
				var app = PicoAvatarApp.instance;
				var config = new FaceExpressionRecordConfig();
				config.version = 0;
				config.recordVersion = app.netFaceExpressionPlaybackSettings.recordVersion;

				pav_AvatarEntity_SetFaceExpressionRecordConfig(_nativeHandle, ref config);
			}

			/// <summary>
			/// Sets face expression packet playback configuration
			/// </summary>
			private void SetFaceExpressionPlaybackConfig()
			{
				var app = PicoAvatarApp.instance;

				var config = new FaceExpressionPlaybackConfig();
				config.version = 0;
				config.maxPacketCountPerFrame = app.netFaceExpressionPlaybackSettings.maxPacketCountPerFrame;
				config.avgDelayTime = app.netFaceExpressionPlaybackSettings.avgPlaybackDelayTime;
				config.maxPlaybackSpeedRatio = app.netFaceExpressionPlaybackSettings.maxPlaybackSpeedRatio;
				config.minPlaybackSpeedRatio = app.netFaceExpressionPlaybackSettings.minPlaybackSpeedRatio;

				pav_AvatarEntity_SetFaceExpressionPlaybackConfig(_nativeHandle, ref config);
			}

			#endregion


			#region Public Face Expression animation Record/Playback

			/// <summary>
			/// Gets the avatar face expression packet of the current action synchronization,
			/// which is used for network action data transmission. 
			/// </summary>
			/// <remarks>
			///     Used in conjunction with ApplyFaceExpressionPacket remotely
			/// </remarks>
			/// <returns>FaceExpressionPacket MemoryView</returns>
			public MemoryView GetFixedFaceExpressionPacketMemoryView()
			{
				if (_ftPacketMemoryView == null && _nativeHandle != System.IntPtr.Zero)
				{
					_ftPacketMemoryView =
						new MemoryView(pav_AvatarEntity_GetFixedFaceExpressionPacketMemoryView(_nativeHandle));
					_ftPacketMemoryView.Retain();
				}

				return _ftPacketMemoryView;
			}


			public MemoryView RecordFaceExpressionPacket(double timeStamp)
			{
				if (_nativeHandle != System.IntPtr.Zero)
				{
					// Done in main thread since small size.
					pav_AvatarEntity_RecordFaceExpressionPacket(_nativeHandle, timeStamp);
					//
					var packetMemoryView = GetFixedFaceExpressionPacketMemoryView();
					// Update new size.
					packetMemoryView.GetSize(true);
					//
					return packetMemoryView;
				}

				return null;
			}

			/// <summary>
			/// Queue remote face expression animation playback packet
			/// </summary>
			/// <param name="newPacketData">Packet from remote avatar. The content will be copied in c++ part,
			/// thus application can reuse the object to transfer packet data</param>
			public int ApplyFaceExpressionPacket(MemoryView newPacketData)
			{
				// Apply in main thread synchronously since small size.
				return pav_AvatarEntity_ApplyFaceExpressionPacket(_nativeHandle, newPacketData.nativeHandle);
			}

			#endregion


			#region Protected Methods

			/// <summary>
			/// Build avatar lods.
			/// </summary>
			/// <param name="curLodLevel"></param>
			/// <returns></returns>
			protected bool CheckBuildAvatarLods(AvatarLodLevel curLodLevel)
			{
				// Check to load current lod level mesh.
				if ((uint)curLodLevel < (uint)AvatarLodLevel.Count)
				{
					if (_avatarLods[(int)curLodLevel] == null)
					{
						// Reference to native handle has been increased.
						var avatarLodHandle = pav_AvatarEntity_GetAvatarLod(_nativeHandle, (uint)curLodLevel);
						//
						if (avatarLodHandle != System.IntPtr.Zero)
						{
							var go = new GameObject(string.Format("Lod{0}", curLodLevel));
							go.hideFlags = HideFlags.DontSave;
							go.layer = gameObject.layer;

							// Inactive in default 
							go.SetActive(false);

							// Add game object to entity.
							go.transform.parent = this.transform;
							go.transform.localPosition = Vector3.zero;
							go.transform.localRotation = Quaternion.identity;
							go.transform.localScale = Vector3.one;

							var avatarLod = go.AddComponent<AvatarLod>();
							// Attach 
							avatarLod.AttachNative(this, avatarLodHandle, (AvatarLodLevel)curLodLevel);
							//
							_avatarLods[(int)curLodLevel] = avatarLod;
							//
						}
						else
						{
							return false;
						}
					}

					return true;
				}

				return false;
			}

			//Destroy all avatar lods.
			protected void DestroyAvatarLods()
			{
				for (uint i = 0; i < (uint)AvatarLodLevel.Count; ++i)
				{
					if (_avatarLods[i] != null)
					{
						// Manually destroy AvatarLods.
						_avatarLods[i].Destroy();
						//
						GameObject.Destroy(_avatarLods[i].gameObject);
						_avatarLods[i] = null;
					}
				}

				//
				_activeLod = null;
				_activeLodLevel = AvatarLodLevel.Invisible;
				_deactivingLodLevel = AvatarLodLevel.Invisible;
			}

			/// <summary>
			/// Unity framework invoke the method when scene object destroyed.
			/// Derived class MUST invoke the method if override it.
			/// </summary>
			[UnityEngine.Scripting.PreserveAttribute]
			protected virtual void OnDestroy()
			{
				Destroy();
			}

			#endregion


			#region Private Fields

			// Whether has been destroyed.
			private bool _isDestroyed = false;

			// User id.
			private string _userId = "0";

			// Ownder avatar
			private PicoAvatar _owner = null;

			// Entity id.
			private uint _nativeEntityId = 0;

			// Active lod.
			private AvatarLod _activeLod = null;

			// Current lod level
			private AvatarLodLevel _activeLodLevel = AvatarLodLevel.Invalid;

			// Currently deactiving lode leve. only when new lod level built, can hide privious lod leve.
			private AvatarLodLevel _deactivingLodLevel = AvatarLodLevel.Invalid;

			// Force lod leve.
			private AvatarLodLevel _forceLodLevel = AvatarLodLevel.Invisible;

			//
			private NativeCall_AvatarEntity _rmiObject;

			// Avatar lods.
			private AvatarLod[] _avatarLods = new AvatarLod[(int)AvatarLodLevel.Count];

			// Native handle.
			private System.IntPtr _nativeHandle = IntPtr.Zero;

			// Desired playback level.
			RecordBodyAnimLevel _desiredPlaybackLevel;

			// Body animation controller.
			private AvatarBodyAnimController _bodyAnimController;

			// Device input reader
			private IDeviceInputReader _deviceInputReader;

			// Net synchronization packet. Only used for main avatar that need record animation packet from.
			private MemoryView _shotPacketMemoryView;

			// Net synchronization packet. Only used for main avatar that need record face expression animation packet from.
			private MemoryView _ftPacketMemoryView;

			// Interval to record packet in seconds.
			private float _animationRecordPacketInterval = -1.0f;

			// Global table for avatar entities. key is native entity id.
			static Dictionary<uint, AvatarEntity> _allAvatarEntities = new Dictionary<uint, AvatarEntity>();

			private TypeID2NameIDTable _faceBSChannelIDTable;
			private IDParameterTable _faceBSIDParamTable;

			private bool leftCustomHand = false;

			private bool rightCustomHand = false;


			// Last time that skip updation enen the entity is visible.
			private float _lastSkipVisibleUpdateTime = 0.0f;

			// Last time that skip updation when avatar is culled by camera frustum.
			private float _lastSkipInvisibleUpdateTime = 0.0f;

			// Whether apply packet data directly to skeleton transform or bs weights table
			// Packet apply mode, 0 means write to animation system, 1 means update directly to skeleton transform or bs weights table
			private RecordPacketApplyMode _packetApplyMode = RecordPacketApplyMode.Invalid;

			// A client to avatar bunch.
			private AvatarBunchItem _avatarBunchItem;

			// Avatar bunch to show the avatar entity.
			private AvatarBunch _avatarBunch;

			// Whether is in camera frustum.
			private bool _inCameraFrustum = true;

			private Transform _hipsTarget = null;
			private Transform _rightFootTarget = null;
			private Transform _leftFootTarget = null;

			#endregion


			#region Private / Friend Methods

			/// <summary>
			/// Invoked by AvatarBase to set owner
			/// </summary>
			/// <param name="owner"></param>
			internal void SetOwner(PicoAvatar owner)
			{
				_owner = owner;
				_userId = owner == null ? "0" : owner.userId;

				//
				_statsStartLoadTime = AvatarEnv.realtimeSinceStartup;

				// Scale to zero before animation updated.
				if (owner.capabilities.controlSourceType == ControlSourceType.OtherPlayer)
				{
					this.transform.localScale = Vector3.zero;
				}
			}

			/// <summary>
			/// On Lod Changed
			/// </summary>
			/// <param name="lodLevel"></param>
			internal void Notify_LodChanged(AvatarLodLevel lodLevel)
			{
				if (_forceLodLevel >= 0)
				{
					return;
				}

				if (lodLevel == _activeLodLevel)
					return;

				// Create AvatarLod instance.
				if (CheckBuildAvatarLods(lodLevel))
				{
					// Updat new lod level.
					_deactivingLodLevel = _activeLodLevel;
					_activeLodLevel = lodLevel;
					_activeLod = _avatarLods[(int)lodLevel];
					// Build lod primitives asynchronously.
					_activeLod.AsyncBuildPrimitives();
				}
			}

			/// <summary>
			/// Invoked by AvatarLod when all primitives built. AvatarEntity should switch visible lod here.
			/// </summary>
			/// <param name="lodLevel"></param>
			internal void Notify_AvatarLodBuildFinished(AvatarLodLevel lodLevel)
			{
				if (owner == null)
				{
					UnityEngine.Debug.LogWarning("AvatarEntity load finished but has been destroyed!");
					return;
				}

				if ((uint)_deactivingLodLevel < (uint)AvatarLodLevel.Count)
				{
					_avatarLods[(int)_deactivingLodLevel]?.gameObject.SetActive(false);
					_deactivingLodLevel = AvatarLodLevel.Invalid;
				}

				// Check whether the lod is the active level.
				if (lodLevel == _activeLodLevel && _avatarLods[(int)lodLevel] != null)
				{
					_activeLod = _avatarLods[(int)lodLevel];
					_activeLod.gameObject.SetActive(true);

					//
					if (!isAnyLodReady)
					{
						isAnyLodReady = true;
						Initialize();
						owner.Notify_AvatarEntityLodReadyToShow(this, lodLevel);

						// Notify available.
						if (OnAvatarLodReady != null)
						{
							OnAvatarLodReady.Invoke();

							//
							if (PicoAvatarStats.instance != null)
							{
								if (_statsFirstLodReadyTime == 0.0f)
								{
									_statsFirstLodReadyTime = AvatarEnv.realtimeSinceStartup;
									PicoAvatarStats.instance.AvatarLodReady(owner.avatarId, lodLevel,
										_statsFirstLodReadyTime - _statsStartLoadTime);
								}
							}
						}
					}
					else
					{
						owner.Notify_AvatarEntityLodReadyToShow(this, lodLevel);
					}

					// Check avatar bunch
					if (_avatarBunch != null)
					{
						// Check whether turn off avatar bunch item when user higher lod level.
						if (this._activeLodLevel < this.owner.capabilities.avatarBunchLodLevel)
						{
							if (_avatarBunchItem != null)
							{
								_avatarBunchItem.RemoveFromAvatarBunch();
								_avatarBunchItem = null;
							}
						} // Check whether turn of avatar bunch item.
						else if (this.owner.capabilities.avatarBunchLodLevel >= AvatarLodLevel.Lod0 &&
						         this._activeLodLevel > this.owner.capabilities.avatarBunchLodLevel)
						{
							_avatarBunchItem = _avatarBunch.AddAvatarBunchItem(this);
						}
					}
				}

				// If remote avatar, set playback
				if (owner != null)
				{
					if (owner.capabilities.controlSourceType == ControlSourceType.OtherPlayer)
					{
						SetAnimationPlaybackConfig();
						SetPacketApplyMode();
						SetFaceExpressionPlaybackConfig();
					}
					else if (owner.capabilities.controlSourceType == ControlSourceType.MainPlayer)
					{
						SetLocalAnimationConfig();
						SetAnimationRecordPacketConfig();
						SetFaceExpressionRecordConfig();
					}
				}

				// Notify native script that lod level loaded and can recycle assets that created the lod level.
				if (_rmiObject != null)
				{
					_rmiObject.SetLodLevelLoaded(lodLevel);
				}
			}

			internal void Initialize()
			{
				if (deviceInputReader == null && _nativeHandle != System.IntPtr.Zero)
				{
					if (owner.capabilities.inputSourceType == DeviceInputReaderBuilderInputType.Invalid)
					{
#if UNITY_EDITOR
                        owner.capabilities.inputSourceType = owner.capabilities.isMainAvatar?
                            DeviceInputReaderBuilderInputType.Editor:DeviceInputReaderBuilderInputType.RemotePackage;
#else
                        owner.capabilities.inputSourceType = owner.capabilities.isMainAvatar?
                            DeviceInputReaderBuilderInputType.PicoXR:DeviceInputReaderBuilderInputType.RemotePackage;
#endif
					}

					InitDeviceInputReader(new DeviceInputReaderBuilder().SetType(owner.capabilities.inputSourceType)
						.SetUserId(userId));
				}

				// Main avatar need calculation ik and BodyAnimController.
				needIK = owner.capabilities.isMainAvatar ||
				         owner.capabilities.recordBodyAnimLevel == RecordBodyAnimLevel.DeviceInput;

				// Set current animation playback level. currently switch bs updation.
				if (!owner.capabilities.isMainAvatar)
				{
					lastAnimationPlaybackLevel = owner.capabilities.recordBodyAnimLevel;
					pav_AvatarEntity_SetAnimationPlaybackLevel(_nativeHandle, (uint)lastAnimationPlaybackLevel);
				}

				//
				if (PicoAvatarApp.instance != null)
				{
					expressionPlaybackEnabled = owner.capabilities.enableExpression;
				}
			}

			/// <summary>
			/// Destroy the object. Destroy all avatar lods and release native object.
			/// </summary>
			internal void Destroy()
			{
				//
				if (PicoAvatarStats.instance != null && !isDestroyed)
				{
					PicoAvatarStats.instance.DecreaseInstanceCount(PicoAvatarStats.InstanceType.AvatarEnity);
				}

				_isDestroyed = true;

				// Clear ready flag.
				isAnyLodReady = false;

				// Release pack memory view
				NativeObject.ReleaseField(ref _shotPacketMemoryView);
				NativeObject.ReleaseField(ref _ftPacketMemoryView);

				// Release input reader.
				if (_deviceInputReader != null)
				{
					_deviceInputReader.Release();
					_deviceInputReader = null;
				}

				if (_nativeHandle != IntPtr.Zero)
				{
					if (OnAvatarWillUnBind != null)
						OnAvatarWillUnBind.Invoke();

					// Destroy all vatar lods.
					DestroyAvatarLods();

					// Release body controller.
					ReferencedObject.ReleaseField(ref _bodyAnimController);

					// Release reference to native object.
					_allAvatarEntities.Remove(_nativeEntityId);
					_nativeEntityId = 0;
					NativeObject.ReleaseNative(ref _nativeHandle);
				}

				// Destroy the client object.
				if (_avatarBunchItem != null)
				{
					_avatarBunchItem.RemoveFromAvatarBunch();
					_avatarBunchItem = null;
				}

				if (_rmiObject != null)
				{
					_rmiObject.Release();
					_rmiObject = null;
				}

				//
				_owner = null;

				// Stop record.
				if (debugPlaybackRecorder != null)
				{
					debugPlaybackRecorder.StopRecord();
					debugPlaybackRecorder = null;
				}
			}

			/// <summary>
			/// Notification from js when a non-active lod level unloaded
			/// </summary>
			/// <param name="lodLevel"></param>
			internal void Notify_UnloadNonActiveLodLevel(AvatarLodLevel lodLevel)
			{
				if ((uint)lodLevel < (uint)this._avatarLods.Length)
				{
					var entityLod = this._avatarLods[(int)lodLevel];
					if (entityLod)
					{
						_avatarLods[(int)lodLevel] = null;
						entityLod.Destroy();
						GameObject.Destroy(entityLod.gameObject);
					}
				}
			}

			internal void UpdateCustomHandPose()
			{
				if (bodyAnimController != null)
				{
					if (leftCustomHand && leftCustomHandPose != null)
					{
						leftCustomHandPose.SetHandPose(bodyAnimController);
					}

					if (rightCustomHand && rightCustomHandPose != null)
					{
						rightCustomHandPose.SetHandPose(bodyAnimController);
					}
				}
			}

			internal void CalcuateHeadToBodyRatio()
			{
				float avatarHeadHeight = bodyAnimController.avatarHeadHeight;
				headToBodyRatio = avatarIKTargetsConfig.eyePositionOffset.y / avatarHeadHeight;
			}


			internal void Notify_RebuildMaterials(AvatarLodLevel lodLevel)
			{
				if (lodLevel >= 0 && lodLevel < AvatarLodLevel.Count)
				{
					var lod = _avatarLods[(int)lodLevel];
					if (lod)
					{
						lod.RebuildMaterials();
					}
				}
			}

			/// <summary>
			/// Pre-update for this frame
			/// </summary>
			/// <returns>true if the avatar should be visible this frame</returns>
			internal bool PreUpdateFrame()
			{
				if (owner == null)
				{
					return false;
				}

				var isMainAvatar = owner.capabilities.isMainAvatar;
				//
				var avatarApp = PicoAvatarApp.instance;

				if (needIK)
				{
					if (bodyAnimController != null)
					{
						bodyAnimController.UpdateFrame();
					}

					if (avatarIKTargetsConfig.IsTargetDirty())
					{
						if (deviceInputReader != null)
						{
							deviceInputReader.SetTargets(avatarIKTargetsConfig);
						}

						//recalculate headToBodyRatio
						if (bodyAnimController != null)
						{
							CalcuateHeadToBodyRatio();
						}

						avatarIKTargetsConfig.SetTargetDirty(false);
					}

					if (deviceInputReader != null)
					{
						deviceInputReader.UpdateFrame();
					}

					if (bodyAnimController != null)
					{
						bodyAnimController.UpdateDefaultAnimation();
						//
						UpdateCustomHandWristXForm();
					}
				}

				// Reset flag that native updation not skipped this frame
				isNativeUpdateSkippedThisFrame = true;

				//
				var frustemPlanes = avatarApp.frustumPlanes;
				if (frustemPlanes != null)
				{
					Bounds bodyBounds = new Bounds(this.transform.position + new Vector3(0, 1.0f, 0.0f)
						, new Vector3(1.5f, 2.0f, 1.5f));
					var inCameraFrustum = GeometryUtility.TestPlanesAABB(frustemPlanes, bodyBounds);

					// Check whether culling visibility changed.
					if (inCameraFrustum != _inCameraFrustum)
					{
						_inCameraFrustum = inCameraFrustum;
						pav_AvatarEntity_SetVisibleThisFrame(_nativeHandle, inCameraFrustum);
					}

					// Set visibility for avatar bunch.
					if (_avatarBunchItem != null)
					{
						_avatarBunchItem.visibleThisFrame = _inCameraFrustum;
					}
				}
				else
				{
					pav_AvatarEntity_SetVisibleThisFrame(_nativeHandle, true);
				}

				// Update visible this frame
				if (this.isAnyLodReady)
				{
					// If
					if (isMainAvatar)
					{
						var placeHolderAvatar = owner as PicoPlaceholderAvatar;
						// If nobody reference the source placeholder, skip update.
						if (placeHolderAvatar != null && !placeHolderAvatar.isReferenced)
						{
							return false;
						}
					}
					else // If (!isMainAvatar)
					{
						if (avatarApp.optimizationSettings.skipUpdateIfInvisibleThisFrame && !_inCameraFrustum &&
						    !owner.forceUpdateSkeleton)
						{
							// if invisible, we MUST at least update native avatar once each 0.1 seconds.
							if (Time.time - _lastSkipInvisibleUpdateTime < 0.2f)
							{
								return false;
							}

							// to avoid skipped by skipUpdateEvenVisibleDistance
							_lastSkipVisibleUpdateTime = -1.0f;
						}

						// keep the last time that is updated.
						_lastSkipInvisibleUpdateTime = Time.time;

						// Distance to camera.
						float sqrtDist = (avatarApp.currentCullingCamera.transform.position - this.transform.position)
							.sqrMagnitude;

						// Set current animation playback level. currently switch bs updation.
						if (_owner.capabilities.recordBodyAnimLevel > RecordBodyAnimLevel.FullBone)
						{
							// All level.
							RecordBodyAnimLevel desiredLevel = _owner.capabilities.recordBodyAnimLevel;
							if (sqrtDist > avatarApp.squaredSkipFaceExpressionPlaybackDist)
							{
								// Skip blendshape level.
								desiredLevel = RecordBodyAnimLevel.FullBone;
							}

							//
							if (desiredLevel != lastAnimationPlaybackLevel)
							{
								lastAnimationPlaybackLevel = desiredLevel;
								pav_AvatarEntity_SetAnimationPlaybackLevel(_nativeHandle, (uint)desiredLevel);
							}
						}

						// Skip update even visible if too far.
						if (avatarApp.optimizationSettings.skipUpdateEvenVisibleDistance > 0)
						{
							if (sqrtDist > avatarApp.squaredSkipUpdateEvenVisibleDistance &&
							    Time.time - _lastSkipVisibleUpdateTime <
							    avatarApp.optimizationSettings.skipUpdateEvenVisibleInterval)
							{
								return false;
							}

							_lastSkipVisibleUpdateTime = Time.time;
						}
					}

					// Notify c++ that visible this frame.
					pav_AvatarEntity_SetNeedUpdateSimulationRenderData(_nativeHandle, true);

					// Reset flag that native updation not skipped this frame
					isNativeUpdateSkippedThisFrame = false;

					// If avatar bunch item attached, no need to update simulation render data！
					if (_avatarBunchItem != null && !owner.forceUpdateSkeleton)
					{
						return false;
					}

					// Need update simulation render data.
					return true;
				}

				return false;
			}

			/// <summary>
			/// Invoked from PicoAvatar.PostUpdateFrame
			/// </summary>
			/// <param name="elapsedTime"></param>
			internal void PostUpdateFrame(float elapsedTime)
			{
				// Checked by PicoAvatar.PostUpdateFrame
				//if (owner == null || !this.isAnyLodReady)
				//{
				//    return;
				//}
				if (bodyAnimController != null)
				{
					bodyAnimController.PostUpdateFrame();
				}

				// Only main avatar may has custom hand pose.
				if (owner.capabilities.isMainAvatar)
				{
					UpdateCustomHandPose();
				}
			}

			/// <summary>
			/// Job to pre-update render data. currently bone matrices updated from native part
			/// </summary>
			private struct PreUpdateSimulationRenderDataTJob : IJob
			{
				// By default containers are assumed to be read & write
				[ReadOnly] public int avatarIndex;

				// The code actually running on the job
				public void Execute()
				{
					var entity = PicoAvatarManager.instance.avatarEntitiesToUpdateSimulationDataThisFrame[avatarIndex];
					entity.PreUpdateSimulationRenderDataT();
				}
			}

			/// <summary>
			/// Invoked from PicoAvatarManager to schedule new jobs for updating render data
			/// </summary>
			/// <param name="avatarEntityIndex"></param>
			/// <param name="jobHandles"></param>
			/// <param name="jobIndex"></param>
			internal void SchedulePreUpdateSimulationRenderDataJobs(int avatarEntityIndex,
				NativeArray<Unity.Jobs.JobHandle> jobHandles, ref int jobIndex)
			{
				var job = new PreUpdateSimulationRenderDataTJob()
				{
					avatarIndex = avatarEntityIndex
				};
				jobHandles[jobIndex++] = job.Schedule();
			}

			/// <summary>
			/// Invoked from Job to do pre-update for render data. mainly update data from native part in work thread
			/// </summary>
			private void PreUpdateSimulationRenderDataT()
			{
				pav_AvatarEntity_UpdateSimulationRenderDataT(_nativeHandle);
				//
				_activeLod.PreUpdateSimulationRenderDataT();
			}


			/// <summary>
			/// Invoked from PicoAvatarManager to apply bone matrices to unity scene transforms
			/// </summary>
			/// <param name="avatarEntityIndex"></param>
			/// <param name="jobHandles"></param>
			/// <param name="jobIndex"></param>
			internal void SchedulePostUpdateSimulationRenderDataJobs(int avatarEntityIndex,
				NativeArray<Unity.Jobs.JobHandle> jobHandles, ref int jobIndex)
			{
				_activeLod.SchedulePostUpdateSimulationRenderDataJobs(avatarEntityIndex, jobHandles, ref jobIndex);
			}

			/// <summary>
			/// Invoked from PicoAvatarManager to update render data like blend shape weights from native part to unity objects
			/// </summary>
			/// <param name="elapsedTime"></param>
			internal void UpdateSimulationRenderData(float elapsedTime)
			{
				_activeLod.UpdateSimulationRenderData(elapsedTime);
			}

			/// <summary>
			/// Update light env
			/// </summary>
			/// <param name="lightEnv"></param>
			internal void Notify_AvatarSceneLightEnvChanged(PicoAvatarSceneLightEnv lightEnv)
			{
				for (int i = 0; i < (int)AvatarLodLevel.Count; ++i)
				{
					var avatarLod = _avatarLods[i];
					if (avatarLod != null)
					{
						avatarLod.OnAvatarSceneLightEnvChanged(lightEnv);
					}
				}
			}

			/// <summary>
			/// Invoked from PicoAvatar when active and enable changed
			/// </summary>
			/// <param name="_visibleAndEnable"></param>
			internal void OnParentVisiblilityChanged(bool _visibleAndEnable)
			{
				if (_avatarBunchItem != null)
				{
					_avatarBunchItem.visibleThisFrame = _visibleAndEnable;
				}
			}

			/// <summary>
			/// Unity late update chance to start read next frame data of xr device
			/// </summary>
			private void LateUpdate()
			{
				if (_deviceInputReader != null)
				{
					_deviceInputReader.StartReadDeviceForNextFrame();
				}
			}

			private bool InitDeviceInputReader(DeviceInputReaderBuilder builder)
			{
				if (_deviceInputReader != null)
				{
					return false;
				}
				else
				{
					var deviceHandler = pav_AvatarEntity_GetDeviceInputReader(_nativeHandle);
					if (deviceHandler != System.IntPtr.Zero)
					{
						_deviceInputReader = builder.Build();
						if (_deviceInputReader != null)
						{
#if UNITY_EDITOR
							var device = EditorInputDevice.GetDevice(builder.userId);
							if (device != null)
							{
								avatarIKTargetsConfig.xrRoot = device.transRoot;
								avatarIKTargetsConfig.headTarget = device.transHead;
								avatarIKTargetsConfig.leftHandTarget = device.transLeftHand;
								avatarIKTargetsConfig.rightHandTarget = device.transRightHand;
								avatarIKTargetsConfig.hipsTarget = device.transHips;
								avatarIKTargetsConfig.leftFootTarget = device.transLeftFoot;
								avatarIKTargetsConfig.rightFootTarget = device.transRightFoot;
							}
#endif

							_deviceInputReader.actionBased = actionBasedControl;
							_deviceInputReader.positionActions = positionActions;
							_deviceInputReader.rotationActions = rotationActions;
							_deviceInputReader.buttonActions = buttonActions;
							_deviceInputReader.Initialize(deviceHandler, this);
							_deviceInputReader.SetTargets(avatarIKTargetsConfig);
							avatarIKTargetsConfig.SetTargetDirty(false);
							_deviceInputReader.Retain();
						}
						else
						{
							Debug.Log("@@@@@@@@@@@@@@@@@InitDeviceInputReader failed");
							NativeObject.ReleaseNative(ref deviceHandler);
						}
					}

					return _deviceInputReader != null;
				}
			}

			private void UpdateCustomHandWristXForm()
			{
				if (bodyAnimController != null)
				{
					UnityEngine.Profiling.Profiler.BeginSample("UpdateCustomHandWristXForm");

					if (leftCustomHand && leftCustomHandPose != null)
					{
						leftCustomHandPose.SetWristXForm(bodyAnimController);
					}

					if (rightCustomHand && rightCustomHandPose != null)
					{
						rightCustomHandPose.SetWristXForm(bodyAnimController);
					}

					UnityEngine.Profiling.Profiler.EndSample();
				}
			}

			#endregion


			#region For Editor

			/// <summary>
			/// Notification from SDK engine that need modify avatar
			/// </summary>
			/// <param name="lodLevel"></param>
			/// <param name="modificationJsonText"></param>
			internal void OnLodPartialRebuild(AvatarLodLevel lodLevel, string modificationJsonText)
			{
				var entityLod = (uint)lodLevel < (uint)AvatarLodLevel.Count ? _avatarLods[(int)lodLevel] : null;
				if (entityLod == null)
				{
					return;
				}

				//
				entityLod.PartialRebuild(modificationJsonText);
			}

			#endregion
		}
	}
}