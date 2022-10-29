using System.Collections.Generic;
using UnityEngine;

public class MeshDetails
{
    private readonly List<Vector3> _vertices;
    private readonly List<int> _triangles;
    private readonly List<Color> _colors;
    public MeshDetails(List<Vector3> vertices, List<int> triangles, List<Color> colors)
    {
        _vertices = vertices;
        _triangles = triangles;
        _colors = colors;
    }

    public List<Vector3> Vertices => _vertices;

    public List<int> Triangles => _triangles;

    public List<Color> Colors => _colors;
}
