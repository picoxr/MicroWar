using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static MicroWar.GameManager;

namespace MicroWar
{
    public enum SessionType
    {
        SinglePlayer,
        Multiplayer,
    }

    public enum SessionState
    {
        Idle,
        SessionStarting,
        RoundStarting,
        RoundPlaying,
        RoundEnding
    }
    public abstract class SessionManagerBase : MonoBehaviour
    {
        public SessionType SessionType { get; protected set; }

        protected GameManager gameManager;
        protected GameManagerSettings gameSettings;
        protected InGameUIHandler inGameUIHandler;
        protected VehicleConfigManager vehicleConfigManager;
        protected EnvironmentManager environmentManager;

        protected WaitForSeconds startWait;         // Used to have a delay whilst the round starts.
        protected WaitForSeconds endWait;           // Used to have a delay whilst the round or game ends.
        protected VehicleManagerBase sessionWinner;

        //protected VehicleManagerBase[] vehicles;
        protected List<VehicleManagerBase> vehicles;
        public VehicleManagerBase SessionWinner{ get { return sessionWinner; }}

        protected VehicleManagerBase roundWinner;
        public VehicleManagerBase RoundWinner { get { return roundWinner; } }
        public int RoundNumber { get; protected set; }

        public Action<SessionState> OnSessionStateChanged;
        //Session manager life cycle
        public virtual void Initialize(GameManager manager)
        {
            vehicles = new List<VehicleManagerBase>();
            gameManager = manager;

            gameSettings = gameManager.Settings;
            inGameUIHandler = gameManager.InGameUIHandler;
            vehicleConfigManager = gameManager.VehicleConfigManager;
            environmentManager = gameManager.EnvironmentManager;

            startWait = new WaitForSeconds(gameManager.Settings.StartDelay);
            endWait = new WaitForSeconds(gameManager.Settings.EndDelay);
        }

        public abstract void StartSession();
        //protected abstract void EndSession();

        //Common functions
        protected abstract void SpawnAllVehicles();
        protected abstract IEnumerator SessionLoop();
        protected abstract IEnumerator SessionStarting();
        protected abstract IEnumerator RoundStarting();
        protected abstract IEnumerator RoundPlaying();
        protected abstract IEnumerator RoundEnding();
        protected abstract void ResetAllVehicles();
        public abstract void EnableVehicleControl(); //Temp
        public abstract void DisableVehicleControl();//Temp

        protected bool OnePlayerLeft()// Same
        {
            // Start the count of tanks left at zero.
            int numPlayersLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < vehicles.Count; i++)
            {
                // ... and if they are active, increment the counter.
                if (vehicles[i].m_Instance.activeSelf)
                    numPlayersLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numPlayersLeft <= 1;
        }

        protected VehicleManagerBase GetRoundWinner()// Same
        {
            // Go through all the tanks...
            for (int i = 0; i < vehicles.Count; i++)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (vehicles[i].m_Instance.activeSelf)
                    return vehicles[i];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }


        // This function is to find out if there is a winner of the game.
        protected VehicleManagerBase GetGameWinner()// Same
        {
            // Go through all the tanks...
            for (int i = 0; i < vehicles.Count; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (vehicles[i].m_Wins == GameManager.Instance.Settings.NumRoundsToWin)
                    return vehicles[i];
            }
            // If no tanks have enough rounds to win, return null.
            return null;
        }

        public List<VehicleManagerBase> GetAliveEnemiesOf(VehicleManagerBase vehicle)
        {
            List<VehicleManagerBase> vehicleList = new List<VehicleManagerBase>();
            for (int i = 0; i < vehicles.Count; i++)
            {
                VehicleManagerBase currentVehicle = vehicles[i];
                if (currentVehicle == vehicle || currentVehicle.GetVehicleHealth().IsDead()) continue;

                vehicleList.Add(currentVehicle);
            }
            return vehicleList;
        }

        public List<Transform> GetAliveEnemyTransformsOf(VehicleManagerBase vehicle)
        {
            List<Transform> vehicleList = new List<Transform>();
            for (int i = 0; i < vehicles.Count; i++)
            {
                VehicleManagerBase currentVehicle = vehicles[i];
                if (currentVehicle == vehicle || currentVehicle.GetVehicleHealth().IsDead()) continue;

                vehicleList.Add(currentVehicle.m_Instance.transform);
            }
            return vehicleList;
        }

        public List<VehicleManagerBase> GetAlivePlayers()
        {
            List<VehicleManagerBase> vehicleList = new List<VehicleManagerBase>();
            for (int i = 0; i < vehicles.Count; i++)
            {
                VehicleManagerBase currentVehicle = vehicles[i];
                if (currentVehicle == null || currentVehicle.GetVehicleHealth().IsDead()) continue;

                vehicleList.Add(currentVehicle);
            }
            return vehicleList;
        }

        public List<VehicleManagerBase> GetVehicles()
        {
            return vehicles;
        }

        public WaitForSeconds GetEndWait()
        {
            return endWait;
        }

    }
}
