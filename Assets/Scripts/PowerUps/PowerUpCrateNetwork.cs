using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MicroWar;
using System;

public class PowerUpCrateNetwork : NetworkBehaviour
{
    PowerUpCrate Crate;
    // Start is called before the first frame update
    void Start()
    {
        Crate = GetComponent<PowerUpCrate>();
        if (IsClient)
        {
            Crate.OnAllAnimFinishMultiplayer = OnAllAnimFinished;
        }
    }

    private void OnAllAnimFinished()
    {
        Crate.gameObject.SetActive(false);
    }
}
