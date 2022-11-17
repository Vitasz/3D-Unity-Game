using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;
using UnityEngine.XR;

public class GenerateMeshForTile
{
    private readonly HexDetails _details = new();
    private readonly List<Face> _faces;
    private readonly List<Face> _hexFaces = new();
    private readonly List<Point> _points;
    private MeshDetails _resourseMeshDetails = new(new(), new(), new(), new(), HexMetrics.hexMaterial);

    private readonly List<Face> icosahedronFaces;
    public List<MeshDetails> meshes = new();
    private List<GenerateMeshForTile> _neighbours;

    public int WaterLevel;
    public int Height
    {
        get { return _details.Height; }
        set { _details.Height = value; }
    }
    //private readonly float delta_height = 1f;
    private Color _color, _color_grad;
    private readonly List<Color> _colors;
    private readonly List<Color> _hexColors; 
    private readonly HashSet<GenerateMeshForTile> _buildedBridge = new ();
    private readonly Dictionary<Point, Color> _colors_Points = new ();
    public GenerateMeshForTile(Point center, float radius, float delta_height)
    {
        _details = new();
        _points = new();
        _details.Points = new();
        _faces = new List<Face>();
        _colors = new List<Color>();
        _hexColors = new List<Color>();

        _details.IcoCenter = center;
        _details.Radius = radius;
        _details.DeltaHeight = delta_height;
        icosahedronFaces = center.GetOrderedFaces();
        _details.IcoPoints = icosahedronFaces.Select(face => face.GetCenter().Position).ToList();
    }
    public void SetNeighbours(List<Tile> neighbours)
    {
        _neighbours = neighbours.Select(x => x._generateMesh).ToList();
    }
    public void BuildHex()
    {
        _details.IcoPoints.ForEach(pos =>
        {
            AddHexPoint(new Point(pos).ProjectToSphere(_details.Radius + _details.Height * _details.DeltaHeight, 0.5f), _color);
        });
        for (int i = 1; i < _details.Points.Count - 1; i++)
        {
            _hexFaces.Add(new Face(_details.Points[0], _details.Points[i], _details.Points[i + 1]));
        }

    }
    public void BuildBridges()
    {
        for (int i = 0; i < _details.IcoPoints.Count; i++)
        {

            int index_A = i, index_B = (i + 1) % _details.IcoPoints.Count;

            Vector3 A = _details.IcoPoints[index_A], B = _details.IcoPoints[index_B];

            GenerateMeshForTile nowTile = _neighbours[0];
            Vector3 A1 = new(), B1 = new();
            foreach (GenerateMeshForTile tile in _neighbours)
            {
                bool ok1, ok2;
                ok1 = ok2 = false;
                foreach (Vector3 x in tile._details.IcoPoints)
                {
                    
                    if (Vector3.Distance(x, A) < 0.001f)
                    {
                        ok1 = true; ;
                        A1 = x;
                    }
                    if (Vector3.Distance(x, B) < 0.001f)
                    {
                        ok2 = true;
                        B1 = x;
                    }
                    if (ok1 && ok2) break;
                }
                if (ok1 && ok2)
                {
                    nowTile = tile;
                    break;
                }
            }

            if (_details.Height < nowTile.Height) continue;
            
            AddPoint(nowTile._details.Points[nowTile._details.IcoPoints.IndexOf(B1)], nowTile._colors_Points[nowTile._details.Points[nowTile._details.IcoPoints.IndexOf(B1)]]);
            AddPoint(nowTile._details.Points[nowTile._details.IcoPoints.IndexOf(A1)], nowTile._colors_Points[nowTile._details.Points[nowTile._details.IcoPoints.IndexOf(A1)]]);


            //Мост между двумя шестиугольниками
            if (!nowTile._buildedBridge.Contains(this) && _details.Height >= nowTile.Height)
            {
                CreateBridge(index_A, index_B, _points[^2], _points[^1], _details.Height, nowTile.Height, _color);;
                _buildedBridge.Add(nowTile);
            }

        }
    }
    private void CreateRectangle(Point p1, Point p2, Point p3, Point p4)
    {
        CreateTriangle(p1, p2, p4);
        CreateTriangle(p3, p2, p4);
    }
    private void CreateBridge(int index_A, int index_B, Point next_Point_B, Point next_Point_A,
        int startHeight, int toHeight, Color startColor)
    {
        Point prevA = _details.Points[index_A], prevB = _details.Points[index_B];
        for (int i = startHeight - 1; i > toHeight; i--)
        {
            Point nowA = new Point(_details.IcoPoints[index_A]).ProjectToSphere(_details.Radius + _details.DeltaHeight * i, 0.5f);
            Point nowB = new Point(_details.IcoPoints[index_B]).ProjectToSphere(_details.Radius + _details.DeltaHeight * i, 0.5f); ;
            AddPoint(nowA, startColor);
            AddPoint(nowB, startColor);
            CreateRectangle(prevA, prevB, nowB, nowA);
            prevA = nowA;
            prevB = nowB;
        }
        CreateRectangle(prevA, prevB, next_Point_B, next_Point_A);
    }
    private void AddPoint(Point point, Color color)
    {
        _points.Add(point);
        _colors.Add(color);
        _colors_Points[point] = color;
    }
    private void AddHexPoint(Point point, Color color)
    {
        _details.Points.Add(point);
        _hexColors.Add(color);
        _colors_Points[point] = color;
    }
    private void CreateTriangle(Point a, Point b, Point c) => _faces.Add(new Face(a, b, c));
    public void AddResourse(Resourse resourse)
    {
        
        if (resourse is GenerateMeshAble)
        {
            bool hasOwnGround = false;
            _resourseMeshDetails = (resourse as GenerateMeshAble).GetMesh(_details, ref hasOwnGround);
            _color = resourse.color;
        }
        
    }
    public MeshDetails GetHexDetails()
    {
        List<Point> vertices = new ();
        List<Face> triangles = new ();
        List<Color> colors = new ();
        _details.Points.ForEach(point => vertices.Add(point));
        _hexColors.ForEach(color => colors.Add(color));
        _hexFaces.ForEach(face => triangles.Add(face));
        //_resourseMeshDetails.Vertices.ForEach(point => vertices.Add(point));
        //_resourseMeshDetails.Triangles.ForEach(triangle => triangles.Add(triangle));
        //_resourseMeshDetails.Colors.ForEach(color => colors.Add(color));
        _points.ForEach(point =>
        {
            vertices.Add(point);
        });
        _faces.ForEach(face => triangles.Add(face));

        _colors.ForEach(color =>
        {
            colors.Add(color);
        });

        return new MeshDetails(vertices, triangles, colors, new(), HexMetrics.hexMaterial);
    }
    public MeshDetails GetResourseMesh()
    {
        return _resourseMeshDetails;
    }
    public Vector3 GetNormal()
    {
        return _details.Center.Position;
    }
    public float GetRadius()
    {
        return (_points[0].Position - _details.Center.Position).magnitude * 2;
    }
    public void SetFinalHeight()
    {
        if (_details.Height < WaterLevel) _details.Height = WaterLevel - 1;
        _color = GetColor(_details.Height);
        float grad = Random.value % 0.4f;
        _color_grad = new Color(grad, grad, grad);
        _color -= _color_grad;
        _details.Center = _details.IcoCenter.ProjectToSphere(_details.Radius  + _details.Height * _details.DeltaHeight, 0.5f);
    }
    public Color GetColor(int height)
    {
        if (height == WaterLevel - 1) return Color.blue;
        else if (height == WaterLevel || height == 1 + WaterLevel) return Color.yellow;
        else if (height == 2 + WaterLevel || height == 3 + WaterLevel) return Color.green;
        else return Color.white;
    }
    public List<Vector2> test;
    
    public void AddDecoration(Mesh decor, Material material, float scale)
    {
        if (_details.IcoPoints.Count == 5) return;
        float x = Random.Range(0, 2f), y = Random.Range(0, 1f);
        Vector3 position, A, B;
        if (x <= 1f)
        {
            A = Vector3.Lerp(_details.Points[0].Position, _details.Points[1].Position, x);
            B = Vector3.Lerp(_details.Points[5].Position, _details.Points[4].Position, x);
        }
        else
        {
            A = Vector3.Lerp(_details.Points[2].Position, _details.Points[1].Position, x-1f);
            B = Vector3.Lerp(_details.Points[3].Position, _details.Points[4].Position, x-1f);
        }
        
        position = Vector3.Lerp(A, B, y);
  
        List<Face> faces = new();
        List<Point> vertices = new();
        List<Color> colors = new();
        Quaternion rotation = Quaternion.LookRotation(GetNormal()) * Quaternion.Inverse(Quaternion.Euler(270, 90, 0));
        for (int i = 0; i < decor.vertices.Length; i++)
        {
            Vector3 nowpos = rotation * (decor.vertices[i]* scale) + position;
            if (colors.Count>i)colors.Add(decor.colors[i]);
            vertices.Add(new Point(nowpos));
            
        }
        for (int i = 0; i < decor.triangles.Length; i+=3)
        {
            faces.Add(new Face(vertices[decor.triangles[i]],
            vertices[decor.triangles[i + 1]],
            vertices[decor.triangles[i + 2]]));
        }
        meshes.Add(new MeshDetails(vertices, faces, colors, decor.uv.ToList(), material));
    }
    public List<MeshDetails> GetAllMeshes()
    {
        List<MeshDetails> details = new();
        details.Add(GetHexDetails());
        details.Add(GetResourseMesh());
        foreach(var mesh in meshes)
            details.Add(mesh);
        return details;
    }

}

