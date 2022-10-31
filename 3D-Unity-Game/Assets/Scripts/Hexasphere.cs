using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Hexasphere: MonoBehaviour
{
    private readonly List<Tile> _tiles = new List<Tile>();
    private List<Face> _icosahedronFaces;
    private List<Point> _points = new List<Point>();
    public HexSphereGenerator _mapGen;
    [Min(5f)]
    [SerializeField] private float radius = 10f;
    [Range(1, 50)]
    [SerializeField] private int divisions = 4;
    [Range(0.1f, 1f)]
    [SerializeField] private float hexSize = 1f;
    public GameObject ForestPrefab, CoalPrefab;
    public GameObject TilePrefab;
    public CameraSphere CameraSphere;

    public void Awake()
    { 
        _icosahedronFaces = ConstructIcosahedron();
        SubdivideIcosahedron();
        ConstructTiles();
        UpdateMesh();
        Debug.Log(_tiles.Count);
    }

    public List<Tile> Tiles => _tiles;

    private List<Face> ConstructIcosahedron()
    {
        const float tao = Mathf.PI / 2;
        const float defaultSize = 100f;
            
        List<Point> icosahedronCorners = new List<Point>
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
        Point existingPoint = _points.FirstOrDefault(candidatePoint => Point.IsOverlapping(candidatePoint, point));
        if (existingPoint != null)
        {
            return existingPoint;
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
            List<Point> bottomSide = new List<Point> {facePoints[0]};
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
        _points.ForEach(point =>
        {
            _tiles.Add(GameObject.Instantiate(TilePrefab, transform).GetComponent<Tile>());
            _tiles[_tiles.Count - 1].CreateTile(point, radius, hexSize, this);
        });
        _tiles.ForEach(tile => tile.ResolveNeighbourTiles(_tiles));
        //_tiles.ForEach(tile => tile.BuildBridges());
        //_tiles.ForEach(tile => tile.BuildTriangles());
        //_tiles.ForEach(tile => {
        //    if (tile._type == Type_of_Tiles.Ground && Random.value < 0.8f) tile.addResourse(ForestPrefab);
        //    if ((tile._type == Type_of_Tiles.Sand) && Random.value < 0.15f) tile.addResourse(CoalPrefab);
        //    else if (tile._type == Type_of_Tiles.Mountains && Random.value < 0.65f) tile.addResourse(CoalPrefab);
        //});
        //_tiles.ForEach(tile => _mapGen.GenerateMap(tile));
        _mapGen.GenerateMap(_tiles.Count);
        //_tiles.ForEach(tile =>
        //{
        ///    if (tile.Height == -1) _mapGen.initHeight(tile);
        //});
    }
    public Tile GetRandomTile() => _tiles[Random.Range(0, _tiles.Count)];
    public Tile GetTile(int index) => _tiles[index];
    public void UpdateMesh() => _tiles.ForEach(tile => tile.RecalculateMesh());
}

