using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Face
{
    public string ID { get; }
    public List<Point> Points { get; }
    
    public Face(Point point1, Point point2, Point point3, bool trackFaceInPoints = true, bool correctNormal = true)
    {
        ID = Guid.NewGuid().ToString();

        var centerX = (point1.Position.x + point2.Position.x + point3.Position.x) / 3;
        var centerY = (point1.Position.y + point2.Position.y + point3.Position.y) / 3;
        var centerZ = (point1.Position.z + point2.Position.z + point3.Position.z) / 3;
        var center = new Vector3(centerX, centerY, centerZ);

        // Determine correct winding order
        var normal = GetNormal(point1, point2, point3);
        Points = IsNormalPointingAwayFromOrigin(center, normal) || !correctNormal ?
            new List<Point> { point1, point2, point3 } :
            new List<Point> { point1, point3, point2 };
        
        if (trackFaceInPoints)
        {
            Points.ForEach(point => point.AssignFace(this));
        }
    }
    
    public List<Point> GetOtherPoints(Point point)
    {
        if (!IsPointPartOfFace(point))
            throw new ArgumentException("Given point must be one of the points on the face!");


        return Points.Where(facePoint => facePoint.ID != point.ID).ToList();
    }

    public bool IsAdjacentToFace(Face face)
    {
        var thisFaceIds = Points.Select(point => point.ID).ToList();
        var otherFaceIds = face.Points.Select(point => point.ID).ToList();
        return thisFaceIds.Intersect(otherFaceIds).ToList().Count == 2;
    }

    public Point GetCenter()
    {
        var centerX = (Points[0].Position.x + Points[1].Position.x + Points[2].Position.x) / 3;
        var centerY = (Points[0].Position.y + Points[1].Position.y + Points[2].Position.y) / 3;
        var centerZ = (Points[0].Position.z + Points[1].Position.z + Points[2].Position.z) / 3;

        return new Point(new Vector3(centerX, centerY, centerZ));
    }

    private bool IsPointPartOfFace(Point point) => Points.Any(facePoint => facePoint.ID == point.ID);

    private static Vector3 GetNormal(Point point1, Point point2, Point point3)
    {
        var side1 = point2.Position - point1.Position;
        var side2 = point3.Position - point1.Position;

        var cross = Vector3.Cross(side1, side2);

        return cross / cross.magnitude;
    }

    private static bool IsNormalPointingAwayFromOrigin(Vector3 surface, Vector3 normal)
    {
        // Does adding the normal vector to the center point of the face get you closer or further from the center of the polyhedron?
        return Vector3.Distance(Vector3.zero, surface) < Vector3.Distance(Vector3.zero, surface + normal);
    }
}

