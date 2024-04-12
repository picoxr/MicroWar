using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;

namespace Pico.Avatar.Sample
{
    public class XRRotateController : MonoBehaviour
    {
        private PxrControllerTracking pxrControllerTrackingLeft = new PxrControllerTracking();
        private PxrControllerTracking pxrControllerTrackingRight = new PxrControllerTracking();
        private float[] headData = new float[7] { 0, 0, 0, 0, 0, 0, 0 };

        public float speed = 1.0f;
        public GameObject targetGo = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnGUI()
        {
            if (GUILayout.RepeatButton("-"))
            {
                //Debug.Log("左");    
                RotateAroundLocal(Vector3.up, speed);
            }
            if (GUILayout.RepeatButton("+"))
            {
                RotateAroundLocal(-Vector3.up, speed);
            }
        }
        // Update is called once per frame
        void Update()
        {
          //  PXR_Plugin.Controller.UPxr_GetControllerTrackingState((uint)PXR_Input.Controller.LeftController, 0, headData, ref pxrControllerTrackingLeft);
            PXR_Plugin.Controller.UPxr_GetControllerTrackingState((uint)PXR_Input.Controller.RightController, 0, headData, ref pxrControllerTrackingRight);
        //    uint leftControllerStatus = (uint)pxrControllerTrackingLeft.localControllerPose.status;
            uint rightControllerStatus = (uint)pxrControllerTrackingRight.localControllerPose.status;

          
            
            //判断控制器是否连接可以使用
            if (rightControllerStatus > 0)
            {
                Vector2 touchPosition;
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out touchPosition);


                float angle = VectorAngle(new Vector2(1, 0), touchPosition);

                //上    
                if (angle > 45 && angle < 135)
                {
                }
                //下      
                else if (angle < -45 && angle > -135)
                {
                }
                //左      
                else if ((angle < 180 && angle > 135) || (angle < -135 && angle > -180))
                {
                    //Debug.Log("左");
                    RotateAroundLocal(Vector3.up, speed);
                }
                //右      
                else if ((angle > 0 && angle < 45) || (angle > -45 && angle < 0))
                {
                    //Debug.Log("右");    
                   RotateAroundLocal(-Vector3.up, speed);
                }


                Debug.Log("触摸点位置" + touchPosition);

            }
            else
            {
                return;
            }
        }

        private void RotateAroundLocal(Vector3 dir, float angle)
        {
            if (targetGo == null)
                return;

            targetGo.transform.Rotate(dir, angle);
        }
        public void LinkTarget(GameObject go)
        {
            targetGo = go;
        }
        /// <summary>
        /// 根据在圆盘按下的位置，返回一个角度值  
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        float VectorAngle(Vector2 from, Vector2 to)
        {
            float angle;
            Vector3 cross = Vector3.Cross(from, to);
            angle = Vector2.Angle(from, to);
            return cross.z > 0 ? angle : -angle;
        }

    }


}
