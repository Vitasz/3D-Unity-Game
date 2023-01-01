using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind
{
    //List<Tile> direction = new List<Tile>();
    public float force;
    Dictionary<Tile, Tile> next = new Dictionary<Tile, Tile>();
    public Wind(List<Tile> direction, float force)
    {
        for (int i = 0; i < direction.Count-1; i++)
        {
            next[direction[i]] = direction[i+1];
        }
        this.force = force;
    }

    public (Tile next, float force) GetNext(Tile now)
    {
        if (!next.ContainsKey(now)) return (null, 0f);
        return (next[now], force);
    }
}
