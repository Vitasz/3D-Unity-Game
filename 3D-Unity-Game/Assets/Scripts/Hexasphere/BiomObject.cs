using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Biom")]
public class BiomObject : ScriptableObject
{
    public string type;
    public bool isLiquid;
    public int minHeightPercent;
    public int maxHeightPercent;
    public int maxObjects;
    public List<ObjectOnScene> objects;
    public List<Material> TileMaterials;
}
