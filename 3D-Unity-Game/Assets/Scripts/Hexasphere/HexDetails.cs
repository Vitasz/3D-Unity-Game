using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class HexDetails 
{
    public List<Point> Points { get; set; }
    public List<Vector3> IcoPoints { get; set; }
    public Point Center { get; set; } = Point.Zero;
    public Point IcoCenter { get; set; }
    public int Height { get; set; }
}
