using MicroWar.AI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MicroWar.Multiplayer;

namespace MicroWar
{
    public enum VehiclePlayerType
    {
        Local,
        AI,
        Network,
    }

    public enum VehicleType
    {
        Tiger,
        Bear,
        Cheetah
    }

    public abstract class VehicleManagerBase
    {
        public VehicleType VehicleType;
        public Color m_PlayerColor;  
        public Transform m_SpawnPoint;  
        public VehiclePlayerType m_PlayerType;
        protected VehicleSettings vehicleSettings;
        [HideInInspector] public int m_PlayerNumber;  
        [HideInInspector] public string m_ColoredPlayerText; 
        [HideInInspector] public GameObject m_Instance;
        [HideInInspector] public int m_Wins; 

        //Multiplayer/AI
        [HideInInspector] public string m_userName;
        [HideInInspector] public ulong m_clientId;
        [HideInInspector] public string m_pId;
        [HideInInspector] protected NetworkObject m_NetworkObject;
        [HideInInspector] protected NetPlayer m_NetworkPlayer;
        [HideInInspector] public bool IsSpawned;
        [HideInInspector] public bool IsSelf;

        protected GameObject m_CanvasGameObject; 
        protected XRGrabInteractable m_XRGrabInteractable;

        //AI
        protected VehicleAIStateController vehicleAIController;

        //PowerUp
        [HideInInspector] public CrateContainerQueue crateContainer;

        public virtual void Setup(VehicleSettings vehicleSettings) { this.vehicleSettings = vehicleSettings; }
        public abstract void DisableControl();
        public abstract void EnableControl();
        public abstract void Reset();
        public abstract void Spawn();
        public abstract void DespawnTank();
        public abstract void RespawnTank();
        public abstract ITankHealth GetVehicleHealth();
    }
}

