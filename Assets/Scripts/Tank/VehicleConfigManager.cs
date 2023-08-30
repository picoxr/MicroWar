using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroWar 
{
    public class VehicleConfigManager : MonoBehaviour
    {
        [Serializable]
        public struct VehiclePrefabGroup 
        {
            public VehicleType VehicleType;
            public GameObject PlayerPrefab;
            public GameObject AiPrefab;
            public GameObject MultiplayerPrefab;
        }

        public VehiclePrefabGroup[] VehiclePrefabGroups;
        public VehicleSettings[] VehicleSettings;

        private Dictionary<VehicleType, VehiclePrefabGroup> vehiclePrefabGroupsMap;
        private Dictionary<VehicleType, VehicleSettings> vehicleSettingsMap;

        private void Awake()
        {
            vehiclePrefabGroupsMap = new Dictionary<VehicleType, VehiclePrefabGroup>(VehiclePrefabGroups.Length);
            for (int i = 0; i < VehiclePrefabGroups.Length; i++)
            {
                VehiclePrefabGroup group = VehiclePrefabGroups[i];
                vehiclePrefabGroupsMap.Add(group.VehicleType, group);
            }

            vehicleSettingsMap = new Dictionary<VehicleType, VehicleSettings>(VehicleSettings.Length);
            for (int i = 0; i < VehicleSettings.Length; i++)
            {
                VehicleSettings settings = VehicleSettings[i];
                vehicleSettingsMap.Add(settings.VehicleType, settings);
            }
        }

        public GameObject GetVehiclePrefab(VehicleType vehicleType, VehiclePlayerType vehiclePlayerType)
        {
            GameObject prefab = null;
            VehiclePrefabGroup vehicleGroup = vehiclePrefabGroupsMap[vehicleType];
     
            switch (vehiclePlayerType)
            {
                case VehiclePlayerType.Local:
                    prefab = vehicleGroup.PlayerPrefab;
                    break;
                case VehiclePlayerType.AI:
                    prefab = vehicleGroup.AiPrefab;
                    break;
                case VehiclePlayerType.Network:
                    prefab = vehicleGroup.MultiplayerPrefab;
                    break;
            }
            
            if (prefab == null)
            {
                Debug.LogError($"Couldn't find a vehicle prefab for: vehicleType = {vehicleType}, vehiclePlayerType = {vehiclePlayerType}");
            }

            return prefab;
        }

        public VehicleSettings GetVehicleSettings(VehicleType vehicleType)
        {
            return vehicleSettingsMap[vehicleType];
        }
    }
}
