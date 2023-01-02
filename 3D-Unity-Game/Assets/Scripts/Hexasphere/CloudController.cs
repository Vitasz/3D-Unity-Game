using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public Hexasphere sphere;
    public float Radius = 1f;
    public int MinCloudsInChunkOfCloud = 4;
    public int MaxCloudsInChunkOfCloud = 4;
    public GameObject ChunkOfCloudsPrefab;
    private readonly HashSet<Tile> was = new();
    private readonly HashSet<ChunkOfClouds> chunks = new();
    public int maxClouds = 100;
    public Vector3 rotateAround = Vector3.up;
    [Range(1, 20)]
    public float speed;
    
    public void Start()
    {
        Radius = sphere.Radius * 1.2f;
        
    }
    public void FixedUpdate()
    {
       
        transform.Rotate(rotateAround, Time.deltaTime * speed);
        if (was.Count < maxClouds)
        {
            ChunkOfClouds chunkOfClouds = Instantiate(ChunkOfCloudsPrefab, transform).GetComponent<ChunkOfClouds>();
            chunkOfClouds.controller = this;
            int cnt = Random.Range(MinCloudsInChunkOfCloud, MaxCloudsInChunkOfCloud + 1);
            int nowIndex = Random.Range(0, sphere.Tiles.Count);
            if (was.Contains(sphere.Tiles[nowIndex])) return;
            Queue<Tile> next = new();
            next.Enqueue(sphere.Tiles[nowIndex]);
            while (cnt-- != 0)
            {
                Tile now = next.Dequeue();
                foreach (Tile x in now.Neighbours) next.Enqueue(x);
                if (was.Contains(now)) continue;
                was.Add(now);
                Cloud cloud = new(chunkOfClouds, now._generateMesh._details.IcoPoints, (int)(Random.value*10)%5);

                chunkOfClouds.AddCloud(cloud);
            }
            chunks.Add(chunkOfClouds);
        }
    }
}
