using System;
using System.Collections;
using System.Collections.Generic;
using Pico.Avatar;
using UnityEngine;
using UnityEngine.XR;


public class MoveByThumbstickSample : MonoBehaviour
{
    public enum HandSide
    {
        LeftHand = 1,
        RightHand = 2
    }
    public float moveSpeed = 1.0f;
    
    public HandSide handside = HandSide.RightHand;

    protected InputFeatureUsage<Vector2> thunbStiskInputFeature;

    public Vector2 moveDir;

    // Start is called before the first frame update
    void Start()
    {
        thunbStiskInputFeature = CommonUsages.primary2DAxis;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        transform.localPosition += new Vector3(moveDir.x, 0, moveDir.y) * moveSpeed * Time.deltaTime;
        return;
#endif
        var xrNode = handside == HandSide.RightHand ? XRNode.RightHand : XRNode.LeftHand;
        
        Vector3 angle = Camera.main.transform.eulerAngles;
        Quaternion quat = Quaternion.Euler(0,angle.y,0);

        if (InputDevices.GetDeviceAtXRNode(xrNode).TryGetFeatureValue(thunbStiskInputFeature, out moveDir))
        {
            Vector3 deltaPos = new Vector3(moveDir.x,  0, moveDir.y) * moveSpeed * Time.deltaTime;
            deltaPos = quat * deltaPos;
            transform.localPosition += deltaPos;
        }
    }
}
