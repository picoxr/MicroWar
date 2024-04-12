using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar body animation controller.
		/// </summary>
		/// <remarks>
		/// Here is a list of avatar animation and IK related modules,
		/// you can learn how to play action on PicoAvatar through these classes, how to set IK to drive somatosensory in picoVR
		/// BodyAnimationController control skeleton/blendshape for a AvatarEntity since one Avatar may has one more
		/// seperated AvatarEntity which owns different skeleton instance at the same time.
		/// </remarks>
		public class AvatarBodyAnimController : NativeObject
		{
			#region Public Properties

			/// <summary>
			/// Gets avatar joint type id to name id table
			/// </summary>
			public TypeID2NameIDTable jointNameIDTable
			{
				get
				{
					if (_jointNameIDTables == null)
					{
						_jointNameIDTables =
							new TypeID2NameIDTable(pav_AvatarBodyAnimController_GetJointNameIDTable(nativeHandle));
						_jointNameIDTables.Retain();
					}

					return _jointNameIDTables;
				}
			}

			// animationParameter name to value table.
			internal IDParameterTable animationParameterTable
			{
				get
				{
					if (_animationParameterTable == null)
					{
						_animationParameterTable =
							new IDParameterTable(pav_AvatarBodyAnimController_GetAnimationParameters(nativeHandle));
						_animationParameterTable.Retain();
					}

					return _animationParameterTable;
				}
			}

			/// <summary>
			/// Gets avatar autoFit controller.
			/// </summary>
			public AvatarAutoFitController autoFitController
			{
				get
				{
					if (_autoFitController == null)
					{
						var animHandler = pav_AvatarBodyAnimController_GetAutoFitController(nativeHandle);
						if (animHandler != System.IntPtr.Zero)
						{
							_autoFitController = new AvatarAutoFitController(animHandler, this);
							_autoFitController.Retain();
						}
					}

					return _autoFitController;
				}
			}


			/// <summary>
			/// Gets avatar biped ik controller.
			/// </summary>
			public AvatarBipedIKController bipedIKController
			{
				get
				{
					if (_bipedIKController == null)
					{
						var animHandler = pav_AvatarBodyAnimController_GetBipedIKController(nativeHandle);
						if (animHandler != System.IntPtr.Zero)
						{
							_bipedIKController = new AvatarBipedIKController(animHandler, this);
							_bipedIKController.Retain();
						}
					}

					return _bipedIKController;
				}
			}

			/// <summary>
			/// Gets body anim controller started status.
			/// </summary>
			public bool started { get; set; } = false;

			/// <summary>
			/// Gets owner avatar entity.
			/// </summary>
			public AvatarEntity owner { get; private set; } = null;

			/// <summary>
			/// Gets avatar sitting status
			/// </summary>
			public bool isSitting
			{
				get { return _isSitting; }
			}

			/// <summary>
			/// Whether enable avatar auto stand in sitting mode
			/// </summary>
			public bool autoStandUp = true;

			/// <summary>
			/// Whether body tracking is used in body tracking mode
			/// </summary>
			public bool isUsingBodyTracking { get; private set; } = false;

			/// <summary>
			/// Whether animator is enabled. 
			/// </summary>
			public bool isAnimatorEnabled
			{
				get { return pav_AvatarBodyAnimController_IsAnimatorEnabled(this.nativeHandle); }
				set { SetEnableAnimator(value); }
			}

			/// <summary>
			/// Avatar runtime scale caused by changing avatar height.
			/// </summary>
			public Vector3 avatarScale
			{
				get { return _avatarScale; }
			}

			/// <summary>
			/// Height of the head bone in avatar Space in Tpose;
			/// </summary>
			public float avatarHeadHeight { get; private set; } = 0.0f;

			#endregion


			#region Public Methods

			//Constructor invoked by AvatarEntity.
			internal AvatarBodyAnimController(System.IntPtr nativeHandler_, AvatarEntity owner_)
			{
				owner = owner_;
				SetNativeHandle(nativeHandler_, false);

				InitNativeAnimationLayers();
			}


			/// <summary>
			/// Get transform of avatar joint in avatar space
			/// </summary>
			/// <param name="jointType">JointType of the joint</param>
			/// <returns>XForm of joint transform</returns>
			public XForm GetJointXForm(JointType jointType)
			{
				var ret = new XForm();
				pav_AvatarBodyAnimController_GetJointXForm(nativeHandle, (uint)jointType, ref ret);
				return ret;
			}

			/// <summary>
			/// Set transform of avatar joint
			/// </summary>
			/// <param name="jointType">JointType of the joint</param>
			/// <param name="xform">>XForm value</param>
			public void SetJointXForm(JointType jointType, XForm xform)
			{
				pav_AvatarBodyAnimController_SetJointXForm(nativeHandle, (uint)jointType, ref xform);
			}


			/// <summary>
			/// Get transform of avatar joint in world space
			/// </summary>
			/// <param name="jointType">JointType of the joint</param>
			/// <returns>XForm of joint transform</returns>
			public XForm GetJointWorldXForm(JointType jointType)
			{
				var ret = new XForm();
				pav_AvatarBodyAnimController_GetJointWorldXForm(nativeHandle, (uint)jointType, ref ret);
				return ret;
			}


			/// <summary>
			/// Get list of transforms of avatar joints in avatar space
			/// </summary>
			/// <param name="jointType">list of jointTypes</param>
			/// <param name="xforms">Returned value of joint transforms</param>
			public void GetJointXForms(JointType[] jointTypes, ref XForm[] xforms)
			{
				if (jointTypes.Length != xforms.Length)
				{
					return;
				}

				//
				var gcHandle = GCHandle.Alloc(jointTypes, GCHandleType.Pinned);
				var gcXFormsHandle = GCHandle.Alloc(xforms, GCHandleType.Pinned);
				//
				pav_AvatarBodyAnimController_GetJointXForms(nativeHandle, (uint)jointTypes.Length,
					gcHandle.AddrOfPinnedObject(), gcXFormsHandle.AddrOfPinnedObject());
				gcHandle.Free();
				gcXFormsHandle.Free();
			}


			/// <summary>
			/// Set world orientation for a list of joints.
			/// </summary>
			/// <param name="jointType">list of jointTypes</param>
			/// <param name="jointQuats">Value of joint orientations</param>
			public void SetWorldOrientation(uint count, JointType[] jointTypes, Quaternion[] jointQuats)
			{
				if (jointTypes.Length != jointQuats.Length)
				{
					return;
				}

				var gcHandle = GCHandle.Alloc(jointTypes, GCHandleType.Pinned);
				var gcXFormsHandle = GCHandle.Alloc(jointQuats, GCHandleType.Pinned);
				pav_AvatarBodyAnimController_SetWorldOrientations(nativeHandle, count, gcHandle.AddrOfPinnedObject(),
					gcXFormsHandle.AddrOfPinnedObject());
				gcHandle.Free();
				gcXFormsHandle.Free();
			}

			/// gets avatar transform in remote net package
			internal XForm GetRemotePackageAvatarXForm()
			{
				XForm xForm = new XForm();
				pav_AvatarBodyAnimController_GetRemotePackageAvatarXForm(nativeHandle, ref xForm);
				return xForm;
			}
			//----------------------------------------------------------------------------------------------------------


			//----------------------------------------------------------------------------------------------------------
			/// Avatar Face Tracker
			/// <summary>
			/// Starts avatar face tracker
			/// </summary>
			/// <param name="enableLipSync">Whether enable lip sync</param>
			/// <param name="enableFT">Whether enable face tracking </param>
			/// <param name="useXR">Use face tracking through runtime sdk. Should always set to true. </param>
			public void StartFaceTrack(bool enableLipSync, bool enableFT, bool useXR = true)
			{
				if (_rmiObject != null)
				{
					_rmiObject.StartFaceTracker(enableLipSync, enableFT, useXR);
				}
			}

			/// <summary>
			/// Stops avatar face tracker
			/// </summary>
			public void StopFaceTracker()
			{
				if (_rmiObject != null)
				{
					_rmiObject.StopFaceTracker();
				}
			}

			/// <summary>
			/// Whether blend with idle face animation in face tracking mode
			/// </summary>
			/// <param name="enableLipSync">Whether enable lip sync</param>
			/// <param name="enableFT">Whether enable face tracking </param>
			public void SetIdleEnable(bool useIdle)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetIdleEnable(useIdle);
				}
			}

			//----------------------------------------------------------------------------------------------------------

			//----------------------------------------------------------------------------------------------------------
			///Avatar Height Control
			/// <summary>
			/// Set avatar height, scale avatar to a specific height
			/// </summary>
			/// <param name="height">Avatar target eye height or head bone height</param>
			/// <param name="isEyeHeight">Flag indicating whether the height param is for avatar eye height or head height</param>
			public void SetAvatarHeight(float height, bool isEyeHeight = true)
			{
				owner.owner.SetAvatarHeight(height, isEyeHeight);
				if (_autoFitController != null && _autoFitController.localAvatarHeightFittingEnable == true)
				{
					_autoFitController.UpdateAvatarHeightOffset();
				}

				Vector3 currentScale = GetJointWorldXForm(JointType.Root).scale;
				// Vector3 avatarScale;
				_avatarScale.x = currentScale.x / _initScale.x;
				_avatarScale.y = currentScale.y / _initScale.y;
				_avatarScale.z = currentScale.z / _initScale.z;
			}

			/// <summary>
			/// Trigger avatar sitting at certain position with height offset
			/// </summary>
			/// <param name="sitTarget">Avatar sitting target transform</param>
			/// <param name="sitHeight">Avatar sitting hips height</param>
			public void SitDown(Transform sitTarget, float sitHeight)
			{
				if (sitTarget == null)
				{
					return;
				}

				// If in body tracking mode, close body tracking and run only IK.
				if (owner.owner.capabilities.isMainAvatar && owner.owner.capabilities.inputSourceType ==
				    DeviceInputReaderBuilderInputType.BodyTracking)
				{
					SetUseBodyTracking(false);
				}

				if (_bipedIKController != null)
				{
					sitHeight += _bipedIKController.GetHipsSittingOffset();
				}

				if (autoFitController != null)
				{
					//TODO: set target hips height
					//      set autofit params
					autoFitController.SetTargetHipsHeight(sitHeight);
					autoFitController.ApplyPreset(autoFitController.presetSitting);
					autoFitController.UpdateAvatarHeightOffset();
					if (!_sitCallBackInititated)
					{
						autoFitController.AddAvatarOffsetChangedCallback(OnAvatarOffsetChangedCallBack);
						_sitCallBackInititated = true;
					}
				}

				if (_bipedIKController != null)
				{
					//TODO: set to fix hips mode, use current avatar base posXZ and hips height as avatar hips pos
					_bipedIKController.SetIKEnable(IKEffectorType.Hips, true);
					_bipedIKController.SetAutoStandUpCheckEnable(false);
					owner.avatarIKTargetsConfig.hipsTarget = sitTarget;
					Vector3 hipsOffset = owner.avatarIKTargetsConfig.hipsPositionOffset;
					hipsOffset.y = -(sitHeight);
					owner.avatarIKTargetsConfig.hipsPositionOffset = hipsOffset;
				}

				//TODO: set animation param for sit
				SetAnimationParameterBoolByName("_sit", true);

				SetAnimationParameterFloatByName("_sitHeight", sitHeight);

				_isSitting = true;
				_sitTargetDirty = true;
			}

			/// <summary>
			/// Trigger avatar standing up
			/// </summary>
			public void StandUp()
			{
				if (autoFitController != null)
				{
					autoFitController.SetTargetHipsHeight(-1);
					autoFitController.ApplyPreset(autoFitController.presetStanding);
					autoFitController.UpdateAvatarHeightOffset();
					if (_sitCallBackInititated)
					{
						autoFitController.ClearAvatarOffsetChangedCallback(OnAvatarOffsetChangedCallBack);
						_sitCallBackInititated = false;
					}
				}

				if (bipedIKController != null)
				{
					//TODO: turn off fix hips mode
					_bipedIKController.SetIKEnable(IKEffectorType.Hips, false);
					_bipedIKController.SetAutoStandUpCheckEnable(false);
					owner.avatarIKTargetsConfig.hipsTarget = null;
				}

				//TODO: stop sit animation
				SetAnimationParameterBoolByName("_sit", false);

				_isSitting = false;

				// If in body tracking mode, resume body tracking from sitting.
				if (owner.owner.capabilities.isMainAvatar && owner.owner.capabilities.inputSourceType ==
				    DeviceInputReaderBuilderInputType.BodyTracking)
				{
					SetUseBodyTracking(true);
				}
			}

			//----------------------------------------------------------------------------------------------------------


			//----------------------------------------------------------------------------------------------------------
			///Avatar Animation Control
			///New animation layers can be created for users to play customized animation clips
			///Default state machine layers (IK locomotion layer, default hand pose layers, eyeBlink layer) are not exposed to users for now, you cannot add new animations to those layers
			///But you can create new layers to override default animatons
			/// <summary>
			/// Creates a new animation layer by name
			/// </summary>
			/// <param name="layerName">Animation layer name</param>
			public AvatarAnimationLayer CreateAnimationLayerByName(string layerName)
			{
				var layerHandler = pav_AvatarBodyAnimController_CreateAnimationLayerByName(nativeHandle, layerName);
				if (layerHandler != System.IntPtr.Zero)
				{
					AvatarAnimationLayer animationLayer = new AvatarAnimationLayer(layerHandler);
					animationLayer.Retain();
					_animationLayersMap[layerName] = animationLayer;
					//animationLayers.Add(animationLayer);
					animationLayer.name = layerName;
					animationLayer.owner = this;
					return animationLayer;
				}

				return null;
			}

			/// <summary>
			/// Gets an existing animation layer by name
			/// </summary>
			/// <param name="layerName">Animation layer name</param>
			public AvatarAnimationLayer GetAnimationLayerByName(string layerName)
			{
				if (_animationLayersMap.ContainsKey(layerName))
				{
					return _animationLayersMap[layerName];
				}

				return null;
			}

			// Gets the animation layer for remote package joint transform net sync
			internal AvatarAnimationLayer GetRemotePackageJointLayer()
			{
				if (_remotePackageJointLayer == null)
				{
					_remotePackageJointLayer = GetAnimationLayerByName("RemotePackageJoint");
					if (_remotePackageJointLayer != null)
					{
						_remotePackageJointLayer.layerType = AnimLayerType.RemotePackage;
					}
				}

				return _remotePackageJointLayer;
			}


			// Gets the animation layer for remote package blendShape net sync
			internal AvatarAnimationLayer GetRemotePackageBlendShapeLayer()
			{
				if (_remotePackageBlendShapeLayer == null)
				{
					_remotePackageBlendShapeLayer = GetAnimationLayerByName("RemotePackageBlendShape");
					if (_remotePackageBlendShapeLayer != null)
					{
						_remotePackageBlendShapeLayer.layerType = AnimLayerType.RemotePackage;
					}
				}

				return _remotePackageBlendShapeLayer;
			}

			// Set manual avatar record mask.
			internal void SetAvatarRecordMask(System.IntPtr recordMask)
			{
				pav_AvatarBodyAnimController_SetAvatarRecordMask(nativeHandle, recordMask);
			}

			/// <summary>
			/// Register or set bool animation paramater by name
			/// </summary>
			/// <param name="parameterName">Parameter name</param>
			/// <param name="value">Parameter value</param>
			public void SetAnimationParameterBoolByName(string parameterName, bool value)
			{
				pav_AvatarBodyAnimController_SetAnimationParameterBoolByName(nativeHandle, parameterName, value);
			}

			/// <summary>
			/// Register or set uint animation paramater by name
			/// </summary>
			/// <param name="parameterName">Parameter name</param>
			/// <param name="value">Parameter value</param>
			public void SetAnimationParameterUIntByName(string parameterName, uint value)
			{
				pav_AvatarBodyAnimController_SetAnimationParameterUIntByName(nativeHandle, parameterName, value);
			}

			/// <summary>
			/// Register or set float animation paramater by name
			/// </summary>
			/// <param name="parameterName">Parameter name</param>
			/// <param name="value">Parameter value</param>
			public void SetAnimationParameterFloatByName(string parameterName, float value)
			{
				pav_AvatarBodyAnimController_SetAnimationParameterFloatByName(nativeHandle, parameterName, value);
			}


			/// <summary>
			/// Get value of bool animation parameter by name
			/// </summary>
			/// <param name="parameterName">Parameter name</param>
			/// <returns>Parameter value </returns>
			public bool GetAnimationParameterBoolByName(string parameterName)
			{
				return pav_AvatarBodyAnimController_GetAnimationParameterBoolByName(nativeHandle, parameterName);
			}

			/// <summary>
			/// Get value of uint animation parameter by name
			/// </summary>
			/// <param name="parameterName">Parameter name</param>
			/// <returns>Parameter value </returns>
			public uint GetAnimationParameterUIntByName(string parameterName)
			{
				return pav_AvatarBodyAnimController_GetAnimationParameterUIntByName(nativeHandle, parameterName);
			}

			/// <summary>
			/// Get value of float animation parameter by name
			/// </summary>
			/// <param name="parameterName">Parameter name</param>
			/// <returns>Parameter value </returns>
			public float GetAnimationParameterFloatByName(string parameterName)
			{
				return pav_AvatarBodyAnimController_GetAnimationParameterFloatByName(nativeHandle, parameterName);
			}

			//----------------------------------------------------------------------------------------------------------


			//----------------------------------------------------------------------------------------------------------
			///Avatar Custom Hand

			// Update custom hand
			internal void UpdateCustomHandPose(uint count, uint[] poseTypes, Vector3[] worldOffsets,
				XForm wristWorldXForm, uint side, bool syncWristTransform, uint fingerPoseSyncMode, bool scaleHand)
			{
				var gcHandle = GCHandle.Alloc(poseTypes, GCHandleType.Pinned);
				var gcOffsetHandle = GCHandle.Alloc(worldOffsets, GCHandleType.Pinned);
				pav_AvatarBodyAnimController_UpdateCustomHandPose(nativeHandle, count, gcHandle.AddrOfPinnedObject(),
					gcOffsetHandle.AddrOfPinnedObject(), ref wristWorldXForm, side, syncWristTransform,
					fingerPoseSyncMode, scaleHand);
				gcHandle.Free();
				gcOffsetHandle.Free();
			}

			// Get inital skeleton joint offsets
			internal void GetSkeletonJointInitOffsets(uint count, uint[] jointTypes, ref Vector3[] initPositions)
			{
				if (jointTypes.Length < count || initPositions.Length < count)
				{
					return;
				}

				var gcHandle = GCHandle.Alloc(jointTypes, GCHandleType.Pinned);
				var gcPositionsHandle = GCHandle.Alloc(initPositions, GCHandleType.Pinned);
				pav_AvatarBodyAnimController_GetSkeletonJointInitOffsets(nativeHandle, count,
					gcHandle.AddrOfPinnedObject(), gcPositionsHandle.AddrOfPinnedObject());
				gcHandle.Free();
				gcPositionsHandle.Free();
			}

			//late update 
			internal void PostUpdateFrame()
			{
				// TODO: add a switch to open this
				UpdateAvtarEntityUnityXFormFromNative();

				if (_autoFitController != null)
				{
					_autoFitController.LateUpdateFrame();
				}
			}

			// update
			internal void UpdateFrame()
			{
				UnityEngine.Profiling.Profiler.BeginSample("BodyController_UpdateFrame");

				if (!_headXformInitialized)
				{
					SetDefaultHeadXform();
				}

				if (_autoFitController != null)
				{
					_autoFitController.UpdateFrame();
				}

				if (_isSitting && _bipedIKController != null)
				{
					if (_bipedIKController.CheckNeedStandUp())
					{
						StandUp();
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();
			}

			// update default animation parameters
			internal void UpdateDefaultAnimation()
			{
				UnityEngine.Profiling.Profiler.BeginSample("BodyController_UpdateAnimationParam");


				if (_defaultAnimation != null)
				{
					_defaultAnimation.UpdateFrame();
				}

				UnityEngine.Profiling.Profiler.EndSample();
			}

			// offset changed call back for sitting 
			internal void OnAvatarOffsetChangedCallBack(AvatarAutoFitController controller, Vector3 cameraOffsetPos)
			{
				if (!this.autoStandUp || !isSitting || !_sitTargetDirty)
					return;
				if (_bipedIKController != null)
				{
					_bipedIKController.SetAutoStandUpCheckEnable(true);
					_sitTargetDirty = false;
				}
			}

			// Initialize native body anim controller.
			internal void Initialize(uint nativeEntityId)
			{
				if (_initialized)
				{
					return;
				}

				if (this._rmiObject == null)
				{
					_rmiObject = new NativeCall_AvatarBodyAnimController(this, nativeEntityId);
					_rmiObject.Retain();
				}


				if (owner.owner.capabilities.isMainAvatar ||
				    owner.owner.capabilities.recordBodyAnimLevel == RecordBodyAnimLevel.DeviceInput)
				{
					if (_defaultAnimation == null)
					{
						_defaultAnimation = new AvatarDefaultAnimation(this);
						_defaultAnimation.InitAnimationLayers();
					}
				}

				if (owner.owner.capabilities.isMainAvatar ||
				    owner.owner.capabilities.recordBodyAnimLevel == RecordBodyAnimLevel.DeviceInput)
				{
					if (bipedIKController != null)
					{
						bipedIKController.Initialize(nativeEntityId);
					}
				}

				if (owner.owner.capabilities.isMainAvatar && owner.owner.capabilities.inputSourceType ==
				    DeviceInputReaderBuilderInputType.BodyTracking)
				{
					SetUseBodyTracking(true);
				}

				_initScale = GetJointWorldXForm(JointType.Root).scale;
				avatarHeadHeight = GetJointXForm(JointType.Head).position.y;

				_initialized = true;
			}
			
			internal void Destroy()
			{
				if (this.nativeHandle != IntPtr.Zero)
				{
					foreach (var animationLayer in _animationLayersMap)
					{
						animationLayer.Value.Release();
					}

					//
					_animationLayersMap.Clear();
					_defaultAnimation = null;
					//
					if (_autoFitController != null)
					{
						_autoFitController.Release();
						_autoFitController = null;
					}

					if (_jointNameIDTables != null)
					{
						_jointNameIDTables.Release();
						_jointNameIDTables = null;
					}

					//
					if (_animationParameterTable != null)
					{
						_animationParameterTable.Release();
						_animationParameterTable = null;
					}

					if (_bipedIKController != null)
					{
						_bipedIKController.Release();
						_bipedIKController = null;
					}

					if (_rmiObject != null)
					{
						_rmiObject.Release();
						_rmiObject = null;
					}

					SetNativeHandle(System.IntPtr.Zero, false);
				}
			}


			protected override void OnDestroy()
			{
				Destroy();
				//
				base.OnDestroy();
			}

			// animation start callback
			internal void OnAnimationStart()
			{
				//
				started = true;
				SetDefaultHeadXform();
			}

			// set default head XForm
			internal void SetDefaultHeadXform()
			{
				_headInitXform = GetJointXForm(JointType.Head);
				XForm nativeEntityXForm = owner.GetNativeXForm();
				var entityTrans = owner.transform;

				entityTrans.localPosition = nativeEntityXForm.position;
				entityTrans.localRotation = nativeEntityXForm.orientation;
				entityTrans.localScale = nativeEntityXForm.scale;
				//
				_headOrientationInvAvatarSpace =
					Quaternion.Inverse(nativeEntityXForm.orientation * _headInitXform.orientation);
				_headOrientationInvMatAvatarSpace.SetTRS(new Vector3(0, 0, 0), _headOrientationInvAvatarSpace,
					new Vector3(1, 1, 1));

				_headXformInitialized = true;
			}

			// XForm of AvatarEntity is managed by AvatarSDK, and application should not modify it.
			internal void UpdateAvtarEntityUnityXFormFromNative()
			{
				XForm nativeEntityXForm = owner.GetNativeXForm();

				//Set Entity Transform
				owner.transform.localPosition = nativeEntityXForm.position;
				owner.transform.localRotation = nativeEntityXForm.orientation;
				owner.transform.localScale = nativeEntityXForm.scale;
			}


			/// <summary>
			/// Gets current eye transform
			/// </summary>
			/// <returns>XForm of eye transform</returns>
			public XForm GetEyeXForm()
			{
				if (!_headXformInitialized)
				{
					SetDefaultHeadXform();
				}

				XForm ret = new XForm();
				//if (!_owner.owner.capabilites.isLocalAvatar || !_isDefaultSetXFormSucceed) return ret;
				// 0. Get Avatar Joint Transform: avatar space
				var _headTrans = GetJointXForm(JointType.Head);
				Matrix4x4 _headTransMat = new Matrix4x4();
				_headTransMat.SetTRS(_headTrans.position, _headTrans.orientation, _headTrans.scale);

				//avatar entity xform in avatar space
				XForm _avatarTrans = owner.GetNativeXForm();
				Matrix4x4 _avatarTransMat = new Matrix4x4();
				_avatarTransMat.SetTRS(_avatarTrans.position, _avatarTrans.orientation, _avatarTrans.scale);

				// 1. Cal Eye Offset
				Quaternion _headRotationQuatAvatarSpace = (_avatarTrans.orientation * _headTrans.orientation);
				Matrix4x4 _headMatAvatarSpace = new Matrix4x4();

				_headMatAvatarSpace = _avatarTransMat * _headTransMat;
				_headMatAvatarSpace = _headMatAvatarSpace * _headOrientationInvMatAvatarSpace;
				ret.position = _headMatAvatarSpace.MultiplyPoint(owner.avatarIKTargetsConfig.eyePositionOffset);
				ret.orientation = _headRotationQuatAvatarSpace * _headOrientationInvAvatarSpace;

				return ret;
			}

			//----------------------------------------------------------------------------------------------------------
			///Avatar Animation Test, for internal use only
			public void PlayThreePointAnimationByPath(string path)
			{
				if (_rmiObject != null)
				{
					_rmiObject.PlayThreePointAnimationByPath(path);
				}
			}


			public void StopThreePointAnimation()
			{
				if (_rmiObject != null)
				{
					_rmiObject.StopThreePointAnimation();
				}
			}

			/// <summary>
			/// For remote controlled avatar, should disable animator to reduce cost if no local animation
			/// blending needed.
			/// </summary>
			/// <param name="enabled">whether enable native animator</param>
			public void SetEnableAnimator(bool enabled)
			{
				pav_AvatarBodyAnimController_SetEnableAnimator(this.nativeHandle, enabled);
			}

			#endregion


			#region Private Fields

			// Avatar Entity manage the object.
			//private AvatarEntity owner;
			private AvatarAutoFitController _autoFitController = null;
			private AvatarBipedIKController _bipedIKController = null;
			private AvatarDefaultAnimation _defaultAnimation = null;

			private XForm _headInitXform;
			private Quaternion _headOrientationInvAvatarSpace;
			private Matrix4x4 _headOrientationInvMatAvatarSpace;

			private bool _initialized = false;
			private bool _headXformInitialized = false;

			private Dictionary<string, AvatarAnimationLayer> _animationLayersMap =
				new Dictionary<string, AvatarAnimationLayer>();

			private AvatarAnimationLayer _remotePackageJointLayer = null;
			private AvatarAnimationLayer _remotePackageBlendShapeLayer = null;

			// joint id table. used to find joint name id with JointType.
			private TypeID2NameIDTable _jointNameIDTables;

			// remote object.
			private NativeCall_AvatarBodyAnimController _rmiObject;

			private IDParameterTable _animationParameterTable = null;


			private AvatarAnimationLayer _actionLayer = null;

			private bool _isPlayingIdle = false;
			private bool _isSitting = false;
			private bool _sitTargetDirty = false;
			private bool _sitCallBackInititated = false;
			private Vector3 _initScale = Vector3.one;

			private Vector3 _avatarScale = Vector3.one;

			private void InitNativeAnimationLayers()
			{
				uint layerCount = pav_AvatarBodyAnimController_GetAnimationLayersCount(nativeHandle);
				System.IntPtr[] animationLayersNativeHandle = new System.IntPtr[layerCount];
				var gcHandle = GCHandle.Alloc(animationLayersNativeHandle, GCHandleType.Pinned);
				pav_AvatarBodyAnimController_GetAnimationLayers(nativeHandle, layerCount,
					gcHandle.AddrOfPinnedObject());
				for (uint i = 0; i < layerCount; ++i)
				{
					AvatarAnimationLayer animationLayer = new AvatarAnimationLayer(animationLayersNativeHandle[i]);
					animationLayer.Retain();
					//animationLayers.Add(animationLayer);
					_animationLayersMap[animationLayer.name] = animationLayer;
				}

				gcHandle.Free();
				GetRemotePackageJointLayer();
				GetRemotePackageBlendShapeLayer();
			}

			// In body tracking mode, call this function to enable or disable body tracking at run time.
			private void SetUseBodyTracking(bool useBodyTracking)
			{
				isUsingBodyTracking = useBodyTracking;

				// If using body tracking, disable IK; otherwise enable IK.
				if (_bipedIKController != null)
				{
					_bipedIKController.SetAllIKEnable(!isUsingBodyTracking);
				}

				// If using body tracking, disable AutoFit; otherwise enable AutoFit.
				if (autoFitController != null)
				{
					autoFitController.localAvatarHeightFittingEnable = !isUsingBodyTracking;
				}

				if (_rmiObject != null)
				{
					_rmiObject.SetUseBodyTracking(isUsingBodyTracking);
				}
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBodyAnimController_GetJointNameIDTable(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_GetJointXForm(System.IntPtr entityHandle,
				uint jointType, ref XForm entityXForm);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_GetJointWorldXForm(
				System.IntPtr entityHandle, uint jointType, ref XForm entityXForm);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_SetJointXForm(System.IntPtr entityHandle,
				uint jointType, ref XForm entityXForm);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_GetJointXForms(System.IntPtr entityHandle,
				uint jointCount, System.IntPtr jointTypes, System.IntPtr jointXForms);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_GetRemotePackageAvatarXForm(
				System.IntPtr nativeHandle, ref XForm xform);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_UpdateCustomHandPose(
				System.IntPtr nativeHandle, uint jointCount,
				System.IntPtr poseTypes, System.IntPtr worldOffsets, ref XForm wristWorldXForm, uint side,
				bool syncWristTransform, uint fingerPoseSyncMode, bool scaleHand);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_SetWorldOrientations(
				System.IntPtr nativeHandle, uint jointCount, System.IntPtr jointTypes, System.IntPtr jointQuats);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBodyAnimController_GetSkeletonJointInitOffsets(
				System.IntPtr nativeHandle, uint jointCount, System.IntPtr jointTypes, System.IntPtr initPositions);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_SetEnableAnimator(
				System.IntPtr nativeHandle, bool enabled);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_AvatarBodyAnimController_IsAnimatorEnabled(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBodyAnimController_CreateAnimationLayerByName(
				System.IntPtr nativeHandle, string layerName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBodyAnimController_GetAnimationLayerByName(
				System.IntPtr nativeHandle, string layerName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarBodyAnimController_GetAnimationLayersCount(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_GetAnimationLayers(
				System.IntPtr nativeHandle, uint layerCount, System.IntPtr nativeHandleLayers);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBodyAnimController_GetAnimationParameters(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_SetAnimationParameterBoolByName(
				System.IntPtr nativeHandle, string parameterName, bool value);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_SetAnimationParameterUIntByName(
				System.IntPtr nativeHandle, string parameterName, uint value);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBodyAnimController_SetAnimationParameterFloatByName(
				System.IntPtr nativeHandle, string parameterName, float value);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBodyAnimController_SetAvatarRecordMask(
				System.IntPtr nativeHandle, System.IntPtr recordMask);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_AvatarBodyAnimController_GetAnimationParameterBoolByName(
				System.IntPtr nativeHandle, string parameterName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarBodyAnimController_GetAnimationParameterUIntByName(
				System.IntPtr nativeHandle, string parameterName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern float pav_AvatarBodyAnimController_GetAnimationParameterFloatByName(
				System.IntPtr nativeHandle, string parameterName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBodyAnimController_GetAutoFitController(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBodyAnimController_GetBipedIKController(
				System.IntPtr nativeHandle);

			#endregion
		}
	}
}