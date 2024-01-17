using Unity.AI.Navigation;
using UnityEngine;

public class BattlegroundManager : MonoBehaviour
{
    public NavMeshSurface battleGroundNavMesh;

    private BattlegroundRotationHandler battlegroundRotationHandler;

    private void Start()
    {
        battlegroundRotationHandler = GetComponent<BattlegroundRotationHandler>();
        battlegroundRotationHandler.enabled = false;
    }

    public void EnableBattlegroundRotation()
    {
        battlegroundRotationHandler.enabled = true;
    }

    public void DisableBattlegroundRotation()
    {
        battlegroundRotationHandler.enabled = false;
    }

    public void RebuildNavMesh()
    {
        battleGroundNavMesh.BuildNavMesh();
    }
}
