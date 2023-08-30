using UnityEngine;

namespace MicroWar 
{
    public class HittableObject : MonoBehaviour
    {
        private const string TagFoot = "Foot";
        private const string TagHand = "Hand";
        private const float displacementThreshold = 0.1f;

        private bool isDisabled = false;

        public System.Action<bool> OnInteracted;

        private void OnTriggerEnter(Collider other)
        {
            if ((!other.gameObject.CompareTag(TagFoot) && !other.gameObject.CompareTag(TagHand)) || isDisabled)
            {
                return;
            }

            IMotionTrackedObject foot = other.gameObject.GetComponent<IMotionTrackedObject>();
            Vector3 maxDisplacementPerAxis = foot.GetMaxDistancesForEachAxis();
            bool isSuccessfulHit = false;

            if (Mathf.Max(maxDisplacementPerAxis.x, maxDisplacementPerAxis.y, maxDisplacementPerAxis.z) > displacementThreshold)
            {
                isSuccessfulHit = true;
            }

            OnInteracted?.Invoke(isSuccessfulHit);
        }

        public void SimulateOnTriggerEnter()
        {
            if (isDisabled) return;
            OnInteracted?.Invoke(true);
        }

        public void DisableInteraction()
        {
            isDisabled = true;
        }

        public void EnableInteraction()
        {
            isDisabled = false;
        }
    }

}
