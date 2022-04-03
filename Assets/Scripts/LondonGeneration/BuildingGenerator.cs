using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;

public class Building
{
    public Vector2 topLeftCorner;
    public Vector2 topRightCorner;
    public Vector2 bottomLeftCorner;
    public Vector2 bottomRightCorner;

    public Building(Vector2 topLeftCorner, Vector2 topRightCorner, Vector2 bottomLeftCorner, Vector2 bottomRightCorner)
    {
        this.topLeftCorner = topLeftCorner;
        this.topRightCorner = topRightCorner;
        this.bottomLeftCorner = bottomLeftCorner;
        this.bottomRightCorner = bottomRightCorner;
    }
}

public class BuildingGenerator : MonoBehaviour
{
    float floorHeight = 5f;
    float buildingScale = 8f;
    float maxBuildDepth = 3f;
    float secondaryRoadSize = 4f;

    Building GenerateBuilding(Block block, bool vertical)
    {
        float width = UnityEngine.Random.Range(1, 3) * buildingScale;
        float depth = GetDepth() * buildingScale;

        Vector2 topLeftBound, bottomLeftBound, topRightBound, bottomRightBound;
        
        topLeftBound = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, Vector2.Distance(block.topLeft, block.bottomLeft) / 2 - width / 2);
        bottomLeftBound = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, Vector2.Distance(block.topLeft, block.bottomLeft) / 2 + width / 2);
        
        Line bottomP = Line.ParallelAtPoint(block.bottomEdge, bottomLeftBound);
        Line topP = Line.ParallelAtPoint(block.topEdge, topLeftBound);

        bottomRightBound = bottomP.PointOnLine(bottomLeftBound, bottomP.PointFromX(block.bottomRight.x), depth);
        topRightBound = bottomP.PointOnLine(bottomLeftBound, bottomP.PointFromX(block.bottomRight.x), depth);

        return new Building(topLeftBound, topRightBound, bottomLeftBound, bottomRightBound);
    } 

    Building GenerateBuilding(Block block, bool vertical, Building ajacent, bool isRight)
    {
        float width = UnityEngine.Random.Range(1, 3) * buildingScale;
        float depth = GetDepth() * buildingScale;

    }
    
    Building GenerateBuilding(Block block, bool vertical)
    {
        float width = UnityEngine.Random.Range(1, 3) * buildingScale;
        float depth = GetDepth() * buildingScale;
    }
    
    Dictionary<Vector2Int, Building> GenerateHorizontal(Block block, bool startWithHouse)
    {
        Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();

        Building leftBuilding, rightBuilding;

        //horizontal, starting from left to right

        if(startWithHouse)
        {
            float width = UnityEngine.Random.Range(1, 3) * buildingScale;
            float depth = GetDepth() * buildingScale;
            topLeftBound = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, Vector2.Distance(block.topLeft, block.bottomLeft) / 2 - width / 2);
            bottomLeftBound = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, Vector2.Distance(block.topLeft, block.bottomLeft) / 2 + width / 2);
            
            if()
        }
        else
        {

        }

        do
        {
            do
            {
            }
            while(canFitMoreBuildings(block, buildings));
        }
        while(canFitMoreRows(block, buildings));
    }
    Dictionary<Vector2Int, Building> GenerateVertical(Block block, bool startWithHouse)
    {
        Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();

        Building leftBuilding, rightBuilding;
        Vector2 frontLeftBound, backLeftBound, frontRightBound, frontBackBound;

        //vertical, starting from top to bottom

        do
        {
            do
            {
            }
            while(canFitMoreBuildings(block, buildings));
        }
        while(canFitMoreRows(block, buildings));
    }

    int GetDepth(int depth = 1)
    {
        if(RandomChance(10, 100))
            depth = GetDepth(depth + 1);
        return depth;
    }

    public Dictionary<Vector2Int, Building> GenerateBuildings(Block block)
    {
        bool startWithHouse = RandomChance(50, 100);
        if(RandomChance(50, 100))
            return GenerateHorizontal(block, startWithHouse);
        else
            return GenerateVertical(block, startWithHouse);
    }

    bool canFitMoreBuildings(Block block)
    {

    }

    bool canFitMoreRows(Block block)
    {

    }

    public void RenderBuildings(Block block)
    {
        foreach(Building building in block.buildings)
        {

        }
    }
}
