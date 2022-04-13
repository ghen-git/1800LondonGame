using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;
using static GraphicsUtil;
using static LondonSettings;

public class RoadGenerator : MonoBehaviour
{
    Dictionary<Vector2Int, Block> blockMap = new Dictionary<Vector2Int, Block>();

    // Start is called before the first frame update
    public void Init()
    {
        blockMap = GetComponent<LondonGenerator>().blockMap;
    }

    void RenderRoad(Vector2Int block)
    {
        Vector2[] verticalRoad = new Vector2[4];
        Vector2[] horizontalRoad = new Vector2[4];

        string verticalType = 
            blockMap[block].end == "west" && blockMap[new Vector2Int(block.x + 1, block.y)].end == "west" ? "west" :
            blockMap[block].end == "east" && blockMap[new Vector2Int(block.x + 1, block.y)].end == "east" ? "east" : "west";
        string horizontalType = 
            blockMap[block].end == "west" && blockMap[new Vector2Int(block.x, block.y + 1)].end == "west" ? "west" :
            blockMap[block].end == "east" && blockMap[new Vector2Int(block.x, block.y + 1)].end == "east" ? "east" : "west";
        
        //frequently used formulas calculation for performance
        Vector2 xy = new Vector2(block.x, block.y) * blockSize;
        Vector2 x1y = new Vector2(block.x + 1, block.y) * blockSize;
        Vector2 xy1 = new Vector2(block.x, block.y + 1) * blockSize;
        Vector2 x1y1 = new Vector2(block.x + 1, block.y + 1) * blockSize;

        //roads centers
        Vector2 verticalCenter = QuadCenter
        (
            blockMap[block].topRight + xy,
            blockMap[new Vector2Int(block.x + 1, block.y)].topLeft + x1y,
            blockMap[block].bottomRight + xy,
            blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft + x1y
        );
        Vector2 horizontalCenter = QuadCenter
        (
            blockMap[new Vector2Int(block.x, block.y + 1)].bottomLeft + xy1,
            blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight + xy1,
            blockMap[block].topLeft + xy,
            blockMap[block].topRight + xy
        );

        //vertical road top left
        verticalRoad[0] = blockMap[block].topRight + xy - verticalCenter;
        //vertical road top right
        verticalRoad[1] = blockMap[new Vector2Int(block.x + 1, block.y)].topLeft + x1y - verticalCenter;
        //vertical road bottom left
        verticalRoad[2] = blockMap[block].bottomRight + xy - verticalCenter;
        //vertical road bottom right
        verticalRoad[3] = blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft + x1y - verticalCenter;

        //horizontal road top left
        horizontalRoad[0] = blockMap[new Vector2Int(block.x, block.y + 1)].bottomLeft + xy1 - horizontalCenter;
        //horizontal road top right
        horizontalRoad[1] = blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight + xy1 - horizontalCenter;
        //horizontal road bottom left
        horizontalRoad[2] = blockMap[block].topLeft + xy - horizontalCenter;
        //horizontal road bottom right
        horizontalRoad[3] = blockMap[block].topRight + xy - horizontalCenter;

        //quads rendering
        GameObject verticalRoadGO = RenderQuad
        (
            verticalRoad, verticalCenter,  VectToName(block) + "vertical",
            Resources.Load<Material>($"Materials/{ (verticalType == "west" ? westEndRoadMat : eastEndRoadMat) }"), 
            verticalType == "west" ? 0.15f : 0.3f
        );
        GameObject horizontalRoadGO = RenderQuad
        (
            horizontalRoad, horizontalCenter,  VectToName(block) + "horizontal", 
            Resources.Load<Material>($"Materials/{ (horizontalType == "west" ? westEndRoadMat : eastEndRoadMat) }"),
            horizontalType == "west" ? 0.15f : 0.3f
        );

        Vector2 lastPoint;

        //vertical sidewalk left
        lastPoint = Vector2.zero;
        foreach(Vector2 sidewalkPoint in blockMap[block].rightSidewalkPoints)
        {
            if(lastPoint.Equals(Vector2.zero))
                lastPoint = sidewalkPoint;
            else
            {
                Line topLine = blockMap[block].rightEdge.PerpendicularAtPoint(lastPoint);
                Line bottomLine = blockMap[block].rightEdge.PerpendicularAtPoint(sidewalkPoint);
                Vector2[] sidewalk = new Vector2[4];
                
                Vector2 top, bottom;

                if(sidewalkPoint.y < lastPoint.y)
                {
                    top = lastPoint;
                    bottom = sidewalkPoint;
                }
                else
                {
                    top = sidewalkPoint;
                    bottom = lastPoint;
                }

                //sidewalk top left
                sidewalk[0] = top + xy - verticalCenter;
                //sidewalk top right
                sidewalk[1] = topLine.PointOnLine(top, topLine.PointFromX(top.x + 1), sidewalkSize) + xy - verticalCenter;
                //sidewalk bottom left
                sidewalk[2] = bottom + xy - verticalCenter;
                //sidewalk bottom right
                sidewalk[3] = bottomLine.PointOnLine(bottom, bottomLine.PointFromX(bottom.x + 1), sidewalkSize) + xy - verticalCenter;

                Vector2 sidewalkCenter = GetGlobalCenter(sidewalk, verticalCenter);

                GameObject sidewalkGO = RenderQuad(GetRelativeVertices(sidewalk, verticalCenter, sidewalkCenter), sidewalkCenter, sidewalkHeight, "", Resources.Load<Material>($"Materials/{ westEndRoadMat }"), 0.15f);
                sidewalkGO.transform.SetParent(verticalRoadGO.transform, true);

                lastPoint = Vector2.zero;
            }
        }
        //horizontal sidewalk bottom
        foreach(Vector2 sidewalkPoint in blockMap[block].topSidewalkPoints)
        {
            debugPoints.Add(sidewalkPoint + xy);
        }
        //horizontal sidewalk top
        foreach(Vector2 sidewalkPoint in blockMap[new Vector2Int(block.x, block.y + 1)].bottomSidewalkPoints)
        {
            debugPoints.Add(sidewalkPoint + xy1);
        }
    }

    List<Vector2> debugPoints = new List<Vector2>();

    void OnDrawGizmos()
    {
        foreach(Vector2 p in debugPoints)
            Gizmos.DrawSphere(new Vector3(p.x, 0, p.y), 1f);
    }
    
    void RenderCenter(Vector2Int block)
    {
        Vector2[] centerRoad = new Vector2[4];
        
        //frequently used formulas calculation for performance
        Vector2 xy = new Vector2(block.x, block.y) * blockSize;
        Vector2 x1y = new Vector2(block.x + 1, block.y) * blockSize;
        Vector2 xy1 = new Vector2(block.x, block.y + 1) * blockSize;
        Vector2 x1y1 = new Vector2(block.x + 1, block.y + 1) * blockSize;

        //roads centers
        Vector2 centerPos = QuadCenter
        (
            blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight + xy1,
            blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft + x1y1,
            blockMap[block].topRight + xy,
            blockMap[new Vector2Int(block.x + 1, block.y)].topLeft + x1y
        );

        //center road top left
        centerRoad[0] = blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight + xy1 - centerPos;
        //center road top right
        centerRoad[1] = blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft + x1y1 - centerPos;
        //center road bottom left
        centerRoad[2] = blockMap[block].topRight + xy - centerPos;
        //center road bottom right
        centerRoad[3] = blockMap[new Vector2Int(block.x + 1, block.y)].topLeft + x1y - centerPos;

        string centerType = 
        blockMap[block].end == "east" &&
        blockMap[new Vector2Int(block.x + 1, block.y)].end == "east" &&
        blockMap[new Vector2Int(block.x, block.y + 1)].end == "east" &&
        blockMap[new Vector2Int(block.x + 1, block.y + 1)].end == "east" ?
        "east" : "west";

        RenderQuad
        (
            centerRoad, centerPos,  VectToName(block) + "center", 
            Resources.Load<Material>($"Materials/{ (centerType == "west" ? westEndRoadMat : eastEndRoadMat) }"),
            centerType == "west" ? 0.15f : 0.3f
        );
    }

    public void LoadRoads(Vector2Int[] bounds)
    {
        for(int x = bounds[2].x; x < bounds[1].x; x++)
            for(int y = bounds[2].y; y < bounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                RenderRoad(block);
                RenderCenter(block);
            }
    }

    public void LoadRoads(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {
        //bounds adjusting to fix road generation on positive coordinates
        Vector2Int[] roadBounds = new Vector2Int[]
        {
            new Vector2Int(loadedBounds[0].x, loadedBounds[0].y - 1),
            new Vector2Int(loadedBounds[1].x - 1, loadedBounds[1].y - 1),
            loadedBounds[2],
            new Vector2Int(loadedBounds[3].x - 1, loadedBounds[3].y)
        };

        for(int x = bounds[2].x; x < bounds[1].x; x++)
            for(int y = bounds[2].y; y < bounds[1].y; y++)
            {
                if
                (
                    blockMap.ContainsKey(new Vector2Int(x + 1, y)) &&
                    blockMap.ContainsKey(new Vector2Int(x, y + 1)) &&
                    blockMap.ContainsKey(new Vector2Int(x + 1, y + 1))
                )
                {
                    Vector2Int block = new Vector2Int(x, y);

                    if(IsInBounds(block, bounds) && !IsInBounds(block, roadBounds))
                    {
                        RenderRoad(block);
                        RenderCenter(block);
                    }
                }
            }
    }

    public void UnloadRoad(Vector2Int block)
    {
        Destroy(GameObject.Find(VectToName(block) + "horizontal"));
        Destroy(GameObject.Find(VectToName(block) + "vertical"));
        
        Destroy(GameObject.Find(VectToName(block) + "center"));
    }

    public void UnloadRoads(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {
        for(int x = loadedBounds[2].x; x <= loadedBounds[1].x; x++)
            for(int y = loadedBounds[2].y; y <= loadedBounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(IsInBounds(block, loadedBounds) && !IsInBounds(block, bounds))
                    UnloadRoad(block);
            }
    }
}
