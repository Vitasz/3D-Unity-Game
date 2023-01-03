using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Tile")]
public class TileObject : ScriptableObject
{
    public string type;
    public bool isLiquid;
    public int minHeight;
    public int maxHeight;
    public Material material;
}
