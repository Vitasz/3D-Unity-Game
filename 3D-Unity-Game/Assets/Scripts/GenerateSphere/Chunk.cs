using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    //public GameObject mesh;
    public MeshFilter _meshFilter;
    public MeshCollider _meshCollider;
    //public LineRenderer _lineRenderer;
    private List<Tile> _tiles = new List<Tile>();
    public Hexasphere Sphere;
    public void AddTile(Tile details)
    {
        _tiles.Add(details);
    }
    public void OnMouseDrag() => Sphere.CameraSphere.RotateAround();
    public void GenerateMesh()
    {
        Dictionary<Point, int> points = new Dictionary<Point, int>();
        List<Point> vertices = new List<Point>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        foreach (Tile tile in _tiles)
        {
            TileMeshDetails details = tile.GetMesh();
            for (int i = 0; i < details.Vertices.Count; i++)
            {
                Point point = details.Vertices[i];
                if (!points.ContainsKey(point))
                {
                    point.positionInMesh = vertices.Count;
                    points[point] = vertices.Count;
                    vertices.Add(point);
                    colors.Add(details.Colors[i]);
                }
            }
            
            details.Triangles.ForEach(point =>
            {
                triangles.Add(points[point]);
            });
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.Select(point => point.Position).ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
        /*_lineRenderer.positionCount = Points_for_lineRenderer.Count;
        _lineRenderer.SetPositions(details.Points_for_lineRenderer.ToArray());
        _lineRenderer.startColor = details.Color_for_lineRenderer;
        _lineRenderer.endColor = details.Color_for_lineRenderer;*/
        //_meshCollider.convex = true;
    }
}
