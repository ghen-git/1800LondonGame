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
    float insetScale = 4f;

    Building GenerateBuilding(Block block, float depth, float inset, float width, bool rightConstraint)
    {
        Vector2 topPivot, bottomPivot;
        Vector2 topLeftBound, bottomLeftBound, topRightBound, bottomRightBound;
        Line bottomP, topP, leftP, rightP;
        
        float segmentLength;

        if(!block.direction) //horizontal generation
        {
            segmentLength = Vector2.Distance(block.topLeft, block.bottomLeft);
            if(!rightConstraint)
            {
                topPivot = block.topLeft;
                bottomPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, width);

                bottomP = block.leftEdge.PerpendicularAtPoint(bottomPivot);
                topP = Line.ParallelAtPoint(block.topEdge, topPivot);
            }
            else
            {
                topPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, segmentLength - width);
                bottomPivot = block.bottomLeft;

                bottomP = Line.ParallelAtPoint(block.bottomEdge, bottomPivot);
                topP = block.leftEdge.PerpendicularAtPoint(topPivot);
            }

            topLeftBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x), inset);
            topRightBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x), depth + inset);
            bottomLeftBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x), inset);
            bottomRightBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x), depth + inset);
        }
        else //vertical generation
        {
            segmentLength = Vector2.Distance(block.topRight, block.topLeft);
            if(!rightConstraint)
            {
                topPivot = block.topRight;
                bottomPivot = block.topEdge.PointOnLine(block.topRight, block.topLeft, width);

                leftP = block.topEdge.PerpendicularAtPoint(bottomPivot);
                rightP = Line.ParallelAtPoint(block.rightEdge, topPivot);
            }
            else
            {
                topPivot = block.topEdge.PointOnLine(block.topRight, block.topLeft, segmentLength - width);
                bottomPivot = block.topLeft;

                leftP = Line.ParallelAtPoint(block.leftEdge, bottomPivot);
                rightP = block.topEdge.PerpendicularAtPoint(topPivot);
            }
            

            topLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y), inset);
            topRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y), inset);
            bottomLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y), depth + inset);
            bottomRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y), depth + inset);
        }

        return new Building(topLeftBound, topRightBound, bottomLeftBound, bottomRightBound);
    }

    Building GenerateBuilding(Block block, float width, float depth, float inset, float startDistance, bool rightConstraint, float rowOffset)
    {
        Vector2 topPivot, bottomPivot;
        Vector2 topLeftBound, bottomLeftBound, topRightBound, bottomRightBound;
        
        Vector2 top, bottom;

        if(!block.direction)
        {
            top = block.topEdge.PointOnLine(block.topLeft, block.topRight, rowOffset);
            bottom = block.bottomEdge.PointOnLine(block.bottomLeft, block.bottomRight, rowOffset);
        }
        else
        {
            top = block.rightEdge.PointOnLine(block.topRight, block.bottomRight, rowOffset);
            bottom = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, rowOffset);
        }
        float segmentLength = Vector2.Distance(top, bottom);

        if(!block.direction) //horizontal generation
        {
            if(!rightConstraint)
            {
                topPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, startDistance);
                bottomPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, startDistance + width);
            }
            else
            {
                topPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, segmentLength - startDistance - width);
                bottomPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, segmentLength - startDistance);
            }
            
            Line bottomP = block.leftEdge.PerpendicularAtPoint(bottomPivot);
            Line topP = block.leftEdge.PerpendicularAtPoint(topPivot);

            topLeftBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x), rowOffset + inset);
            topRightBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x), rowOffset + depth + inset);
            bottomLeftBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x), rowOffset + inset);
            bottomRightBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x), rowOffset + depth + inset);
        }
        else //vertical generation
        {
            if(!rightConstraint)
            {
                topPivot = block.topEdge.PointOnLine(block.topRight, block.topLeft, startDistance);
                bottomPivot = block.topEdge.PointOnLine(block.topRight, block.topLeft, startDistance + width);
            }
            else
            {
                topPivot = block.topEdge.PointOnLine(block.topRight, block.topLeft, segmentLength - startDistance - width);
                bottomPivot = block.topEdge.PointOnLine(block.topRight, block.topLeft, segmentLength - startDistance);
            }
            
            Line leftP = block.topEdge.PerpendicularAtPoint(bottomPivot);
            Line rightP = block.topEdge.PerpendicularAtPoint(topPivot);

            topLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y), rowOffset + inset);
            topRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y), rowOffset + inset);
            bottomLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y), rowOffset + depth + inset);
            bottomRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y), rowOffset + depth + inset);
        }

        return new Building(topLeftBound, topRightBound, bottomLeftBound, bottomRightBound);
    }
    
    public Dictionary<Vector2Int, Building> GenerateBuildings(Block block)
    {
        Dictionary<Vector2Int, Building> buildings = new Dictionary<Vector2Int, Building>();

        Building leftBuilding, rightBuilding;
        float leftBDistance, rightBDistance;
        
        bool startWithHouse;

        int buildStep;
        float segmentLength;

        float leftWidth;
        float leftDepth;
        float leftInset;
        float rightWidth;
        float rightDepth;
        float rightInset;

        float maxRowDepth;
        float rowOffset = 0;
        int rowStep = 0;
        
        do
        {
            startWithHouse = RandomChance(50, 100);
            maxRowDepth = 0;
            buildStep = 0;

            Vector2 top, bottom;

            if(!block.direction)
            {
                top = block.topEdge.PointOnLine(block.topLeft, block.topRight, rowOffset);
                bottom = block.bottomEdge.PointOnLine(block.bottomLeft, block.bottomRight, rowOffset);
            }
            else
            {
                top = block.rightEdge.PointOnLine(block.topRight, block.bottomRight, rowOffset);
                bottom = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, rowOffset);
            }
            segmentLength = Vector2.Distance(top, bottom);

            if(startWithHouse)
            {
                float width = Random.Range(1, maxBuildWidth + 1) * buildingScale;
                float depth = GetDepth() * buildingScale;
                if(depth > 1 * buildingScale)
                    width = 1 * buildingScale;
                float inset = Random.Range(1, maxBuildInset + 1) * insetScale;
                leftBuilding = GenerateBuilding(block, width, depth, inset, segmentLength / 2 - width / 2, false, rowOffset);
                leftBDistance = segmentLength / 2 - width / 2;
                
                buildings.Add(new Vector2Int(0, rowStep), leftBuilding);
                buildStep = 1;

                rightBDistance = segmentLength / 2 - width / 2;

                if(depth + inset > maxRowDepth)
                    maxRowDepth = depth + inset;
            }
            else
            {
                leftWidth = Random.Range(1, maxBuildWidth + 1) * buildingScale;
                leftDepth = GetDepth() * buildingScale;
                if(leftDepth > 1 * buildingScale)
                    leftWidth = 1 * buildingScale;
                leftInset = Random.Range(1, maxBuildInset + 1) * insetScale;
                rightWidth = Random.Range(1, maxBuildWidth + 1) * buildingScale;
                rightDepth = GetDepth() * buildingScale;
                if(rightDepth > 1 * buildingScale)
                    rightWidth = 1 * buildingScale;
                rightInset = Random.Range(1, maxBuildInset + 1) * insetScale;

                float startDistance = segmentLength / 2 - secondaryRoadSize / 2;
                leftBuilding = GenerateBuilding(block, leftWidth, leftDepth, leftInset, startDistance - leftWidth, false, rowOffset);
                rightBuilding = GenerateBuilding(block, rightWidth, rightDepth, rightInset, startDistance - rightWidth, true, rowOffset);

                leftBDistance = startDistance - leftWidth;
                rightBDistance = startDistance - rightWidth;

                buildings.Add(new Vector2Int(1, rowStep), leftBuilding);
                buildings.Add(new Vector2Int(-1, rowStep), rightBuilding);
                buildStep = 2;

                if(leftDepth + leftInset > maxRowDepth)
                    maxRowDepth = leftDepth + leftInset;
                if(rightDepth + rightInset > maxRowDepth)
                    maxRowDepth = rightDepth + rightInset;
            }

            do
            {
                leftWidth = Random.Range(1, maxBuildWidth + 1) * buildingScale;
                leftDepth = GetDepth() * buildingScale;
                if(leftDepth > 1 * buildingScale)
                    leftWidth = 1 * buildingScale;
                leftInset = Random.Range(1, maxBuildInset + 1) * insetScale;
                rightWidth = Random.Range(1, maxBuildWidth + 1) * buildingScale;
                rightDepth = GetDepth() * buildingScale;
                if(rightDepth > 1 * buildingScale)
                    rightWidth = 1 * buildingScale;
                rightInset = Random.Range(1, maxBuildInset + 1) * insetScale;

                leftBDistance = leftBDistance - (RandomChance(50, 100) ? secondaryRoadSize : 0) - leftWidth;
                rightBDistance = rightBDistance - (RandomChance(50, 100) ? secondaryRoadSize : 0) - rightWidth;

                leftBuilding = GenerateBuilding(block, leftWidth, leftDepth, leftInset, leftBDistance, false, rowOffset);
                rightBuilding = GenerateBuilding(block, rightWidth, rightDepth, rightInset, rightBDistance, true, rowOffset);

                buildings.Add(new Vector2Int(buildStep, rowStep), leftBuilding);
                buildings.Add(new Vector2Int(-buildStep, rowStep), rightBuilding);
                buildStep++;
                
                if(leftDepth + leftInset > maxRowDepth)
                    maxRowDepth = leftDepth + leftInset;
                if(rightDepth + rightInset > maxRowDepth)
                    maxRowDepth = rightDepth + rightInset;
            }
            while(canFitMoreBuildings(block, leftBDistance, rightBDistance));

            leftDepth = GetDepth() * buildingScale;
            if(leftDepth > 1 * buildingScale)
                leftWidth = 1 * buildingScale;
            leftInset = Random.Range(1, maxBuildInset + 1) * insetScale;
            rightDepth = GetDepth() * buildingScale;
            if(rightDepth > 1 * buildingScale)
                rightWidth = 1 * buildingScale;
            rightInset = Random.Range(1, maxBuildInset + 1) * insetScale;

            leftBDistance = leftBDistance - (RandomChance(50, 100) ? secondaryRoadSize : 0);
            rightBDistance = rightBDistance - (RandomChance(50, 100) ? secondaryRoadSize : 0);
            
            leftBuilding = GenerateBuilding(block, leftDepth, leftInset, leftBDistance, false);
            rightBuilding = GenerateBuilding(block, rightDepth, rightInset, rightBDistance, true); 

            buildings.Add(new Vector2Int(buildStep, rowStep), leftBuilding);
            buildings.Add(new Vector2Int(-buildStep, rowStep), rightBuilding);

            if(leftDepth + leftInset > maxRowDepth)
                maxRowDepth = leftDepth + leftInset;
            if(rightDepth + rightInset > maxRowDepth)
                maxRowDepth = rightDepth + rightInset;

            rowOffset += maxRowDepth + secondaryRoadSize;
            rowStep++;
            print(rowOffset);
        }
        while(canFitMoreRows(block, rowOffset));     

        return buildings;
    }

    int GetDepth(int depth = 1)
    {
        if(RandomChance(10, 100) && depth < maxBuildDepth)
            depth = GetDepth(depth + 1);
        return depth;
    }

    bool canFitMoreBuildings(Block block, float leftBDistance, float rightBDistance)
    {
        return 
            leftBDistance - secondaryRoadSize - maxBuildWidth * buildingScale > 0 ||
            rightBDistance - secondaryRoadSize - maxBuildWidth * buildingScale > 0;
    }
    
    bool canFitMoreRows(Block block, float rowOffset)
    {
        float segmentLength;

        if(!block.direction)
            segmentLength = Vector2.Distance
            (
                Line.MidPoint(block.topLeft, block.bottomLeft), 
                Line.MidPoint(block.topRight, block.bottomRight)
            );
        else
            segmentLength = Vector2.Distance
            (
                Line.MidPoint(block.topRight, block.topLeft), 
                Line.MidPoint(block.bottomRight, block.bottomLeft)
            );

        return 
            segmentLength - rowOffset > 0;
    }

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
    GameObject RenderWalls(Vector2[] groundPoints, Vector2 pos, float height, string name, string textureName)
    {
        
        GameObject wall = new GameObject();

        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();

        meshRenderer.material = Resources.Load<Material>($"Materials/Ground/{textureName}");

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
            GameObject walls = RenderWalls(groundPoints, center, 15f, buildingGO.name + "-walls", block.direction ? "RedBricks" : "Road");
            walls.transform.SetParent(buildingGO.transform, true);

            //render gizmos
            gizmosPoints.Add(new Vector3(building.topLeftCorner.x, 0, building.topLeftCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.topRightCorner.x, 0, building.topRightCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.bottomLeftCorner.x, 0, building.bottomLeftCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
            gizmosPoints.Add(new Vector3(building.bottomRightCorner.x, 0, building.bottomRightCorner.y) + new Vector3(coords.x, 0, coords.y) * blockSize);
        }
    }
}
