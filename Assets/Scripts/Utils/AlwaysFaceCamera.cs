
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        //Vector3 euler = Quaternion.LookRotation(cam.transform.forward).eulerAngles;
        Vector3 euler = Quaternion.LookRotation(this.transform.position - cam.transform.position).eulerAngles;
        this.transform.rotation = Quaternion.Euler(this.transform.rotation.x, euler.y,this.transform.rotation.z);
    }
}
