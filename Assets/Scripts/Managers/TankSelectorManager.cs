using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankSelectorManager : MonoBehaviour
{
    public List<GameObject> tanks;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        ResetTanks();
    }

    public void ResetTanks()
    {
        foreach (var tank in tanks) 
        {
            tank.transform.localPosition = Vector3.zero;
            tank.SetActive(true);
        }
    }

}
