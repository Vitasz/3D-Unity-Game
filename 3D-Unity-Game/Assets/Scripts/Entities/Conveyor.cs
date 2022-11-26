using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public int MaxCountObject = 10;

    private Team _team;
    private List<Item> _items;
    private List<Item> _filters;

    public Item First => _items.FirstOrDefault();

    public Conveyor(Team team)
    {
        _team = team;
    }

    public bool AddItem(Item item)
    {
        if (!_filters.Contains(item, (v1, v2) => v1.typeOfItem == v2.typeOfItem))
            return false;

        if (_items.Count == MaxCountObject)
            return false;

        // Добавление gameobject для отрисоки элемента

        _items.Add(item);
        return true;
    }

    public Item GetItem()
    {
        var item = _items[0];

        _items.RemoveAt(0);

        return item;
    }

    public void MoveItems()
    {
        // Смещение предметов отрисовки
    }
}
