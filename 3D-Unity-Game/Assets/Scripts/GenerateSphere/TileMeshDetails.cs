using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDetails
{
    private readonly List<Point> _vertices;
    private readonly List<Face> _triangles;
    private readonly List<Color> _colors;
    private readonly List<Vector2> _uvs;
    public readonly Material material;
    public MeshDetails(List<Point> vertices, List<Face> triangles, List<Color> colors, List<Vector2> uvs, Material material)
    {
        _vertices = vertices;
        _triangles = triangles;
        _colors = colors;
        _uvs = uvs;
        while (colors.Count < vertices.Count) colors.Add(Color.white);
        while (_uvs.Count < vertices.Count) _uvs.Add(new());
        this.material = material;   
    }
    

    public List<Point> Vertices => _vertices;

    public List<Face> Triangles => _triangles;

    public List<Color> Colors => _colors;
    public List<Vector2> Uvs => _uvs;
}
