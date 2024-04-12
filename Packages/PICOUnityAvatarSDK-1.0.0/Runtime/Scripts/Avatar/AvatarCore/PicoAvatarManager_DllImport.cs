using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		public partial class PicoAvatarManager
		{
			#region DllAPI

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarManager_New();

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarManager_Initialize(System.IntPtr nativeHandle,
				string avatarAppId, string avatarAppToken, string userToken, double startGameTime,
				ServerType serverType, AccessType accessType, string nationType, string configString,
				System.IntPtr sceneData);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarManager_Shutdown(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarManager_SyncNetSimulation(System.IntPtr nativeHandle,
				double timeStamp);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarManager_UpdateAvatarSimulationRenderDatas(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarManager_SetSkipUpdateWhenInvisibleThisFrame(
				System.IntPtr nativeHandle, bool skip);

			#endregion
		}
	}
}