using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;
using static LondonSettings;

public class Line
{
    public float m;
    public float q;

    public Line(Vector2 a, Vector2 b)
    {
        this.m = (b.y - a.y) / (b.x - a.x);
        this.q = (-a.x) / (b.x - a.x) * (b.y - a.y) + a.y;
    }

    public Line(float m, float q)
    {
        this.m = m;
        this.q = q;
    }

    public Vector2 PointFromX(float x)
    {
        return new Vector2(x, x * m + q);
    }

    public Vector2 PointFromY(float y)
    {
        return new Vector2((y - q) / m, y);
    }

    public bool ContainsPoint(Vector2 p)
    {
        return PointFromX(p.x).y == p.y;
    }

    public static Vector2 MidPoint(Vector2 a, Vector2 b)
    {
        return new Vector2
        (
            (a.x + b.x) / 2,
            (a.y + b.y) / 2
        );
    }
    
    public static Line ParallelAtPoint(Line line, Vector2 a)
    {
        Vector2 b = line.PointFromX(a.x);

        return new Line
        (
            line.m,
            line.q + (a.y - b.y)
        );
    }

    public Line PerpendicularAtPoint(Vector2 p)
    {
        float m = -(1 / this.m);
        float q = p.y - (m * p.x);
        
        return new Line(m, q);
    }

    public Vector2 PointOnLine(Vector2 a, Vector2 b, float distance)
    {
        float xc = a.x - (distance * (a.x - b.x)) / Vector2.Distance(a, b);

        return PointFromX(xc);
    }

    public static Vector2 Intersection(Line a, Line b)
    {
        float x = (-b.q - -a.q) / (-a.m - -b.m);
        float y = (-a.q*-b.m - -b.q*-a.m) / (-a.m - -b.m);
        return new Vector2(x, y);
    }

    public Vector2 PointOnLine(Vector2 p, float distance, bool getRight = true)
    {
        float a = m*m + 1;
        float b = 2 * p.x + 2 * m * q - 2 * p.y * m;
        float c = p.x + q*q + p.y*p.y - 2 * p.y * q - distance*distance;

        float delta = b*b - 4 * a * c;

        float x1 = (-b + Mathf.Sqrt(delta)) / 2 * a;
        float x2 = (-b - Mathf.Sqrt(delta)) / 2 * a;

        Vector2 left, right;

        if(x1 < x2)
        {
            left = PointFromX(x1);
            right = PointFromX(x2);
        }
        else
        {
            left = PointFromX(x2);
            right = PointFromX(x1);
        }

        if(getRight)
            return right;
        else
            return left;
    }
}

public class Block
{
    public GameObject block;
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
    public Vector2 center;
    public bool direction;
    //false: horizontal
    //true: vertical
    public string end;
    public Line leftEdge;
    public Line topEdge;
    public Line rightEdge;
    public Line bottomEdge;
    public List<Vector2> leftSidewalkPoints;
    public List<Vector2> topSidewalkPoints;
    public List<Vector2> rightSidewalkPoints;
    public List<Vector2> bottomSidewalkPoints;

    public Dictionary<Vector2Int, Building> buildings;


    public Block(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
    {
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.bottomLeft = bottomLeft;
        this.bottomRight = bottomRight;

        this.center = QuadCenter(topLeft, topRight, bottomLeft, bottomRight);

        leftEdge = new Line(bottomLeft, topLeft);
        topEdge = new Line(topLeft, topRight);
        rightEdge = new Line(topRight, bottomRight);
        bottomEdge = new Line(bottomRight, bottomLeft);

        leftSidewalkPoints = new List<Vector2>();
        topSidewalkPoints = new List<Vector2>();
        rightSidewalkPoints = new List<Vector2>();
        bottomSidewalkPoints = new List<Vector2>();
    }

    public void CalculateSidewalks(Building building)
    {
        //adds left edge sidewalks
        if(leftEdge.ContainsPoint(building.bottomLeftCorner))
            AddSidewalkPoint(building.bottomLeftCorner, leftSidewalkPoints);
        if(leftEdge.ContainsPoint(building.topLeftCorner))
            AddSidewalkPoint(building.topLeftCorner, leftSidewalkPoints);
        if(leftEdge.ContainsPoint(building.topRightCorner))
            AddSidewalkPoint(building.topRightCorner, leftSidewalkPoints);
        if(leftEdge.ContainsPoint(building.bottomRightCorner))
            AddSidewalkPoint(building.bottomRightCorner, leftSidewalkPoints);

        //adds top edge sidewalks
        if(topEdge.ContainsPoint(building.bottomLeftCorner))
            AddSidewalkPoint(building.bottomLeftCorner, topSidewalkPoints);
        if(topEdge.ContainsPoint(building.topLeftCorner))
            AddSidewalkPoint(building.topLeftCorner, topSidewalkPoints);
        if(topEdge.ContainsPoint(building.topRightCorner))
            AddSidewalkPoint(building.topRightCorner, topSidewalkPoints);
        if(topEdge.ContainsPoint(building.bottomRightCorner))
            AddSidewalkPoint(building.bottomRightCorner, topSidewalkPoints);

        //adds right edge sidewalks
        if(rightEdge.ContainsPoint(building.bottomLeftCorner))
            AddSidewalkPoint(building.bottomLeftCorner, rightSidewalkPoints);
        if(rightEdge.ContainsPoint(building.topLeftCorner))
            AddSidewalkPoint(building.topLeftCorner, rightSidewalkPoints);
        if(rightEdge.ContainsPoint(building.topRightCorner))
            AddSidewalkPoint(building.topRightCorner, rightSidewalkPoints);
        if(rightEdge.ContainsPoint(building.bottomRightCorner))
            AddSidewalkPoint(building.bottomRightCorner, rightSidewalkPoints);

        //adds bottom edge sidewalks
        if(bottomEdge.ContainsPoint(building.bottomLeftCorner))
            AddSidewalkPoint(building.bottomLeftCorner, bottomSidewalkPoints);
        if(bottomEdge.ContainsPoint(building.topLeftCorner))
            AddSidewalkPoint(building.topLeftCorner, bottomSidewalkPoints);
        if(bottomEdge.ContainsPoint(building.topRightCorner))
            AddSidewalkPoint(building.topRightCorner, bottomSidewalkPoints);
        if(bottomEdge.ContainsPoint(building.bottomRightCorner))
            AddSidewalkPoint(building.bottomRightCorner, bottomSidewalkPoints);
    }

    public void AddSidewalkPoint(Vector2 point, List<Vector2> edge)
    {
        Vector2 oldPoint = edge.Find( p => p.x == point.x && p.y == point.y);
        
        if(!oldPoint.Equals(new Vector2()))
            edge.Remove(oldPoint);
        else
            edge.Add(point);
    }

    public static Vector2[] SortSidewalkY(List<Vector2> sidewalkList)
    {
        Vector2[] sidewalk = sidewalkList.ToArray();
        int maxYIndex;
        Vector2 tmp;

        for(int i = 0; i < sidewalk.Length; i++)
        {
            maxYIndex = sidewalk.Length - 1;

            for(int j = i; j < sidewalk.Length; j++)
                if(sidewalk[j].y > sidewalk[maxYIndex].y)
                    maxYIndex = j;

            tmp = sidewalk[i];
            sidewalk[i] = sidewalk[maxYIndex];
            sidewalk[maxYIndex] = tmp;
        }

        return sidewalk;
    }

    public static Vector2[] SortSidewalkX(List<Vector2> sidewalkList)
    {
        Vector2[] sidewalk = sidewalkList.ToArray();
        int maxXIndex;
        Vector2 tmp;

        for(int i = 0; i < sidewalk.Length; i++)
        {
            maxXIndex = sidewalk.Length - 1;

            for(int j = i; j < sidewalk.Length; j++)
                if(sidewalk[j].x > sidewalk[maxXIndex].x)
                    maxXIndex = j;

            tmp = sidewalk[i];
            sidewalk[i] = sidewalk[maxXIndex];
            sidewalk[maxXIndex] = tmp;
        }

        return sidewalk;
    }
}

public class BlockGenerator : MonoBehaviour
{
    Dictionary<Vector2Int, Block> blockMap = new Dictionary<Vector2Int, Block>();
    BuildingGenerator buildingGenerator;

    void Awake()
    {
        LondonGenerator.onLondonGenerationStart += Init;
    }

    public void Init()
    {
        blockMap = GetComponent<LondonGenerator>().blockMap;
        buildingGenerator = GetComponent<BuildingGenerator>();
    }

    void LoadBlock(Vector2Int coords)
    {
        Block block = blockMap[coords];
        RenderBase(coords);
        buildingGenerator.RenderBuildings(block, coords);
    }

    public void LoadBlocks(Vector2Int[] bounds)
    {
        for(int x = bounds[2].x; x <= bounds[1].x; x++)
            for(int y = bounds[2].y; y <= bounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(blockMap.ContainsKey(block))
                    LoadBlock(block);
                else
                {
                    GenerateBlock(block);
                    LoadBlock(block);
                }
            }
    }

    void GenerateBlock(Vector2Int startCoords)
    {
        Block block = GenerateBounds(startCoords);
        
        block.direction = RandomChance(50, 100);

        //smoothly transitions between west and east end "areas" with perlin noise
        float perlinX = perlinOffset.x + (float)(startCoords.x + 10000) / 100 * perlinFrequency;
        float perlinY = perlinOffset.y + (float)(startCoords.y + 10000) / 100 * perlinFrequency;
        block.end = Mathf.PerlinNoise(perlinX, perlinY) > 0.5f ? "east" : "west";

        block.buildings = buildingGenerator.GenerateBuildings(block);

        blockMap.Add(startCoords, block);
    }

    //special quad rendering function that doesnt stretch the block's uv
    //in a specific direction, thus can't use the one in GraphicsUtil
    void RenderBase(Vector2Int coords)
    {
        Block blockPars = blockMap[coords];
        GameObject block = new GameObject();

        MeshRenderer meshRenderer = block.AddComponent<MeshRenderer>();

        meshRenderer.material = Resources.Load<Material>($"Materials/Ground/SecondaryRoad");

        MeshFilter meshFilter = block.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            //bottom quad
            new Vector3(blockPars.topLeft.x, 0, blockPars.topLeft.y), //top left 0
            new Vector3(blockPars.topRight.x, 0, blockPars.topRight.y), //top right 1 
            new Vector3(blockPars.bottomLeft.x, 0, blockPars.bottomLeft.y), //bottom left 2 
            new Vector3(blockPars.bottomRight.x, 0, blockPars.bottomRight.y) //bottom right 3
        };
        mesh.vertices = vertices;

        int[] tris = new int[]
        {
            2, 0, 3,
            0, 1, 3
        };
        mesh.triangles = tris;

        float uvScale = 0.3f;
        
        Vector2[] uv = new Vector2[4]
        {
            blockPars.topLeft * uvScale,
            blockPars.topRight * uvScale,
            blockPars.bottomLeft * uvScale,
            blockPars.bottomRight * uvScale
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        block.transform.position = new Vector3(coords.x * blockSize, 0, coords.y * blockSize);
        block.name = VectToName(coords);
        block.AddComponent<MeshCollider>();
        
        blockPars.block = block;
    }

    //calculates bounds for each block, and does some complex math to
    //snap a new block to the nearest one, if already generated
    Block GenerateBounds(Vector2Int coords)
    {
        Vector2 topLeft, topRight, bottomLeft, bottomRight;

        //top left corner
        if(blockMap.ContainsKey(new Vector2Int(coords.x - 1, coords.y)))
            topLeft = new Vector2
            (
                blockMap[new Vector2Int(coords.x - 1, coords.y)].topRight.x - (blockSize - roadSize), 
                blockMap[new Vector2Int(coords.x - 1, coords.y)].topRight.y
            );
        else if(blockMap.ContainsKey(new Vector2Int(coords.x, coords.y + 1)))
            topLeft = new Vector2
            (
                blockMap[new Vector2Int(coords.x, coords.y + 1)].bottomLeft.x, 
                blockMap[new Vector2Int(coords.x, coords.y + 1)].bottomLeft.y + (blockSize - roadSize) 
            );
        else 
            topLeft = new Vector2
            (
                - ((blockSize - roadSize) / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), 
                ((blockSize - roadSize) / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation)
            );

        //top right corner
        if(blockMap.ContainsKey(new Vector2Int(coords.x + 1, coords.y)))
            topRight = new Vector2
            (
                blockMap[new Vector2Int(coords.x + 1, coords.y)].bottomRight.x, 
                blockMap[new Vector2Int(coords.x + 1, coords.y)].bottomRight.y + (blockSize - roadSize)
            );
        else if(blockMap.ContainsKey(new Vector2Int(coords.x, coords.y + 1)))
            topRight = new Vector2
            (
                blockMap[new Vector2Int(coords.x, coords.y + 1)].bottomRight.x, 
                blockMap[new Vector2Int(coords.x, coords.y + 1)].bottomRight.y + (blockSize - roadSize)
            );
        else
            topRight = new Vector2
            (
                ((blockSize - roadSize) / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), 
                ((blockSize - roadSize) / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation)
            );
            
        //bottom left corner 
        if(blockMap.ContainsKey(new Vector2Int(coords.x - 1, coords.y)))
            bottomLeft = new Vector2
            (
                blockMap[new Vector2Int(coords.x - 1, coords.y)].bottomRight.x - (blockSize - roadSize), 
                blockMap[new Vector2Int(coords.x - 1, coords.y)].bottomRight.y
            );
        else if(blockMap.ContainsKey(new Vector2Int(coords.x, coords.y - 1)))
            bottomLeft = new Vector2
            (
                blockMap[new Vector2Int(coords.x, coords.y - 1)].topLeft.x, 
                blockMap[new Vector2Int(coords.x, coords.y - 1)].topLeft.y - (blockSize - roadSize)
            );
        else
            bottomLeft = new Vector2
            (
                - ((blockSize - roadSize) / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation),
                - ((blockSize - roadSize) / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation)
            );

        //bottom right        
        if(blockMap.ContainsKey(new Vector2Int(coords.x + 1, coords.y)))
            bottomRight = new Vector2
            (
                blockMap[new Vector2Int(coords.x + 1, coords.y)].bottomLeft.x + (blockSize - roadSize), 
                blockMap[new Vector2Int(coords.x + 1, coords.y)].bottomLeft.y
                );
        else if(blockMap.ContainsKey(new Vector2Int(coords.x, coords.y - 1)))
            bottomRight = new Vector2
            (
                blockMap[new Vector2Int(coords.x, coords.y - 1)].topRight.x, 
                blockMap[new Vector2Int(coords.x, coords.y - 1)].topRight.y - (blockSize - roadSize)
            );
        else
            bottomRight = new Vector2
            (
                ((blockSize - roadSize) / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), 
                - ((blockSize - roadSize) / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation)
            );

        return new Block(topLeft, topRight, bottomLeft, bottomRight);
    }

    /*
    Temporairly deprecated function that insets each edge of a block by a specified amount
    doesnt work with road centers needing to be the same size

    Vector2 RoadSpace(Vector2 left, Vector2 center, Vector2 right, Vector2Int startCoords)
    {
        float mainAngle = Util.AngleFrom3Points(left, center, right) / 2;
        float angleToXAxis = Util.AngleFrom3Points(right, center, new Vector2(center.x + 1f, center.y));

        print("main angle_:" + mainAngle);
        print("at angle_:" + angleToXAxis);

        return new Vector2();
    }
    */

    public void LoadBlocks(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {
        for(int x = bounds[2].x; x <= bounds[1].x; x++)
            for(int y = bounds[2].y; y <= bounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(IsInBounds(block, bounds) && !IsInBounds(block, loadedBounds))
                {
                    if(blockMap.ContainsKey(block))
                        LoadBlock(block);
                    else
                    {
                        GenerateBlock(block);
                        LoadBlock(block);
                    }
                }
            }
    }
    public void UnloadBlocks(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {
        for(int x = loadedBounds[2].x; x <= loadedBounds[1].x; x++)
            for(int y = loadedBounds[2].y; y <= loadedBounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(IsInBounds(block, loadedBounds) && !IsInBounds(block, bounds))
                {
                    Destroy(GameObject.Find(VectToName(block)));
                }
            }
    }
}
