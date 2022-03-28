using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSizeFix : MonoBehaviour
{

    float blockSize = 7f;
    float blockSizeVariation = 2f;
    float roadSize = 3f;

    Block block;
    Vector2Int pos;

    // Start is called before the first frame update
    void Start()
    {
        pos = new Vector2Int(3, 5);
        Vector2 topLeft = new Vector2(-blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation));
        Vector2 topRight = new Vector2(blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation));
        Vector2 bottomLeft = new Vector2(-blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), -blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation));
        Vector2 bottomRight = new Vector2(blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), -blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation));

        topLeft = RoadSpace(bottomLeft, topLeft, topRight);

        block = new Block(topLeft, topRight, bottomLeft, bottomRight);
    }

    Vector2 RoadSpace(Vector2 left, Vector2 center, Vector2 right)
    {

        Vector2 left1 = new Vector2(left.x - center.x, left.y - center.y);
        Vector2 center1 = new Vector2(0, 0);
        Vector2 right1 = new Vector2(right.x - center.x, right.y - center.y);

        float angle = Mathf.Atan2(right1.y, right1.x) - Mathf.Atan2(left1.y, left1.x);

        

        float otherLeg = (roadSize / 2) * Mathf.Tan((Mathf.PI/2) - (angle / 2));
        float hypothenuse = Mathf.Sqrt(Mathf.Pow(roadSize / 2, 2) + Mathf.Pow(otherLeg, 2));

        Vector2 newPoint = new Vector2(Mathf.Cos(angle / 2), Mathf.Sin(angle / 2)) * hypothenuse;
        newPoint = new Vector2(center.x - newPoint.x, center.y - newPoint.y);

        print(newPoint);

        return newPoint;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(block.topLeft + new Vector2(pos.x, pos.y) * blockSize, block.topRight + new Vector2(pos.x, pos.y) * blockSize);
        Gizmos.DrawLine(block.topRight + new Vector2(pos.x, pos.y) * blockSize, block.bottomRight + new Vector2(pos.x, pos.y) * blockSize);
        Gizmos.DrawLine(block.bottomRight + new Vector2(pos.x, pos.y) * blockSize, block.bottomLeft + new Vector2(pos.x, pos.y) * blockSize);
        Gizmos.DrawLine(block.bottomLeft + new Vector2(pos.x, pos.y) * blockSize, block.topLeft + new Vector2(pos.x, pos.y) * blockSize);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
