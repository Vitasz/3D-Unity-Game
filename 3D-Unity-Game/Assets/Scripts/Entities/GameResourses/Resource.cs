using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Resource
{
    public readonly string type;
    public Item drop;
    public int Durability { get; private set;  }

    public Resource(string type, int durability, Item drop)
    {
        this.type = type;
        Durability = durability;
        this.drop = drop;
    }
    public Item GetResource()
    {
        Durability--;
        if (Durability == 0)
        {
            Destruct();
        }
        return drop;
    }

    public void Destruct()
    {
        // Destroy(gameObject.transform);
    }
}
