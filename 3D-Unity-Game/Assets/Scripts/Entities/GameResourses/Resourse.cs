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
    public int Durability { 
        get { return currDurabillity; }
        set { currDurabillity = value; }
    }
    public Resourse(string type, int durability)
    {
        this.type = type;
        this.currDurabillity = durability;
    }
    /*public TypeOfItem GetResource()
    {
        currDurabillity--;
        if (currDurabillity == 0)
        {
            Destruct();
        }
        return drop;
    }*/

    public void Destruct()
    {
        //Destroy(gameObject.transform);
    }
}
