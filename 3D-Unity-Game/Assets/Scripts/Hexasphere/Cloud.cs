using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cloud
{
    private List<Vector3> ICOpositions;
    Vector3 ICOcenter = new();
    Vector3 down_center;
    private int level = 1;
    private ChunkOfClouds chunk;
    private float width = 0.01f;
    private Material material = HexMetrics.cloudMaterial;
    public Cloud(ChunkOfClouds chunk, List<Vector3> positions, int level)
    {
        ICOpositions = positions;
        this.chunk = chunk;
        this.level = level;
        for (int i = 0; i < ICOpositions.Count; i++)
        {
            ICOcenter += ICOpositions[i];
        }
        width = 0.005f * level;
        ICOcenter /= 6;
        down_center = new Point(ICOcenter).ProjectToSphere(chunk.controller.Radius, 0.5f).Position;
    }
    public MeshDetails GetMesh()
    {
        //MeshDetails mesh = new();
        List<Point> points1 = HexPoints(1f);
        Vector3 toCenter = -down_center;
        toCenter.Normalize();
        Vector3 side1 = points1[0].Position - points1[1].Position;
        Vector3 side2 = points1[0].Position - points1[2].Position;
        Vector3 perp = Vector3.Cross(side1, side2).normalized;
        bool x = (toCenter - perp).sqrMagnitude >= 1f;
        List<Point> HexPoints(float height)
        {
            List<Point> points = new();
            Point x = new Point(ICOcenter).ProjectToSphere(chunk.controller.Radius * height, 0.5f);
            points.Add(x);
            foreach (Vector3 y in ICOpositions)
            {
                x = new Point(y).ProjectToSphere(chunk.controller.Radius * height, 0.5f);
                points.Add(x);
            }
            return points;
        }
        List<Face> HexTriangles(List<Point> points, bool up)
        {
            List<Face> triangles = new();
            
            if (up) x = !x;
            if (x)
            {
                for (int i = 1; i < points.Count - 1; i++)
                    triangles.Add(new Face(points[0], points[(i + 1) % points.Count], points[i], correctNormal: false));
                triangles.Add(new Face(points[0], points[1], points[^1], correctNormal: false));
            }
            else
            {
                for (int i = 1; i < points.Count - 1; i++)
                    triangles.Add(new Face(points[0], points[i], points[(i + 1) % points.Count], correctNormal: false));
                triangles.Add(new Face(points[0], points[^1], points[1], correctNormal: false));
            }
            return triangles;
        }
        List<Face> CliffsTriangles(List<Point> points1, List<Point> points2)
        {
            
            List<Face> triangles = new();
            if (x)
            {
                for (int i = 1; i < points1.Count - 1; i++)
                {
                    triangles.Add(new Face(points1[i], points2[i], points1[i + 1], correctNormal: false));
                    triangles.Add(new Face(points2[i + 1], points1[i + 1], points2[i], correctNormal: false));
                }
                triangles.Add(new Face(points1[^1], points2[^1], points1[1], correctNormal: false));
                triangles.Add(new Face(points2[1], points1[1], points2[^1], correctNormal: false));
            }
            else
            {
                
                for (int i = 1; i < points1.Count - 1; i++)
                {
                    triangles.Add(new Face(points1[i], points1[i + 1], points2[i], correctNormal: false));
                    triangles.Add(new Face(points2[i + 1], points2[i], points1[i + 1], correctNormal: false));
                }
                triangles.Add(new Face(points1[^1], points1[1], points2[^1], correctNormal: false));
                triangles.Add(new Face(points2[1], points2[^1], points1[1], correctNormal: false));
            }
            return triangles;
        }
        
       
        List<Face> triangles1 = HexTriangles(points1, false);
        List<Point> points2 = HexPoints(1f + width);
        List<Face> triangles2 = HexTriangles(points2, true);
        List<Point> vertices = new();
        vertices.AddRange(points1);
        vertices.AddRange(points2);
        List<Face> triangles = new();
        triangles.AddRange(triangles1);
        triangles.AddRange(triangles2);
        
        triangles.AddRange(CliffsTriangles(points1, points2));
        
        return new(vertices, triangles, new(), material);
    }
    public void LevelUp(int to)
    {
        if (level >= 25) return;
        level += to;
        if (level <= 10)
        {
            width = 0.005f * level;
        }    
        
        if (level >= 22) material = HexMetrics.UraganMaterial;
        else if (level >= 18) material = HexMetrics.StormCloudMaterial;
        else if (level >= 14) material = HexMetrics.RainyCloudMaterial;
        else material = HexMetrics.cloudMaterial;
    }

}
