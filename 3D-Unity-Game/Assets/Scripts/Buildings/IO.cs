using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using System;

[Serializable]
public class IO : MonoBehaviour
{
    public Conveyor Conveyor;
    private bool canOutput = false;
    public BuildingIOSystemController controller;
    public void OnMouseDown()
    {
        EventAggregator.ClickOnObject.Publish(this);
    }
    public bool CanOutput { 
        get { return canOutput; } 
        set { canOutput = value;
            if (value) controller.Output();
        } }
    public void Output(Item item)
    {
        Conveyor?.Get(item);
    }
    public bool Input(Item item)
    {
        return controller.Input(item);
    }
}
