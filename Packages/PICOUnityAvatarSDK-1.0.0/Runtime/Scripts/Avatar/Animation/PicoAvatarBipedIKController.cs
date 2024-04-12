using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar BipedIKController
		/// </summary>
		public class AvatarBipedIKController : NativeObject
		{
			#region Public Properties

			AvatarBodyAnimController owner = null;

			/// <summary>
			/// max tracking distance of the hand controllers.
			/// After exceeding scaled max controller distance, the hands will stop IK tracking and enter the idle state. 
			/// </summary>
			/// <param name="distance">Maximum tracking distance in meters</param>
			public float maxControllerDistance
			{
				get { return _maxControllerDistance; }
				set
				{
					_maxControllerDistance = value;
					UpdateMaxControllerDistance();
				}
			}

			/// <summary>
			/// controller scale for max tracking distance of the hand controllers (usually the scale value of xr root in vr environment). 
			/// After exceeding scaled max controller distance, the hands will stop IK tracking and enter the idle state. 
			/// </summary>
			/// <param name="distance">Maximum tracking distance in meters</param>
			public float controllerScale
			{
				get { return _controllerScale; }
				set
				{
					_controllerScale = value;
					UpdateMaxControllerDistance();
				}
			}

			#endregion


			#region Public Methods

			// Constructor invoked by AvatarBodyAnimController
			internal AvatarBipedIKController(System.IntPtr nativeHandler_, AvatarBodyAnimController owner_)
			{
				owner = owner_;
				SetNativeHandle(nativeHandler_, false);
			}

			internal void Initialize(uint nativeEntityId)
			{
				if (_rmiObject == null)
				{
					_rmiObject = new NativeCall_AvatarBipedIKController(this, nativeEntityId);
					_rmiObject.Retain();
				}

				// set default params
				SetIKTrackingSource(IKEffectorType.Head, IKTrackingSource.DeviceInput);
				SetIKTrackingSource(IKEffectorType.LeftHand, IKTrackingSource.DeviceInput);
				SetIKTrackingSource(IKEffectorType.RightHand, IKTrackingSource.DeviceInput);
				SetIKTrackingSource(IKEffectorType.Hips, IKTrackingSource.DeviceInput);
				SetIKTrackingSource(IKEffectorType.LeftFoot, IKTrackingSource.DefaultLocomotion);
				SetIKTrackingSource(IKEffectorType.RightFoot, IKTrackingSource.DefaultLocomotion);

				SetIKAutoStopModeEnable(IKAutoStopMode.ControllerDisconnect, true);
				SetIKAutoStopModeEnable(IKAutoStopMode.ControllerIdle, true);
				SetIKAutoStopModeEnable(IKAutoStopMode.ControllerLoseTracking, true);
				SetIKAutoStopModeEnable(IKAutoStopMode.ControllerFarAway, true);

				SetStretchEnable(IKEffectorType.LeftHand, true);
				SetStretchEnable(IKEffectorType.RightHand, true);
				// SetMaxStretchRatio((int)IKEffectorType.LeftHand, 0.2f);
				// SetMaxStretchRatio((int)IKEffectorType.RightHand, 0.2f);
				UpdateMaxControllerDistance();

				SetProceduralFootstepEnable(true);
				SetFootstepPositionThreshold(0.10f);
				SetFootstepRotationThreshold(40.0f);
				SetFootstepHeight(0.05f);
				SetFootstepSpeed(2.0f);
				SetVelocityWeight(0.3f);
				SetTransitionSpeed(8.0f);
				SetSupportingLegRotationThreshold(20.0f);
				SetSupportingLegRotationSpeed(1.0f);
				SetAnimationVelocityThresholds(0.8f, 0.5f);
				SetAnimationLerpSpeed(8.0f);
				SetFootCollisionRadius(0.1f);

				SetAutoStandUpTimeThreshold(0.01f);
				SetAutoStandUpDistanceThreshold(0.05f);
			}

			internal void Destroy()
			{
				if (this.nativeHandle != IntPtr.Zero)
				{
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
			//--------------------------------------------------------------------------------------------------

			/// <summary>
			/// Enable or disable ik processing of one ik target
			/// </summary>
			/// <param name="ikEffectorType">IKEffectorType of the effector</param>
			/// <param name="enable">true - enable, false - disable</param>
			public void SetIKEnable(IKEffectorType ikEffectorType, bool enable)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetIKEnable((uint)ikEffectorType, enable);
				}
			}

			/// <summary>
			/// Enable or disable ik processing of all ik targets
			/// </summary>
			/// <param name="enable">true - enable, false - disable</param>
			public void SetAllIKEnable(bool enable)
			{
				if (_rmiObject != null)
				{
					for (uint i = 0; i < (uint)IKEffectorType.Count; ++i)
					{
						_rmiObject.SetIKEnable(i, enable);
					}
				}
			}

			/// <summary>
			/// Set the IKEffector XForm object. C# need set xform for IKEffector
			/// </summary>
			/// <param name="iKEffectorType">IKEffectorType of the effector</param>
			/// <param name="xform">XForm of the iKEffectorType transform</param>
			public void SetIKEffectorXForm(IKEffectorType iKEffectorType, XForm xform)
			{
				pav_AvatarBipedIKController_SetIKEffectorXForm(nativeHandle, (uint)iKEffectorType, ref xform);
			}

			/// <summary>
			/// Set the Tracking XForms. C# need set xform for tracking joint
			/// </summary>
			/// <param name="iKEffectorTypes">IKEffectorType list of the effectors</param>
			/// <param name="xforms">XForm list of the iKEffectorType transforms</param>
			public void SetIKEffectorXForms(uint[] iKEffectorTypes, XForm[] xforms)
			{
				pav_AvatarBipedIKController_SetIKEffectorXForms(nativeHandle, (uint)iKEffectorTypes.Length,
					ref iKEffectorTypes, ref xforms);
			}

			/// <summary>
			/// Get transform of ik effector in avatar space
			/// </summary>
			/// <param name="iKEffectorType">IKEffectorType of the effector</param>
			/// <returns>XForm of the iKEffectorType transform</returns>
			public XForm GetIKEffectorXForm(IKEffectorType iKEffectorType)
			{
				var ret = new XForm();
				pav_AvatarBipedIKController_GetIKEffectorXForm(nativeHandle, (uint)iKEffectorType, ref ret);
				return ret;
			}

			/// <summary>
			/// Enable or disable update ik target from device
			/// </summary>
			/// <param name="ikEffectorType">IKEffectorType of the effector</param>
			/// <param name="enable">true - enable, false - disable(update ik target from animation)</param>
			public void SetUpdateIKTargetFromDevice(IKEffectorType ikEffectorType, bool enable)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetUpdateIKTargetFromDevice((uint)ikEffectorType, enable);
				}
			}

			/// <summary>
			/// Set tracking source of ik target
			/// </summary>
			/// <param name="ikEffectorType">IKEffectorType of the effector</param>
			/// <param name="trackingSource">Tracking source: DeviceInput, Animation, Custom, DefaultLocomotion</param>
			public void SetIKTrackingSource(IKEffectorType ikEffectorType, IKTrackingSource trackingSource)
			{
				if (trackingSource == IKTrackingSource.DeviceInput)
				{
					SetUpdateIKTargetFromDevice(ikEffectorType, true);
				}
				else
				{
					SetUpdateIKTargetFromDevice(ikEffectorType, false);
				}

				if (trackingSource == IKTrackingSource.Animation)
				{
					pav_AvatarBipedIKController_SetUpdateIKTargetFromAnimation(nativeHandle, (uint)ikEffectorType,
						true);
				}
				else
				{
					pav_AvatarBipedIKController_SetUpdateIKTargetFromAnimation(nativeHandle, (uint)ikEffectorType,
						false);
				}
			}

			/// <summary>
			/// Set joint limit for joint
			/// </summary>
			/// <param name="jointType">JointType of the joint</param>
			/// <param name="minAngle">Minimum angle that the joint can be rotated</param>
			/// <param name="maxAngle">Maximum angle that the joint can be rotated</param>
			public void SetRotationLimit(JointType jointType, Vector3 minAngle, Vector3 maxAngle)
			{
				pav_AvatarBipedIKController_SetRotationLimit(nativeHandle, (uint)jointType, ref minAngle, ref maxAngle);
			}

			/// <summary>
			/// Enable or disable joint limit for joint
			/// </summary>
			/// <param name="jointType">JointType of the joint</param>
			/// <param name="enable">true - enable, false - disable</param>
			public void SetRotationLimitEnable(JointType jointType, bool enable)
			{
				pav_AvatarBipedIKController_SetRotationLimitEnable(nativeHandle, (uint)jointType, enable);
			}

			/// <summary>
			/// Enable or disable limb stretch, only support arms for now
			/// </summary>
			/// <param name="ikEffectorType">IKEffectorType of the effector</param>
			/// <param name="enable">true - enable, false - disable</param>
			public void SetStretchEnable(IKEffectorType ikEffectorType, bool enable)
			{
				pav_AvatarBipedIKController_SetStretchEnable(nativeHandle, (uint)ikEffectorType, enable);
			}

			/// <summary>
			/// Set max length for limb, only support arms for now
			/// </summary>
			/// <param name="ikEffectorType">IKEffectorType of the effector</param>
			/// <param name="length">Maximum length that the limb can be stretched</param>
			public void SetMaxStretchLength(IKEffectorType ikEffectorType, float length)
			{
				pav_AvatarBipedIKController_SetMaxStretchLength(nativeHandle, (uint)ikEffectorType, length);
			}

			/// <summary>
			/// Set max stretch ratio for limb, only support arms for now
			/// </summary>
			/// <param name="ikEffectorType">IKEffectorType of the effector</param>
			/// <param name="ratio">Maximum ratio that the limb can be stretched</param>
			public void SetMaxStretchRatio(IKEffectorType ikEffectorType, float ratio)
			{
				pav_AvatarBipedIKController_SetMaxStretchRatio(nativeHandle, (uint)ikEffectorType, ratio);
			}

			/// <summary>
			/// Set hips height range when avatar moving up or down
			/// </summary>
			/// <param name="minHeight">Minimum height value in meters</param>
			/// <param name="maxHeight">Maximum height value in meters</param>
			public void SetValidHipsHeightRange(float minHeight, float maxHeight)
			{
				_minHeight = minHeight;
				_maxHeight = maxHeight;
				pav_AvatarBipedIKController_SetValidHipsHeightRange(nativeHandle, minHeight, maxHeight);
			}

			/// <summary>
			/// Set min head height for avatar
			/// </summary>
			/// <param name="minHeight">Minimum height value in meters</param>
			public void SetMinHeadHeight(float minHeight)
			{
				pav_AvatarBipedIKController_SetMinHeadHeight(nativeHandle, minHeight);
			}

			/// <summary>
			/// Set spine rotation weight influnced by avatar head rotation
			/// </summary>
			/// <param name="weight">Weight of spine rotation(ranging from 0 to 1)</param>
			public void SetSpineRotationWeight(float weight)
			{
				pav_AvatarBipedIKController_SetSpineRotationWeight(nativeHandle, weight);
			}

			/// <summary>
			/// Update scale for max tracking distance of the hand controllers.
			/// After exceeding it, the hands will stop IK tracking and enter the idle state. 
			/// </summary>
			/// <param name="distance">Maximum tracking distance in meters</param>
			public void UpdateMaxControllerDistance()
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetMaxControllerDistance(_maxControllerDistance * _controllerScale);
				}
			}

			/// <summary>
			/// Set IK AutoStop mode enable, avatar will restore to idle pose when ik stopped
			/// </summary>
			/// <param name="ikAutoStopMode">IK auto stop mode: ControllerDisconnect, ControllerIdle, ControllerLoseTracking, ControllerFarAway</param>
			/// <param name="enable">true - enable, false - disable</param>
			public void SetIKAutoStopModeEnable(IKAutoStopMode ikAutoStopMode, bool enable)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetIKAutoStopModeEnable((uint)ikAutoStopMode, enable);
				}
			}

			/// <summary>
			/// Set IK InvalidRegion mode enable, avatar will restore to idle pose when controller reaches invalid region
			/// </summary>
			/// <param name="enable">true - enable, false - disable</param>
			public void SetIKHandInvalidRegionEnable(bool enable)
			{
				if (_rmiObject != null)
				{
					_rmiObject.SetIKHandInvalidRegionEnable(enable);
				}
			}


			//------------------------------------------------------------------------------
			//Avatar Locomotion
			//Users can set avatar locomotion parameters such as step threshold, speed, etc

			/// <summary>
			/// Enable or disable procedural footstep
			/// </summary>
			/// <param name="enable">true - enable, false - disable</param>
			public void SetProceduralFootstepEnable(bool enable)
			{
				pav_AvatarBipedIKController_SetProceduralFootstepEnable(nativeHandle, enable);
			}

			/// <summary>
			/// Sets footstep position threshold for avatar locomotion
			/// </summary>
			/// <param name="positionThreshold">Moving distance that triggers a footstep</param>
			public void SetFootstepPositionThreshold(float positionThreshold)
			{
				pav_AvatarBipedIKController_SetFootstepPositionThreshold(nativeHandle, positionThreshold);
			}

			/// <summary>
			/// Sets footstep rotation threshold for avatar locomotion
			/// </summary>
			/// <param name="rotationThreshold">Rotating angle that triggers a footstep</param>
			public void SetFootstepRotationThreshold(float rotationThreshold)
			{
				pav_AvatarBipedIKController_SetFootstepRotationThreshold(nativeHandle, rotationThreshold);
			}

			/// <summary>
			/// Sets footstep height for avatar locomotion
			/// </summary>
			/// <param name="stepHeight">Max height of each footstep in meters</param>
			public void SetFootstepHeight(float stepHeight)
			{
				pav_AvatarBipedIKController_SetFootstepHeight(nativeHandle, stepHeight);
			}

			/// <summary>
			/// Sets footstep speed for avatar locomotion
			/// </summary>
			/// <param name="stepSpeed">Speed of each footstep</param>
			public void SetFootstepSpeed(float stepSpeed)
			{
				pav_AvatarBipedIKController_SetFootstepSpeed(nativeHandle, stepSpeed);
			}

			/// <summary>
			/// Sets collision radius for avatar foot
			/// </summary>
			/// <param name="radius">Foot collision radius</param>
			public void SetFootCollisionRadius(float radius)
			{
				pav_AvatarBipedIKController_SetFootCollisionRadius(nativeHandle, radius);
			}

			/// <summary>
			/// Sets velocity weight for avatar footstep that contribute to step length
			/// </summary>
			/// <param name="velocityWeight">Target velocity weight</param>
			public void SetVelocityWeight(float velocityWeight)
			{
				pav_AvatarBipedIKController_SetVelocityWeight(nativeHandle, velocityWeight);
			}

			/// <summary>
			/// Speed for procedural/animation footstep transition
			/// </summary>
			/// <param name="speed">Transition speed </param>
			public void SetTransitionSpeed(float speed)
			{
				pav_AvatarBipedIKController_SetTransitionSpeed(nativeHandle, speed);
			}

			/// <summary>
			/// Sets foot roatation threshold if not stepping
			/// </summary>
			/// <param name="rotationThreshold">Rotation angle that triggers a footstep</param>
			public void SetSupportingLegRotationThreshold(float rotationThreshold)
			{
				pav_AvatarBipedIKController_SetSupportingLegRotationThreshold(nativeHandle, rotationThreshold);
			}

			/// <summary>
			/// Sets foot rotation speed for avatar locomotion
			/// </summary>
			/// <param name="rotationSpeed">Rotation speed of supporting leg</param>
			public void SetSupportingLegRotationSpeed(float rotationSpeed)
			{
				pav_AvatarBipedIKController_SetSupportingLegRotationSpeed(nativeHandle, rotationSpeed);
			}

			/// <summary>
			/// Sets thresholds that triggers or stops animation for avatar locomotion
			/// </summary>
			/// <param name="enter">Speed that triggers locomotion animation</param>
			/// <param name="leave">Speed that stops locomition animation</param>
			public void SetAnimationVelocityThresholds(float enter, float leave)
			{
				pav_AvatarBipedIKController_SetAnimationVelocityThresholds(nativeHandle, enter, leave);
			}

			/// <summary>
			/// Sets lerp speed of locomition animation velocity
			/// </summary>
			/// <param name="speed">Velocity lerp speed</param>
			public void SetAnimationLerpSpeed(float speed)
			{
				pav_AvatarBipedIKController_SetAnimationLerpSpeed(nativeHandle, speed);
			}

			/// <summary>
			/// Resets avatar root offset 
			/// </summary>
			public void ResetAvatarRoot()
			{
				pav_AvatarBipedIKController_ResetAvatarRoot(nativeHandle);
			}

			/// <summary>
			/// Gets hips offset in sitting mode
			/// </summary>
			/// <returns>Hips Offset</returns>
			public float GetHipsSittingOffset()
			{
				return pav_AvatarBipedIKController_GetHipsSittingHeightOffset(nativeHandle);
			}

			/// <summary>
			/// Set whether auto check stand up
			/// </summary>
			/// <param name="enable">Enable or not</param>
			public void SetAutoStandUpCheckEnable(bool enable)
			{
				pav_AvatarBipedIKController_SetAutoMoveHipsCheck(nativeHandle, enable);
			}

			/// <summary>
			/// Sets time threshold for avatar automatically standing up in sitting mode.
			/// Avatar will automatically stand up when time exceeds threshold.
			/// Time starts counting when distance or angle between head tracking target and hips 
			/// exceeds the specified threshold (by 'SetAutoStandUpDistanceThreshold' or 'SetAutoStandUpAngularThreshold').
			/// </summary>
			/// <param name="threshold">Time in seconds</param>
			public void SetAutoStandUpTimeThreshold(float threshold)
			{
				pav_AvatarBipedIKController_SetAutoStandTimeThreshold(nativeHandle, threshold);
			}

			/// <summary>
			/// Sets distance threshold for avatar automatically standing up in sitting mode.
			/// Avatar will automatically stand up when distance exceeds threshold for a time period(by 'SetAutoStandUpTimeThreshold').
			/// Distance is the length between head tracking target and hips position.
			/// </summary>
			/// <param name="threshold">Distance in meters</param>
			public void SetAutoStandUpDistanceThreshold(float threshold)
			{
				pav_AvatarBipedIKController_SetAutoStandDistanceThreshold(nativeHandle, threshold);
			}

			/// <summary>
			/// Sets angular threshold for avatar automatically standing up in sitting mode.
			/// Avatar will automatically stand up when angle between head and sit target exceeds threshold.
			/// </summary>
			/// <param name="threshold">Angular threshold in degrees</param>
			public void SetAutoStandUpAngularThreshold(float threshold)
			{
				pav_AvatarBipedIKController_SetAutoStandAngularThreshold(nativeHandle, threshold);
			}

			/// <summary>
			/// Checks whether avatar needs stand up form sitting mode
			/// </summary>
			/// <returns>True if avatar needs stand up</returns>
			public bool CheckNeedStandUp()
			{
				return pav_AvatarBipedIKController_CheckStandUp(nativeHandle);
			}

			/// <summary>
			/// Sets avatar spine bend enable status. If enabled, avatar will have better spine performance but costs more cpu time
			/// </summary>
			/// <param name="enable">true - enable, false - disable</param>
			public void SetSpineBendEnable(bool enable)
			{
				pav_AvatarBipedIKController_SetSpineBendEnable(nativeHandle, enable);
			}


			/// <summary>
			/// Reset avatar ik effector 
			/// </summary>
			/// <param name="iKEffectorType">Avatar IK Effector type</param>
			public void ResetEffector(IKEffectorType iKEffectorType)
			{
				pav_AvatarBipedIKController_ResetEffector(nativeHandle, (uint)iKEffectorType);
			}


			/// <summary>
			/// Get avatar arm span in avatar space
			/// </summary>
			/// <returns>Value of avatar arm span</returns>
			public float GetAvatarArmSpan()
			{
				return pav_AvatarBipedIKController_GetAvatarArmSpan(nativeHandle);
			}

			#endregion


			#region Private Fields

			private float _minHeight = -10;
			private float _maxHeight = 10;
			private float _maxControllerDistance = 1.0f;
			private float _controllerScale = 1.0f;

			private NativeCall_AvatarBipedIKController _rmiObject;

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_GetIKEffectorXForm(
				System.IntPtr nativeHandle, uint ikEffectorType, ref XForm XForm);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetIKEffectorXForm(
				System.IntPtr nativeHandle, uint ikEffectorType, ref XForm xform);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetIKEffectorXForms(
				System.IntPtr nativeHandle, uint jointCount, ref uint[] ikEffectorTypes, ref XForm[] xforms);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetUpdateIKTargetFromAnimation(
				System.IntPtr nativeHandle, uint ikEffectorType, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetMaxStretchLength(
				System.IntPtr nativeHandle, uint ikEffectorType, float length);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetMaxStretchRatio(
				System.IntPtr nativeHandle, uint ikEffectorType, float ratio);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetStretchEnable(System.IntPtr nativeHandle,
				uint ikEffectorType, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetRotationLimit(System.IntPtr nativeHandle,
				uint jointType, ref Vector3 minAngle, ref Vector3 maxAngle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetRotationLimitEnable(
				System.IntPtr nativeHandle, uint jointType, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetValidHipsHeightRange(
				System.IntPtr nativeHandle, float minHeight, float maxHeight);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetMinHeadHeight(System.IntPtr nativeHandle,
				float minHeight);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetSpineRotationWeight(
				System.IntPtr nativeHandle, float weight);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetProceduralFootstepEnable(
				System.IntPtr nativeHandle, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetFootstepPositionThreshold(
				System.IntPtr nativeHandle, float positionThreshold);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetFootstepRotationThreshold(
				System.IntPtr nativeHandle, float rotationThreshold);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetFootstepHeight(System.IntPtr nativeHandle,
				float stepHeight);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetFootstepSpeed(System.IntPtr nativeHandle,
				float stepSpeed);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetFootCollisionRadius(
				System.IntPtr nativeHandle, float radius);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetVelocityWeight(System.IntPtr nativeHandle,
				float velocityWeight);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetTransitionSpeed(
				System.IntPtr nativeHandle, float speed);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetSupportingLegRotationThreshold(
				System.IntPtr nativeHandle, float rotationThreshold);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetSupportingLegRotationSpeed(
				System.IntPtr nativeHandle, float rotationSpeed);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetAnimationVelocityThresholds(
				System.IntPtr nativeHandle, float enter, float leave);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetAnimationLerpSpeed(
				System.IntPtr nativeHandle, float speed);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_ResetAvatarRoot(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern float pav_AvatarBipedIKController_GetHipsSittingHeightOffset(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetAutoMoveHipsCheck(
				System.IntPtr nativeHandle, bool autoCheck);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_AvatarBipedIKController_CheckStandUp(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetAutoStandTimeThreshold(
				System.IntPtr nativeHandle, float threshold);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetAutoStandDistanceThreshold(
				System.IntPtr nativeHandle, float threshold);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetAutoStandAngularThreshold(
				System.IntPtr nativeHandle, float threshold);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_ResetEffector(System.IntPtr nativeHandle,
				uint ikEffectorType);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBipedIKController_SetSpineBendEnable(
				System.IntPtr nativeHandle, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern float pav_AvatarBipedIKController_GetAvatarArmSpan(System.IntPtr nativeHandle);

			#endregion
		}
	}
}