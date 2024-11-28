#if UNITY_EDITOR
using System.Runtime.InteropServices;

namespace Pico.Avatar
{
    public class PicoAvatarAccessory : NativeObject
    {
        #region Public Properties

        #endregion

        #region Private Fields

		#endregion

		#region Private Fields

		#endregion

		#region Native Methods

		const string PavDLLName = DllLoaderHelper.PavDLLName;

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern System.IntPtr pav_AvatarAccessory_DeformAccessory_Content(
			System.IntPtr baseFaceMeshData, System.IntPtr baseFaceMeshDiffData, string stemMeshIdx2ObjIdx,
			System.IntPtr assetMesh, string accessoryConfigContent);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern System.IntPtr pav_AvatarAccessory_DeformAccessory_Text(
			System.IntPtr baseFaceMeshData, System.IntPtr baseFaceMeshDiffData, string stemMeshIdx2ObjIdx,
			System.IntPtr assetMesh, string accessoryConfigPath);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern System.IntPtr pav_AvatarAccessory_DeformAccessory_BinContent(
			System.IntPtr baseFaceMeshData, System.IntPtr baseFaceMeshDiffData, string stemMeshIdx2ObjIdx,
			System.IntPtr assetMesh, System.IntPtr accessoryConfigContent);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern System.IntPtr pav_AvatarAccessory_DeformAccessory_Bin(
			System.IntPtr baseFaceMeshData, System.IntPtr baseFaceMeshDiffData, string stemMeshIdx2ObjIdx,
			System.IntPtr assetMesh, string accessoryConfigPath);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern System.IntPtr pav_AvatarAccessory_LoadPrefabData(
			string stemMeshIdx2ObjIdx, string accessoryConfigContent);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool pav_AvatarAccessory_LoadJsonConfig(string jsonConfigPath);

		[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool pav_AvatarAccessory_SaveJsonConfig(string jsonConfigPath);

		#endregion
	}
}

#endif