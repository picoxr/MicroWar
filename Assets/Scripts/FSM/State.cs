using UnityEngine;

namespace MicroWar.FSM
{

    [CreateAssetMenu(menuName = "MicroWar/FSM/State")]
    public class State : ScriptableObject
    {
        public Action[] actions;
        public Transition[] transitions;
        public Color sceneGizmoColor = Color.grey;

        public virtual void UpdateState(StateController controller)
        {
            DoActions(controller);
        }

        private void DoActions(StateController controller)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Act(controller);
            }
        }
        
    }

}
