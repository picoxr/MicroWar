using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MicroWar
{
    public class DebugLogView : MonoBehaviour
    {
        [Header("Switch Button")]
        public Button SwitchButton; // Controls open/close of the log panel
        public TMP_Text SwitchButtonText;

        [Header("Log Content Panel")]
        public RectTransform LogContentPanel;
        public TMP_Text LogText;

        [Header("Clear Button")]
        public Button ClearButton; // Controls open/close of the log panel
        
        private int maxLineCount = 100;
        private Queue<string> logQueue;
        
        private void OnEnable()
        {
            logQueue = new Queue<string>(maxLineCount);
            DebugUtils.PrintLog += ReceiveLog;
            SwitchButton.onClick.AddListener(ShowHideLogContentPanel);
            ClearButton.onClick.AddListener(Clear);
            UpdateSwitchButtonState();
        }
        private void OnDisable()
        {
            DebugUtils.PrintLog -= ReceiveLog;  
        }
#region Button Events
        public void Clear()
        {
            logQueue.Clear();
            ShowLog();
        }

        public void SaveLogFile(bool isOpen)
        {
            UnityEngine.Debug.Log(isOpen);
            if (isOpen)
            {
                DebugUtils.SaveLog = true;
            }
            else
            {
                DebugUtils.SaveLog = false;
            }
        }
#endregion
        private void ReceiveLog(LogLevel level, string message)
        {
            string format=null;
            switch (level)
            {
                case LogLevel.LOG:
                    format = $"<color #FFFFFF>{message}</color> \n";
                    break;
                case LogLevel.WARNING:
                    format = $"<color #FAEE00>{message}</color> \n";
                    break;
                case LogLevel.ERROR:
                    format = $"<color #FB0000>{message}</color> \n";
                    break;
            }

            if(logQueue.Count >= maxLineCount)
            {
                logQueue.Dequeue();
            }
            logQueue.Enqueue(format);
            ShowLog();
        }
        
        private void ShowLog()
        {
            string text = "";
            foreach(string str in logQueue)
            {
                text += str;
            }
            LogText.text = text;
        }
        private void ShowHideLogContentPanel()
        {
            var state = UpdateSwitchButtonState();
            if (state) //The log panel is activated, close the panel
            {
                LogContentPanel.gameObject.SetActive(false);
            }
            else
            {
                LogContentPanel.gameObject.SetActive(true);
            }
            UpdateSwitchButtonState();
        }

        private bool UpdateSwitchButtonState()
        {
            bool IsActive = LogContentPanel.gameObject.activeInHierarchy;
            if (IsActive)
            {
                SwitchButtonText.text = "Hide";
                return true;
            }
            else
            {
                SwitchButtonText.text = "Show";
                return false;
            }

        }
    }
}
