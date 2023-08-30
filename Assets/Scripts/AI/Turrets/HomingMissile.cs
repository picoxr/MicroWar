using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    private Vector3 targetPosition = Vector3.positiveInfinity;
    private Transform targetTransform = null;

    public float speed = 2f; // The speed at which the missile moves
    public float rotationSpeed = 5f; // The speed at which the missile turns towards the target
    public float homingActivationDelay = 0.25f;

    private Rigidbody rigidBody;
    private bool isHomingActive = false;
    private bool isTargetSet = false;
    private bool isDynamicTarget = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        // Activate homing functionality after 1 second
        Invoke(nameof(ActivateHoming), homingActivationDelay);
    }

    public void SetTarget(Vector3 position)
    {
        targetPosition = position;
        isDynamicTarget = false;
        isTargetSet = true;
    }

    public void SetTarget(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
        isTargetSet = true;
        isDynamicTarget = true;
    }

    private void ActivateHoming()
    {
        isHomingActive = true;
    }

    private void FixedUpdate()
    {
        if (!isHomingActive || !isTargetSet)
            return;

        if (isDynamicTarget && targetTransform!=null)
        {
            targetPosition = targetTransform.position;
        }

        //TODO: If target is not active or destroyed attack a random area

        if (!rigidBody.isKinematic) rigidBody.isKinematic = true;
        // Calculate direction from the missile to the target
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Rotate the missile smoothly towards the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, lookRotation, rotationSpeed * Time.fixedDeltaTime));

        // Move the missile in the direction of the target
        rigidBody.MovePosition(rigidBody.position + transform.forward * speed * Time.fixedDeltaTime);
    }
}
