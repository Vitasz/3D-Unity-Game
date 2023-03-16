using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using Object = UnityEngine.Object;

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
    private readonly Point _center;
    private readonly List<Point> _neighbourCenters;
    private readonly List<Face> icosahedronFaces;
    private List<string> _neighboursId;
    private readonly List<GameObject> _objects = new();
    public string ID { get; }
    public string Type { get; set; } = "";
    public BiomObject Biom { get; set; }
    public Resource Resource { get; private set; }
    // TODO: Сделать, чтобы присвоение было приватным
    public Chunk Chunk { get; set; }
    public GenerateMeshForTile GenerateMesh { get; private set; }
    
    public List<Tile> Neighbours { get; private set; } = new();

    public int Height
    {
        get => GenerateMesh.Height;
        set {
            if (GenerateMesh.Height == value) return;
            if (value < 0) value = 0;

            GenerateMesh.Height = value;
            
            if (Chunk != null) Chunk.UpdateMesh();
        }
    }
    
    public Tile(Chunk chunk, Point center)
    {
        _neighbourCenters = new List<Point>();
        ID = Guid.NewGuid().ToString();
        _center = center;
        GenerateMesh = new GenerateMeshForTile(this, _center);
        Chunk = chunk;
        icosahedronFaces = center.GetOrderedFaces();
        
        StoreNeighbourCenters(icosahedronFaces);
    }

    public void ResolveNeighbourTiles(Dictionary<string, Tile> allTiles)
    {
        _neighboursId ??= _neighbourCenters.Select(center => center.ID).ToList();
        
        foreach (var nowID in _neighboursId)
        {
            Neighbours.Add(allTiles[nowID]);
        }
    }
    
    private void StoreNeighbourCenters(List<Face> icosahedronFaces)
    {
        icosahedronFaces.ForEach(face =>
        {
            face.GetOtherPoints(_center).ForEach(point =>
            {
                if (_neighbourCenters.FirstOrDefault(centerPoint => centerPoint.ID == point.ID) == null)
                {
                    _neighbourCenters.Add(point);
                }
            });
        });
    }
    
    public void CreateHex() => GenerateMesh.BuildHex();
    public List<MeshDetails> GetMesh() =>  GenerateMesh.GetAllMeshes();

    public void AddResource(Resource resource)
    {
        Resource = resource;
        Type = resource.type;
    }
    
    public void AddBuilding(GameObject building)
    {
        RemoveObjects();
        var rotation = Quaternion.FromToRotation(Vector3.up, GenerateMesh.Normalize);
        building.transform.SetPositionAndRotation(GenerateMesh.Center, rotation);
        
        var positionZ = building.transform.TransformVector(Vector3.forward);
        var to = GenerateMesh._details.Points[0];
        
        foreach (var item in GenerateMesh._details.Points)
        {
            if ((to.Position - positionZ).sqrMagnitude > (item.Position - positionZ).sqrMagnitude)
            {
                to = item;
            }
        }
        
        var normalPositionZ = to.Position - GenerateMesh._details.Center.Position;
        rotation = Quaternion.FromToRotation(positionZ, normalPositionZ) * rotation;
        building.transform.SetPositionAndRotation(GenerateMesh.Center - GenerateMesh.Normalize * 0.05f, rotation);
    }
    
    public void AddObject(string type)
    {
        AddObject(type, GenerateMesh._details.Points.Count == 5 ? GetLerpPositionForPentagon() : GetLerpPositionForHexagon());
    }
    
    public void AddObject(string type, Vector3 position)
    {
        var gameObject = Object.Instantiate(HexMetrics.objects[type].Prefab, Chunk.transform);
        var rotation = Quaternion.LookRotation(GenerateMesh.Normalize) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
        gameObject.transform.SetPositionAndRotation(position, rotation);
        _objects.Add(gameObject);
        Chunk.AddObject(gameObject);
    }

    public void RemoveObjects()
    {
        foreach (var @object in _objects) GameObject.Destroy(@object.gameObject);
        _objects.Clear();
    }
    private Vector3 GetLerpPositionForHexagon() => GetLerpPosition(
        0, 1,
        5, 4,
        2, 1,
        3, 4
    );
    
    private Vector3 GetLerpPositionForPentagon() => GetLerpPosition(
        0, 1,
        3, 4,
        2, 1,
        3, 4
    );

    private Vector3 GetLerpPosition(int p11, int p12, int p21, int p22, int p31, int p32, int p41, int p42)
    {
        float x = UnityEngine.Random.Range(0, 2f), y = UnityEngine.Random.Range(0, 1f);
        Vector3 first, second;
        
        if (x <= 1f)
        {
            first = Vector3.Lerp(GenerateMesh._details.Points[p11].Position, GenerateMesh._details.Points[p12].Position, x);
            second = Vector3.Lerp(GenerateMesh._details.Points[p21].Position, GenerateMesh._details.Points[p22].Position, x);
        }
        else
        {
            first = Vector3.Lerp(GenerateMesh._details.Points[p31].Position, GenerateMesh._details.Points[p32].Position, x - 1f);
            second = Vector3.Lerp(GenerateMesh._details.Points[p41].Position, GenerateMesh._details.Points[p42].Position, x - 1f);
        }

        return Vector3.Lerp(first, second, y);
    }
}

