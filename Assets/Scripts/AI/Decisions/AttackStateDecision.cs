using MicroWar.FSM;
using UnityEngine;

namespace MicroWar.AI
{
    [CreateAssetMenu(menuName = "MicroWar/FSM/Vehicle/AttackStateDecision")]
    public class AttackStateDecision : Decision
    {
        public override bool Decide(StateController stateController)
        {
            VehicleAIStateController vehicleStateController = (VehicleAIStateController) stateController;

            if (vehicleStateController.stateTimeElapsed < vehicleStateController.attackStateChangeCurrentPeriod)
            { 
                return false; 
            }

            //if (Random.value > 0.5)
            //{
            //    //Simple randomization; Stay in the current state for an additional period.
            //    tankStateController.attackStateChangeCurrentPeriod += TankAIStateController.attackStateChangeInitialPeriod;
            //    return false;
            //}

            //Change state
            return true;
        }
    }
}
