using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;

[Serializable]
public struct SaveDataTile
{
    public string type;
    public List<Vector3> IcoPoints;
    public Vector3 IcoCenter;
    public int height;
    public string id;
    public List<string> NeighboresId;
    public int resourse_cnt;
    [SerializeField]
    public List<SaveDataObject> gameObjects;
}
public class Tile
{
    public string _type = "";
    public bool isOre = false;
    private readonly string _id;
    private readonly Point _center;
    private readonly List<Point> _neighbourCenters;
    private readonly List<Face> icosahedronFaces;
    private readonly List<Tile> _neighbours = new();
    private List<string> _neighboursId = null;
    public Resourse resourse;
    public Chunk chunk;
    //public Building building;
    public List<SaveDataObject> objects = new();
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

            _generateMesh.Height = value;
            if (chunk != null)
                chunk.UpdateMesh();

            /*if (building != null)
            {
                Quaternion rotation = Quaternion.LookRotation(_generateMesh.GetNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
                building.transform.SetPositionAndRotation(_generateMesh.GetCenter(), rotation);
            }*/
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
        _type = save.type;
        this.chunk = chunk;
        this._id = save.id;
        _neighboursId = save.NeighboresId;
        _generateMesh = new GenerateMeshForTile(this, save);
        if (save.gameObjects != null)
        foreach(SaveDataObject x in save.gameObjects)
        {
            AddObject(x.type, x.Position);
        }
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
    public List<MeshDetails> GetMesh() =>  _generateMesh.GetAllMeshes();

    public void AddResourse(Resourse resourse)
    {
        this.resourse = resourse;
        isOre = true;
        this._type = resourse.type;
    }
    public void AddBuilding(GameObject building)
    {
        Quaternion rotation = Quaternion.LookRotation(_generateMesh.GetNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
        building.transform.SetPositionAndRotation(_generateMesh.GetCenter(), rotation);
        //this.building = building.GetComponent<Building>();
        //chunk.AddBuilding(this.building);
    }
    
    public void AddObject(string type) {
        if (_generateMesh._details.Points.Count == 5) return;
        GameObject gameObject = GameObject.Instantiate(HexMetrics.objects[type].Prefab, chunk.transform);
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
        Vector3 position = Vector3.Lerp(A, B, y);
        gameObject.transform.SetPositionAndRotation(position, rotation);
        chunk.AddObject(gameObject);
        objects.Add(new SaveDataObject
        {
            type = type,
            Position = position
        });
    }
    public void AddObject(string type, Vector3 position)
    {
        if (_generateMesh._details.Points.Count == 5) return;
        GameObject gameObject = GameObject.Instantiate(HexMetrics.objects[type].Prefab, chunk.transform);
        Quaternion rotation = Quaternion.LookRotation(_generateMesh.GetNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
        gameObject.transform.SetPositionAndRotation(position, rotation);
        chunk.AddObject(gameObject);
        objects.Add(new SaveDataObject { 
            type =type,
            Position = position
        });
    }

    public SaveDataTile Save()
    {
        SaveDataTile save = new()
        {
            height = _generateMesh.Height,
            IcoPoints = _generateMesh._details.IcoPoints,
            IcoCenter = _generateMesh._details.IcoCenter.Position,
            type = _type,
            id = _id,
            resourse_cnt = resourse == null ? 0 : resourse.Durability,
            NeighboresId = Neighbours.Select(neighbour => neighbour._id).ToList(),
            gameObjects = objects
        };
        return save;
    }
    
}

