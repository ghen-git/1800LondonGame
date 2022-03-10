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
    void Start()
    {
        blockMap = GetComponent<LondonGenerator>().blockMap;
        renderDistance = GetComponent<LondonGenerator>().renderDistance;
        blockSize = GetComponent<LondonGenerator>().blockSize;
        roadSize = GetComponent<LondonGenerator>().roadSize;
        variationAmount = GetComponent<LondonGenerator>().variationAmount;
    }

    void LoadVertical()
    {

    }

    void LoadHorizontal()
    {

    }

    public void LoadRoads(Vector2Int[] bounds)
    {

    }

    public void LoadRoads(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {
        for(int x = bounds[2].x; x <= bounds[1].x; x++)
            for(int y = bounds[2].y; y <= bounds[1].y; y++)
            {
                if(x % 2 == 0)
                    LoadHorizontal();
                 if(y % 2 == 0)
                    LoadVertical();
      
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

    public void UnloadRoads(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {

    }
}
