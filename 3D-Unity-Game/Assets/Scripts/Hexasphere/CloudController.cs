using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public Hexasphere sphere;
    public int minCloudsInChunkOfCloud = 4;
    public int maxCloudsInChunkOfCloud = 4;
    public GameObject chunkOfCloudsPrefab;
    public int maxClouds = 100;
    public Vector3 rotateAround = Vector3.up;
    [Range(1, 20)] public float speed;
    
    private readonly HashSet<Tile> was = new();
    private readonly HashSet<ChunkOfClouds> chunks = new();

    public float Radius { get; private set; }

    public void Start()
    {
        Radius = sphere.Radius * 1.2f;
    }
    
    public void FixedUpdate()
    {
        transform.Rotate(rotateAround, Time.deltaTime * speed);
        
        if (was.Count < maxClouds)
        {
            var chunkOfClouds = Instantiate(chunkOfCloudsPrefab, transform).GetComponent<ChunkOfClouds>();
            chunkOfClouds.controller = this;
            
            var count = Random.Range(minCloudsInChunkOfCloud, maxCloudsInChunkOfCloud + 1);
            var nowIndex = Random.Range(0, sphere.Tiles.Count);
            
            if (was.Contains(sphere.Tiles[nowIndex])) return;
            
            Queue<Tile> next = new();
            next.Enqueue(sphere.Tiles[nowIndex]);
            
            while (count-- != 0)
            {
                var now = next.Dequeue();
                
                foreach (var x in now.Neighbours) 
                    next.Enqueue(x);
                
                if (was.Contains(now)) 
                    continue;
                
                was.Add(now);
                Cloud cloud = new(chunkOfClouds, now.GenerateMesh._details.IcoPoints, (int)(Random.value*10)%5);

                chunkOfClouds.AddCloud(cloud);
            }
            
            chunks.Add(chunkOfClouds);
        }
    }
}
