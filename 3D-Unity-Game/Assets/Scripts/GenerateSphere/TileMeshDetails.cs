using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMeshDetails
{
    private readonly List<Point> _vertices;
    private readonly List<Point> _triangles;
    private readonly List<Color> _colors;
    public TileMeshDetails(List<Point> vertices, List<Point> triangles, List<Color> colors, 
        List<Vector3> pointsLineRenderer, Color colorLineRenderer)
    {
        _vertices = vertices;
        _triangles = triangles;
        _colors = colors;
    }

    public List<Point> Vertices => _vertices;

    public List<Point> Triangles => _triangles;

    public List<Color> Colors => _colors;
}
