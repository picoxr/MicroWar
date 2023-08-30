using System.Collections.Generic;
using UnityEngine;
using Pico.Platform.Models;

namespace MicroWar.Platform
{
    public class NetcodeRoomData
    {
        public ulong HostUId;
        public string HostPId;
        public Room currentRoom;
        public Dictionary<ulong, string> ClientId2PIdMap;
        private Dictionary<ulong, string> UId2PIdMap;
        private Dictionary<string, ulong> PId2UIdMap;

        public NetcodeRoomData()
        {
            UId2PIdMap = new Dictionary<ulong, string>();
            PId2UIdMap = new Dictionary<string, ulong>();
            ClientId2PIdMap = new Dictionary<ulong, string>();
        }
        public void UpdateRoomData(Room PICORoomData)
        {
            if (PICORoomData == null)
                return;

            if (null != PICORoomData.OwnerOptional)
            {
                HostPId = PICORoomData.OwnerOptional.ID;
                HostUId = (ulong)PICORoomData.OwnerOptional.ID.GetHashCode();
            }

            if (null != PICORoomData.UsersOptional)
            {
                UpdateIDMaps(PICORoomData.UsersOptional);
            }

            currentRoom = PICORoomData;
        }

        private void UpdateIDMaps(UserList currentRoomUsers)
        {
            if (currentRoomUsers.Count == 0)
                return;
            PId2UIdMap.Clear();
            UId2PIdMap.Clear();
            foreach (var user in currentRoomUsers)
            {
                var userPID = user.ID;
                var hashCode = (ulong)user.ID.GetHashCode();
                PId2UIdMap.Add(userPID, hashCode);
                UId2PIdMap.Add(hashCode, user.ID);
                Debug.Log($"Pid:{user.ID} -> Uid:{hashCode}");
            }
        }
        public bool ParseUID2PID(ulong UID, out string PID)
        {
            if (UId2PIdMap.TryGetValue(UID, out PID))
            {
                return true;
            }
            else
            {
                Debug.LogWarning($"[{nameof(PlatformController_Network)}]: UID To PID Unknown User!");
                return false;
            }
        }
        public bool ParsePID2UID(string PID, out ulong UID)
        {
            if (PId2UIdMap.TryGetValue(PID, out UID))
            {
                return true;
            }
            else
            {
                Debug.LogWarning($"[{nameof(PlatformController_Network)}]: PID to UID Unknown User!");
                return false;
            }
        }

        internal void Clear()
        {
            HostUId = default;
            HostPId = default;
            UId2PIdMap.Clear();
            PId2UIdMap.Clear();
            ClientId2PIdMap.Clear();
        }
    }
}

