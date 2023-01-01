using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindController : MonoBehaviour
{
    public Hexasphere Sphere;
    public GameObject CloudPrefab;
    Dictionary<Tile, Wind> winds = new();
    Dictionary<Tile, Cloud> clouds = new();
    
    private void Start()
    {
        //foreach(Tile tile in Sphere.Tiles)
        //    if (tile.Neighbours.Count!=5)
         //   CreateCloud(tile);
    }

    public void CreateWind(Tile from, Tile from_next)
    {
        Vector3 direction = (from_next._generateMesh._details.IcoCenter.Position - from._generateMesh._details.IcoCenter.Position).normalized;
        List<Tile> tiles = new() { from, from_next };
        float force = Random.value;
        while (true)
        {
            float minangle = float.MaxValue;
            Tile ans = null;
            foreach(Tile x in tiles[^1].Neighbours)
            {
                if (x.Neighbours.Count == 5) continue;
                Vector3 xdirection = (x._generateMesh._details.IcoCenter.Position - tiles[^1]._generateMesh._details.IcoCenter.Position).normalized;
                float angle = Mathf.Abs(Vector3.Angle(xdirection, direction));
                if (angle < minangle)
                {
                    minangle = angle;
                    ans = x;
                   
                }
            }
            if (tiles.Contains(ans))
            {
                tiles.Add(ans);
                break;
            }
            tiles.Add(ans);
            direction = (tiles[^1]._generateMesh._details.IcoCenter.Position - tiles[^2]._generateMesh._details.IcoCenter.Position).normalized;
        }
        Wind wind = new Wind(tiles, force%0.1f);
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            if (winds.ContainsKey(tiles[i]) && winds[tiles[i]] != null)
            {
                if (winds[tiles[i]].force < force)
                {
                    winds[tiles[i]] = wind;
                }
                else break;
            }
            else winds[tiles[i]] = wind;
        }

    }
    public Wind GetWind(Tile where)
    {
        if (!winds.ContainsKey(where)) return null;
        return winds[where];
    }
    private void CreateCloud(Tile where)
    {
        Cloud cloud = Instantiate(CloudPrefab, transform).GetComponent<Cloud>();
       
        cloud.now = where;
        cloud.controller = this;
        clouds[where] = cloud;
    }
    public void MoveCloud(Tile from, Tile to, Cloud cloud)
    {
        clouds[from] = null;
        if (clouds.ContainsKey(to) && clouds[to] != null)
        {
            clouds[to].LevelUp(cloud.level);
            Destroy(cloud.gameObject);
        }
        clouds[to] = cloud;
    }
}
