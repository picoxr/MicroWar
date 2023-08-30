using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Transform ForceFieldSphere;
    
    public void EnableForceField()
    {
        ForceFieldSphere.gameObject.SetActive(true);
    }

    public void DisableForceField()
    {
        ForceFieldSphere.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
