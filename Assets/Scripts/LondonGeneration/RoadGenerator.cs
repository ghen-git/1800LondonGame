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
    float blockSizeVariation;
    float roadSizeVariation;
    // Start is called before the first frame update
    public void Init()
    {
        blockMap = GetComponent<LondonGenerator>().blockMap;
        renderDistance = GetComponent<LondonGenerator>().renderDistance;
        blockSize = GetComponent<LondonGenerator>().blockSize;
        roadSize = GetComponent<LondonGenerator>().roadSize;
        blockSizeVariation = GetComponent<LondonGenerator>().blockSizeVariation;
        roadSizeVariation = GetComponent<LondonGenerator>().roadSizeVariation;
    }

    void RenderQuad(Vector2[] vertxs, Vector2 pos, string name)
    {
        GameObject road = new GameObject();

        MeshRenderer meshRenderer = road.AddComponent<MeshRenderer>();

        meshRenderer.material = Resources.Load<Material>("Materials/Ground/TestBricks");

        MeshFilter meshFilter = road.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(vertxs[0].x, 0, vertxs[0].y), //top left 0
            new Vector3(vertxs[1].x, 0, vertxs[1].y), //top right 1 
            new Vector3(vertxs[2].x, 0, vertxs[2].y), //bottom left 2 
            new Vector3(vertxs[3].x, 0, vertxs[3].y)  //bottom right 3
        };
        mesh.vertices = vertices;

        int[] tris = new int[]
        {
            2, 0, 3,
            3, 0, 1
        };
        mesh.triangles = tris;
        
        mesh.uv = CalculateUVs(vertxs, pos);

        meshFilter.mesh = mesh;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        road.transform.position = new Vector3(pos.x, 0, pos.y);
        road.name = name;
        road.AddComponent<MeshCollider>();
    }

    Vector2[] CalculateUVs(Vector2[] vertices, Vector2 pos)
    {
        Vector2[] worldVertices = new Vector2[4]
        {
            vertices[0] + pos,
            vertices[1] + pos,
            vertices[2] + pos,
            vertices[3] + pos
        };
        Vector2[] uvs = new Vector2[4];

        float width = Vector2.Distance(vertices[0], vertices[1]);
        float height = Vector2.Distance(vertices[0], vertices[2]);
        
        if(width > height)
        {
            uvs[0] = new Vector2(-width / 2, -height / 2);
            uvs[1] = new Vector2(width / 2, -height / 2);
            uvs[2] = new Vector2(-width / 2, height / 2);
            uvs[3] = new Vector2(width / 2, height / 2);
        }
        else
        {
            uvs[0] = new Vector2(-width / 2, -height / 2);
            uvs[1] = new Vector2(width / 2, -height / 2);
            uvs[2] = new Vector2(-width / 2, height / 2);
            uvs[3] = new Vector2(width / 2, height / 2);
        }

        return uvs;
    }

    void RenderRoad(Vector2Int block)
    {
        Vector2[] verticalRoad = new Vector2[4];
        Vector2[] horizontalRoad = new Vector2[4];
        
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
        RenderQuad(verticalRoad, verticalCenter, VectToName(block) + "vertical");
        RenderQuad(horizontalRoad, horizontalCenter, VectToName(block) + "horizontal");
    }

    void RenderBackwardsRoad(Vector2Int block)
    {
        Vector2[] verticalRoad = new Vector2[4];
        Vector2[] horizontalRoad = new Vector2[4];
        
        //frequently used formulas calculation for performance
        Vector2 xy = new Vector2(block.x, block.y) * blockSize;
        Vector2 x1y = new Vector2(block.x - 1, block.y) * blockSize;
        Vector2 xy1 = new Vector2(block.x, block.y - 1) * blockSize;
        Vector2 x1y1 = new Vector2(block.x - 1, block.y - 1) * blockSize;

        //roads centers
        Vector2 verticalCenter = QuadCenter
        (
            blockMap[new Vector2Int(block.x - 1, block.y)].topRight + x1y,
            blockMap[block].topLeft + xy,
            blockMap[new Vector2Int(block.x - 1, block.y)].bottomRight + x1y,
            blockMap[block].bottomLeft + xy
        );
        Vector2 horizontalCenter = QuadCenter
        (
            blockMap[block].bottomLeft + xy,
            blockMap[block].bottomRight + xy,
            blockMap[new Vector2Int(block.x, block.y - 1)].topLeft + xy1,
            blockMap[new Vector2Int(block.x, block.y - 1)].topRight + xy1
        );

        //vertical road top left
        verticalRoad[0] = blockMap[new Vector2Int(block.x - 1, block.y)].topRight + x1y - verticalCenter;
        //vertical road top right
        verticalRoad[1] = blockMap[block].topLeft + xy - verticalCenter;
        //vertical road bottom left
        verticalRoad[2] = blockMap[new Vector2Int(block.x - 1, block.y)].bottomRight + x1y - verticalCenter;
        //vertical road bottom right
        verticalRoad[3] = blockMap[block].bottomLeft + xy - verticalCenter;

        //horizontal road top left
        horizontalRoad[0] = blockMap[block].bottomLeft + xy - horizontalCenter;
        //horizontal road top right
        horizontalRoad[1] = blockMap[block].bottomRight + xy - horizontalCenter;
        //horizontal road bottom left
        horizontalRoad[2] = blockMap[new Vector2Int(block.x, block.y - 1)].topLeft + xy1 - horizontalCenter;
        //horizontal road bottom right
        horizontalRoad[3] = blockMap[new Vector2Int(block.x, block.y - 1)].topRight + xy1 - horizontalCenter;

        //quads rendering
        RenderQuad(verticalRoad, verticalCenter, VectToName(block) + "vertical");
        RenderQuad(horizontalRoad, horizontalCenter, VectToName(block) + "horizontal");
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

        //quads rendering
        RenderQuad(centerRoad, centerPos, VectToName(block) + "center");
    }

    void RenderBackwardsCenter(Vector2Int block)
    {
        Vector2[] centerRoad = new Vector2[4];
        
        //frequently used formulas calculation for performance
        Vector2 xy = new Vector2(block.x, block.y) * blockSize;
        Vector2 x1y = new Vector2(block.x - 1, block.y) * blockSize;
        Vector2 xy1 = new Vector2(block.x, block.y - 1) * blockSize;
        Vector2 x1y1 = new Vector2(block.x - 1, block.y - 1) * blockSize;

        //roads centers
        Vector2 centerPos = QuadCenter
        (
            blockMap[new Vector2Int(block.x, block.y - 1)].bottomRight + xy1,
            blockMap[new Vector2Int(block.x - 1, block.y - 1)].bottomLeft + x1y1,
            blockMap[block].topRight + xy,
            blockMap[new Vector2Int(block.x - 1, block.y)].topLeft + x1y
        );

        //center road top left
        centerRoad[0] = blockMap[new Vector2Int(block.x, block.y - 1)].bottomRight + xy1 - centerPos;
        //center road top right
        centerRoad[1] = blockMap[new Vector2Int(block.x - 1, block.y - 1)].bottomLeft + x1y1 - centerPos;
        //center road bottom left
        centerRoad[2] = blockMap[block].topRight + xy - centerPos;
        //center road bottom right
        centerRoad[3] = blockMap[new Vector2Int(block.x - 1, block.y)].topLeft + x1y - centerPos;

        //quads rendering
        RenderQuad(centerRoad, centerPos, VectToName(block) + "center");
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
