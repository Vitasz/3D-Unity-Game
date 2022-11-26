using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingWithStackInventory : Building
{
    protected Dictionary<TypeOfItem, uint> _inventory = new();

    public BuildingWithStackInventory(Tile tile, Team team) : base(tile, team) { }

    public void AddItem(Item item)
    {
        if (!_inventory.ContainsKey(item.typeOfItem))
            _inventory.Add(item.typeOfItem, 0);

        _inventory[item.typeOfItem]++;
    }

    public bool RemoveItem(Item item)
    {
        if (!_inventory.ContainsKey(item.typeOfItem))
            return false;

        _inventory[item.typeOfItem]--;
        return true;
    }
}
