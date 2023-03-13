using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Sun : MonoBehaviour
{
    [Range(0, 1)]
    public float speed;
    private float _angle;
    
    private void FixedUpdate()
    {
        _angle += Time.deltaTime * speed; // меняется плавно значение угла
        RenderSettings.skybox.SetFloat("_Rotation", _angle + 235f);
        transform.rotation = Quaternion.AngleAxis(-_angle, Vector3.up);
        if (_angle > 360f) _angle %= 360f;
        if (_angle < 0) _angle = 360f - Math.Abs(_angle) % 360;
    }
}
