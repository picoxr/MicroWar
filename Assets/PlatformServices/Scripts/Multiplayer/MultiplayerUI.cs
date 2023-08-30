using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MicroWar.Multiplayer;
using UnityEngine.UI;
using Pico.Platform;
using Pico.Platform.Models;
using TMPro;
using MicroWar.Platform;
using MicroWar;

public class MultiplayerUI : MonoBehaviour
{
    public Button CreateRoomBtn;
    public Button LeaveRoomBtn;
    public Button RefreshBtn;
    public Button ReadyBtn;
    public Button StartGameBtn;


    public TMP_Text RoomInfoID;
    public TMP_Text RoomInfoPlayerCount;
    public TMP_Text RoomListText; 
    public TMP_Text PlayerListText; 
    public GameObject PlayerEntity;
    public Transform PlayerEntityParent;

    public UIGlobalErrorPanel errorPanel;

    public RectTransform RoomEntityParent;
    public GameObject RoomEntity;

    //public RectTransform ErrorPanel;

    private Room roomInfo;
    private RoomServiceStatus roomStatus;

    private PlatformController_Rooms roomController;
    private List<GameObject> playerEntities = new List<GameObject>();

    #region Unity Messages
    private void Awake()
    {
        CreateRoomBtn.onClick.AddListener(CreateRoom);
        LeaveRoomBtn.onClick.AddListener(LeaveRoom);
        RefreshBtn.onClick.AddListener(RetrieveRoomList);
        StartGameBtn.onClick.AddListener(StartGame);
        //EnableBot.isOn = true; //[TEMP] Enable bot in multiplayer mode by default.
    }

    private void Start()
    {
        var initCheck = BootManager.Instance.ErrorNotificationFlag;

        if (initCheck)
        {
           errorPanel.SetErrorText("Unable connect to server, Please check the network connection and try again.");
           errorPanel.Show();
        }

        roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>();
        // Handle room status event

        // Subscribe the event handler to the RoomUpdateEvent
        PlatformServiceManager.Instance.RegisterNotification<RoomUpdateEvent>(HandleRoomUpdateEvent);
        // Subscribe the event handler to the RoomListEvent
        PlatformServiceManager.Instance.RegisterNotification<RoomListEvent>(HandleRoomListEvent);
    }

    private void OnDestroy()
    {
        if (PlatformServiceManager.Instance == null) return;
        PlatformServiceManager.Instance.UnregisterNotification<RoomUpdateEvent>(HandleRoomUpdateEvent);
        PlatformServiceManager.Instance.UnregisterNotification<RoomListEvent>(HandleRoomListEvent);
    }
    #endregion
    private void StartGame()
    {
        if (roomInfo.PlayerNumber >= 1 || MultiplayerBehaviour.Instance.IsUseBot) // [TEMP] Always use bot
        {
            GameManager.Instance.StartSession(SessionType.Multiplayer);
        }
    }

    /*
    private void Ready()
    {
        //MultiplayerManager.Instance.NetworkPlayerReady(TankType.Bear);
    }*/

    private void UpdateRoomList(RoomList list)
    {
        if (list != null)
        {
            // Clear Room Entities
            if (RoomEntityParent.childCount > 0)
            {
                for (int i = 0; i < RoomEntityParent.childCount; i++)
                {
                    GameObject.Destroy(RoomEntityParent.GetChild(i).gameObject);
                }
            }

            foreach (Room room in list)
            {
                if (roomInfo != null && room.RoomId == roomInfo.RoomId)
                {
                    continue;
                }
                var roomEntity = GameObject.Instantiate(RoomEntity);
                roomEntity.transform.SetParent(RoomEntityParent, false);
                var component = roomEntity.AddComponent<RoomEntity>();
                component.Init(room.RoomId);
            }

            if(RoomListText!=null)//TODO: Temp fixed this, should know if there are logic error
            RoomListText.gameObject.SetActive(false); // Hide the room list message if there are rooms available
        }
        else
        {
            RoomListText.gameObject.SetActive(true); // Show the room list message
            RoomListText.text = "No rooms found."; // Set the message text
        }
    }

    private void UpdateRoomInfo(Room currentRoom)
    {
        // Clear player entities
        foreach (var playerEntity in playerEntities)
        {
            Destroy(playerEntity);
        }
        playerEntities.Clear();

        if (currentRoom == null || currentRoom.RoomId.Equals(0))
        {
            RoomInfoID.text = string.Empty;
            RoomInfoPlayerCount.text = string.Empty;
            PlayerListText.gameObject.SetActive(false); // Hide the player list message if there is no current room
            //TODO: Reload the scene?/Error? Solution: Pop-up error and back to the main menu.
        }
        else
        {
            roomInfo = currentRoom;
            var roomID = currentRoom.RoomId.ToString(); // Convert the room ID to a string
            var playerList = currentRoom.UsersOptional;
            RoomInfoID.text = roomID;
            RoomInfoPlayerCount.text = playerList.Count.ToString(); // Convert the player count to a string
            if (playerList.Count > 0)
            {
                foreach (User p in playerList)
                {
                    var playerEntity = GameObject.Instantiate(PlayerEntity);
                    playerEntity.transform.SetParent(PlayerEntityParent, false);
                    playerEntity.GetComponentInChildren<TMP_Text>().text = p.DisplayName;
                    playerEntities.Add(playerEntity);
                }
                //TODO: [Temp] Disable start button for clients.
                if (!CheckIsOwner(currentRoom.OwnerOptional))
                {
                    StartGameBtn.gameObject.SetActive(false);
                }
                else
                {
                    StartGameBtn.gameObject.SetActive(true);
                    StartGameBtn.interactable = false;
                    if (playerList.Count >= 1 || MultiplayerBehaviour.Instance.IsUseBot) // [TEMP] Always use bot
                    {
                        StartGameBtn.interactable = true;
                    }
                }

                PlayerListText.gameObject.SetActive(false); // Hide the player list message if there are players in the current room
            }
            else
            {
                PlayerListText.gameObject.SetActive(true); // Show the player list message
                PlayerListText.text = "No players found."; // Set the message text
            }
        }
    }

    public void CreateRoom()
    {
        roomController.CreateRoom(4);
    }

    public void RetrieveRoomList()
    {
        roomController.RetrieveRoomList();
    }

    public void LeaveRoom()
    {
        roomController.LeaveRoom();
    }

    private void HandleRoomUpdateEvent(EventWrapper<RoomUpdateEvent> msg)
    {
        if (msg.Data.RoomServiceStatus != RoomServiceStatus.Idle) // Don't allow creating rooms unless the state is idle.
        {
            CreateRoomBtn.interactable = false; // Update UI to prevent the user from clicking the button.
            if (msg.Data.RoomServiceStatus == RoomServiceStatus.InRoom) // If in a room, update the room info UI.
            {
                UpdateRoomInfo(msg.Data.CurrentRoom); // Refresh UI
            }
            return;
        }
        CreateRoomBtn.interactable = true;
        UpdateRoomInfo(msg.Data.CurrentRoom);
        //RetrieveRoomList(); 
    }

    private void HandleRoomListEvent(EventWrapper<RoomListEvent> msg)
    {
        if (msg.Data.RoomListRetrieveStatus != RoomListRetrieveStatus.Idle)
            return;
        UpdateRoomList(msg.Data.RoomList);
    }

    private bool CheckIsOwner(User owner)
    {
        if (owner != null)
        {
            if(owner.ID == PlatformServiceManager.Instance.Me.ID)
            {
                Debug.Log("Is owner");
                return true;
            }
        }
        return false;
    }

}
