using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Furnace : Building, IIO
{
    public int OutputInventorySize = 1;
    public int InputInventorySize = 1;
    InventoryStack inputInventory;
    InventoryStack outputInventory;
    public Recipe Recipe;

    public float State = 0f; //{ get; private set; }

    public override void Start()
    {
        base.Start();
        inputInventory = new("Storage input", OutputInventorySize);
        outputInventory = new("Storage output", InputInventorySize);
        StartCoroutine(Work());

    }
    public void OnMouseDown()
    {
        inputInventory?.Print();
        outputInventory?.Print();
        EventAggregator.ClickOnObject.Publish(this);
    }
    public bool Input(Item item)
    {
        
        if (Recipe?.input.Where(x => x.item == item).Count() > 0)
        {
            return inputInventory.Insert(item);
        }
        return false;
    }
    IEnumerator Work()
    {
        while (true)
        {
            bool ok = CheckRecipe();
            if (!ok)
            {
                //animator.enabled = false;
                yield return new WaitForEndOfFrame();
                continue;
            }
            StartRecipe();
            while (State < Recipe.time)
            {
                State += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
            EndRecipe();
            yield return new WaitForEndOfFrame();
        }

    }
    public Item Output()
    {
        return outputInventory.Get();
    }

    public bool IsInventoryEmpty() => outputInventory.IsEmpty();

    private bool CheckRecipe()
    {
        if (Recipe == null) return false;
        foreach(var x in Recipe.input)
        {
            if (inputInventory?.GetCount(x.item) >= x.Count) continue;
            return false;
        }
        foreach (var x in Recipe.output) {
            if (outputInventory?.GetCount(x.item) + x.Count > x.item.StackLimit * Recipe.output.Count(y => y.item == x.item)) return false;
        }
        return true;
    }
    private void StartRecipe()
    {
        State = 0f;
        foreach (var x in Recipe.input)
        {
            inputInventory.RemoveItems(x.item, x.Count);
        }
    }

    private void EndRecipe()
    {
        foreach (var x in Recipe.output)
        {
            for (int i = 0; i < x.Count; i++)
            {
                outputInventory.Insert(x.item);
            }
        }
        State = 0f;
    }

    public void SetRecipe(Recipe recipe)
    {
        Recipe = recipe;
        inputInventory = new ("Furnace", recipe.input.Count);
        outputInventory = new("Furnace", recipe.output.Count);
    }
}
