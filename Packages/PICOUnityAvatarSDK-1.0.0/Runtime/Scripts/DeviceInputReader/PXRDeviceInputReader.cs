#if NO_XR
namespace Pico
{
    namespace Avatar
    {
        
        public class PXRDeviceInputReader : XRDeviceInputReader
        {
	        internal override void InitInputFeatureUsage()
            {
            }
	        internal override void UpdateButtonStatus()
            {
            }
	        internal override void UpdateConnectionStatus()
            {
            }
	        internal override void UpdateDevicePose()
            {
            }
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