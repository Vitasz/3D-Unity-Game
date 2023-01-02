using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkOfClouds : MonoBehaviour
{
    List<Cloud> clouds = new();
    public CloudController controller;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public void Start()
    {
        StartCoroutine(LevelUP());
    }
    public void AddCloud(Cloud cloud)
    {
        clouds.Add(cloud);
        UpdateMesh();
    }
    public void UpdateMesh()
    {

        Dictionary<Point, int> points = new();
        List<Point> vertices = new();

        Mesh mesh = new();
        List<Vector2> uvs = new();
        List<MeshDetails> allMeshes = new();
        foreach (Cloud tile in clouds)
        {

            var details = tile.GetMesh();
            allMeshes.Add(details);
        }
        List<Material> materials = new();
        int counter = 0;
        Dictionary<Material, List<int>> meshes = new();
        for (int i = 0; i < allMeshes.Count; i++)
        {
            List<int> nowTris = new();
            points.Clear();
            for (int j = 0; j < allMeshes[i].Vertices.Count; j++)
            {
                if (!points.ContainsKey(allMeshes[i].Vertices[j]))
                {
                    uvs.Add(allMeshes[i].Uvs[j]);
                    points.Add(allMeshes[i].Vertices[j], counter);
                    vertices.Add(allMeshes[i].Vertices[j]);
                    counter += 1;
                }
            }
            allMeshes[i].Triangles.ForEach(face =>
            {
                face.Points.ForEach(point => nowTris.Add(points[point]));
            });
            //uvs.AddRange(mesh.uv);
            if (!meshes.ContainsKey(allMeshes[i].material)) meshes[allMeshes[i].material] = new();
            meshes[allMeshes[i].material].AddRange(nowTris);
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
        //mesh.RecalculateTangents();
        meshFilter.mesh = mesh;
        meshRenderer.materials = materials.ToArray();
       // _meshCollider.sharedMesh = mesh;
    }
    private IEnumerator LevelUP()
    {
        while (true)
        {

            if (clouds.Count != 0)
            {
                Cloud cloud = clouds[Random.Range(0, clouds.Count)];
                cloud.LevelUp(1);
                UpdateMesh();
            }
            yield return new WaitForSeconds(Random.value * 100);
        }
    }
}
