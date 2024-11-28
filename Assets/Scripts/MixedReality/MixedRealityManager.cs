using System;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MicroWar 
{
    public struct AnchorVolume
    {
        public ulong key;
        public Vector3 center;
        public Vector3 extent;
        public Transform anchorTransform;
        public Quaternion rotation;
    }

    public class MixedRealityManager : MonoBehaviour
    {
        private static readonly Color bgColor = new Color(0, 0, 0, 0);
        private const float battlegroundOffsetY = 0.02f;

        public Transform[] disableIfPassthrough;

        private CameraClearFlags cameraClearFlags;
        private Color cameraBgColor;

        private bool isPassthroughEnabled = false;

    
        public GameObject anchorPrefab;
        public GameObject sofaPrefab;
        public GameObject tablePrefab;
        public GameObject windowDoorPrefab;
        public GameObject wallPrefab;
        public GameObject floorCeilingPrefab;

        public GameObject toyShelfPrefab;
        public GameObject microWarPosterPrefab;

        public Texture2D LutTexture;

        [SerializeField]
        private float maxDriftDelay = 0.5f;
        private float currDriftDelay = 0f;

        private Dictionary<ulong, Transform> anchorMap = new Dictionary<ulong, Transform>();
        private Transform ceilingTransform = null;
        private Transform floorTransform = null;

        private Dictionary<ulong, AnchorVolume> tableAnchors;
        private Dictionary<ulong, AnchorVolume> wallAnchors;

        public Transform debugTable;

        private static readonly PxrSemanticLabel[] mrQueryLabels = { PxrSemanticLabel.Table, PxrSemanticLabel.Wall };

        private UniversalAdditionalCameraData urpCameraData;

        private void Awake()
        {
            tableAnchors = new Dictionary<ulong, AnchorVolume>();
            wallAnchors = new Dictionary<ulong, AnchorVolume>();
        }

        private void Start()
        {
            urpCameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
            CacheCameraData();
        }

        private void CacheCameraData()
        {
            cameraClearFlags = Camera.main.clearFlags;
            cameraBgColor = Camera.main.backgroundColor;
        }

        private void LateUpdate()
        {
            HandleSpatialDrift();
        }

        public async void EnableMixedReality()
        {
            await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SceneCapture);
            await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);

            if (PXR_Input.GetControllerDeviceType() != PXR_Input.ControllerDevice.PICO_4)
            {
                PxrResult res = await PXR_MixedReality.StartSceneCaptureAsync();

                if (res != PxrResult.SUCCESS)
                {
                    Debug.Log("Failed to retrieve scene capture data.");
                }
            }
               
            if (null != disableIfPassthrough)
            {
                for (int i = 0; i < disableIfPassthrough.Length; i++)
                {
                    disableIfPassthrough[i].gameObject.SetActive(false);
                }
            }

            //Setup Passthrough
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = bgColor;
            PXR_Manager.EnableVideoSeeThrough = true;


            SetPostProcessing(false);//Disabling post-processing otherwise passthrough won't work.
            PXR_MixedReality.EnableVideoSeeThroughEffect(true);
            PXR_MixedReality.SetVideoSeeThroughLut(LutTexture, 8, 8);


            isPassthroughEnabled = true;

            GameManager.Instance.EnvironmentManager.RescaleBattleground(0.4f);

            if (UnityEngine.Device.Application.isEditor)
            {
                AnchorVolume tableAnchor = new AnchorVolume { anchorTransform = debugTable, extent = debugTable.GetComponent<BoxCollider>().size };
                TryAttachBattlegroundToTable(tableAnchor);
            }
            else
            {
                LoadSceneData();
            }
        }


        private void SetPostProcessing(bool isEnabled)
        {
            urpCameraData.renderPostProcessing = isEnabled;
        }


        private void TryAttachBattlegroundToTable(AnchorVolume tableAnchor)
        {
            Transform tableTransform = tableAnchor.anchorTransform;
            GameManager.Instance.EnvironmentManager.AttachBattlegroundTo(tableTransform, new Vector3(0, battlegroundOffsetY, 0));
        }

        private void TryAttachDecorations()
        {
            if (wallAnchors.Count == 0) return;

            float maxArea = 0;
            AnchorVolume? maxAreaAnchor = null;

            //find the largest wall
            foreach (KeyValuePair<ulong, AnchorVolume> anchorData in wallAnchors)
            {
                float area = anchorData.Value.extent.x * anchorData.Value.extent.y;
                if (area > maxArea)
                {
                    maxAreaAnchor = anchorData.Value;
                    maxArea = area;
                }
            }

            if(maxAreaAnchor.HasValue)
            {
                Transform poster = Instantiate(microWarPosterPrefab, maxAreaAnchor.Value.anchorTransform).transform;
                Transform toyShelf = Instantiate(toyShelfPrefab, maxAreaAnchor.Value.anchorTransform).transform;
                Transform playerTransform = Camera.main.transform;
                Vector3 toyShelfPos = toyShelf.position;
                poster.localPosition += Vector3.up * 0.75f;
                if (Vector3.Dot(toyShelf.forward, new Vector3(playerTransform.position.x, toyShelfPos.y, playerTransform.position.z) - toyShelfPos) < 0)
                {
                    toyShelf.Rotate(new Vector3(0, 180f, 0));
                    poster.Rotate(new Vector3(0, 180f, 0));
                }
                
            }
        }

        public void DisablePassthrough()
        {
            if (null != disableIfPassthrough)
            {
                for (int i = 0; i < disableIfPassthrough.Length; i++)
                {
                    disableIfPassthrough[i].gameObject.SetActive(true);
                }
            }

            Camera.main.clearFlags = cameraClearFlags; 
            Camera.main.backgroundColor = cameraBgColor;

            PXR_Manager.EnableVideoSeeThrough = false;

            PXR_MixedReality.EnableVideoSeeThroughEffect(false);
            SetPostProcessing(true);
            isPassthroughEnabled = false;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause && isPassthroughEnabled)
            {
                PXR_Manager.EnableVideoSeeThrough = true;
            }
        }

        private async void LoadSceneData()
        {
            (PxrResult res, List<ulong> anchors) = await PXR_MixedReality.QuerySceneAnchorAsync(mrQueryLabels);
            if (res == PxrResult.SUCCESS)
            {
                if (anchors == null || anchors.Count == 0)
                {
                    Debug.Log("No anchors found.");
                }
                else
                {
                    foreach (ulong anchorKey in anchors)
                    {
                        GameObject anchorObject = Instantiate(anchorPrefab);

                        PXR_MixedReality.LocateAnchor(anchorKey, out Vector3 anchorPos, out Quaternion anchorRot);
                        anchorObject.transform.SetPositionAndRotation(anchorPos, anchorRot);
                        //Now anchor is at correct position in our space
                        
                        anchorMap.Add(anchorKey, anchorObject.transform);

                        PXR_MixedReality.GetSceneSemanticLabel(anchorKey, out var label);

                        Debug.Log($"[LoadSceneData] - Anchor={anchorKey} - Anchor Label={label}");

                        if (PXR_MixedReality.GetSceneAnchorComponentTypes(anchorKey, out PxrSceneComponentType[] types) == PxrResult.SUCCESS)
                        {
                            foreach (PxrSceneComponentType compType in types)
                            {
                                switch(compType) 
                                {
                                    case PxrSceneComponentType.Box3D:
                                        
                                        PXR_MixedReality.GetSceneBox3DData(anchorKey, out Vector3 position, out Quaternion rotation, out Vector3 extent);
                                        Debug.Log($"[LoadSceneData] GetSceneBox3DData / Anchor={anchorKey} - Anchor Label={label}");
                                        switch (label)
                                        {
                                            case PxrSemanticLabel.Table:
                                                var newTable = Instantiate(sofaPrefab);
                                                //All info is relative to the anchor position
                                                newTable.transform.SetParent(anchorObject.transform);
                                                newTable.transform.localPosition = position;
                                                newTable.transform.localRotation = Quaternion.identity;
                                                newTable.transform.localScale = extent;

                                                tableAnchors.TryAdd(anchorKey, new AnchorVolume { key = anchorKey, center = position, extent = extent, anchorTransform = anchorObject.transform });
                                                Debug.Log($"[LoadSceneData] added to tableAnchors / Anchor={anchorKey} - Anchor Label={label}");
                                                break;
                                        }
                                        
                                        break;
                                    case PxrSceneComponentType.Box2D:

                                        PXR_MixedReality.GetSceneBox2DData(anchorKey, out Vector2 offset2D, out Vector2 extent2D);
                                        Debug.Log($"[LoadSceneData] GetSceneBox2DData / Anchor={anchorKey} - Anchor Label={label}");
                                        switch (label)
                                        {
                                            case PxrSemanticLabel.Wall:
                                                wallAnchors.TryAdd(anchorKey, new AnchorVolume { key = anchorKey, center = offset2D, extent = extent2D, anchorTransform = anchorObject.transform });
                                                Debug.Log($"[LoadSceneData] added to wallAnchors / Anchor={anchorKey} - Anchor Label={label}");
                                                break;
                                        }

                                        break;
                                }
                            }
                            
                        }
                        else 
                        {
                            Debug.Log($"Failed to retrieve tables anchor component types for anchorId={anchorKey}.");
                        }
                    }
                }
            }
            else 
            {
                Debug.Log("Failed to retrieve tables anchors.");
            }

            if (tableAnchors.Count > 0)
            {
                IEnumerator<ulong> enumerator = tableAnchors.Keys.GetEnumerator();
                enumerator.MoveNext();
                AnchorVolume anchor = tableAnchors[enumerator.Current];
                TryAttachBattlegroundToTable(anchor);
            }

            TryAttachDecorations();
        }

        private async void HandleSpatialDrift()
        {
            //if no anchors, we don't need to handle drift
            if (anchorMap.Count == 0)
                return;

            currDriftDelay += Time.deltaTime;
            if (currDriftDelay >= maxDriftDelay)
            {
                //Need this for PICO4Ultra Only:
                //The SDK caches the anchor locations for PICO4U. However, the user might re-center after the anchors have been cached.
                //Therefore the cached anchors will be invalid. That's why we need to call QuerySceneAnchorAsync to reload anchors before we call PXR_MixedReality.LocateAnchor
                //in order to get the correct anchor data.
                (PxrResult res, List<ulong> anchors) = await PXR_MixedReality.QuerySceneAnchorAsync(mrQueryLabels);

                currDriftDelay = 0f;
                foreach (var handlePair in anchorMap)
                {
                    var handle = handlePair.Key;
                    var anchorTransform = handlePair.Value;

                    if (handle == UInt64.MinValue)
                    {
                        Debug.LogError("Handle is invalid");
                        continue;
                    }

                    Vector3 position;
                    Quaternion rotation;

                    PXR_MixedReality.LocateAnchor(handle, out position, out rotation);
                    
                    anchorTransform.position = position;
                    anchorTransform.rotation = rotation;
                }
            }
        }

        private void OnDestroy()
        {
            PXR_MixedReality.GetSenseDataProviderState(PxrSenseDataProviderType.SceneCapture, out PxrSenseDataProviderState state);
            if (state == PxrSenseDataProviderState.Running)
            {
                PXR_MixedReality.StopSenseDataProvider(PxrSenseDataProviderType.SceneCapture);
            }

            PXR_MixedReality.GetSenseDataProviderState(PxrSenseDataProviderType.SpatialAnchor, out state);
            if (state == PxrSenseDataProviderState.Running)
            {
                PXR_MixedReality.StopSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);
            }
        }
    }

}