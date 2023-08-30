using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MicroWar.Platform;
public class UIGlobalErrorPanel : MonoBehaviour
{
    public Button Retrybtn;
    public Button Backbtn;
    public TMP_Text ErrorText;
    // Start is called before the first frame update
    void Start()
    {
        Retrybtn.onClick.AddListener(RetryInit);
        Backbtn.onClick.AddListener(Back);
        PlatformServiceManager.Instance.RegisterNotification<ErrorEvent>(errorEventHandler);
    }

    private void errorEventHandler(EventWrapper<ErrorEvent> EventData)
    {
        var msg = EventData.Data;
        SetErrorText(msg.ErrorMessage);
        Show();
    }

    private void Back()
    {
        MicroWar.UIPanelManager.Instance.ResetControllerUI(); // Reset UI index TODO: temp code. 
        MicroWar.UIPanelManager.Instance.ShowPanelUI(); // Show the default UI (index 0). TODO: Optimize this. Uncleared panel structure may cause confusion in the later stage
        Hide();
    }

    private void RetryInit()
    {
        PlatformServiceManager.Instance.InitPlatformServices();
        Back();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetErrorText(string error)
    {
        ErrorText.text = error;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        //Add method to disable general control for other panels.
    }

    public void Hide()
    {
        ErrorText.text = string.Empty;
        gameObject.SetActive(false);
    }

}
