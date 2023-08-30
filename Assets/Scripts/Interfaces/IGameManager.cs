using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MicroWar
{
    public interface IGameManager
    {
        public List<VehicleManagerBase> GetAliveEnemiesOf(VehicleManagerBase tank);
        public int GetActivePowerUpCount();
    }
}

