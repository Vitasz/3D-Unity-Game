using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public Tile now;
    public WindController controller;
    public int level = 1;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public Vector3 direction;
    //public MeshRenderer meshRenderer;
    public void Start()
    {
        LevelUp(0);
        //transform.position = now._generateMesh.GetCenter() * 1.1f;
        StartCoroutine(Tonext());

    }
    public IEnumerator Tonext()
    {
       
        while (true)
        {
            transform.position = new();
            meshFilter.mesh = CreateCloud(now);

            yield return new WaitForEndOfFrame();
        }
    }
    private Mesh CreateCloud(Tile tile)
    {
        Mesh mesh = new();
        List<Vector3> vertices = new();
        vertices.Add(tile._generateMesh._details.Center.Position * 1.1f);

        foreach (Point x in tile._generateMesh._details.Points)
        {
            vertices.Add(x.Position * 1.1f);
        }
        vertices.Add(tile._generateMesh._details.Center.Position * 1.2f);
        foreach (Point x in tile._generateMesh._details.Points)
        {
            vertices.Add(x.Position * 1.2f);
        }
        List<int> triangles = new();
        for (int i = 1; i < vertices.Count; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add((i + 1) % vertices.Count);
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        //mesh.RecalculateNormals();
        return mesh;
    }
    public void LevelUp(int to)
    {
        level += to;
        if (level >= 10) transform.GetComponent<MeshRenderer>().material = HexMetrics.UraganMaterial;
        else if (level >= 7) transform.GetComponent<MeshRenderer>().material = HexMetrics.StormCloudMaterial;
        else if (level >= 4) transform.GetComponent<MeshRenderer>().material = HexMetrics.RainyCloudMaterial;
        else if (level >= 1) transform.GetComponent<MeshRenderer>().material = HexMetrics.cloudMaterial;
    }
}
