using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    [SerializeField]
    public TypeOfItem typeOfItem;
    [SerializeField]
    public Texture2D texture;
}
