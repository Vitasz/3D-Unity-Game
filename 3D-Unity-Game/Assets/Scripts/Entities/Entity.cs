using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Entity : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> gameObjects = new List<GameObject>();
}