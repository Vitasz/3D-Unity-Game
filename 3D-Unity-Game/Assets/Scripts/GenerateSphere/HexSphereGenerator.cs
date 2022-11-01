using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
    [Range(5, 95)]
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
    public void GenerateMap(int cellsCount)
    {
        Random.State originalRandomState = Random.state;
        Random.InitState(seed);
        this.cellsCount = cellsCount;
        HashSet<Tile> tiles = new HashSet<Tile>();
        for (int i = 0; i < cellsCount; i++)
        {
            Tile tile = grid.GetTile(i);
            tile.WaterLevel = waterLevel;
            tiles.Add(tile);
        }
        CreateLand();
        SetTerraintype();
        GenerateChunks(tiles);
        Random.state = originalRandomState;
    }
    private int RaiseTerrain(int chunkSize, int budget)
    {
        Tile start = grid.GetRandomTile();
        List<List<Tile>> queue = new List<List<Tile>>() { new List<Tile>() };
        HashSet<Tile> was = new HashSet<Tile>() { start };
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
        List<List<Tile>> queue = new List<List<Tile>>() { new List<Tile>() };
        HashSet<Tile> was = new HashSet<Tile>() { start };
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
        for (int i = 0; i < cellsCount; i++)
        {
            Tile tile = grid.GetTile(i);
            tile.UpdateType();
            tile.CreateHex();
        }
        for (int i = 0; i < cellsCount; i++)
        {
            Tile tile = grid.GetTile(i);
            tile.BuildBridges();
            
        }
        for (int i = 0; i < cellsCount; i++)
        {
            Tile tile = grid.GetTile(i);
            tile.BuildTriangles();

        }
    }
    
    private void GenerateChunks(HashSet<Tile> tiles) {
        while(tiles.Count != 0)
        {
            Chunk chunk = Instantiate(ChunkPrefab, transform).GetComponent<Chunk>();
            chunk.Sphere = grid;
            Tile start = tiles.First();
            tiles.Remove(start);
            chunk.AddTile(start);
            List<Tile> neighbors = new List<Tile>();
            int cnt = 1;
            foreach (Tile tile in start.Neighbours)
            {
                if (tiles.Contains(tile) && cnt < tilesInChunk)
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
                    if (tiles.Contains(tile) && cnt < tilesInChunk)
                    {
                        neighbors.Add(tile);
                        cnt++;
                        tiles.Remove(tile);
                        chunk.AddTile(tile);
                    }
                }
            }
            chunk.GenerateMesh();
        }
    }
}
