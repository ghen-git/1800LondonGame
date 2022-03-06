using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGraphicsTesting : MonoBehaviour
{
    float width = 5;
    float height = 5;

    int[,] grid = new int[50, 50];
    // {
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
    //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
    // }
    float gridOffset = 3.5f;

    GameObject housePrefab;

    public void Start()
    {
        housePrefab = Resources.Load<GameObject>("shitass-house");

        RandomizeGrid();
        PopulateGrid();

        #region renderingTesting
        /*
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

        texture.SetPixel(0, 0, new Color(1.0f, 0.0f, 0.0f, 1.0f));
        texture.Apply();

        meshRenderer.material.mainTexture = texture;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            //bottom quad
            new Vector3(-(width / 2), -(height / 2), -(height / 2)), //low bottom left 0
            new Vector3(-(width / 2), -(height / 2), (height / 2)), //low top left 1
            new Vector3((width / 2), -(height / 2), -(height / 2)), //low bottom right 2
            new Vector3((width / 2), -(height / 2), (height / 2)), //low top right 3

            //front quad
            new Vector3(-(width / 2), -(height / 2), -(height / 2)), //low bottom left 4
            new Vector3(-(width / 2), (height / 2), -(height / 2)), //high bottom left 5
            new Vector3((width / 2), -(height / 2), -(height / 2)), //low bottom right 6
            new Vector3((width / 2), (height / 2), -(height / 2)), //high bottom right 7

            //left quad
            new Vector3(-(width / 2), -(height / 2), (height / 2)), //low top left 8
            new Vector3(-(width / 2), (height / 2), (height / 2)), //high top left 9
            new Vector3(-(width / 2), -(height / 2), -(height / 2)), //low bottom left 10
            new Vector3(-(width / 2), (height / 2), -(height / 2)), //high bottom left 11

            //right quad
            new Vector3((width / 2), -(height / 2), -(height / 2)), //low bottom right 12 
            new Vector3((width / 2), (height / 2), -(height / 2)), //high bottom right 13
            new Vector3((width / 2), -(height / 2), (height / 2)), //low top right 14
            new Vector3((width / 2), (height / 2), (height / 2)), //high top right 15

            //back quad
            new Vector3((width / 2), -(height / 2), (height / 2)), //low top right 16
            new Vector3((width / 2), (height / 2), (height / 2)), //high top right 17
            new Vector3(-(width / 2), -(height / 2), (height / 2)), //low top left 18
            new Vector3(-(width / 2), (height / 2), (height / 2)), //high top left 19

            //top quad
            new Vector3(-(width / 2), (height / 2), -(height / 2)), //high bottom left 20
            new Vector3(-(width / 2), (height / 2), (height / 2)), //high top left 21
            new Vector3((width / 2), (height / 2), -(height / 2)), //high bottom right 22
            new Vector3((width / 2), (height / 2), (height / 2)) //high top right 23
        };
        mesh.vertices = vertices;

        int[] tris = new int[]
        {
            1, 3, 0,
            0, 3, 2,
            4, 5, 6,
            6, 5, 7,
            8, 9, 10,
            10, 9, 11,
            12, 13, 14,
            14, 13, 15,
            16, 17, 18,
            18, 17, 19,
            20, 21, 22,
            22, 21, 23
        };
        mesh.triangles = tris;

        Vector2[] uv = new Vector2[24]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();*/
        #endregion renderingTesting
    }

    void RandomizeGrid()
    {
        Random.seed = System.DateTime.Now.GetHashCode();

        for(int x = 0; x < grid.GetLength(0); x++)
            for(int y = 0; y < grid.GetLength(1); y++)
                grid[x, y] = Random.value > .5f ? 1 : 0;
    }

    void PopulateGrid()
    {  
        float startXPos = -(((gridOffset * 2) * grid.GetLength(0)) / 2) + gridOffset;
        float startYPos = -(((gridOffset * 2) * grid.GetLength(1)) / 2) + gridOffset;

        for(int x = 0; x < grid.GetLength(0); x++)
            for(int y = 0; y < grid.GetLength(1); y++)
                if(grid[x, y] == 1)
                {
                    float xPos = startXPos + x * gridOffset * 2;
                    float zPos = startYPos + y * gridOffset * 2;
        
                    GameObject.Instantiate(housePrefab, new Vector3(xPos, 0, zPos), new Quaternion()).AddComponent<MeshCollider>();
                }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.green;

        float xPos = (((gridOffset * 2) * grid.GetLength(0)) / 2);
        float yPos = (((gridOffset * 2) * grid.GetLength(1)) / 2);

        for(int x = 0; x < grid.GetLength(0) + 1; x++)
            Gizmos.DrawLine(new Vector3(-xPos, 0, -xPos + x * gridOffset * 2), new Vector3(xPos, 0, -xPos + x * gridOffset * 2));
        for(int y = 0; y < grid.GetLength(1) + 1; y++)
            Gizmos.DrawLine(new Vector3(-yPos + y * gridOffset * 2, 0, -yPos), new Vector3(-yPos + y * gridOffset * 2, 0, yPos));
    }
}
