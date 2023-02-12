using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Ore")]
public class OreObject : ScriptableObject
{
    public string type;
    public int minSize;
    public int maxSize;
    public int minCount;
    public int maxCount;
    public float chance;
    public Material material;
    public Item drop;
}
