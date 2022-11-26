using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct IORecipe
{
    [SerializeField]
    public readonly Dictionary<TypeOfItem, uint> Items;
    [SerializeField]
    public readonly uint Enegry;

    public IORecipe(Dictionary<TypeOfItem, uint> items, uint enegry) => (Items, Enegry) = (items, enegry);
}

