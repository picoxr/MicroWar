using System.Text;
using TMPro;
using UnityEngine;

namespace MicroWar.AI.Debug
{
    public class DebugUIVehicleAI : MonoBehaviour
    {
        public Transform debugCanvasTransform;
        public TMP_Text stateText;
        public TMP_Text debugText;

        public VehicleAIStateController stateController;

        private Vector3 initialScale = Vector3.zero;

#if !ENABLE_AI_DEBUGMODE
        private void Awake()
        {
            if(debugCanvasTransform != null) 
            {
                debugCanvasTransform.gameObject.SetActive(false);
            }
        }
#endif
#if ENABLE_AI_DEBUGMODE

        StringBuilder debugTextBuffer = new StringBuilder(200);

        private void Start()
        {
            debugCanvasTransform.gameObject.SetActive(true);
            initialScale = debugCanvasTransform.localScale;
        }

        private void Update()
        {
            Vector3 mainCamPos = Camera.main.transform.position;
            debugCanvasTransform.LookAt(mainCamPos);
            debugCanvasTransform.Rotate(Vector3.up, 180f);

            float distance = (debugCanvasTransform.position - mainCamPos).magnitude;

            if (distance > 1)
            {
                debugCanvasTransform.localScale = initialScale * (1 + (distance - 1f) * 0.5f);
            }
            else 
            {
                debugCanvasTransform.localScale = initialScale;
            }

            //Update the text based on events
            stateText.text = stateController.currentState.name;
            debugTextBuffer.Clear(); 
            debugTextBuffer.AppendFormat("DistToNext: {0}\nDistToDest: {1}\nAngleToNext: {2}", stateController.debugDistanceToSteeringTarget, stateController.debugDistanceToDestination, stateController.debugAngleToSteeringTarget);
            debugText.text = debugTextBuffer.ToString();
        }
#endif

    }
}

