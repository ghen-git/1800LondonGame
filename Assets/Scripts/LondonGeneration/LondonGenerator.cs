using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LondonSettings;
using static GraphicsUtil;

public class LondonGenerator : MonoBehaviour
{

    [System.NonSerialized]
    public Dictionary<Vector2Int, Block> blockMap = new Dictionary<Vector2Int, Block>();
    Transform player;
    Vector2Int[] bounds; // top-left, top-right, bottom-left, bottom-right
    Vector2Int[] loadedBounds; // top-left, top-right, bottom-left, bottom-right
    BlockGenerator blockGenerator;
    RoadGenerator roadGenerator;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();

        blockGenerator = GetComponent<BlockGenerator>();
        roadGenerator = GetComponent<RoadGenerator>();
        roadGenerator = GetComponent<RoadGenerator>();

        CalculateBounds();
        blockGenerator.Init();
        roadGenerator.Init();
        blockGenerator.LoadBlocks(bounds);
        roadGenerator.LoadRoads(bounds);
        loadedBounds = bounds;

        RenderQuad
        (
            new Vector2[]{new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1)}, 
            Vector2.zero,
            2f, 
            "cock", 
            Resources.Load<Material>("Materials/Ground/Road"), 
            0.15f,
            new bool[]{true, true, true, true, true, false}
        );
    }
    
    void Update()
    {
        CalculateBounds();
        if(BoundsChanged())
        {
            blockGenerator.LoadBlocks(bounds, loadedBounds);
            blockGenerator.UnloadBlocks(bounds, loadedBounds);
            roadGenerator.LoadRoads(bounds, loadedBounds);
            roadGenerator.UnloadRoads(bounds, loadedBounds);
            loadedBounds = bounds;
        }
    }

    void CalculateBounds()
    {
        bounds = new Vector2Int[4];

        Vector2Int currentBlock = new Vector2Int((int)(player.position.x / blockSize), (int)(player.position.z / blockSize));
        //print(currentBlock);
        
        bounds[0] = new Vector2Int(currentBlock.x - renderDistance, currentBlock.y + renderDistance);
        bounds[1] = new Vector2Int(currentBlock.x + renderDistance, currentBlock.y + renderDistance);
        bounds[2] = new Vector2Int(currentBlock.x - renderDistance, currentBlock.y - renderDistance);
        bounds[3] = new Vector2Int(currentBlock.x + renderDistance, currentBlock.y - renderDistance);
    }

    bool IsInBounds(Vector2Int pos, Vector2Int[] bounds)
    {
        return 
        pos.x >= bounds[2].x && pos.y >= bounds[2].y &&
        pos.x <= bounds[1].x && pos.y <= bounds[1].y;
    }

    bool BoundsChanged()
    {
        return 
        !bounds[0].Equals(loadedBounds[0]) ||
        !bounds[1].Equals(loadedBounds[1]) ||
        !bounds[2].Equals(loadedBounds[2]) ||
        !bounds[3].Equals(loadedBounds[3]);
    }
}
