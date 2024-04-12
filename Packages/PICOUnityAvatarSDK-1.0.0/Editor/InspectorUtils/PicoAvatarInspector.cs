using System;
using UnityEngine;
using UnityEditor;

namespace Pico
{
    namespace Avatar
    {
        [CustomEditor(typeof(PicoAvatar), true)]
        public class PicoAvatarInspector : Editor
        {
            private string _specificationText;

            float height = 1.5f;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                var myTarget = this.target as PicoAvatar;
                if(myTarget.entity == null)
                {
                    return;
                }

                if (GUILayout.Button("Get Specifiation"))
                {
                    _specificationText = myTarget.GetAvatarSpecification();
                }
                EditorGUILayout.TextArea(_specificationText);


                var avatarLod = myTarget.entity.GetCurrentAvatarLod();
                if(avatarLod != null && avatarLod.avatarSkeleton != null)
                {
                    avatarLod.avatarSkeleton.enableUpdateFromNativeSkeleton = EditorGUILayout.Toggle("enableUpdateFromNativeSkeleton", avatarLod.avatarSkeleton.enableUpdateFromNativeSkeleton);
                }
                //
                myTarget.depressUpdateSimulationRenderData = EditorGUILayout.Toggle("depressUpdateSimulationRenderData", myTarget.depressUpdateSimulationRenderData);

                height = EditorGUILayout.Slider(height, 0, 3);
                if (GUILayout.Button("Set Height for IK"))
                {
                    myTarget.entity.bodyAnimController.SetAvatarHeight(height, false);

                    Debug.Log($"Set height = {height} for IK");
                }
            }
        }
    }
}
