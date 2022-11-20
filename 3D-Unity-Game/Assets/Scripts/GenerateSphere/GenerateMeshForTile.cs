using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class GenerateMeshForTile
{
    private readonly HexDetails _details = new();
    private readonly List<Face> _hexFaces = new();
    private MeshDetails _resourseMeshDetails = new(new(), new(), new(), new(), HexMetrics.hexMaterial);
    private List<MeshDetails> bridges = new();
    private readonly List<Face> icosahedronFaces;
    public List<MeshDetails> meshes = new();
    private List<GenerateMeshForTile> _neighbours;
    private Tile tile;
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
    public GenerateMeshForTile(Tile tile, Point center, float radius, float delta_height)
    {
        this.tile = tile;
        _details = new();
        //_points = new();
        _details.Points = new();
       // _faces = new List<Face>();
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
        for (int i = 0; i < _details.Points.Count; i++)
        {
            _hexFaces.Add(new Face(_details.Center, _details.Points[i], _details.Points[(i + 1)%_details.Points.Count]));
            //_hexFaces.Add(new Face(_details.Center, _details.Points[(i + 1) % _details.Points.Count], _details.Points[i]));
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

            //Мост между двумя шестиугольниками
            if (!nowTile._buildedBridge.Contains(this) && _details.Height >= nowTile.Height)
            {
                List<Point> points = new();
                List<Face> faces = new();
                List<Face> faces2 = new();
                List<Vector2> uvs = new();
                points.Add(_details.Points[index_A]);
                points.Add(_details.Points[index_B]);
                points.Add(nowTile._details.Points[nowTile._details.IcoPoints.IndexOf(B1)]);
                points.Add(nowTile._details.Points[nowTile._details.IcoPoints.IndexOf(A1)]);
                faces.Add(new Face(points[0], points[1], points[2]));
                faces2.Add(new Face(points[0], points[2], points[1]));
                faces.Add(new Face(points[0], points[2], points[3]));
                faces2.Add(new Face(points[0], points[3], points[2]));
                uvs.Add(new Vector2(0.1f, 0.3f));
                uvs.Add(new Vector2(0.9f, 0.3f));
                uvs.Add(new Vector2(0.9f, 0.7f));
                uvs.Add(new Vector2(0.1f, 0.7f));
                bridges.Add(new MeshDetails(points, faces, new(), uvs, GetHexMaterial()));
                //bridges.Add(new MeshDetails(points, faces2, new(), uvs, GetHexMaterial()));
                //CreateBridge(index_A, index_B, _points[^2], _points[^1], _details.Height, nowTile.Height, _color);;
                _buildedBridge.Add(nowTile);
            }

        }
    }
    private Material GetHexMaterial()
    {
        if (tile._type == Type_of_Tiles.Sand)
        {
            return  HexMetrics.desertMaterial;
        }
        if (tile._type == Type_of_Tiles.Ground)
        {
            return HexMetrics.groundMaterial;
        }
        if (tile._type == Type_of_Tiles.Water)
        {
            return HexMetrics.waterMaterial;
        }
        if (tile._type == Type_of_Tiles.Mountains)
        {
            return HexMetrics.snowMaterial;
        }
        return HexMetrics.hexMaterial;
    }
    /*private void CreateRectangle(Point p1, Point p2, Point p3, Point p4)
    {
        CreateTriangle(p1, p2, p4);
        CreateTriangle(p3, p2, p4);
    }*/
    /*private void CreateBridge(int index_A, int index_B, Point next_Point_B, Point next_Point_A,
        int startHeight, int toHeight, Color startColor)
    {
        Point prevA = _details.Points[index_A], prevB = _details.Points[index_B];
        AddPoint(prevA, startColor);
        AddPoint(prevB, startColor);
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
        AddPoint(next_Point_B, startColor);
        AddPoint(next_Point_A, startColor);
    }
    private void AddPoint(Point point, Color color)
    {
        _points.Add(point);
        _colors.Add(color);
        _colors_Points[point] = color;
    }*/
    private void AddHexPoint(Point point, Color color)
    {
        _details.Points.Add(point);
        _hexColors.Add(color);
        _colors_Points[point] = color;
    }
    /*private void CreateTriangle(Point a, Point b, Point c)
    {
        _faces.Add(new Face(a, b, c));
        _faces.Add(new Face(a, c, b));
    }*/
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
        List<Vector2> uv = new ();
        vertices.Add(_details.Center);
        _details.Points.ForEach(point => vertices.Add(point));
        _hexColors.ForEach(color => colors.Add(color));
        _hexFaces.ForEach(face => triangles.Add(face));
        uv.Add(new Vector2(0.5f, 0.5f));
        uv.Add(new Vector2(0, 0.5f));
        uv.Add(new Vector2(0.25f, 0f));
        uv.Add(new Vector2(0.75f, 0f));
        uv.Add(new Vector2(1, 0.5f));
        uv.Add(new Vector2(0.75f, 1f));
        if (vertices.Count != 6)uv.Add(new Vector2(0.25f, 1f));
        return new MeshDetails(vertices, triangles, colors, uv, GetHexMaterial());
    }
    public List<MeshDetails> GetBridgesDetails()
    {
        /*List<Point> vertices = new();
        List<Face> triangles = new();
        List<Color> colors = new();
        List<Vector2> uv = new();
        _points.ForEach(point =>
        {
            vertices.Add(point);
        });
        _faces.ForEach(face => triangles.Add(face));

        _colors.ForEach(color =>
        {
            colors.Add(color);
        });
        for (int i = 0; i < _points.Count; i += 4)
        {
            uv.Add(new Vector2(0f, 0.5f));
            uv.Add(new Vector2(0.5f, 0f));
            uv.Add(new Vector2(1f, 0.5f));
            uv.Add(new Vector2(0.5f, 1f));
        }
        return new() { new MeshDetails(vertices, triangles, colors, uv, HexMetrics.earthMaterial) };*/
        return bridges;
    }
    public MeshDetails GetResourseMesh()
    {
        return _resourseMeshDetails;
    }
    public Vector3 GetNormal()
    {
        return _details.Center.Position;
    }
    /*public float GetRadius()
    {
        return (_points[0].Position - _details.Center.Position).magnitude * 2;
    }*/
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
        details.AddRange(GetBridgesDetails());
        details.Add(GetResourseMesh());
        foreach(var mesh in meshes)
            details.Add(mesh);
        return details;
    }
    public void updateHeightCenter(float height)
    {
        
        _details.Center.Position = _details.Center.Position + height * GetNormal().normalized;
        
    }
}

