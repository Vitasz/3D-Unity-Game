using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Tile
{
    private readonly Point _center;
    private readonly List<Point> _neighbourCenters;
    private readonly List<Face> icosahedronFaces;
    private readonly List<Tile> _neighbours;
    public Type_of_Tiles _type;
    private Resourse resourse;
    public Chunk chunk;
    public GenerateMeshForTile _generateMesh;
    public int WaterLevel
    {
        get { return _generateMesh.WaterLevel;  }
        set { _generateMesh.WaterLevel = value; }
    }
    public int Height
    {
        get { return _generateMesh.Height; }
        set { _generateMesh.Height = value;
            if (value == 0) _type = Type_of_Tiles.Water;//new Color(0f, 0.761f, 1f);
            else if (value == 1 || value == 2) _type = Type_of_Tiles.Sand;//new Color(1f, 0.929f, 0f);
            else if (value == 3 || value == 4) _type = Type_of_Tiles.Ground;//new Color(0.508f, 1f, 0f);
            else _type = Type_of_Tiles.Mountains;
        }
    }
    public List<Tile> Neighbours
    {
        get { return _neighbours; }
    }
    public Tile(Point center, float radius, float delta_height)
    {
        _neighbourCenters = new List<Point>();
        _neighbours = new List<Tile>();

        _center = center;

        _generateMesh = new GenerateMeshForTile(_center, radius, delta_height);

        icosahedronFaces = center.GetOrderedFaces();
        StoreNeighbourCenters(icosahedronFaces);
    }
    
        
    public void ResolveNeighbourTiles(Dictionary<string, Tile> allTiles)
    {
        List<string> neighbourIds = _neighbourCenters.Select(center => center.ID).ToList();
        for (int i = 0; i < neighbourIds.Count; i++)
        {
            string nowID = neighbourIds[i];
            _neighbours.Add(allTiles[nowID]);
        }
        _generateMesh.SetNeighbours(_neighbours);
    }
        
    public override string ToString()
    {
        return $"{_center.Position.x},{_center.Position.y},{_center.Position.z}";
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
    public void CreateHex() => _generateMesh.BuildHex();
    public void BuildBridges() => _generateMesh.BuildBridges();
    public void UpdateType()
    {
        _generateMesh.SetFinalHeight();
        int Height = _generateMesh.Height;
        if (Height < WaterLevel) _type = Type_of_Tiles.Water;
        else if (Height == WaterLevel || Height == 1 + WaterLevel) _type = Type_of_Tiles.Sand;
        else if (Height == 2 + WaterLevel || Height == 3 + WaterLevel) _type = Type_of_Tiles.Ground;
        else _type = Type_of_Tiles.Mountains;
    }
    public List<MeshDetails> GetMesh()
    {
        return _generateMesh.GetAllMeshes();
    }
    
    public void AddResourse(Resourse resourse)
    {
        _generateMesh.AddResourse(resourse);
        this.resourse = resourse;
    }
    public TypeOfItem getTypeOfDrop()
    {
        if (resourse != null)
        {
            return resourse.drop;
        }
        else
        {
            return TypeOfItem.Nothing;
        }
    }
    public void AddDecoration(Mesh decor, Material material) => _generateMesh.AddDecoration(decor, material);
    //public MeshDetails GetTreesMesh() => _generateMesh.GetTreesMesh();
}

