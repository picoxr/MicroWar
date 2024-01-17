using UnityEngine;


public class UIAlwaysFaceCamera : MonoBehaviour
{
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        transform.LookAt(cameraTransform.position);
    }
}
