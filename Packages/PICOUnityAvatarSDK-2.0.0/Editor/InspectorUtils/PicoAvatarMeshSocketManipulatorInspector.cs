using System;
using UnityEngine;
using UnityEditor;

namespace Pico
{
    namespace Avatar
    {
        [CustomEditor(typeof(PicoAvatarMeshSocketManipulator), true)]
        public class PicoAvatarMeshSocketManipulatorInspector : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                var myTarget = this.target as PicoAvatarMeshSocketManipulator;

                if (GUILayout.Button("Attach"))
                {
                    myTarget.AttachTarget();
                }
            }
        }
    }
}
