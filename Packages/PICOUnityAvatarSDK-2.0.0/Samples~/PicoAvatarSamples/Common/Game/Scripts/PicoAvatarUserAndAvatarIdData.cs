using System.Collections.Generic;
using UnityEngine;
using System;


namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            [CreateAssetMenu(fileName = "PicoAvatarUserAndAvatarIdData", menuName = "Pico/Demo/PicoAvatarUserAndAvatarIdData", order = 1)]
            public class PicoAvatarUserAndAvatarIdData : ScriptableObject
            {
                [Serializable]
                public class AvatarIdConfig {
                    public string userId = "";
                    public string avatarId = "";
                    public string characterType = "0";
                }
                
                public int getAvatarCount()
                {
                    if(avatarIdList != null)
                    {
                        return avatarIdList.Count;
                    }
                    return 0;
                }

                [SerializeField]
                public List<AvatarIdConfig> avatarIdList = new List<AvatarIdConfig>();
            }
        }
    }
}