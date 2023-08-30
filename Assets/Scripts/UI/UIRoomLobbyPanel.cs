using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MicroWar.Multiplayer;
using UnityEngine.UI;

public class UIRoomLobbyPanel : MonoBehaviour
{
    public Toggle EnableBot;
    public Toggle EnableRTC;
    // Start is called before the first frame update
    void Start()
    {
        EnableBot.onValueChanged.AddListener((bool value) => { MultiplayerBehaviour.Instance.SetIsUseBot(value); });
        EnableBot.onValueChanged.AddListener((bool value) => { MultiplayerBehaviour.Instance.SetIsUseRTC(value); });
    }
}
