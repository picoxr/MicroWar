using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TurretControllerNetwork : NetworkBehaviour
{
    private TurretController turretController;
    #region UnityMessages
    // Start is called before the first frame update
    void Start()
    {
        turretController = GetComponent<TurretController>();
        turretController.OnFireMultiple += OnFireMultipleHandler;
    }
    private void OnDestroy()
    {
        turretController.OnFireMultiple -= OnFireMultipleHandler;
    }
    #endregion
    //Host
    private void OnFireMultipleHandler(List<Transform> targetTransforms)
    {
        List<ulong> targetNetworkIds = new List<ulong>();
        foreach (Transform trans in targetTransforms)
        {
            NetworkObject playerNetworkObject;
            if (trans.TryGetComponent<NetworkObject>(out playerNetworkObject))
            {
                targetNetworkIds.Add(playerNetworkObject.NetworkObjectId);
            }
        }
        Debug.Log("<color=fff00ffff>Try send turret fire client RPC</color>");
        TurretFireClientRpc(targetNetworkIds.ToArray());
    }
    //Client
    [ClientRpc]
    public void TurretFireClientRpc(ulong[] targetNetworkIds) //TODO: Optimize this
    {
        if (IsHost)
            return;
        Debug.Log("<color=fff00ffff>Receive turret fire client RPC</color>");
        
        turretController.SimulateFire(targetNetworkIds);
    }
}
