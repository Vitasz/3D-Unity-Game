using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class Hexasphere: MonoBehaviour
{
    private readonly List<Tile> _tiles = new ();
    private List<Face> _icosahedronFaces;
    private readonly List<Point> _points = new ();
    public HexSphereGenerator _mapGen;
    [Min(5f)]
    [SerializeField] private float radius = 10f;
    [Range(1, 100)]
    [SerializeField] public int divisions = 10;
    public CameraSphere CameraSphere;
    public Sun sun;
    public Tile ClickedTile = null;
    public LineRenderer BackLight;
    public void Awake()
    {
        //sun.Radius = radius * 1.1f;
        Application.targetFrameRate = 1000;
        _icosahedronFaces = ConstructIcosahedron();
        Stopwatch stopwatch = new ();
        stopwatch.Start();
        SubdivideIcosahedron();
        stopwatch.Stop();
        UnityEngine.Debug.Log("SUBDIVIDE ICOSAHEDRON: " + stopwatch.ElapsedMilliseconds.ToString());
        ConstructTiles();
        UnityEngine.Debug.Log(_tiles.Count);
    }

    public List<Tile> Tiles => _tiles;

    private List<Face> ConstructIcosahedron()
    {
        const float tao = Mathf.PI / 2;
        const float defaultSize = 100f;
            
        List<Point> icosahedronCorners = new ()
        {
            new Point(new Vector3(defaultSize, tao * defaultSize, 0f)),
            new Point(new Vector3(-defaultSize, tao * defaultSize, 0f)),
            new Point(new Vector3(defaultSize, -tao * defaultSize, 0f)),
            new Point(new Vector3(-defaultSize, -tao * defaultSize, 0f)),
            new Point(new Vector3(0, defaultSize, tao * defaultSize)),
            new Point(new Vector3(0, -defaultSize, tao * defaultSize)),
            new Point(new Vector3(0, defaultSize, -tao * defaultSize)),
            new Point(new Vector3(0, -defaultSize, -tao * defaultSize)),
            new Point(new Vector3(tao * defaultSize, 0f, defaultSize)),
            new Point(new Vector3(-tao * defaultSize, 0f, defaultSize)),
            new Point(new Vector3(tao * defaultSize, 0f, -defaultSize)),
            new Point(new Vector3(-tao * defaultSize, 0f, -defaultSize))
        };
        icosahedronCorners.ForEach(point => CachePoint(point));
            
        return new List<Face>
        {
            new Face(icosahedronCorners[0], icosahedronCorners[1], icosahedronCorners[4], false),
            new Face(icosahedronCorners[1], icosahedronCorners[9], icosahedronCorners[4], false),
            new Face(icosahedronCorners[4], icosahedronCorners[9], icosahedronCorners[5], false),
            new Face(icosahedronCorners[5], icosahedronCorners[9], icosahedronCorners[3], false),
            new Face(icosahedronCorners[2], icosahedronCorners[3], icosahedronCorners[7], false),
            new Face(icosahedronCorners[3], icosahedronCorners[2], icosahedronCorners[5], false),
            new Face(icosahedronCorners[7], icosahedronCorners[10], icosahedronCorners[2], false),
            new Face(icosahedronCorners[0], icosahedronCorners[8], icosahedronCorners[10], false),
            new Face(icosahedronCorners[0], icosahedronCorners[4], icosahedronCorners[8], false),
            new Face(icosahedronCorners[8], icosahedronCorners[2], icosahedronCorners[10], false),
            new Face(icosahedronCorners[8], icosahedronCorners[4], icosahedronCorners[5], false),
            new Face(icosahedronCorners[8], icosahedronCorners[5], icosahedronCorners[2], false),
            new Face(icosahedronCorners[1], icosahedronCorners[0], icosahedronCorners[6], false),
            new Face(icosahedronCorners[3], icosahedronCorners[9], icosahedronCorners[11], false),
            new Face(icosahedronCorners[6], icosahedronCorners[10], icosahedronCorners[7], false),
            new Face(icosahedronCorners[3], icosahedronCorners[11], icosahedronCorners[7], false),
            new Face(icosahedronCorners[11], icosahedronCorners[6], icosahedronCorners[7], false),
            new Face(icosahedronCorners[6], icosahedronCorners[0], icosahedronCorners[10], false),
            new Face(icosahedronCorners[11], icosahedronCorners[1], icosahedronCorners[6], false),
            new Face(icosahedronCorners[9], icosahedronCorners[1], icosahedronCorners[11], false)
        };
    }

    private Point CachePoint(Point point)
    {
        for (int i = 0; i < _points.Count; i++)
        {
            if (Point.IsOverlapping(_points[i], point))
            {
                return _points[i];
            }
        } 
        _points.Add(point);
        return point;
    }

    private void SubdivideIcosahedron()
    {
        _icosahedronFaces.ForEach(icoFace =>
        {
            List<Point> facePoints = icoFace.Points;
            List<Point> previousPoints;
            List<Point> bottomSide = new () {facePoints[0]};
            List<Point> leftSide = facePoints[0].Subdivide(facePoints[1], divisions, CachePoint);
            List<Point> rightSide = facePoints[0].Subdivide(facePoints[2], divisions, CachePoint);
            for (int i = 1; i <= divisions; i++)
            {
                previousPoints = bottomSide;
                bottomSide = leftSide[i].Subdivide(rightSide[i], i, CachePoint);
                for (int j = 0; j < i; j++)
                {
                    //Don't need to store faces, their points will have references to them.
                    new Face(previousPoints[j], bottomSide[j], bottomSide[j+1]);
                    if (j == 0) continue;
                    new Face(previousPoints[j-1], previousPoints[j], bottomSide[j]);
                }
            }
        });
    }

    private void ConstructTiles()
    {
        Stopwatch stopwatch = new ();
        stopwatch.Start();
        Dictionary<string, Tile> tilesID = new ();
        _points.ForEach(point =>
        {
            _tiles.Add(new Tile(point, radius, radius / 300 * 40 / divisions));
            tilesID[point.ID] = _tiles[^1];
        });
        
        _tiles.ForEach(tile => tile.ResolveNeighbourTiles(tilesID));
        stopwatch.Stop();
        UnityEngine.Debug.Log("RESOLVE NEIGHBOURS: " + stopwatch.ElapsedMilliseconds.ToString());
        stopwatch.Restart(); ;
        _mapGen.GenerateMap(_tiles.Count);
        stopwatch.Stop();
        UnityEngine.Debug.Log("GENERATE MAP: " + stopwatch.ElapsedMilliseconds.ToString());
    }
    public Tile GetRandomTile() => _tiles[Random.Range(0, _tiles.Count)];
    public Tile GetTile(int index) => _tiles[index];
    public void ClickOnTile(Tile tile)
    {
        Vector3[] positions = tile._generateMesh.GetPositions();
        Vector3 normal = tile._generateMesh.GetNormal();
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector3(positions[i].x + normal.x/100, positions[i].y + normal.y / 100, positions[i].z + normal.z / 100);
        }
        BackLight.positionCount = positions.Length;
        BackLight.SetPositions(positions);
        ClickedTile = tile;
    }
    public void DisableClicked()
    {
        BackLight.positionCount = 0;
        ClickedTile = null;
    }
}

