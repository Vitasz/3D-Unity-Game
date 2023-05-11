using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter MeshFilter; 
    public MeshCollider MeshCollider;
    public MeshRenderer MeshRenderer;
    public HashSet<Tile> Tiles { get; private set; } = new();
    public Hexasphere sphere;
    public GameObject buildings, objects;
    public string ID { get; private set; }
    
    public void AddTile(Tile details)
    {
        Tiles.Add(details);
        details.Chunk = this;
    }
    
    public void Awake()
    {
        if (ID == "") 
            ID = Guid.NewGuid().ToString();
    }

    public void UnloadFromScene()
    {
        MeshRenderer.enabled = false;
        objects.SetActive(false);
        buildings.SetActive(false);
    }
    
    public void LoadToScene()
    {
        MeshRenderer.enabled = true;
        objects.SetActive(true);
        buildings.SetActive(true);
    }

    public void OnMouseDrag() => sphere.cameraSphere.RotateAround();
    
    public void OnMouseDown()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out var hit, 10000);
        var point = hit.point;
        Tile clicked = GetTile(point);
        if (clicked != null)
            EventAggregator.ClickOnTile.Publish(GetTile(point));
    }
    
    public Tile GetTile(Vector3 position)
    {
        var minDistance = float.MaxValue;
        Tile result = null;
        
        foreach (var tile in Tiles)
        {
            var now = Vector3.Distance( tile.GenerateMesh.Center, position);
            if (now > 3 * Vector3.Distance(tile.GenerateMesh.GetPositions()[0], tile.GenerateMesh.Center))
            {
                continue;
            }
            if (now < minDistance)
            {
                minDistance = now;
                result = tile;
            }
        }
        
        return result;
    }
    
    public void AddObject(GameObject gameObject) => gameObject.transform.SetParent(objects.transform);
    
    public void UpdateMesh()
    {
        Dictionary<Point, int> points = new ();
        List<Point> vertices = new ();
       
        Mesh mesh = new();
        List<Vector2> uvs = new();
        List<MeshDetails> allMeshes = new();
        
        foreach (var tile in Tiles)
            allMeshes.AddRange(tile.GetMesh());

        List<Material> materials = new();
        var counter = 0;
        Dictionary<Material, List<int>> meshes = new();
        
        foreach (var item in allMeshes)
        {
            List<int> nowTris = new();
            points.Clear();
            
            for (var j = 0; j < item.Vertices.Count; j++)
            {
                if (!points.ContainsKey(item.Vertices[j]))
                {
                    uvs.Add(item.Uvs[j]);
                    points.Add(item.Vertices[j], counter);
                    vertices.Add(item.Vertices[j]);
                    counter += 1;
                }
            }
            
            item.Triangles.ForEach(face =>
            {
                face.Points.ForEach(point => nowTris.Add(points[point]));
            });
            
            if (!meshes.ContainsKey(item.Material)) meshes[item.Material] = new();
            
            meshes[item.Material].AddRange(nowTris);
        }
        
        mesh.subMeshCount = meshes.Count;
        mesh.vertices = vertices.Select(point => point.Position).ToArray();
        
        mesh.SetUVs(0, uvs.ToArray());
        foreach(var x in meshes)
        {
            mesh.SetTriangles(x.Value.ToArray(), materials.Count);
            materials.Add(x.Key);
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        MeshFilter.mesh = mesh;
        MeshRenderer.materials = materials.ToArray();
        MeshCollider.sharedMesh = mesh;
    }

}