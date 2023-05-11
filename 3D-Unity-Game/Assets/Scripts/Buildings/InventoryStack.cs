using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Transactions;
using UnityEngine;

public class InventoryStack : Inventory
{
    public int size = 2;
    private readonly List<(Item item, int cnt)> items = new();
    public InventoryStack(string buildingName, int size) : base(buildingName) {
        this.size = size;
    }

    private bool Increase(int index)
    {
        if (items[index].item.StackLimit == items[index].cnt) return false; 
        items[index] = (items[index].item, items[index].cnt+1);
        return true;
    }
    private void Decrease(int index)
    {
        items[index] = (items[index].item, items[index].cnt - 1);
        if (items[index].cnt == 0) items.RemoveAt(index);
    }
    public int FindItem(Item item, bool toIncrease = false)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (item == items[i].item)
            {
                if (toIncrease && item.StackLimit == items[i].cnt) continue;
                return i;
            }
        }
        return -1;
    }
    public override Item Get()
    {
        if (items.Count == 0) return null;
        Item item = items[0].item;
        Decrease(0);
        return item;
    }
   
    public override bool Insert(Item item)
    {
        int index = FindItem(item, true);
        if (index == -1 && items.Count == size) return false;
        if (index == -1)
        {
            items.Add((item, 1));
            return true;
        }
        return Increase(index);
    }
    public override bool IsFull()
    {
        return items.Count >= size;
    }
    public override bool IsEmpty()
    {
        return items.Count == 0;
    }
    public override void Print()
    {
        base.Print();
        Debug.Log(items.Count);
        foreach (var item in items)
        {
            Debug.Log(item.ToString());
        }
    }

    public int GetCount(Item item)
    {
        int count = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (item == items[i].item)
            {
                count += items[i].cnt;
            }
        }
        return count;
    }

    public bool RemoveItems(Item item, int count)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (item == items[i].item)
            {
                int tmp = items[i].cnt;
                items[i] = (items[i].item, items[i].cnt - count);
                if (items[i].cnt <= 0)
                {
                    items.RemoveAt(i);
                    i--;
                }
                count -= tmp;
                if (count <= 0)
                {
                    return true;
                }
                
            }
        }
        return false;
    }
    
    
}
