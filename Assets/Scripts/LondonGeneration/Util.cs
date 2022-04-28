using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{

    public static string VectToName(Vector2Int vect)
    {
        return vect.x.ToString() + ";" + vect.y.ToString();
    }

    public static Vector2Int NameToVect(string name)
    {
        return new Vector2Int(Convert.ToInt32(name.Split(';')[0]), Convert.ToInt32(name.Split(';')[1]));
    }

    public static bool IsInBounds(Vector2Int pos, Vector2Int[] bounds)
    {
        return 
        pos.x >= bounds[2].x && pos.y >= bounds[2].y &&
        pos.x <= bounds[1].x && pos.y <= bounds[1].y;
    }

    public static Vector2 QuadCenter(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
    {
        float xs = topLeft.x + topRight.x + bottomLeft.x + bottomRight.x;
        float ys = topLeft.y + topRight.y + bottomLeft.y + bottomRight.y;

        return new Vector2(xs / 4, ys / 4);
    }

    public static float AngleFrom3Points(Vector2 left, Vector2 center, Vector2 right)
    {
        return Mathf.Atan2(right.y - center.y, right.x - center.x) - Mathf.Atan2(left.y - center.y, left.x - center.x);
    }

    public static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return (1f/2f) * Mathf.Abs((a.x - c.x) * (b.y - a.y) - (a.x - b.x) * (c.y - a.y));
    }

    public static bool PointInQuad(Vector2 p, Block quad)
    {
        float quadArea = TriangleArea(quad.bottomLeft, quad.topLeft, quad.topRight) + TriangleArea(quad.bottomLeft, quad.topRight, quad.bottomRight);

        float pointArea = 
        TriangleArea(quad.bottomLeft, quad.topLeft, p) + TriangleArea(quad.bottomLeft, quad.bottomRight, p) + 
        TriangleArea(quad.topLeft, quad.topRight, p) + TriangleArea(quad.topRight, quad.bottomRight, p);

        return pointArea - quadArea < 0.1f;
    }

    public static bool RandomChance(int percentage, int max = 100)
    {
        return UnityEngine.Random.Range(0, max) <= percentage;
    }
}
