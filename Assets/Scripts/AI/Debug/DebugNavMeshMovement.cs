using UnityEngine;
using UnityEngine.AI;

namespace MicroWar.AI.Debug
{
    public class DebugNavMeshMovement : MonoBehaviour
    {
        public Transform goal;

        void Start()
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            agent.destination = goal.position;
        }
    }
}

