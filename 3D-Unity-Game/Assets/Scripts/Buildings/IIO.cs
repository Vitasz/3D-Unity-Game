using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIO
{
    public bool Input(Item item);
    public Item Output();
    public bool IsInventoryEmpty();
}
