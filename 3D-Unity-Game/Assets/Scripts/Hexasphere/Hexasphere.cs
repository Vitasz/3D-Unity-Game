using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class Hexasphere: MonoBehaviour
{
    public List<Tile> _tiles = new ();
    public bool generate = false;
    public HexSphereGenerator _mapGen;
    [Min(5f)]
    private float radius = 40f;
    [Range(1, 100)]
    public int divisions = 10;
    public CameraSphere CameraSphere;
    public Sun sun;
    public Tile ClickedTile = null;
    public LineRenderer BackLight;
    private float delta_height = 0;
    public GameObject InsideSphere, ChunkPrefab;
    private readonly Dictionary<Collider, Chunk> colliders = new();
    private readonly HashSet<Chunk> Loaded = new();
    [Range(0, 100)]
    public int Range = 1;
    Tile prevLoad = null;
    private readonly List<Chunk> _chunks = new ();

    public float Radius { get { return radius; } }
    public float DeltaHeight { get { return delta_height; } }
    public void Awake()
    {
        if (generate)
        {
            radius *= divisions / 10f;
            delta_height = radius / 300 * 40 / divisions;
            _tiles = new HexSphereMeshGen().CreateSphere(divisions);
            _mapGen.GenerateMap(_tiles.Count);
        }
        else Load("Saves/Save1.json");
        UnityEngine.Debug.Log("TOTAL TILES: " + _tiles.Count.ToString());
        InsideSphere.transform.localScale = new Vector3(radius - 0.1f, radius - 0.1f, radius - 0.1f);
        CameraSphere.zoomMin = radius / 2;
        CameraSphere.zoomMax = radius * 1.5f;
        CameraSphere.offset.z = radius * 1.5f;
        CameraSphere.zoomMin += 20 * delta_height;
    }

    public List<Tile> Tiles => _tiles;

    public void AddChunk(Collider col, Chunk chunk)
    {
        colliders[col] = chunk;
        _chunks.Add(chunk);
    }
    public Tile GetRandomTile() => _tiles[UnityEngine.Random.Range(0, _tiles.Count)];
    public Tile GetTile(int index) => _tiles[index];
    public void ClickOnTile(Tile tile)
    {
        Vector3[] positions = tile._generateMesh.GetPositions();
        Vector3 normal = tile._generateMesh.GetNormal();
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector3(positions[i].x + normal.x/100, positions[i].y + normal.y / 100, positions[i].z + normal.z / 100);
        }
        BackLight.positionCount = positions.Length;
        BackLight.SetPositions(positions);
        ClickedTile = tile;
    }
    public void DisableClicked()
    {
        BackLight.positionCount = 0;
        ClickedTile = null;
    }
    
    public void FixedUpdate()
    {
        Ray MyRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2));
        Physics.Raycast(MyRay, out RaycastHit hit, 100);
        Vector3 p = hit.point;
        Collider col = hit.collider;
        Tile ans = null;
        if (col == null) return;
        if (!colliders.ContainsKey(col))
        {
            float minDistance = float.MaxValue;
            foreach (Tile tile in _tiles)
            {
                Vector3 now = tile._generateMesh.GetCenter();
                if (Vector3.Distance(now, p) < minDistance)
                {
                    minDistance = Vector3.Distance(now, p);
                    ans = tile;
                }
            }
        }
        else ans = colliders[col].GetTile(p);
        if (ans == prevLoad) return;
        prevLoad = ans;
        HashSet<Tile> nowLoad = new() { ans };
        Queue<Tile> check = new();
        check.Enqueue(ans);
        HashSet<Chunk> toOn = new();
        for (int i = 0; i <= Range; i++)
        {
            int sizeQueue = check.Count;
            while(sizeQueue-->0)
            {
                Tile now = check.Dequeue();
                toOn.Add(now.chunk);
                foreach (Tile tile in now.Neighbours)
                {
                    if (!nowLoad.Contains(tile))
                    {
                        nowLoad.Add(tile);
                        check.Enqueue(tile);
                    }
                }
            }
        }
        HashSet<Chunk> toRemove = new();
        foreach (Chunk chunk in Loaded) {
            if (!toOn.Contains(chunk))
            {
                chunk.UnloadFromScene();
                toRemove.Add(chunk);
            }
            else toOn.Remove(chunk);
        }

        foreach (Chunk chunk in toRemove)
        {
           
            Loaded.Remove(chunk);
        }

        foreach (Chunk chunk in toOn)
        {
            chunk.LoadtoScene();
            Loaded.Add(chunk);
        }
    }
    
    public void Save()
    {
        List<SaveDataChunk> saves = _chunks.Select(chunk => chunk.Save()).ToList();
        SaveDataSphere save = new()
        {
            Chunks = saves,
            delta_height = delta_height,
            radius = radius
        };
        using StreamWriter sw = new("Saves/Save1.json");
        sw.Write(JsonUtility.ToJson(save));
    }
    public void Load(string path)
    {
        string loaded_data = File.ReadAllText(path);
        SaveDataSphere lod = JsonUtility.FromJson<SaveDataSphere>(loaded_data);
        delta_height = lod.delta_height;
        radius = lod.radius;

        
        Dictionary<string, Tile> allTiles = new();
        for (int i = 0; i < lod.Chunks.Count; i++)
        {
            Chunk x = Instantiate(ChunkPrefab, transform).GetComponent<Chunk>();
            x.Load(this, lod.Chunks[i]);
            x.UnloadFromScene();
            //Loaded.Add(x);
            foreach(Tile tile in x._tiles)
            {
                allTiles.Add(tile.ID, tile);
                _tiles.Add(tile);
            }
        }
        foreach(Tile tile in _tiles)
        {
            tile.ResolveNeighbourTiles(allTiles);
        }
        //string loaded_data = File.ReadAllText("Assets/Scripts/tiledata.json");
        //HexDetails lod = JsonUtility.FromJson<HexDetails>(loaded_data);
    }
   
}
[Serializable]
public struct SaveDataSphere
{
    public float delta_height;
    public float radius;
    public List<SaveDataChunk> Chunks;
}
