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

    public void LoadRoads(Vector2Int[] bounds)
    {

    }

    public void LoadRoads(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {

    }
    public void UnloadRoads(Vector2Int[] bounds, Vector2Int[] loadedBounds)
    {

    }
}
