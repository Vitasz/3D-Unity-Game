using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingType Type;
    public List<IO> IOs;
    public Tile tile;
    //protected Inventory inventory;
    public BuildingIOSystemController buildingIOSystemController;
    public virtual void Start()
    {
        buildingIOSystemController = new(this);
        foreach (var io in IOs) io.controller = buildingIOSystemController;
    }
}
