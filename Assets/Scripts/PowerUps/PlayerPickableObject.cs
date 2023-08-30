using System;
using UnityEngine;

namespace MicroWar
{
    public class PlayerPickableObject : MonoBehaviour
    {
        public Action<GameObject> OnPickedUp;
        public uint Id { get; set; }
        public bool IsPickedUp { get; private set; } = false;

        private void OnTriggerEnter(Collider other)
        {
            IsPickedUp = true;
            OnPickedUp?.Invoke(other.gameObject);
        }

        private void ResetStatus()
        {
            IsPickedUp = false;
        }


    }
}
