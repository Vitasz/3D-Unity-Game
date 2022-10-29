using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDetails
{
    private readonly Mesh _mesh;
    private readonly List<Vector3> _points_for_lineRenderer;
    private readonly Color _color_for_lineRenderer;
    public TileDetails(Mesh mesh, List<Vector3> points_for_lineRenderer, Color color_for_lineRenderer)
    {
        _mesh = mesh;
        _color_for_lineRenderer = color_for_lineRenderer;
        _points_for_lineRenderer = points_for_lineRenderer;
    }

    public Mesh mesh => _mesh;

    public Color Color_for_lineRenderer => _color_for_lineRenderer;

    public List<Vector3> Points_for_lineRenderer => _points_for_lineRenderer;
}
