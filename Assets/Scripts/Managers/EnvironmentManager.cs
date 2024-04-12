using System;
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

        [Header("Battleground Root")]
        public Transform Battleground;

        [Header("UI Root")]
        public Transform UIRoot;

        public Action<float> OnBattlegroundScaled; //param = scaleFactor
        public Action OnBattlegroundRepositioned; 

        public float CurrentBattleGroundScaleFactor { get { return currentBattlegroundScaleFactor; } }

        private Vector3 originalBattlegroundScale = Vector3.one;
        private float currentBattlegroundScaleFactor = 1f;

        private int walkableNavMeshAreaMask;

        private BattlegroundManager battlegroundManager;


        private void Start()
        {
            walkableNavMeshAreaMask = 1 << NavMesh.GetAreaFromName("Walkable");
            originalBattlegroundScale = Battleground.localScale;
            battlegroundManager = Battleground.gameObject.GetComponent<BattlegroundManager>();
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
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * maxDistanceFromMapOrigin;
            Vector3 sourcePoint = new Vector3(MapOrigin.position.x + randomPoint.x, MapOrigin.position.y, MapOrigin.position.z + randomPoint.y);

            return GetRandomPointOnMap(sourcePoint, maxDistanceFromTheSource);
        }

        public void SetupXROriginPos(int index) //TODO: move this out of environment manager
        {
            Transform spawnTrans = PlayerSpawnPoints[index];
            XROrigin.position = new Vector3(spawnTrans.position.x, XROrigin.position.y, spawnTrans.position.z); 
            XROrigin.rotation = spawnTrans.rotation;
            Vector3 uiEulerRot = UIRoot.localRotation.eulerAngles;
            UIRoot.localRotation = Quaternion.Euler(uiEulerRot.x, GetUIRotationByPlayerSpawnIndex(index), uiEulerRot.z);
        }

        public Vector3 GetSpawnPointPositionByIndex(int playerIndex)
        {
            return MapSpawnPoints[playerIndex].transform.position;
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

        //scaleFactor 0.1 - 1
        public void RescaleBattleground(float scaleFactor)
        {
            currentBattlegroundScaleFactor = Mathf.Clamp(scaleFactor, 0.1f, 1f);
            Battleground.localScale = originalBattlegroundScale * currentBattlegroundScaleFactor;

            battlegroundManager.RebuildNavMesh();

            OnBattlegroundScaled?.Invoke(currentBattlegroundScaleFactor);
        }

        public void EnableBattlegroundRotation()
        {
            battlegroundManager.EnableBattlegroundRotation();
        }

        public void DisableBattlegroundRotation()
        {
            battlegroundManager.DisableBattlegroundRotation();
            battlegroundManager.RebuildNavMesh();
        }

        private float GetUIRotationByPlayerSpawnIndex(int index)
        {
            switch (index)
            {
                case 0: return 0f;
                case 1: return 180f;
                case 2: return 90f;
                case 3: return -90f;
            }

            return 0f;
        }

        public void AttachBattlegroundTo(Transform deskAnchor, Vector3 offset)
        {
            battlegroundManager.transform.parent = deskAnchor;
            battlegroundManager.transform.localPosition = Vector3.zero;
            battlegroundManager.transform.position = battlegroundManager.transform.position + offset;

            Vector3 battlegroundEuler = battlegroundManager.transform.eulerAngles;
            
#if UNITY_EDITOR
            battlegroundManager.transform.localRotation = Quaternion.identity;
#else
            battlegroundManager.transform.rotation = deskAnchor.rotation;
            battlegroundManager.transform.Rotate(Vector3.up, 180f);
            battlegroundManager.transform.Rotate(Vector3.right, -90f);
#endif
            OnBattlegroundRepositioned?.Invoke();
        }
    }
}
