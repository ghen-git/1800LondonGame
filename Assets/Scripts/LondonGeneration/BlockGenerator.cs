using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;

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

    public static Vector2 MidPoint(Vector2 a, Vector2 b)
    {
        return new Vector2
        (
            (a.x - b.x) / 2,
            (a.y - b.y) / 2
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

    public Vector2 PointOnLine(Vector2 a, Vector2 b, float distance)
    {
        float xc = a.x - (distance * (a.x - b.x)) / Vector2.Distance(a, b);

        return PointFromX(xc);
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
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
    public Vector2 center;
    public bool direction;
    //false: horizontal
    //true: vertical
    public Line leftEdge;
    public Line topEdge;
    public Line rightEdge;
    public Line bottomEdge;

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
    }
}

public class BlockGenerator : MonoBehaviour
{
    Dictionary<Vector2Int, Block> blockMap = new Dictionary<Vector2Int, Block>();
    int renderDistance;
    float blockSize;
    float roadSize;
    float blockSizeVariation;
    float roadSizeVariation;
    BuildingGenerator buildingGenerator;

    // Start is called before the first frame update
    public void Init()
    {
        blockMap = GetComponent<LondonGenerator>().blockMap;
        renderDistance = GetComponent<LondonGenerator>().renderDistance;
        blockSize = GetComponent<LondonGenerator>().blockSize;
        roadSize = GetComponent<LondonGenerator>().roadSize;
        blockSizeVariation = GetComponent<LondonGenerator>().blockSizeVariation;
        roadSizeVariation = GetComponent<LondonGenerator>().roadSizeVariation;
        buildingGenerator = GetComponent<BuildingGenerator>();
    }

    void LoadBlock(Vector2Int coords)
    {
        Block block = blockMap[coords];
        RenderBlock(coords);
        buildingGenerator.RenderBuildings(block);
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
        
        block.direction = Convert.ToBoolean(UnityEngine.Random.Range(0, 1));

        block.buildings = buildingGenerator.GenerateBuildings(block);

        blockMap.Add(startCoords, block);
    }

    void RenderBlock(Vector2Int coords)
    {
        Block blockPars = blockMap[coords];
        GameObject block = new GameObject();

        MeshRenderer meshRenderer = block.AddComponent<MeshRenderer>();
        //Resources.Load<Material>("Materials/Ground/Bricks");
        meshRenderer.material = new Material(Shader.Find("Standard"));

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

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        block.transform.position = new Vector3(coords.x * blockSize, 0, coords.y * blockSize);
        block.name = VectToName(coords);
        block.AddComponent<MeshCollider>();
    }

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
                blockMap[new Vector2Int(coords.x + 1, coords.y)].topLeft.x + (blockSize - roadSize),
                blockMap[new Vector2Int(coords.x + 1, coords.y)].topLeft.y
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

    Vector2 RoadSpace(Vector2 left, Vector2 center, Vector2 right, Vector2Int startCoords)
    {

        Vector2 left1 = new Vector2(left.x - center.x, left.y - center.y);
        Vector2 center1 = new Vector2(0, 0);
        Vector2 right1 = new Vector2(right.x - center.x, right.y - center.y);

        float angle = Mathf.Atan2(right1.y, right1.x) - Mathf.Atan2(left1.y, left1.x);

        float otherLeg = (roadSize / 2) * Mathf.Tan((Mathf.PI/2) - (angle / 2));
        float hypothenuse = Mathf.Sqrt(Mathf.Pow(roadSize / 2, 2) + Mathf.Pow(otherLeg, 2));

        Vector2 newPoint = new Vector2(Mathf.Cos(angle / 2), Mathf.Sin(angle / 2)) * hypothenuse;
        newPoint = new Vector2(center.x - newPoint.x, center.y - newPoint.y);

        print(newPoint);

        return newPoint;
    }

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
