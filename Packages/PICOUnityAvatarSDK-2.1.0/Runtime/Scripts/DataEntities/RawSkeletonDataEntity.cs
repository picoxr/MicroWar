using UnityEngine;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Internally used skeleton data, should not used outside AvatarSDK.
		/// </summary>
		[UnityEngine.Scripting.PreserveAttribute]
		[System.Serializable]
		public class RawSkeletonDataEntity : AvatarDataEntityBase
		{
			#region Types

			// TODO: should be renamed to Bone data.
			[System.Serializable]
			public class AvatarBone
			{
				public string boneName;
				public int boneNameHash;
				public int parentBoneNameHash;
				public Vector3 position;
				public Quaternion rotation;
				public Vector3 scale;
			}

			#endregion


			#region Framework Methods

			[UnityEngine.Scripting.PreserveAttribute]
			public RawSkeletonDataEntity()
			{
				SetNativeHandle(pav_SkeletonDataEntity_New(), false);
			}

			[UnityEngine.Scripting.PreserveAttribute]
			public RawSkeletonDataEntity(System.IntPtr _nativeHandle, bool needRetain = false)
			{
				SetNativeHandle(_nativeHandle, needRetain);
			}

			// Check reference count.
			~RawSkeletonDataEntity()
			{
#if DEBUG
				if (this.refCount > 0)
				{
				}
#endif
			}

			[UnityEngine.Scripting.PreserveAttribute]
			protected override void OnDestroy()
			{
				base.OnDestroy();
			}

			#endregion


			#region Public Properties
			/// <summary>
			/// Bone count
			/// </summary>
			/// <exception cref="Exception"></exception>
			public int boneCount
			{
				get
				{
					if (nativeHandle == System.IntPtr.Zero)
					{
						throw new System.Exception("AvatarSkeletonData.bones wrong for null _NativeHandle!");
					}

					return (int)pav_SkeletonDataEntity_GetBoneLength(nativeHandle);
				}
			}
			/// <summary>
			/// bones
			/// </summary>
			/// <exception cref="Exception"></exception>
            public AvatarBone[] bones
            {
                get
                {
                    return GetBones(false);
                }
            }

            #endregion


            #region Public Methods

            /**
             * Gets bone.
             * @param reget whether force to get from native part.
             */ 
            internal AvatarBone[] GetBones(bool reget = false)
            {
                if (_bones != null && !reget)
                {
                    return _bones;
                }

                //
                if (nativeHandle == System.IntPtr.Zero)
                {
                    throw new System.Exception("AvatarSkeletonData.bones wrong for null _NativeHandle!");
                }

                var length = pav_SkeletonDataEntity_GetBoneLength(nativeHandle);
                _bones = new AvatarBone[length];
                //
                for (int i = 0; i < length; ++i)
                {
                    var boneLayout = new SkeletonBoneLayout();

                    //
                    boneLayout.name = Utility.sharedStringBuffer.Lock();
                    boneLayout.nameCharCount = 128;

                    pav_SkeletonDataEntity_GetBone(nativeHandle, (uint)i, ref boneLayout);

                    Utility.sharedStringBuffer.Unlock();

                    var bone = new AvatarBone();
                    bone.boneName = Utility.sharedStringBuffer.GetANSIString(boneLayout.nameCharCount);
                    bone.boneNameHash = (int)boneLayout.boneNameHash;
                    bone.parentBoneNameHash = (int)boneLayout.parentBoneNameHash;
                    bone.position = boneLayout.position;
                    bone.rotation = boneLayout.rotation;
                    bone.scale = boneLayout.scale;

                    //
                    _bones[i] = bone;
                }
                //
                return _bones;
            }

            #endregion


			#region Private Fields

			private AvatarBone[] _bones; // the first bone is root bone.

			#endregion


			#region Native Part

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[StructLayout(LayoutKind.Sequential)]
			struct SkeletonBoneLayout
			{
				public System.IntPtr name;
				public uint nameCharCount;
				public uint boneNameHash;
				public uint parentBoneNameHash;
				public Vector3 position;
				public Quaternion rotation;
				public Vector3 scale;
			};

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_SkeletonDataEntity_New();

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_SkeletonDataEntity_GetBone(System.IntPtr dataHandle, uint boneIndex,
				ref SkeletonBoneLayout boneData);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_SkeletonDataEntity_SetBoneFieldData(System.IntPtr dataHandle, uint boneIndex,
				string boneName
				, uint boneNameHash
				, uint parentBoneNameHash
				, System.IntPtr position
				, System.IntPtr rotation
				, System.IntPtr scale);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_SkeletonDataEntity_SetBoneLength(System.IntPtr dataHandle, uint length);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_SkeletonDataEntity_GetBoneLength(System.IntPtr dataHandle);

			#endregion
		}
	}
}