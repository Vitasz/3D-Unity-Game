using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDetails
{
    private readonly List<Point> _vertices;
    private readonly List<Face> _triangles;
    private readonly List<Vector2> _uvs;
    public readonly Material material;
    public MeshDetails(List<Point> vertices, List<Face> triangles, List<Vector2> uvs, Material material)
    {
        _vertices = vertices;
        _triangles = triangles;
        _uvs = uvs;
        while (_uvs.Count < vertices.Count) _uvs.Add(new());
        this.material = material;

    }
    public List<Point> Vertices => _vertices;

    public List<Face> Triangles => _triangles;
    public List<Vector2> Uvs => _uvs;
}
