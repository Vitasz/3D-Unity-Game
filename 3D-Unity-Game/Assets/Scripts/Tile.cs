using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Tile : MonoBehaviour
{
    private Point _center;
    private List<Point> _neighbourCenters;
    private List<Face> icosahedronFaces;
    private List<Tile> _neighbours;
    private int _height = 0;
    public Type_of_Tiles _type;
    private Resourse resourse;
    public GenerateMeshForTile _generateMesh;
    public GameObject mesh;
    public MeshFilter _meshFilter;
    public MeshCollider _meshCollider;
    public LineRenderer _lineRenderer;
    private Hexasphere _sphere;
    public void CreateTile(Point center, float radius, float size, Hexasphere hexasphere)
    {
        _neighbourCenters = new List<Point>();
        _neighbours = new List<Tile>();

        SetRandomHeight();
        _center = center;
        size = Mathf.Max(0.01f, Mathf.Min(1f, size));
        _sphere = hexasphere;
       
        _generateMesh = new GenerateMeshForTile(_center, radius, size, _height);

        icosahedronFaces = center.GetOrderedFaces();
        StoreNeighbourCenters(icosahedronFaces);

    }
    public void OnMouseDrag() => _sphere.CameraSphere.RotateAround();
        
    public void ResolveNeighbourTiles(List<Tile> allTiles)
    {
        List<string> neighbourIds = _neighbourCenters.Select(center => center.ID).ToList();
        _neighbours = allTiles.Where(tile => neighbourIds.Contains(tile._center.ID)).ToList();
        _generateMesh.setNeighbours(_neighbours);
    }
        
    public override string ToString()
    {
        return $"{_center.Position.x},{_center.Position.y},{_center.Position.z}";
    }

    private void StoreNeighbourCenters(List<Face> icosahedronFaces)
    {
        icosahedronFaces.ForEach(face =>
        {
            List<Point> otherPoints = face.GetOtherPoints(_center);
            otherPoints.ForEach(point =>
            {
                if (_neighbourCenters.FirstOrDefault(centerPoint => centerPoint.ID == point.ID) == null)
                {
                    _neighbourCenters.Add(point);
                }
            });
        });
    }
    public void BuildBridges() => _generateMesh.BuildBridges();
    public void BuildTriangles() => _generateMesh.BuildTriangles();
    public void SetRandomHeight()
    {
        float value = Random.value;
        if (value < 0.4f) _height = 0;
        else if (value < 0.45f) _height = 1;
        else if (value < 0.50f) _height = 2;
        else if (value < 0.7f) _height = 3;
        else if (value < 0.9f) _height = 4;
        else _height = 5;

        if (_height == 0) _type = Type_of_Tiles.Water;//new Color(0f, 0.761f, 1f);
        else if (_height == 1 || _height == 2) _type = Type_of_Tiles.Sand;//new Color(1f, 0.929f, 0f);
        else if (_height == 3 || _height == 4) _type = Type_of_Tiles.Ground;//new Color(0.508f, 1f, 0f);
        else _type = Type_of_Tiles.Mountains;
    }
    public void RecalculateMesh()
    {
        TileDetails details = _generateMesh.RecalculateDetails();
        _meshFilter.mesh = details.mesh;
        _meshCollider.sharedMesh = details.mesh;
        _lineRenderer.positionCount = details.Points_for_lineRenderer.Count;
        _lineRenderer.SetPositions(details.Points_for_lineRenderer.ToArray());
        _lineRenderer.startColor = details.Color_for_lineRenderer;
        _lineRenderer.endColor = details.Color_for_lineRenderer;
        _meshCollider.convex = true; 
    }
    
    public void addResourse(GameObject ObjectPrefab)
    {
        resourse = Instantiate(ObjectPrefab, transform).GetComponent<Resourse>();
        resourse.transform.position = _generateMesh._hexCenter.Position;
        resourse.transform.rotation = Quaternion.LookRotation(_generateMesh.getNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
        resourse.Radius = _generateMesh.GetRadius();
        resourse.GenerateTexture();
    }
      
}

