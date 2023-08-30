using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MicroWar.Platform;
//TODO: We need an abstract class/ manager class to handle all UI panel classes in order to implement more UI specific features like UI animation...
public class UIRoomCreationPanel : MonoBehaviour 
{
    // Trying to decouple logic part with UI panel control.
    public UIGlobalErrorPanel errorPanel; //[Warn (Ilyas)]: Shouldn't add more direct reference in individual UI scripts. It's easy to ignore them when we have any changes on the UI prefab.
    private PlatformController_Rooms roomController;
    // Start is called before the first frame update
    private void OnEnable()
    {
        //This is the first panel when we click multiplayer mode. 
        //Do initialize check here.
        //TODO: Each UI panel should have a place to do the UI animation initialization stuff/ init check. It's not so good to place them here, since we should have dedicated logic to handle UI lifetime.
        if(PlatformServiceManager.Instance.GameServiceInitializeStatus != ServiceInitializeStatus.initialized)
        {
            //Show error panel
            errorPanel.SetErrorText("Unable connect to server, Please check the network connection and try again.");
            errorPanel.Show();
        }
    }
    void Start()
    {
        //Retrieve room list when open the room creation panel.
        roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>();
        if (roomController != null)
        {
            Debug.Log("Retrieve Room List");
            roomController.RetrieveRoomList();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
