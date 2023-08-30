using Unity.XR.PXR;
using UnityEngine;

namespace MicroWar 
{
    public class TrackedBodyPart : MotionTrackedObject
    {
        public BodyTrackerRole trackedPart = BodyTrackerRole.LEFT_FOOT;

        private void Start()
        {
            if (SwiftTrackerManager.Instance == null)
            {
                Debug.LogError("There is no active SwiftTrackerManager instance. Add a SwiftTrackerManager to your scene.");
            }
            SwiftTrackerManager.Instance.RegisterTrackedObject(this);
        }
    }
}

