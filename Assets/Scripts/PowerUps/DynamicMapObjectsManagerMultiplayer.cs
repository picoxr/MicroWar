using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
namespace MicroWar
{

    public class DynamicMapObjectsManagerMultiplayer : NetworkBehaviour
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

        public CrateContainerQueue[] crateContainers;

        private const float maxDistanceFromTheSource = 3f;
        private const float maxDistanceFromMapOrigin = 2.5f;

        private WaitForSeconds powerUpSpawnIdleTime = new WaitForSeconds(1f);

        private uint pickableObjectId = 0;
        private Dictionary<uint, PlayerPickableObject> pickableObjects;

        private EnvironmentManager environmentManager;
        //private IEnvironmentManager environmentManager;

        private void Awake()
        {
            pickableObjects = new Dictionary<uint, PlayerPickableObject>();
        }

        public void Initialize(GameManager gameManager)
        {
            environmentManager = gameManager.EnvironmentManager;
            gameManager.currentSession.OnSessionStateChanged = GameManagerOnGameStateChanged;
        }

        private void Start()
        {
            //MultiplayerManager.OnGameStateChanged += GameManagerOnGameStateChanged;
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

        private void GameManagerOnGameStateChanged(SessionState sessionState)
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
            Vector3 randomPoint = environmentManager.GetRandomPointOnMap(sourcePoint, maxDistanceFromTheSource);

            if (randomPoint != sourcePoint)
            {
                GameObject turretActivator = Instantiate(TurretActivatorPrefab, randomPoint, Quaternion.identity);
                NetworkObject turretActivatorNetworkObject = turretActivator.GetComponent<NetworkObject>();
                TurretActivatorMultiplayer turretActivatorMultiplayer = turretActivator.GetComponent<TurretActivatorMultiplayer>();
                turretActivatorNetworkObject.Spawn();
                turretActivatorMultiplayer.DrawCurveClientRpc(turret.transform.position); //Send RPC to tell instantiated turret to draw the activator line.
                PlayerPickableObject pickableObject = turretActivator.GetComponent<PlayerPickableObject>();
                RegisterPickableObject(pickableObject);
                pickableObject.OnPickedUp = (playerPicked) =>
                    {

                        SoundManager.Instance.PlayPowerUpSFX(GenericPowerUpSFX);
                        turretActivatorNetworkObject.Despawn();
                        if (!IsServer)
                            return;
                        turretActivator.SetActive(false);
                        List<Transform> targetTransforms = GameManager.Instance.currentSession.GetAliveEnemyTransformsOf(playerPicked.GetComponent<TankHealth>().VehicleManagerBase);
                        turret.FireMultiple(targetTransforms);
                        DeregisterPickableObject(pickableObject.Id);
                        //TODO: Create object pools for PowerUps
                        Destroy(turretActivator, 2f);
                      //  Destroy(turretLinkCurve, 2f);
                    };
            }
        }

        public void SpawnCrate(CrateType crateType)
        {
            Vector3 randomPoint = environmentManager.GetRandomPointOnMap(maxDistanceFromMapOrigin, maxDistanceFromTheSource);

            GameObject cratePickup = Instantiate(CratePickupPrefab, randomPoint, Quaternion.identity);
            NetworkObject crateNetworkObject = cratePickup.GetComponent<NetworkObject>();
            crateNetworkObject.Spawn();
            PlayerPickableObject pickableObject = cratePickup.GetComponent<PlayerPickableObject>();
            RegisterPickableObject(pickableObject);
            pickableObject.OnPickedUp = (playerPicked) =>
                {
                    SoundManager.Instance.PlayPowerUpSFX(GenericPowerUpSFX);
                    cratePickup.SetActive(false);
                    if (!IsHost)
                        return;

                    VehicleManagerBase tankManager = playerPicked.GetComponent<TankHealth>().VehicleManagerBase;
                    //Place the breakable crate in front of the user
                    int mapSpawnPointIndex = environmentManager.GetMapSpawnPointIndex(tankManager.m_SpawnPoint);
                    crateContainers[mapSpawnPointIndex].Enqueue(crateType, tankManager.m_Instance.transform);
                    if(tankManager.crateContainer == null)
                    {
                        tankManager.crateContainer = crateContainers[mapSpawnPointIndex];
                    }

                    DeregisterPickableObject(pickableObject.Id);
                    crateNetworkObject.Despawn();
                    //Destroy(cratePickup, 2f);
                };
        }

        public CrateContainerQueue GetCrateContainer(int mapSpawnPointIndex)
        {
            if (mapSpawnPointIndex < crateContainers.Length)
                return crateContainers[mapSpawnPointIndex];
            else
                return null;
        }

        private TurretController GetRandomTurret()
        {
            return Turrets[Random.Range(0, Turrets.Length)];
        }

        private void OnDestroy()
        {
            //MultiplayerManager.OnGameStateChanged -= GameManagerOnGameStateChanged;
        }
    }

}