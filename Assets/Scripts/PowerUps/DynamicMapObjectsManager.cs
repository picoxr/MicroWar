using MicroWar.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MicroWar
{

    public class DynamicMapObjectsManager : MonoBehaviour
    {
        private const int MAX_ACTIVE_POWER_UPS = 3;

        public TurretController[] Turrets;

        public AudioClip GenericPowerUpSFX;

        public GameObject TurretActivatorPrefab;
        public GameObject TurretLinkCurvePrefab;

        public GameObject CratePickupPrefab;
        public GameObject CrateContainerPrefab;

        [Header("PowerUp Settings")]
        public PowerUpSettings[] PowerUpSettings;

        private CrateContainerQueue[] crateContainers;

        private const float maxDistanceFromSource = 3f;
        private const float maxDistanceFromMapOrigin = 2.5f;

        private WaitForSeconds powerUpSpawnIdleTime = new WaitForSeconds(1f);

        private uint pickableObjectId = 0;
        private Dictionary<uint, PlayerPickableObject> pickableObjects;

        private IEnvironmentManager environmentManager;
        private GameManager gameManager;

        private void Awake()
        {
            pickableObjects = new Dictionary<uint, PlayerPickableObject>();
        }

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
            environmentManager = gameManager.EnvironmentManager;
            gameManager.currentSession.OnSessionStateChanged = OnGameSessionStateChange;
            //gameManager.OnGameSessionStateChange += OnGameSessionStateChange;
            InstantiateCrateContainers();
        }



        private void InstantiateCrateContainers()
        {
            Transform[] playerSpawnPoints = environmentManager.GetPlayerSpawnPoints();
            crateContainers = new CrateContainerQueue[playerSpawnPoints.Length];
            for (int i = 0; i < playerSpawnPoints.Length; i++)
            {
                CrateContainerQueue container = crateContainers[i] = Instantiate(CrateContainerPrefab, null).GetComponent<CrateContainerQueue>();
                Transform playerSpawnTransform = playerSpawnPoints[i];
                container.gameObject.transform.SetPositionAndRotation(playerSpawnTransform.position + playerSpawnTransform.forward * 0.1f, playerSpawnTransform.rotation);// Position the container in front of the player.
            }
        }

        private uint GeneratePickableObjectId()
        {
            return pickableObjectId++;
        }

        private bool RegisterPickableObject(PlayerPickableObject pickableObject)
        {
            pickableObject.Id = GeneratePickableObjectId();
            return pickableObjects.TryAdd(pickableObject.Id, pickableObject);
        }

        private bool DeregisterPickableObject(uint pickableObjectId)
        {
            return pickableObjects.Remove(pickableObjectId);
        }

        public int GetActivePowerUpCount()
        {
            return pickableObjects.Count;
        }

        private void OnGameSessionStateChange(SessionState sessionState)
        {
            switch (sessionState)
            {
                case SessionState.RoundStarting:
                    break;
                case SessionState.RoundPlaying:
                    SpawnPowerUps();
                    break;
                case SessionState.RoundEnding:
                    StopAllCoroutines();
                    break;
                default:
                    break;
            }
        }

        private void SpawnPowerUps()
        {
            if (PowerUpSettings == null || PowerUpSettings.Length == 0) return;

            for (int i = 0; i < PowerUpSettings.Length; i++)
            {
                PowerUpSettings currSetting = PowerUpSettings[i];
                StartCoroutine(TrySpawnPowerUp(currSetting));
            }
        }

        private IEnumerator TrySpawnPowerUp(PowerUpSettings currSetting)
        {
            if (currSetting.powerUpType == PowerUpType.None) yield break;

            WaitForSeconds periodWait = new WaitForSeconds(currSetting.period);
            WaitForSeconds initialRandomWait = new WaitForSeconds(Random.Range(0f, currSetting.period));

            yield return initialRandomWait;

            while (true)
            {
                //Don't spawn any power-ups if there is already max number of power-ups on the map.
                if (GetActivePowerUpCount() >= MAX_ACTIVE_POWER_UPS)
                {
                    yield return powerUpSpawnIdleTime;
                    continue;
                }

                if (Random.value <= currSetting.probability)
                {
                    switch (currSetting.powerUpType)
                    {
                        case PowerUpType.Health:
                            SpawnCrate(CrateType.Health);
                            break;
                        case PowerUpType.Shield:
                            SpawnCrate(CrateType.Shield);
                            break;
                        case PowerUpType.TurretActivator:
                            SpawnTurretActivator();
                            break;
                    }
                }

                yield return periodWait;
            }
        }

        public void SpawnTurretActivator()
        {
            TurretController turret = GetRandomTurret();

            //Find a random position on the navmesh for the location of the turret activator
            Vector3 sourcePoint = Random.insideUnitSphere + turret.transform.position + turret.transform.forward;
            Vector3 randomPoint = environmentManager.GetRandomPointOnMap(sourcePoint, maxDistanceFromSource);

            if (randomPoint != sourcePoint)
            {
                GameObject turretActivator = Instantiate(TurretActivatorPrefab, randomPoint, Quaternion.identity);
                GameObject turretLinkCurve = Instantiate(TurretLinkCurvePrefab, Vector3.zero, Quaternion.identity);

                PlayerPickableObject pickableObject = turretActivator.GetComponent<PlayerPickableObject>();
                RegisterPickableObject(pickableObject);

                //Draw a connector link between the turret and it's activator object on the map.
                CurveUtils.DrawCurveBetween(turretActivator, turret.gameObject, 20, turretLinkCurve.GetComponent<LineRenderer>());

                pickableObject.OnPickedUp = (playerPicked) =>
                    {
                        //Play pick-up SFX
                        SoundManager.Instance.PlayPowerUpSFX(GenericPowerUpSFX);
                        //Deactive turret activator
                        turretActivator.SetActive(false);
                        turretLinkCurve.SetActive(false);
                        //Get the alive enemies of the player who has just picked up this turret activator
                        List<Transform> targetTransforms = GetTurretTargets(playerPicked);
                        //Fire!
                        turret.FireMultiple(targetTransforms);

                        DeregisterPickableObject(pickableObject.Id);

                        //TODO: Create object pools for PowerUps
                        Destroy(turretActivator, 2f);
                        Destroy(turretLinkCurve, 2f);
                    };
            }
        }

        private List<Transform> GetTurretTargets(GameObject playerPicked)
        {
            TankHealth vehicleHealth = playerPicked.GetComponent<TankHealth>();

            return gameManager.currentSession.GetAliveEnemyTransformsOf(vehicleHealth.VehicleManagerBase);
        }

        public void SpawnCrate(CrateType crateType)
        {
            Vector3 randomPoint = environmentManager.GetRandomPointOnMap(maxDistanceFromMapOrigin, maxDistanceFromSource);

            GameObject cratePickup = Instantiate(CratePickupPrefab, randomPoint, Quaternion.identity);
            PlayerPickableObject pickableObject = cratePickup.GetComponent<PlayerPickableObject>();
            RegisterPickableObject(pickableObject);
            pickableObject.OnPickedUp = (playerPicked) =>
                {
                    SoundManager.Instance.PlayPowerUpSFX(GenericPowerUpSFX);
                    cratePickup.SetActive(false);

                    VehicleManagerBase vehicleManager = playerPicked.GetComponent<TankHealth>().VehicleManagerBase;
                    //Place the breakable crate in front of the player
                    int mapSpawnPointIndex = environmentManager.GetMapSpawnPointIndex(vehicleManager.m_SpawnPoint);
                    crateContainers[mapSpawnPointIndex].Enqueue(crateType, vehicleManager.m_Instance.transform);
                    
                    //Make sure the vehicle has an assigned crate container so that AI can access it too.
                    if (vehicleManager.crateContainer == null)
                    { 
                        vehicleManager.crateContainer = crateContainers[mapSpawnPointIndex];
                    }

                    DeregisterPickableObject(pickableObject.Id);
                    Destroy(cratePickup, 2f);
                };
        }

        public void FireAllTurrets()
        {
            List<Transform> targetTransforms = gameManager.currentSession.GetAliveEnemyTransformsOf(gameManager.Vehicles[0]);

            for (int i = 0; i < Turrets.Length; i++)
            {
                Turrets[i].FireMultiple(targetTransforms);
            }
        }

        private TurretController GetRandomTurret()
        {
            return Turrets[Random.Range(0, Turrets.Length)];
        }

        private void OnDestroy()
        {
            //gameManager.OnGameSessionStateChange -= OnGameSessionStateChange;
        }
    }

}