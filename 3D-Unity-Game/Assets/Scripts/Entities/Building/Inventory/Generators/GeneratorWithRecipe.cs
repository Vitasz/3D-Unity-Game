using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneratorWithRecipe : Generator
{
    [SerializeField]
    public List<Recipe> Recipes;

    public GeneratorWithRecipe(Tile tile, Team team) : base(tile, team) {}

    public bool CheckAvailableRecipes()
    {
        if (GetAvailableRecipes() != default)
            return true;

        return false;
    }

    public void ExecuteRecipe()
    {
        var recipe = GetAvailableRecipes();

        if (recipe == default)
            return;

        _currSaveEnegry -= recipe.Input.Enegry;

        foreach (var item in recipe.Input.Items)
        {
            if (_inventory[item.Key] == item.Value)
                _inventory.Remove(item.Key);
            else
                _inventory[item.Key] -= item.Value;
        }

        _currSaveEnegry += recipe.Output.Enegry;

        foreach (var item in recipe.Output.Items)
        {
            if (!_inventory.ContainsKey(item.Key))
                _inventory.Add(item.Key, 0);
            
            _inventory[item.Key] += item.Value;
        }
    }

    private Recipe GetAvailableRecipes()
    {
        foreach (var recipe in Recipes)
        {
            if (CheckRecipe(recipe))
                return recipe;
        }

        return default;
    }

    private bool CheckRecipe(Recipe recipe)
    {
        foreach (var item in recipe.Input.Items)
        {
            if (!_inventory.ContainsKey(item.Key) || _inventory[item.Key] < item.Value)
                return false;
        }

        return _currSaveEnegry >= recipe.Input.Enegry;
    }
}
