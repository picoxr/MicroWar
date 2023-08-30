using UnityEngine;

public class TankHighlightEffect : MonoBehaviour
{
    public Vector3 axis = Vector3.up;  // The axis around which the object will rotate
    public float rotationSpeed = 10f; // The speed of rotation

    public float minScale = 0.95f;     // The minimum scale value for the highlight effect
    public float maxScale = 1.05f;     // The maximum scale value for the highlight effect
    public float scaleSpeed = 0.5f;   // The speed of scaling for the highlight effect

    private void Update()
    {
        // Rotate the object around the specified axis
        transform.localRotation = Quaternion.AngleAxis(Time.time * rotationSpeed, axis);

        // Apply the highlight effect by scaling the object
        //float scale = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(Time.time * scaleSpeed, 1f));
        //transform.localScale = new Vector3(scale, scale, scale);
    }
}