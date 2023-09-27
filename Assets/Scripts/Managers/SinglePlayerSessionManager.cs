using MicroWar.AI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MicroWar
{
    public class SinglePlayerSessionManager : SessionManagerBase
    {
        public override void Initialize(GameManager gameManager)
        {
            base.Initialize(gameManager);
            SessionType = SessionType.SinglePlayer;
            for (int i = 0; i < gameManager.Vehicles.Count; i++) //Add vehicles
            {
                vehicles.Add( gameManager.Vehicles[i]);
            }
        }
        public override void StartSession()
        {
            inGameUIHandler.ShowMessageCanvas();
            // Create the delays so they only have to be made once.
            //m_TankSelectWait = new WaitForSeconds(m_TankSelectDelay);
            StartCoroutine(SessionStarting());
        }

        protected override void SpawnAllVehicles()//Abstract
        {
            for (int i = 0; i < gameManager.Vehicles.Count; i++)
            {
                SinglePlayerVehicleManager vehicleManager = gameManager.Vehicles[i];
                VehiclePlayerType vehiclePlayerType = vehicleManager.m_PlayerType;
                if (vehicleManager.m_PlayerType == VehiclePlayerType.Local)
                {
                    vehicleManager.VehicleType = gameManager.Settings.PlayerVehicleType;
                }

                GameObject vehiclePrefab = gameManager.VehicleConfigManager.GetVehiclePrefab(vehicleManager.VehicleType, vehiclePlayerType);

                vehicleManager.m_Instance = Instantiate(vehiclePrefab, vehicleManager.m_SpawnPoint.position, vehicleManager.m_SpawnPoint.rotation, gameManager.BoardGameOrigin);
                vehicleManager.m_PlayerNumber = i + 1;
                vehicleManager.m_Instance.transform.localScale = Vector3.one;
                vehicleManager.Setup(gameManager.VehicleConfigManager.GetVehicleSettings(vehicleManager.VehicleType));

                vehicleManager.GetVehicleHealth().SetShieldPrefab(gameManager.shieldPrefab);

                if (vehicleManager.m_PlayerType == VehiclePlayerType.AI)
                {
                    VehicleAIStateController aiController = vehicleManager.m_Instance.GetComponent<VehicleAIStateController>();
                    aiController.SetGameManager(gameManager);
                    aiController.SetVehicleManager(vehicleManager);
                }
            }
        }

        protected override IEnumerator SessionLoop() //Abstract
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (sessionWinner != null)
            {
                GC.Collect();
                // If there is a game winner, restart the level.
                SceneManager.LoadScene("Main");
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine(SessionLoop());
            }
        }

        protected override IEnumerator SessionStarting()//Abstract
        {
            OnSessionStateChanged?.Invoke(SessionState.SessionStarting);
            gameManager.Sphereforce.SetActive(true);
            gameManager.VehicleSelector.SetActive(true);
            gameManager.LeftHandInteractor.enabled = true;
            gameManager.RightHandInteractor.enabled = true;
            
            string message = "Please grab your vehicle and place it in the sphere force field";
            message += "\n\n";
            float vehicleSelectDelay = gameManager.Settings.VehicleSelectDelay;
            while (vehicleSelectDelay > 0 && !gameManager.VehicleIsDeployed)
            {
                inGameUIHandler.UpdateMessage(message + vehicleSelectDelay.ToString("#"));
                vehicleSelectDelay -= Time.deltaTime;
                yield return null;
            }
            inGameUIHandler.UpdateMessage(string.Empty);
            SpawnAllVehicles();
            gameManager.Sphereforce.SetActive(false);
            gameManager.VehicleSelector.SetActive(false);
            gameManager.LeftHandInteractor.enabled = false;
            gameManager.RightHandInteractor.enabled = false;
            gameManager.LefthandLaser.enabled = false;
            gameManager.RighthandLaser.enabled = false;
            StartCoroutine(SessionLoop());
        }

        protected override IEnumerator RoundStarting()// Abstract
        {
            ResetAllVehicles();
            DisableVehicleControl();


            // Increment the round number and display text showing the players what round it is.
            RoundNumber++;
            inGameUIHandler.UpdateMessage("ROUND " + RoundNumber);

            OnSessionStateChanged?.Invoke(SessionState.RoundStarting);

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return startWait;
        }

        protected override IEnumerator RoundPlaying()// Abstract
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableVehicleControl();

            // Clear the text from the screen.
            inGameUIHandler.UpdateMessage(string.Empty);

            OnSessionStateChanged?.Invoke(SessionState.RoundPlaying);

            while (!OnePlayerLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        }

        protected override IEnumerator RoundEnding()// Abstract
        {
            // Stop tanks from moving.
            DisableVehicleControl();

            // Clear the winner from the previous round.
            roundWinner = null;

            // See if there is a winner now the round is over.
            //gameManager.RoundWinner = GetRoundWinner();
            roundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (roundWinner != null)
            {
                roundWinner.m_Wins++;
                if (roundWinner.m_PlayerType == VehiclePlayerType.Local)
                {
                    // Achievement : Win a round
                    AchievementManager.Instance.UpdateWinRoundsAchievement(roundWinner.m_userName);

                    // Achievement : Play a round
                    AchievementManager.Instance.UpdatePlayRoundAchievement();
                }
            }

            // Now the winner's score has been incremented, see if someone has one the game.
            sessionWinner = GetGameWinner();
            if (sessionWinner != null)
            {
                // Achievement : Win a match
                AchievementManager.Instance.UpdateWinMatchAchievement(roundWinner.m_userName);

                // Achievement : Play a match
                AchievementManager.Instance.UpdatePlayMatchAchievement();
            }


            inGameUIHandler.ShowEndMessage();

            OnSessionStateChanged?.Invoke(SessionState.RoundEnding);

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return endWait;
        }

        protected override void ResetAllVehicles()// Abstract
        {
            for (int i = 0; i < gameManager.Vehicles.Count; i++)
            {
                gameManager.Vehicles[i].Reset();
            }
        }


        public override void EnableVehicleControl()//Abstract
        {
            for (int i = 0; i < gameManager.Vehicles.Count; i++)
            {
                gameManager.Vehicles[i].EnableControl();
            }
        }


        public override void DisableVehicleControl()//Abstract
        {
            for (int i = 0; i < gameManager.Vehicles.Count; i++)
            {
                gameManager.Vehicles[i].DisableControl();
            }
        }


    }
}
