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
    List<Vector3> gizmosPoints = new List<Vector3>();
    [System.NonSerialized]
    public float blockSize =200f;

    float floorHeight = 5f;
    float buildingScale = 20f;
    int maxBuildDepth = 3;
    float secondaryRoadSize = 4f;

    Building GenerateBuilding(Block block, bool vertical)
    {
        Vector2 topLeftBound, bottomLeftBound, topRightBound, bottomRightBound;

        if(!vertical)
        {
            topLeftBound = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, Vector2.Distance(block.topLeft, block.bottomLeft) / 2 - width / 2);
            bottomLeftBound = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, Vector2.Distance(block.topLeft, block.bottomLeft) / 2 + width / 2);
            
            Line bottomP = Line.ParallelAtPoint(block.bottomEdge, bottomLeftBound);
            Line topP = Line.ParallelAtPoint(block.topEdge, topLeftBound);

            bottomRightBound = bottomP.PointOnLine(bottomLeftBound, bottomP.PointFromX(block.bottomRight.x), depth);
            topRightBound = topP.PointOnLine(topLeftBound, topP.PointFromX(block.topRight.x), depth);
        }
        else
        {
            topLeftBound = block.topEdge.PointOnLine(block.topLeft, block.topRight, Vector2.Distance(block.topLeft, block.topRight) / 2 - width / 2);
            topRightBound = block.topEdge.PointOnLine(block.topLeft, block.topRight, Vector2.Distance(block.topLeft, block.topRight) / 2 + width / 2);
            
            Line leftP = Line.ParallelAtPoint(block.leftEdge, topLeftBound);
            Line rightP = Line.ParallelAtPoint(block.rightEdge, topRightBound);

            bottomLeftBound = leftP.PointOnLine(topLeftBound, leftP.PointFromY(block.bottomLeft.y), depth);
            bottomRightBound = rightP.PointOnLine(topRightBound, rightP.PointFromY(block.bottomRight.y), depth);
        }

        return new Building(topLeftBound, topRightBound, bottomLeftBound, bottomRightBound);
    } 

    /*Building GenerateBuilding(Block block, bool vertical, Building ajacent, bool isRight)
    {
        float width = UnityEngine.Random.Range(1, 3) * buildingScale;
        float depth = GetDepth() * buildingScale;

    }
    
    Building GenerateBuilding(Block block, bool vertical)
    {
        float width = UnityEngine.Random.Range(1, 3) * buildingScale;
        float depth = GetDepth() * buildingScale;
    }*/
    
    Dictionary<Vector2Int, Building> GenerateHorizontal(Block block, bool startWithHouse)
    {
        Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();

        Building leftBuilding, rightBuilding;

        //horizontal, starting from left to right

        buildings.Add(new Vector2Int(0, 0), GenerateBuilding(block, false));

        /*if(startWithHouse)
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
        while(canFitMoreRows(block, buildings));*/

        return buildings;
    }

    Dictionary<Vector2Int, Building> GenerateVertical(Block block, bool startWithHouse)
    {
        Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();

        Building leftBuilding, rightBuilding;

        //vertical, starting from left to right

        buildings.Add(new Vector2Int(0, 0), GenerateBuilding(block, true));

        /*if(startWithHouse)
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
        while(canFitMoreRows(block, buildings));*/

        return buildings;
    }

    int GetDepth(int depth = 1)
    {
        if(RandomChance(10, 100) && depth < maxBuildDepth)
            depth = GetDepth(depth + 1);
        return depth;
    }

    public Dictionary<Vector2Int, Building> GenerateBuildings(Block block)
    {
        if(!block.direction)
            return GenerateHorizontal(block, true);
        else    
            return GenerateVertical(block, true);
    }

    /*bool canFitMoreBuildings(Block block)
    {

    }

    bool canFitMoreRows(Block block)
    {

    }*/

    void OnDrawGizmos()
    {
        foreach(Vector3 point in gizmosPoints)
            Gizmos.DrawSphere(point, 1f);
    }
    
    void RenderWall(Vector2 bottomLeft, Vector2 bottomRight, Vector2 pos, string name)
    {
        GameObject wall = new GameObject();

        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();

        meshRenderer.material = Resources.Load<Material>("Materials/Ground/Road");

        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();

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

        wall.transform.position = new Vector3(pos.x, 0, pos.y);
        wall.name = name;
        wall.AddComponent<MeshCollider>();
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

        float scale = 0.15f;
        
        if(width > height)
        {
            uvs[0] = new Vector2(-width / 2, -height / 2) * scale;
            uvs[1] = new Vector2(width / 2, -height / 2) * scale;
            uvs[2] = new Vector2(-width / 2, height / 2) * scale;
            uvs[3] = new Vector2(width / 2, height / 2) * scale;
        }
        else
        {
            uvs[0] = new Vector2(-width / 2, -height / 2) * scale;
            uvs[1] = new Vector2(width / 2, -height / 2) * scale;
            uvs[2] = new Vector2(-width / 2, height / 2) * scale;
            uvs[3] = new Vector2(width / 2, height / 2) * scale;
        }

        return uvs;
    }

    public void RenderBuildings(Block block, Vector2Int coords)
    {
        foreach(Building building in block.buildings.Values)
        {
            gizmosPoints.Add(new Vector3(building.topLeftCorner.x, 0, building.topLeftCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.topRightCorner.x, 0, building.topRightCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.bottomLeftCorner.x, 0, building.bottomLeftCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.bottomRightCorner.x, 0, building.bottomRightCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
        }
    }
}
