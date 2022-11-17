using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface GenerateMeshAble
{
    
    public MeshDetails GetMesh(HexDetails details, ref bool hasOwnGround);
}
