using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class GenerateMeshForTile
{
    private readonly HexDetails _details = new();
    private List<Face> _hexFaces = new();
    private MeshDetails _resourseMeshDetails = new(new(), new(), new(), HexMetrics.hexMaterial);
    private List<MeshDetails> bridges = new();
    private readonly List<Face> icosahedronFaces;
    public List<MeshDetails> meshes = new();
    private List<GenerateMeshForTile> _neighbours;
    private Tile tile;
    public int WaterLevel;
    public int Height
    {
        get { return _details.Height; }
        set { 
            _details.Height = value;
            SetFinalHeight();
            if (_details.Points == null) return;
            BuildHex();
            foreach (var tile in _neighbours)
            {
                tile.BuildBridges();
                tile.tile.chunk.UpdateMesh();
            }
            
            ClearDecorations();
        }
    }
    public GenerateMeshForTile(Tile tile, Point center, float radius, float delta_height)
    {
        this.tile = tile;
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
        _details.Points = new();
        _hexFaces = new();
        _details.IcoPoints.ForEach(pos =>
        {
            AddHexPoint(new Point(pos).ProjectToSphere(_details.Radius + _details.Height * _details.DeltaHeight, 0.5f));
        });
        for (int i = 0; i < _details.Points.Count; i++)
        {
            _hexFaces.Add(new Face(_details.Center, _details.Points[i], _details.Points[(i + 1)%_details.Points.Count]));
        }

    }
    public void BuildBridges()
    {
        bridges.Clear();
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
            if (_details.Height <= nowTile.Height) continue;

            //Мост между двумя шестиугольниками
            List<Point> points = new();
            List<Face> faces = new();
            List<Vector2> uvs = new();
            points.Add(_details.Points[index_A]);
            points.Add(_details.Points[index_B]);
            points.Add(nowTile._details.Points[nowTile._details.IcoPoints.IndexOf(B1)]);
            points.Add(nowTile._details.Points[nowTile._details.IcoPoints.IndexOf(A1)]);
            Vector3 toCenter = _details.Center.Position - (points[0].Position + points[1].Position) / 2;
            toCenter.Normalize();
            Vector3 side1 = points[0].Position - points[1].Position;
            Vector3 side2 = points[0].Position - points[2].Position;
            Vector3 perp = Vector3.Cross(side1, side2);
            perp.Normalize();
            
            if ((toCenter - perp).sqrMagnitude >= 1f)
            {
                faces.Add(new Face(points[0], points[1], points[2]));
                faces.Add(new Face(points[0], points[2], points[3]));
            }
            else
            {

                faces.Add(new Face(points[0], points[2], points[1]));
                faces.Add(new Face(points[0], points[3], points[2]));
            }
            uvs.Add(new Vector2(0.1f, 0.3f));
            uvs.Add(new Vector2(0.9f, 0.3f));
            uvs.Add(new Vector2(0.9f, 0.7f));
            uvs.Add(new Vector2(0.1f, 0.7f));
            bridges.Add(new MeshDetails(points, faces, uvs, GetHexMaterial()));
            //bridges.Add(new MeshDetails(points, faces2, uvs, GetHexMaterial()));
        }
    }
    public void ClearDecorations()
    {
        if (meshes.Count == 0) return;
        meshes.Clear();
        tile.chunk.UpdateMesh();
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
    private void AddHexPoint(Point point)
    {
        _details.Points.Add(point);
    }
    public void AddResourse(Resourse resourse)
    {
        
        if (resourse is GenerateMeshAble)
        {
            bool hasOwnGround = false;
            _resourseMeshDetails = (resourse as GenerateMeshAble).GetMesh(_details, ref hasOwnGround);
        }
        
    }
    public MeshDetails GetHexDetails()
    {
        List<Point> vertices = new ();
        List<Face> triangles = new ();
        List<Vector2> uv = new ();
        vertices.Add(_details.Center);
        _details.Points.ForEach(point => vertices.Add(point));
        _hexFaces.ForEach(face => triangles.Add(face));
        uv.Add(new Vector2(0.5f, 0.5f));
        uv.Add(new Vector2(0, 0.5f));
        uv.Add(new Vector2(0.25f, 0f));
        uv.Add(new Vector2(0.75f, 0f));
        uv.Add(new Vector2(1, 0.5f));
        uv.Add(new Vector2(0.75f, 1f));
        if (vertices.Count != 6)uv.Add(new Vector2(0.25f, 1f));
        return new MeshDetails(vertices, triangles, uv, GetHexMaterial());
    }
    public List<MeshDetails> GetBridgesDetails()
    {
        return bridges;
    }
    public MeshDetails GetResourseMesh()
    {
        return _resourseMeshDetails;
    }
    public Vector3 GetNormal()
    {
        return _details.Center.Position.normalized;
    }
    public void SetFinalHeight()
    {
        //if (_details.Height < WaterLevel) _details.Height = WaterLevel - 1;
        _details.Center = _details.IcoCenter.ProjectToSphere(_details.Radius  + _details.Height * _details.DeltaHeight, 0.5f);
    }
    
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
        float delta_y = float.MaxValue;
        for (int i = 0; i < decor.vertices.Length; i++)
        {
            delta_y = Mathf.Min(delta_y, decor.vertices[i].y);
        }
        for (int i = 0; i < decor.vertices.Length; i++)
        {
            Vector3 pos = new Vector3(decor.vertices[i].x, decor.vertices[i].y - delta_y,
                decor.vertices[i].z);
            Vector3 nowpos = rotation * (pos * scale) + position;
            if (colors.Count>i)colors.Add(decor.colors[i]);
            vertices.Add(new Point(nowpos));
            
        }
        for (int i = 0; i < decor.triangles.Length; i+=3)
        {
            faces.Add(new Face(vertices[decor.triangles[i]],
            vertices[decor.triangles[i + 1]],
            vertices[decor.triangles[i + 2]]));
        }
        meshes.Add(new MeshDetails(vertices, faces, decor.uv.ToList(), material));
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
    public Vector3 GetCenter() => _details.Center.Position;
    public Vector3[] GetPositions() => _details.Points.Select(point => point.Position).ToArray();

}

