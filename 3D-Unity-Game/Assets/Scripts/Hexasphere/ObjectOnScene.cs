using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public struct ObjectOnScene {
    public GameObject Prefab;
    public List<Type_of_Tiles> Spawn;
    public float chance;
}
