using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Recipe
{
    public IORecipe Input { get; set; }
    public IORecipe Output { get; set; }
}
