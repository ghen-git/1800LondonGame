using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;

public class RoadGenerator : MonoBehaviour
{
    Dictionary<Vector2Int, Block> blockMap = new Dictionary<Vector2Int, Block>();
    int renderDistance;
    float blockSize;
    float roadSize;
    float variationAmount;
    // Start is called before the first frame update
    public void Init()
    {
        blockMap = GetComponent<LondonGenerator>().blockMap;
        renderDistance = GetComponent<LondonGenerator>().renderDistance;
        blockSize = GetComponent<LondonGenerator>().blockSize;
        roadSize = GetComponent<LondonGenerator>().roadSize;
        variationAmount = GetComponent<LondonGenerator>().variationAmount;
    }

    void RenderQuad(Vector2[] vertxs, Vector2 pos, string name)
    {
        GameObject road = new GameObject();

        MeshRenderer meshRenderer = road.AddComponent<MeshRenderer>();

        meshRenderer.material = Resources.Load<Material>("Materials/Ground/Bricks");

        MeshFilter meshFilter = road.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            //bottom quad
            new Vector3(vertxs[0].x, 0, vertxs[0].y), //top left 0
            new Vector3(vertxs[1].x, 0, vertxs[1].y), //top right 1 
            new Vector3(vertxs[2].x, 0, vertxs[2].y), //bottom left 2 
            new Vector3(vertxs[3].x, 0, vertxs[3].y) //bottom right 3
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

        road.transform.position = new Vector3(pos.x, 0, pos.y);
        road.name = name;
        road.AddComponent<MeshCollider>();
    }

    void RenderVertical(Vector2Int block)
    {
        Vector2[] leftRoad = new Vector2[4];
        Vector2[] bottomRoad = new Vector2[4];

        //roads centers
        Vector2 topPos = QuadCenter
        (
            blockMap[block].topRight,
            new Vector2(-blockMap[new Vector2Int(block.x + 1, block.y)].topLeft.x + blockSize, blockMap[new Vector2Int(block.x + 1, block.y)].topLeft.y),
            blockMap[block].bottomRight,
            new Vector2(-blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft.x + blockSize, blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft.y)
        ); 
        topPos = topPos + (new Vector2(block.x, block.y) * blockSize);
 
        Vector2 bottomPos = QuadCenter
        (
            blockMap[new Vector2Int(block.x, block.y + 1)].topRight,
            new Vector2(-blockMap[new Vector2Int(block.x + 1, block.y + 1)].topLeft.x + blockSize, blockMap[new Vector2Int(block.x + 1, block.y + 1)].topLeft.y),
            blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight,
            new Vector2(-blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft.x + blockSize, blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft.y)
        );
        bottomPos = bottomPos + (new Vector2(block.x, block.y + 1) * blockSize);

        //top road top left
        leftRoad[0] = blockMap[block].topRight + (new Vector2(block.x, block.y) * blockSize) - topPos;
        //top road top right
        leftRoad[1] = blockMap[new Vector2Int(block.x + 1, block.y)].topLeft + (new Vector2(block.x + 1, block.y) * blockSize) - topPos;
        //top road bottom left
        leftRoad[2] = blockMap[block].bottomRight + (new Vector2(block.x, block.y) * blockSize) - topPos;
        //top road bottom right
        leftRoad[3] = blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft + (new Vector2(block.x + 1, block.y) * blockSize) - topPos;

        //bottom road top left
        bottomRoad[0] = blockMap[new Vector2Int(block.x, block.y + 1)].topRight + (new Vector2(block.x, block.y + 1) * blockSize) - bottomPos;
        //bottom road top right
        bottomRoad[1] = blockMap[new Vector2Int(block.x + 1, block.y + 1)].topLeft + (new Vector2(block.x + 1, block.y + 1) * blockSize) - bottomPos;
        //bottom road bottom left
        bottomRoad[2] = blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight + (new Vector2(block.x, block.y + 1) * blockSize) - bottomPos;
        //bottom road bottom right
        bottomRoad[3] = blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft + (new Vector2(block.x + 1, block.y + 1) * blockSize) - bottomPos;

        //quads rendering
        RenderQuad(leftRoad, topPos, VectToName(block) + "top");
        RenderQuad(bottomRoad, bottomPos, VectToName(block) + "bottom");
    }

    void RenderHorizontal(Vector2Int block)
    {
        Vector2[] leftRoad = new Vector2[4];
        Vector2[] rightRoad = new Vector2[4];

        //roads centers
        Vector2 leftPos = QuadCenter
        (
            blockMap[block].topLeft,
            blockMap[block].topRight,
            new Vector2(blockMap[new Vector2Int(block.x, block.y + 1)].bottomLeft.x, blockMap[new Vector2Int(block.x, block.y + 1)].bottomLeft.y + blockSize),
            new Vector2(blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight.x, blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight.y + blockSize)
        ); 
        leftPos = leftPos + (new Vector2(block.x, block.y) * blockSize);
 
        Vector2 rightPos = QuadCenter
        (
            blockMap[new Vector2Int(block.x + 1, block.y)].topLeft,
            blockMap[new Vector2Int(block.x + 1, block.y)].topRight,
            new Vector2(blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft.x, blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft.y + blockSize),
            new Vector2(blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomRight.x, blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomRight.y + blockSize)
        ); 
        rightPos = rightPos + (new Vector2(block.x, block.y + 1) * blockSize);

        //left road top left
        leftRoad[0] = blockMap[new Vector2Int(block.x, block.y + 1)].bottomLeft + (new Vector2(block.x, block.y + 1) * blockSize) - leftPos;
        //left road top right
        leftRoad[1] = blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight + (new Vector2(block.x, block.y + 1) * blockSize) - leftPos;
        //left road bottom left
        leftRoad[2] = blockMap[block].topLeft + (new Vector2(block.x, block.y) * blockSize) - leftPos;
        //left road bottom right
        leftRoad[3] = blockMap[block].topRight + (new Vector2(block.x, block.y) * blockSize) - leftPos;

        //right road top left
        rightRoad[0] = blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft + (new Vector2(block.x + 1, block.y + 1) * blockSize) - rightPos;
        //right road top right
        rightRoad[1] = blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomRight + (new Vector2(block.x + 1, block.y + 1) * blockSize) - rightPos;
        //right road bottom left
        rightRoad[2] = blockMap[new Vector2Int(block.x + 1, block.y)].topLeft + (new Vector2(block.x + 1, block.y) * blockSize) - rightPos;
        //right road bottom right
        rightRoad[3] = blockMap[new Vector2Int(block.x + 1, block.y)].topRight + (new Vector2(block.x + 1, block.y) * blockSize) - rightPos;

        //quads rendering
        RenderQuad(leftRoad, leftPos, VectToName(block) + "left");
        RenderQuad(rightRoad, rightPos, VectToName(block) + "right");
    }

    void RenderCenter(Vector2Int block)
    {
        Vector2[] center = new Vector2[4];

        //roads center
        Vector2 pos = QuadCenter
        (
            blockMap[block].topRight,
            new Vector2(-blockMap[new Vector2Int(block.x + 1, block.y)].topLeft.x + blockSize, blockMap[new Vector2Int(block.x + 1, block.y)].topLeft.y),
            blockMap[new Vector2Int(block.x, block.y + 1)].bottomRight,
            new Vector2(-blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft.x + blockSize, blockMap[new Vector2Int(block.x + 1, block.y + 1)].bottomLeft.y + blockSize)
        ); 
        pos = pos + (new Vector2(block.x, block.y) * blockSize);

        //road top left
        center[0] = blockMap[block].bottomRight + (new Vector2(block.x, block.y) * blockSize) - pos;
        //road top right
        center[1] = blockMap[new Vector2Int(block.x + 1, block.y)].bottomLeft + (new Vector2(block.x + 1, block.y) * blockSize) - pos;
        //road bottom left
        center[2] = blockMap[new Vector2Int(block.x, block.y + 1)].topRight + (new Vector2(block.x, block.y + 1) * blockSize) - pos;
        //road bottom right
        center[3] = blockMap[new Vector2Int(block.x + 1, block.y + 1)].topLeft + (new Vector2(block.x + 1, block.y + 1) * blockSize) - pos;

        //quads rendering
        RenderQuad(center, pos, VectToName(block) + "center");
    }

    public void LoadRoads(Vector2Int[] bounds)
    {
        for(int x = bounds[2].x; x < bounds[1].x; x++)
            for(int y = bounds[2].y; y < bounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(x % 2 == 0)
                    RenderHorizontal(block);
                if(y % 2 == 0)
                    RenderVertical(block);

                RenderCenter(block);
            }
    }

    public void LoadRoads(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {
        for(int x = bounds[2].x; x < bounds[1].x; x++)
            for(int y = bounds[2].y; y < bounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(IsInBounds(block, bounds) && !IsInBounds(block, loadedBounds))
                {
                    if(x % 2 == 0)
                        RenderHorizontal(block);
                    if(y % 2 == 0)
                        RenderVertical(block);

                    RenderCenter(block);
                }
            }
    }

    public void UnloadRoads(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {

    }
}
