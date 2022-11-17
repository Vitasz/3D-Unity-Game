using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Stone : Resourse, GenerateMeshAble
{
    public void Awake()
    {
        hasOwnGroundColor = true;
        color = new Color(50f / 256, 50f / 255f, 50f / 255f);
        drop = TypeOfItem.Stone;
    }
    public MeshDetails GetMesh(HexDetails details, ref bool hasOwnGround)
    {
        hasOwnGround = this.hasOwnGroundColor;
        Color mainColor = new Color(106f / 256f, 106f / 256f, 106f / 256f);
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
        return new(points, faces, colors, new(), HexMetrics.stoneMaterial);
    }
}
