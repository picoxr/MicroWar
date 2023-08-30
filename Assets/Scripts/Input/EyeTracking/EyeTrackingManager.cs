using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;
using TMPro;

public class EyeTrackingManager : MonoBehaviour
{
    public Transform Origin;
    public GameObject SpotLight;
    
    private Vector3 combineEyeGazeVector;
    private Vector3 combineEyeGazeOriginOffset;
    private Vector3 combineEyeGazeOrigin;
    private Matrix4x4 headPoseMatrix;
    private Matrix4x4 originPoseMatrix;

    private Vector3 combineEyeGazeVectorInWorldSpace;
    private Vector3 combineEyeGazeOriginInWorldSpace;

    private Vector2 primary2DAxis;

    private RaycastHit hitinfo;

    private Transform selectedObj;

    private bool wasPressed;

    private bool hasEyeTracking = false;
    void Start()
    {
        combineEyeGazeOriginOffset = Vector3.zero;
        combineEyeGazeVector = Vector3.zero;
        combineEyeGazeOrigin = Vector3.zero;
        originPoseMatrix = Origin.localToWorldMatrix;

        if (PXR_EyeTracking.GetCombinedEyePoseStatus(out uint status))
        {
            hasEyeTracking = true;
        }
    }

    void Update()
    {
        if (hasEyeTracking)
        {

            //Offest Adjustment
            if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis))
            {

                combineEyeGazeOriginOffset.x += primary2DAxis.x * 0.001f;
                combineEyeGazeOriginOffset.y += primary2DAxis.y * 0.001f;

            }

            PXR_EyeTracking.GetHeadPosMatrix(out headPoseMatrix);

            bool isSuccess = PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector);
            if (!isSuccess) return;

            PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin);
            //Translate Eye Gaze point and vector to world space
            combineEyeGazeOrigin += combineEyeGazeOriginOffset;
            combineEyeGazeOriginInWorldSpace = originPoseMatrix.MultiplyPoint(headPoseMatrix.MultiplyPoint(combineEyeGazeOrigin));
            combineEyeGazeVectorInWorldSpace = originPoseMatrix.MultiplyVector(headPoseMatrix.MultiplyVector(combineEyeGazeVector));

            if (SpotLight != null)
            {
                SpotLight.transform.position = combineEyeGazeOriginInWorldSpace;
                SpotLight.transform.rotation = Quaternion.LookRotation(combineEyeGazeVectorInWorldSpace, Vector3.up);
            }

            GazeTargetControl(combineEyeGazeOriginInWorldSpace, combineEyeGazeVectorInWorldSpace);
        }
        else
        {
            GazeTargetControl(Camera.main.transform.position, Camera.main.transform.forward);
        }
        
    }


    void GazeTargetControl(Vector3 origin,Vector3 vector)
    {
        if (Physics.SphereCast(origin,0.15f,vector,out hitinfo))
        {
            if (selectedObj != null && selectedObj != hitinfo.transform)
            {
                if(selectedObj.GetComponent<ETObject>()!=null)
                    selectedObj.GetComponent<ETObject>().UnFocused();
                selectedObj = null;
            }
            else if (selectedObj == null)
            {
                selectedObj = hitinfo.transform;
                if (selectedObj.GetComponent<ETObject>() != null)
                    selectedObj.GetComponent<ETObject>().IsFocused();
            }

        }
        else
        {
            if (selectedObj != null)
            {
               if (selectedObj.GetComponent<ETObject>() != null)
                    selectedObj.GetComponent<ETObject>().UnFocused();
                selectedObj = null;
            }
        }    
    }
}
