using UnityEngine;
using UnityEngine.AI;

namespace MicroWar
{
    public class EnvironmentManager : MonoBehaviour, IEnvironmentManager
    {
        [Header("Spawn Points")]
        public Transform[] PlayerSpawnPoints;
        public Transform[] MapSpawnPoints;

        [Header("Origin Point Of The Map")]
        public Transform MapOrigin;
       
        [Header("XROrigin Of The Map")]
        public Transform XROrigin;
        
        private int walkableNavMeshAreaMask;

        private void Start()
        {
            walkableNavMeshAreaMask = 1 << NavMesh.GetAreaFromName("Walkable");
        }

        public Transform[] GetPlayerSpawnPoints() 
        {
            return PlayerSpawnPoints;
        }

        public Transform[] GetMapSpawnPoints()
        {
            return MapSpawnPoints;
        }

        public Transform GetMapOrigin()
        {
            return MapOrigin;
        }

        public int GetMapSpawnPointIndex(Transform transform)
        {
            for (int i = 0; i < MapSpawnPoints.Length; i++)
            {
                if (MapSpawnPoints[i] == transform) return i;
            }

            return -1;
        }

        public Vector3 GetRandomPointOnMap(Vector3 sourcePoint, float maxDistanceFromSource)
        {
            if (NavMesh.SamplePosition(sourcePoint, out NavMeshHit hit, maxDistanceFromSource, walkableNavMeshAreaMask))
            {
                return hit.position;
            }
            else
            {
                Debug.Log("Unable to find a position on the nav mesh!");
                return sourcePoint; //Meaning that we couldn't get a point on the NavMesh
            }
        }

        public Vector3 GetRandomPointOnMap(float maxDistanceFromMapOrigin, float maxDistanceFromTheSource)
        {
            Vector2 randomPoint = Random.insideUnitCircle * maxDistanceFromMapOrigin;
            Vector3 sourcePoint = new Vector3(MapOrigin.position.x + randomPoint.x, MapOrigin.position.y, MapOrigin.position.z + randomPoint.y);

            return GetRandomPointOnMap(sourcePoint, maxDistanceFromTheSource);
        }

        public void SetupXROriginPos(int index) //TODO: move this out of environment manager
        {
            XROrigin.position = PlayerSpawnPoints[index].position; 
            XROrigin.rotation = PlayerSpawnPoints[index].rotation;
        }

        public void ShowSphereForceByIndex(int playerIndex)
        {
            for (int i = 0; i < MapSpawnPoints.Length; i++)
            {
                SpawnPoint Sp;
                MapSpawnPoints[i].TryGetComponent<SpawnPoint>(out Sp);
                if (Sp != null && playerIndex == i)
                {
                    Sp.EnableForceField();
                }
            }
        }

        public void HideAllSphereForce()
        {
            for (int i = 0; i < MapSpawnPoints.Length; i++)
            {
                SpawnPoint Sp;
                MapSpawnPoints[i].TryGetComponent<SpawnPoint>(out Sp);
                if (Sp != null)
                {
                    Sp.DisableForceField();
                }
            }
        }

    }
}
