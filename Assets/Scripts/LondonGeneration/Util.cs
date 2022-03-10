using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static bool IsInBounds(Vector2Int pos, Vector2Int[] bounds)
    {
        return 
        pos.x >= bounds[2].x && pos.y >= bounds[2].y &&
        pos.x <= bounds[1].x && pos.y <= bounds[1].y;
    }
}
