using System.Runtime.InteropServices;
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		// Socket is used to drive xform of mesh with main skeleton. socket has local position, scale and rotation.
		public class AvatarMeshSocket : NativeObject
		{
			// transform of the mesh to drive.
			public Transform targetMeshTransform { get; set; } = null;

			
            //@brief Whether need update unity target transform. if native target transform is null, means that unity transform
            //target is needed to drive the target mesh.
            public bool needUpdateUnitySocketTransform
			{
				get => GetNativeSocketTransformNameHash() == 0 ? false : true;
			}


			#region Public Methods

			
            //@brief Constructor
            //@param nativeHandle_ native handle of mesh socket object.
            //@param meshTrans unity transform of the mesh to drive.
            public AvatarMeshSocket(System.IntPtr nativeHandle_)
			{
				// check out of memory exception.
				if (nativeHandle_ == System.IntPtr.Zero)
				{
					throw new System.OutOfMemoryException("IDParameterTable.New");
				}

				SetNativeHandle(nativeHandle_, false);
				// cache name hash of source and target transforms.
				_nativeSocketTransformNameHash = pav_AvatarMeshSocket_GetSocketTransformNameHash(nativeHandle);
				_nativeTargetTransformNameHash = pav_AvatarMeshSocket_GetTargetTransformNameHash(nativeHandle);
			}

			
            //Destroy the object and release native reference count.
            //@note Derived class MUST invoke the method if override it.
            [UnityEngine.Scripting.PreserveAttribute]
			protected override void OnDestroy()
			{
				targetMeshTransform = null;
				// Do Nothing.
				base.OnDestroy();
			}

			// @brief Invoked from AvatarPrimitive to update xform for the transform.
			public void UpdateUnityTargetTransformFromNative()
			{
				if (pav_AvatarMeshSocket_GetRelativeToRootXForm(nativeHandle, ref _cachedXForm) == NativeResult.Success)
				{
					if (targetMeshTransform != null)
					{
						targetMeshTransform.localScale = _cachedXForm.scale;

#if UNITY_2021_3_OR_NEWER || UNITY_2022_2_OR_NEWER
						targetMeshTransform.SetLocalPositionAndRotation(_cachedXForm.position,
							_cachedXForm.orientation);
#else
                        targetMeshTransform.localPosition = _cachedXForm.position;
                        targetMeshTransform.localRotation = _cachedXForm.orientation;
#endif
					}
				}
			}

			// @brief Gets name hash of transform that drive the socket.
			public uint GetNativeTargetTransformNameHash()
			{
				if (_nativeTargetTransformNameHash == 0)
				{
					_nativeTargetTransformNameHash = pav_AvatarMeshSocket_GetTargetTransformNameHash(nativeHandle);
				}

				return _nativeTargetTransformNameHash;
			}

			
            //@brief Gets name hash of socket proxy transform, if it is not 0, means native socket is created as 
			//additive skeleton and no need to create unity part socket transform.
			public uint GetNativeSocketTransformNameHash()
			{
				if (_nativeSocketTransformNameHash == 0)
				{
					_nativeSocketTransformNameHash = pav_AvatarMeshSocket_GetSocketTransformNameHash(nativeHandle);
				}

				return _nativeSocketTransformNameHash;
			}

			// @brief Sets local position
			public XForm GetLocalXForm()
			{
				XForm localXForm = new XForm();
				pav_AvatarMeshSocket_GetXForm(nativeHandle, ref localXForm);
				return localXForm;
			}

			
			//@brief Sets local position
			public void SetLocalPosition(Vector3 pos)
			{
				pav_AvatarMeshSocket_SetPosition(nativeHandle, pos);
			}

			// @brief Sets local scale
			public void SetLocalScale(Vector3 scale)
			{
				pav_AvatarMeshSocket_SetScale(nativeHandle, scale);
			}

			// @brief Sets local position
			public void SetLocalOrientation(Quaternion rotate)
			{
				pav_AvatarMeshSocket_SetRotation(nativeHandle, rotate);
			}

			/// <summary>
			/// Dirty cached name hashes of source and target transform 
			/// </summary>
			public void OnSocketOrTargetTransformChanged()
			{
				_nativeSocketTransformNameHash = 0;
				_nativeTargetTransformNameHash = 0;
			}

			#endregion


			#region Private Fields

			private XForm _cachedXForm = new XForm();

			private uint
				_nativeSocketTransformNameHash =
					0; // name hash of source transform in main skeleton that drive the socket

			private uint _nativeTargetTransformNameHash = 0; // transform that is driven by source transform.

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarMeshSocket_GetSocketTransformNameHash(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarMeshSocket_GetTargetTransformNameHash(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMeshSocket_GetRelativeToRootXForm(System.IntPtr nativeHandle,
				ref XForm worrldXForm);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMeshSocket_GetXForm(System.IntPtr nativeHandle,
				ref XForm xform);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMeshSocket_SetPosition(System.IntPtr nativeHandle,
				Vector3 localPosition);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMeshSocket_SetScale(System.IntPtr nativeHandle,
				Vector3 localScale);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMeshSocket_SetRotation(
				System.IntPtr nativeHandle, Quaternion rotation);

			#endregion
		}
	}
}