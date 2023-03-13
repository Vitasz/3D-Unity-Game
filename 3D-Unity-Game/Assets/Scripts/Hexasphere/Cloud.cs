using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cloud
{
    private List<Vector3> _ICOpositions;
    private Vector3 _ICOcenter;
    private Vector3 _downCenter;
    private int _level = 1;
    private ChunkOfClouds _chunk;
    private float _width = 0.01f;
    private Material _material = HexMetrics.cloudMaterial;
    
    public Cloud(ChunkOfClouds chunk, List<Vector3> positions, int level)
    {
        _ICOpositions = positions;
        _chunk = chunk;
        _level = level;
        
        foreach (var item in _ICOpositions)
        {
            _ICOcenter += item;
        }
        
        _width = 0.005f * level;
        _ICOcenter /= 6;
        _downCenter = new Point(_ICOcenter).ProjectToSphere(chunk.controller.Radius, 0.5f).Position;
    }
    public MeshDetails GetMesh()
    {
        var points1 = HexPoints(1f);
        var toCenter = -_downCenter;
        toCenter.Normalize();
        
        var side1 = points1[0].Position - points1[1].Position;
        var side2 = points1[0].Position - points1[2].Position;
        var perp = Vector3.Cross(side1, side2).normalized;
        var x = (toCenter - perp).sqrMagnitude >= 1f;
        
        List<Point> HexPoints(float height)
        {
            List<Point> points = new();
            var point = new Point(_ICOcenter).ProjectToSphere(_chunk.controller.Radius * height, 0.5f);
            points.Add(point);
            
            foreach (var y in _ICOpositions)
            {
                point = new Point(y).ProjectToSphere(_chunk.controller.Radius * height, 0.5f);
                points.Add(point);
            }
            
            return points;
        }
        
        IEnumerable<Face> HexTriangles(List<Point> points, bool up)
        {
            List<Face> triangles = new();
            
            if (up) x = !x;
            
            if (x)
            {
                for (var i = 1; i < points.Count - 1; i++)
                    triangles.Add(new Face(points[0], points[(i + 1) % points.Count], points[i], correctNormal: false));
                
                triangles.Add(new Face(points[0], points[1], points[^1], correctNormal: false));
            }
            else
            {
                for (var i = 1; i < points.Count - 1; i++)
                    triangles.Add(new Face(points[0], points[i], points[(i + 1) % points.Count], correctNormal: false));
                
                triangles.Add(new Face(points[0], points[^1], points[1], correctNormal: false));
            }
            
            return triangles;
        }
        
        IEnumerable<Face> CliffsTriangles(List<Point> points1, List<Point> points2)
        {
            List<Face> triangles = new();
            
            if (x)
            {
                for (var i = 1; i < points1.Count - 1; i++)
                {
                    triangles.Add(new Face(points1[i], points2[i], points1[i + 1], correctNormal: false));
                    triangles.Add(new Face(points2[i + 1], points1[i + 1], points2[i], correctNormal: false));
                }
                
                triangles.Add(new Face(points1[^1], points2[^1], points1[1], correctNormal: false));
                triangles.Add(new Face(points2[1], points1[1], points2[^1], correctNormal: false));
            }
            else
            {
                
                for (var i = 1; i < points1.Count - 1; i++)
                {
                    triangles.Add(new Face(points1[i], points1[i + 1], points2[i], correctNormal: false));
                    triangles.Add(new Face(points2[i + 1], points2[i], points1[i + 1], correctNormal: false));
                }
                
                triangles.Add(new Face(points1[^1], points1[1], points2[^1], correctNormal: false));
                triangles.Add(new Face(points2[1], points2[^1], points1[1], correctNormal: false));
            }
            
            return triangles;
        }
        
       
        var triangles1 = HexTriangles(points1, false);
        var points2 = HexPoints(1f + _width);
        var triangles2 = HexTriangles(points2, true);
        
        List<Point> vertices = new();
        vertices.AddRange(points1);
        vertices.AddRange(points2);
        
        List<Face> triangles = new();
        triangles.AddRange(triangles1);
        triangles.AddRange(triangles2);
        
        triangles.AddRange(CliffsTriangles(points1, points2));
        
        return new(vertices, triangles, new(), _material);
    }
    public void LevelUp(int to)
    {
        if (_level >= 25) return;
        _level += to;
        
        if (_level <= 10)
        {
            _width = 0.005f * _level;
        }    
        
        if (_level >= 22) _material = HexMetrics.UraganMaterial;
        else if (_level >= 18) _material = HexMetrics.StormCloudMaterial;
        else if (_level >= 14) _material = HexMetrics.RainyCloudMaterial;
        else _material = HexMetrics.cloudMaterial;
    }

}
