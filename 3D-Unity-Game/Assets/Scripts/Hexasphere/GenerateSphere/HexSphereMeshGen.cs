using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HexSphereMeshGen
{
    private readonly List<Tile> _tiles = new();
    private List<Face> _icosahedronFaces;
    private readonly List<Point> _points = new();
    private int divisions = 10;
    private readonly Dictionary<Vector3, Point> cachedPoints = new();
    public List<Tile> CreateSphere(int divisions)
    {
        this.divisions = divisions; 
        _icosahedronFaces = ConstructIcosahedron();
        SubdivideIcosahedron();
        ConstructTiles();
        cachedPoints.Clear();
        return _tiles;
    }
    private List<Face> ConstructIcosahedron()
    {
        const float tao = Mathf.PI / 2;
        const float defaultSize = 100f;

        List<Point> icosahedronCorners = new()
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
        if (cachedPoints.ContainsKey(point.Position)) return cachedPoints[point.Position];
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
            List<Point> bottomSide = new() { facePoints[0] };
            List<Point> leftSide = facePoints[0].Subdivide(facePoints[1], divisions, CachePoint);
            List<Point> rightSide = facePoints[0].Subdivide(facePoints[2], divisions, CachePoint);
            for (int i = 1; i <= divisions; i++)
            {
                previousPoints = bottomSide;
                bottomSide = leftSide[i].Subdivide(rightSide[i], i, CachePoint);

                for (int j = 0; j < i; j++)
                {
                    //Don't need to store faces, their points will have references to them.
                    new Face(previousPoints[j], bottomSide[j], bottomSide[j + 1]);
                    if (j == 0) continue;
                    new Face(previousPoints[j - 1], previousPoints[j], bottomSide[j]);
                }
            }
        });
    }

    private void ConstructTiles()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        Dictionary<string, Tile> tilesID = new();
        _points.ForEach(point =>
        {
            _tiles.Add(new Tile(null, point));
            tilesID[point.ID] = _tiles[^1];
        });

        _tiles.ForEach(tile => tile.ResolveNeighbourTiles(tilesID));
    }
}
