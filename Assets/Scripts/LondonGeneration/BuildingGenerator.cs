using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;
using static GraphicsUtil;
using static LondonSettings;
using System.Threading.Tasks;

public class Building
{
    public Vector2 topLeftCorner;
    public Vector2 topRightCorner;
    public Vector2 bottomLeftCorner;
    public Vector2 bottomRightCorner;
    public int floorsNumber;
    public string wallMaterial;
    public char direction;
    public bool roofDirection;
    public float width;
    public float depth;

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

    public Building(Vector2 topLeftCorner, Vector2 topRightCorner, Vector2 bottomLeftCorner, Vector2 bottomRightCorner, char direction)
    {
        this.topLeftCorner = topLeftCorner;
        this.topRightCorner = topRightCorner;
        this.bottomLeftCorner = bottomLeftCorner;
        this.bottomRightCorner = bottomRightCorner;
        this.direction = direction;

        if(direction == 'w' || direction == 'e')
        {
            width = Vector2.Distance(topLeftCorner, bottomLeftCorner);
            depth = Vector2.Distance(topLeftCorner, topRightCorner);

            roofDirection = width > depth;
        }
        else
        {
            width = Vector2.Distance(topLeftCorner, topRightCorner);
            depth = Vector2.Distance(topLeftCorner, bottomLeftCorner);

            roofDirection = width < depth;
        }
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

    //doors and windows meshes reference
    GameObject blankDoorGO;
    GameObject blankWindowGO;

    public Dictionary<Vector2Int, Building> GenerateBuildings(Block block)
    {
        GenerateBuildingsBounds(block);

        return buildings;
    }

    async Task<GameObject> RenderBuilding(Block block, Vector2 xy, Building building, Vector2 center, string name)
    {
        Vector2 topLeft, topRight, bottomLeft, bottomRight;
        float rotation;

        switch(building.direction)
        {
        case 'n':
            topLeft = building.topLeftCorner;
            topRight = building.topRightCorner;
            bottomLeft = building.bottomLeftCorner;
            bottomRight = building.bottomRightCorner;
            rotation = 0f;
            break;
        case 's':
            topLeft = building.bottomRightCorner;
            topRight = building.bottomLeftCorner;
            bottomLeft = building.topRightCorner;
            bottomRight = building.topLeftCorner;
            rotation = 180f;
            break;
        case 'w':
            topLeft = building.bottomLeftCorner;
            topRight = building.topLeftCorner;
            bottomLeft = building.bottomRightCorner;
            bottomRight = building.topRightCorner;
            rotation = -90f;
            break;
        case 'e':
            topLeft = building.topRightCorner;
            topRight = building.bottomRightCorner;
            bottomLeft = building.topLeftCorner;
            bottomRight = building.bottomLeftCorner;
            rotation = 90f;
            break;
        default:
            topLeft = building.topLeftCorner;
            topRight = building.topRightCorner;
            bottomLeft = building.bottomLeftCorner;
            bottomRight = building.bottomRightCorner;
            rotation = 0f;
            break;
        }

        return await RenderBuildingMesh(block, xy, building, center, topLeft, topRight, bottomLeft, bottomRight, rotation, "BlankDoor", "BlankWindow");
    }

    /*
        The 4 vectors you pass represent the corners of the building, and they are called
        as if the building was oriented from north, so the door will be placed on the
        northern side. Thus, you shall orient the points accordingly

        Hope this made sense, it's the most modular appoach i came up with.
    */
    public async Task<GameObject> RenderBuildingMesh(Block block, Vector2 xy, Building building, Vector2 center, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, float rotation, string doorMeshName, string windowMeshName)
    {
        GameObject buildingGO = null;
        MeshFilter buildingMesh = null;
        CombineInstance[] combineData = null;

        await Dispatcher.RunOnMainThreadAsync(() =>
        {
            GameObject doorMesh = GameObject.Find(doorMeshName);
            GameObject windowMesh = GameObject.Find(windowMeshName);

            //building gameObject setup
            buildingGO = new GameObject(name);
            
            buildingMesh = buildingGO.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = buildingGO.AddComponent<MeshRenderer>();

            meshRenderer.material = Resources.Load<Material>($"Materials/{building.wallMaterial}");
            
            //calculation of variables handy in the building's mesh generation
            Line leftEdge = new Line(topLeft, bottomLeft);
            Line topEdge = new Line(topRight, topLeft);
            Line rightEdge = new Line(bottomRight, topRight);
            Line bottomEdge = new Line(bottomLeft, bottomRight);
            float alignment = AngleFrom3Points(building.bottomRightCorner, building.bottomLeftCorner, new Vector2(building.bottomLeftCorner.x + 1, building.bottomLeftCorner.y));

            int meshesPerFloor = System.Convert.ToInt32((building.width / buildingWallSize) * 2 + (building.depth / buildingWallSize) * 2);

            //variable used to store the combine mesh data
            combineData = new CombineInstance[System.Convert.ToInt32
            (
                meshesPerFloor * building.floorsNumber
            )];

            //first floor generation

            //top side
            GameObject targetMesh;
            int doorIndex = System.Convert.ToInt32(Mathf.Ceil((building.width / buildingWallSize) / 2));

            
                for(int i = 0; i < building.width / buildingWallSize; i++)
                {
                    if(i == doorIndex - 1)
                        targetMesh = doorMesh;
                    else 
                        targetMesh = windowMesh;
                    
                    Vector2 pos = topEdge.PointOnLine(topRight, topLeft, i * buildingWallSize + buildingWallSize / 2) + xy;

                    targetMesh.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                    targetMesh.transform.Rotate(new Vector3(0, -90f + rotation + alignment * Mathf.Rad2Deg, 0));
                    targetMesh.transform.position = new Vector3(pos.x, 0 * floorHeight, pos.y);

                    combineData[meshesPerFloor * 0 + i].mesh = targetMesh.GetComponent<MeshFilter>().mesh;
                    combineData[meshesPerFloor * 0 + i].transform = targetMesh.transform.localToWorldMatrix;
                }

                //left side
                for(int i = 0; i < building.depth / buildingWallSize - buildingWallSize / 8; i++)
                {            
                    Vector2 pos = leftEdge.PointOnLine(topLeft, bottomLeft, i * buildingWallSize + buildingWallSize / 2) + xy;

                    windowMesh.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                    windowMesh.transform.Rotate(new Vector3(0, 180f + rotation + alignment * Mathf.Rad2Deg, 0));
                    windowMesh.transform.position = new Vector3(pos.x, 0 * floorHeight, pos.y);

                    combineData[meshesPerFloor * 0 + i + System.Convert.ToInt32(building.width / buildingWallSize)].mesh = windowMesh.GetComponent<MeshFilter>().mesh;
                    combineData[meshesPerFloor * 0 + i + System.Convert.ToInt32(building.width / buildingWallSize)].transform = windowMesh.transform.localToWorldMatrix;
                }
                //bottom side
                for(int i = 0; i < building.width / buildingWallSize; i++)
                {            
                    Vector2 pos = bottomEdge.PointOnLine(bottomLeft, bottomRight, i * buildingWallSize + buildingWallSize / 2) + xy;

                    windowMesh.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                    windowMesh.transform.Rotate(new Vector3(0, 90f + rotation + alignment * Mathf.Rad2Deg, 0));
                    windowMesh.transform.position = new Vector3(pos.x, 0 * floorHeight, pos.y);

                    combineData[meshesPerFloor * 0 + i + System.Convert.ToInt32(building.width / buildingWallSize + building.depth / buildingWallSize)].mesh = windowMesh.GetComponent<MeshFilter>().mesh;
                    combineData[meshesPerFloor * 0 + i + System.Convert.ToInt32(building.width / buildingWallSize + building.depth / buildingWallSize)].transform = windowMesh.transform.localToWorldMatrix;
                }
                //right side
                for(int i = 0; i < building.depth / buildingWallSize - buildingWallSize / 8; i++)
                {            
                    Vector2 pos = rightEdge.PointOnLine(bottomRight, topRight, i * buildingWallSize + buildingWallSize / 2) + xy;

                    windowMesh.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                    windowMesh.transform.Rotate(new Vector3(0, 0f + rotation + alignment * Mathf.Rad2Deg, 0));
                    windowMesh.transform.position = new Vector3(pos.x, 0 * floorHeight, pos.y);

                    combineData[meshesPerFloor * 0 + i + System.Convert.ToInt32((building.width / buildingWallSize) * 2 + building.depth / buildingWallSize)].mesh = windowMesh.GetComponent<MeshFilter>().mesh;
                    combineData[meshesPerFloor * 0 + i + System.Convert.ToInt32((building.width / buildingWallSize) * 2 + building.depth / buildingWallSize)].transform = windowMesh.transform.localToWorldMatrix;
                }

            for(int floorN = 1; floorN < building.floorsNumber; floorN++)
            {
                for(int i = 0; i < building.width / buildingWallSize; i++)
                {                    
                    Vector2 pos = topEdge.PointOnLine(topRight, topLeft, i * buildingWallSize + buildingWallSize / 2) + xy;

                    windowMesh.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                    windowMesh.transform.Rotate(new Vector3(0, -90f + rotation + alignment * Mathf.Rad2Deg, 0));
                    windowMesh.transform.position = new Vector3(pos.x, floorN * floorHeight, pos.y);

                    combineData[meshesPerFloor * floorN + i].mesh = windowMesh.GetComponent<MeshFilter>().mesh;
                    combineData[meshesPerFloor * floorN + i].transform = windowMesh.transform.localToWorldMatrix;
                }

                //left side
                for(int i = 0; i < building.depth / buildingWallSize - buildingWallSize / 8; i++)
                {            
                    Vector2 pos = leftEdge.PointOnLine(topLeft, bottomLeft, i * buildingWallSize + buildingWallSize / 2) + xy;

                    windowMesh.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                    windowMesh.transform.Rotate(new Vector3(0, 180f + rotation + alignment * Mathf.Rad2Deg, 0));
                    windowMesh.transform.position = new Vector3(pos.x, floorN * floorHeight, pos.y);

                    combineData[meshesPerFloor * floorN + i + System.Convert.ToInt32(building.width / buildingWallSize)].mesh = windowMesh.GetComponent<MeshFilter>().mesh;
                    combineData[meshesPerFloor * floorN + i + System.Convert.ToInt32(building.width / buildingWallSize)].transform = windowMesh.transform.localToWorldMatrix;
                }
                //bottom side
                for(int i = 0; i < building.width / buildingWallSize; i++)
                {            
                    Vector2 pos = bottomEdge.PointOnLine(bottomLeft, bottomRight, i * buildingWallSize + buildingWallSize / 2) + xy;

                    windowMesh.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                    windowMesh.transform.Rotate(new Vector3(0, 90f + rotation + alignment * Mathf.Rad2Deg, 0));
                    windowMesh.transform.position = new Vector3(pos.x, floorN * floorHeight, pos.y);

                    combineData[meshesPerFloor * floorN + i + System.Convert.ToInt32(building.width / buildingWallSize + building.depth / buildingWallSize)].mesh = windowMesh.GetComponent<MeshFilter>().mesh;
                    combineData[meshesPerFloor * floorN + i + System.Convert.ToInt32(building.width / buildingWallSize + building.depth / buildingWallSize)].transform = windowMesh.transform.localToWorldMatrix;
                }
                //right side
                for(int i = 0; i < building.depth / buildingWallSize - buildingWallSize / 8; i++)
                {            
                    Vector2 pos = rightEdge.PointOnLine(bottomRight, topRight, i * buildingWallSize + buildingWallSize / 2) + xy;

                    windowMesh.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                    windowMesh.transform.Rotate(new Vector3(0, 0f + rotation + alignment * Mathf.Rad2Deg, 0));
                    windowMesh.transform.position = new Vector3(pos.x, floorN * floorHeight, pos.y);

                    combineData[meshesPerFloor * floorN + i + System.Convert.ToInt32((building.width / buildingWallSize) * 2 + building.depth / buildingWallSize)].mesh = windowMesh.GetComponent<MeshFilter>().mesh;
                    combineData[meshesPerFloor * floorN + i + System.Convert.ToInt32((building.width / buildingWallSize) * 2 + building.depth / buildingWallSize)].transform = windowMesh.transform.localToWorldMatrix;
                }
            }
            
            buildingMesh.mesh.CombineMeshes(combineData);
        });

        await Dispatcher.RunOnMainThreadAsync(() =>
        {
            buildingGO.AddComponent<MeshCollider>();
            buildingGO.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        });
        return buildingGO;
    }

    /*public void WallsInit()
    {
        //gets the assets from 
        blankDoorGO = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Walls/BlankDoor"));
        blankWindowGO = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Walls/BlankWindow"));

        //deactivates the gameObjects so they won't make a mess when rendering
        blankDoorGO.SetActive(false);
        blankWindowGO.SetActive(false);
    }

    public void ClearWalls()
    {
        Destroy(blankDoorGO);
        Destroy(blankWindowGO);
    }*/

    public async void RenderBuildings(Block block, Vector2Int coords)
    {
        foreach(Vector2Int buildingCoords in block.buildings.Keys)
        {
            
            Building building = block.buildings[buildingCoords];

            Vector2 xy = new Vector2(coords.x, coords.y) * blockSize;

            //get building center
            Vector2 center = GetGlobalCenter(building.vertices, xy);
            
            string blockName = "";
            
            await Dispatcher.RunOnMainThreadAsync(() =>
            {
                if(block.block != null)
                    blockName = block.block.name;
                else    
                    blockName = "destroyed";
            });

            if(blockName != "destroyed")
            {
                //building gameObject
                GameObject buildingGO = await RenderBuilding(block, xy, building, center, blockName + "|building-" + VectToName(buildingCoords));
                Dispatcher.RunOnMainThread(() =>
                {
                    if(block.block != null)
                        buildingGO.transform.SetParent(block.block.transform, true);
                });
            }
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

        //to generate secondary segments
        bool hasSecondarySegments;
        List<SegmentData> shortSegments = new List<SegmentData>();

        //first segment
        do
        {
            firstSegment = PickRowSegment(block, edgeWidth, maxRowDepth);
            rowLeftWidth = firstSegment.width / 2;
            rowRightWidth = firstSegment.width / 2;
        }
        while(rowLeftWidth > edgeCenter || rowRightWidth > edgeWidth - edgeCenter);

        hasSecondarySegments = firstSegment.depth > buildingScale;
        GenerateBuildingsSegment(block, firstSegment, generateBounds, edgeCenter - rowLeftWidth, rowOffset);

        if(firstSegment.depth <= buildingScale)
            shortSegments.Add(new SegmentData(firstSegment, edgeCenter - rowLeftWidth, rowOffset));

        topRowDepth = firstSegment.depth;

        //segment loop
        while(AddSegments(block, edgeWidth, edgeCenter, maxRowDepth, ref rowLeftWidth, ref rowRightWidth, ref topRowDepth, ref hasSecondarySegments, shortSegments, generateBounds, rowOffset));
        
        //secondary segments generation
        if(hasSecondarySegments)
        {
            foreach(SegmentData segmentData in shortSegments)
                GenerateBuildingsSegment(block, segmentData.segment, generateBounds, segmentData.offsetFromTopToLeft, rowOffset + buildingScale + secondaryRoadSize);
        }

        return topRowDepth + (topRowDepth == buildingScale ? secondaryRoadSize : 0);
    }

    struct SegmentData
    {
        public BuildingRowSegment segment;
        public float offsetFromTopToLeft;
        public float rowOffset;

        public SegmentData(BuildingRowSegment segment, float offsetFromTopToLeft, float rowOffset)
        {
            this.segment = segment;
            this.offsetFromTopToLeft = offsetFromTopToLeft;
            this.rowOffset = rowOffset;
        }
    }

    bool AddSegments(Block block, float edgeWidth, float edgeCenter, float maxRowDepth, ref float rowLeftWidth, ref float rowRightWidth, ref float topRowDepth, ref bool hasSecondarySegments, List<SegmentData> shortSegments, BuildingBoundsGenerator generateBounds, float rowOffset)
    {
        bool canAddMoreLeft = buildingScale * minBuildWidth + secondaryRoadSize < edgeCenter - rowLeftWidth;
        bool canAddMoreRight = buildingScale * minBuildWidth + secondaryRoadSize < edgeWidth - edgeCenter - rowRightWidth;

        //left segment
        if(canAddMoreLeft)
        {
            BuildingRowSegment leftSegment;

            leftSegment = PickRowSegment(block, edgeCenter - rowLeftWidth, maxRowDepth);
            GenerateBuildingsSegment(block, leftSegment, generateBounds, edgeCenter - rowLeftWidth - leftSegment.width, rowOffset);

            if(leftSegment.depth > topRowDepth)
                topRowDepth = leftSegment.depth;

            if(!hasSecondarySegments)
                hasSecondarySegments = leftSegment.depth > buildingScale;

            if(leftSegment.depth <= buildingScale)
                shortSegments.Add(new SegmentData(leftSegment, edgeCenter - rowLeftWidth - leftSegment.width, rowOffset));


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
                
            if(!hasSecondarySegments)
                hasSecondarySegments = rightSegment.depth > buildingScale;

            if(rightSegment.depth <= buildingScale)
                shortSegments.Add(new SegmentData(rightSegment, edgeCenter + rowRightWidth, rowOffset));

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
        //touples
        else if(segment.name == "touple")
        {
            //buildings parameters
            string wallsMat = block.end == "west" ? 
                westEndWallMats[Random.Range(0, westEndWallMats.Length)] :
                eastEndWallMats[Random.Range(0, eastEndWallMats.Length)];
            int identicalBuildingsFloorsNumber = Random.Range(minFloors, maxFloors + 1);
            float width = (segment.width - (segment.hasSecondaryRoad ? secondaryRoadSize * 2 : 0)) / segment.buildingsNumber;
            offsetFromTopToLeft += segment.hasSecondaryRoad ? secondaryRoadSize : 0;
            
            for(int i = 0; i < segment.buildingsNumber; i++)
            {
                Building building = generateBounds(block, offsetFromTopToLeft + width * i, rowOffset, width, segment.depth, segment.inset);
                building.wallMaterial = wallsMat;
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
        if(block.end == "west" && identicalBuildingsMinAmount * minBuildWidth * buildingScale < edgeWidth && RandomChance(identicalBuildingsSegmentChance))
        {
            do
                buildingsNumber = Random.Range(identicalBuildingsMinAmount, identicalBuildingsMaxAmount + 1);
            while(buildingsNumber * minBuildWidth * buildingScale > edgeWidth);

            segmentName = "identicalBuildings";
        }
        //touples (two buildings close to each others)
        else if(2 * minBuildWidth * buildingScale < edgeWidth && RandomChance(toupleBuildingsChance))
        {
            buildingsNumber = 2;
            segmentName = "touple";
        }
        //just one building
        else
        {
            buildingsNumber = 1;
            segmentName = "single";
        }

        do
            buildWidthModulo = Random.Range(minBuildWidth, maxBuildWidth + 1);
        while(buildWidthModulo * buildingScale * buildingsNumber > edgeWidth);
        
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
            buildWidthModulo * buildingScale * buildingsNumber + (hasSecondaryRoad ? secondaryRoadSize * 2 : 0), 
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

                        return new Building(topLeft, topRight, bottomLeft, bottomRight, 'w');
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

                        return new Building(topLeft, topRight, bottomLeft, bottomRight, 'n');
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

                        return new Building(topLeft, topRight, bottomLeft, bottomRight, 'e');
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

                        return new Building(topLeft, topRight, bottomLeft, bottomRight, 's');
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
