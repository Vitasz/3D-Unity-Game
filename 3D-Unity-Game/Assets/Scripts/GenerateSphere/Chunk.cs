using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter _meshFilter;
    public MeshCollider _meshCollider;
    public MeshRenderer _meshRenderer;
    private HashSet<Tile> _tiles = new();
    public Hexasphere Sphere;
    public void AddTile(Tile details)
    {
        _tiles.Add(details);
        details.chunk = this;
    }
    
    public void OnMouseDrag() => Sphere.CameraSphere.RotateAround();
    public void OnMouseDown()
    {
        Ray MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(MyRay, out RaycastHit hit, 100);
        Vector3 p = hit.point;
        Sphere.ClickOnTile(GetTile(p));
    }
    public Tile GetTile(Vector3 position)
    {
        float minDistance = float.MaxValue;
        Tile ans = null;
        foreach (Tile tile in _tiles)
        {
            Vector3 now = tile._generateMesh.GetCenter();
            if (Vector3.Distance(now, position) < minDistance)
            {
                minDistance = Vector3.Distance(now, position);
                ans = tile;
            }
        }
        return ans;
    }
    public void UpdateMesh()
    {
        
        Dictionary<Point, int> points = new ();
        List<Point> vertices = new ();
       
        Mesh mesh = new();
        List<Vector2> uvs = new();
        List<MeshDetails> allMeshes = new();
        foreach (Tile tile in _tiles)
        {
           
            var details  = tile.GetMesh();
            allMeshes.AddRange(details);
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
        foreach(var x in meshes)
        {
            mesh.SetTriangles(x.Value.ToArray(), materials.Count);
            materials.Add(x.Key);
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //mesh.RecalculateTangents();
        _meshFilter.mesh = mesh;
        _meshRenderer.materials = materials.ToArray();
        _meshCollider.sharedMesh = mesh;
    }
}
