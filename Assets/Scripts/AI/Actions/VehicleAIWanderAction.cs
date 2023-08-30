using MicroWar.FSM;
using UnityEngine;
using static MicroWar.AI.VehicleAIStateController;

namespace MicroWar.AI
{
    [CreateAssetMenu(menuName = "MicroWar/FSM/Vehicle/VehicleAIWanderAction")]
    public class VehicleAIWanderAction : MicroWar.FSM.Action
    {
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
                    vehicleStateController.currentMoveTarget = vehicleStateController.currentEnemyTarget.position;
                    break;
                default:
                    return;
            }

            vehicleStateController.Move();
        }
    }
}
