using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Resourse
{
    private int currDurabillity;
    public readonly string type;
    public Item drop;
    public int Durability { 
        get { return currDurabillity; }
        set { currDurabillity = value; }
    }
    public Resourse(string type, int durability, Item drop)
    {
        this.type = type;
        this.currDurabillity = durability;
        this.drop = drop;
    }
    public Item GetResource()
    {
        currDurabillity--;
        if (currDurabillity == 0)
        {
            Destruct();
        }
        return drop;
    }

    public void Destruct()
    {
        //Destroy(gameObject.transform);
    }
}
