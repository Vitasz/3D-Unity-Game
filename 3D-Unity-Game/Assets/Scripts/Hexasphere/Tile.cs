using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;

[Serializable]
public struct SaveDataTile
{
    public Type_of_Tiles TypeOfTile;
    public List<Vector3> IcoPoints;
    public Vector3 IcoCenter;
    public int height;
    public string id;
    public List<string> NeighboresId;
    public TypeOfItem resourse;
    public int resourse_cnt;
}
public class Tile
{
    private readonly string _id;
    private readonly Point _center;
    private readonly List<Point> _neighbourCenters;
    private readonly List<Face> icosahedronFaces;
    private readonly List<Tile> _neighbours = new();
    private List<string> _neighboursId = null;
    public Type_of_Tiles _type;
    public Resourse resourse;
    public Chunk chunk;
    public Building building;
    public List<GameObject> objects;
    public Cloud Cloud;
    public GenerateMeshForTile _generateMesh;
    public int WaterLevel, MaximumHeight;
    public string ID { get { return _id; } }
    public int Height
    {
        get { return _generateMesh.Height; }
        set {
            if (_generateMesh.Height == value) return;
            if (value < 0) value = 0;
            if (value >= MaximumHeight) value = MaximumHeight;

            if (value <= WaterLevel) _type = Type_of_Tiles.Water;
            else if (value == 1 + WaterLevel || value == 2 + WaterLevel) _type = Type_of_Tiles.Sand;
            else if (value == 3 + WaterLevel || value == 4 + WaterLevel) _type = Type_of_Tiles.Ground;
            else _type = Type_of_Tiles.Mountains;

            _generateMesh.Height = value;
            if (chunk != null)
                chunk.UpdateMesh();

            if (building != null)
            {
                Quaternion rotation = Quaternion.LookRotation(_generateMesh.GetNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
                building.transform.SetPositionAndRotation(_generateMesh.GetCenter(), rotation);
            }
        }
    }
    public List<Tile> Neighbours
    {
        get { return _neighbours; }
    }
    public Tile(Chunk chunk, Point center)
    {
        _neighbourCenters = new List<Point>();
        _id = Guid.NewGuid().ToString();
        _center = center;
        _generateMesh = new GenerateMeshForTile(this, _center);
        this.chunk = chunk;
        icosahedronFaces = center.GetOrderedFaces();
        StoreNeighbourCenters(icosahedronFaces);
       

    }
    public Tile(Chunk chunk, SaveDataTile save)
    {
        _center = new Point(save.IcoCenter);
        _type = save.TypeOfTile;
        this.chunk = chunk;
        this._id = save.id;
        if (save.resourse != TypeOfItem.Nothing)
            this.resourse = new(save.resourse, save.resourse_cnt);
        _neighboursId = save.NeighboresId;
        _generateMesh = new GenerateMeshForTile(this, save);
    }

    public void ResolveNeighbourTiles(Dictionary<string, Tile> allTiles)
    {
        _neighboursId ??= _neighbourCenters.Select(center => center.ID).ToList();
        for (int i = 0; i < _neighboursId.Count; i++)
        {
            string nowID = _neighboursId[i];
            _neighbours.Add(allTiles[nowID]);
        }
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
    public void UpdateType()
    {
        //_generateMesh.SetFinalHeight();
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
        this.resourse = resourse;
    }

    public TypeOfItem GetTypeOfDrop()
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
    public void AddBuilding(GameObject building)
    {
        Quaternion rotation = Quaternion.LookRotation(_generateMesh.GetNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
        building.transform.SetPositionAndRotation(_generateMesh.GetCenter(), rotation);
        this.building = building.GetComponent<Building>();
        chunk.AddBuilding(this.building);
    }
    
    public void AddObject(GameObject prefab) {
        if (_generateMesh._details.Points.Count == 5) return;
        GameObject gameObject = GameObject.Instantiate(prefab, chunk.transform);
        Quaternion rotation = Quaternion.LookRotation(_generateMesh.GetNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
        float x = UnityEngine.Random.Range(0, 2f), y = UnityEngine.Random.Range(0, 1f);
        Vector3  A, B;
        if (x <= 1f)
        {
            A = Vector3.Lerp(_generateMesh._details.Points[0].Position, _generateMesh._details.Points[1].Position, x);
            B = Vector3.Lerp(_generateMesh._details.Points[5].Position, _generateMesh._details.Points[4].Position, x);
        }
        else
        {
            A = Vector3.Lerp(_generateMesh._details.Points[2].Position, _generateMesh._details.Points[1].Position, x - 1f);
            B = Vector3.Lerp(_generateMesh._details.Points[3].Position, _generateMesh._details.Points[4].Position, x - 1f);
        }
        gameObject.transform.SetPositionAndRotation(Vector3.Lerp(A, B, y), rotation);
        chunk.AddObject(gameObject);
    }


    public SaveDataTile Save()
    {
        SaveDataTile save = new()
        {
            height = _generateMesh.Height,
            IcoPoints = _generateMesh._details.IcoPoints,
            IcoCenter = _generateMesh._details.IcoCenter.Position,
            TypeOfTile = _type,
            id = _id,
            resourse = resourse == null ? TypeOfItem.Nothing : resourse.drop,
            resourse_cnt = resourse == null ? 0 : resourse.Durability,
            NeighboresId = Neighbours.Select(neighbour => neighbour._id).ToList()
        };
        return save;
    }
    
}

