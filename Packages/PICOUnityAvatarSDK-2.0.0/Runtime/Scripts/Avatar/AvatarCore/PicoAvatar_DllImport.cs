using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		public partial class PicoAvatar
		{
			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarInstance_GetObject(uint nativeAvatarId);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarInstance_SetXForm(System.IntPtr avatarHandle, ref XForm xform);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarInstance_GetXForm(System.IntPtr avatarHandle, ref XForm xform);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult
				pav_AvatarInstance_CompactMemory(System.IntPtr avatarHandle, ulong flags);

			#endregion
		}
	}
}