using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

namespace Pico
{
	namespace Avatar
	{
		public abstract class IDeviceInputReader : NativeObject
		{
			#region Public Properties

			public enum ControllerButtons : int
			{
				LPrimary2DButton = 0,
				RPrimary2DButton = 1,
				LMenuButton = 2,
				RMenuButton = 3,
				LGripButton = 4,
				RGripButton = 5,
				LTriggerButton = 6,
				RTriggerButton = 7,
				XButton = 8,
				YButton = 9,
				AButton = 10,
				BButton = 11,

				Count,
			}

			public enum DeviceType : int
			{
				Head = 0,
				LeftHand = 1,
				RightHand = 2,
				Hips = 3,
				LeftFoot = 4,
				RightFoot = 5,
				Count,
			};

			public enum ConnectionStatus : int
			{
				Disconnected = 0,
				Avaliable = 1,
				Lost = 2,
				Idle = 3,
				Count,
			};

			public DeviceData deviceData;

			public bool[] connectionStatus = new bool[(int)DeviceType.Count];
			public bool[] controllerButtonStatus = new bool[(int)ControllerButtons.Count];

			public bool actionBased = false;
			public bool useRelative = true;
			public InputActionProperty[] positionActions = null;
			public InputActionProperty[] rotationActions = null;
			public InputActionProperty[] buttonActions = null;

			public Transform xrRoot = null;
			public Transform[] targetTransforms = new Transform[(int)DeviceType.Count];

			public float deviceArmSpan = 1.0f;

			protected Vector3[] deviceOffsets = new Vector3[(int)DeviceType.Count];
			protected Quaternion[] deviceRotationOffsets = new Quaternion[(int)DeviceType.Count];

			#endregion


			#region Internal Methods

			// Constructor invoked by AvatarEntity.
			internal virtual void Initialize(System.IntPtr nativeHandler_, AvatarEntity owner)
			{
				_owner = owner;
				SetNativeHandle(nativeHandler_, false);
				deviceData.version = 0;
				deviceData.connectionStatus = new uint[(int)DeviceType.Count];
				deviceData.controllerButtonStatus = new uint[(int)ControllerButtons.Count];
				deviceData.positions = new Vector3[(int)DeviceType.Count];
				deviceData.orientations = new Quaternion[(int)DeviceType.Count];
			}

			internal void SetTargets(AvatarIKTargetsConfig avatarIKTargetsConfig)
			{
				useRelative = !avatarIKTargetsConfig.worldSpaceDrive;
				xrRoot = avatarIKTargetsConfig.xrRoot;
				targetTransforms[(uint)DeviceType.Head] = avatarIKTargetsConfig.headTarget;
				targetTransforms[(uint)DeviceType.LeftHand] = avatarIKTargetsConfig.leftHandTarget;
				targetTransforms[(uint)DeviceType.RightHand] = avatarIKTargetsConfig.rightHandTarget;
				targetTransforms[(uint)DeviceType.Hips] = avatarIKTargetsConfig.hipsTarget;
				targetTransforms[(uint)DeviceType.LeftFoot] = avatarIKTargetsConfig.leftFootTarget;
				targetTransforms[(uint)DeviceType.RightFoot] = avatarIKTargetsConfig.rightFootTarget;

				deviceOffsets[(int)DeviceType.Head] = avatarIKTargetsConfig.eyePositionOffset;
				deviceOffsets[(int)DeviceType.LeftHand] = avatarIKTargetsConfig.leftHandPositionOffset;
				deviceOffsets[(int)DeviceType.RightHand] = avatarIKTargetsConfig.rightHandPositionOffset;
				deviceOffsets[(int)DeviceType.LeftFoot] = avatarIKTargetsConfig.leftFootPositionOffset;
				deviceOffsets[(int)DeviceType.RightFoot] = avatarIKTargetsConfig.rightFootPositionOffset;
				deviceOffsets[(int)DeviceType.Hips] = avatarIKTargetsConfig.hipsPositionOffset;

				deviceRotationOffsets[(int)DeviceType.Head] =
					Quaternion.Inverse(avatarIKTargetsConfig.eyeRotationOffset);
				deviceRotationOffsets[(int)DeviceType.LeftHand] =
					Quaternion.Inverse(avatarIKTargetsConfig.leftHandRotationOffset);
				deviceRotationOffsets[(int)DeviceType.RightHand] =
					Quaternion.Inverse(avatarIKTargetsConfig.rightHandRotationOffset);
				deviceRotationOffsets[(int)DeviceType.LeftFoot] =
					Quaternion.Inverse(avatarIKTargetsConfig.leftFootRotationOffset);
				deviceRotationOffsets[(int)DeviceType.RightFoot] =
					Quaternion.Inverse(avatarIKTargetsConfig.rightFootRotationOffset);
				deviceRotationOffsets[(int)DeviceType.Hips] =
					Quaternion.Inverse(avatarIKTargetsConfig.hipsRotationOffset);
			}

			internal virtual void InitInputFeatureUsage()
			{
				return;
			}
			
			internal virtual void UpdateFrame()
			{
				UnityEngine.Profiling.Profiler.BeginSample("InputReader_UpdateFrame");

				UpdateConnectionStatus();
				UpdateButtonStatus();
				UpdateDevicePose();
				pav_AvatarDeviceInputReader_SetDeviceInputData(nativeHandle, ref deviceData);

				UnityEngine.Profiling.Profiler.EndSample();
			}

			// Invoked in late update to prepare next frame.
			internal virtual void StartReadDeviceForNextFrame()
			{
			}

			abstract internal void UpdateButtonStatus();
			abstract internal void UpdateDevicePose();
			abstract internal void UpdateConnectionStatus();

			#endregion

			#region Private Fields

			// Avatar Entity manage the object.
			protected AvatarEntity _owner;

			#endregion

			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarDeviceInputReader_SetDeviceInputData(
				System.IntPtr nativeHandle, ref DeviceData deviceData);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarDeviceInputReader_GetDeviceInputData(
				System.IntPtr nativeHandle, ref DeviceData deviceData);

			#endregion
		}
	}
}