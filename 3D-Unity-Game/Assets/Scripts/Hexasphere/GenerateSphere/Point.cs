using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class Point
{
    private readonly List<Face> _faces;
    
    private const float PointComparisonAccuracy = 0.0001f;

    public string ID { get; }
    public Vector3 Position { get; set; }

    public static Point Zero => new Point(Vector3.zero);

    public Point(Vector3 position) : this(position, Guid.NewGuid().ToString(), new List<Face>()) {}

    private Point(Vector3 position, string id, List<Face> faces)
    {
        ID = id;
        Position = position;
        _faces = faces;
    }

    public void AssignFace(Face face)
    {
        _faces.Add(face);
    }

    public List<Point> Subdivide(Point target, int count, Func<Point, Point> findDuplicatePointIfExists)
    {
        var segments = new List<Point> { this };

        for (var i = 1; i <= count; i++)
        {
            var x = Position.x * (1 - (float)i / count) + target.Position.x * ((float)i / count);
            var y = Position.y * (1 - (float)i / count) + target.Position.y * ((float)i / count);
            var z = Position.z * (1 - (float)i / count) + target.Position.z * ((float)i / count);
            x = (float)Math.Round(x, 5, MidpointRounding.AwayFromZero);
            y = (float)Math.Round(y, 5, MidpointRounding.AwayFromZero);
            z = (float)Math.Round(z, 5, MidpointRounding.AwayFromZero);
            var newPoint = findDuplicatePointIfExists(new Point(new Vector3(x, y, z)));
            segments.Add(newPoint);
        }

        segments.Add(target);
        return segments;
    }

    public Point ProjectToSphere(float radius, float t)
    {
        var projectionPoint = radius / Position.magnitude;
        var x = Position.x * projectionPoint * t;
        var y = Position.y * projectionPoint * t;
        var z = Position.z * projectionPoint * t;
        return new Point(new Vector3(x, y, z), ID, _faces);
    }

    public List<Face> GetOrderedFaces()
    {
        if (_faces.Count == 0) return _faces;
        var orderedList = new List<Face> {_faces[0]};

        var currentFace = orderedList[0];
        while (orderedList.Count < _faces.Count)
        {
            var existingIds = orderedList.Select(face => face.ID).ToList();
            var neighbour = _faces.First(face => !existingIds.Contains(face.ID) && face.IsAdjacentToFace(currentFace));
            currentFace = neighbour;
            orderedList.Add(currentFace);
        }

        return orderedList;
    }

    public static bool IsOverlapping(Point a, Point b)
    {
        return
            Mathf.Abs(a.Position.x - b.Position.x) <= PointComparisonAccuracy &&
            Mathf.Abs(a.Position.y - b.Position.y) <= PointComparisonAccuracy &&
            Mathf.Abs(a.Position.z - b.Position.z) <= PointComparisonAccuracy;
    }

    public override string ToString()
    {
        return $"{Position.x},{Position.y},{Position.z}";
    }

    public string ToJson()
    {
        return $"{{\"x\":{Position.x},\"y\":{Position.y},\"z\":{Position.z}, \"guid\":\"{ID}\"}}";
    }
}

