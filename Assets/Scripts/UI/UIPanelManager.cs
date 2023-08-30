using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using MicroWar.Platform;

namespace MicroWar
{
    [System.Serializable]
    public class PanelGroup
    {
        public List<GameObject> panels;
    }

    public class UIPanelManager : MonoBehaviour, ISingleton
    {
        public List<PanelGroup> panelGroups; // List to hold groups of UI panels

        public Button singleplayerSelectBtn;
        public Button multiplayerSelectBtn;

        private int panelIndex = 0; // Current index of the panel to be shown
        public GameObject tankControllerUI; // Reference to the Tank Controller UI GameObject

        private static UIPanelManager _instance;
        public static UIPanelManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void Start()
        {
            ShowPanel(); // Show the initial panel
            multiplayerSelectBtn.onClick.AddListener(NextPanel); // Assign the NextPanel function to the button's click event
        }

        private void ShowPanel()
        {
            // Disable all panels
            foreach (PanelGroup group in panelGroups)
            {
                foreach (GameObject panel in group.panels)
                {
                    panel.SetActive(false);
                }
            }

            // Enable the current panel group
            if (panelIndex < panelGroups.Count)
            {
                PanelGroup currentGroup = panelGroups[panelIndex];
                foreach (GameObject panel in currentGroup.panels)
                {
                    panel.SetActive(true);
                }
            }
        }
        public void OpenSpecificPanel(GameObject panelToOpen)
        {
            // Disable all panels
            foreach (PanelGroup group in panelGroups)
            {
                foreach (GameObject panel in group.panels)
                {
                    panel.SetActive(false);
                }
            }

            // Enable the specified panel
            panelToOpen.SetActive(true);
        }

        public void NextPanel()
        {
            panelIndex++; // Increment the panel index

            ShowPanel(); // Show the next panel
        }

        public void PreviousPanel()
        {
            panelIndex--; // Increment the panel index

            ShowPanel(); // Show the next panel
        }

        public void OnSingleplayerSelectBtnClick()
        {
            GameManager.Instance.IsSinglePlayer = true;

            // Disable the multiplayer selection button
            multiplayerSelectBtn.gameObject.SetActive(false);

            // Enable the singleplayer selection button
            singleplayerSelectBtn.gameObject.SetActive(true);
        }

        public void OnMultiplayerSelectBtnClick()
        {
            GameManager.Instance.IsSinglePlayer = false;

            // Disable the singleplayer selection button
            singleplayerSelectBtn.gameObject.SetActive(false);

            // Enable the multiplayer selection button
            multiplayerSelectBtn.gameObject.SetActive(true);
        }

        public void GoToRoomPanel()
        {
            // Only switch panel if room can be created - To be adjusted
            if (!PlatformServiceManager.Instance.GetController<PlatformController_Rooms>().CanCreateRoom())
                return;

            NextPanel();
        }

        public void ResetControllerUI()
        {
            tankControllerUI.SetActive(false);
            panelIndex = 0;

        }
        public void HideControllerUI()
        {
            tankControllerUI.SetActive(false);
        }

        public void ShowPanelUI()
        {
            Debug.Log("Show panel");
            tankControllerUI.SetActive(true);
            ShowPanel();
        }

    }
}