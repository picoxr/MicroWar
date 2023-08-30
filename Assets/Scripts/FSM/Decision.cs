using UnityEngine;

namespace MicroWar.FSM
{ 

    public abstract class Decision : ScriptableObject
    {
        public abstract bool Decide(StateController controller);
    }

}
