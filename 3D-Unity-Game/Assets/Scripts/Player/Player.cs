using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Hexasphere Hexasphere;

    private Team _team;
    private Tile _position;
    private List<Tile> Way = new();
    void Start()
    {
        SetPosition(Hexasphere.Tiles[0]);
        StartCoroutine(Move());
    }
    void Awake()
    {
        EventAggregator.ClickOnTile.Subscribe(CreateWay);
    }

    
    private IEnumerator Move()
    {
        while (true)
        {
            if (Way.Count != 0)
            {
                SetPosition(Way[0]);
                Way.RemoveAt(0);
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForFixedUpdate();
        }
    }
    private void SetPosition(Tile tile)
    {
        _position = tile;
        transform.SetPositionAndRotation(_position.GenerateMesh.Center,
            Quaternion.FromToRotation(Vector3.up, tile.GenerateMesh.Normalize));
    }
    public void CreateWay(Tile to)
    {
        if (to == _position) return;
        Way = Hexasphere.FindWay(_position, to);
        Way.Add(to);
    }
}
