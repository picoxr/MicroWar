using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MicroWar
{
    public class InGameUIHandler : MonoBehaviour
    {
        public TMP_Text MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject MessageCanvas;
        public GameObject HandTrackingUICanvas;
        public void ShowMessageCanvas()
        {
            MessageCanvas.SetActive(true);
        }

        public void HideMessageCanvas()
        {
            MessageCanvas.SetActive(false);
        }

        public void UpdateMessage(string message)
        {
            MessageText.text = message;
        }

        public void ShowHandTrackingUICanvas()
        {
            HandTrackingUICanvas.SetActive(true);
        }

        public void HideHandTrackingUICanvas()
        {
            HandTrackingUICanvas.SetActive(false);
        }

        public void ShowEndMessage() 
        {
            string message = EndMessage();
            UpdateMessage(message);
        }

        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (GameManager.Instance.currentSession.RoundWinner != null)
                message = GameManager.Instance.currentSession.RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (int i = 0; i < GameManager.Instance.Vehicles.Count; i++)
            {
                message += GameManager.Instance.Vehicles[i].m_ColoredPlayerText + ": " + GameManager.Instance.Vehicles[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that.
            if (GameManager.Instance.currentSession.SessionWinner != null)
                message = GameManager.Instance.currentSession.SessionWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }
    }
}
