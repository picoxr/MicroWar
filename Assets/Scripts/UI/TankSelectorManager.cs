using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using MicroWar;
namespace MicroWar
{
    [Serializable]
    public class TankButtonModelPair
    {
        public Button button;
        public GameObject model;
        [SerializeField] // Add the serialized field attribute
        private TankInfo tankInfo;
        public TankInfo TankInfo => tankInfo; // Add a property to access the TankInfo variable
    }

    [Serializable]
    public class TankInfo
    {
        public string tankName;
        public string description;
        public TankStatistics statistics;
        public VehicleType tankType; // Add the tank type field
    }

    [Serializable]
    public class TankStatistics
    {
        public float speed;
        public int damage;
        public int healthPoints;
    }

    public class TankSelectorManager : MonoBehaviour
    {
        public List<TankButtonModelPair> tankButtonModelPairs = new List<TankButtonModelPair>(); // List to store button-model pairs

        private Dictionary<Button, GameObject> modelDictionary = new Dictionary<Button, GameObject>(); // Dictionary to associate buttons with 3D models

        public TextMeshProUGUI tankNameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI damageText;
        public TextMeshProUGUI healthPointsText;

        public Slider speedSlider;
        public Slider damageSlider;
        public Slider healthPointsSlider;

        public Button singleConfirmButton;
        public Button multiConfirmButton;

        public GameObject tankControllerUI; // Reference to the Tank Controller UI GameObject

        private void Start()
        {
            if (tankButtonModelPairs.Count > 0)
            {
                foreach (var pair in tankButtonModelPairs)
                {
                    Button button = pair.button;
                    GameObject model = pair.model;
                    TankInfo tankInfo = pair.TankInfo;

                    button.onClick.AddListener(() =>
                    {
                        OnTankModelClick(model, tankInfo);
                    });

                    modelDictionary.Add(button, model);
                }

                var firstModel = modelDictionary.Values.GetEnumerator();
                firstModel.MoveNext();
                firstModel.Current.SetActive(true);

                // Display the information of the first tank
                UpdateTankInfo(tankButtonModelPairs[0].TankInfo);
            }
            else
            {
                Debug.LogError("No valid tank-button pairs found!");
            }

            singleConfirmButton.onClick.AddListener(OnClickConfirm);
            multiConfirmButton.onClick.AddListener(OnClickConfirm);
        }

        private void OnTankModelClick(GameObject model, TankInfo tankInfo)
        {
            // Deactivate all models
            foreach (var pair in modelDictionary)
            {
                pair.Value.SetActive(false);
            }

            model.SetActive(true); // Activate the clicked model

            UpdateTankInfo(tankInfo); // Update the displayed tank info
        }

        private void OnClickConfirm()
        {
            //var selectedModel = modelDictionary.Values.FirstOrDefault(model => model.activeSelf);
            //if (selectedModel != null)
            //{
            //    var selectedPair = tankButtonModelPairs.FirstOrDefault(pair => pair.model == selectedModel);
            //    if (selectedPair != null)
            //    {
            //        TankType selectedTankType = selectedPair.TankInfo.tankType;
            //        Debug.Log("Selected Tank Type: " + selectedTankType);

            //        GameManager.Instance.SetTankType(selectedTankType); // Set the tank type in the GameManager using the Instance property

            //        if(GameManager.Instance.IsSinglePlayer)
            //            GameManager.Instance.StartGame(); // Start the game in the GameManager using the Instance property
            //    }
            //}

            GameManager.Instance.StartSession(SessionType.SinglePlayer);
            tankControllerUI.SetActive(false); // Completely close the UI window
        }

        private void UpdateTankInfo(TankInfo tankInfo)
        {
            tankNameText.text = string.Format("Tank Name: {0}", tankInfo.tankName);
            descriptionText.text = string.Format("Description: {0}", tankInfo.description);

            // Update the values of the sliders
            speedText.text = string.Format("Speed: {0}", tankInfo.statistics.speed);
            damageText.text = string.Format("Damage: {0}", tankInfo.statistics.damage);
            healthPointsText.text = string.Format("Health Points: {0}", tankInfo.statistics.healthPoints);

            // Update the values of the sliders
            speedSlider.value = tankInfo.statistics.speed / 10f;
            damageSlider.value = tankInfo.statistics.damage / 10f;
            healthPointsSlider.value = tankInfo.statistics.healthPoints / 10f;
        }
    }
}

