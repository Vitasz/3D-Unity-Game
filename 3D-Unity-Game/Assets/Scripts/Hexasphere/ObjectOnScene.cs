using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
[CreateAssetMenu(menuName = "Object On Scene")]
public class ObjectOnScene : ScriptableObject {
    public string type;
    public GameObject Prefab;
    public List<string> Spawn;
    public float chance;
}
[Serializable]
public struct SaveDataObject
{
    public string type;
    public Vector3 Position;
}
