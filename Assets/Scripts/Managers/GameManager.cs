using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using MicroWar.Multiplayer;
using System.Collections.Generic;
namespace MicroWar
{

    public enum VehicleControlType
    {
        RemoteControlToyLeftControllerOnly,
        RemoteControlToyBothController,
        RealisticControl,
        VideoGameControl,
    }

    public class GameManager : MonoBehaviour
    {
        public bool VehicleIsDeployed = false;
        public GameObject VehicleSelector;
        public GameObject Sphereforce;
        public GameManagerSettings Settings;
        public InGameUIHandler InGameUIHandler;
        public MicroWarHand LeftHandInteractor;
        public MicroWarHand RightHandInteractor;
        public XRInteractorLineVisual LefthandLaser;
        public XRInteractorLineVisual RighthandLaser;

        //Vehicle Shield 
        public GameObject shieldPrefab;

        //SFX Overrides
        public AudioClip VehicleChargeSFXOverride;

        public List<SinglePlayerVehicleManager> Vehicles;               // A collection of managers for enabling and disabling different aspects of the tanks.
        public const float SCALE_FACTOR = .05f;
        public const float SQR_SCALE_FACTOR = SCALE_FACTOR * SCALE_FACTOR;
        public Transform BoardGameOrigin;
        public Transform XROrigin;

        [Header("Managers")]
        public DynamicMapObjectsManager DynamicMapObjectsManager;
        public DynamicMapObjectsManagerMultiplayer DynamicMapObjectsManagerMultiplayer;
        public VehicleConfigManager VehicleConfigManager;
        public EnvironmentManager EnvironmentManager;

        public SessionManagerBase currentSession { get; private set; }

        public const string playerLayerName = "Players";
        [Space]
        public int playerLayer = -1;
        public LayerMask playerLayerMask = 0;
    
        #region Comment Out
        /*
        public TMP_Dropdown tankControlDropDown;


        public Slider baseMovementSpeedSlider;
        public TMP_Text movementSpeedText;


        public Slider baseTurnSpeedSlider;
        public TMP_Text turnSpeedText;
        */
        #endregion
        private bool isSinglePlayer = true;
        public bool IsSinglePlayer
        {
            get { return isSinglePlayer; }
            set { isSinglePlayer = value; }
        }

        private static GameManager _instance;

        public static GameManager Instance { get { return _instance; } }

        #region Refactor Functions
        public void StartSession(SessionType sessionType)
        {
            switch (sessionType)
            {
                case SessionType.SinglePlayer:
                    currentSession = gameObject.AddComponent<SinglePlayerSessionManager>();
                    DynamicMapObjectsManager.Initialize(this);
                    break;
                case SessionType.Multiplayer:
                    GameObject multiplayerSessionManagerObj = new GameObject("multiplayerSessionManagerObj");
                    currentSession = multiplayerSessionManagerObj.AddComponent<MultiplayerSessionManager>();
                    DynamicMapObjectsManagerMultiplayer.Initialize(this);
                    break;
            }

            currentSession.Initialize(this);
            currentSession.StartSession();
        }
        #endregion

        public void SetVehicleType(VehicleType vehicleType)
        {
            Settings.PlayerVehicleType = vehicleType;
        }

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
            InitRandomSeed();
        }

        private void InitRandomSeed()
        {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        }

        private void Start()
        {
            EnsurePlayerMaskIsDefined();
            InGameUIHandler.HideMessageCanvas();

            if (XROrigin == null)
            {
                XROrigin = FindAnyObjectByType<XROrigin>().transform;
            }
        }

        private void EnsurePlayerMaskIsDefined()
        {
            playerLayer = LayerMask.NameToLayer(playerLayerName);
            if (playerLayerMask == 0)
            {
                playerLayerMask = 1 << playerLayer;
            }
        }

        public int GetActivePowerUpCount()
        {
            int count = 0;
            if (currentSession == null) return count;
            
            if(currentSession.SessionType == SessionType.SinglePlayer)
            {
                if (DynamicMapObjectsManager != null)
                {
                    count = DynamicMapObjectsManager.GetActivePowerUpCount();
                }
            }
            else
            {
                if (DynamicMapObjectsManagerMultiplayer != null)
                {
                    count = DynamicMapObjectsManagerMultiplayer.GetActivePowerUpCount();
                }
            }
            return count;
        }
        #region Comment Out
        //TODO: Move these methods out of GameManager
        /*
        public List<TankManager> GetAliveEnemiesOf(TankManager tank)
        {
            List<TankManager> tankList = new List<TankManager>();
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                TankManager currentTank = m_Tanks[i];
                if (currentTank == tank || currentTank.GetTankHealth().IsDead()) continue;

                tankList.Add(currentTank);
            }
            return tankList;
        }
      */

        /*
        public List<VehicleManagerBase> GetAliveEnemiesOf(VehicleManagerBase vehicle)
        {
            List<VehicleManagerBase> vehicleList = new List<VehicleManagerBase>();
            for (int i = 0; i < Vehicles.Length; i++)
            {
                VehicleManagerBase currentVehicle = Vehicles[i];
                if (currentVehicle == vehicle || currentVehicle.GetVehicleHealth().IsDead()) continue;

                vehicleList.Add(currentVehicle);
            }
            return vehicleList;
        }

        public List<Transform> GetAliveEnemyTransformsOf(SinglePlayerVehicleManager vehicle)
        {
            List<Transform> vehicleList = new List<Transform>();
            for (int i = 0; i < Vehicles.Length; i++)
            {
                SinglePlayerVehicleManager currentVehicle = Vehicles[i];
                if (currentVehicle == vehicle || currentVehicle.GetVehicleHealth().IsDead()) continue;

                vehicleList.Add(currentVehicle.m_Instance.transform);
            }
            return vehicleList;
        }

        public List<SinglePlayerVehicleManager> GetAlivePlayers()
        {
            List<SinglePlayerVehicleManager> vehicleList = new List<SinglePlayerVehicleManager>();
            for (int i = 0; i < Vehicles.Length; i++)
            {
                SinglePlayerVehicleManager currentVehicle = Vehicles[i];
                if (currentVehicle == null || currentVehicle.GetVehicleHealth().IsDead()) continue;

                vehicleList.Add(currentVehicle);
            }
            return vehicleList;
        }
        */
        #endregion
        #region Refactor




        #endregion

        private void OnDestroy()
        {
            /*
            if (currentSession != null)
            {
                currentSession.OnSessionStateChanged -= OnSessionStateChanged;
            }*/
        }
    }
}