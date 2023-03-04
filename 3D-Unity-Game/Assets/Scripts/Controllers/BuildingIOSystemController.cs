using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BuildingIOSystemController
{
    private readonly IIO building;
    private int priorityOutputIndex = 0;
    private readonly List<IO> iOs;
    public BuildingIOSystemController(Building building)
    {
        this.building = building as IIO;
        iOs = building.IOs;
    }
    //OUTPUT
    public void Output()
    {
        if (building.IsInventoryEmpty()) return;
        int index = GetIndexNextOutput();
        if (index == -1 || building.IsInventoryEmpty()) return;

        iOs[index].Output(building.Output());
        priorityOutputIndex = (index + 1) % iOs.Count;
    }
    public int GetIndexNextOutput()
    {
        for (int i = priorityOutputIndex; i < iOs.Count; i++)
        {
            IO now = iOs[i];
            if (!now.CanOutput) continue;
            return i;
        }
        for (int i = 0; i < priorityOutputIndex; i++)
        {
            IO now = iOs[i];
            if (!now.CanOutput) continue;
            return i;
        }
        return -1;
    }
    
    //INPUT
    public bool Input(Item item)
    {
        return building.Input(item);
    }
}
