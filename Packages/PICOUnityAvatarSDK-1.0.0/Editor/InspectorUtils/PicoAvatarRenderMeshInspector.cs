using UnityEngine;
using UnityEditor;

namespace Pico
{
    namespace Avatar
    {
        [CustomEditor(typeof(PicoAvatarRenderMesh), true)]
        public class PicoAvatarRenderMeshInspector : Editor
        {
            private PreviewRenderUtility m_PreviewUtility;
            private PicoAvatarRenderMesh m_PreviewInstance;
            private Mesh mPreviewMesh;
            private Material mPreviewMaterial;
            private float angle;
            private float previewPositionY = -2;
            private float previewScale = 2;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Update Material When Shader Changed"))
                {
                    var renderMesh = (PicoAvatarRenderMesh)this.target;
                    renderMesh.OnShaderChanged();
                }
                GUILayout.Space(1);
                GUILayout.Label("previewOffsetY:" + previewPositionY.ToString());
                previewPositionY = GUILayout.HorizontalScrollbar(previewPositionY, 0.05f, -4, 0);
                GUILayout.Label("previewScale:" + previewScale.ToString());
                previewScale = GUILayout.HorizontalScrollbar(previewScale, 0.05f, 2, 5);
            }
       

            private void InitPreview()
            {
                if (m_PreviewUtility == null)
                {
                    //
                    m_PreviewUtility = new PreviewRenderUtility();
                    m_PreviewUtility.camera.farClipPlane = 500;
                    m_PreviewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
                    m_PreviewUtility.camera.transform.position = new Vector3(0, 0, -10);

                    // 绘制场景上已经存在的游戏对象
                    m_PreviewInstance = (this.target as PicoAvatarRenderMesh);

                    if (m_PreviewInstance.GetComponent<MeshRenderer>())
                    {
                        var meshFilter = m_PreviewInstance.GetComponent<MeshFilter>();
                        if (meshFilter != null)
                        {
                            mPreviewMesh = meshFilter.sharedMesh;
                            mPreviewMaterial = m_PreviewInstance.GetComponent<MeshRenderer>().sharedMaterial;
                        }
                    } else if (m_PreviewInstance.GetComponent<SkinnedMeshRenderer>())
                    {
                        var renderer = m_PreviewInstance.GetComponent<SkinnedMeshRenderer>();
                        mPreviewMesh = renderer.sharedMesh;
                        mPreviewMaterial = renderer.sharedMaterial;
                    }
                    else
                    {
                        Debug.LogError("Unable to find MeshRenderer or SkinnedMeshRenderer");
                    }
                }
            }

            private void DestroyPreview()
            {
                if (m_PreviewUtility != null)
                {
                    m_PreviewUtility.Cleanup();
                    m_PreviewUtility = null;
                }
            }
            private void OnEnable()
            {
                if (m_PreviewUtility != null)
                {
                    m_PreviewUtility.Cleanup();
                }
            }
            private void OnDisable()
            {
                if (m_PreviewUtility != null)
                {
                    m_PreviewUtility.Cleanup();
                }
            }
     

            private void DestroyPreviewInstances()
            {
                m_PreviewInstance = null;
            }


            void OnDestroy()
            {
                DestroyPreviewInstances();
                DestroyPreview();
            }


            public override bool HasPreviewGUI()
            {
                return true;
            }

            public override void OnPreviewGUI(Rect r, GUIStyle background)
            {
                InitPreview();

                GUI.Box(r, "Preview");
                angle = GUI.HorizontalSlider(r, angle, 0, 360);

                if (Event.current.type != EventType.Repaint || m_PreviewInstance == null)
                {
                    return;
                }

                if (m_PreviewUtility.camera == null)
                    return;

                Quaternion quaternion = Quaternion.Euler(0, angle, 0f);

                m_PreviewUtility.BeginPreview(r, background);
                m_PreviewUtility.camera.backgroundColor = Color.white;

                Vector3 pos = new Vector3(0, this.previewPositionY);
                Vector3 scale = new Vector3(this.previewScale, this.previewScale, this.previewScale);

                m_PreviewUtility.DrawMesh(mPreviewMesh, Matrix4x4.TRS(pos, quaternion, scale), mPreviewMaterial, 0);
                m_PreviewUtility.camera.Render();

                var texture = m_PreviewUtility.EndPreview();

                Rect newRect = new Rect(r.x, r.y + 20, r.width, r.height - 20);
                GUI.Box(newRect, texture);

            }
        }
    }
}