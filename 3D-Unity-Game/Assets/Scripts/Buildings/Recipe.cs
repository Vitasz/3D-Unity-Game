using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Recipe")]
public class Recipe : ScriptableObject
{
    public BuildingType BuildingType;

    public float time;
    public int energy;

    public List<ElementRecipe> input;
    public List<ElementRecipe> output;

}

[Serializable]
public struct ElementRecipe
{
    public Item item;
    public int Count;
}