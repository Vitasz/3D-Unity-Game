using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkOfClouds : MonoBehaviour
{
    public CloudController controller;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    private List<Cloud> _clouds = new();
    
    public void Start()
    {
        StartCoroutine(LevelUP());
    }
    
    public void AddCloud(Cloud cloud)
    {
        _clouds.Add(cloud);
        UpdateMesh();
    }
    
    public void UpdateMesh()
    {
        Dictionary<Point, int> points = new();
        List<Point> vertices = new();

        Mesh mesh = new();
        List<Vector2> uvs = new();
        List<MeshDetails> allMeshes = new();
        
        foreach (var tile in _clouds)
        {

            var details = tile.GetMesh();
            allMeshes.Add(details);
        }
        
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
            
            if (!meshes.ContainsKey(item.Material) && item.Material != null) meshes[item.Material] = new();
            
            meshes[item.Material].AddRange(nowTris);
        }
        
        mesh.subMeshCount = meshes.Count;
        mesh.vertices = vertices.Select(point => point.Position).ToArray();

        mesh.SetUVs(0, uvs.ToArray());
        
        foreach (var x in meshes)
        {
            mesh.SetTriangles(x.Value.ToArray(), materials.Count);
            materials.Add(x.Key);
        }
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        meshFilter.mesh = mesh;
        meshRenderer.materials = materials.ToArray();
    }
    private IEnumerator LevelUP()
    {
        while (true)
        {
            if (_clouds.Count != 0)
            {
                var cloud = _clouds[Random.Range(0, _clouds.Count)];
                cloud.LevelUp(1);
                UpdateMesh();
            }
            
            yield return new WaitForSeconds(Random.value * 100);
        }
    }
}
