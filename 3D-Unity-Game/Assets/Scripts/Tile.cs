
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private  Point _center;
    private  float _radius;
    private  float _size;
    private  List<Face> _faces;
    private  List<Point> _points;
    private  List<Point> _neighbourCenters;
    private List<Vector3> _icosahedronPoints;
    private List<Face> icosahedronFaces;
    private List<Tile> _neighbours;
    private List<Tile> _neighboursAround = new List<Tile>();
    private int _height = 0;
    private float _height_percent_of_radius = 0.05f;
    private Type_of_Tiles _type;
    private Color _color;
    public GameObject mesh;
    public MeshFilter _meshFilter;
    public MeshCollider _meshCollider;
    private Mesh _mesh;
    private Hexasphere _sphere;
    private  Dictionary<Tile, (int, int)> _neighbourAndConnections = new Dictionary<Tile, (int, int)>();
    private  HashSet<Tile> _buildedBridge = new HashSet<Tile>();
    private  List<(List<Point>, List<Point>)> _bridge = new List<(List<Point>, List<Point>)>();
    public void CreateTile(Point center, float radius, float size, Hexasphere hexasphere)
    {
        _points = new List<Point>();
        _faces = new List<Face>();
        _neighbourCenters = new List<Point>();
        _neighbours = new List<Tile>();
        
        //Debug.Log(Color.red);
        _center = center;
        _radius = radius;
        _size = Mathf.Max(0.01f, Mathf.Min(1f, size));
        _sphere = hexasphere;
        _color = new Color(Random.value, Random.value, Random.value);
        SetHeight(Random.Range(0, 3));
        //Type_of_Tiles type = Type_of_Tiles.Water;
        // if (_height >= 0 && _height<= _radius * 0.025f) type = Type_of_Tiles.Sand;
        // else if (_height >= _radius * 0.025f) type = Type_of_Tiles.Ground;
        // SetType(type);
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
        icosahedronFaces = center.GetOrderedFaces();
        _icosahedronPoints = icosahedronFaces.Select(face => Vector3.Lerp(_center.Position, face.GetCenter().Position, 1f)).ToList();
        StoreNeighbourCenters(icosahedronFaces);
        BuildFaces(icosahedronFaces);
        
        
    }
    public void OnMouseDrag() => _sphere.cameraSphere.RotateAround();
    public List<Point> Points => _points;
    public Point Center => _center;
    public List<Face> Faces => _faces;
    public List<Tile> Neighbours => _neighbours;

    public List<Color> Colors = new List<Color>();
    
        
    public void ResolveNeighbourTiles(List<Tile> allTiles)
    {
        List<string> neighbourIds = _neighbourCenters.Select(center => center.ID).ToList();
        _neighbours = allTiles.Where(tile => neighbourIds.Contains(tile._center.ID)).ToList();
    }
        
    public override string ToString()
    {
        return $"{_center.Position.x},{_center.Position.y},{_center.Position.z}";
    }

    public string ToJson()
    {
        return $"{{\"centerPoint\":{_center.ToJson()},\"boundary\":[{string.Join(",",_points.Select(point => point.ToJson()))}]}}";
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

    private void BuildFaces(List<Face> icosahedronFaces)
    {
        List<Vector3> polygonPoints = icosahedronFaces.Select(face => Vector3.Lerp(_center.Position, face.GetCenter().Position, _size)).ToList();
        polygonPoints.ForEach(pos =>
        {
            addPoint(new Point(pos).ProjectToSphere(_radius * (1 + _height * _height_percent_of_radius), 0.5f), _color);
        });

        //Основной шестиугольник
        for (int i = 1; i < _points.Count - 1; i++)
        {
            _faces.Add(new Face(_points[0], _points[i], _points[i + 1]));
        }

        
    }
    public void BuildBridges()
    {
        for (int i = 0; i < _icosahedronPoints.Count; i++)
        {
            int index_A = i, index_B = (i + 1) % _icosahedronPoints.Count;
           
            Vector3 A = _icosahedronPoints[index_A], B = _icosahedronPoints[index_B];
            Tile nowTile = _neighbours[0];
            Vector3 A1 = new Vector3(), B1 = new Vector3();
            bool ok1, ok2;
            foreach (Tile tile in _neighbours)
            {
                ok1 = false;
                ok2 = false;
                foreach(Vector3 point in tile._icosahedronPoints)
                {
                    if (Vector3.Distance(point, A) < 0.001f)
                    {
                        ok1 = true;
                        A1 = point;
                    }
                    if (Vector3.Distance(point, B) < 0.001f)
                    {
                        ok2 = true;
                        B1 = point;
                    }
                }
                if (ok1 && ok2)
                {
                    nowTile = tile;
                    break;
                }
            }
            //Сохраняем общие точки с соседом
            _neighbourAndConnections.Add(nowTile, (Mathf.Min(index_A, index_B), Mathf.Max(index_A, index_B)));
            _neighboursAround.Add(nowTile);
            _bridge.Add((new List<Point>(), new List<Point>()));
            //Мост между двумя шестиугольниками
            addPoint(nowTile._points[nowTile._icosahedronPoints.IndexOf(B1)], nowTile._color);
            addPoint(nowTile._points[nowTile._icosahedronPoints.IndexOf(A1)], nowTile._color);
            if (!nowTile._buildedBridge.Contains(this))
            {
                    createBridge(_points[index_A], _points[index_B], _points[_points.Count-2], _points[_points.Count - 1], _height - nowTile._height,
                        _color, nowTile._color);
                _buildedBridge.Add(nowTile);
            }
            
        }
    }
    private void createRectangle(Point p1, Point p2, Point p3, Point p4)
    {
        _faces.Add(new Face(p1, p2, p4));
        _faces.Add(new Face(p1, p4, p2));
        _faces.Add(new Face(p2, p3, p4));
        _faces.Add(new Face(p2, p4, p3));
    }
    public void RecalculateMesh()
    {
        List<Point> vertices = new List<Point>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        _points.ForEach(point =>
        {
            point.positionInMesh = vertices.Count;

            vertices.Add(point);
        });
        _faces.ForEach(face =>
        {
            face.Points.ForEach(point =>
            {
                triangles.Add(point.positionInMesh);
            });
        });

        Colors.ForEach(color =>
        {
            colors.Add(color);
        });
        
        _mesh.vertices = vertices.Select(vertex => vertex.Position).ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.colors = Colors.ToArray();
        _mesh.RecalculateNormals();
        _meshCollider.convex = true;
    }
    
    private void createBridge(Point p1, Point p2, Point p3, Point p4, float delta_height, Color color_from, Color color_to)
    {
        int cnt = 2;
        float percent_ledge = 0.75f;
        float delta_ledge = percent_ledge * 2 * (1 - _size) / cnt;
        float delta_slope = (1 - percent_ledge) * 2 * (1 - _size) / (cnt+1);
        delta_height /= (cnt+1);

        int index_A = _points.IndexOf(p1), index_B = _points.IndexOf(p2);
        Plane planeBB = new Plane(p2.Position, p3.Position, new Vector3());
        Plane planeAA = new Plane(p1.Position, p4.Position, new Vector3());
        Point prevA = p1, prevB = p2;
        _bridge[_bridge.Count - 1].Item1.Add(p1);
        _bridge[_bridge.Count - 1].Item2.Add(p2);
        float now_height = _radius * (1 + _height * _height_percent_of_radius);
        float now_size = _size;
        for (int i = 0; i < cnt; i++)
        {

            //Цвет
            Color nowColor = new Color(Mathf.Lerp(color_from.r, color_to.r, (float)(i + 1) / (cnt + 1)),
                Mathf.Lerp(color_from.g, color_to.g, (float)(i + 1) / (cnt + 1)),
                Mathf.Lerp(color_from.b, color_to.b, (float)(i + 1) / (cnt + 1)));

            //Склон
            now_size += delta_slope;
            List<Vector3> nowPoints = icosahedronFaces.Select(face => _center.Position + (face.GetCenter().Position - _center.Position)*now_size).ToList();
            Vector3 posB11 = nowPoints[index_B], posA11 = nowPoints[index_A];
            Vector3 right_posB11 = planeBB.ClosestPointOnPlane(posB11);
            Vector3 right_posA11 = planeAA.ClosestPointOnPlane(posA11);
            now_height -= delta_height * _radius * _height_percent_of_radius;
            Point B11 = new Point(right_posB11).ProjectToSphere(now_height, 0.5f);
            Point A11 = new Point(right_posA11).ProjectToSphere(now_height, 0.5f);
            _bridge[_bridge.Count - 1].Item1.Add(A11);
            _bridge[_bridge.Count - 1].Item2.Add(B11);
            addPoint(B11, nowColor);
            addPoint(A11, nowColor);
            createRectangle(prevA, prevB, B11, A11);
            prevA = A11;
            prevB = B11;

            //Ступенька
            now_size += delta_ledge;
            nowPoints = icosahedronFaces.Select(face => _center.Position + (face.GetCenter().Position - _center.Position) * now_size).ToList();
            posB11 = nowPoints[index_B]; posA11 = nowPoints[index_A];
            right_posB11 = planeBB.ClosestPointOnPlane(posB11);
            right_posA11 = planeAA.ClosestPointOnPlane(posA11);

            B11 = new Point(right_posB11).ProjectToSphere(now_height, 0.5f);
            A11 = new Point(right_posA11).ProjectToSphere(now_height, 0.5f);
            _bridge[_bridge.Count - 1].Item1.Add(A11);
            _bridge[_bridge.Count - 1].Item2.Add(B11);
            addPoint(B11, nowColor);
            addPoint(A11, nowColor);
            createRectangle(prevA, prevB, B11, A11);
            prevA = A11;
            prevB = B11;

        }
        _bridge[_bridge.Count - 1].Item1.Add(p4);
        _bridge[_bridge.Count - 1].Item2.Add(p3);
        //Последний склон
        createRectangle(prevA, prevB, p3, p4);
    }
    private void addPoint(Point point, Color color)
    {
        _points.Add(point);
        Colors.Add(color);
    }
    
    private void createTriangle(List<Point> a, List<Point> b, List<Point> c, int height1, int height2, int height3)
    {
        if (height1== height2 && height2 == height3)
        {
            _faces.Add(new Face(a[0], b[0], c[0]));
        }
    }
    private (List<Point>, List<Point>) getBridgePoints(Tile tile)
    {
        (List<Point>, List<Point>) ans1 = _bridge[_neighboursAround.IndexOf(tile)], ans2 = tile._bridge[tile._neighboursAround.IndexOf(this)];
        return ans1.Item1.Count > ans2.Item1.Count ? ans1 : ans2; 
    }
    public void SetType(Type_of_Tiles type)
    {

        _color = Color.black;
        if (type == Type_of_Tiles.Ground) _color = Color.green;
        if (type == Type_of_Tiles.Sand) _color = Color.yellow;
        if (type == Type_of_Tiles.Water) _color = Color.blue;
       
    }
    
    public void SetHeight(int height)
    {
        _height = height;

    }
}

