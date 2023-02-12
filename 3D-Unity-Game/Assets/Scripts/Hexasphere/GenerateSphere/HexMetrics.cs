using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor;
using UnityEngine;

public static class HexMetrics
{
    public static Material hexMaterial = (Material)Resources.Load("Materials/HexMaterial", typeof(Material));
    public static Material treeMaterial = (Material)Resources.Load("Materials/TreeMaterial", typeof(Material));
    public static Material cloudMaterial = (Material)Resources.Load("Materials/Cloud", typeof(Material));
    public static Material RainyCloudMaterial = (Material)Resources.Load("Materials/RainyCloud", typeof(Material));
    public static Material StormCloudMaterial = (Material)Resources.Load("Materials/StormCloud", typeof(Material));
    public static Material UraganMaterial = (Material)Resources.Load("Materials/UraganCloud", typeof(Material));
    public static Dictionary<string, ObjectOnScene> objects = FindAssetsByType<ObjectOnScene>().ToDictionary(x => x.type);
   // public static Dictionary<string, Item> items = FindAssetsByType<Item>().ToDictionary(x => x.type);
    public static Dictionary<string, TileObject> tiles = FindAssetsByType<TileObject>().ToDictionary(x => x.type);
    public static Dictionary<string, OreObject> ores = FindAssetsByType<OreObject>().ToDictionary(x => x.type);
    

    public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new ();
        var x = Resources.LoadAll("MyAsset", typeof(T)).ToList();
        foreach(var y in x)
        {
            assets.Add((T) y);
        }
       
        return assets;
    }
}
