#if NO_XR
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pico
{
    namespace Avatar
    {
        public class PXRDeviceInputReader : XRDeviceInputReader
        {
            #region Device Data Definition
            private enum ControllerType
            {
                LEFT_CONTROLLER = 1,
                RIGHT_CONTROLLER = 2,
            }

            private struct SPIEventConnect
            {
                ControllerType controller;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
                public byte[] software_ver;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
                public byte[] hardware_ver;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
                public byte[] sn;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
                public byte[] addr;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
                public byte[] ndi_version;
            }
            
            private enum ControllerConnectStatus
            {
                CONTROLLER_OFFLINE,
                CONTROLLER_ONLINE,
                CONTROLLER_ONLINEINVALID,
                CONTROLLER_OLDVER = 0x5F,
            }

            private struct ControllerStatus
            {
                public ControllerConnectStatus online;
                public SPIEventConnect info;
            }

            private enum PoseErrorType
            {
                BRIGHT_LIGHT_ERROR = (1 << 0),
                LOW_LIGHT_ERROR = (1 << 1),
                LOW_FEATURE_COUNT_ERROR = (1 << 2),
                CAMERA_CALIBRATION_ERROR = (1 << 3),
                RELOCATION_IN_PROGRESS = (1 << 4),
                INITILIZATION_IN_PROGRESS = (1 << 5),
                NO_CAMERA_ERROR = (1 << 6),
                NO_IMU_ERROR = (1 << 7),
                IMU_JITTER_ERROR = (1 << 8),
                UNKNOWN_ERROR = (1 << 9)
            }

            private struct SixDof
            {
                public long timestamp;
                public double x; // positon X
                public double y; // position Y
                public double z; // position Z
                public double rw; // rotation W
                public double rx; // rotation X
                public double ry; // rotation Y
                public double rz; // rotation Z
                public byte type; //1:6DOF 0:3DOF 
                public byte confidence; //1:good 0:bad
                public PoseErrorType error;
                public double plane_height;
                public byte plane_status;
                public byte relocation_status;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
                public byte[] reserved;
            }
            
            private struct AlgoResult
            {
                public SixDof pose;
                public SixDof relocation_pose;
                public double vx, vy, vz; //
                public double ax, ay, az; //
                public double wx, wy, wz; //
                public double w_ax, w_ay, w_az; //

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
                public byte[] reserved;
            }
            #endregion

            #region Private Fields
            private ControllerStatus _leftStatus = new ControllerStatus();
            private ControllerStatus _rightStatus = new ControllerStatus();
            private SixDof _headData = new SixDof();
            private AlgoResult _gLeftData = new AlgoResult();
            private AlgoResult _gRightData = new AlgoResult();
            private AlgoResult _leftData = new AlgoResult();
            private AlgoResult _rightData = new AlgoResult();

            private float _toAdjustHeight = 0f;  // use for adjusting the feet of Avatar to fit to the ground
            private float _tPoseEyeHeight = 0f;  // record the eye height in T-pose.
            private bool _isFirstFrame = true;

            private ErrorLogger _headDataLogger = new ErrorLogger();
            private ErrorLogger _controllerDataLogger = new ErrorLogger();
            private ErrorLogger _controllerStatusLogger = new ErrorLogger();
            #endregion

            #region Public Methods
            /// <summary>
            ///Adjust the feet of Avatar to fit to the ground(requires the user to stand upright on the ground, not squatting, not jumping).
            /// </summary>
            public virtual void FitGround()
            {
                _toAdjustHeight = (float)_headData.y - _tPoseEyeHeight;
            }
            #endregion

            #region Internal Methods
            internal override void Initialize(System.IntPtr nativeHandler_, AvatarEntity owner)
            {
                if (owner == null || owner.bodyAnimController == null) return;

                base.Initialize(nativeHandler_, owner);

                const float kEyeHeightOffset = 0.012f;
                _tPoseEyeHeight = _owner.bodyAnimController.GetEyeXForm().position.y + kEyeHeightOffset;
            }

            internal override void UpdateButtonStatus()
            {
                base.UpdateButtonStatus();
            }

            internal override void UpdateConnectionStatus()
            {
                base.UpdateConnectionStatus();
                
                for (int i = 0; i < (int)DeviceType.Count; ++i)
                {
                    deviceData.connectionStatus[i] = 1;
                }

                int ret = PT_GetControllerStatus(ref _leftStatus, ref _rightStatus);
                if (ret != 0)
                {
                    _controllerStatusLogger.LogErrorWithInterval($"PT_GetControllerStatus error code is {ret}");
                    return;
                }

                deviceData.connectionStatus[(uint)DeviceType.LeftHand] = (uint)(_leftStatus.online == ControllerConnectStatus.CONTROLLER_ONLINE ? 1 : 0);
                deviceData.connectionStatus[(uint)DeviceType.RightHand] = (uint)(_rightStatus.online == ControllerConnectStatus.CONTROLLER_ONLINE ? 1 : 0);
            }

            internal override void UpdateDevicePose()
            {
                if (_owner == null || _owner.bodyAnimController == null) return;
                
                base.UpdateDevicePose();

                int ret = PT_GetHeadTrackingData(ref _headData);
                if (ret != 0)
                {
                    _headDataLogger.LogErrorWithInterval($"PT_GetHeadTrackingData error code is {ret}");
                    return;
                }

                ret = PT_GetControllerData(ref _gLeftData, ref _gRightData, ref _leftData, ref _rightData);
                if (ret != 0)
                {
                    _controllerDataLogger.LogErrorWithInterval($"PT_GetControllerData error code is {ret}");
                    return;
                }

                ProcessTrackingData();

                if (isAvatarHubMode)
                {
                    DisableMoving();
                    DisableJumping();
                }

                deviceData.positions[(int)DeviceType.Head].Set((float)_headData.x, (float)_headData.y, (float)_headData.z);
                deviceData.orientations[(int)DeviceType.Head].Set((float)_headData.rx, (float)_headData.ry, (float)_headData.rz, (float)_headData.rw);

                deviceData.positions[(int)DeviceType.LeftHand].Set((float)_leftData.pose.x, (float)_leftData.pose.y, (float)_leftData.pose.z);
                deviceData.orientations[(int)DeviceType.LeftHand].Set((float)_leftData.pose.rx, (float)_leftData.pose.ry, (float)_leftData.pose.rz, (float)_leftData.pose.rw);

                deviceData.positions[(int)DeviceType.RightHand].Set((float)_rightData.pose.x, (float)_rightData.pose.y, (float)_rightData.pose.z);
                deviceData.orientations[(int)DeviceType.RightHand].Set((float)_rightData.pose.rx, (float)_rightData.pose.ry, (float)_rightData.pose.rz, (float)_rightData.pose.rw);

                deviceArmSpan = Vector3.Distance(deviceData.positions[(int)DeviceType.LeftHand], deviceData.positions[(int)DeviceType.RightHand]);

                // device offsets
                for (int i = 0; i < (int)DeviceType.Count; ++i)
                {
                    deviceData.orientations[i] = deviceData.orientations[i] * deviceRotationOffsets[i];
                    Vector3 positionOffset = deviceData.orientations[i] * deviceOffsets[i];
                    positionOffset = Vector3.Scale(positionOffset, _owner.GetAvatarScale());
                    deviceData.positions[i] -= positionOffset;
                }
            }
            #endregion

            #region Private Methods
            private void ProcessTrackingData()
            {
                // Record the '_toAdjustHeight' value.
                if (_isFirstFrame)
                {
                    FitGround();
                    _isFirstFrame = false;
                }

                // Process head data
                _headData.z *= -1;
                _headData.rz *= -1;
                _headData.rw *= -1;
                _headData.y -= _toAdjustHeight;  // Adjust the height of both feet to fit the ground.

                // Process left hand data
                _leftData.pose.x /= 1000f;
                _leftData.pose.y /= 1000f;
                _leftData.pose.z /= 1000f;
                _leftData.pose.z *= -1;
                _leftData.pose.y -= _toAdjustHeight;
                    
                _gLeftData.pose.x /= 1000f;
                _gLeftData.pose.y /= 1000f;
                _gLeftData.pose.z /= 1000f;
                _gLeftData.pose.z *= -1;
                _gLeftData.pose.y -= _toAdjustHeight;
                
                // Process right hand data
                _rightData.pose.x /= 1000f;
                _rightData.pose.y /= 1000f;
                _rightData.pose.z /= 1000f;
                _rightData.pose.z *= -1;
                _rightData.pose.y -= _toAdjustHeight;

                _gRightData.pose.x /= 1000f;
                _gRightData.pose.y /= 1000f;
                _gRightData.pose.z /= 1000f;
                _gRightData.pose.z *= -1;
                _gRightData.pose.y -= _toAdjustHeight;
            }

            // Fix the XZ coordinate of Avatar.
            private void DisableMoving()
            {
                _leftData.pose.x -= _headData.x;
                _leftData.pose.z -= _headData.z;

                _rightData.pose.x -= _headData.x;
                _rightData.pose.z -= _headData.z;

                _headData.x = 0;
                _headData.z = 0;

                _owner.bodyAnimController.needUpdateEntityUnityXFormFromNative = false;
            }

            // Fix the Avatar's feet on the ground.
            private void DisableJumping()
            {
                if (_headData.y > _tPoseEyeHeight)
                {
                    var deltaHeight = _headData.y - _tPoseEyeHeight;
                    _leftData.pose.y -= deltaHeight;
                    _rightData.pose.y -= deltaHeight;
                    _headData.y = _tPoseEyeHeight;
                }
            }
            #endregion

            #region Native Methods
            protected const string libTrackingName = "libtracking";

            [DllImport(libTrackingName, CallingConvention = CallingConvention.Cdecl)]
            private static extern int PT_GetHeadTrackingData(ref SixDof sixDof);

            [DllImport(libTrackingName, CallingConvention = CallingConvention.Cdecl)]
            private static extern int PT_GetControllerData(ref AlgoResult gLeft, ref AlgoResult gRight, ref AlgoResult left,
                ref AlgoResult right);

            [DllImport(libTrackingName, CallingConvention = CallingConvention.Cdecl)]
            private static extern int PT_GetControllerStatus(ref ControllerStatus left, ref ControllerStatus right);
            #endregion
        }
    }
}
#else
using Unity.XR.PXR;
using System.Threading;

namespace Pico
{
	namespace Avatar
	{
		public class PXRDeviceInputReader : XRDeviceInputReader
		{
			#region Public Fields

			public float unitScale = 1;
			public PxrControllerTracking pxrControllerTrackingLeft = new PxrControllerTracking();
			public PxrControllerTracking pxrControllerTrackingRight = new PxrControllerTracking();
			public float[] headData = new float[7] { 0, 0, 0, 0, 0, 0, 0 };

			#endregion


			#region Internal Methods

			// Destroy the object and release native reference count.
			//  Derived class MUST invoke the method if override it.
			protected override void OnDestroy()
			{
				// wait reading thread finished.
				while (Interlocked.CompareExchange(ref _deviceAccessState, (int)DeviceAccessState.None,
					       (int)DeviceAccessState.Reading) == (int)DeviceAccessState.Reading)
				{
					Thread.Sleep(0);
				}

				// clear main thread state.
				_deviceAccessState = (int)DeviceAccessState.None;
				// Do Nothing.
				base.OnDestroy();
			}

			internal override void InitInputFeatureUsage()
			{
				base.InitInputFeatureUsage();
			}

			internal override void UpdateConnectionStatus()
			{
				base.UpdateConnectionStatus();
#if UNITY_EDITOR
				return;
#endif

				if (Interlocked.CompareExchange(ref _deviceAccessState, (int)DeviceAccessState.Consuming,
					    (int)DeviceAccessState.Ready) != (int)DeviceAccessState.Ready)
				{
					return;
				}

				//updateControllerStatus();
				{
					uint leftControllerStatus = (uint)pxrControllerTrackingLeft.localControllerPose.status;

					//AvatarEnv.Log(DebugLogMask.AvatarIK, "left Controller Status: " + leftControllerStatus.ToString());

					uint rightControllerStatus = (uint)pxrControllerTrackingRight.localControllerPose.status;
					if (deviceData.connectionStatus[(uint)DeviceType.LeftHand] > 0 && leftControllerStatus == 0)
					{
						deviceData.connectionStatus[(uint)DeviceType.LeftHand] = (uint)ConnectionStatus.Idle;
					}
					else
					{
						deviceData.connectionStatus[(uint)DeviceType.LeftHand] = leftControllerStatus;
					}

					if (deviceData.connectionStatus[(uint)DeviceType.RightHand] > 0 && rightControllerStatus == 0)
					{
						deviceData.connectionStatus[(uint)DeviceType.RightHand] = (uint)ConnectionStatus.Idle;
					}
					else
					{
						deviceData.connectionStatus[(uint)DeviceType.RightHand] = rightControllerStatus;
					}
				}

				if (Interlocked.CompareExchange(ref _deviceAccessState, (int)DeviceAccessState.None,
					    (int)DeviceAccessState.Consuming) != (int)DeviceAccessState.Consuming)
				{
					UnityEngine.Debug.LogError("ReadControllerStatusT swaping corrupted!");
				}
			}

			internal override void UpdateDevicePose()
			{
				base.UpdateDevicePose();
				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					deviceData.positions[i] *= unitScale;
				}
			}

			#endregion


			#region Async Read

			// access state for control device.
			private enum DeviceAccessState : int
			{
				None = 0,
				Reading = 1,
				Ready = 2,
				Consuming = 3,
			}

			// current access state for control device. should be accessed or modified thread safely.
			private volatile int _deviceAccessState = (int)DeviceAccessState.None;

			// Invoked in late update to prepare next frame.
			internal override void StartReadDeviceForNextFrame()
			{
				// if is already in reading state, do nothing.
				if (Interlocked.CompareExchange(ref _deviceAccessState, (int)DeviceAccessState.Reading,
					    (int)DeviceAccessState.Reading) == (int)DeviceAccessState.Reading)
				{
					return;
				}

				//
				ThreadPool.QueueUserWorkItem(new WaitCallback(ReadControllerStatusT));
			}

			private void ReadControllerStatusT(object state)
			{
				while (Interlocked.CompareExchange(ref _deviceAccessState, (int)DeviceAccessState.Reading,
					       (int)DeviceAccessState.None) != (int)DeviceAccessState.None
				       && Interlocked.CompareExchange(ref _deviceAccessState, (int)DeviceAccessState.Reading,
					       (int)DeviceAccessState.Ready) != (int)DeviceAccessState.Ready)
				{
					Thread.Sleep(0);
				}

				PXR_Plugin.Controller.UPxr_GetControllerTrackingState((uint)PXR_Input.Controller.LeftController, 0,
					headData, ref pxrControllerTrackingLeft);
				PXR_Plugin.Controller.UPxr_GetControllerTrackingState((uint)PXR_Input.Controller.RightController, 0,
					headData, ref pxrControllerTrackingRight);

				if (Interlocked.CompareExchange(ref _deviceAccessState, (int)DeviceAccessState.Ready,
					    (int)DeviceAccessState.Reading) != (int)DeviceAccessState.Reading)
				{
					UnityEngine.Debug.LogError("ReadControllerStatusT thread confliction!");
				}
			}

			#endregion
		}
	}
}
#endif