using UnityEngine;

namespace MicroWar
{
    [CreateAssetMenu(menuName = "MicroWar/GameManagerSettings")]
    public class GameManagerSettings : ScriptableObject
    {
        public int NumRoundsToWin = 3;            
        public float VehicleSelectDelay = 20f;
        public float StartDelay = 3f;             
        public float EndDelay = 3f;
        public VehicleType PlayerVehicleType = VehicleType.Tiger;
        public float BaseMovementSpeed = 8;
        public float BaseTurnSpeed = 180;
        public VehicleControlType PlayerTankControlType = VehicleControlType.VideoGameControl;
    }
}
