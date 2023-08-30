using UnityEngine;

namespace MicroWar.FSM
{

    public abstract class StateController : MonoBehaviour
    {
        public State currentState;
        public State remainState;

        [HideInInspector] public float stateTimeElapsed = 0f;
        private float stateStartRealTime;

        private void Start()
        {
            stateStartRealTime = Time.realtimeSinceStartup;    
        }

        protected virtual void Update()
        {
            stateTimeElapsed += Time.deltaTime;
            currentState.UpdateState(this);
            CheckTransitions();
        }

        private void CheckTransitions()
        {
            for (int i = 0; i < currentState.transitions.Length; i++)
            {
                bool decisionSucceeded = currentState.transitions[i].decision.Decide(this);

                if (decisionSucceeded)
                {
                    TransitionToState(currentState.transitions[i].trueState);
                }
                else
                {
                    TransitionToState(currentState.transitions[i].falseState);
                }
            }
        }

        public virtual void TransitionToState(State nextState)
        {
            if (nextState != remainState)
            {
                DoOnStateExits(currentState);
                DoOnStateEnters(nextState);

                currentState = nextState;   
                OnExitState();
            }
        }

        private void DoOnStateEnters(State state)
        {
            for (int i = 0; i < state.actions.Length; i++)
            {
                state.actions[i].OnStateEnter(this);
            }
        }

        private void DoOnStateExits(State state) 
        {
            for (int i = 0; i < state.actions.Length; i++)
            {
                state.actions[i].OnStateExit(this);
            }
        }

        protected virtual void OnExitState()
        {
            stateTimeElapsed = 0;
            stateStartRealTime = Time.realtimeSinceStartup;
        }
    }
}
