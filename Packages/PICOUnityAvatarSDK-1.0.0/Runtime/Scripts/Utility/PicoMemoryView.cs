using System;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Native memory view that wraps a chunk of memory.
		/// @note It is reference count based object, thus you should invoke Retain/Release to manage the lifetime of the object.
		/// </summary>
		public class MemoryView : NativeObject
		{
			#region Public Properties

			// memory chunk byte size.
			public uint length
			{
				get => nativeHandle == System.IntPtr.Zero ? 0 : (uint)_length;
			}

			// memory chunk memory address.
			public System.IntPtr bufferAddress
			{
				get => nativeHandle == System.IntPtr.Zero ? System.IntPtr.Zero : pav_MemoryView_GetBuffer(nativeHandle);
			}

			// memory chunk byte size.
			public bool sharedExternal
			{
				get => nativeHandle == System.IntPtr.Zero ? false : pav_MemoryView_GetIsShared(nativeHandle);
			}

			#endregion


			#region Public Methods

			/// <summary>
			/// Constructor with native handle.
			/// </summary>
			/// <param name="nativeData">Native data</param>
			public MemoryView(NativeData nativeData)
			{
				SetNativeHandle(nativeData.handle, false);
				_length = nativeData.length;
			}

			/// <summary>
			/// Loads local file and cache data in c++ layer.
			/// </summary>
			/// <param name="localFilePathName">Local file path</param>
			public MemoryView(string localFilePathName)
			{
				var nativeData = pav_MemoryView_ReadBytesFromFile(localFilePathName);
				//
				SetNativeHandle(nativeData.handle, false);
				_length = nativeData.length;
			}

			/// <summary>
			/// Constructor with byte array.
			/// </summary>
			/// <param name="buffer">Data to set</param>
			/// <param name="shared">True if use shared mode, shared mode means MemoryView will not be responsible for the management of the memory</param>
			public MemoryView(byte[] buffer, bool shared)
			{
				if (buffer != null && buffer.Length > 0)
				{
					_SharedDataHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
					//
					var nativeHandle_ = pav_MemoryView_New(_SharedDataHandle.AddrOfPinnedObject(), (uint)buffer.Length,
						shared);

					if (!shared)
					{
						_SharedData = buffer;
						_SharedDataHandle.Free();
					}

					//
					SetNativeHandle(nativeHandle_, false);
					_length = buffer.Length;
				}
			}

			/// <summary>
			/// Gets handle to wrapped MemoryStream object.
			/// @remark No reference count added for returned object pointer.
			/// </summary>
			/// <returns>Pointer to the MemoryStream</returns>
			public System.IntPtr GetMemoryStreamPtr()
			{
				return pav_MemoryView_GetMemoryStreamPtr(nativeHandle);
			}

			/// <summary>
			/// Set memory of the object to native object by shared mode.
			/// @note Caller MUST assure the lifetime of the array.
			/// </summary>
			/// <param name="buffer">Data to set</param>
			/// <returns>True if success</returns>
			public bool TransferSharedBuffer(byte[] buffer)
			{
				if (nativeHandle == System.IntPtr.Zero)
				{
					return false;
				}

				// free self memory data.
				if (_SharedDataHandle.IsAllocated)
				{
					_SharedDataHandle.Free();
					_SharedData = null;
				}

				if (buffer == null)
				{
					pav_MemoryView_Reset(nativeHandle, System.IntPtr.Zero, (uint)0, false);
					return true;
				}

				//
				_SharedData = buffer;
				_SharedDataHandle = GCHandle.Alloc(_SharedData, GCHandleType.Pinned);

				// set new data.
				_length = buffer.Length;
				return pav_MemoryView_Reset(nativeHandle, _SharedDataHandle.AddrOfPinnedObject(), (uint)_length,
					true) == NativeResult.Success;
			}

			/// <summary>
			/// Sets data. Copy data to native object.
			/// @note TransferSharedBuffer are prefered if buffer object will not be used outisde the method.
			/// </summary>
			/// <param name="buffer">Data to set</param>
			/// <returns>True if success</returns>
			public bool SetData(byte[] buffer)
			{
				if (nativeHandle == System.IntPtr.Zero)
				{
					return false;
				}

				// free self memory data.
				if (_SharedDataHandle.IsAllocated)
				{
					_SharedDataHandle.Free();
					_SharedData = null;
				}

				if (buffer == null)
				{
					pav_MemoryView_Reset(nativeHandle, System.IntPtr.Zero, (uint)0, false);
					return true;
				}

				//
				var dDataHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

				// set new data.
				_length = buffer.Length;
				var result = pav_MemoryView_Reset(nativeHandle, dDataHandle.AddrOfPinnedObject(), (uint)_length, false);
				dDataHandle.Free();
				//
				return result == NativeResult.Success;
			}

			[Obsolete("Deprecated, use GetData instead!")]
			public byte[] getData(bool reGetFromNative = true)
			{
				return GetData(reGetFromNative);
			}


			/// <summary>
			/// Copy memory from native MemoryView no matter actual memory located.
			/// </summary>
			/// <param name="reGetFromNative">If true means to get data from native and not use the cached data</param>
			/// <returns>Data of MemoryView</returns>
			public byte[] GetData(bool reGetFromNative = true)
			{
				if (nativeHandle == System.IntPtr.Zero)
				{
					return null;
				}

				_length = (int)pav_MemoryView_GetSize(nativeHandle);

				//
				if (_SharedDataHandle.IsAllocated)
				{
					if (_length == _SharedData.Length)
					{
						return _SharedData;
					}

					_SharedDataHandle.Free();
					// reallocate
					System.Array.Resize<byte>(ref _SharedData, _length);
				}

				//
				if (_SharedData == null || _SharedData.Length != _length)
				{
					_SharedData = new byte[_length];
				}

				unsafe
				{
					var destHandle = GCHandle.Alloc(_SharedData, GCHandleType.Pinned);
					var destBuf = destHandle.AddrOfPinnedObject();

					void* buffer = (void*)this.bufferAddress;
					System.Buffer.MemoryCopy(buffer, (void*)destBuf, _length, _length);
					destHandle.Free();
				}

				return _SharedData;
			}

			/// <summary>
			/// Get MemoryView size.
			/// </summary>
			/// <param name="reGetFromNative">If true means to get data from native and not use the cached data</param>
			/// <returns>MemoryView size</returns>
			public int GetSize(bool reGetFromNative = true)
			{
				if (nativeHandle == System.IntPtr.Zero)
				{
					return 0;
				}

				//
				_length = (int)pav_MemoryView_GetSize(nativeHandle);
				//
				return _length;
			}

			/// <summary>
			/// Save to local file.
			/// </summary>
			/// <param name="filePathName">File path to save</param>
			/// <returns>Write result</returns>
			public NativeResult WriteBytesToFile(string filePathName)
			{
				if (nativeHandle == System.IntPtr.Zero)
				{
					return NativeResult.InvalidObject;
				}

				//
				return pav_MemoryView_WriteBytesToFile(nativeHandle, filePathName);
			}

			/// <summary>
			/// Read bytes from file and return the memory view.
			/// </summary>
			/// <param name="filePathName">Read file path</param>
			/// <returns>The MemoryView created from file</returns>
			public static MemoryView ReadBytesFromFile(string filePathName)
			{
				var data = pav_MemoryView_ReadBytesFromFile(filePathName);
				if (data.handle == System.IntPtr.Zero)
				{
					return null;
				}

				var mv = new MemoryView(data);
				return mv;
			}

			/** 
             * Destroy the object and release native reference count.
             * @note Derived class MUST invoke the method if override it.
             */
			[UnityEngine.Scripting.PreserveAttribute]
			protected override void OnDestroy()
			{
				//
				if (_SharedData != null)
				{
					if (_SharedDataHandle.IsAllocated)
					{
						_SharedDataHandle.Free();
						_SharedData = null;
						//
						pav_MemoryView_Reset(nativeHandle, System.IntPtr.Zero, (uint)0, false);
					}

					_SharedData = null;
				}

				// Do Nothing.
				base.OnDestroy();
			}

			#endregion


			#region Private Fields

			private byte[] _SharedData;
			private GCHandle _SharedDataHandle;

			private int _length = 0;

			#endregion


			#region Private Methods

			// Hide default constructor.
			private MemoryView()
			{
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[StructLayout(LayoutKind.Sequential)]
			public struct NativeData
			{
				public System.IntPtr handle; // native handle.
				public int length; // buffer bytes size.
			}

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_MemoryView_New(System.IntPtr buffer, uint size, bool shared);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_MemoryView_WriteBytesToFile(System.IntPtr handle,
				string localFilePathName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeData pav_MemoryView_ReadBytesFromFile(string localFilePathName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_MemoryView_Reset(System.IntPtr nativeHandle, System.IntPtr buffer,
				uint size, bool shared);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_MemoryView_GetSize(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_MemoryView_GetBuffer(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_MemoryView_GetIsShared(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_MemoryView_GetMemoryStreamPtr(System.IntPtr nativeHandle);

			#endregion
		}
	}
}