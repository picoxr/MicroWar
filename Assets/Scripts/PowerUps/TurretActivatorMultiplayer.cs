using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MicroWar.Utils;
public class TurretActivatorMultiplayer : NetworkBehaviour
{
    public LineRenderer TurretLinkCurve;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (TurretLinkCurve == null)
            TurretLinkCurve = GetComponentInChildren<LineRenderer>();
    }

    [ClientRpc]
    public void DrawCurveClientRpc(Vector3 turretPosition)
    {
        CurveUtils.DrawCurveBetween(transform.position, turretPosition, 20, TurretLinkCurve);
    }

}
