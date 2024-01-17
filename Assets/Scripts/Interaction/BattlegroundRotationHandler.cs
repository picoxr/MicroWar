using UnityEngine;
using UnityEngine.InputSystem;

public class BattlegroundRotationHandler : MonoBehaviour
{
    public InputActionReference leftHandPosition; 
    public InputActionReference rightHandPosition;

    public InputActionReference rotateGripLeft;
    public InputActionReference rotateGripRight;

    private bool isModifying = false;
    private Quaternion initialRotation = Quaternion.identity;
    private Vector3 initialVector = Vector3.zero;

    private Vector3 initialScale = Vector3.zero;
    private float initialSqrDistance = 0f;

    public float scaleModifierSpeed = 1f;

    public float maxScaleModifier = 1.2f;
    public float minScaleModifier = 0.4f;


    void Update()
    {

        if (rotateGripLeft.action.IsPressed() && rotateGripRight.action.IsPressed())
        {
            Vector3 left = leftHandPosition.action.ReadValue<Vector3>();
            Vector3 right = rightHandPosition.action.ReadValue<Vector3>();

            if (!isModifying)
            {
                //Rotation
                initialRotation = transform.rotation;
                isModifying = true;
                initialVector = right - left;
                initialVector.y = 0f;

                //Scale
                initialScale = transform.localScale;
                initialSqrDistance = (right - left).sqrMagnitude;
            }

            //Rotation
            Vector3 currentVector = right - left;
            currentVector.y = 0f;
            float angle = Vector3.SignedAngle(initialVector, currentVector, Vector3.up);
            transform.rotation = initialRotation;
            transform.Rotate(Vector3.up, angle);

            //Scale
            transform.localScale = initialScale * (scaleModifierSpeed * ((right - left).sqrMagnitude) / initialSqrDistance);
            transform.localScale = new Vector3(
                Mathf.Clamp(transform.localScale.x, minScaleModifier, maxScaleModifier),
                Mathf.Clamp(transform.localScale.y, minScaleModifier, maxScaleModifier),
                Mathf.Clamp(transform.localScale.z, minScaleModifier, maxScaleModifier));
        }
        else 
        {
            if (isModifying) isModifying = false;
        }

    }
}
