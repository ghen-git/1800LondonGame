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
    public int floorsNumber;
    public string wallMaterial;

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

public class BuildingRowSegment
{
    public string name;
    public float width;
    public float depth;
    public float inset;
    public int buildingsNumber;
    public bool hasSecondaryRoad;

    public BuildingRowSegment(string name, float width, float depth, float inset, int buildingsNumber, bool hasSecondaryRoad)
    {
        this.name = name;
        this.width = width;
        this.depth = depth;
        this.inset = inset;
        this.buildingsNumber = buildingsNumber;
        this.hasSecondaryRoad = hasSecondaryRoad;
    }
}

public class BuildingGenerator : MonoBehaviour
{
    Dictionary<Vector2Int, Building> buildings;
    int buildStep;
    int rowStep;

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

            //walls rendering
            GameObject walls = RenderQuad
            (
                GetRelativeVertices(building.vertices, xy, center), 
                center,
                building.floorsNumber * floorHeight, 
                buildingGO.name + "-walls", 
                Resources.Load<Material>($"Materials/{building.wallMaterial}"), 
                0.15f,
                new bool[]{true, true, true, true, true, false}
            );

            walls.transform.SetParent(buildingGO.transform, true);
        }
    }

    void GenerateBuilding(Block block, Building building)
    {
        if(!CanGenerate(building, block))
            return;

        block.CalculateSidewalks(building);

        Vector2Int id = new Vector2Int(buildStep, rowStep);

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

    delegate Building BuildingBoundsGenerator(Block block, float offsetFromTopToLeft, float rowOffset, float width, float depth, float inset);

    //returns the buildings row's depth
    float GenerateBuildingsRow(Block block, float edgeWidth, float edgeCenter, BuildingBoundsGenerator generateBounds, float rowOffset, float maxRowDepth)
    {
        buildStep = 0;
        float topRowDepth = 0;

        //segment generation
        float rowLeftWidth = 0;
        float rowRightWidth = 0;
        BuildingRowSegment firstSegment = null;

        //first segment
        do
        {
            firstSegment = PickRowSegment(block, edgeWidth, maxRowDepth);
            rowLeftWidth = firstSegment.width / 2;
            rowRightWidth = firstSegment.width / 2;
        }
        while(rowLeftWidth > edgeCenter || rowRightWidth > edgeWidth - edgeCenter);
        GenerateBuildingsSegment(block, firstSegment, generateBounds, edgeCenter - rowLeftWidth, rowOffset);

        //segment loop
        while(AddSegments(block, edgeWidth, edgeCenter, maxRowDepth, ref rowLeftWidth, ref rowRightWidth, ref topRowDepth, generateBounds, rowOffset));

        return topRowDepth + (topRowDepth == buildingScale ? secondaryRoadSize : 0);
    }

    bool AddSegments(Block block, float edgeWidth, float edgeCenter, float maxRowDepth, ref float rowLeftWidth, ref float rowRightWidth, ref float topRowDepth, BuildingBoundsGenerator generateBounds, float rowOffset)
    {
        bool canAddMoreLeft = buildingScale + secondaryRoadSize < edgeCenter - rowLeftWidth;
        bool canAddMoreRight = buildingScale + secondaryRoadSize < edgeWidth - edgeCenter - rowRightWidth;

        //left segment
        if(canAddMoreLeft)
        {
            BuildingRowSegment leftSegment;

            leftSegment = PickRowSegment(block, edgeCenter - rowLeftWidth, maxRowDepth);
            GenerateBuildingsSegment(block, leftSegment, generateBounds, edgeCenter - rowLeftWidth - leftSegment.width, rowOffset);

            if(leftSegment.depth > topRowDepth)
                topRowDepth = leftSegment.depth;

            rowLeftWidth += leftSegment.width + (leftSegment.hasSecondaryRoad ? secondaryRoadSize : 0);
        }

        //right segment
        if(canAddMoreRight)
        {
            BuildingRowSegment rightSegment;

            rightSegment = PickRowSegment(block, edgeWidth - edgeCenter - rowRightWidth, maxRowDepth);
            GenerateBuildingsSegment(block, rightSegment, generateBounds, edgeCenter + rowRightWidth, rowOffset);

            if(rightSegment.depth > topRowDepth)
                topRowDepth = rightSegment.depth;

            rowRightWidth += rightSegment.width + (rightSegment.hasSecondaryRoad ? secondaryRoadSize : 0);
        }

        return canAddMoreLeft || canAddMoreRight;
    }

    void GenerateBuildingsSegment(Block block, BuildingRowSegment segment, BuildingBoundsGenerator generateBounds, float offsetFromTopToLeft, float rowOffset)
    {
            //segment of identical buildings
            if(segment.name == "identicalBuildings")
            {
                //buildings parameters
                string segmentMat = westEndWallMats[Random.Range(0, westEndWallMats.Length)];
                int identicalBuildingsFloorsNumber = Random.Range(minFloors, maxFloors + 1);
                float width = (segment.width - (segment.hasSecondaryRoad ? secondaryRoadSize * 2 : 0)) / segment.buildingsNumber;
                offsetFromTopToLeft += segment.hasSecondaryRoad ? secondaryRoadSize : 0;
                
                for(int i = 0; i < segment.buildingsNumber; i++)
                {
                    Building building = generateBounds(block, offsetFromTopToLeft + width * i, rowOffset, width, segment.depth, segment.inset);
                    building.wallMaterial = segmentMat;
                    building.floorsNumber = identicalBuildingsFloorsNumber;
                    GenerateBuilding(block, building);
                    buildStep++;
                }
            }
            //just one building
            else /*if(segment.name == "single")*/
            {
                string wallsMat = block.end == "west" ? 
                    westEndWallMats[Random.Range(0, westEndWallMats.Length)] :
                    eastEndWallMats[Random.Range(0, eastEndWallMats.Length)];

                int floorsNumber = Random.Range(minFloors, maxFloors + 1);
                float width = (segment.width - (segment.hasSecondaryRoad ? secondaryRoadSize * 2 : 0));
                offsetFromTopToLeft += segment.hasSecondaryRoad ? secondaryRoadSize : 0;

                Building building = generateBounds(block, offsetFromTopToLeft, rowOffset, width, segment.depth, segment.inset);
                building.wallMaterial = wallsMat;
                building.floorsNumber = floorsNumber;
                GenerateBuilding(block, building);
                buildStep++;
            }
    }

    BuildingRowSegment PickRowSegment(Block block, float edgeWidth, float maxRowDepth, bool canInset = false)
    {
        BuildingRowSegment segment;
        string segmentName;
        int buildWidthModulo;
        float buildingInset;
        float buildingDepth;
        int buildingsNumber;

        //segment of identical buildings
        if(block.end == "west" && identicalBuildingsMinAmount * buildingScale < edgeWidth && RandomChance(identicalBuildingsSegmentChance))
        {
            if(identicalBuildingsMinAmount * minBuildWidth * buildingScale < edgeWidth)
                do
                    buildingsNumber = Random.Range(identicalBuildingsMinAmount, identicalBuildingsMaxAmount + 1);
                while(buildingsNumber * minBuildWidth * buildingScale > edgeWidth);
            else
                buildingsNumber = identicalBuildingsMinAmount;

            segmentName = "identicalBuildings";
        }
        //just one building
        else
        {
            buildingsNumber = 1;
            segmentName = "single";
        }

        if(minBuildWidth * buildingScale * buildingsNumber < edgeWidth)
                do
                    buildWidthModulo = Random.Range(minBuildWidth, maxBuildWidth + 1);
                while(buildWidthModulo * buildingScale * buildingsNumber > edgeWidth);
            else
                buildWidthModulo = minBuildWidth + 1;
        
        if(buildWidthModulo == 1 && buildingScale * 2 + secondaryRoadSize * 2 + maxBuildInset <= maxRowDepth)
            if(RandomChance(impasseChance))
                buildingDepth = 2 * buildingScale + secondaryRoadSize * 2;
            else
                buildingDepth = 2 * buildingScale + secondaryRoadSize;
        else
            buildingDepth = buildingScale;

        if(buildingDepth + minBuildInset * insetScale < maxRowDepth && canInset)
            do
                buildingInset = Random.Range(minBuildInset, maxBuildInset + 1) * insetScale;
            while(buildingDepth + buildingInset > maxRowDepth);
        else
            buildingInset = 0;

        bool hasSecondaryRoad = RandomChance(secondaryRoadChance);

        segment = new BuildingRowSegment
        (
            segmentName, 
            buildWidthModulo * buildingScale + (hasSecondaryRoad ? secondaryRoadSize * 2 : 0), 
            buildingDepth - buildingInset, 
            buildingInset, 
            buildingsNumber, 
            hasSecondaryRoad
        );

        return segment;
    }

    void GenerateBuildingsBounds(Block block)
    {
        buildings = new Dictionary<Vector2Int, Building>();
        rowStep = 0;

        //block midline considering block orientation
        float blockDepth;

        if(!block.direction)
            blockDepth = Vector2.Distance
            (
                Line.MidPoint(block.topLeft, block.bottomLeft),
                Line.MidPoint(block.topRight, block.bottomRight)
            );
        else
            blockDepth = Vector2.Distance
            (
                Line.MidPoint(block.topLeft, block.topRight),
                Line.MidPoint(block.bottomLeft, block.bottomRight)
            );

        float rowOffset;
        float expectedDepth = buildingScale * 2 + secondaryRoadSize * 2 + maxBuildInset;

        //left side
        rowOffset = 0;
        do
        {
            if(!block.direction)
            {
                Line offsetLine = block.leftEdge.PerpendicularAtPoint(block.topLeft);
                Vector2 offsetPoint = offsetLine.PointOnLine(block.topLeft, offsetLine.PointFromX(block.topRight.x), rowOffset);
                
                rowOffset += GenerateBuildingsRow
                (
                    block,
                    Vector2.Distance
                    (
                        offsetPoint,
                        Line.Intersection(block.bottomEdge, Line.ParallelAtPoint(block.leftEdge, offsetPoint))
                    ),
                    Vector2.Distance
                    (
                        offsetPoint,
                        Line.Intersection(block.leftEdge.PerpendicularAtPoint(Line.MidPoint(block.topLeft, block.bottomLeft)), Line.ParallelAtPoint(block.leftEdge, offsetPoint))
                    ), 
                    (Block block, float offsetFromTopToLeft, float rowOffset, float width, float depth, float inset) =>
                    {
                        Vector2 topPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, offsetFromTopToLeft);
                        Vector2 bottomPivot = block.leftEdge.PointOnLine(block.topLeft, block.bottomLeft, offsetFromTopToLeft + width);

                        Line topLine = block.leftEdge.PerpendicularAtPoint(topPivot);
                        Line bottomLine = block.leftEdge.PerpendicularAtPoint(bottomPivot);

                        Vector2 topLeft = topLine.PointOnLine(topPivot, topLine.PointFromX(block.topRight.x), rowOffset + inset);
                        Vector2 topRight = topLine.PointOnLine(topPivot, topLine.PointFromX(block.topRight.x), rowOffset + inset + depth);
                        Vector2 bottomLeft = bottomLine.PointOnLine(bottomPivot, bottomLine.PointFromX(block.bottomRight.x), rowOffset + inset);
                        Vector2 bottomRight = bottomLine.PointOnLine(bottomPivot, bottomLine.PointFromX(block.bottomRight.x), rowOffset + depth + inset);

                        return new Building(topLeft, topRight, bottomLeft, bottomRight);
                    },
                    rowOffset,
                    expectedDepth
                );
            }
            else
            {
                Line offsetLine = block.topEdge.PerpendicularAtPoint(block.topRight);
                Vector2 offsetPoint = offsetLine.PointOnLine(block.topRight, offsetLine.PointFromY(block.bottomRight.y), rowOffset);
                
                rowOffset += GenerateBuildingsRow
                (
                    block,
                    Vector2.Distance
                    (
                        offsetPoint,
                        Line.Intersection(block.leftEdge, Line.ParallelAtPoint(block.topEdge, offsetPoint))
                    ),
                    Vector2.Distance
                    (
                        offsetPoint,
                        Line.Intersection(block.topEdge.PerpendicularAtPoint(Line.MidPoint(block.topRight, block.topLeft)), Line.ParallelAtPoint(block.topEdge, offsetPoint))
                    ), 
                    (Block block, float offsetFromTopToLeft, float rowOffset, float width, float depth, float inset) =>
                    {
                        Vector2 topPivot = block.topEdge.PointOnLine(block.topRight, block.topLeft, offsetFromTopToLeft);
                        Vector2 bottomPivot = block.topEdge.PointOnLine(block.topRight, block.topLeft, offsetFromTopToLeft + width);

                        Line topLine = block.topEdge.PerpendicularAtPoint(topPivot);
                        Line bottomLine = block.topEdge.PerpendicularAtPoint(bottomPivot);

                        Vector2 topLeft = bottomLine.PointOnLine(bottomPivot, bottomLine.PointFromY(block.bottomLeft.y), rowOffset + inset);
                        Vector2 topRight = topLine.PointOnLine(topPivot, topLine.PointFromY(block.bottomRight.y), rowOffset + inset);
                        Vector2 bottomLeft = bottomLine.PointOnLine(bottomPivot, bottomLine.PointFromY(block.bottomLeft.y), rowOffset + depth + inset);
                        Vector2 bottomRight = topLine.PointOnLine(topPivot, topLine.PointFromY(block.bottomRight.y), rowOffset + depth + inset);

                        return new Building(topLeft, topRight, bottomLeft, bottomRight);
                    },
                    rowOffset,
                    expectedDepth
                );
            }

            //calculates the expected new max build depth
            expectedDepth = buildingScale * 2 + secondaryRoadSize * 2 + maxBuildInset;
            if(rowOffset + expectedDepth < blockDepth / 2)
                expectedDepth = buildingScale;
            
            rowStep++;
        }
        while(rowOffset < blockDepth / 2);

        //right side
        rowOffset = 0;
        do
        {
            if(!block.direction)
            {
                Line offsetLine = block.rightEdge.PerpendicularAtPoint(block.bottomRight);
                Vector2 offsetPoint = offsetLine.PointOnLine(block.bottomRight, offsetLine.PointFromX(block.bottomLeft.x), rowOffset);
                
                rowOffset += GenerateBuildingsRow
                (
                    block,
                    Vector2.Distance
                    (
                        offsetPoint,
                        Line.Intersection(block.topEdge, Line.ParallelAtPoint(block.rightEdge, offsetPoint))
                    ),
                    Vector2.Distance
                    (
                        offsetPoint,
                        Line.Intersection(block.rightEdge.PerpendicularAtPoint(Line.MidPoint(block.bottomRight, block.topRight)), Line.ParallelAtPoint(block.rightEdge, offsetPoint))
                    ), 
                    (Block block, float offsetFromTopToLeft, float rowOffset, float width, float depth, float inset) =>
                    {
                        Vector2 topPivot = block.rightEdge.PointOnLine(block.bottomRight, block.topRight, offsetFromTopToLeft);
                        Vector2 bottomPivot = block.rightEdge.PointOnLine(block.bottomRight, block.topRight, offsetFromTopToLeft + width);

                        Line topLine = block.rightEdge.PerpendicularAtPoint(topPivot);
                        Line bottomLine = block.rightEdge.PerpendicularAtPoint(bottomPivot);

                        Vector2 topLeft = bottomLine.PointOnLine(bottomPivot, bottomLine.PointFromX(block.topLeft.x), rowOffset + inset + depth);
                        Vector2 topRight = bottomLine.PointOnLine(bottomPivot, bottomLine.PointFromX(block.topLeft.x), rowOffset + inset);
                        Vector2 bottomLeft = topLine.PointOnLine(topPivot, topLine.PointFromX(block.bottomLeft.x), rowOffset + inset + depth);
                        Vector2 bottomRight = topLine.PointOnLine(topPivot, topLine.PointFromX(block.bottomLeft.x), rowOffset + inset);

                        return new Building(topLeft, topRight, bottomLeft, bottomRight);
                    },
                    rowOffset,
                    expectedDepth
                );
            }
            else
            {
                Line offsetLine = block.bottomEdge.PerpendicularAtPoint(block.bottomLeft);
                Vector2 offsetPoint = offsetLine.PointOnLine(block.bottomLeft, offsetLine.PointFromY(block.topLeft.y), rowOffset);
                
                rowOffset += GenerateBuildingsRow
                (
                    block,
                    Vector2.Distance
                    (
                        offsetPoint,
                        Line.Intersection(block.rightEdge, Line.ParallelAtPoint(block.bottomEdge, offsetPoint))
                    ),
                    Vector2.Distance
                    (
                        offsetPoint,
                        Line.Intersection(block.bottomEdge.PerpendicularAtPoint(Line.MidPoint(block.bottomLeft, block.bottomRight)), Line.ParallelAtPoint(block.bottomEdge, offsetPoint))
                    ), 
                    (Block block, float offsetFromTopToLeft, float rowOffset, float width, float depth, float inset) =>
                    {
                        Vector2 topPivot = block.bottomEdge.PointOnLine(block.bottomLeft, block.bottomRight, offsetFromTopToLeft);
                        Vector2 bottomPivot = block.bottomEdge.PointOnLine(block.bottomLeft, block.bottomRight, offsetFromTopToLeft + width);

                        Line topLine = block.bottomEdge.PerpendicularAtPoint(topPivot);
                        Line bottomLine = block.bottomEdge.PerpendicularAtPoint(bottomPivot);

                        Vector2 topLeft = topLine.PointOnLine(topPivot, topLine.PointFromY(block.topLeft.y), rowOffset + depth + inset);
                        Vector2 topRight = bottomLine.PointOnLine(bottomPivot, bottomLine.PointFromY(block.topRight.y), rowOffset + depth + inset);
                        Vector2 bottomLeft = topLine.PointOnLine(topPivot, topLine.PointFromY(block.topLeft.y), rowOffset + inset);
                        Vector2 bottomRight = bottomLine.PointOnLine(bottomPivot, bottomLine.PointFromY(block.topRight.y), rowOffset + inset);

                        return new Building(topLeft, topRight, bottomLeft, bottomRight);
                    },
                    rowOffset,
                    expectedDepth
                );
            }

            //calculates the expected new max build depth
            expectedDepth = buildingScale * 2 + secondaryRoadSize * 2 + maxBuildInset;
            if(rowOffset + expectedDepth < blockDepth / 2)
                expectedDepth = buildingScale;
            
            rowStep++;
        }
        while(rowOffset + expectedDepth < blockDepth / 2);
    }

    // int GetDepth(int depth = minBuildDepth)
    // {
        // if(RandomChance(depthIncreaseChance, 100) && depth < maxBuildDepth)
            // depth = GetDepth(depth + 1);
        // return depth;
    // }

    bool CanFitMoreBuildings(Block block, float leftBDistance, float rightBDistance)
    {
        return 
            leftBDistance - secondaryRoadSize - maxBuildWidth * buildingScale > 0 ||
            rightBDistance - secondaryRoadSize - maxBuildWidth * buildingScale > 0;
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
