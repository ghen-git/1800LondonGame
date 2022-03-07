using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Vector3 topLeft;
    public Vector3 topRight;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;
}

public class RoadGenerator : MonoBehaviour
{
    int renderDistance = 3;
    float blockSize = 10f;
    Dictionary<Vector2Int, Block> worldMap = new Dictionary<Vector2Int, Block>();
    Transform player;
    GameObject housePrefab;
    Vector2Int[] bounds; // top-left, top-right, bottom-left, bottom-right
    Vector2Int[] loadedBounds; // top-left, top-right, bottom-left, bottom-right

    void Start()
    {
        housePrefab = Resources.Load<GameObject>("shitass-house");
        player = GameObject.Find("Player").GetComponent<Transform>();

        CalculateBounds();
        LoadStart();
        loadedBounds = bounds;
    }
    
    void Update()
    {
        CalculateBounds();
        if(ShouldLoadBlocks())
        {
            LoadBlocks();
            UnloadBlocks();
            loadedBounds = bounds;
        }
    }

    string VectToName(Vector2Int vect)
    {
        return vect.x.ToString() + ";" + vect.y.ToString();
    }

    Vector2Int NameToVect(string name)
    {
        return new Vector2Int(Convert.ToInt32(name.Split(';')[0]), Convert.ToInt32(name.Split(';')[1]));
    }

    void LoadStart()
    {
        for(int x = bounds[2].x; x <= bounds[1].x; x++)
            for(int y = bounds[2].y; y <= bounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);
                GameObject house = GameObject.Instantiate(housePrefab, new Vector3(block.x * blockSize, 0, block.y * blockSize), new Quaternion());
                house.name = VectToName(block);
                house.AddComponent<MeshCollider>();
            }
    }

    void LoadBlocks()
    {
        for(int x = bounds[2].x; x <= bounds[1].x; x++)
            for(int y = bounds[2].y; y <= bounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(IsInBounds(block, bounds) && !IsInBounds(block, loadedBounds))
                {
                    GameObject house = GameObject.Instantiate(housePrefab, new Vector3(block.x * blockSize, 0, block.y * blockSize), new Quaternion());
                    house.name = VectToName(block);
                    house.AddComponent<MeshCollider>();
                }
            }

    }
    void UnloadBlocks()
    {
        for(int x = loadedBounds[2].x; x <= loadedBounds[1].x; x++)
            for(int y = loadedBounds[2].y; y <= loadedBounds[1].y; y++)
            {
                Vector2Int block = new Vector2Int(x, y);

                if(IsInBounds(block, loadedBounds) && !IsInBounds(block, bounds))
                {
                    Destroy(GameObject.Find(VectToName(block)));
                }
            }
    }

    bool IsInBounds(Vector2Int pos, Vector2Int[] bounds)
    {
        return 
        pos.x >= bounds[2].x && pos.y >= bounds[2].y &&
        pos.x <= bounds[1].x && pos.y <= bounds[1].y;
    }

    void CalculateBounds()
    {
        bounds = new Vector2Int[4];

        Vector2Int currentBlock = new Vector2Int((int)(player.position.x / blockSize), (int)(player.position.z / blockSize));
        print(currentBlock);
        
        bounds[0] = new Vector2Int(currentBlock.x - renderDistance, currentBlock.y + renderDistance);
        bounds[1] = new Vector2Int(currentBlock.x + renderDistance, currentBlock.y + renderDistance);
        bounds[2] = new Vector2Int(currentBlock.x - renderDistance, currentBlock.y - renderDistance);
        bounds[3] = new Vector2Int(currentBlock.x + renderDistance, currentBlock.y - renderDistance);
    }

    bool ShouldLoadBlocks()
    {
        return 
        !bounds[0].Equals(loadedBounds[0]) ||
        !bounds[1].Equals(loadedBounds[1]) ||
        !bounds[2].Equals(loadedBounds[2]) ||
        !bounds[3].Equals(loadedBounds[3]);
    }

    #region oldGenerator
    /*const int gridWidth = 20, gridHeight = 20;
    const float gridSpacing = 15f;
    const float maxOffset = 6f;
    const float gridStartX = -(gridWidth * gridSpacing) / 2;
    const float gridStartY = -(gridHeight * gridSpacing) / 2;

    Vector2Int[,] roadGrid = new Vector2Int[gridWidth, gridHeight];
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    void Start()
    {
        //Random.seed = System.DateTime.Now.GetHashCode();

        PopulateGrid();
        RenderRoad();
    }

    void RenderRoad()
    {
        int steps = 5;
        int startingX = 10, startingY = 10;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        Mesh mesh = new Mesh();

        DrawCross(startingX, startingY);
        for(int step = 1; step < steps; step++)
        {
            for(int pointN = 0; pointN < step + 1; pointN++)
            {
                //left section
                if(startingX - step >= 0 && startingY - step + pointN * 2 >= 0)
                    RenderCross(startingX - step, startingY - step + pointN * 2);

                //right section
                if(startingX + step < gridWidth && startingY - step + pointN * 2 >= 0)
                    RenderCross(startingX + step, startingY - step + pointN * 2);

                //down section
                if(startingY - step >= 0 && startingX - step + pointN * 2 >= 0)
                    RenderCross(startingX - step + pointN * 2, startingY - step);

                //up section
                if(startingY + step < gridHeight && startingX - step + pointN * 2 >= 0)
                    RenderCross(startingX - step + pointN * 2, startingY + step);
            }
        }
    }

    void PopulateGrid()
    {
        for(int x = 0; x < gridWidth; x++)
            for(int y = 0; y < gridHeight; y++)
            {
                roadGrid[x, y] = CalculateOffset();
            }
    }

   Vector2Int CalculateOffset()
    {
        int direction = Random.Range(0, 360);

        float offsetX = Mathf.Cos(Mathf.Deg2Rad * direction) * Random.Range(0, maxOffset);
        float offsetY = Mathf.Sin(Mathf.Deg2Rad * direction) * Random.Range(0, maxOffset);

        return new Vector2Int(offsetX, offsetY);
    }

    // void OnDrawGizmos()
    // {

    //     calculatedPoints = new List<string>();


    //     for(int x = 0; x < gridWidth - 1; x++)
    //         for(int y = 0; y < gridHeight - 1; y++)
    //         {
    //             float gridX = gridStartX + roadGrid[x, y].x + (x * gridSpacing);
    //             float gridY = gridStartY + roadGrid[x, y].y + (y * gridSpacing);

    //             Gizmos.DrawSphere(new Vector3(gridX, 0, gridY), 0.1f);
    //         }
    // }

void RenderCross(int x, int y)
{
    //left
    if(x - 1 >= 0)
        RenderStake(GetVector(x, y), GetVector(x - 1, y));
    //right
    if(x + 1 < gridWidth)
        RenderStake(GetVector(x, y), GetVector(x - 1, y));
    //down
    if(y - 1 >= 0)
        RenderStake(GetVector(x, y), GetVector(x - 1, y));
    //up
    if(y + 1 < gridHeight)
        RenderStake(GetVector(x, y), GetVector(x - 1, y));
}

void RenderStake(Vector3 from, Vector3 to)
{
}

    void DrawCross(int x, int y)
    {
        //left
        if(x - 1 >= 0)
            Gizmos.DrawLine(GetVector(x, y), GetVector(x - 1, y));
        //right
        if(x + 1 < gridWidth)
            Gizmos.DrawLine(GetVector(x, y), GetVector(x + 1, y));
        //down
        if(y - 1 >= 0)
            Gizmos.DrawLine(GetVector(x, y), GetVector(x, y - 1));
        //up
        if(y + 1 < gridHeight)
            Gizmos.DrawLine(GetVector(x, y), GetVector(x, y + 1));
    }

    Vector3 GetVector(int x, int y)
    {
        float gridX = gridStartX + roadGrid[x, y].x + (x * gridSpacing);
        float gridY = gridStartY + roadGrid[x, y].y + (y * gridSpacing);

        return new Vector3(gridX, 0, gridY);
    }*/
    #endregion oldGenerator
}
