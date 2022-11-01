using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Tile
{
    private Point _center;
    private List<Point> _neighbourCenters;
    private List<Face> icosahedronFaces;
    private List<Tile> _neighbours;
    public Type_of_Tiles _type;
    private Resourse resourse;
    public GenerateMeshForTile _generateMesh;
    private Hexasphere _sphere;
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
    public Tile(Point center, float radius, float size, Hexasphere hexasphere)
    {
        _neighbourCenters = new List<Point>();
        _neighbours = new List<Tile>();

        //SetRandomHeight();
        _center = center;
        size = Mathf.Max(0.01f, Mathf.Min(1f, size));
        _sphere = hexasphere;
       
        _generateMesh = new GenerateMeshForTile(_center, radius, size);

        icosahedronFaces = center.GetOrderedFaces();
        StoreNeighbourCenters(icosahedronFaces);

    }
    
        
    public void ResolveNeighbourTiles(List<Tile> allTiles)
    {
        List<string> neighbourIds = _neighbourCenters.Select(center => center.ID).ToList();
        _neighbours = allTiles.Where(tile => neighbourIds.Contains(tile._center.ID)).ToList();
        _generateMesh.setNeighbours(_neighbours);
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
    public void CreateHex() => _generateMesh.CreateHex();
    public void BuildBridges() => _generateMesh.BuildBridges();
    public void BuildTriangles() { }// => _generateMesh.BuildTriangles();
    public void UpdateType()
    {
        _generateMesh.SetFinalHeight();
        int Height = _generateMesh.Height;
        if (Height == WaterLevel) _type = Type_of_Tiles.Water;
        else if (Height == 1 + WaterLevel || Height == 2 + WaterLevel) _type = Type_of_Tiles.Sand;
        else if (Height == 3 + WaterLevel || Height == 4 + WaterLevel) _type = Type_of_Tiles.Ground;
        else _type = Type_of_Tiles.Mountains;
    }
    public TileMeshDetails GetMesh()
    {
        TileMeshDetails details = _generateMesh.RecalculateDetails();


        return details;
    }
    
    /*public void addResourse(GameObject ObjectPrefab)
    {
        resourse = Instantiate(ObjectPrefab, transform).GetComponent<Resourse>();
        resourse.transform.position = _generateMesh._hexCenter.Position;
        resourse.transform.rotation = Quaternion.LookRotation(_generateMesh.getNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
        resourse.Radius = _generateMesh.GetRadius();
        resourse.GenerateTexture();
    } */
}

