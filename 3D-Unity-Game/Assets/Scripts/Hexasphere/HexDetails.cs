using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class HexDetails 
{
    private List<Point> _points;
    private List<Vector3> _icoPoints;
    private Point _center = new(new());
    private Point _icoCenter;
    private int _height;
    public HexDetails() { }
    public List<Vector3> IcoPoints
    {
        get { return _icoPoints; }
        set { _icoPoints = value; }
    }
    public List<Point> Points
    {
        get { return _points; }
        set { _points = value; }
    }
    public int Height
    {
        get { return _height; }
        set { _height = value; }
    }
    public Point Center
    {
        get { return _center; }
        set { 
            _center = value;
        }
    }
    public Point IcoCenter
    {
        get { return _icoCenter; }
        set { _icoCenter = value; }
    }
}
