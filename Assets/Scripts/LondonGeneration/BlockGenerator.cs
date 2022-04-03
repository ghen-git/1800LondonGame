using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;
public class Block
{
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;


    public Block(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
    {
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.bottomLeft = bottomLeft;
        this.bottomRight = bottomRight;
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

    List<Vector2> newPoints;

    // Start is called before the first frame update
    public void Init()
    {
        blockMap = GetComponent<LondonGenerator>().blockMap;
        renderDistance = GetComponent<LondonGenerator>().renderDistance;
        blockSize = GetComponent<LondonGenerator>().blockSize;
        roadSize = GetComponent<LondonGenerator>().roadSize;
        blockSizeVariation = GetComponent<LondonGenerator>().blockSizeVariation;
        roadSizeVariation = GetComponent<LondonGenerator>().roadSizeVariation;

        newPoints = new List<Vector2>();
    }

    public void LoadBlocks(Vector2Int[] bounds)
    {
        for(int x = bounds[2].x; x <= bounds[1].x; x++)
            for(int y = bounds[2].y; y <= bounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(blockMap.ContainsKey(block))
                    RenderBlock(block);
                else
                {
                    GenerateBlock(block);
                    RenderBlock(block);
                }
            }
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

    void GenerateBlock(Vector2Int startCoords)
    {
        Vector2 topLeft, topRight, bottomLeft, bottomRight;

        //top left corner
        if(blockMap.ContainsKey(new Vector2Int(startCoords.x - 1, startCoords.y)))
            topLeft = new Vector2
            (
                blockMap[new Vector2Int(startCoords.x - 1, startCoords.y)].topRight.x - blockSize, 
                blockMap[new Vector2Int(startCoords.x - 1, startCoords.y)].topRight.y
            );
        else if(blockMap.ContainsKey(new Vector2Int(startCoords.x, startCoords.y + 1)))
            topLeft = new Vector2
            (
                blockMap[new Vector2Int(startCoords.x, startCoords.y + 1)].bottomLeft.x, 
                blockMap[new Vector2Int(startCoords.x, startCoords.y + 1)].bottomLeft.y + blockSize
            );
        else
            topLeft = new Vector2
            (
                - (blockSize / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), 
                (blockSize / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation)
            );

        //top right corner
        if(blockMap.ContainsKey(new Vector2Int(startCoords.x + 1, startCoords.y)))
            topRight = new Vector2
            (
                blockMap[new Vector2Int(startCoords.x + 1, startCoords.y)].topLeft.x + blockSize,
                blockMap[new Vector2Int(startCoords.x + 1, startCoords.y)].topLeft.y
            );
        else if(blockMap.ContainsKey(new Vector2Int(startCoords.x, startCoords.y + 1)))
            topRight = new Vector2
            (
                blockMap[new Vector2Int(startCoords.x, startCoords.y + 1)].bottomRight.x, 
                blockMap[new Vector2Int(startCoords.x, startCoords.y + 1)].bottomRight.y + blockSize
            );
        else
            topRight = new Vector2
            (
                (blockSize / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), 
                (blockSize / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation)
            );
            
        //bottom left corner
        if(blockMap.ContainsKey(new Vector2Int(startCoords.x - 1, startCoords.y)))
            bottomLeft = new Vector2
            (
                blockMap[new Vector2Int(startCoords.x - 1, startCoords.y)].bottomRight.x - blockSize, 
                blockMap[new Vector2Int(startCoords.x - 1, startCoords.y)].bottomRight.y
            );
        else if(blockMap.ContainsKey(new Vector2Int(startCoords.x, startCoords.y - 1)))
            bottomLeft = new Vector2
            (
                blockMap[new Vector2Int(startCoords.x, startCoords.y - 1)].topLeft.x, 
                blockMap[new Vector2Int(startCoords.x, startCoords.y - 1)].topLeft.y - blockSize
            );
        else
            bottomLeft = new Vector2
            (
                - (blockSize / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation),
                - (blockSize / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation)
            );

        //bottom right        
        if(blockMap.ContainsKey(new Vector2Int(startCoords.x + 1, startCoords.y)))
            bottomRight = new Vector2
            (
                blockMap[new Vector2Int(startCoords.x + 1, startCoords.y)].bottomLeft.x + blockSize, 
                blockMap[new Vector2Int(startCoords.x + 1, startCoords.y)].bottomLeft.y
                );
        else if(blockMap.ContainsKey(new Vector2Int(startCoords.x, startCoords.y - 1)))
            bottomRight = new Vector2
            (
                blockMap[new Vector2Int(startCoords.x, startCoords.y - 1)].topRight.x, 
                blockMap[new Vector2Int(startCoords.x, startCoords.y - 1)].topRight.y - blockSize
            );
        else
            bottomRight = new Vector2
            (
                (blockSize / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), 
                - (blockSize / 2) + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation)
            );

        RoadSpace(bottomLeft, topLeft, topRight, startCoords);
        RoadSpace(topLeft, topRight, bottomRight, startCoords);
        RoadSpace(topRight, bottomRight, bottomLeft, startCoords);
        RoadSpace(bottomRight, bottomLeft, topLeft, startCoords);

        Block block = new Block(topLeft, topRight, bottomLeft, bottomRight);
        blockMap.Add(startCoords, block);
    }

    Vector2 RoadSpace(Vector2 left, Vector2 center, Vector2 right, Vector2Int startCoords)
    {

        /*
        

        */

        float mainAngle = Util.AngleFrom3Points(left, center, right) / 2;
        float angleToXAxis = Util.AngleFrom3Points(right, center, new Vector2(center.x + 1f, center.y));

        print("main angle_:" + mainAngle);
        print("at angle_:" + angleToXAxis);

        return new Vector2();
    }

    void OnDrawGizmos()
    {
        foreach(Vector2 point in newPoints)
            Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), 1f);
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
                        RenderBlock(block);
                    else
                    {
                        GenerateBlock(block);
                        RenderBlock(block);
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
