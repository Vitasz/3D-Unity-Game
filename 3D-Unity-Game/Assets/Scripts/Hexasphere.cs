using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Hexasphere: MonoBehaviour
{
    public readonly List<Tile> _tiles = new ();
    private List<Face> _icosahedronFaces;
    private readonly List<Point> _points = new ();
    public HexSphereGenerator _mapGen;
    [Min(5f)]
    private float radius = 40f;
    [Range(1, 100)]
    [SerializeField] public int divisions = 10;
    public CameraSphere CameraSphere;
    public Sun sun;
    public Tile ClickedTile = null;
    public LineRenderer BackLight;
    private float delta_height = 0;
    public GameObject InsideSphere;
    Dictionary<Vector3, Point> cachedPoints = new();
    Dictionary<Collider, Chunk> colliders = new();
    HashSet<Chunk> Loaded = new();
    [Range(0, 100)]
    public int Range = 1;
    Tile prevLoad = null;
    public void Awake()
    {
        //sun.Radius = radius * 1.1f;
        //Application.targetFrameRate = 1000;
        radius *= divisions / 10f;
        InsideSphere.transform.localScale = new Vector3(radius - 0.1f, radius - 0.1f, radius - 0.1f);
        delta_height = radius / 300 * 40 / divisions;
        CameraSphere.zoomMin = radius / 2;
        CameraSphere.zoomMax = radius*1.5f;
        CameraSphere.offset.z = radius * 1.5f;

        _icosahedronFaces = ConstructIcosahedron();
        Stopwatch stopwatch = new ();
        stopwatch.Start();
        SubdivideIcosahedron();
        stopwatch.Stop();
        UnityEngine.Debug.Log("SUBDIVIDE ICOSAHEDRON: " + stopwatch.ElapsedMilliseconds.ToString());
        ConstructTiles();
        CameraSphere.zoomMin += 20 * delta_height;
        UnityEngine.Debug.Log(_tiles.Count);
        cachedPoints.Clear();
    }

    public List<Tile> Tiles => _tiles;

    public void AddChunk(Collider col, Chunk chunk)
    {
        colliders[col] = chunk;
    }
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
        if (cachedPoints.ContainsKey(point.Position))return cachedPoints[point.Position];
        cachedPoints.Add(point.Position, point);
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
                UnityEngine.Debug.Log(bottomSide);
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
            _tiles.Add(new Tile(point, radius, delta_height));
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
    
    public void FixedUpdate()
    {
        Ray MyRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2));
        Physics.Raycast(MyRay, out RaycastHit hit, 100);
        Vector3 p = hit.point;
        Collider col = hit.collider;
        Tile ans = null;
        if (col == null) return;
        if (!colliders.ContainsKey(col))
        {
            float minDistance = float.MaxValue;
            foreach (Tile tile in _tiles)
            {
                Vector3 now = tile._generateMesh.GetCenter();
                if (Vector3.Distance(now, p) < minDistance)
                {
                    minDistance = Vector3.Distance(now, p);
                    ans = tile;
                }
            }
        }
        else ans = colliders[col].GetTile(p);
        if (ans == prevLoad) return;
        prevLoad = ans;
        HashSet<Tile> nowLoad = new() { ans };
        Queue<Tile> check = new();
        check.Enqueue(ans);
        HashSet<Chunk> toOn = new();
        for (int i = 0; i <= Range; i++)
        {
            int sizeQueue = check.Count;
            while(sizeQueue-->0)
            {
                Tile now = check.Dequeue();
                toOn.Add(now.chunk);
                foreach (Tile tile in now.Neighbours)
                {
                    if (!nowLoad.Contains(tile))
                    {
                        nowLoad.Add(tile);
                        check.Enqueue(tile);
                    }
                }
            }
        }
        HashSet<Chunk> toRemove = new();
        foreach (Chunk chunk in Loaded) {
            if (!toOn.Contains(chunk))
            {
                chunk._meshRenderer.enabled = false;
                chunk.transform.GetChild(0).gameObject.SetActive(false);
                toRemove.Add(chunk);
            }
            else toOn.Remove(chunk);
            
        }

        foreach (Chunk chunk in toRemove) 
            Loaded.Remove(chunk);

        foreach (Chunk chunk in toOn)
        {
            chunk._meshRenderer.enabled = true;
            chunk.transform.GetChild(0).gameObject.SetActive(true);
            Loaded.Add(chunk);
        }
    }
    
}

