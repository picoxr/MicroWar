using System.Collections.Generic;
using UnityEngine;

namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            [CreateAssetMenu(fileName = "PicoAvatarJsonSpecData", menuName = "Pico/Demo/PicoAvatarJsonSpecData", order = 1)]
            public class PicoAvatarJsonSpecData : ScriptableObject
            {
                [SerializeField]
                public List<string> jsonSpecList = new List<string>();
            }
        }
    }
}
        