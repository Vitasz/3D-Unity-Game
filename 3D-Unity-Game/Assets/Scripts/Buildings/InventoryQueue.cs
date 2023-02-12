using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class InventoryQueue : Inventory
{
    public int size = 5;
    private readonly Queue<Item> items = new ();
    public InventoryQueue(string buildingName) : base(buildingName) { }

    public override Item Get()
    {
        if (items.Count == 0) return null;
        return items.Dequeue();
    }
    public override bool Insert(Item item)
    {
        if (items.Count < size)
        {
            items.Enqueue(item);
            return true;
        }
        return false;
    }
    public override bool IsFull()
    {
        return items.Count >= size;
    }
    public override void Print()
    {
        base.Print();
        Debug.Log("Total items: " + items.Count.ToString());
        foreach(Item item in items)
        {
            Debug.Log(item);
        }
    }
}
