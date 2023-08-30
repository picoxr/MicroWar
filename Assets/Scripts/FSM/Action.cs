using UnityEngine;

namespace MicroWar.FSM
{ 

    public abstract class Action : ScriptableObject
    {
        public abstract void Act(StateController controller);
        public virtual void OnStateExit(StateController controller) { }
        public virtual void OnStateEnter(StateController controller) { }
    }
}
