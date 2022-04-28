using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;
using static GraphicsUtil;
using static LondonSettings;

public class Road
{
    public Vector2 center;
    public GameObject road;
    public string end;
    public Vector2[] vertices;

    public Road(Vector2 center, GameObject road, string end, Vector2[] vertices)
    {
        this.center = center;
        this.road = road;
        this.end = end;
        this.vertices = vertices;
    }
}

public class RoadGenerator : MonoBehaviour
{
    Dictionary<Vector2Int, Block> blockMap = new Dictionary<Vector2Int, Block>();

    void Awake()
    {
        LondonGenerator.onLondonGenerationStart += Init;
    }

    public void Init()
    {
        blockMap = GetComponent<LondonGenerator>().blockMap;
    }

    void RenderRoad(Vector2Int block)
    {
        Vector2[] verticalVertices = new Vector2[4];
        Vector2[] horizontalVertices = new Vector2[4];

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
        verticalVertices[0] = blockMap[block].topRight + xy - verticalCenter;
        //vertical road top right
        verticalVertices[1] = blockMap[new Vector2Int(block.x + 1, block.y)].topLeft + x1y - verticalCenter;
        //vertical road bottom left
        verticalVertices[2] = blockMap[block].bottomRight + xy - verticalCenter;
        //vertical road bottom right
        verticalVertices[3] = blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft + x1y - verticalCenter;

        //horizontal road top left
        horizontalVertices[0] = blockMap[new Vector2Int(block.x, block.y + 1)].bottomLeft + xy1 - horizontalCenter;
        //horizontal road top right
        horizontalVertices[1] = blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight + xy1 - horizontalCenter;
        //horizontal road bottom left
        horizontalVertices[2] = blockMap[block].topLeft + xy - horizontalCenter;
        //horizontal road bottom right
        horizontalVertices[3] = blockMap[block].topRight + xy - horizontalCenter;

        //quads rendering
        GameObject verticalRoadGO = RenderQuad
        (
            verticalVertices, verticalCenter,  VectToName(block) + "vertical",
            Resources.Load<Material>($"Materials/{ (verticalType == "west" ? westEndRoadMat : eastEndRoadMat) }"), 
            verticalType == "west" ? 0.15f : 0.3f
        );
        GameObject horizontalRoadGO = RenderQuad
        (
            horizontalVertices, horizontalCenter,  VectToName(block) + "horizontal", 
            Resources.Load<Material>($"Materials/{ (horizontalType == "west" ? westEndRoadMat : eastEndRoadMat) }"),
            horizontalType == "west" ? 0.15f : 0.3f
        );

        Road verticalRoad = new Road(verticalCenter, verticalRoadGO, verticalType, verticalVertices);
        Road horizontalRoad = new Road(horizontalCenter, horizontalRoadGO, horizontalType, horizontalVertices);

        GenerateSidewalks(block, xy, x1y, xy1, horizontalRoad, verticalRoad);

        //other objects placement

        //fence posts
        GameObject fencePost = Resources.Load<GameObject>("Prefabs/FencePost");
        PlaceAtOffset(fencePost, fencePostDistance, fencePostOffset, block, verticalRoad, true);
        PlaceAtOffset(fencePost, fencePostDistance, fencePostOffset, block, horizontalRoad, false);
    }

    void PlaceRandom()
    {

    }

    void PlaceAtOffset(GameObject prefab, float distance, float offset, Vector2Int block, Road road, bool vertical)
    {
        float leftLength;
        float rightLength;
        Vector2 startingLeft;
        Vector2 startingRight;
        Line leftLine;
        Line rightLine;
        Vector2 objectPos;     

        //frequently used formulas calculation for performance
        Vector2 xy = new Vector2(block.x, block.y) * blockSize;
        Vector2 x1y = new Vector2(block.x + 1, block.y) * blockSize;
        Vector2 xy1 = new Vector2(block.x, block.y + 1) * blockSize;

        if(vertical)
        {
            leftLength = Vector2.Distance(blockMap[block].bottomRight, blockMap[block].topRight);
            rightLength = Vector2.Distance(blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft, blockMap[new Vector2Int(block.x + 1, block.y)].topLeft);

            startingLeft = blockMap[block].rightEdge.PerpendicularAtPoint(blockMap[block].bottomRight).PointOnLine
            (
                blockMap[block].bottomRight, 
                blockMap[block].rightEdge.PerpendicularAtPoint(blockMap[block].bottomRight).PointFromX(blockMap[block].bottomRight.x + offsetTolerance), 
                offset
            );
            startingRight = blockMap[new Vector2Int(block.x + 1, block.y)].leftEdge.PerpendicularAtPoint(blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft).PointOnLine
            (
                blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft, 
                blockMap[new Vector2Int(block.x + 1, block.y)].leftEdge.PerpendicularAtPoint(blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft).PointFromX(blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft.x - offsetTolerance), 
                offset
            );

            leftLine = Line.ParallelAtPoint
            (
                blockMap[block].rightEdge,
                startingLeft
            );
            rightLine = Line.ParallelAtPoint
            (
                blockMap[new Vector2Int(block.x + 1, block.y)].leftEdge, 
                startingRight
            );

            for(float i = distance / 2; i < leftLength || i < rightLength; i += distance)
            {
                if(i < leftLength)
                {
                    objectPos = leftLine.PointOnLine(startingLeft, leftLine.PointFromY(startingLeft.y + offsetTolerance), i) + xy;
                    GameObject itemGO = GameObject.Instantiate(prefab);
                    itemGO.name = "";
                    itemGO.transform.position = new Vector3(objectPos.x, 0, objectPos.y);
                    itemGO.transform.SetParent(road.road.transform, true);
                }
                if(i < rightLength)
                {
                    objectPos = rightLine.PointOnLine(startingRight, rightLine.PointFromY(startingRight.y + offsetTolerance), i) + x1y;
                    GameObject itemGO = GameObject.Instantiate(prefab);
                    itemGO.name = "";
                    itemGO.transform.position = new Vector3(objectPos.x, 0, objectPos.y);
                    itemGO.transform.SetParent(road.road.transform, true);
                }
            }
        }
        else
        {
            leftLength = Vector2.Distance(blockMap[block].topRight, blockMap[block].topLeft);
            rightLength = Vector2.Distance(blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight, blockMap[new Vector2Int(block.x, block.y + 1)].bottomLeft);

            startingLeft = blockMap[block].topEdge.PerpendicularAtPoint(blockMap[block].topRight).PointOnLine
            (
                blockMap[block].topRight, 
                blockMap[block].topEdge.PerpendicularAtPoint(blockMap[block].topRight).PointFromY(blockMap[block].topRight.y + offsetTolerance), 
                offset
            );
            startingRight = blockMap[new Vector2Int(block.x, block.y + 1)].bottomEdge.PerpendicularAtPoint(blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight).PointOnLine
            (
                blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight, 
                blockMap[new Vector2Int(block.x, block.y + 1)].bottomEdge.PerpendicularAtPoint(blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight).PointFromY(blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight.y - offsetTolerance), 
                offset
            );

            leftLine = Line.ParallelAtPoint
            (
                blockMap[block].topEdge,
                startingLeft
            );
            rightLine = Line.ParallelAtPoint
            (
                blockMap[new Vector2Int(block.x, block.y + 1)].bottomEdge, 
                startingRight
            );

            for(float i = distance / 2; i < leftLength || i < rightLength; i += distance)
            {
                if(i < leftLength)
                {
                    objectPos = leftLine.PointOnLine(startingLeft, leftLine.PointFromX(startingLeft.y - offsetTolerance), i) + xy;
                    GameObject itemGO = GameObject.Instantiate(prefab);
                    itemGO.name = "";
                    itemGO.transform.position = new Vector3(objectPos.x, 0, objectPos.y);
                    itemGO.transform.SetParent(road.road.transform, true);
                }
                if(i < rightLength)
                {
                    objectPos = rightLine.PointOnLine(startingRight, rightLine.PointFromX(startingRight.y - offsetTolerance), i) + xy1;
                    GameObject itemGO = GameObject.Instantiate(prefab);
                    itemGO.name = "";
                    itemGO.transform.position = new Vector3(objectPos.x, 0, objectPos.y);
                    itemGO.transform.SetParent(road.road.transform, true);
                }
            }
        }
    }

    void GenerateSidewalks(Vector2Int block, Vector2 xy, Vector2 x1y, Vector2 xy1, Road verticalRoad, Road horizontalRoad)
    {
        if(verticalRoad.end == "west")
        {
            //vertical sidewalk left
            RenderSidewalk
            (
                Block.SortSidewalkY(blockMap[block].rightSidewalkPoints),
                blockMap[block].rightEdge,
                (Vector2 sidewalkPoint, Vector2 lastPoint) =>
                {
                    return sidewalkPoint.y < lastPoint.y;
                },
                (Vector2 bottom, Vector2 top, Line bottomLine, Line topLine) => 
                {
                    return new Vector2[4]
                    {
                        //sidewalk top left
                        top + xy - verticalRoad.center,
                        //sidewalk top right
                        topLine.PointOnLine(top, topLine.PointFromX(top.x + offsetTolerance), sidewalkSize) + xy - verticalRoad.center,
                        //sidewalk bottom left
                        bottom + xy - verticalRoad.center,
                        //sidewalk bottom right
                        bottomLine.PointOnLine(bottom, bottomLine.PointFromX(bottom.x + offsetTolerance), sidewalkSize) + xy - verticalRoad.center
                    };
                },
                verticalRoad.center,
                verticalRoad.road,
                new bool[6]{false, true, true, true, true, false}
            );
            
            //vertical sidewalk right
            RenderSidewalk
            (
                Block.SortSidewalkY(blockMap[new Vector2Int(block.x + 1, block.y)].leftSidewalkPoints),
                blockMap[new Vector2Int(block.x + 1, block.y)].leftEdge,
                (Vector2 sidewalkPoint, Vector2 lastPoint) =>
                {
                    return sidewalkPoint.y < lastPoint.y;
                },
                (Vector2 bottom, Vector2 top, Line bottomLine, Line topLine) => 
                {
                    return new Vector2[4]
                    {
                        //sidewalk top left
                        topLine.PointOnLine(top, topLine.PointFromX(top.x - offsetTolerance), sidewalkSize) + x1y - verticalRoad.center,
                        //sidewalk top right
                        top + x1y - verticalRoad.center,
                        //sidewalk bottom left
                        bottomLine.PointOnLine(bottom, bottomLine.PointFromX(bottom.x - offsetTolerance), sidewalkSize) + x1y - verticalRoad.center,
                        //sidewalk bottom right
                        bottom + x1y - verticalRoad.center
                    };
                },
                verticalRoad.center,
                verticalRoad.road,
                new bool[6]{true, true, false, true, true, false}
            );
        }

        if(horizontalRoad.end == "west")
        {
            //horizontal sidewalk bottom
            RenderSidewalk
            (
                Block.SortSidewalkX(blockMap[block].topSidewalkPoints),
                blockMap[block].topEdge,
                (Vector2 sidewalkPoint, Vector2 lastPoint) =>
                {
                    return sidewalkPoint.x < lastPoint.x;
                },
                (Vector2 bottom, Vector2 top, Line bottomLine, Line topLine) => 
                {
                    return new Vector2[4]
                    {
                        //sidewalk top left
                        bottomLine.PointOnLine(bottom, bottomLine.PointFromY(bottom.y + offsetTolerance), sidewalkSize) + xy - horizontalRoad.center,
                        //sidewalk top right
                        topLine.PointOnLine(top, topLine.PointFromY(top.y + offsetTolerance), sidewalkSize) + xy - horizontalRoad.center,
                        //sidewalk bottom left
                        bottom + xy - horizontalRoad.center,
                        //sidewalk bottom right
                        top + xy - horizontalRoad.center
                    };
                },
                horizontalRoad.center,
                horizontalRoad.road,
                new bool[6]{true, true, true, false, true, false}
            );

            //horizontal sidewalk top
            RenderSidewalk
            (
                Block.SortSidewalkX(blockMap[new Vector2Int(block.x, block.y + 1)].bottomSidewalkPoints),
                blockMap[new Vector2Int(block.x, block.y + 1)].bottomEdge,
                (Vector2 sidewalkPoint, Vector2 lastPoint) =>
                {
                    return sidewalkPoint.x < lastPoint.x;
                },
                (Vector2 bottom, Vector2 top, Line bottomLine, Line topLine) => 
                {
                    return new Vector2[4]
                    {
                        //sidewalk top left
                        bottom + xy1 - horizontalRoad.center,
                        //sidewalk top right
                        top + xy1 - horizontalRoad.center,
                        //sidewalk bottom left
                        bottomLine.PointOnLine(bottom, bottomLine.PointFromY(bottom.y - offsetTolerance), sidewalkSize) + xy1 - horizontalRoad.center,
                        //sidewalk bottom right
                        topLine.PointOnLine(top, topLine.PointFromY(top.y - offsetTolerance), sidewalkSize) + xy1 - horizontalRoad.center
                    };
                },
                horizontalRoad.center,
                horizontalRoad.road,
                new bool[6]{true, false, true, true, true, false}
            );
        }
    }

    delegate Vector2[] SidewalkBoundsGenerator(Vector2 bottom, Vector2 top, Line bottomLine, Line topLine);
    delegate bool SidewalkTopBottomEvaluator(Vector2 sidewalkPoint, Vector2 lastPoint);

    void RenderSidewalk(Vector2[] sidewalkPoints, Line blockEdge, SidewalkTopBottomEvaluator evaluateTopBottom, SidewalkBoundsGenerator generateBounds, Vector2 roadCenter, GameObject roadGO, bool[] renderedSides)
    {
        Vector2 lastPoint = Vector2.zero;
        foreach(Vector2 sidewalkPoint in sidewalkPoints)
        {
            if(lastPoint.Equals(Vector2.zero))
                lastPoint = sidewalkPoint;
            else
            {
                Line topLine = blockEdge.PerpendicularAtPoint(lastPoint);
                Line bottomLine = blockEdge.PerpendicularAtPoint(sidewalkPoint);
                Vector2[] sidewalk = new Vector2[4];
                
                Vector2 top, bottom;

                if(evaluateTopBottom(sidewalkPoint, lastPoint))
                {
                    top = lastPoint;
                    bottom = sidewalkPoint;
                }
                else
                {
                    top = sidewalkPoint;
                    bottom = lastPoint;
                }

                sidewalk = generateBounds(bottom, top, bottomLine, topLine);

                Vector2 sidewalkCenter = GetGlobalCenter(sidewalk, roadCenter);

                GameObject sidewalkGO = RenderQuad
                (
                    GetRelativeVertices(sidewalk, roadCenter, sidewalkCenter), 
                    sidewalkCenter, 
                    sidewalkHeight, 
                    "", 
                    Resources.Load<Material>($"Materials/{ sidewalkMat }"), 
                    sidewalkMatScale,
                    renderedSides,
                    new Vector2(1.5f, 1.5f)
                );
                sidewalkGO.transform.SetParent(roadGO.transform, true);

                lastPoint = Vector2.zero;
            }
        }
    }

    // List<Vector2> debugPoints = new List<Vector2>();

    // void OnDrawGizmos()
    // {
    //     foreach(Vector2 p in debugPoints)
    //         Gizmos.DrawSphere(new Vector3(p.x, 0, p.y), 1f);
    // }
    
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
