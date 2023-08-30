using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

public class UILookAtPlayer : MonoBehaviour
{
    private Transform playerTransform;
    public float angleOffset = 10f;

    private void Start()
    {
        // Find a reference to the player's transform
        playerTransform = Camera.main.transform;
    }

    private void Update()
    {
        // Calculate the direction towards the target on the Y axis
        Vector3 direction = playerTransform.position - transform.position;
        direction.x = 0f;
        direction.z = 0f;

        // Calculate the rotation towards the target on the Y axis
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        // Apply the angle offset
        Vector3 euler = targetRotation.eulerAngles;
        euler.y += angleOffset;
        targetRotation = Quaternion.Euler(euler);

        // Apply the rotation only on the Y axis
        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
    }
}
