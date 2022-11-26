using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingWithQueueInventory : Building
{
    protected List<TypeOfItem> _inventory;

    public BuildingWithQueueInventory(Tile tile, Team team) : base(tile, team) {}

    public void AddItem(Item item)
    {
        _inventory.Add(item.typeOfItem);
    }

    public TypeOfItem? RemoveItem()
    {
        if (_inventory.Count == 0)
            return null;

        var first = _inventory[0];
        _inventory.RemoveAt(0);
        return first;
    }
}
