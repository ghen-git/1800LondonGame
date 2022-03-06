using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    const int gridWidth = 20, gridHeight = 20;
    const float gridSpacing = 15f;
    const float maxOffset = 6f;
    const float gridStartX = -(gridWidth * gridSpacing) / 2;
    const float gridStartY = -(gridHeight * gridSpacing) / 2;

    Vector2[,] roadGrid = new Vector2[gridWidth, gridHeight];
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

   Vector2 CalculateOffset()
    {
        int direction = Random.Range(0, 360);

        float offsetX = Mathf.Cos(Mathf.Deg2Rad * direction) * Random.Range(0, maxOffset);
        float offsetY = Mathf.Sin(Mathf.Deg2Rad * direction) * Random.Range(0, maxOffset);

        return new Vector2(offsetX, offsetY);
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
    }
}
