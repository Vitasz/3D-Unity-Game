using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Sun : MonoBehaviour
{
    public float Speed;
    public float Radius;
    private float angle;
    public GameObject GameLight;
    private void FixedUpdate()
    {
        angle += Time.deltaTime * Speed; // меняется плавно значение угла
        RenderSettings.skybox.SetFloat("_Rotation", angle + 235f);
        GameLight.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.up);
        if (angle > 360f) angle -= 360f;
    }
}
