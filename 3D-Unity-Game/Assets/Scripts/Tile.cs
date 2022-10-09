using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile
{
    private readonly Point _center;
    private readonly float _radius;
    private readonly float _size;
    private readonly List<Face> _faces;
    private readonly List<Point> _points;
    private readonly List<Point> _neighbourCenters;
    private List<Tile> _neighbours;
        
    public Tile(Point center, float radius, float size)
    {
        _points = new List<Point>();
        _faces = new List<Face>();
        _neighbourCenters = new List<Point>();
        _neighbours = new List<Tile>();
        
        //Debug.Log(Color.red);
        _center = center;
        _radius = radius;
        _size = Mathf.Max(0.01f, Mathf.Min(1f, size));
            
        List<Face> icosahedronFaces = center.GetOrderedFaces();

        StoreNeighbourCenters(icosahedronFaces);
        BuildFaces(icosahedronFaces);
        
        SetColor(new Color(Random.value, Random.value, Random.value));
    }

    public List<Point> Points => _points;
    public Point Center => _center;
    public List<Face> Faces => _faces;
    public List<Tile> Neighbours => _neighbours;

    public List<Color> Colors = new List<Color>();
        
    public void ResolveNeighbourTiles(List<Tile> allTiles)
    {
        List<string> neighbourIds = _neighbourCenters.Select(center => center.ID).ToList();
        _neighbours = allTiles.Where(tile => neighbourIds.Contains(tile._center.ID)).ToList();
    }
        
    public override string ToString()
    {
        return $"{_center.Position.x},{_center.Position.y},{_center.Position.z}";
    }

    public string ToJson()
    {
        return $"{{\"centerPoint\":{_center.ToJson()},\"boundary\":[{string.Join(",",_points.Select(point => point.ToJson()))}]}}";
    }

    private void StoreNeighbourCenters(List<Face> icosahedronFaces)
    {
        icosahedronFaces.ForEach(face =>
        {
            List<Point> otherPoints = face.GetOtherPoints(_center);
            otherPoints.ForEach(point =>
            {
                if (_neighbourCenters.FirstOrDefault(centerPoint => centerPoint.ID == point.ID) == null)
                {
                    _neighbourCenters.Add(point);
                }
            });
        });
    }

    private void BuildFaces(List<Face> icosahedronFaces)
    {
        List<Vector3> polygonPoints = icosahedronFaces.Select(face => Vector3.Lerp(_center.Position, face.GetCenter().Position, _size)).ToList();
        polygonPoints.ForEach(pos => _points.Add(new Point(pos).ProjectToSphere(_radius, 0.5f)));
       
        _faces.Add(new Face(_points[0], _points[1], _points[2]));
        _faces.Add(new Face(_points[0], _points[2], _points[3]));
        _faces.Add(new Face(_points[0], _points[3], _points[4]));
        if (_points.Count > 5)
        {
            _faces.Add(new Face(_points[0], _points[4], _points[5]));
        }
    }

    public void SetColor(Color color)
    {
        Colors.Clear();
        
        for (int i = 0; i < _points.Count; i++) Colors.Add(color);
    }
}

