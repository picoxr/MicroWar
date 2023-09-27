using MicroWar.Utils;
using UnityEngine;

namespace MicroWar
{
    public class TankSelectorBehaviour : MonoBehaviour
    {
        public Transform holder;
        public GameObject infoCanvas;
        public VehicleType m_Type;
        private Vector3 spawnPointPosition = new Vector3(float.NaN, float.NaN);
        public LineRenderer curve;
        private bool isCurveEnabled = false;

        public void SelectEntered()
        {
            GetSpawnPoint();
            Debug.Log(gameObject.name + ": SelectEntered");
            GameManager.Instance.SetVehicleType(m_Type);
            EnableVehicleToSpawnPointCurve();
            //infoCanvas.SetActive(true);
        }
        public void SelectExited()
        {
            DisableVehicleToSpawnPointCurve();
            Debug.Log(gameObject.name + ": SelectExited");
            ResetTank();
        }

        public void ResetTank()
        {
            //infoCanvas.SetActive(false);
            if (holder != null)
                transform.parent = holder;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void EnableVehicleToSpawnPointCurve()
        {
            isCurveEnabled = true;
            curve.enabled = true;
        }
        private void DisableVehicleToSpawnPointCurve()
        {
            isCurveEnabled = false;
            curve.enabled = false;
        }


        private void GetSpawnPoint()
        {
            if (float.IsNaN(spawnPointPosition.x))
            {
                spawnPointPosition = GameManager.Instance.GetCurrentMapSpawnPoint();
            }
        }

        private void Update()
        {
            //Draw Curve between the vehicle and the Spawn Point
            if(isCurveEnabled) 
            {
                CurveUtils.DrawCurveBetween(transform.position, spawnPointPosition, 20, curve);
            }
        }

    }
}
