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
    
    public static bool RandomChance(float passing, float max)
    {
        return UnityEngine.Random.Range(0, max) >= passing;
    }

    public static bool RandomChance(int passing, int max)
    {
        return UnityEngine.Random.Range(0, max) >= passing;
    }
}
