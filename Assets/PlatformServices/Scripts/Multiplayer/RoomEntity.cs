using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MicroWar.Platform;
using Unity.VisualScripting;
using MicroWar;

public class RoomEntity : MonoBehaviour, ISingleton
{
    ulong _roomID;
    Button button;
    TMP_Text text;
    PlatformController_Rooms roomController;
    public void Init(ulong roomID)
    {
        _roomID = roomID;
        text = transform.GetComponentInChildren<TMP_Text>();
        text.text = $"Room: {_roomID}";
       roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>();
    }

    private void Awake()
    {
        button = gameObject.GetComponentInChildren<Button>();
        button.onClick.AddListener(OnButtonClick);
    }
    private void OnButtonClick()
    {
        roomController.JoinToRoom(_roomID);
        UIPanelManager.Instance.NextPanel();
    }

    public void DestorySelf()
    {
        GameObject.Destroy(this.gameObject);
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
