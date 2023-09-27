
using UnityEngine;

namespace MicroWar
{ 
    public interface IEnvironmentManager
    {
        Transform[] GetPlayerSpawnPoints();
        Transform[] GetMapSpawnPoints();
        Transform GetMapOrigin();

        int GetMapSpawnPointIndex(Transform transform);
        Vector3 GetRandomPointOnMap(float maxDistanceFromMapOrigin, float maxDistanceFromTheSource);
        Vector3 GetRandomPointOnMap(Vector3 sourcePoint, float maxDistanceFromSource);

        Vector3 GetSpawnPointPositionByIndex(int playerIndex);
    }
}