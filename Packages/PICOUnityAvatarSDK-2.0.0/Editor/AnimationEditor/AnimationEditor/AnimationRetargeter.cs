#if UNITY_EDITOR
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pico.Avatar
{
    public class AnimationRetargeter
    {
        public static void RetargetAnimations(string[] toRetargetClipNames, string sourceSkeletonPath, string sourceAnimPath, string targetSkeletonPath, string targetAnimPath)
        {
            if (toRetargetClipNames.Length == 0 || string.IsNullOrEmpty(sourceSkeletonPath) || string.IsNullOrEmpty(sourceAnimPath) ||
                string.IsNullOrEmpty(targetSkeletonPath) || string.IsNullOrEmpty(targetAnimPath))
            {
                return;
            }
            
            CreateRetargeter();
            LoadSkeleton(sourceSkeletonPath, true);
            LoadSkeleton(targetSkeletonPath, false);
            string tempTargetAnimPath = targetAnimPath + "/retarget";
            ExecuteRetargeting(sourceAnimPath, tempTargetAnimPath);
            DestroyRetargeter();

            foreach (string clipName in toRetargetClipNames)
            {
                string tempAnimazPath = tempTargetAnimPath + "/anim/" + clipName + ".animaz";
                string targetAnimazPath = targetAnimPath + "/" + clipName + ".animaz";
                if (File.Exists(tempAnimazPath))
                {
                    File.Move(tempAnimazPath, targetAnimazPath);
                }
                else
                {
                    Debug.LogError($"clip {clipName} retarget failed");
                }
            }

            // clean temp dir
            Directory.Delete(tempTargetAnimPath, true);
        }

        public static void CreateRetargeter()
        {
            _nativeHandle = pav_Serialization_CreateAnimationRetargetingContext();
        }

        public static void DestroyRetargeter()
        {
            pav_Serialization_ReleaseAnimationRetargetingContext(_nativeHandle);
            _nativeHandle = System.IntPtr.Zero;
        }

        public static void LoadSkeleton(string prefabZipPath, bool isSource)
        {
            pav_Serialization_LoadSkeletonForRetargeting(_nativeHandle, prefabZipPath, isSource);
        }

        public static void SetAnimationReductionErrorMargins(float positionError, float rotationError, float scaleError)
        {
            pav_Serialization_SetAnimationReductionErrorMargins(_nativeHandle, positionError, rotationError, scaleError);
        }

        public static void ExecuteRetargeting(string sourceAnimazZipPath, string targetAnimazZipPath)
        {
            pav_Serialization_ExecuteAnimationRetargeting(_nativeHandle, sourceAnimazZipPath, targetAnimazZipPath);
        }

        private static System.IntPtr _nativeHandle = System.IntPtr.Zero;

        [DllImport("effect", CallingConvention = CallingConvention.Cdecl)]
        private static extern System.IntPtr pav_Serialization_CreateAnimationRetargetingContext();

        [DllImport("effect", CallingConvention = CallingConvention.Cdecl)]
        private static extern System.IntPtr pav_Serialization_LoadSkeletonForRetargeting(System.IntPtr retargeterHandle, string prefabZipPath, bool isSource);

        [DllImport("effect", CallingConvention = CallingConvention.Cdecl)]
        private static extern System.IntPtr pav_Serialization_SetAnimationReductionErrorMargins(System.IntPtr retargeterHandle, float positionError, float rotationError, float scaleError);

        [DllImport("effect", CallingConvention = CallingConvention.Cdecl)]
        private static extern System.IntPtr pav_Serialization_ExecuteAnimationRetargeting(System.IntPtr retargeterHandle, string sourceAnimazZipPath, string targetAnimazZipPath);

        [DllImport("effect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void pav_Serialization_ReleaseAnimationRetargetingContext(System.IntPtr nativeHandle);
    }
}

#endif