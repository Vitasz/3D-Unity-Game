using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class Bour : Building, IIO
{   
    private Resource resourse;

    public int InventorySize;
    public InventoryStack inventory;
    public Animator animator;
    public float speed = 0.5f;
    public Item drop;
    public new void Start()
    {
        base.Start();
        inventory = new("Bour", InventorySize);
        resourse = tile?.Resource;
        if (resourse != null) StartCoroutine(Work());
        else
        {
            animator.enabled = false;
        }
    }
    public void OnMouseDown()
    {
        inventory.Print();
        EventAggregator.ClickOnObject.Publish(this);
    }

    IEnumerator Work()
    {
        while (true)
        {
            bool ok = inventory.Insert(resourse.drop);
            if (!ok)
            {
                animator.enabled = false;
                yield return new WaitForSeconds(speed);
                continue;
            }
            animator.enabled = true;
            resourse.GetResource();
            yield return new WaitForSeconds(speed);
        }
       
    }
    //Input - Output
    public Item Output()
    {
        return inventory.Get();
    }
    public bool Input(Item item)
    {
        return false;
    }
    public bool IsInventoryEmpty()
    {
        return inventory.IsEmpty();
    }

    
}
