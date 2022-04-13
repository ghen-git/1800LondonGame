using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;

public static class GraphicsUtil
{
    public static Vector2[] GetRelativeVertices(Vector2[] vertices, Vector2 relativeTo, Vector2 center)
    {
        return new Vector2[4]
        {
            vertices[0] + relativeTo - center,
            vertices[1] + relativeTo - center,
            vertices[2] + relativeTo - center,
            vertices[3] + relativeTo - center
        };
    }

    public static Vector2 GetGlobalCenter(Vector2[] vertices, Vector2 relativeTo)
    {
        return QuadCenter
        (
            vertices[0] + relativeTo, 
            vertices[1] + relativeTo, 
            vertices[2] + relativeTo, 
            vertices[3] + relativeTo
        );
    }

    //3d quad rendering
    public static GameObject RenderQuad(Vector2[] baseVertices, Vector2 pos, float height, string name, Material material, float uvScale, bool[] faces = null)
    {
        GameObject quad = new GameObject();

        MeshRenderer meshRenderer = quad.AddComponent<MeshRenderer>();

        meshRenderer.material = material;

        MeshFilter meshFilter = quad.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        
        /*
        faces rendering:

        0: left
        1: front (positive z)
        2: right
        3: back (negative z)
        4: top
        5: bottom

        */

        int vertexCount = 0;

        if(faces == null)
            faces = new bool[6]{true, true, true, true, true, true};

        foreach(bool face in faces)
            if(face)
                vertexCount += 4;

        //vertices and triangles definition
        int vertexLoadingOffset = 0;
        int trisLoadingOffset = 0;
        
        Vector3[] vertices = new Vector3[vertexCount];
        int[] tris = new int[(vertexCount / 2) * 3];
        Vector2[] uvs = new Vector2[vertexCount];

        //left
        if(faces[0])
        {   
            vertices[vertexLoadingOffset] = new Vector3(baseVertices[0].x, height, baseVertices[0].y); //top left 0
            vertices[vertexLoadingOffset + 1] = new Vector3(baseVertices[2].x, height, baseVertices[2].y); //top right 1 
            vertices[vertexLoadingOffset + 2] = new Vector3(baseVertices[0].x, 0, baseVertices[0].y); //bottom left 2 
            vertices[vertexLoadingOffset + 3] = new Vector3(baseVertices[2].x, 0, baseVertices[2].y);  //bottom right 3

            tris[trisLoadingOffset] = vertexLoadingOffset + 2;
            tris[trisLoadingOffset + 1] = vertexLoadingOffset;
            tris[trisLoadingOffset + 2] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 3] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 4] = vertexLoadingOffset;
            tris[trisLoadingOffset + 5] = vertexLoadingOffset + 1;

            uvs = CalculateWallUVs(baseVertices[0], baseVertices[2], height, uvScale, uvs, vertexLoadingOffset);
            
            vertexLoadingOffset += 4;
            trisLoadingOffset += 6;
        }

        //front
        if(faces[1])
        {   
            vertices[vertexLoadingOffset] = new Vector3(baseVertices[1].x, height, baseVertices[1].y); //top left 0
            vertices[vertexLoadingOffset + 1] = new Vector3(baseVertices[0].x, height, baseVertices[0].y); //top right 1 
            vertices[vertexLoadingOffset + 2] = new Vector3(baseVertices[1].x, 0, baseVertices[1].y); //bottom left 2 
            vertices[vertexLoadingOffset + 3] = new Vector3(baseVertices[0].x, 0, baseVertices[0].y);  //bottom right 3

            tris[trisLoadingOffset] = vertexLoadingOffset + 2;
            tris[trisLoadingOffset + 1] = vertexLoadingOffset;
            tris[trisLoadingOffset + 2] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 3] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 4] = vertexLoadingOffset;
            tris[trisLoadingOffset + 5] = vertexLoadingOffset + 1;

            uvs = CalculateWallUVs(baseVertices[1], baseVertices[0], height, uvScale, uvs, vertexLoadingOffset);
            
            vertexLoadingOffset += 4;
            trisLoadingOffset += 6;
        }

        //right
        if(faces[2])
        {   
            vertices[vertexLoadingOffset] = new Vector3(baseVertices[3].x, height, baseVertices[3].y); //top left 0
            vertices[vertexLoadingOffset + 1] = new Vector3(baseVertices[1].x, height, baseVertices[1].y); //top right 1 
            vertices[vertexLoadingOffset + 2] = new Vector3(baseVertices[3].x, 0, baseVertices[3].y); //bottom left 2 
            vertices[vertexLoadingOffset + 3] = new Vector3(baseVertices[1].x, 0, baseVertices[1].y);  //bottom right 3

            tris[trisLoadingOffset] = vertexLoadingOffset + 2;
            tris[trisLoadingOffset + 1] = vertexLoadingOffset;
            tris[trisLoadingOffset + 2] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 3] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 4] = vertexLoadingOffset;
            tris[trisLoadingOffset + 5] = vertexLoadingOffset + 1;

            uvs = CalculateWallUVs(baseVertices[3], baseVertices[1], height, uvScale, uvs, vertexLoadingOffset);
            
            vertexLoadingOffset += 4;
            trisLoadingOffset += 6;
        }

        //back
        if(faces[3])
        {   
            vertices[vertexLoadingOffset] = new Vector3(baseVertices[2].x, height, baseVertices[2].y); //top left 0
            vertices[vertexLoadingOffset + 1] = new Vector3(baseVertices[3].x, height, baseVertices[3].y); //top right 1 
            vertices[vertexLoadingOffset + 2] = new Vector3(baseVertices[2].x, 0, baseVertices[2].y); //bottom left 2 
            vertices[vertexLoadingOffset + 3] = new Vector3(baseVertices[3].x, 0, baseVertices[3].y);  //bottom right 3

            tris[trisLoadingOffset] = vertexLoadingOffset + 2;
            tris[trisLoadingOffset + 1] = vertexLoadingOffset;
            tris[trisLoadingOffset + 2] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 3] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 4] = vertexLoadingOffset;
            tris[trisLoadingOffset + 5] = vertexLoadingOffset + 1;
            
            uvs = CalculateWallUVs(baseVertices[2], baseVertices[3], height, uvScale, uvs, vertexLoadingOffset);
            
            vertexLoadingOffset += 4;
            trisLoadingOffset += 6;
        }

        //top
        if(faces[4])
        {   
            vertices[vertexLoadingOffset] = new Vector3(baseVertices[0].x, height, baseVertices[0].y); //top left 0
            vertices[vertexLoadingOffset + 1] = new Vector3(baseVertices[1].x, height, baseVertices[1].y); //top right 1 
            vertices[vertexLoadingOffset + 2] = new Vector3(baseVertices[2].x, height, baseVertices[2].y); //bottom left 2 
            vertices[vertexLoadingOffset + 3] = new Vector3(baseVertices[3].x, height, baseVertices[3].y);  //bottom right 3

            tris[trisLoadingOffset] = vertexLoadingOffset + 2;
            tris[trisLoadingOffset + 1] = vertexLoadingOffset;
            tris[trisLoadingOffset + 2] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 3] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 4] = vertexLoadingOffset;
            tris[trisLoadingOffset + 5] = vertexLoadingOffset + 1;
            
            uvs = CalculateFaceUVs(baseVertices, uvScale, uvs, vertexLoadingOffset);
            
            vertexLoadingOffset += 4;
            trisLoadingOffset += 6;
        }

        //bottom
        if(faces[5])
        {   
            vertices[vertexLoadingOffset] = new Vector3(baseVertices[2].x, 0, baseVertices[2].y); //top left 0
            vertices[vertexLoadingOffset + 1] = new Vector3(baseVertices[3].x, 0, baseVertices[3].y); //top right 1 
            vertices[vertexLoadingOffset + 2] = new Vector3(baseVertices[0].x, 0, baseVertices[0].y); //bottom left 2 
            vertices[vertexLoadingOffset + 3] = new Vector3(baseVertices[1].x, 0, baseVertices[1].y);  //bottom right 3

            tris[trisLoadingOffset] = vertexLoadingOffset + 2;
            tris[trisLoadingOffset + 1] = vertexLoadingOffset;
            tris[trisLoadingOffset + 2] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 3] = vertexLoadingOffset + 3;
            tris[trisLoadingOffset + 4] = vertexLoadingOffset;
            tris[trisLoadingOffset + 5] = vertexLoadingOffset + 1;
            
            uvs = CalculateFaceUVs(baseVertices, uvScale, uvs, vertexLoadingOffset);
            
            vertexLoadingOffset += 4;
            trisLoadingOffset += 6;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uvs;

        meshFilter.mesh = mesh;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        quad.transform.position = new Vector3(pos.x, 0, pos.y);
        quad.name = name;
        quad.AddComponent<MeshCollider>();

        return quad;
    }

    static Vector2[] CalculateWallUVs(Vector2 left, Vector2 right, float height, float uvScale, Vector2[] uvs, int arrayOffset = 0)
    {
        float width = Vector2.Distance(left, right);
        
        uvs[arrayOffset] = new Vector2(-width / 2, -height / 2) * uvScale;
        uvs[arrayOffset + 1] = new Vector2(width / 2, -height / 2) * uvScale;
        uvs[arrayOffset + 2] = new Vector2(-width / 2, height / 2) * uvScale;
        uvs[arrayOffset + 3] = new Vector2(width / 2, height / 2) * uvScale;

        return uvs;
    }

    static Vector2[] CalculateFaceUVs(Vector2[] vertices, float uvScale, Vector2[] uvs, int arrayOffset = 0)
    {
        float width = Vector2.Distance(vertices[0], vertices[1]);
        float height = Vector2.Distance(vertices[0], vertices[2]);
        
        uvs[arrayOffset] = new Vector2(-width / 2, -height / 2) * uvScale;
        uvs[arrayOffset + 1] = new Vector2(width / 2, -height / 2) * uvScale;
        uvs[arrayOffset + 2] = new Vector2(-width / 2, height / 2) * uvScale;
        uvs[arrayOffset + 3] = new Vector2(width / 2, height / 2) * uvScale;

        return uvs;
    }

    //plane rendering
    public static GameObject RenderQuad(Vector2[] vertxs, Vector2 pos, string name, Material material, float uvScale)
    {
        GameObject quad = new GameObject();

        MeshRenderer meshRenderer = quad.AddComponent<MeshRenderer>();

        meshRenderer.material = material;

        MeshFilter meshFilter = quad.AddComponent<MeshFilter>();

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
        
        mesh.uv = CalculatePlaneUVs(vertxs, uvScale);

        meshFilter.mesh = mesh;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        quad.transform.position = new Vector3(pos.x, 0, pos.y);
        quad.name = name;
        quad.AddComponent<MeshCollider>();

        return quad;
    }

    static Vector2[] CalculatePlaneUVs(Vector2[] vertices, float uvScale)
    {
        Vector2[] uvs = new Vector2[4];

        float width = Vector2.Distance(vertices[0], vertices[1]);
        float height = Vector2.Distance(vertices[0], vertices[2]);
        
        uvs[0] = new Vector2(-width / 2, -height / 2) * uvScale;
        uvs[1] = new Vector2(width / 2, -height / 2) * uvScale;
        uvs[2] = new Vector2(-width / 2, height / 2) * uvScale;
        uvs[3] = new Vector2(width / 2, height / 2) * uvScale;

        return uvs;
    }
}
