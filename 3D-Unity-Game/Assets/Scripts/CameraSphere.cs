using UnityEngine;
using System.Collections;

public class CameraSphere : MonoBehaviour
{

    public Transform target;
    public Vector3 offset;
    public float sensitivity = 3; // чувствительность мышки
    public float limit = 80; // ограничение вращения по Y
    public float zoom = 0.25f; // чувствительность при увеличении, колесиком мышки
    public float zoomMax = 25; // макс. увеличение
    public float zoomMin = 3; // мин. увеличение
    private float X, Y;
    //TODO Cinemachine
    void Start()
    {
        limit = Mathf.Abs(limit);
        if (limit > 90) limit = 90;
        offset = new Vector3(offset.x, offset.y, -Mathf.Abs(zoomMax) / 2);
        transform.position = target.position + offset;
    }

    public void Update()
    {
        if (Camera.main.transform != transform)
            return;
        if (Input.GetAxis("Mouse ScrollWheel") > 0) offset.z += zoom; //TODO Unity Input system
        else if (Input.GetAxis("Mouse ScrollWheel") < 0) offset.z -= zoom;
        offset.z = Mathf.Clamp(offset.z, -Mathf.Abs(zoomMax), -Mathf.Abs(zoomMin));
        transform.position = transform.localRotation * offset + target.position;
    }

    public void RotateAround()
    {
        X = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime * 500;
        Y += Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime * 500;
        Y = Mathf.Clamp(Y, -limit, limit);
        transform.localEulerAngles = new Vector3(-Y, X, 0);
        transform.position = transform.localRotation * offset + target.position;
    }
}