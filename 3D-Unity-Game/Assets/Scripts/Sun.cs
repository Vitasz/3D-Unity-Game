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
        angle += Time.deltaTime; // меняется плавно значение угла
        var x = Mathf.Cos(angle * Speed) * Radius;
        var y = Mathf.Sin(angle * Speed) * Radius;
        Quaternion rotation = Quaternion.LookRotation(new Vector3(-x, 0, -y));
        
        GameLight.transform.rotation = rotation;
        transform.position = new Vector3(x, 0, y);
    }
}
