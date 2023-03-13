using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDetails
{
    public List<Point> Vertices { get; }
    public List<Face> Triangles { get; }
    public List<Vector2> Uvs { get; }
    public Material Material { get; }
    
    public MeshDetails(List<Point> vertices, List<Face> triangles, List<Vector2> uvs, Material material)
    {
        Vertices = vertices;
        Triangles = triangles;
        Uvs = uvs;
        while (Uvs.Count < vertices.Count) Uvs.Add(new Vector2());
        Material = material;

    }
}
