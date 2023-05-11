using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : Building, IIO
{
    public int InventorySize;
    InventoryStack inventory;

    public override void Start()
    {
        base.Start();
        inventory = new("Storage", InventorySize);
    }
    public void OnMouseDown()
    {
        inventory.Print();
        EventAggregator.ClickOnObject.Publish(this);
    }
    public bool Input(Item item)
    {
        return inventory.Insert(item);
    }

    public Item Output()
    {
        return inventory.Get();
    }

    public bool IsInventoryEmpty() => inventory.IsEmpty();
}
