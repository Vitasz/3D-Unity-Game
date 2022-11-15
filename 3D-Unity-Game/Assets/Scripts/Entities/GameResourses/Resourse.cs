using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Resourse : ScriptableObject
{
    public int MaxDurability = 20000;
    private int currDurabillity;
    public bool hasOwnGroundColor = false;
    public Color color;
    public TypeOfItem drop;
    public void SetDurability(int durability)
    {
        currDurabillity = durability;
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
