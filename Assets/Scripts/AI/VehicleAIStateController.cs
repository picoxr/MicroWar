using MicroWar.FSM;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MicroWar.Multiplayer;
namespace MicroWar.AI 
{
    [RequireComponent(typeof(TankMovement_AI))]
    [RequireComponent(typeof(TankShooting_AI))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class VehicleAIStateController : StateController
    {
        public enum AIMovementTargetPriority
        { 
            None = 0,
            Enemy = 1,
            PowerUp = 2,
            Random = 3
        }

        //Const
        private const string PLAYER_PICKABLE_OBJECTS_LAYER = "PlayerPickableObjects";    //We keep objects that can be picked up by the player in this layer
        private const float TARGET_REFRESH_TIME = 1f;                                    //Determines how often AI should recalculate its path to an enemy.
        private const float ATTACK_STATE_CHANGE_INITIAL_PERIOD = 5f;                     //seconds
        private const float POWERUP_SEARCH_RADIUS = 2f;
        private const int MAX_POWERUP_SEARCH_RESULT = 3;
        private const float MIN_SQR_DISTANCE_TO_DESTINATION = 0.2f * MicroWar.GameManager.SQR_SCALE_FACTOR;

        public float attackStateChangeCurrentPeriod = ATTACK_STATE_CHANGE_INITIAL_PERIOD;

        private int playerPickableObjectsLayerMask = 0;

        [HideInInspector] public NavMeshAgent agent;
        [HideInInspector] public VehicleManagerBase VehicleManager;
        [HideInInspector] public GameManager gameManager;
        [HideInInspector] public SessionManagerBase currentSession;
        [HideInInspector] public TankMovement_AI moveController;
        [HideInInspector] public TankShooting_AI shootController;
        [HideInInspector] public Transform currentEnemyTarget = null;
        [HideInInspector] public Vector3? currentMoveTarget = null;
        private ITankHealth currentEnemyHealth = null;

        //For Future Use
        private ShotRecord lastShotRecord;

        //Attack
        [HideInInspector] public bool previousShootDecision = false;
        [HideInInspector] public float lastShootTime = 0f;

        //Movement Target
        [HideInInspector] public float lastTargetRefreshTime = 0f;

        //PowerUp Search
        [HideInInspector] public PlayerPickableObject activePowerUpTarget = null;
        private Collider[] powerUpSearchBuffer;
        private PlayerPickableObject[] powerUpBuffer;
        private float[] powerUpDistanceBuffer;

        //AI Start/Stop Flag
        private bool isStopped = false;

        #region DebugParams
        [HideInInspector] public GameObject debugPlayer;
        [HideInInspector] public Ray debugObjectInBetweenRay;
        [HideInInspector] public Vector3 debugLocation;
        [HideInInspector] public Color detectedColor = Color.grey;
        [HideInInspector] public Vector3 debugTarget;
        [HideInInspector] public string debugCastString;
        [HideInInspector] public bool debugPlayerIsWithinFOV = false;
        [HideInInspector] public float debugMovementAmount = 0f;
        [HideInInspector] public float debugDistanceToDestination = 0f;
        [HideInInspector] public float debugDistanceToSteeringTarget = 0f;
        [HideInInspector] public float debugAngleToSteeringTarget = 0f;
        #endregion

        private void Awake()
        {
            playerPickableObjectsLayerMask = LayerMask.GetMask(PLAYER_PICKABLE_OBJECTS_LAYER);
            powerUpSearchBuffer = new Collider[MAX_POWERUP_SEARCH_RESULT];
            powerUpBuffer = new PlayerPickableObject[MAX_POWERUP_SEARCH_RESULT];
            powerUpDistanceBuffer = new float[MAX_POWERUP_SEARCH_RESULT];
        }

        private void Start()
        {
            moveController = GetComponent<TankMovement_AI>();
            shootController = GetComponent<TankShooting_AI>();
            agent = GetComponent<NavMeshAgent>();

            //Enable manual control for the navmesh agent
            agent.updatePosition = false;
            agent.updateRotation = false;
            PickAnEnemy();
        }

        private void OnEnable()
        {
            shootController.OnShotHit += OnShotHit;
        }

        private void OnDisable()
        {
            shootController.OnShotHit -= OnShotHit;
        }

        protected override void Update()
        {
            if (!isStopped)
            {
                ScanPowerUps();
                PickAnEnemy();
                base.Update();
            }
        }

        private void ScanPowerUps()
        {
            activePowerUpTarget = null;
            if (gameManager.GetActivePowerUpCount() == 0)
            {
                return;
            }

            if (activePowerUpTarget == null || !activePowerUpTarget.gameObject.activeInHierarchy)
            {
                activePowerUpTarget = GetPowerUpNearby();
            }
        }

        protected override void OnExitState()
        {
            base.OnExitState();
        }

        public override void TransitionToState(FSM.State nextState)
        {
            base.TransitionToState(nextState);
        }

        public void SetVehicleManager(VehicleManagerBase manager)
        {
            VehicleManager = manager;
        }

        public void SetGameManager(GameManager manager)
        {
            gameManager = manager;
            currentSession = gameManager.currentSession;
        }

        public void Stop()
        {
            this.isStopped = true;
        }

        public void Resume()
        {
            this.isStopped = false;
        }

        public bool IsStopped() => isStopped;

        private void OnShotHit(ShotRecord shotRecord)
        {
            if (currentEnemyTarget == null)
            {
                return;
            }

            if (shotRecord.objectHit == currentEnemyTarget)
            {
                shotRecord.isHit = true;
            }

            lastShotRecord = shotRecord;
        }

        private void PickAnEnemy()
        {
            //TODO: If the there is an enemy closer than the target enemy, switch to the closer enemy
            if (currentEnemyTarget == null || currentEnemyHealth.IsDead())
            {
                List<VehicleManagerBase> aliveEnemies = currentSession.GetAliveEnemiesOf(this.VehicleManager);
                int enemyCount = aliveEnemies.Count;
                if (enemyCount != 0)
                {
                    VehicleManagerBase pickedEnemy = aliveEnemies[UnityEngine.Random.Range(0, enemyCount)]; //TODO: Add a decision-making parameter: Check if there is an active attacker. Or choose enemy based on distance.
                    currentEnemyTarget = pickedEnemy.m_Instance.transform;
                    currentEnemyHealth = pickedEnemy.GetVehicleHealth();
                }
            }
        }

        public bool CanMove()
        {
            return agent.isOnNavMesh && !agent.pathPending;
        }

        public void Move()
        {
            float timeSinceLastTargetRefresh = Time.realtimeSinceStartup - this.lastTargetRefreshTime;
            if (!agent.hasPath || timeSinceLastTargetRefresh > VehicleAIStateController.TARGET_REFRESH_TIME)
            {
                this.agent.destination = currentMoveTarget.Value;
                this.lastTargetRefreshTime = Time.realtimeSinceStartup;
            }
            else
            {
                //If very close to the destination assume that we have arrived.
                if ((agent.destination - agent.nextPosition).sqrMagnitude < VehicleAIStateController.MIN_SQR_DISTANCE_TO_DESTINATION)
                {
                    agent.nextPosition = agent.destination;
                    return;
                }

                Transform agentTransform = agent.transform;
                float movementAmount = 1f; //1 = max movement | 0 = no movement

                //Update the simulated agent's position so that it's always at the same positon as the actual bot
                agent.nextPosition = agent.transform.position;

                Vector3 agentForward = agentTransform.forward;
                Vector3 directionToNextCorner = agent.steeringTarget - agentTransform.position;

                float signedAngle = Vector3.SignedAngle(agentForward, directionToNextCorner, Vector3.up);
                float turnInput = Mathf.Sign(signedAngle); //1 = max turn | 0 = no turn | -1 = opposite max turn

                float turnStep = MicroWar.GameManager.Instance.Settings.BaseTurnSpeed * Time.fixedDeltaTime;
                if (turnStep > Mathf.Abs(signedAngle))
                {
                    turnInput /= turnStep;
                }

                movementAmount *= Mathf.Cos(Mathf.Deg2Rad * signedAngle);

                //Set Debug Params
                this.debugDistanceToDestination = Vector3.Distance(agent.transform.position, agent.destination);
                this.debugDistanceToSteeringTarget = Vector3.Distance(agent.transform.position, agent.steeringTarget);
                this.debugAngleToSteeringTarget = signedAngle;

                //Move and Turn
                moveController.SetMovementInput(movementAmount);
                moveController.SetTurnInput(turnInput);
            }
        }

        public AIMovementTargetPriority GetMovementTargetPriority() 
        {
            if (activePowerUpTarget == null)
            {
                return AIMovementTargetPriority.Enemy;
            }
            else 
            {
                return AIMovementTargetPriority.PowerUp;
            }
        }

        private PlayerPickableObject GetPowerUpNearby()
        {
            PlayerPickableObject chosenObject = null;

            Vector3 position = this.transform.position;

            int count = Physics.OverlapSphereNonAlloc(position, POWERUP_SEARCH_RADIUS, powerUpSearchBuffer, playerPickableObjectsLayerMask);
            if (count > 0) 
            {
                for (int i = 0; i < count; i++)
                {
                    GameObject currentPickable = powerUpSearchBuffer[i].gameObject;
                    powerUpBuffer[i] = currentPickable.GetComponent<PlayerPickableObject>();
                    powerUpDistanceBuffer[i] = Vector3.SqrMagnitude(currentPickable.transform.position - position);
                }

                //Get the closest one
                Array.Sort(powerUpDistanceBuffer, powerUpBuffer);
                chosenObject = powerUpBuffer[0];
            }

            return chosenObject;
        }

        private void OnDrawGizmos()
        {
            if (debugLocation != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(debugLocation, 0.1f);
            }

            if (agent != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(agent.destination, agent.destination + Vector3.up);

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(agent.pathEndPosition, 0.025f);

                Gizmos.color = Color.red;
                foreach (Vector3 corner in agent.path.corners)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(corner, new Vector3(0.025f,0.4f,0.025f));
                }

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(agent.nextPosition, 0.04f);

                Gizmos.color = Color.black;
                Gizmos.DrawLine(agent.steeringTarget, agent.transform.position);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(agent.transform.position, agent.transform.forward.normalized);

                Gizmos.color = Color.red;
                Gizmos.DrawCube(agent.steeringTarget, new Vector3(0.015f, 0.6f, 0.015f));

                Gizmos.color = Color.blue;
                Gizmos.DrawCube(agent.destination, new Vector3(0.025f, 0.5f, 0.025f));
            }

            //Tank State Gizmo
            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawCube(this.transform.position + Vector3.up * 1, Vector3.one * 0.1f);
        }

    }
}
