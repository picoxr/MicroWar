using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;

namespace MicroWar 
{
    public class SwiftTrackerManager : MonoBehaviour
    {
        public static SwiftTrackerManager Instance;
        public Transform[] SkeletonObjects; //For Debugging

        private BodyTrackerResult bodyTrackerResult = new BodyTrackerResult();
        private int connectedTrackerCount = 0;
        private List<TrackedBodyPart> trackedObjects;

        private bool bothTrackersConnected { get { return connectedTrackerCount > 1; } }


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            trackedObjects = new List<TrackedBodyPart>();
        }

        private void Start()
        {
            PXR_MotionTracking.MotionTrackerNumberOfConnections += NumberOfConnections;

#if SWIFT_DEBUG
            bodyTrackerResult.trackingdata = new BodyTrackerTransform[24];
#else
            Destroy(SkeletonObjects[0].parent.gameObject);
#endif
            PxrMotionTracker1ConnectState state = new PxrMotionTracker1ConnectState();
            int result = PXR_Input.GetMotionTrackerConnectStateWithID(ref state);

            UpdateConnectedTrackerCount(state.num);
        }

        private void UpdateConnectedTrackerCount(int count)
        {
            connectedTrackerCount = count;
            TrackedObjectsSetActive(bothTrackersConnected);
        }

        private void TrackedObjectsSetActive(bool isActive)
        {
            IEnumerator<TrackedBodyPart> trackedObjectsIterator = trackedObjects.GetEnumerator();
            while (trackedObjectsIterator.MoveNext())
            {
                trackedObjectsIterator.Current.gameObject.SetActive(isActive);
            }
        }

        private void Update()
        {
            GetBodyTrackingData();
        }

        private void GetBodyTrackingData()
        {
            if (!bothTrackersConnected) return;

            PXR_Input.GetBodyTrackingPose(PXR_System.GetPredictedDisplayTime(), ref bodyTrackerResult);

            UpdateBodyTrackingForObjects();
#if SWIFT_DEBUG
            UpdateDebugSkeleton();
            PaintSkeletonPartIfTouchingGround(BodyTrackerRole.LEFT_ANKLE);
            PaintSkeletonPartIfTouchingGround(BodyTrackerRole.LEFT_FOOT);
            PaintSkeletonPartIfTouchingGround(BodyTrackerRole.RIGHT_ANKLE);
            PaintSkeletonPartIfTouchingGround(BodyTrackerRole.RIGHT_FOOT);
#endif
        }

        private void UpdateBodyTrackingForObjects()
        {
            IEnumerator<TrackedBodyPart> bodyObjects = trackedObjects.GetEnumerator();

            while (bodyObjects.MoveNext())
            {
                TrackedBodyPart foot = bodyObjects.Current;
                UpdatePositionAndRotation(foot.gameObject.transform, foot.trackedPart);
            }
        }

        private void UpdatePositionAndRotation(Transform transform, BodyTrackerRole trackerRole)
        {
            BodyTrackerTransPose poseData = bodyTrackerResult.trackingdata[(int)trackerRole].localpose;
            Vector3 position = new Vector3((float)poseData.PosX, (float)poseData.PosY, (float)poseData.PosZ);
            Quaternion rotation = new Quaternion((float)poseData.RotQx, (float)poseData.RotQy, (float)poseData.RotQz, (float)poseData.RotQw);
            transform.SetPositionAndRotation(position, rotation);
        }

        public void RegisterTrackedObject(TrackedBodyPart trackedObject)
        {
            //TODO: Check duplicates
            trackedObjects.Add(trackedObject);
            trackedObject.gameObject.SetActive(bothTrackersConnected);
        }

        public void NumberOfConnections(int state, int num)
        {
            UpdateConnectedTrackerCount(num);
            Debug.Log($"NumberOfConnections: state:{state} - num:{num}");
        }

        public void OpenSwiftCalibrationApp()
        {
            PXR_MotionTracking.StartMotionTrackerCalibApp();
        }

        private void PaintSkeletonPartIfTouchingGround(BodyTrackerRole part)
        {
            if ((bodyTrackerResult.trackingdata[(int)part].Action & (int)BodyActionList.PxrTouchGround) == (int)BodyActionList.PxrTouchGround)
            {
                SkeletonObjects[(int)part].gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else
            {
                SkeletonObjects[(int)part].gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }

        private void UpdateDebugSkeleton()
        {
            for (int i = 0; i < SkeletonObjects.Length; i++)
            {
                BodyTrackerTransPose poseData = bodyTrackerResult.trackingdata[i].localpose;
                Transform currTransform = SkeletonObjects[i].transform;
                currTransform.localPosition = new Vector3((float)poseData.PosX, (float)poseData.PosY, (float)poseData.PosZ);
                currTransform.localRotation = new Quaternion((float)poseData.RotQx, (float)poseData.RotQy, (float)poseData.RotQz, (float)poseData.RotQw);
            }
        }
    }
}
