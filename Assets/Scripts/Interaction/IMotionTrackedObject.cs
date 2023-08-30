using UnityEngine;

namespace MicroWar
{
    public interface IMotionTrackedObject
    {
        public Vector3 GetMaxDistancesForEachAxis();
        public Vector3 GetMotionVector();
        public Vector3 GetMotionVelocity();
        public float GetMotionTime();
    }
}