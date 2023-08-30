using System;
using System.Collections.Generic;
using Pico.Platform.Models;

namespace MicroWar.Platform
{
    /// <summary>
    /// In Room data comes from PICO SDK
    /// </summary>
    public class PICOInRoomData
    {
        public Room currentRoom; //The room you are current in.
        public Dictionary<string, User> userDictionary; //PID/User
        public Dictionary<string, User> latestUserDictionary;

        public Action<User> OnPlayerJoinRoom;
        public Action<User> OnPlayerLeaveRoom;
        public PICOInRoomData()
        {
            userDictionary = new Dictionary<string, User>();
            latestUserDictionary = new Dictionary<string, User>();
        }
        /// <summary>
        /// set in room data when first join a room
        /// </summary>
        /// <param name="room">current room</param>
        public void InitInRoomData(Room room)
        {
            currentRoom = room;
            ParsePlayerList(currentRoom, ref userDictionary);
        }
        /// <summary>
        /// Update InRoomData when receive Room updates.
        /// </summary>
        /// <param name="room">current room</param>
        public void UpdateInRoomData(Room room)
        {
            currentRoom = room;
            ParsePlayerList(room, ref latestUserDictionary);
            if (latestUserDictionary == userDictionary)
                return;

            foreach (KeyValuePair<string, User> kvp in userDictionary)
            {
                if (!latestUserDictionary.ContainsKey(kvp.Key))//Player leave
                {
                    OnPlayerLeaveRoom?.Invoke(kvp.Value);
                }
            }

            foreach (KeyValuePair<string, User> kvp in latestUserDictionary)
            {
                if (!userDictionary.ContainsKey(kvp.Key)) //new Player
                {
                    OnPlayerJoinRoom?.Invoke(kvp.Value);
                }
            }

            var temp = userDictionary;
            userDictionary = latestUserDictionary;
            latestUserDictionary = temp;
            latestUserDictionary.Clear();
        }

        public void Clear()
        {
            currentRoom = null;
            userDictionary.Clear();
            latestUserDictionary.Clear();
        }
        private bool ParsePlayerList(Room room, ref Dictionary<string, User> playerDictionary)
        {
            if (null != room && null != room.UsersOptional && room.UsersOptional.Count > 0)
            {
                playerDictionary.Clear();
                //Update the User dictionary
                foreach (User user in room.UsersOptional)
                {
                    playerDictionary.Add(user.ID, user);
                }
                return true;
            }
            else
            {
                //TODO: Error handler
                return false;
            }
        }
    }
}

