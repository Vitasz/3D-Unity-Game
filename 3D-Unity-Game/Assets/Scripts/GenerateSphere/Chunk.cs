using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    //public GameObject mesh;
    public MeshFilter _meshFilter;
    public MeshCollider _meshCollider;
    public MeshRenderer _meshRenderer;
    //public LineRenderer _lineRenderer;
    private List<Tile> _tiles = new List<Tile>();
    public Hexasphere Sphere;
    public void AddTile(Tile details)
    {
        _tiles.Add(details);
        details.chunk = this;
    }
    public void OnMouseDrag() => Sphere.CameraSphere.RotateAround();
    public void GenerateMesh()
    {
        Dictionary<Point, int> points = new ();
        List<Point> vertices = new ();
        
        List<Color> colors = new ();
        Mesh mesh = new();
        List<Vector2> uvs = new();
        List<List<int>> triangles = new();
        List<MeshDetails> allMeshes = new();
        foreach (Tile tile in _tiles)
        {
            triangles.Clear();
            var details  = tile.GetMesh();
            allMeshes.AddRange(details);
        }
        List<Material> materials = new();
        mesh.subMeshCount = allMeshes.Count;
        for (int i = 0; i < allMeshes.Count; i++)
        {
            materials.Add(allMeshes[i].material);
            List<int> nowTris = new();   
            for (int j = 0; j < allMeshes[i].Vertices.Count; j++)
            {
                if (!points.ContainsKey(allMeshes[i].Vertices[j]))
                {
                    uvs.Add(allMeshes[i].Uvs[j]);
                    points.Add(allMeshes[i].Vertices[j], points.Count);
                    vertices.Add(allMeshes[i].Vertices[j]);
                    colors.Add(allMeshes[i].Colors[j]);
                }
            }
            allMeshes[i].Triangles.ForEach(face =>
            {
                face.Points.ForEach(point => nowTris.Add(points[point]));
            });
            uvs.AddRange(mesh.uv);
            triangles.Add(nowTris);
        }
        mesh.vertices = vertices.Select(point => point.Position).ToArray();
        mesh.colors = colors.ToArray();
         mesh.SetUVs(0, uvs.ToArray());
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            mesh.SetTriangles(triangles[i].ToArray(), i);
        }
        _meshFilter.mesh = mesh;
        _meshRenderer.materials = materials.ToArray();
        _meshCollider.sharedMesh = mesh;
    }
}
