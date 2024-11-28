using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;

namespace Pico
{
    namespace Avatar
    {
        public class AvatarCustomMeshSocket
        {
            public static void ConnectToAvatar(GameObject go, PicoAvatar avatar, JointType mountPoint)
            {
                avatar.AddCriticalJoint(mountPoint);
                GameObject jointObject = avatar.GetJointObject(mountPoint);
                if (jointObject != null)
                {
                    go.transform.parent = jointObject.transform;
                }
            }

            public static void Transfer(GameObject go, PicoAvatar avatar, JointType jointType)
            {
                XForm xform = new XForm();
                xform.position = go.transform.localPosition;
                xform.orientation = go.transform.localRotation;
                xform.scale = go.transform.localScale;
                CustomMeshSocketTransfer(avatar.entity, ref xform, jointType);

                //go.transform.SetLocalPositionAndRotation(xform.position, xform.orientation);
                go.transform.localPosition = xform.position;
                go.transform.localRotation = xform.orientation;
                go.transform.localScale = xform.scale;
            }

            public static void CustomMeshSocketTransfer(AvatarEntity avatarEntity, ref XForm localXForm, JointType jointType)
            {
                pav_AvatarCustomMeshSocket_Transfer(avatarEntity.nativeHandle, ref localXForm, jointType);
            }

            const string PavDLLName = DllLoaderHelper.PavDLLName;

            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
            private static extern NativeResult pav_AvatarCustomMeshSocket_Transfer(System.IntPtr avatarEntityHandle, ref XForm localXForm, JointType jointType);

        }
    }
}