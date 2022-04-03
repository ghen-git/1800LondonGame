using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSizeFix : MonoBehaviour
{

    float blockSize = 30f;
    float blockSizeVariation = 13f;
    float roadSize = 6f;

    Block block;
    Vector2Int pos;
    List<Vector2> drawPoints = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        pos = new Vector2Int(3, 5);
        Vector2 topLeft = new Vector2(-blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation));
        Vector2 topRight = new Vector2(blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation));
        Vector2 bottomLeft = new Vector2(-blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), -blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation));
        Vector2 bottomRight = new Vector2(blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation), -blockSize / 2 + UnityEngine.Random.Range(-blockSizeVariation, blockSizeVariation));
 
        topLeft = RoadSpace(bottomLeft, topLeft, topRight);
        topRight = RoadSpace(topLeft, topRight, bottomRight);
        bottomRight = RoadSpace(topRight, bottomRight, bottomLeft);
        bottomLeft = RoadSpace(bottomRight, bottomLeft, topLeft);
    }

    Vector2 RoadSpace(Vector2 left, Vector2 center, Vector2 right)
    {

        Vector2 left1 = new Vector2(left.x - center.x, left.y - center.y);
        Vector2 center1 = new Vector2(0, 0);
        Vector2 right1 = new Vector2(right.x - center.x, right.y - center.y);
        
        float angle, angleFromXAxis, finalAngle;

        if(left1.x < right1.x && left1.y < right1.y)
        {
            angle = Mathf.Atan2(right1.y, right1.x) - Mathf.Atan2(left1.y, left1.x);
            angleFromXAxis = Mathf.Atan2(0, 1) - Mathf.Atan2(left1.y, left1.x);
            finalAngle = (angle / 2 - angleFromXAxis);
        }
        else if(left1.x < right1.x && left1.y > right1.y)
        {
            angle = Mathf.Atan2(right1.y, right1.x) - Mathf.Atan2(left1.y, left1.x);
            if(angle < 0)
             angle = Mathf.Atan2(left1.y, left1.x) - Mathf.Atan2(right1.y, right1.x);
            angleFromXAxis = Mathf.Atan2(0, 1) - Mathf.Atan2(right1.y, right1.x);
            finalAngle = -(angleFromXAxis + angle / 2);
        }
        else if(left1.x > right1.x && left1.y > right1.y)
        {
            angle = Mathf.Atan2(right1.y, right1.x) - Mathf.Atan2(left1.y, left1.x);
            if(angle < 0)
            {
                angle = -(Mathf.Atan2(right1.y, right1.x) + Mathf.Atan2(left1.y, left1.x));
            }
            angleFromXAxis = Mathf.Atan2(left1.y, left1.x) - Mathf.Atan2(0, 1);
            finalAngle = (angleFromXAxis + angle / 2);
            print(angleFromXAxis);
            print(angle);
            print(finalAngle);
        }
        else 
        {
            angle = Mathf.Atan2(right1.y, right1.x) - Mathf.Atan2(left1.y, left1.x);
            angleFromXAxis = Mathf.Atan2(right1.y, right1.x) - Mathf.Atan2(0, 1);
            finalAngle = (angleFromXAxis - angle / 2);
        }

        print("angle 1 " + angle);
        print("angle x " + angleFromXAxis);

        float horizLength = (roadSize / 2) * Mathf.Tan((Mathf.PI/2) - (angle / 2));

        //the hypothenuse of the triangle is also the distance the point needs to move
        float hypothenuse = Mathf.Sqrt(Mathf.Pow(roadSize / 2, 2) + Mathf.Pow(horizLength, 2));

        Vector2 newPoint = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle)) * hypothenuse;

        newPoint = new Vector2(center.x + newPoint.x, center.y + newPoint.y);
        drawPoints.Add(newPoint + new Vector2(pos.x, pos.y) * blockSize);
        drawPoints.Add(center + new Vector2(pos.x, pos.y) * blockSize);

        return newPoint;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(block.topLeft + new Vector2(pos.x, pos.y) * blockSize, block.topRight + new Vector2(pos.x, pos.y) * blockSize);
        Gizmos.DrawLine(block.topRight + new Vector2(pos.x, pos.y) * blockSize, block.bottomRight + new Vector2(pos.x, pos.y) * blockSize);
        Gizmos.DrawLine(block.bottomRight + new Vector2(pos.x, pos.y) * blockSize, block.bottomLeft + new Vector2(pos.x, pos.y) * blockSize);
        Gizmos.DrawLine(block.bottomLeft + new Vector2(pos.x, pos.y) * blockSize, block.topLeft + new Vector2(pos.x, pos.y) * blockSize);

        foreach(Vector2 point in drawPoints)
            Gizmos.DrawSphere(point, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
