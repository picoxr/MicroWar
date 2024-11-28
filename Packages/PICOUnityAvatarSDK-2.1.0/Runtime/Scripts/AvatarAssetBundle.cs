using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Asset Bundle
		/// </summary>
		public class AvatarAssetBundle : NativeObject
		{
			#region Public Methods
			public AvatarAssetBundle()
			{
				SetNativeHandle(pav_AssetBundle_Create(), false);
			}

			/// <summary>
			/// LoadFromZipFile
			/// </summary>
			/// <param name="path">zip file path</param>
			/// <returns>Enum:NativeResult</returns>
			/// <exception cref="Exception">AssetBundle closed</exception>
			public NativeResult LoadFromZipFile(string path)
			{
				if (nativeHandle == System.IntPtr.Zero)
				{
					throw new System.Exception("AssetBundle closed.");
				}

				return pav_AssetBundle_LoadFromZipFile(nativeHandle, path);
			}

			/// <summary>
			/// LoadFromZipMemoryView
			/// </summary>
			/// <param name="mv">Native memory view that wraps a chunk of memory.</param>
			/// <returns>Enum:NativeResult</returns>
			/// <exception cref="Exception">AssetBundle closed</exception>
			public NativeResult LoadFromZipMemoryView(MemoryView mv)
			{
				if (nativeHandle == System.IntPtr.Zero)
				{
					throw new System.Exception("AssetBundle closed.");
				}

				return pav_AssetBundle_LoadFromZipMemoryView(nativeHandle, mv.nativeHandle);
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AssetBundle_Create();

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AssetBundle_LoadFromZipFile(System.IntPtr nativeHandle
				, string path);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AssetBundle_LoadFromZipMemoryView(System.IntPtr nativeHandle
				, System.IntPtr mv);

			#endregion
		}
	}
}