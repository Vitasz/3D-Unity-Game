using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class IronOre : Resourse, GenerateMeshAble
{
    public void Awake()
    {
        hasOwnGroundColor = true;
        color = new (29 / 255, 11 / 255f, 18 / 255f);
        drop = TypeOfItem.IronOre;
    }
    public MeshDetails GetMesh(HexDetails details, ref bool hasOwnGround)
    {
        //color = new Color(100f/256, 100f / 255f, 100f / 255f);
        hasOwnGround = this.hasOwnGroundColor;
        Color mainColor = new (31 / 256f, 37 / 256f, 46 / 256f);
        List<Vector3> up = details.IcoPoints.Select(point => Vector3.Lerp(details.IcoCenter.Position, point, Random.Range(0.5f, 0.75f))).ToList();
        List<Color> colors = new();
        List<Face> faces = new();
        List<Point> points = new();
        foreach (Vector3 point in up)
        {
            points.Add(new Point(point).ProjectToSphere(details.Radius + details.DeltaHeight * (details.Height + Random.Range(1f, 1.5f)), 0.5f));
            colors.Add(Color.Lerp(color, mainColor, Random.Range(0.75f, 0.1f)));
        }
        for (int i = 1; i < points.Count - 1; i++)
        {
            faces.Add(new Face(points[0], points[i], points[i + 1]));
        }
        List<Vector3> down = details.IcoPoints.Select(point => Vector3.Lerp(details.IcoCenter.Position, point, Random.Range(0.8f, 0.85f))).ToList();
        foreach (Vector3 point in down)
        {
            points.Add(new Point(point).ProjectToSphere(details.Radius + details.DeltaHeight * details.Height, 0.5f));
            colors.Add(Color.Lerp(color, mainColor, Random.Range(0.1f, 0.25f)));
        }
        for (int i = 0; i < up.Count; i++)
        {
            faces.Add(new Face(points[i], points[up.Count + i], points[up.Count + (i + 1) % up.Count]));
            faces.Add(new Face(points[(i + 1) % up.Count], points[i], points[up.Count + (i + 1) % up.Count]));

        }
        return new(points, faces, colors, new(),  HexMetrics.hexMaterial);
    }
}
