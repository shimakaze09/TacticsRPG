using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Point
{
    #region Fields

    public int x;
    public int y;

    #endregion

    #region Constructors

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    #endregion

    #region Operator Overloads

    public static Point operator +(Point a, Point b)
    {
        return new Point(a.x + b.x, a.y + b.y);
    }

    public static Point operator -(Point p1, Point p2)
    {
        return new Point(p1.x - p2.x, p1.y - p2.y);
    }

    public static bool operator ==(Point a, Point b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Point a, Point b)
    {
        return !(a == b);
    }

    #endregion

    #region Object Overloads

    public override bool Equals(object obj)
    {
        if (obj is Point point) return x == point.x && y == point.y;
        return false;
    }

    public bool Equals(Point p)
    {
        return x == p.x && y == p.y;
    }

    public override int GetHashCode()
    {
        return x ^ y;
    }

    public override string ToString()
    {
        return $"({x},{y})";
    }

    #endregion
}