using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;
using static GraphicsUtil;
using static LondonSettings;

public class Building
{
    public Vector2 topLeftCorner;
    public Vector2 topRightCorner;
    public Vector2 bottomLeftCorner;
    public Vector2 bottomRightCorner;
    public int floorNumber;

    public Vector2[] vertices
    {
        get
        {
            return new Vector2[4]
            {
                topLeftCorner,
                topRightCorner,
                bottomLeftCorner,
                bottomRightCorner
            };
        }
    }

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
    Dictionary<Vector2Int, Building> buildings;

    public Dictionary<Vector2Int, Building> GenerateBuildings(Block block)
    {
        GenerateBuildingsBounds(block);

        return buildings;
    }

    public void RenderBuildings(Block block, Vector2Int coords)
    {
        foreach(Vector2Int buildingCoords in block.buildings.Keys)
        {
            Building building = block.buildings[buildingCoords];
            Vector2 xy = new Vector2(coords.x, coords.y) * blockSize;

            //get building center
            Vector2 center = GetGlobalCenter(building.vertices, xy);

            //building gameObject
            GameObject buildingGO = new GameObject(block.block.name + "|building-" + VectToName(buildingCoords));
            buildingGO.transform.SetParent(block.block.transform, true);

            //building parameters
            string wallsMat = block.end == "west" ? 
                westEndWallMats[Random.Range(0, westEndWallMats.Length)] :
                eastEndWallMats[Random.Range(0, eastEndWallMats.Length)];

            //walls rendering
            GameObject walls = RenderQuad
            (
                GetRelativeVertices(building.vertices, xy, center), 
                center,
                building.floorNumber * floorHeight, 
                buildingGO.name + "-walls", 
                Resources.Load<Material>($"Materials/{wallsMat}"), 
                0.15f,
                new bool[]{true, true, true, true, true, false}
            );

            walls.transform.SetParent(buildingGO.transform, true);
        }
    }

    void GenerateBuilding(Block block, Vector2Int id, Building building)
    {
        if(!CanGenerate(building, block))
            return;

        block.CalculateSidewalks(building);

        building.floorNumber = Random.Range(minFloors, maxFloors + 1);

        buildings.Add(id, building);
    }

    bool CanGenerate(Building building, Block block)
    {
        //kills weird looking ninja star buildings
        if(Mathf.Abs(AngleFrom3Points(building.bottomLeftCorner, building.topLeftCorner, building.topRightCorner)) < Mathf.PI / 4 + 0.1f)
            return false;
        if(Mathf.Abs(AngleFrom3Points(building.topLeftCorner, building.topRightCorner, building.bottomRightCorner)) < Mathf.PI / 4 + 0.1f)
            return false;
        if(Mathf.Abs(AngleFrom3Points(building.topRightCorner, building.bottomRightCorner, building.bottomLeftCorner)) < Mathf.PI / 4 + 0.1f)
            return false;
        if(Mathf.Abs(AngleFrom3Points(building.bottomRightCorner, building.bottomLeftCorner, building.topLeftCorner)) < Mathf.PI / 4 + 0.1f)
            return false;

        //kills buildings that exit from the block borders
        if(!PointInQuad(building.topLeftCorner, block))
            return false;
        if(!PointInQuad(building.topRightCorner, block))
            return false;
        if(!PointInQuad(building.bottomLeftCorner, block))
            return false;
        if(!PointInQuad(building.bottomRightCorner, block))
            return false;

        return true;
    }

    Building GenerateBounds(Block block, float depth, float inset, bool rightConstraint, float initialDistance, Vector2 top, Vector2 bottom, Line edge)
    {
        Vector2 topPivot, bottomPivot;
        Vector2 topLeftBound, bottomLeftBound, topRightBound, bottomRightBound;
        Line bottomP, topP, leftP, rightP;
        
        float segmentLength = Vector2.Distance(top, bottom);

        if(!block.direction) //horizontal generation
        {
            if(!rightConstraint)
            {
                topPivot = top;
                bottomPivot = edge.PointOnLine(top, bottom, initialDistance);

                bottomP = edge.PerpendicularAtPoint(bottomPivot);
                topP = block.topEdge;
            }
            else
            {
                topPivot = edge.PointOnLine(top, bottom, segmentLength - initialDistance);
                bottomPivot = bottom;

                bottomP = block.bottomEdge;
                topP = edge.PerpendicularAtPoint(topPivot);
            }

            topLeftBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x + 1), inset);
            topRightBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x + 1), depth + inset);
            bottomLeftBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x + 1), inset);
            bottomRightBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x + 1), depth + inset);
        }
        else //vertical generation
        {
            if(!rightConstraint)
            {
                topPivot = top;
                bottomPivot = edge.PointOnLine(top, bottom, initialDistance);

                leftP = edge.PerpendicularAtPoint(bottomPivot);
                rightP = block.rightEdge;
            }
            else
            {
                topPivot = edge.PointOnLine(top, bottom, segmentLength - initialDistance);
                bottomPivot = bottom;

                leftP = block.leftEdge;
                rightP = edge.PerpendicularAtPoint(topPivot);
            }
            

            topLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y + 1), inset);
            topRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y + 1), inset);
            bottomLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y + 1), depth + inset);
            bottomRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y + 1), depth + inset);
        }

        return new Building(topLeftBound, topRightBound, bottomLeftBound, bottomRightBound);
    }

    Building GenerateBounds(Block block, float width, float depth, float inset, float startDistance, bool rightConstraint, Vector2 top, Vector2 bottom, Line edge)
    {
        Vector2 topPivot, bottomPivot;
        Vector2 topLeftBound, bottomLeftBound, topRightBound, bottomRightBound;

        float segmentLength = Vector2.Distance(top, bottom);

        if(!block.direction) //horizontal generation
        {
            if(!rightConstraint)
            {
                topPivot = edge.PointOnLine(top, bottom, startDistance);
                bottomPivot = edge.PointOnLine(top, bottom, startDistance + width);
            }
            else
            {
                topPivot = edge.PointOnLine(top, bottom, segmentLength - startDistance - width);
                bottomPivot = edge.PointOnLine(top, bottom, segmentLength - startDistance);
            }
            
            Line bottomP = edge.PerpendicularAtPoint(bottomPivot);
            Line topP = edge.PerpendicularAtPoint(topPivot);

            topLeftBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x), inset);
            topRightBound = topP.PointOnLine(topPivot, topP.PointFromX(block.topRight.x), depth + inset);
            bottomLeftBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x), inset);
            bottomRightBound = bottomP.PointOnLine(bottomPivot, bottomP.PointFromX(block.bottomRight.x), depth + inset);
        }
        else //vertical generation
        {
            if(!rightConstraint)
            {
                topPivot = edge.PointOnLine(top, bottom, startDistance);
                bottomPivot = edge.PointOnLine(top, bottom, startDistance + width);
            }
            else
            {
                topPivot = edge.PointOnLine(top, bottom, segmentLength - startDistance - width);
                bottomPivot = edge.PointOnLine(top, bottom, segmentLength - startDistance);
            }
            
            Line leftP = edge.PerpendicularAtPoint(bottomPivot);
            Line rightP = edge.PerpendicularAtPoint(topPivot);

            topLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y), inset);
            topRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y), inset);
            bottomLeftBound = leftP.PointOnLine(bottomPivot, leftP.PointFromY(block.bottomLeft.y), depth + inset);
            bottomRightBound = rightP.PointOnLine(topPivot, rightP.PointFromY(block.bottomRight.y), depth + inset);
        }

        return new Building(topLeftBound, topRightBound, bottomLeftBound, bottomRightBound);
    }
    
    //i swear this code was very hard to write and looks like spaghetti
    //go easy on me
    public void GenerateBuildingsBounds(Block block)
    {
        buildings = new Dictionary<Vector2Int, Building>();

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

        Vector2 top, bottom;
        Line edge;
        
        do
        {
            //variable resetting
            startWithHouse = RandomChance(startWithHouseChance, 100);
            maxRowDepth = 0;
            buildStep = 0;
        
            //row offset alignment calculation
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

            edge = new Line(top, bottom);

            //first building
            if(startWithHouse)
            {
                float width = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;
                float depth = GetDepth() * buildingScale;
                if(depth > 1 * buildingScale)
                    width = 1 * buildingScale;
                float inset = Random.Range(minBuildInset, maxBuildInset + 1) * insetScale;
                leftBuilding = GenerateBounds(block, width, depth, inset, segmentLength / 2 - width / 2, false, top, bottom, edge);
                leftBDistance = segmentLength / 2 - width / 2;
                
                GenerateBuilding(block, new Vector2Int(0, rowStep), leftBuilding);
                buildStep = 1;

                rightBDistance = segmentLength / 2 - width / 2;

                if(depth + inset > maxRowDepth)
                    maxRowDepth = depth + inset;
            }
            else
            {
                leftWidth = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;
                leftDepth = GetDepth() * buildingScale;
                if(leftDepth > 1 * buildingScale)
                    leftWidth = 1 * buildingScale;
                leftInset = Random.Range(minBuildInset, maxBuildInset + 1) * insetScale;
                rightWidth = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;
                rightDepth = GetDepth() * buildingScale;
                if(rightDepth > 1 * buildingScale)
                    rightWidth = 1 * buildingScale;
                rightInset = Random.Range(minBuildInset, maxBuildInset + 1) * insetScale;

                float startDistance = segmentLength / 2 - secondaryRoadSize / 2;
                leftBuilding = GenerateBounds(block, leftWidth, leftDepth, leftInset, startDistance - leftWidth, false, top, bottom, edge);
                rightBuilding = GenerateBounds(block, rightWidth, rightDepth, rightInset, startDistance - rightWidth, true, top, bottom, edge);

                leftBDistance = startDistance - leftWidth;
                rightBDistance = startDistance - rightWidth;

                GenerateBuilding(block, new Vector2Int(1, rowStep), leftBuilding);
                GenerateBuilding(block, new Vector2Int(-1, rowStep), rightBuilding);
                buildStep = 2;

                if(leftDepth + leftInset > maxRowDepth)
                    maxRowDepth = leftDepth + leftInset;
                if(rightDepth + rightInset > maxRowDepth)
                    maxRowDepth = rightDepth + rightInset;
            }

            //building generation loop that runs until BDistances are negative from both corners
            do
            {
                //if the building reached the edge, it just waits for the loop to end and closes up
                //with a corner building    
                if(leftBDistance - secondaryRoadSize - maxBuildWidth * buildingScale > 0)
                {
                    leftWidth = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;
                    leftDepth = GetDepth() * buildingScale;
                    if(leftDepth > 1 * buildingScale)
                        leftWidth = 1 * buildingScale;
                    leftInset = Random.Range(0, maxBuildInset + 1) * insetScale;

                    leftBDistance = leftBDistance - (RandomChance(secondaryRoadChance, 100) ? secondaryRoadSize : 0) - leftWidth;

                    leftBuilding = GenerateBounds(block, leftWidth, leftDepth, leftInset, leftBDistance, false, top, bottom, edge);
                    
                    GenerateBuilding(block, new Vector2Int(buildStep, rowStep), leftBuilding);
                
                    if(leftDepth + leftInset > maxRowDepth)
                        maxRowDepth = leftDepth + leftInset;
                }

                if(rightBDistance - secondaryRoadSize - maxBuildWidth * buildingScale > 0)
                {
                    rightWidth = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;
                    rightDepth = GetDepth() * buildingScale;
                    if(rightDepth > 1 * buildingScale)
                        rightWidth = 1 * buildingScale;
                    rightInset = Random.Range(0, maxBuildInset + 1) * insetScale;

                    rightBDistance = rightBDistance - (RandomChance(secondaryRoadChance, 100) ? secondaryRoadSize : 0) - rightWidth;

                    rightBuilding = GenerateBounds(block, rightWidth, rightDepth, rightInset, rightBDistance, true, top, bottom, edge);

                    GenerateBuilding(block, new Vector2Int(-buildStep, rowStep), rightBuilding);

                    if(rightDepth + rightInset > maxRowDepth)
                        maxRowDepth = rightDepth + rightInset;
                }
                buildStep++;
            }
            while(canFitMoreBuildings(block, leftBDistance, rightBDistance));

            leftDepth = GetDepth() * buildingScale;
            if(leftDepth > 1 * buildingScale)
                leftWidth = 1 * buildingScale;
            leftInset = Random.Range(0, maxBuildInset + 1) * insetScale;
            rightDepth = GetDepth() * buildingScale;
            if(rightDepth > 1 * buildingScale)
                rightWidth = 1 * buildingScale;
            rightInset = Random.Range(0, maxBuildInset + 1) * insetScale;

            leftBDistance = leftBDistance - (RandomChance(secondaryRoadChance, 100) ? secondaryRoadSize : 0);
            rightBDistance = rightBDistance - (RandomChance(secondaryRoadChance, 100) ? secondaryRoadSize : 0);

            leftBuilding = GenerateBounds(block, leftDepth, leftInset, false, leftBDistance, top, bottom, edge);
            rightBuilding = GenerateBounds(block, rightDepth, rightInset, true, rightBDistance, top, bottom, edge); 

            if(Vector2.Distance(leftBuilding.topLeftCorner, leftBuilding.bottomLeftCorner) >= buildingScale)
                GenerateBuilding(block, new Vector2Int(buildStep, rowStep), leftBuilding);

            if(Vector2.Distance(rightBuilding.topLeftCorner, rightBuilding.bottomLeftCorner) >= buildingScale)
                GenerateBuilding(block, new Vector2Int(-buildStep, rowStep), rightBuilding);

            if(leftDepth + leftInset > maxRowDepth)
                maxRowDepth = leftDepth + leftInset;
            if(rightDepth + rightInset > maxRowDepth)
                maxRowDepth = rightDepth + rightInset;

            rowOffset += maxRowDepth + secondaryRoadSize;
            rowStep++;
        }
        while(canFitMoreRows(block, rowOffset));

        //-------------------- FINAL ROW --------------------
        if(canFitLastRow(block, rowOffset))
        {
            //variable resetting
            startWithHouse = RandomChance(startWithHouseChance, 100);
            buildStep = 0;
        
            //row offset alignment calculation
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

            edge = new Line(top, bottom);

            //first building
            if(startWithHouse)
            {
                float width = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;

                leftBuilding = GenerateBounds(block, width, buildingScale, 0, segmentLength / 2 - width / 2, false, top, bottom, edge);
                leftBDistance = segmentLength / 2 - width / 2;
                
                GenerateBuilding(block, new Vector2Int(0, rowStep), leftBuilding);
                buildStep = 1;

                rightBDistance = segmentLength / 2 - width / 2;
            }
            else
            {
                leftWidth = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;
                rightWidth = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;

                float startDistance = segmentLength / 2 - secondaryRoadSize / 2;
                leftBuilding = GenerateBounds(block, leftWidth, buildingScale, 0, startDistance - leftWidth, false, top, bottom, edge);
                rightBuilding = GenerateBounds(block, rightWidth, buildingScale, 0, startDistance - rightWidth, true, top, bottom, edge);

                leftBDistance = startDistance - leftWidth;
                rightBDistance = startDistance - rightWidth;

                GenerateBuilding(block, new Vector2Int(1, rowStep), leftBuilding);
                GenerateBuilding(block, new Vector2Int(-1, rowStep), rightBuilding);
                buildStep = 2;
            }

            //building generation loop that runs until BDistances are negative from both corners
            do
            {
                //if the building reached the edge, it just waits for the loop to end and closes up
                //with a corner building    
                if(leftBDistance - secondaryRoadSize - maxBuildWidth * buildingScale > 0)
                {
                    leftWidth = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;

                    leftBDistance = leftBDistance - (RandomChance(secondaryRoadChance, 100) ? secondaryRoadSize : 0) - leftWidth;

                    leftBuilding = GenerateBounds(block, leftWidth, buildingScale, 0, leftBDistance, false, top, bottom, edge);
                    
                    GenerateBuilding(block, new Vector2Int(buildStep, rowStep), leftBuilding);
                }

                if(rightBDistance - secondaryRoadSize - maxBuildWidth * buildingScale > 0)
                {
                    rightWidth = Random.Range(minBuildWidth, maxBuildWidth + 1) * buildingScale;

                    rightBDistance = rightBDistance - (RandomChance(secondaryRoadChance, 100) ? secondaryRoadSize : 0) - rightWidth;

                    rightBuilding = GenerateBounds(block, rightWidth, buildingScale, 0, rightBDistance, true, top, bottom, edge);

                    GenerateBuilding(block, new Vector2Int(-buildStep, rowStep), rightBuilding);
                }
                buildStep++;
            }
            while(canFitMoreBuildings(block, leftBDistance, rightBDistance));

            leftBDistance = leftBDistance - (RandomChance(secondaryRoadChance, 100) ? secondaryRoadSize : 0);
            rightBDistance = rightBDistance - (RandomChance(secondaryRoadChance, 100) ? secondaryRoadSize : 0);

            leftBuilding = GenerateBounds(block, buildingScale, 0, false, leftBDistance, top, bottom, edge);
            rightBuilding = GenerateBounds(block, buildingScale, 0, true, rightBDistance, top, bottom, edge); 

            if(Vector2.Distance(leftBuilding.topLeftCorner, leftBuilding.bottomLeftCorner) >= buildingScale)
                GenerateBuilding(block, new Vector2Int(buildStep, rowStep), leftBuilding);

            if(Vector2.Distance(rightBuilding.topLeftCorner, rightBuilding.bottomLeftCorner) >= buildingScale)
                GenerateBuilding(block, new Vector2Int(-buildStep, rowStep), rightBuilding);
        }
    }

    int GetDepth(int depth = minBuildDepth)
    {
        if(RandomChance(depthIncreaseChance, 100) && depth < maxBuildDepth)
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
            segmentLength - rowOffset - maxBuildDepth * buildingScale > 0;
    }
    
    bool canFitLastRow(Block block, float rowOffset)
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
            segmentLength - rowOffset - buildingScale > 0;
    }
}
