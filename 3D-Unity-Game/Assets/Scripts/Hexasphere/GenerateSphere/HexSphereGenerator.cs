using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using Unity.VisualScripting;
//using UnityEditor.VersionControl;
using UnityEngine;


public class HexSphereGenerator : MonoBehaviour
{
    public int seed;
    
    public bool generateResources;

    public Hexasphere grid;
    [Range(0f, 0.5f)]
    public float jitterProbability = 0.25f;
    [Range(20, 200)]
    public int chunkSizeMin = 30;
    [Range(20, 200)]
    public int chunkSizeMax = 100;
    [Range(0, 100)]
    public int landPercentage = 50;
    private int cellsCount = 0;
    [Range(1, 10)]
    public int waterLevel = 3;
    [Range(0f, 1f)]
    public float highRiseProbability = 0.25f;
    [Range(0f, 0.4f)]
    public float sinkProbability = 0.2f;

    //[Range(-4, 0)]
    //public int heightMinimum = 0;

    [Range(20, 100)]
    public int heightMaximum = 6;
    [Range(1, 1000)]
    public int tilesInChunk = 200;
    public GameObject сhunkPrefab;

    private int heightMaximumAfterCreate = 0;
    private readonly List<Chunk> _chunks = new();
    private readonly HashSet<Tile> _ground = new();
    public void GenerateMap(int cellsCount)
    {
        Random.State originalRandomState = Random.state;
        Random.InitState(seed);
        this.cellsCount = cellsCount;
        HashSet<Tile> tiles = new ();
        
        for (int i = 0; i < cellsCount; i++)
        {
            Tile tile = grid.Tiles[i];
            tiles.Add(tile);
        }
        
        CreateLand();

        foreach (var tile in tiles) 
            heightMaximumAfterCreate = System.Math.Max(heightMaximumAfterCreate, tile.Height);

        SetTerrainType();


        if (generateResources) 
            GenerateResources(_ground);
        GenerateChunks(tiles);
       
        BuildHexes();
        CreateObjects();
        CreateMesh();
        
        Random.state = originalRandomState;
    }

    private int RaiseTerrain(int chunkSize, int budget) => UpdateTerrain(chunkSize, budget);

    private int SinkTerrain(int chunkSize, int budget) => UpdateTerrain(chunkSize, budget, false);

    private int UpdateTerrain(int chunkSize, int budget, bool up = true)
    {
        var start = grid.GetRandomTile();
        List<List<Tile>> queue = new () { new () };
        HashSet<Tile> was = new () { start };
        queue[0].Add(start);
        
        var cnt = 0;
        var rise = Random.value < highRiseProbability ? 2 : 1;
        
        while (queue.Count != 0 && queue[0].Count != 0 && chunkSize > cnt)
        {
            var originalHeight = queue[0][0].Height;
            var newHeight = queue[0][0].Height + rise;
            
            if (newHeight <= heightMaximum)
            {
                queue[0][0].Height = newHeight;
                
                if (newHeight >= waterLevel && originalHeight < waterLevel)
                {
                    if (up && --budget == 0)
                        break;

                    if (!up)
                        ++budget;
                }
                
                cnt++;
            }
            
            if (queue.Count == 1) queue.Add(new List<Tile>());
            
            foreach (var tile in queue[0][0].Neighbours)
            {
                if (was.Contains(tile)) continue;
                
                was.Add(tile);
                var searchHeuristic = Random.value < jitterProbability ? 1 : 0;
                queue[1 - searchHeuristic].Add(tile);
            }
            
            queue[0].RemoveAt(0);
            while (queue.Count!=0 && queue[0].Count == 0) queue.RemoveAt(0);
        }
        
        return budget;
    }
    
    private void CreateLand()
    {
        var landBudget = Mathf.RoundToInt(cellsCount * landPercentage * 0.01f);
        
        while (landBudget > 0)
        {
            var chunkSize = Random.Range(chunkSizeMin, chunkSizeMax + 1);
            
            if (Random.value < sinkProbability)
                landBudget = SinkTerrain(chunkSize, landBudget);
            else
                landBudget = RaiseTerrain(chunkSize, landBudget);

        }
    }
    
    private void SetTerrainType()
    {
        Dictionary<int, List<BiomObject>> types = new();
        BiomObject water = default;
        foreach(var x in HexMetrics.Bioms.Values)
        {
            if (x.minHeightPercent == 0)
            {
                water = x;
                continue;
            }
            for (var i = (int)((x.minHeightPercent * (heightMaximumAfterCreate + waterLevel)) * 1f / 100f); 
                i <= x.maxHeightPercent * (heightMaximumAfterCreate + waterLevel) * 1f / 100f; i++)
            {
                if (!types.ContainsKey(i)) types[i] = new();
                types[i].Add(x);

            }
        }
        for (var i = 0; i < cellsCount; i++)
        {
            var tile = grid.Tiles[i];
            
            var nowHeight = tile.Height;
            if (tile.Height <= waterLevel)
            {
                tile.Height = waterLevel;
                tile.Type = water.type;
                tile.Biom = water;
                continue;
            }
            if (!types.ContainsKey(nowHeight)) UnityEngine.Debug.LogError("No biom in height: " + nowHeight.ToString());


            var index = Random.Range(0, types[nowHeight].Count);
            tile.Type = types[nowHeight][index].type;
            tile.Biom = types[nowHeight][index];
            if (!HexMetrics.Bioms[tile.Type].isLiquid) _ground.Add(tile);
        }
        
    }
    
    private void BuildHexes()
    {
        for (var i = 0; i < cellsCount; i++)
        {
            var tile = grid.Tiles[i];
            tile.CreateHex();
        }
    }
    
    private void CreateObjects()
    {
        foreach(var tile in _ground)
        {
            if (tile.Resource != default) continue;
            var biom = tile.Biom;
            var cnt = Random.Range(0, biom.maxObjects);

            for (var i = 0; i < cnt; i++)
            {
                var rand = Random.value;
                var index = (int)(rand * 2 * biom.objects.Count) % biom.objects.Count;
                var now = biom.objects.ElementAt(index);
                tile.AddObject(now.Type);
            }
        }
    }
    
    private void GenerateResources(HashSet<Tile> Ground)
    {
        HashSet<Tile> copyGround = new();
        foreach (var tile in Ground) copyGround.Add(tile);
        var now = 0;
        var resourcesAndChances = HexMetrics.Ores.Values.ToDictionary(x => x.type);

        while (copyGround.Count != 0)
        {
            var startTile = copyGround.ElementAt(Random.Range(0, copyGround.Count));

            var type = resourcesAndChances.ElementAt(now % resourcesAndChances.Count).Key;
           
            if (Random.value * 100 > resourcesAndChances[type].chance && now >= resourcesAndChances.Count)
            {
                var x = Random.Range(resourcesAndChances[type].minSize, resourcesAndChances[type].maxSize);
                
                while (x-- > 0 && copyGround.Count != 0)
                {
                    copyGround.Remove(startTile);
                    if (copyGround.Count!=0)startTile = copyGround.ElementAt(Random.Range(0, copyGround.Count));
                }
                
                continue;
            }
            
            var cntTiles = Random.Range(resourcesAndChances[type].minSize, resourcesAndChances[type].maxSize);
            var resourceCount = Random.Range(resourcesAndChances[type].minCount, resourcesAndChances[type].maxCount);
            var resourcePerTile = resourceCount / cntTiles;
            Queue<Tile> queue = new ();
            queue.Enqueue(startTile);
            
            now += 1;
            while (cntTiles != 0 && queue.Count != 0)
            {
                cntTiles--;
                startTile = queue.Dequeue();
                copyGround.Remove(startTile);
                
                Resource nowRes = new (type, resourcePerTile, resourcesAndChances[type].drop);
                startTile.AddResource(nowRes);
                
                foreach(var tile in startTile.Neighbours)
                {
                    if (!resourcesAndChances.ContainsKey(tile.Type)) 
                        queue.Enqueue(tile);
                }
            }
        }
    }

    private void GenerateChunks(ICollection<Tile> tiles) {
        while(tiles.Count != 0)
        {
            var chunk = Instantiate(сhunkPrefab, grid.transform).GetComponent<Chunk>();
            _chunks.Add(chunk);
            
            chunk.sphere = grid;
            chunk.UnloadFromScene();
            grid.AddChunk(chunk.MeshCollider, chunk);
            var start = tiles.First();
            tiles.Remove(start);
            chunk.AddTile(start);
            
            List<Tile> neighbors = new ();
           
            var cnt = 1;

            foreach (var tile in start.Neighbours)
            {
                if (tiles.Contains(tile) && cnt < tilesInChunk && start.Type == tile.Type)
                {
                    neighbors.Add(tile);
                    cnt++;
                    tiles.Remove(tile);
                    chunk.AddTile(tile);
                }
            }
            
            while(tiles.Count != 0 && neighbors.Count!=0)
            {
                var now = neighbors[0];
                neighbors.RemoveAt(0);
                
                foreach (var tile in now.Neighbours)
                {
                    if (tiles.Contains(tile)  && cnt < tilesInChunk && tile.Type == now.Type)
                    {
                        if (now.Type == tile.Type)
                        {
                            neighbors.Add(tile);
                            cnt++;
                            tiles.Remove(tile);
                            chunk.AddTile(tile);
                        }
                    }
                }
            }
            
        }


    }
    
    private void CreateMesh()
    {
        foreach(var chunk in _chunks)
        {
            chunk.UpdateMesh();
        }
    }
}
