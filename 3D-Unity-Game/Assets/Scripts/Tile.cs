
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    private float _height_percent_of_radius = 0.025f;
    private Type_of_Tiles _type;
    private Color _color;
    public GameObject mesh;
    public MeshFilter _meshFilter;
    public MeshCollider _meshCollider;
    private Mesh _mesh;
    private Hexasphere _sphere;
    private  Dictionary<Tile, (int, int)> _neighbourAndConnections = new Dictionary<Tile, (int, int)>();
    private  HashSet<Tile> _buildedBridge = new HashSet<Tile>();
    private  Dictionary<Point, Color> _colors_Points = new Dictionary<Point, Color>();
    Dictionary<Point, ((Point p, List<Point> list, Tile tile) pair1, (Point p, List<Point> list, Tile tile) pair2)> connections = 
        new Dictionary<Point, ((Point, List<Point>, Tile), (Point, List<Point>, Tile))>();
    //private  List<(List<Point>, List<Point>)> _bridge = new List<(List<Point>, List<Point>)>();
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
        List<Color> colors = new List<Color>()
        {
            Color.green, Color.yellow
            //new Color(1f, 1f, 0.5f),
            //new Color(0.5f, 1f, 0f),
            //new Color(0.66f, 0.835f, 1f)
        };
        _color = colors[Random.Range(0, 2)];
        SetHeight(Random.Range(0, 5));
        if (_height == 0) _color = Color.blue;
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
                foreach (Vector3 point in tile._icosahedronPoints)
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
            addPoint(nowTile._points[nowTile._icosahedronPoints.IndexOf(B1)], nowTile._color);
            addPoint(nowTile._points[nowTile._icosahedronPoints.IndexOf(A1)], nowTile._color);

            //

            if (!connections.ContainsKey(_points[index_A]))
                connections[_points[index_A]] = ((null, new List<Point>() { _points[index_A] }, null),
                    (null, new List<Point>() { _points[index_A] }, null));
            if (!connections.ContainsKey(_points[index_B]))
                connections[_points[index_B]] = ((null, new List<Point>() { _points[index_B] }, null),
                    (null, new List<Point>() { _points[index_B] }, null)); ;
            List<Point> bridge_A;
            if (connections[_points[index_A]].pair1.p is null)
            {
                bridge_A = connections[_points[index_A]].pair1.list;
                connections[_points[index_A]] = ((_points[^1], bridge_A, nowTile), connections[_points[index_A]].pair2);
            }
            else
            {
                bridge_A = connections[_points[index_A]].pair2.list;
                connections[_points[index_A]] = (connections[_points[index_A]].pair1, (_points[^1], bridge_A, nowTile));
            }
            List<Point> bridge_B;
            if (connections[_points[index_B]].pair1.p is null)
            {
                bridge_B = connections[_points[index_B]].pair1.list;
                connections[_points[index_B]] = ((_points[^2], bridge_B, nowTile), connections[_points[index_B]].pair2);
            }
            else
            {
                bridge_B = connections[_points[index_B]].pair2.list;
                connections[_points[index_B]] = (connections[_points[index_B]].pair1, (_points[^2], bridge_B, nowTile));
            }
            



            //Мост между двумя шестиугольниками

            if (!nowTile._buildedBridge.Contains(this) && _height >= nowTile._height)
            {
                createBridge(_points[index_A], _points[index_B], _points[^2], _points[^1], _height - nowTile._height,
                    _color, nowTile._color, ref bridge_A, ref bridge_B);
                _buildedBridge.Add(nowTile);
            }

        }
    }
    public void BuildTriangles()
    {
        foreach (Point x in connections.Keys)
        {
            
            List<Point> l1 = connections[x].pair1.list, l2 = connections[x].pair2.list;
            
            Tile tile1 = connections[x].pair1.tile, tile2 = connections[x].pair2.tile;
            if (l1.Count > 1 && l2.Count > 1 && _height == tile1._height && tile1._height == tile2._height)
            {
                createTriangle(x, l1[1], l2[1]);
                continue;
            }
            if (l1.Count > 1 && l2.Count > 1 && tile1._height == tile2._height)
            {
                createTriangle(x, l1[1], l2[1]);
                for (int i = 0; i < l1.Count - 1; i++)
                {
                    createRectangle(l1[i], l2[i], l2[i + 1], l1[i + 1]);
                }
                continue;
            }
            if (l2.Count>2 && tile1._height == _height)
            {
                if (tile1.connections[connections[x].pair1.p].pair1.list[^1] == l2[^1])
                    l1 = tile1.connections[connections[x].pair1.p].pair1.list;
                else
                    l1 = tile1.connections[connections[x].pair1.p].pair2.list;
                for (int i = 0; i < l1.Count; i++)
                    addPoint(l1[i], tile1._colors_Points[l1[i]]);
                for (int i = 0; i < l1.Count - 1; i++)
                {
                    createRectangle(l1[i], l2[i], l2[i + 1], l1[i + 1]);
                }
                createTriangle(l1[^1], l1[^2], l2[^1]);
                continue;
            }
            if (l1.Count > 2 && tile2._height == _height && tile2.connections.ContainsKey(connections[x].pair2.p))
            {
                if (tile2.connections[connections[x].pair2.p].pair1.list[^1] == l1[^1])
                    l2 = tile2.connections[connections[x].pair2.p].pair1.list;
                else
                    l2 = tile2.connections[connections[x].pair2.p].pair2.list;
                for (int i = 0; i < l2.Count; i++)
                    addPoint(l2[i], tile2._colors_Points[l2[i]]);
                for (int i = 0; i < l2.Count - 1; i++)
                {
                    createRectangle(l1[i], l2[i], l2[i + 1], l1[i + 1]);
                }
                createTriangle(l1[^1], l1[^2], l2[^1]);
                continue;
            }
            
            Point center = new Point((x.Position + connections[x].pair1.p.Position + connections[x].pair2.p.Position) / 3);
            Color Color_center = new Color((_colors_Points[x].r + _colors_Points[connections[x].pair1.p].r + _colors_Points[connections[x].pair2.p].r)/3,
                (_colors_Points[x].g + _colors_Points[connections[x].pair1.p].g + _colors_Points[connections[x].pair2.p].g) / 3,
                (_colors_Points[x].b + _colors_Points[connections[x].pair1.p].b + _colors_Points[connections[x].pair2.p].b) / 3);
            addPoint(center, Color_center);
            for (int i = 0; i < connections[x].pair1.list.Count-1; i++)
                createTriangle(center, connections[x].pair1.list[i], connections[x].pair1.list[i + 1]);
            for (int i = 0; i < connections[x].pair2.list.Count - 1; i++)
                createTriangle(center, connections[x].pair2.list[i], connections[x].pair2.list[i + 1]);
        }
    }
    private void createRectangle(Point p1, Point p2, Point p3, Point p4)
    {
        createTriangle(p1, p2, p4);
        createTriangle(p3, p2, p4);
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

    private void createBridge(Point this_Point_A, Point this_Point_B, Point next_Point_B, Point next_Point_A,
        float delta_height, Color color_from, Color color_to, ref List<Point> bridge_face_A, ref List<Point> bridge_face_B)
    {
        if (delta_height == 0)
        {
            createRectangle(this_Point_A, this_Point_B, next_Point_B, next_Point_A);

            bridge_face_A.Add(next_Point_A);
            bridge_face_B.Add(next_Point_B);

            return;
        }
        int cnt = 5;
        float percent_ledge = 0.75f;
        float delta_ledge = percent_ledge * 2 * (1 - _size) / cnt;
        float delta_slope = (1 - percent_ledge) * 2 * (1 - _size) / (cnt+1);
        delta_height /= (cnt+1);
        int index_A = _points.IndexOf(this_Point_A), index_B = _points.IndexOf(this_Point_B);
        Plane planeBB = new Plane(this_Point_B.Position, next_Point_B.Position, new Vector3());
        Plane planeAA = new Plane(this_Point_A.Position, next_Point_A.Position, new Vector3());
        Point prevA = this_Point_A, prevB = this_Point_B;
        float now_height = _radius * (1 + _height * _height_percent_of_radius);
        float now_size = _size;
        for (int i = 0; i <= cnt; i++)
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
            addPoint(B11, nowColor);
            addPoint(A11, nowColor);
            bridge_face_A.Add(A11);
            bridge_face_B.Add(B11);
            createRectangle(prevA, prevB, B11, A11);
            prevA = A11;
            prevB = B11;

            //Ступенька
            if (cnt == i) continue;
            now_size += delta_ledge;
            nowPoints = icosahedronFaces.Select(face => _center.Position + (face.GetCenter().Position - _center.Position) * now_size).ToList();
            posB11 = nowPoints[index_B]; posA11 = nowPoints[index_A];
            right_posB11 = planeBB.ClosestPointOnPlane(posB11);
            right_posA11 = planeAA.ClosestPointOnPlane(posA11);

            B11 = new Point(right_posB11).ProjectToSphere(now_height, 0.5f);
            A11 = new Point(right_posA11).ProjectToSphere(now_height, 0.5f);
            addPoint(B11, nowColor);
            addPoint(A11, nowColor);
            bridge_face_A.Add(A11);
            bridge_face_B.Add(B11);
            createRectangle(prevA, prevB, B11, A11);
            prevA = A11;
            prevB = B11;
        }
        //Последний склон
        bridge_face_A.Add(next_Point_A);
        bridge_face_B.Add(next_Point_B);
        createRectangle(prevA, prevB, next_Point_B, next_Point_A);
    }
    private void addPoint(Point point, Color color)
    {
        _points.Add(point);
        Colors.Add(color);
        _colors_Points[point] = color;
    }
    
    private void createTriangle(Point a, Point b, Point c)
    {
          _faces.Add(new Face(a, b, c));
          _faces.Add(new Face(a, c, b));
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

