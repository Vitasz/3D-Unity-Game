using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Inventory
{
    protected string buildingName = "";

    public Inventory(string buildingName)
    {
        this.buildingName = buildingName;
    }

    public virtual Item Get() => null;
    public virtual bool Insert(Item item) => false;
    public virtual bool IsFull() => true;
    public virtual bool IsEmpty() => false;
    public virtual void Print() {
        Debug.Log("Inventory of " + this.buildingName);
    }

}
