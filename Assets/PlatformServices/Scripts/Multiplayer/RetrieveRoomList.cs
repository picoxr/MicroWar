using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MicroWar.Platform;

/// <summary>
/// A hack to retrieve room list when open the room creation panel.
/// </summary>
public class RetrieveRoomList : MonoBehaviour
{
    public PlatformController_Rooms roomController;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (roomController != null)
        {
            Debug.Log("Retrieve Room List");
            roomController.RetrieveRoomList();
        }
    }
    void Start()
    {
        roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
