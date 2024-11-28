using UnityEngine;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		// parameter table with key id.  Used internally.
		[UnityEngine.Scripting.PreserveAttribute]
		public class IDParameterTable : NativeObject
		{
			#region Types

			// param type.
			public enum ParamType
			{
				Byte = 0,
				Int,
				Float,
				UInt,
				Vector3,
				Vector4,
				IntPtr,
				String,
			}

			#endregion


			#region Public Methods

			// 
			[UnityEngine.Scripting.PreserveAttribute]
			public IDParameterTable()
			{
				//
				SetNativeHandle(pav_IDParameterTable_New(), false);

				// check out of memory exception.
				if (nativeHandle == System.IntPtr.Zero)
				{
					throw new System.OutOfMemoryException("IDParameterTable.New");
				}
			}

			[UnityEngine.Scripting.PreserveAttribute]
			public IDParameterTable(System.IntPtr nativeHandle_)
			{
				// check out of memory exception.
				if (nativeHandle_ == System.IntPtr.Zero)
				{
					throw new System.OutOfMemoryException("IDParameterTable.New");
				}

				SetNativeHandle(nativeHandle_, false);
			}

			// Gets type count of the type.
			public int GetParamCount(ParamType pt)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetParamCount(nativeHandle, (uint)pt);
			}
			
			public void CopyFrom(IDParameterTable parameterTable)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();

				pav_IDParameterTable_CopyFrom(nativeHandle, parameterTable.nativeHandle);
			}
			
			public void Clear()
			{
				pav_IDParameterTable_Clear(nativeHandle);
			}
			
			#region Set/Get Parameter By ID.

			// Sets byte parameter.
			public void SetBoolParam(uint paramId, bool val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetByteParam(nativeHandle, (uint)paramId, (byte)(val ? 1 : 0));
			}

			// Gets int parameter.
			public NativeResult GetBoolParam(uint paramId, ref bool val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				byte byteVal = (byte)(val ? 1 : 0);
				var ret = pav_IDParameterTable_GetByteParam(nativeHandle, paramId, ref byteVal);
				val = byteVal == 0 ? false : true;
				return ret;
			}

			// Sets byte parameter.
			public void SetByteParam(uint paramId, byte val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetByteParam(nativeHandle, (uint)paramId, val);
			}

			// Gets int parameter.
			public NativeResult GetByteParam(uint paramId, ref byte val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetByteParam(nativeHandle, paramId, ref val);
			}

			// Sets int parameter.
			public void SetUIntParam(uint paramId, uint val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetUIntParam(nativeHandle, (uint)paramId, val);
			}

			// Gets int parameter.
			public NativeResult GetUIntParam(uint paramId, ref uint val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetUIntParam(nativeHandle, paramId, ref val);
			}
			// Gets int parameter.
			public NativeResult GetUIntParam(int paramId, ref int val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetUIntParam(nativeHandle, paramId, ref val);
			}

            // Sets int parameter.
            public void SetIntParam(uint paramId, int val)
            {
                if (nativeHandle == System.IntPtr.Zero)
                    throw new System.InvalidOperationException();
                pav_IDParameterTable_SetUIntParam(nativeHandle, (uint)paramId, (uint)val);
            }

            // Gets int parameter.
            public NativeResult GetIntParam(uint paramId, ref int val)
            {
                if (nativeHandle == System.IntPtr.Zero)
                    throw new System.InvalidOperationException();
				uint uval = (uint)val;
                var ret = pav_IDParameterTable_GetUIntParam(nativeHandle, paramId, ref uval);
				val = (int)uval;
				return ret;
            }

            // Sets float parameter.
            public void SetFloatParam(uint paramId, float val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetFloatParam(nativeHandle, (uint)paramId, val);
			}

			// Gets float parameter.
			public NativeResult GetFloatParam(uint paramId, ref float val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetFloatParam(nativeHandle, paramId, ref val);
			}

			// Sets ulong parameter.
			public void SetULongParam(uint paramId, ulong val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetULongParam(nativeHandle, (uint)paramId, val);
			}

			// Gets ulong parameter.
			public NativeResult GetULongParam(uint paramId, ref ulong val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetULongParam(nativeHandle, paramId, ref val);
			}

			// Sets IntPtr parameter.
			public void SetObjectParam(uint paramId, System.IntPtr val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetObjectParam(nativeHandle, (uint)paramId, val);
			}

			// Gets IntPtr parameter.
			public NativeResult GetObjectParam(uint paramId, ref System.IntPtr val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetObjectParam(nativeHandle, paramId, ref val);
			}

			// Sets Vector3 parameter.
			public void SetVector3Param(uint paramId, Vector3 val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetVector3Param(nativeHandle, (uint)paramId, ref val);
			}

			// Gets Vector3 parameter.
			public NativeResult GetVector3Param(uint paramId, ref Vector3 val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetVector3Param(nativeHandle, paramId, ref val);
			}

			// Sets Point parameter.
			public void SetPointParam(uint paramId, Vector3 val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetPointParam(nativeHandle, (uint)paramId, ref val);
			}

			// Gets Point parameter.
			public NativeResult GetPointParam(uint paramId, ref Vector3 val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetPointParam(nativeHandle, paramId, ref val);
			}

			// Sets Vector4 parameter.
			public void SetVector4Param(uint paramId, Vector4 val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetVector4Param(nativeHandle, (uint)paramId, ref val);
			}

			// Gets Vector4 parameter.
			public NativeResult GetVector4Param(uint paramId, ref Vector4 val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				return pav_IDParameterTable_GetVector4Param(nativeHandle, paramId, ref val);
			}

			// Sets Quaternion parameter.
			public void SetQuaternionParam(uint paramId, Quaternion val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();

				pav_IDParameterTable_SetQuaternionParam(nativeHandle, paramId, ref val);
			}

			// Gets Quaternion parameter.
			public NativeResult GetQuaternionParam(uint paramId, ref Quaternion val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();

				var result = pav_IDParameterTable_GetQuaternionParam(nativeHandle, paramId, ref val);

				return result;
			}
			
            //Sets string.
            //@warning Only short string supported. Usually no more than 1MB. It is enough for most cases.
            public void SetStringParam(uint paramId, string val)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();
				pav_IDParameterTable_SetStringParam(nativeHandle, (uint)paramId, val);
			}
            
            //Gets string..
            //@warning Only short string supported. Usually no more than 1MB. It is enough for most cases.
            public string GetStringParam(uint paramId)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();

				uint len = Pico.Avatar.Utility.sharedStringBuffer.length;
				//
				var charCount = (int)pav_IDParameterTable_GetStringParam(nativeHandle, (uint)paramId
					, Pico.Avatar.Utility.sharedStringBuffer.Lock(), (int)len);
				//
				Pico.Avatar.Utility.sharedStringBuffer.Unlock();
				//
				return Pico.Avatar.Utility.sharedStringBuffer.GetANSIString((uint)charCount);
			}
            
            //Gets string..
            //@warning Only short string supported. Usually no more than 1MB. It is enough for most cases.
            public string GetUTF8StringParam(uint paramId)
			{
				if (nativeHandle == System.IntPtr.Zero)
					throw new System.InvalidOperationException();

				uint len = Pico.Avatar.Utility.sharedStringBuffer.length;
				//
				var charCount = (int)pav_IDParameterTable_GetStringParam(nativeHandle, (uint)paramId
					, Pico.Avatar.Utility.sharedStringBuffer.Lock(), (int)len);
				//
				Pico.Avatar.Utility.sharedStringBuffer.Unlock();
				//
				return Pico.Avatar.Utility.sharedStringBuffer.GetUTF8String((uint)charCount);
			}

			#endregion

			#region Set/Get Parameter By String.
			
            //Sets byte parameter.
            public void SetByteParam(string paramName, byte val)
			{
				// get id from id-name table.
				SetByteParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), val);
			}
            
            //Gets int parameter.
            public NativeResult GetByteParam(string paramName, ref byte val)
			{
				return GetByteParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), ref val);
			}
            
            //Sets int parameter.
            public void SetUIntParam(string paramName, uint val)
			{
				// get id from id-name table.
				SetUIntParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), val);
			}
            
            //Gets int parameter.
            public NativeResult GetUIntParam(string paramName, ref uint val)
			{
				return GetUIntParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), ref val);
			}
            
            //Sets float parameter.
            public void SetFloatParam(string paramName, float val)
			{
				// get id from id-name table.
				SetFloatParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), val);
			}
            
            //Gets float parameter.
            public NativeResult GetFloatParam(string paramName, ref float val)
			{
				return GetFloatParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), ref val);
			}

			// Sets ulong parameter.
			public void SetULongParam(string paramName, ulong val)
			{
				// get id from id-name table.
				SetULongParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), val);
			}

			// Gets ulong parameter.
			public NativeResult GetULongParam(string paramName, ref ulong val)
			{
				return GetULongParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), ref val);
			}
			
            //Sets IntPtr parameter.
            //It is a native object.
            public void SetObjecParam(string paramName, System.IntPtr val)
			{
				// get id from id-name table.
				SetObjectParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), val);
			}
            
            //Gets IntPtr parameter.
            public NativeResult GetObjectParam(string paramName, ref System.IntPtr val)
			{
				return GetObjectParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), ref val);
			}
            
            //Sets Vector3 parameter.
            public void SetVector3Param(string paramName, Vector3 val)
			{
				// get id from id-name table.
				SetVector3Param(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), val);
			}
            
            //Gets Vector3 parameter.
            public NativeResult GetVector3Param(string paramName, ref Vector3 val)
			{
				return GetVector3Param(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), ref val);
			}
            
            //Sets Vector4 parameter.
            public void SetVector4Param(string paramName, Vector4 val)
			{
				// get id from id-name table.
				SetVector4Param(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), val);
			}
            
            //Gets Vector4 parameter.
            public NativeResult GetVector4Param(string paramName, ref Vector4 val)
			{
				return GetVector4Param(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), ref val);
			}
            
            //Sets string.
            public void SetStringParam(string paramName, string val)
			{
				// get id from id-name table.
				SetStringParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName), val);
			}
            
            //Gets string.
            public string GetStringParam(string paramName)
			{
				return GetStringParam(Pico.Avatar.Utility.AddNameToIDNameTable(paramName));
			}

			#endregion

			#endregion


			#region Native Part

			const string PavDLLName = DllLoaderHelper.PavDLLName;


			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_IDParameterTable_New();
			
            //查询参数数量.
            //@param paramValueType 0:int 1:float 2:string.
            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_IDParameterTable_GetParamCount(System.IntPtr dataHandle, uint paramValueType);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_Clear(System.IntPtr dataHandle);

			// byte
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetByteParam(System.IntPtr dataHandle, uint paramId,
				byte val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetByteParam(System.IntPtr dataHandle, uint paramId,
				ref byte val);

			// int
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetUIntParam(System.IntPtr dataHandle, uint paramId,
				uint val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetUIntParam(System.IntPtr dataHandle, uint paramId,
				ref uint val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetUIntParam(System.IntPtr dataHandle, int paramId,
				ref int val);
			
			// float
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetFloatParam(System.IntPtr dataHandle, uint paramId,
				float val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetFloatParam(System.IntPtr dataHandle,
				uint paramId, ref float val);

			// ulong
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetULongParam(System.IntPtr dataHandle, uint paramId,
				ulong val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetULongParam(System.IntPtr dataHandle,
				uint paramId, ref ulong val);

			// System.IntPtr
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetObjectParam(System.IntPtr dataHandle, uint paramId,
				System.IntPtr val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetObjectParam(System.IntPtr dataHandle,
				uint paramId, ref System.IntPtr val);

			// Vector3
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetVector3Param(System.IntPtr dataHandle, uint paramId,
				ref Vector3 val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetVector3Param(System.IntPtr dataHandle,
				uint paramId, ref Vector3 val);

			// Point
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetPointParam(System.IntPtr dataHandle, uint paramId,
				ref Vector3 val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetPointParam(System.IntPtr dataHandle,
				uint paramId, ref Vector3 val);

			// Vector4
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetVector4Param(System.IntPtr dataHandle, uint paramId,
				ref Vector4 val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetVector4Param(System.IntPtr dataHandle,
				uint paramId, ref Vector4 val);

			// Quaternion
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetQuaternionParam(System.IntPtr dataHandle, uint paramId,
				ref Quaternion val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_IDParameterTable_GetQuaternionParam(System.IntPtr dataHandle,
				uint paramId, ref Quaternion val);


			// string
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_SetStringParam(System.IntPtr dataHandle, uint paramId,
				string val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_IDParameterTable_GetStringParam(System.IntPtr dataHandle, uint paramId,
				System.IntPtr charBuffer, int bufferSize);

			//CopyFrom
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_IDParameterTable_CopyFrom(System.IntPtr dataHandle,
				System.IntPtr parameterTable);

			#endregion
		}
	}
}