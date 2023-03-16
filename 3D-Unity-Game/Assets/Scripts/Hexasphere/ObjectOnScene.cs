using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
[CreateAssetMenu(menuName = "Object On Scene")]
public class ObjectOnScene : ScriptableObject
{
    public string Type;
    public GameObject Prefab;
}
[Serializable]
public struct SaveDataObject
{
    public string type;
    public Vector3 Position;
}
