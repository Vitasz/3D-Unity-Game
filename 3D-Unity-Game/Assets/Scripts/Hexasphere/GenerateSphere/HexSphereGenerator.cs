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
    [Range(0f, 0.5f)]
    public float jitterProbability = 0.25f;
    public Hexasphere grid;
    [Range(20, 200)]
    public int chunkSizeMin = 30;
    [Range(20, 200)]
    public int chunkSizeMax = 100;
    [Range(0, 100)]
    public int landPercentage = 50;
    private int cellsCount = 0;
    [Range(1, 5)]
    public int waterLevel = 3;
    [Range(0f, 1f)]
    public float highRiseProbability = 0.25f;
    [Range(0f, 0.4f)]
    public float sinkProbability = 0.2f;
    [Range(-4, 0)]
    public int heightMinimum = 0;

    [Range(5, 10)]
    public int heightMaximum = 6;
    [Range(1, 1000)]
    public int tilesInChunk = 200;
    public GameObject сhunkPrefab;
    
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
        SetTerrainType();


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
        Dictionary<int, List<string>> types = new();
        
        foreach(var x in HexMetrics.tiles.Values)
        {
            for (var i = x.minHeight; i <= x.maxHeight; i++)
            {
                if (!types.ContainsKey(i)) types[i] = new();
                types[i].Add(x.type);
            }
        }
        
        for (var i = 0; i < cellsCount; i++)
        {
            var tile = grid.Tiles[i];
            
            var nowHeight = tile.Height;
            if (!types.ContainsKey(nowHeight)) continue;
            var index = Random.Range(0, types[nowHeight].Count);
            tile.Type = types[nowHeight][index];
            if (!HexMetrics.tiles[tile.Type].isLiquid) _ground.Add(tile);
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
        Dictionary<string, HashSet<ObjectOnScene>> tilesAndObjects = new ();
        
        foreach(var @object in HexMetrics.objects.Values)
        {
            foreach(var type in @object.Spawn)
            {
                if (!tilesAndObjects.ContainsKey(type)) tilesAndObjects[type] = new();
                tilesAndObjects[type].Add(@object);
            }
        }
        
        foreach(var tile in _ground)
        {
            if (tile.Resource != default || !tilesAndObjects.ContainsKey(tile.Type)) continue;

            var cnt = (int)(Random.value * 10) % 5;

            for (var i = 0; i < cnt; i++)
            {
                var rand = Random.value;
                var index = (int)(rand * 2 * tilesAndObjects[tile.Type].Count) % tilesAndObjects[tile.Type].Count;
                var now = tilesAndObjects[tile.Type].ElementAt(index);
                
                if (Random.value < now.Chance)
                    tile.AddObject(tilesAndObjects[tile.Type].ElementAt(index).Type);
            }
        }
    }
    
    private void GenerateResources(HashSet<Tile> Ground)
    {
        HashSet<Tile> copyGround = new();
        foreach (var tile in Ground) copyGround.Add(tile);
        var now = 0;
        var resourcesAndChances = HexMetrics.ores.Values.ToDictionary(x => x.type);

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
