using System;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;

namespace MicroWar 
{
    public struct AnchorVolume
    {
        public ulong key;
        public Vector3 center;
        public Vector3 extent;
        public Transform anchorTransform;
    }

    public class MixedRealityManager : MonoBehaviour
    {
        private static readonly Color bgColor = new Color(0, 0, 0, 0);
        private const float battlegroundOffsetY = 0.01f;

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

        private void Awake()
        {
            tableAnchors = new Dictionary<ulong, AnchorVolume>();
            wallAnchors = new Dictionary<ulong, AnchorVolume>();
        }

        private void Start()
        {
            CacheCameraData();
        }

        private void OnEnable()
        {
            PXR_Manager.SpatialTrackingStateUpdate += PXRManager_SpatialTrackingStateUpdate;
            PXR_Manager.AnchorEntityLoaded += PXRManager_AnchorEntityLoaded;
        }


        private void OnDisable()
        {
            PXR_Manager.SpatialTrackingStateUpdate -= PXRManager_SpatialTrackingStateUpdate;
            PXR_Manager.AnchorEntityLoaded -= PXRManager_AnchorEntityLoaded;
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

        public void EnablePassthrough()
        {

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
            PXR_MixedReality.EnableVideoSeeThrough(true);

            
            PXR_MixedReality.EnableVideoSeeThroughEffect(true);
            PXR_MixedReality.SetVideoSeeThroughLut(LutTexture, 8, 8);


            isPassthroughEnabled = true;

            //TODO: Let the user decide the scale of the battleground
            GameManager.Instance.EnvironmentManager.RescaleBattleground(0.4f);

#if !UNITY_EDITOR
            LoadRoomData();
#else
            AnchorVolume tableAnchor = new AnchorVolume { anchorTransform = debugTable, extent = debugTable.GetComponent<BoxCollider>().size };
            TryAttachBattlegroundToTable(tableAnchor);
#endif
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
            PXR_MixedReality.EnableVideoSeeThrough(false);

            PXR_MixedReality.EnableVideoSeeThroughEffect(false);

            isPassthroughEnabled = false;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause && isPassthroughEnabled)
            {
                PXR_MixedReality.EnableVideoSeeThrough(true);
            }
        }

        private void LoadRoomData()
        {
            //Flags can be static, move this outside of this method
            PxrSpatialSceneDataTypeFlags[] flags = {
                PxrSpatialSceneDataTypeFlags.Ceiling,
                PxrSpatialSceneDataTypeFlags.Door, PxrSpatialSceneDataTypeFlags.Floor,
                PxrSpatialSceneDataTypeFlags.Opening,
                PxrSpatialSceneDataTypeFlags.Window,
                PxrSpatialSceneDataTypeFlags.Wall,
                PxrSpatialSceneDataTypeFlags.Object };

            //PXRManager_AnchorEntityLoaded will be trigged when the anchors are loaded
            PxrResult result = PXR_MixedReality.LoadAnchorEntityBySceneFilter(flags, out var taskId);

            Debug.Log($"LoadAnchorEntityBySceneFilter returned: {result}");
            
        }

        private void PXRManager_AnchorEntityLoaded(PxrEventAnchorEntityLoaded result)
        {
            if (result.result == PxrResult.SUCCESS && result.count != 0)
            {
                PXR_MixedReality.GetAnchorEntityLoadResults(result.taskId, result.count, out var loadedAnchors);

                foreach (var key in loadedAnchors.Keys)
                {
                    //Load an anchor at position 
                    GameObject anchorObject = Instantiate(anchorPrefab);

                    PXR_MixedReality.GetAnchorPose(key, out var orientation, out var position);
                    anchorObject.transform.SetPositionAndRotation(position, orientation);
                    //Now anchor is at correct position in our space

                    anchorMap.Add(key, anchorObject.transform);

                    PxrResult labelResult = PXR_MixedReality.GetAnchorSceneLabel(key, out var label);
                    if (labelResult == PxrResult.SUCCESS)
                    {

                        //What labels?
                        //PxrSceneLabel.Wall
                        //System Space Calibration Furniture names -> SDK SceneData Labels
                        //Couch             -> Sofa
                        //Desk              -> Table
                        //Door/Windows      -> Doors
                        //Objects/Unknowns  -> Unknowns
                        //Floors            -> Floors
                        //Ceiling           -> Ceiling
                        //Walls             -> Walls

                        switch (label)
                        {
                            //Sofa&Tables&Unknown/Objects
                            //Volume: The Anchor is located at the center of the rectangle on the upper surface of the cube with Z axis as up
                            case PxrSceneLabel.Sofa:
                                {
                                    PXR_MixedReality.GetAnchorVolumeInfo(key, out var center, out var extent);
                                    //extent: x-width, y-height, z-depth from center
                                    var newSofa = Instantiate(sofaPrefab);
                                    //All info is relative to the anchor position
                                    newSofa.transform.SetParent(anchorObject.transform);
                                    newSofa.transform.localPosition = center;
                                    newSofa.transform.localRotation = Quaternion.identity;
                                    newSofa.transform.localScale = extent;
                                }
                                break;
                            case PxrSceneLabel.Table:
                                {
                                    PXR_MixedReality.GetAnchorVolumeInfo(key, out var center, out var extent);
                                    //extent: x-width, y-height, z-depth from center
                                    var newTable = Instantiate(sofaPrefab);
                                    //All info is relative to the anchor position
                                    newTable.transform.SetParent(anchorObject.transform);
                                    newTable.transform.localPosition = center;
                                    newTable.transform.localRotation = Quaternion.identity;
                                    newTable.transform.localScale = extent;

                                    tableAnchors.TryAdd(key, new AnchorVolume { key = key, center = center, extent = extent, anchorTransform = anchorObject.transform});
                                }
                                break;
                            //Wall/Window/Door
                            //Plane: Anchor is located in the center of the plane
                            //x-axis - width, yaxis - height, zaxis - normal vector
                            case PxrSceneLabel.Wall:
                                {
                                    PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);
                                    wallAnchors.TryAdd(key, new AnchorVolume { key = key, center = center, extent = extent, anchorTransform = anchorObject.transform });
                                }
                                break;
                            //Windows are labeled as Doors
                            case PxrSceneLabel.Window:
                            case PxrSceneLabel.Door:
                                {
                                    PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);
                                    var windowDoor = Instantiate(windowDoorPrefab);
                                    windowDoor.transform.SetParent(anchorObject.transform);
                                    windowDoor.transform.localPosition = Vector3.zero;//we are already at center
                                    windowDoor.transform.localRotation = Quaternion.identity;
                                    windowDoor.transform.Rotate(270, 0, -180);
                                    //extent - Vector2: x-width, y-depth
                                    //0.001f because I want a thin wall
                                    //increase wall height to cover any gaps
                                    windowDoor.transform.localScale = new Vector3(extent.x, 0.002f, extent.y);
                                }
                                break;
                            //Not currently supported in the current SDK Version
                            //!PXR_MixedReality.GetAnchorPlanePolygonInfo(ulong anchorHandle, out Vector3[] vertices)
                            //but! we know the anchor object as at the center
                            case PxrSceneLabel.Floor:
                                {

                                }
                                break;
                            case PxrSceneLabel.Ceiling:
                                {

                                }
                                break;
                                
                        }
                    }
                }

                //Attach the battleground to a table found in the room data
                if (tableAnchors.Count > 0)
                {
                    IEnumerator<ulong> enumerator = tableAnchors.Keys.GetEnumerator(); 
                    enumerator.MoveNext();
                    AnchorVolume anchor = tableAnchors[enumerator.Current];
                    TryAttachBattlegroundToTable(anchor);
                }
                
                TryAttachDecorations();
            }
        }

        private void HandleSpatialDrift()
        {
            //if no anchors, we don't need to handle drift
            if (anchorMap.Count == 0)
                return;

            currDriftDelay += Time.deltaTime;
            if (currDriftDelay >= maxDriftDelay)
            {
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

                    PXR_MixedReality.GetAnchorPose(handle, out var rotation, out var position);
                    anchorTransform.position = position;
                    anchorTransform.rotation = rotation;
                }
            }
        }

        private void PXRManager_SpatialTrackingStateUpdate(PxrEventSpatialTrackingStateUpdate statusUpdate)
        {
            Debug.LogError($"Spatial Tracking State is {statusUpdate.state}. Message: {statusUpdate.message}");
            switch (statusUpdate.state)
            {
                case PxrSpatialTrackingState.Invalid:
                case PxrSpatialTrackingState.Limited:
                    // Invalid data, Room Caliration needs to run 
                    PXR_MixedReality.StartSpatialSceneCapture(out ulong taskId);
                    break;
            }
        }
    }

}