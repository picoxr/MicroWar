using MicroWar.FSM;
using MicroWar.Multiplayer;
using UnityEngine;

namespace MicroWar.AI
{
    [CreateAssetMenu(menuName = "MicroWar/FSM/Vehicle/VehicleAIPowerUpAction")]
    public class VehicleAIPowerUpAction : MicroWar.FSM.Action
    {
        public override void Act(StateController stateController)
        {
            VehicleAIStateController vehicleStateController = (VehicleAIStateController)stateController;
            CrateContainerQueue crateContainer = vehicleStateController.VehicleManager.crateContainer;
            ITankHealth vehicleHealth;

            if (crateContainer == null || !crateContainer.HasCrate())
            {
                return;
            }

            vehicleHealth = vehicleStateController.VehicleManager.GetVehicleHealth();

            if (vehicleHealth.GetStartingHealth() > vehicleHealth.GetCurrentHealth())
            {
                crateContainer.GetActiveCrate()?.BreakCrate();
            }
        }

    }
}
