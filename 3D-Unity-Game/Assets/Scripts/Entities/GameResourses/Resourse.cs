using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Resourse
{
    private int currDurabillity;
    public readonly TypeOfItem drop;
    public int Durability { 
        get { return currDurabillity; }
        set { currDurabillity = value; }
    }
    public Resourse(TypeOfItem drop, int durability)
    {
        this.drop = drop;
        this.currDurabillity = durability;
    }
    public TypeOfItem GetResource()
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
