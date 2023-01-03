using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using UnityEditor.VersionControl;
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
    public GameObject ChunkPrefab;
    
    private readonly List<Chunk> chunks = new();
    private readonly HashSet<Tile> ground = new();
    public void GenerateMap(int cellsCount)
    {
        Random.State originalRandomState = Random.state;
        Random.InitState(seed);
        this.cellsCount = cellsCount;
        HashSet<Tile> tiles = new ();
        
        for (int i = 0; i < cellsCount; i++)
        {
            Tile tile = grid.GetTile(i);
            tile.WaterLevel = waterLevel;
            tile.MaximumHeight = heightMaximum;
            tiles.Add(tile);
        }
        
        CreateLand();
        SetTerraintype();


        GenerateResources(ground);
        GenerateChunks(tiles);
       
        BuildHexes();
        CreateObjects();
        CreateMesh();
        Random.state = originalRandomState;
    }
    private int RaiseTerrain(int chunkSize, int budget)
    {
        Tile start = grid.GetRandomTile();
        List<List<Tile>> queue = new () { new () };
        HashSet<Tile> was = new () { start };
        queue[0].Add(start);
        
        int now_cnt = 0;
        int rise = Random.value < highRiseProbability ? 2 : 1;
        while (queue.Count!=0 && queue[0].Count != 0 && chunkSize > now_cnt)
        {
            int originalHeight = queue[0][0].Height;
            int newHeight = queue[0][0].Height + rise;
            if (newHeight <= heightMaximum)
            {
                queue[0][0].Height = newHeight;
                if (newHeight >= waterLevel &&
                    originalHeight < waterLevel)
                {
                    if (--budget == 0)
                        break;
                }
                now_cnt++;
            }
            if (queue.Count == 1) queue.Add(new List<Tile>());
            foreach (Tile tile in queue[0][0].Neighbours)
            {
                if (was.Contains(tile)) continue;
                was.Add(tile);
                int searchHeuristic = Random.value < jitterProbability ? 1 : 0;
                queue[1 - searchHeuristic].Add(tile);
            }
            queue[0].RemoveAt(0);
            while (queue.Count!=0 && queue[0].Count == 0) queue.RemoveAt(0);
        }
        return budget;
    }
    private int SinkTerrain(int chunkSize, int budget)
    {
        Tile start = grid.GetRandomTile();
        List<List<Tile>> queue = new () { new () };
        HashSet<Tile> was = new () { start };
        queue[0].Add(start);

        int now_cnt = 0;
        int sink = Random.value < highRiseProbability ? 2 : 1;
        while (queue.Count != 0 && queue[0].Count != 0 && chunkSize > now_cnt)
        {
            int originalHeight = queue[0][0].Height;
            int newHeight = queue[0][0].Height - sink;
            if (newHeight >= heightMinimum)
            {
                queue[0][0].Height = newHeight;
                if (originalHeight >= waterLevel &&
                    newHeight < waterLevel)
                {
                    budget++;
                }
                now_cnt++;
            }
            if (queue.Count == 1) queue.Add(new List<Tile>());
            foreach (Tile tile in queue[0][0].Neighbours)
            {
                if (was.Contains(tile)) continue;
                was.Add(tile);
                int searchHeuristic = Random.value < jitterProbability ? 1 : 0;
                queue[1 - searchHeuristic].Add(tile);
            }
            queue[0].RemoveAt(0);
            while (queue.Count != 0 && queue[0].Count == 0) queue.RemoveAt(0);
        }
        return budget;
    }
    private void CreateLand()
    {
        int landBudget = Mathf.RoundToInt(cellsCount * landPercentage * 0.01f);
        while (landBudget > 0)
        {
            int chunkSize = Random.Range(chunkSizeMin, chunkSizeMax + 1);
            if (Random.value < sinkProbability)
            {
                landBudget = SinkTerrain(chunkSize, landBudget);
            }
            else
            {
                landBudget = RaiseTerrain(
                    chunkSize, landBudget
                    );
            }

        }
    }
    private void SetTerraintype()
    {
        Dictionary<int, List<string>> types = new();
        foreach(var x in HexMetrics.tiles.Values)
        {
            for (int i = x.minHeight; i <= x.maxHeight; i++)
            {
                if (!types.ContainsKey(i)) types[i] = new();
                types[i].Add(x.type);
            }
        }
        for (int i = 0; i < cellsCount; i++)
        {
            Tile tile = grid.GetTile(i);
            
            int nowHeight = tile.Height;
            if (!types.ContainsKey(nowHeight)) continue;
            int index = Random.Range(0, types[nowHeight].Count);
            tile._type = types[nowHeight][index];
            if (!HexMetrics.tiles[tile._type].isLiquid) ground.Add(tile);
        }
        
    }
    private void BuildHexes()
    {
        for (int i = 0; i < cellsCount; i++)
        {
            Tile tile = grid.GetTile(i);
            tile.CreateHex();
        }
    }
    private void CreateObjects()
    {
        Dictionary<string, HashSet<ObjectOnScene>> tilesAndObjects = new ();
        foreach(ObjectOnScene @object in HexMetrics.objects.Values)
        {
            foreach(var type in @object.Spawn)
            {
                if (!tilesAndObjects.ContainsKey(type)) tilesAndObjects[type] = new();
                tilesAndObjects[type].Add(@object);
            }
        }
        UnityEngine.Debug.Log(ground.Count);
        foreach(Tile tile in ground)
        {
            if (tile.isOre || tile.Neighbours.Count==5 || !tilesAndObjects.ContainsKey(tile._type)) continue;

            int cnt = (int)(Random.value * 10) % 5;

            for (int i = 0; i < cnt; i++)
            {
                float rand = Random.value;
                int index = (int)(rand * 2 * tilesAndObjects[tile._type].Count) % tilesAndObjects[tile._type].Count;
                ObjectOnScene now = tilesAndObjects[tile._type].ElementAt(index);
                if (Random.value < now.chance)
                {
                    UnityEngine.Debug.Log("HERE");
                    tile.AddObject(tilesAndObjects[tile._type].ElementAt(index).type);
                }
            }
        }
    }
    private void GenerateResources(HashSet<Tile> Ground)
    {
        HashSet<Tile> copyGround = new();
        foreach (Tile tile in Ground) copyGround.Add(tile);
        int now = 0;
        Dictionary<string, OreObject> resourcesAndChances = new();
        foreach(var x in HexMetrics.ores.Values)
        {
            resourcesAndChances.Add(x.type, x);
        }
        while (copyGround.Count!=0)
        {
            Tile startTile = copyGround.ElementAt(Random.Range(0, copyGround.Count));

            string type = resourcesAndChances.ElementAt(now % resourcesAndChances.Count).Key;
           
            if (Random.value * 100 > resourcesAndChances[type].chance && now >= resourcesAndChances.Count)
            {
                int x = Random.Range(resourcesAndChances[type].minSize, resourcesAndChances[type].maxSize);
                while (x-- > 0 && copyGround.Count != 0)
                {
                    copyGround.Remove(startTile);
                    if (copyGround.Count!=0)startTile = copyGround.ElementAt(Random.Range(0, copyGround.Count));
                }
                continue;
            }
            int cntTiles = Random.Range(resourcesAndChances[type].minSize, resourcesAndChances[type].maxSize);
            int resourceCount = Random.Range(resourcesAndChances[type].minCount, resourcesAndChances[type].maxCount);
            int resoursePerTile = resourceCount / cntTiles;
            Queue<Tile> queue = new () ;
            queue.Enqueue(startTile);
            
            now += 1;
            while (cntTiles != 0 && queue.Count != 0)
            {
                cntTiles--;
                startTile = queue.Dequeue();
                copyGround.Remove(startTile);
                Resourse nowRes = new (type, resoursePerTile);
                startTile.AddResourse(nowRes);
                foreach(Tile tile in startTile.Neighbours)
                {
                    if (!resourcesAndChances.ContainsKey(tile._type))queue.Enqueue(tile);
                }
            }
        }
    }

    private void GenerateChunks(HashSet<Tile> tiles) {
        while(tiles.Count != 0)
        {
            Chunk chunk = Instantiate(ChunkPrefab, grid.transform).GetComponent<Chunk>();
            chunks.Add(chunk);
            
            chunk.Sphere = grid;
            chunk.UnloadFromScene();
            grid.AddChunk(chunk._meshCollider, chunk);
            Tile start = tiles.First();
            tiles.Remove(start);
            chunk.AddTile(start);
            
            List<Tile> neighbors = new ();
           
            int cnt = 1;

            foreach (Tile tile in start.Neighbours)
            {
                if (tiles.Contains(tile) && cnt < tilesInChunk && start._type == tile._type)
                {
                    neighbors.Add(tile);
                    cnt++;
                    tiles.Remove(tile);
                    chunk.AddTile(tile);
                }
            }
            
            while(tiles.Count != 0 && neighbors.Count!=0)
            {
                Tile now = neighbors[0];
                neighbors.RemoveAt(0);
                foreach (Tile tile in now.Neighbours)
                {
                    if (tiles.Contains(tile)  && cnt < tilesInChunk && tile._type == now._type)
                    {
                        if (now._type == tile._type)
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
        foreach(Chunk chunk in chunks)
        {
            chunk.UpdateMesh();
        }
    }
}
