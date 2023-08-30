
using MicroWar.Multiplayer;

namespace MicroWar.AI
{
    public class TankMovement_AI : MicroWar.TankMovement
    {
        private float moveAmount;
        private float turnAmount;
        private NetPlayer networkPlayerAI;
        private bool selfUpdate = true;
        private void Start()
        {
            networkPlayerAI = GetComponent<NetPlayer>();
            selfUpdate = networkPlayerAI.IsOwner; //AI tanks owns by the server, disable ai tank movement if it's a client dummy version.
        }
        private void Update()
        {
            // Store the value of both input axes.
            m_MovementInputValue = moveAmount;
            m_TurnInputValue = turnAmount;

            moveAmount = 0f;
            turnAmount = 0f;
        }

        public void SetMovementInput(float moveAmount)
        {
            this.moveAmount = moveAmount;
        }

        public void SetTurnInput(float turnAmount)
        {
            this.turnAmount = turnAmount;
        }
    }
}