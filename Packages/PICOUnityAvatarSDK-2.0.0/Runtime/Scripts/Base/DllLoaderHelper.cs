using UnityEngine;
using System.Runtime.InteropServices;

namespace Pico.Avatar
{
	public class DllLoaderHelper
	{
#if UNITY_IPHONE && !UNITY_EDITOR
	    public const string PavDLLName = "__Internal";
#else
		public const string PavDLLName = "avatarloader";
#endif
		const string LibEffectName = "effect";

		public static string LibEffectFileName
		{
			get
			{
				string prefix = "lib";
				string suffix = "so";
				RuntimePlatform curPlatform = Application.platform;
				switch (curPlatform)
				{
					case RuntimePlatform.Android:
						prefix = "lib";
						suffix = "so";
						break;
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
						prefix = "lib";
						suffix = "dylib";
						break;
					case RuntimePlatform.WindowsEditor:
					case RuntimePlatform.WindowsPlayer:
						prefix = "";
						suffix = "dll";
						break;
					case RuntimePlatform.LinuxPlayer:
					case RuntimePlatform.LinuxEditor:
#if UNITY_2021_3_OR_NEWER
					case RuntimePlatform.LinuxServer:
#endif
						prefix = "lib";
						suffix = "so";
						break;
				}

				string result = prefix + LibEffectName + "." + suffix;
				return result;
			}
		}

		public static void InitDllLoader(bool usingMatrix = false)
		{
			Debug.Log("InitDllLoader:" + LibEffectFileName);
			if (!usingMatrix)
			{
				Debug.Log("pav_AvatarCreate:" + LibEffectFileName);
				pav_AvatarCreate(LibEffectFileName, 1);
				return;
			}
#if UNITY_EDITOR
			Debug.Log("Editor Platform Start:pav_AvatarCreate:" + LibEffectFileName);
			pav_AvatarCreate(LibEffectFileName, 0);

#else
            Debug.Log("Android Matrix Service Start:pav_UnityInitWrapper:" + LibEffectFileName);
            int result = pav_UnityInitWrapper();
            Debug.Log("Android Matrix Service Finish Result:" + result);
#endif
		}

		public static byte[] GetMatrixJSBytes(bool usingMatrix = false)
		{
			if (!usingMatrix)
			{
				return null;
			}

			int length = pav_GetJSAssetLength();
			byte[] data = new byte[length];
			pav_GetJSRunAssetBytes(ref data[0]);
			Debug.Log("pav_GetJSRunAssetBytes,byte = " + data.Length);

			return data;
		}

		public static void HandleLackMethod(string desc, bool shouldPopDialog)
		{
			pav_HandleLackMethod(desc, shouldPopDialog);
		}
		//void Test()
		//{
		//    NativeArrayData data = new NativeArrayData(); ;
		//    byte[] temp = new byte[data.length];
		//    Marshal.Copy(data.handle, temp, 0, data.length);
		//}

		#region Native Methods

		[StructLayout(LayoutKind.Sequential)]
		public struct NativeArrayData
		{
			public System.IntPtr handle; // native handle.
			public int length; // buffer bytes size.
		}

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern NativeResult pav_AvatarCreate([MarshalAs(UnmanagedType.LPStr)] string libpath, int flags);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int pav_UnityInitWrapper();

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int pav_GetJSRunAssetBytes(ref byte data);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int pav_GetJSAssetLength();

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int pav_HandleLackMethod(string desc, bool shouldPopDialog);

		#endregion
	}
}