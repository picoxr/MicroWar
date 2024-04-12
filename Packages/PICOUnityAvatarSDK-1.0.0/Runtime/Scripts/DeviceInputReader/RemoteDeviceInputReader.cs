using UnityEngine;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		public class RemoteDeviceInputReader : IDeviceInputReader
		{
			#region Public Fields

			public bool useEditorTarget = false;
			public Transform transRoot;
			public Transform transHead;
			public Transform transLeft;
			public Transform transRight;

			#endregion


			#region Internal Methods

			internal override void Initialize(System.IntPtr nativeHandler_, AvatarEntity owner)
			{
				base.Initialize(nativeHandler_, owner);
			}

			internal override void UpdateFrame()
			{
				pav_AvatarDeviceInputReader_GetDeviceInputData(nativeHandle, ref deviceData);
				UpdateDevicePose();
				UpdateConnectionStatus();
				UpdateButtonStatus();
			}

			internal override void UpdateDevicePose()
			{
				return;
			}

			internal override void UpdateConnectionStatus()
			{
				for (int i = 0; i < (int)DeviceType.Count; ++i)
				{
					connectionStatus[i] = deviceData.connectionStatus[i] > 0;
				}
			}

			internal override void UpdateButtonStatus()
			{
				for (int i = 0; i < (int)ControllerButtons.Count; ++i)
				{
					controllerButtonStatus[i] = deviceData.controllerButtonStatus[i] > 0;
				}
			}

			#endregion


			#region Native Methods

#if UNITY_IPHONE && !UNITY_EDITOR
	const string PavDLLName = "__Internal";
#else
			const string PavDLLName = DllLoaderHelper.PavDLLName;
#endif

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarDeviceInputReader_GetDeviceInputData(
				System.IntPtr nativeHandle, ref DeviceData deviceData);

			#endregion
		}
	}
}