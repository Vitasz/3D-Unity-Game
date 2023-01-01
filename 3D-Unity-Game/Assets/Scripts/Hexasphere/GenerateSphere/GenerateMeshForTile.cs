//using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEditor.PlayerSettings;

public class GenerateMeshForTile
{
    public readonly HexDetails _details = new();
    private List<Face> _hexFaces = new();
    private List<MeshDetails> bridges = new();
    private readonly List<Face> icosahedronFaces;
    private readonly Tile tile;
    
    public int Height
    {
        get { return _details.Height; }
        set { 
            _details.Height = value;
            if (_details.Points == null) return;
            BuildHex();
        }
    }
    public GenerateMeshForTile(Tile tile, Point center)
    {
        this.tile = tile;
        _details.IcoCenter = center;
        icosahedronFaces = center.GetOrderedFaces();
        Height = 0;
        _details.IcoPoints = icosahedronFaces.Select(face => face.GetCenter().Position).ToList();
    }
    public GenerateMeshForTile(Tile tile, SaveDataTile save)
    {
        this.tile = tile;
        _details.IcoCenter = new (save.IcoCenter);
        
        //icosahedronFaces = center.GetOrderedFaces();
        Height = save.height;
        _details.IcoPoints = save.IcoPoints;
        BuildHex();
    }
    public void BuildHex()
    {
        _details.Points = new();
        _hexFaces = new();
        bridges = new();
        _details.Center = _details.IcoCenter.ProjectToSphere(tile.chunk.Sphere.Radius + _details.Height * tile.chunk.Sphere.DeltaHeight, 0.5f);
        _details.IcoPoints.ForEach(pos =>
        {
            AddHexPoint(new Point(pos).ProjectToSphere(tile.chunk.Sphere.Radius + _details.Height * tile.chunk.Sphere.DeltaHeight, 0.5f));
        });
        for (int i = 0; i < _details.Points.Count; i++)
        {
            _hexFaces.Add(new Face(_details.Center, _details.Points[i], _details.Points[(i + 1)%_details.Points.Count]));
        }
        if (_details.Height == 0) return;
        for (int i = 0; i < _details.Points.Count; i++)
        {
            CreateCliff(_details.Points[i], _details.Points[(i+1) % _details.Points.Count],
                new Point(_details.IcoPoints[i]).ProjectToSphere(tile.chunk.Sphere.Radius, 0.5f),
                new Point(_details.IcoPoints[(i + 1) % _details.Points.Count]).ProjectToSphere(tile.chunk.Sphere.Radius, 0.5f));
        }
    }
    private void CreateCliff(Point A, Point B, Point A1, Point B1)
    {
        List<Point> points = new();
        List<Face> faces = new();
        List<Vector2> uvs = new();
        points.Add(A);
        points.Add(B);
        points.Add(B1);
        points.Add(A1);
        Vector3 toCenter = _details.Center.Position - (points[0].Position + points[1].Position) / 2;
        toCenter.Normalize();
        Vector3 side1 = points[0].Position - points[1].Position;
        Vector3 side2 = points[0].Position - points[2].Position;
        Vector3 perp = Vector3.Cross(side1, side2);
        perp.Normalize();

        if ((toCenter - perp).sqrMagnitude >= 1f)
        {
            faces.Add(new Face(points[0], points[1], points[2]));
            faces.Add(new Face(points[0], points[2], points[3]));
        }
        else
        {

            faces.Add(new Face(points[0], points[2], points[1]));
            faces.Add(new Face(points[0], points[3], points[2]));
        }
        uvs.Add(new Vector2(0.5f, 1f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(1f, 0f));
        uvs.Add(new Vector2(0.5f, 0f));
        bridges.Add(new MeshDetails(points, faces, uvs, GetHexMaterial()));
    }
   
    private Material GetHexMaterial()
    {
        if (tile.resourse != null)
        {
            return tile.resourse.drop switch
            {
                TypeOfItem.CoalOre => HexMetrics.coalMaterial,
                TypeOfItem.IronOre => HexMetrics.ironOreMaterial,
                TypeOfItem.Stone => HexMetrics.stoneMaterial,
                _ => HexMetrics.hexMaterial,
            };
        }
        return tile._type switch
        {
            Type_of_Tiles.Sand => HexMetrics.desertMaterial,
            Type_of_Tiles.Ground => HexMetrics.groundMaterial,
            Type_of_Tiles.Water => HexMetrics.waterMaterial,
            Type_of_Tiles.Mountains => HexMetrics.snowMaterial,
            _ => HexMetrics.groundMaterial,
        };
    }
    private void AddHexPoint(Point point)
    {
        _details.Points.Add(point);
    }
    public MeshDetails GetHexDetails()
    {
        List<Point> vertices = new ();
        List<Face> triangles = new ();
        List<Vector2> uv = new ();
        vertices.Add(_details.Center);
        _details.Points.ForEach(point => vertices.Add(point));
        _hexFaces.ForEach(face => triangles.Add(face));
        uv.Add(new Vector2(0.25f, 0.5f));
        uv.Add(new Vector2(0, 0.5f));
        uv.Add(new Vector2(0.125f, 0f));
        uv.Add(new Vector2(0.375f, 0f));
        uv.Add(new Vector2(0.5f, 0.5f));
        uv.Add(new Vector2(0.375f, 1f));
        if (vertices.Count != 6)uv.Add(new Vector2(0.125f, 1f));
        return new MeshDetails(vertices, triangles, uv, GetHexMaterial());
    }
    public List<MeshDetails> GetBridgesDetails()
    {
        return bridges;
    }
    public Vector3 GetNormal()
    {
        return _details.Center.Position.normalized;
    }
    public List<MeshDetails> GetAllMeshes()
    {
        List<MeshDetails> details = new()
        {
            GetHexDetails()
        };
        details.AddRange(GetBridgesDetails());
        return details;
    }
    public Vector3 GetCenter() => _details.Center.Position;
    public Vector3[] GetPositions() => _details.Points.Select(point => point.Position).ToArray();

}

