using UnityEngine;

namespace MicroWar 
{
    /// <summary>
    /// Use this component on a motion-tracked game object to access the velocity and the related features of it.
    /// </summary>
    public class MotionTrackedObject : MonoBehaviour, IMotionTrackedObject
    {
        //Position history
        private const int positionBufferSize = 7;
        private const int maxDistanceCalculationRange = positionBufferSize - 1;

        private Vector3[] positionBuffer;
        private float[] deltaTimeBuffer;

        private int currentBufferIndex = 0;

        private void Awake()
        {
            positionBuffer = new Vector3[positionBufferSize];
            deltaTimeBuffer = new float[positionBufferSize];
        }

        private void Update()
        {
            UpdatePositionBuffer(this.transform.position);
        }

        private void UpdatePositionBuffer(Vector3 position)
        {
            if (currentBufferIndex == positionBufferSize)
            {
                currentBufferIndex = 0;
            }

            positionBuffer[currentBufferIndex] = position;
            deltaTimeBuffer[currentBufferIndex] = Time.deltaTime;
            currentBufferIndex++;
        }

        public Vector3 GetMotionVelocity()
        {
            float time = GetMotionTime();
            if (time == 0f) //Unexpected
            {
                Debug.LogError("Division By Zero! GetMotionTime returned 0");
                time = 0.1f;
            }
            return GetMotionVector() / time;
        }

        public Vector3 GetMotionVector()
        {
            int latestIndex = currentBufferIndex - 1;
            if (latestIndex < 0) latestIndex += positionBufferSize;
            int endIndex = latestIndex - maxDistanceCalculationRange;
            if (endIndex < 0) endIndex += positionBufferSize;

            return positionBuffer[latestIndex] - positionBuffer[endIndex];
        }

        public float GetMotionTime()
        {
            float totalTime = 0f;
            int latestIndex = currentBufferIndex - 1;
            if (latestIndex < 0) latestIndex += positionBufferSize;
            int endIndex = latestIndex - maxDistanceCalculationRange;
            if (endIndex < 0) endIndex += positionBufferSize;

            while (latestIndex != endIndex)
            {
                totalTime += deltaTimeBuffer[latestIndex];
                latestIndex--;
                if (latestIndex < 0) latestIndex += positionBufferSize;
            }

            return totalTime;
        }

        /// <summary>
        /// Returns the max displacement of the motion on each axis 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMaxDistancesForEachAxis()
        {
            int latestIndex = currentBufferIndex - 1;
            if (latestIndex < 0) latestIndex += positionBufferSize;
            int endIndex = latestIndex - maxDistanceCalculationRange;
            if (endIndex < 0) endIndex += positionBufferSize;

            Vector3 latestPos = positionBuffer[latestIndex];

            Vector3 min = new Vector3(latestPos.x, latestPos.y, latestPos.z);
            Vector3 max = new Vector3(latestPos.x, latestPos.y, latestPos.z);

            do
            {
                latestIndex--;
                if (latestIndex < 0) latestIndex += positionBufferSize;
                Vector3 currPos = positionBuffer[latestIndex];
                min.x = Mathf.Min(min.x, currPos.x);
                max.x = Mathf.Max(max.x, currPos.x);
                min.y = Mathf.Min(min.y, currPos.y);
                max.y = Mathf.Max(max.y, currPos.y);
                min.z = Mathf.Min(min.z, currPos.z);
                max.z = Mathf.Max(max.z, currPos.z);
            }
            while (latestIndex != endIndex);

            return max - min;
        }

    }
}
