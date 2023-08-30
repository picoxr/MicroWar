using MicroWar;
using MicroWar.Extensions;
using MicroWar.FSM;
using UnityEngine;
using UnityEngine.AI;


namespace MicroWar.AI
{
    [CreateAssetMenu(menuName = "MicroWar/FSM/Vehicle/VehicleAIAttackAction")]
    public class VehicleAIAttackAction : MicroWar.FSM.Action
    {
        private const float maxRange = 1.05f;
        private const float maxRangeSquared = maxRange * maxRange;
        private const float cooldownBeforeNext = 0.5f; //Max 2 shots per second - TODO: We need to limit this for the player tanks as well.

        private RaycastHit[] shootingRaycastResults = new RaycastHit[3];

        public override void Act(StateController stateController)
        {

            VehicleAIStateController VehicleStateController = (VehicleAIStateController)stateController;

            if (Time.realtimeSinceStartup - VehicleStateController.lastShootTime < cooldownBeforeNext)
            {
                return;
            }

            NavMeshAgent agent = VehicleStateController.agent;
            TankShooting_AI shootController = VehicleStateController.shootController;
            float distanceToTargetSquared = (VehicleStateController.currentEnemyTarget.position - agent.transform.position).sqrMagnitude;

            if (distanceToTargetSquared < maxRangeSquared)
            {
                //Set target
                shootController.SetTarget(VehicleStateController.currentEnemyTarget.position);

                if (!VehicleStateController.previousShootDecision)
                {
                    shootController.AimAtTheTarget();
                    bool canHit = GetShootingDistance(VehicleStateController, out float distance);
                    if (canHit)
                    {
                        shootController.Shoot(EstimateRequiredLaunchForce(shootController, distance));
                        VehicleStateController.lastShootTime = Time.realtimeSinceStartup;
                        VehicleStateController.previousShootDecision = true;
                    }
                }
                else
                {
                    VehicleStateController.previousShootDecision = false;
                }
            }
            else 
            {
                VehicleStateController.previousShootDecision = false;
                shootController.ResetTarget();
            }
        }

        /// <summary>
        /// Returns false if the gun trajectory is blocked by a non-enemy object and distance is set to -1.
        /// Returns true if there is a clear path to the enemy target and returns a valid distance to the target.
        /// </summary>
        /// <param name="tankController"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private bool GetShootingDistance(VehicleAIStateController tankController, out float distance)
        {
            bool canHitEnemy = false;
            distance = -1;

            Vector3 gunPosition = tankController.shootController.m_Turret.position;
            Vector3 gunForward = tankController.shootController.m_Turret.forward;
            float raycastDistance = Vector3.Distance(gunPosition, tankController.currentEnemyTarget.position);
            int hitCount = Physics.RaycastNonAlloc(gunPosition, gunForward, shootingRaycastResults, raycastDistance);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit rayCastResult = shootingRaycastResults[i];
                GameObject hitObject = rayCastResult.collider.gameObject;

                if (hitObject.layer != MicroWar.GameManager.Instance.playerLayer)
                {
                    break;//We have a blocking object so we can't hit an enemy
                }

                if (hitObject == tankController.gameObject)
                {
                    continue;//Raycast hit the own player, so keep checking other results
                }
                else 
                {
                    canHitEnemy = true;
                    distance = rayCastResult.distance;
                    break;
                }
            }

            return canHitEnemy;
        }

        private float EstimateRequiredLaunchForce(TankShooting_AI shootController, float distanceToTarget)
        {
            return distanceToTarget.RemapClamped(shootController.MinShootingDistance, shootController.MaxShootingDistance, shootController.m_MinLaunchForce, shootController.m_MaxLaunchForce);
        }

    }
}
