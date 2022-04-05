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
    int maxBuildWidth = 2;
    int maxBuildInset = 2;
    float secondaryRoadSize = 4f;
    float insetScale = 1f;

    Building GenerateBuilding(Block block, bool vertical, float depth)
    {
        Vector2 topLeftBound, bottomLeftBound, topRightBound, bottomRightBound;

        if(!vertical)
        {
            topLeftBound = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, Vector2.Distance(block.topLeft, block.bottomLeft) / 2 - width / 2);
            bottomLeftBound = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, Vector2.Distance(block.topLeft, block.bottomLeft) / 2 + width / 2);
            
            Line bottomP = block.leftEdge.PerpendicularAtPoint(bottomLeftBound);
            Line topP = block.leftEdge.PerpendicularAtPoint(topLeftBound);

            bottomRightBound = bottomP.PointOnLine(bottomLeftBound, bottomP.PointFromX(block.bottomRight.x), depth);
            topRightBound = topP.PointOnLine(topLeftBound, topP.PointFromX(block.topRight.x), depth);
        }
        else
        {
            topLeftBound = block.topEdge.PointOnLine(block.topLeft, block.topRight, Vector2.Distance(block.topLeft, block.topRight) / 2 - width / 2);
            topRightBound = block.topEdge.PointOnLine(block.topLeft, block.topRight, Vector2.Distance(block.topLeft, block.topRight) / 2 + width / 2);
            
            Line leftP = block.topEdge.PerpendicularAtPoint(topLeftBound);
            Line rightP = block.topEdge.PerpendicularAtPoint(topRightBound);

            bottomLeftBound = leftP.PointOnLine(topLeftBound, leftP.PointFromY(block.bottomLeft.y), depth);
            bottomRightBound = rightP.PointOnLine(topRightBound, rightP.PointFromY(block.bottomRight.y), depth);
        }

        return new Building(topLeftBound, topRightBound, bottomLeftBound, bottomRightBound);
    } 

    Building GenerateBuilding(Block block, float width, float depth, float inset, float startDistance, bool leftConstraint)
    {
        Vector2 topPivot, bottomPivot;
        Vector2 topLeftBound, bottomLeftBound, topRightBound, bottomRightBound;

        if(!block.direction) //horizontal generation
        {
            if(leftConstraint)
            {
                topPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, startDistance);
                bottomPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, startDistance + width);
            }
            else
            {
                float segmentLength = Vector2.Distance(block.topLeft, block.bottomLeft);
                topPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, segmentLength - startDistance - width);
                bottomPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, segmentLength - startDistance + width);
            }
            
            Line bottomP = block.leftEdge.PerpendicularAtPoint(bottomPivot);
            Line topP = block.leftEdge.PerpendicularAtPoint(topPivot);

            topLeftBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x), inset);
            topRightBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x), inset);
            bottomLeftBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x), depth + inset);
            bottomRightBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x), depth + inset);
        }
        else //vertical generation
        {
            if(leftConstraint)
            {
                topPivot = block.topEdge.PointOnLine(block.topLeft, block.topRight, startDistance);
                bottomPivot = block.topEdge.PointOnLine(block.topLeft, block.topRight, startDistance + width);
            }
            else
            {
                float segmentLength = Vector2.Distance(block.topRight, block.topLeft);
                topPivot = block.topEdge.PointOnLine(block.topLeft, block.topRight, segmentLength - startDistance);
                bottomPivot = block.topEdge.PointOnLine(block.topLeft, block.topRight, segmentLength - startDistance + width);
            }
            
            Line leftP = block.topEdge.PerpendicularAtPoint(bottomPivot);
            Line rightP = block.topEdge.PerpendicularAtPoint(topPivot);

            topLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y), inset);
            topRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y), inset);
            bottomLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y), depth + inset);
            bottomRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y), depth + inset);
        }

        return new Building(topLeftBound, topRightBound, bottomLeftBound, bottomRightBound);
    }
    
    Dictionary<Vector2Int, Building> GenerateBuildings(Block block)
    {
        Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();

        Building leftBuilding, rightBuilding;
        
        bool startWithHouse = RandomChance(50, 100);

        int buildStep;
        float segmentLength;
        Vector2Int buildId;

        //initial building generation
        if(!block.direction)
            segmentLength = Vector2.Distance(block.topLeft, block.bottomLeft);
        else
            segmentLength = Vector2.Distance(block.topRight, block.topLeft);

        if(startWithHouse)
        {
            float width = Random.Range(1, maxBuildWidth + 1) * buildingScale;
            float depth = GetDepth() * buildingScale;
            float inset = Random.Range(1, maxBuildInset + 1) * insetScale;
            leftBuilding = GenerateBuilding(block, width, depth, inset, segmentLength / 2 - width / 2, true);

            rightBuilding = leftBuilding;
            
            buildings.Add(new Vector2Int(0, 0), leftBuilding);
            buildStep = 1;
        }
        else
        {
            float leftWidth = Random.Range(1, maxBuildWidth + 1) * buildingScale;
            float leftDepth = GetDepth() * buildingScale;
            float leftInset = Random.Range(1, maxBuildInset + 1) * insetScale;
            float rightWidth = Random.Range(1, maxBuildWidth + 1) * buildingScale;
            float rightDepth = GetDepth() * buildingScale;
            float rightInset = Random.Range(1, maxBuildInset + 1) * insetScale;

            leftBuilding = GenerateBuilding(block, leftWidth, leftDepth, leftInset, );
            rightBuilding = GenerateBuilding(block, rightWidth, rightDepth, rightInset, );

            buildings.Add(new Vector2Int(1, 0), leftBuilding);
            buildings.Add(new Vector2Int(-1, 0), rightBuilding);
            buildStep = 2;
        }
        
        /*do
        {
            buildings.Add(new Vector2Int(buildStep, 0), leftBuilding);
            buildings.Add(new Vector2Int(-buildStep, 0), rightBuilding);
            buildStep++;
        }
        while(canFitMoreBuildings(block, false, leftBuilding, rightBuilding));*/

        return buildings;
    }

    int GetDepth(int depth = 1)
    {
        if(RandomChance(10, 100) && depth < maxBuildDepth)
            depth = GetDepth(depth + 1);
        return depth;
    }

    bool canFitMoreBuildings(Block block, bool vertical, Building leftBuilding, Building rightBuilding)
    {
        return true;
    }

    /*bool canFitMoreRows(Block block)
    {

    }*/

    void OnDrawGizmos()
    {
        foreach(Vector3 point in gizmosPoints)
            Gizmos.DrawSphere(point, 1f);
    }

    /*
        ground points structure:
        0 - topLeft
        1 - topRight
        2 - bottomLeft
        3 - bottomRight
    */
    GameObject RenderWalls(Vector2[] groundPoints, Vector2 pos, float height, string name)
    {
        
        GameObject wall = new GameObject();

        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();

        meshRenderer.material = Resources.Load<Material>("Materials/Ground/Road");

        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            //left wall
            new Vector3(groundPoints[0].x, height, groundPoints[0].y), //top left 0
            new Vector3(groundPoints[2].x, height, groundPoints[2].y), //top right 1 
            new Vector3(groundPoints[0].x, 0, groundPoints[0].y), //bottom left 2 
            new Vector3(groundPoints[2].x, 0, groundPoints[2].y),  //bottom right 3

            //top wall
            new Vector3(groundPoints[1].x, height, groundPoints[1].y), //top left 4
            new Vector3(groundPoints[0].x, height, groundPoints[0].y), //top right 5 
            new Vector3(groundPoints[1].x, 0, groundPoints[1].y), //bottom left 6 
            new Vector3(groundPoints[0].x, 0, groundPoints[0].y),  //bottom right 7

            //right wall
            new Vector3(groundPoints[3].x, height, groundPoints[3].y), //top left 8
            new Vector3(groundPoints[1].x, height, groundPoints[1].y), //top right 9 
            new Vector3(groundPoints[3].x, 0, groundPoints[3].y), //bottom left 10 
            new Vector3(groundPoints[1].x, 0, groundPoints[1].y),  //bottom right 11

            //bottom wall
            new Vector3(groundPoints[2].x, height, groundPoints[2].y), //top left 12
            new Vector3(groundPoints[3].x, height, groundPoints[3].y), //top right 13 
            new Vector3(groundPoints[2].x, 0, groundPoints[2].y), //bottom left 14 
            new Vector3(groundPoints[3].x, 0, groundPoints[3].y)  //bottom right 15
        };
        mesh.vertices = vertices;

        int[] tris = new int[]
        {
            2, 0, 3,
            3, 0, 1,
            6, 4, 7,
            7, 4, 5,
            10, 8, 11,
            11, 8, 9,
            14, 12, 15,
            15, 12, 13
        };
        mesh.triangles = tris;
        
        mesh.uv = CalculateUVs(groundPoints, height);

        meshFilter.mesh = mesh;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        wall.transform.position = new Vector3(pos.x, 0, pos.y);
        wall.name = name;
        wall.AddComponent<MeshCollider>();

        return wall;
    }
    
    Vector2[] CalculateUVs(Vector2[] vertices, float height)
    {
        Vector2[] uvs = new Vector2[16];

        float leftWidth = Vector2.Distance(vertices[2], vertices[0]);
        float topWidth = Vector2.Distance(vertices[0], vertices[1]);
        float rightWidth = Vector2.Distance(vertices[1], vertices[3]);
        float bottomWidth = Vector2.Distance(vertices[3], vertices[2]);

        float scale = 0.15f;
        
        uvs[0] = new Vector2(-leftWidth / 2, height / 2) * scale;
        uvs[1] = new Vector2(leftWidth / 2, height / 2) * scale;
        uvs[2] = new Vector2(-leftWidth / 2, -height / 2) * scale;
        uvs[3] = new Vector2(leftWidth / 2, -height / 2) * scale;
        
        uvs[4] = new Vector2(-topWidth / 2, height / 2) * scale;
        uvs[5] = new Vector2(topWidth / 2, height / 2) * scale;
        uvs[6] = new Vector2(-topWidth / 2, -height / 2) * scale;
        uvs[7] = new Vector2(topWidth / 2, -height / 2) * scale;
        
        uvs[8] = new Vector2(-rightWidth / 2, height / 2) * scale;
        uvs[9] = new Vector2(rightWidth / 2, height / 2) * scale;
        uvs[10] = new Vector2(-rightWidth / 2, -height / 2) * scale;
        uvs[11] = new Vector2(rightWidth / 2, -height / 2) * scale;
        
        uvs[12] = new Vector2(-bottomWidth / 2, height / 2) * scale;
        uvs[13] = new Vector2(bottomWidth / 2, height / 2) * scale;
        uvs[14] = new Vector2(-bottomWidth / 2, -height / 2) * scale;
        uvs[15] = new Vector2(bottomWidth / 2, -height / 2) * scale;

        return uvs;
    }

    public void RenderBuildings(Block block, Vector2Int coords)
    {
        foreach(Vector2Int buildingCoords in block.buildings.Keys)
        {
            Building building = block.buildings[buildingCoords];
            Vector2 xy = new Vector2(coords.x, coords.y) * blockSize;

            //get building center
            Vector2 center = QuadCenter
            (
                building.topLeftCorner + xy, 
                building.topRightCorner + xy, 
                building.bottomLeftCorner + xy, 
                building.bottomRightCorner + xy
            );
            
            //convert relative coords
            Vector2[] groundPoints = new Vector2[4]
            {
                building.topLeftCorner + xy - center,
                building.topRightCorner + xy - center,
                building.bottomLeftCorner + xy - center,
                building.bottomRightCorner + xy - center
            };

            //building gameObject
            GameObject buildingGO = new GameObject(block.block.name + "|building-" + VectToName(buildingCoords));
            buildingGO.transform.SetParent(block.block.transform, true);

            //walls rendering
            GameObject walls = RenderWalls(groundPoints, center, 15f, buildingGO.name + "-walls");
            walls.transform.SetParent(buildingGO.transform, true);

            //render gizmos
            gizmosPoints.Add(new Vector3(building.topLeftCorner.x, 0, building.topLeftCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.topRightCorner.x, 0, building.topRightCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.bottomLeftCorner.x, 0, building.bottomLeftCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.bottomRightCorner.x, 0, building.bottomRightCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
        }
    }
}
