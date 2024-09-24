using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		namespace IO
		{
			//Archive file system. ONLY used internally.
			public class ArchiveFileSystem : NativeObject
			{
				#region Public Methods

				public ArchiveFileSystem()
				{
					SetNativeHandle(pav_ArchiveFileSystem_Create(), false);
				}

				public NativeResult LoadFromFile(string archiveFilePathName)
				{
					if (nativeHandle == System.IntPtr.Zero)
					{
						throw new System.Exception("PicoArchiveFileSystem closed.");
					}

					return pav_ArchiveFileSystem_Load(nativeHandle, archiveFilePathName);
				}

				// Load from bytes.
				public NativeResult LoadFromBytes(byte[] bytesData)
				{
					var mv = new MemoryView(bytesData, false);
					var result = pav_ArchiveFileSystem_LoadFromMemoryView(nativeHandle, mv.nativeHandle);
					mv.CheckDelete();
					return result;
				}

				// Archive specified files to a archive file.
				/// <param name="srcDirPathName"></param>
				/// <param name="targetFilePathName">output file path name.</param>
				/// <param name="fileNames">file name list relative to srcDirPathName and seperated with '|'.</param>
				/// <param name="flags"></param>
				/// <returns></returns>
				public static NativeResult ArchiveFiles(string srcDirPathName, string targetFilePathName,
					string fileNames, uint flags = 0)
				{
					return pav_ArchiveFileSystem_ArchiveFiles(srcDirPathName, targetFilePathName, fileNames, flags);
				}

				public static NativeResult ArchiveAllFiles(string srcDirPathName, string targetFilePathName)
				{
					// JSCompress jsCompress = new JSCompress(srcDirPathName);
					return pav_ArchiveFileSystem_ArchiveAllFiles(srcDirPathName, targetFilePathName);
				}

				public NativeResult UnarchiveFiles(string targetDirPathName)
				{
					if (nativeHandle == System.IntPtr.Zero)
					{
						throw new System.Exception("PicoArchiveFileSystem closed.");
					}

					return pav_ArchiveFileSystem_UnarchiveFilesToDirectory(nativeHandle, targetDirPathName);
				}

				public string GetFileList(string directory)
				{
					if (nativeHandle == System.IntPtr.Zero)
					{
						throw new System.Exception("PicoArchiveFileSystem closed.");
					}

					uint bufferLen = Pico.Avatar.Utility.sharedStringBuffer.length;
					int len = pav_ArchiveFileSystem_GetFileList(nativeHandle, directory,
						Pico.Avatar.Utility.sharedStringBuffer.Lock(), (int)bufferLen);
					Pico.Avatar.Utility.sharedStringBuffer.Unlock();
					return Pico.Avatar.Utility.sharedStringBuffer.GetANSIString((uint)len);
				}

				#endregion


				#region Native Methods

				const string PavDLLName = DllLoaderHelper.PavDLLName;

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern System.IntPtr pav_ArchiveFileSystem_Create();

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern NativeResult pav_ArchiveFileSystem_Load(System.IntPtr nativeHandle
					, string archiveFilePathName);

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern NativeResult pav_ArchiveFileSystem_LoadFromMemoryView(System.IntPtr nativeHandle
					, System.IntPtr memoryViewHandle);

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern NativeResult pav_ArchiveFileSystem_ArchiveFiles(string srcDirPathName,
					string targetFilePathName, string fileNames, uint flags);

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern NativeResult pav_ArchiveFileSystem_ArchiveAllFiles(string srcDirPathName,
					string targetFilePathName);

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern NativeResult pav_ArchiveFileSystem_UnarchiveFilesToDirectory(
					System.IntPtr nativeHandle, string targetDirPathName);

				[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
				private static extern int pav_ArchiveFileSystem_GetFileList(System.IntPtr nativeHandle,
					string directory, System.IntPtr fileList, int bufferLen);

				#endregion
			}
		}
	}
}