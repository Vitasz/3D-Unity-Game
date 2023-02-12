using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject
{
    public string type;
    public GameObject Prefab;
    public int StackLimit;
    public override string ToString()
    {
        return type;
    }
}
