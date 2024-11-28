using UnityEngine;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		// The class encapsulates reference to AvatarBunch from from AvatarEntity of an online avatar.
		public class AvatarBunchItem
		{
			#region Public Properties

			// Gets native handle.
			internal System.IntPtr nativeHandle
			{
				get => _nativeHandle;
			}

			// whether visible this frame. set from AvatarEntity each frame.
			public bool visibleThisFrame { get; set; } = true;

			// Gets matrix4x4.
			public Matrix4x4 localToWorldMatrix
			{
				get => _targetEntity.transform.localToWorldMatrix;
			}

			// target avatar skeleton.
			public AvatarSkeleton avatarSkeleton
			{
				get => _targetSkeleton;
			}

			// owner
			public AvatarBunch avatarBunch
			{
				get => _avatarBunch;
			}

			// target entity
			public AvatarEntity targetEntity
			{
				get => _targetEntity;
			}

			#endregion


			#region Public Methods

			// Initialize the bunch.
			internal void Initialize(AvatarEntity targetEntity)
			{
				_targetEntity = targetEntity;

				if (targetEntity != null && _nativeHandle != System.IntPtr.Zero)
				{
					var entityLod = targetEntity.GetCurrentAvatarLod();
					if (entityLod == null)
					{
						throw new System.Exception(
							"AvatarBunchItem.Initialize need targetEntity successfully be loaded!");
					}

					//
					_targetSkeleton = entityLod.avatarSkeleton;
					if (_targetSkeleton == null)
					{
						throw new System.Exception(
							"AvatarBunchItem.Initialize need avatar skeleton of targetEntity successfully be loaded!");
					}

					//
					pav_AvatarBunchItem_Initialize(nativeHandle, targetEntity.nativeHandle,
						_targetSkeleton.nativeHandle);
				}
			}

			// destroy the object. removed from AvatarEntity.
			internal void RemoveFromAvatarBunch()
			{
				if (_avatarBunch != null)
				{
					_avatarBunch.RemoveAvatarBunchItem(this);
				}
			}

			// Invoked from AvatarBunch when destroyed.
			internal void OnRemovedFromAvatarBunch()
			{
				NativeObject.ReleaseNative(ref _nativeHandle);
				//
				_avatarBunch = null;
				_targetEntity = null;
			}

			
            //@brief Invoked from AvatarBunch to fill instance bones data. Texture2D<half> each instance occupy one line.
            //@param flags 0: float4x4 matrices,  1: half4x4 matrices
            internal void FillInstanceBoneData(AvatarPrimitive primitive, System.IntPtr dataStart, int dataSize,
				uint flags)
			{
				pav_AvatarBunchItem_FillInstanceBoneData(_nativeHandle, primitive.nativeHandle, dataStart, dataSize,
					flags);
			}

			#endregion


			#region Private Fields

			// native handle.
			System.IntPtr _nativeHandle;

			// avatar entity who need be replaced with the AvatarBunch.
			AvatarEntity _targetEntity = null;

			// avatar skeleton of target avatar entity.
			AvatarSkeleton _targetSkeleton = null;

			// referenced AvatarBunch
			private AvatarBunch _avatarBunch = null;

			#endregion


			#region Private Methods

			internal AvatarBunchItem(AvatarBunch avatarBunch_)
			{
				_avatarBunch = avatarBunch_;
				_nativeHandle = pav_AvatarBunchItem_New(avatarBunch.nativeHandle);
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarBunchItem_New(System.IntPtr avatarBunchHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBunchItem_Initialize(System.IntPtr nativeHandle,
				System.IntPtr avatarEntityHandle, System.IntPtr avatarSkeletonHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarBunchItem_FillInstanceBoneData(System.IntPtr nativeHandle,
				System.IntPtr nativePrimitiveHandle, System.IntPtr dataStart, int dataSize, uint flags);

			#endregion
		}
	}
}