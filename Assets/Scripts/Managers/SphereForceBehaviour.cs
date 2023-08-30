using UnityEngine;
using MicroWar.Multiplayer;
namespace MicroWar
{
    public class SphereForceBehaviour : MonoBehaviour
    {
        public SphereCollider sphereCollider;
        private void OnEnable()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.enabled = true;
        }
        void OnTriggerEnter(Collider other )
        {
            if (other.gameObject.layer == GameManager.Instance.playerLayer)
            {
                other.gameObject.SetActive(false);
                sphereCollider.enabled = false;
                GameManager.Instance.VehicleIsDeployed = true;
                var tankType = (VehicleType)GameManager.Instance.Settings.PlayerVehicleType;
                MultiplayerBehaviour.Instance.PlayerReady(tankType);
                gameObject.SetActive(false);
            }
        }
    }
}
