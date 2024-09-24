using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		namespace IO
		{
			// Helper method to invoke serialization method for Native Object. ONLY used internally.
			public class Serialization
			{
				#region Native Methods

				const string PavDLLName = DllLoaderHelper.PavDLLName;

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern bool pav_Serialization_LoadFromMemoryView(System.IntPtr obj,
					System.IntPtr memoryViewHandle);

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern MemoryView.NativeData pav_Serialization_SaveToMemoryView(
					System.IntPtr obj, uint resourcePlatform, uint encryptMethod);

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				public static extern bool pav_Serialization_ConvertGLTFToPrefab(string dependResDir,
					string gltfFilePath, string prefabDirPath, bool armNeon);

				#endregion
			}
		}
	}
}