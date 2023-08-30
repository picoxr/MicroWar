using MicroWar.FSM;
using UnityEngine;
using static MicroWar.AI.VehicleAIStateController;

namespace MicroWar.AI
{
    [CreateAssetMenu(menuName = "MicroWar/FSM/Vehicle/VehicleAIRangedWanderAction")]
    public class VehicleAIRangedWanderAction : MicroWar.FSM.Action
    {
        private const float rangeFromEnemy = 1f;

        public override void Act(StateController stateController)
        {
            VehicleAIStateController vehicleStateController = (VehicleAIStateController)stateController;

            if (!vehicleStateController.CanMove())
            {
                return;
            }

            AIMovementTargetPriority movementPriority = vehicleStateController.GetMovementTargetPriority();

            switch (movementPriority)
            {
                case AIMovementTargetPriority.None:
                case AIMovementTargetPriority.PowerUp:
                    if (vehicleStateController.activePowerUpTarget == null) return;
                    vehicleStateController.currentMoveTarget = vehicleStateController.activePowerUpTarget.transform.position;
                    break;
                case AIMovementTargetPriority.Enemy:
                    if (vehicleStateController.currentEnemyTarget == null) return;
                    vehicleStateController.currentMoveTarget = vehicleStateController.currentEnemyTarget.position + Vector3.ProjectOnPlane(Random.onUnitSphere * rangeFromEnemy, Vector3.up);
                    break;
                default:
                    return;
            }

            vehicleStateController.Move();
        }

    }
}
