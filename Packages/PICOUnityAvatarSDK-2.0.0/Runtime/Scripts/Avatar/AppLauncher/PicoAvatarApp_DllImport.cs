using System.Runtime.InteropServices;
using UnityEngine;

namespace Pico.Avatar
{
	public partial class PicoAvatarApp : MonoBehaviour
	{
		#region Native Methods

		const string PavDLLName = DllLoaderHelper.PavDLLName;

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern System.IntPtr pav_AvatarApp_New(bool autoUnloadEngine);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern NativeResult pav_AvatarApp_SetConfig(System.IntPtr nativeHandle,
			ref AvatarAppConfig config);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern NativeResult pav_AvatarApp_PreInitialize(System.IntPtr nativeHandle, double starTime);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern NativeResult pav_AvatarApp_PostInitialize(System.IntPtr nativeHandle);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern NativeResult pav_AvatarApp_Update(System.IntPtr nativeHandle, double gameTime);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern NativeResult pav_AvatarApp_PreUninitialize(System.IntPtr nativeHandle);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern NativeResult pav_AvatarApp_PostUninitialize(System.IntPtr nativeHandle);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern System.IntPtr pav_AvatarApp_GetVersion();

		#endregion
	}
}