using System.Collections.Generic;
using UnityEngine;


public class Hexasphere : MonoBehaviour
{
    [Range(1, 100)]
    public int divisions = 10;
    
    [Range(0, 100)]
    public int range = 1;
    
    public bool generate;
    public HexSphereGenerator mapGen;
    public CameraSphere cameraSphere;
    public Tile ClickedTile;
    public LineRenderer backLight;
    public GameObject insideSphere, chunkPrefab;
    
    private Tile _prevLoad;
    private readonly List<Chunk> _chunks = new ();
    private readonly Dictionary<Collider, Chunk> _colliders = new();
    private readonly HashSet<Chunk> _loaded = new();
    
    public float Radius { get; private set; } = 40f;
    public List<Tile> Tiles { get; private set; } = new ();
    public float DeltaHeight { get; private set; }

    public void Awake()
    {
        if (generate)
        {
            Radius *= divisions / 8f;
            DeltaHeight = Radius / 300 * 40 / divisions;
            Tiles = new HexSphereMeshGen().CreateSphere(divisions);
            mapGen.GenerateMap(Tiles.Count);
        }
        // Load save
        // else Load("Saves/Save1.json");

        insideSphere.transform.localScale = new Vector3(Radius - 0.1f, Radius - 0.1f, Radius - 0.1f);
        cameraSphere.zoomMin = Radius / 2;
        cameraSphere.zoomMax = Radius * 1.5f;
        cameraSphere.offset.z = Radius * 1.5f;
        cameraSphere.zoomMin += 20 * DeltaHeight;
    }

    public void AddChunk(Collider collider, Chunk chunk)
    {
        _colliders[collider] = chunk;
        _chunks.Add(chunk);
    }
    
    public Tile GetRandomTile() => Tiles[UnityEngine.Random.Range(0, Tiles.Count)];
    
    public void ClickOnTile(Tile tile)
    {
        var positions = tile.GenerateMesh.GetPositions();
        var normal = tile.GenerateMesh.Normalize;
        
        for (var i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector3(
                positions[i].x + normal.x / 100, 
                positions[i].y + normal.y / 100, 
                positions[i].z + normal.z / 100
            );
        }
        
        backLight.positionCount = positions.Length;
        backLight.SetPositions(positions);
        ClickedTile = tile;
    }
    
    public void DisableClicked()
    {
        backLight.positionCount = 0;
        ClickedTile = null;
    }
    
    public void FixedUpdate()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Physics.Raycast(ray, out var hit, 100);
        
        Tile result = null;
        
        if (hit.collider == null) return;
        
        if (!_colliders.ContainsKey(hit.collider))
        {
            var minDistance = float.MaxValue;
            
            foreach (var tile in Tiles)
            {
                var now = Vector3.Distance(tile.GenerateMesh.Center, hit.point);
                
                if (now < minDistance)
                {
                    minDistance = now;
                    result = tile;
                }
            }
        }
        else 
            result = _colliders[hit.collider].GetTile(hit.point);
        
        if (result == _prevLoad) 
            return;
        
        _prevLoad = result;
        
        HashSet<Tile> nowLoad = new() { result };
        Queue<Tile> check = new();
        HashSet<Chunk> toOn = new();
        
        check.Enqueue(result);

        for (var i = 0; i <= range; i++)
        {
            var sizeQueue = check.Count;
            
            while(sizeQueue-- > 0)
            {
                var now = check.Dequeue();
                toOn.Add(now.Chunk);
                
                foreach (var tile in now.Neighbours)
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
        
        foreach (var chunk in _loaded) {
            if (!toOn.Contains(chunk))
            {
                chunk.UnloadFromScene();
                toRemove.Add(chunk);
            }
            else 
                toOn.Remove(chunk);
        }

        foreach (var chunk in toRemove)

            _loaded.Remove(chunk);

        foreach (var chunk in toOn)
        {
            chunk.LoadToScene();
            _loaded.Add(chunk);
        }
    }
}