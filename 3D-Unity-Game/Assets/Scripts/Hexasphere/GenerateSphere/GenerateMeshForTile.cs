//using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
//using static UnityEditor.PlayerSettings;

public class GenerateMeshForTile
{
    public readonly HexDetails _details = new();
    private List<Face> _hexFaces = new();
    private List<MeshDetails> _bridges = new();
    private readonly List<Face> _icosahedronFaces;
    private readonly Tile _tile;
    
    public Vector3 Normalize => _details.Center.Position.normalized;
    public Vector3 Center =>_details.Center.Position;
    
    public int Height
    {
        get => _details.Height;
        set { 
            _details.Height = value;
            
            if (_details.Points == null) return;
            
            BuildHex();
        }
    }
    
    public GenerateMeshForTile(Tile tile, Point center)
    {
        _tile = tile;
        _details.IcoCenter = center;
        _icosahedronFaces = center.GetOrderedFaces();
        Height = 0;
        _details.IcoPoints = _icosahedronFaces.Select(face => face.GetCenter().Position).ToList();
    }
    
    public GenerateMeshForTile(Tile tile, SaveDataTile save)
    {
        _tile = tile;
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
        _bridges = new();
        _details.Center = _details.IcoCenter.ProjectToSphere(_tile.Chunk.sphere.Radius + _details.Height * _tile.Chunk.sphere.DeltaHeight, 0.5f);
        
        _details.IcoPoints.ForEach(pos =>
        {
            AddHexPoint(new Point(pos).ProjectToSphere(_tile.Chunk.sphere.Radius + _details.Height * _tile.Chunk.sphere.DeltaHeight, 0.5f));
        });
        
        for (var i = 0; i < _details.Points.Count; i++)
        {
            _hexFaces.Add(new Face(_details.Center, _details.Points[i], _details.Points[(i + 1)%_details.Points.Count]));
        }
        
        if (_details.Height == 0) return;
        
        for (var i = 0; i < _details.Points.Count; i++)
        {
            CreateCliff(_details.Points[i], _details.Points[(i+1) % _details.Points.Count],
                new Point(_details.IcoPoints[i]).ProjectToSphere(_tile.Chunk.sphere.Radius, 0.5f),
                new Point(_details.IcoPoints[(i + 1) % _details.Points.Count]).ProjectToSphere(_tile.Chunk.sphere.Radius, 0.5f));
        }
    }
    
    private void CreateCliff(Point A, Point B, Point A1, Point B1)
    {
        List<Point> points = new()
        {
            A, B, B1, A1
        };
        List<Vector2> uvs = new()
        {
            new Vector2(0.5f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f)
        };
        List<Face> faces = new();
        
        var toCenter = _details.Center.Position - (points[0].Position + points[1].Position) / 2;
        toCenter.Normalize();
        var side1 = points[0].Position - points[1].Position;
        var side2 = points[0].Position - points[2].Position;
        var perp = Vector3.Cross(side1, side2);
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
        
        _bridges.Add(new MeshDetails(points, faces, uvs, GetHexMaterial()));
    }
   
    private Material GetHexMaterial()
    {
        if (HexMetrics.tiles.ContainsKey(_tile.Type)) return HexMetrics.tiles[_tile.Type].material;
        if (HexMetrics.ores.ContainsKey(_tile.Type)) return HexMetrics.ores[_tile.Type].material;
        return HexMetrics.hexMaterial;
    }
    
    private void AddHexPoint(Point point) => _details.Points.Add(point);

    public MeshDetails GetHexDetails()
    {
        List<Point> vertices = new ();
        List<Face> triangles = new ();
        List<Vector2> uv = new ()
        {
            new Vector2(0.25f, 0.5f),
            new Vector2(0, 0.5f),
            new Vector2(0.125f, 0f),
            new Vector2(0.375f, 0f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.375f, 1f)
        };
        
        vertices.Add(_details.Center);
        _details.Points.ForEach(point => vertices.Add(point));
        _hexFaces.ForEach(face => triangles.Add(face));
        
        if (vertices.Count != 6) uv.Add(new Vector2(0.125f, 1f));
        return new MeshDetails(vertices, triangles, uv, GetHexMaterial());
    }
    
    public List<MeshDetails> GetAllMeshes()
    {
        List<MeshDetails> details = new()
        {
            GetHexDetails()
        };
        
        details.AddRange(_bridges);
        return details;
    }
    
    public Vector3[] GetPositions() => _details.Points.Select(point => point.Position).ToArray();

}

