using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		// It is a lookup table for JointType.
		// Can be obtained from AvatarBodyAnimController.
		public class TypeID2NameIDTable : NativeObject
		{
			#region Public Methods

			// Constructor with native handle.
			internal TypeID2NameIDTable(System.IntPtr nativeHandle_)
			{
				SetNativeHandle(nativeHandle_, false);
			}

			#endregion


			#region Public Fields

			/// <summary>
			/// Query joint name id from JointType, Some api need joint name id as input parameter.
			/// </summary>
			/// <param name="typeId">joint type id</param>
			/// <returns></returns>
			public uint GetNameID(uint typeId)
			{
				if (_cachedTypeId2NameIds == null)
				{
					_cachedTypeId2NameIds = new Dictionary<uint, uint>();
				}

				uint nameId = 0;
				if (_cachedTypeId2NameIds.TryGetValue(typeId, out nameId))
				{
					return nameId;
				}

				// search in native table.
				nameId = pav_TypeID2NameIDTable_GetNameID(nativeHandle, (uint)typeId);

				// cache ite.
				_cachedTypeId2NameIds.Add(typeId, nameId);

				return nameId;
			}

			#endregion


			#region Private Methods

			// Hide default constructor.
			private TypeID2NameIDTable()
			{
			}

			#endregion


			#region Private Fields

			private Dictionary<uint, uint> _cachedTypeId2NameIds;

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_TypeID2NameIDTable_GetNameID(System.IntPtr nativeHandle, uint typeId);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_TypeID2NameIDTable_SetTypeIdWithName(System.IntPtr nativeHandle, uint typeId,
				string nameStr);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_TypeID2NameIDTable_SetTypeIDWithNameID(System.IntPtr nativeHandle,
				uint typeId, uint nameID);

			#endregion
		}
	}
}