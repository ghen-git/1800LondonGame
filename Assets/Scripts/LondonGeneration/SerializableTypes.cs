using UnityEngine;
using System.Collections.Generic;

namespace SerializableTypes
{
    [System.Serializable]
    public class SVector2
    {
        public float x;
        public float y;

        public SVector2(Vector2 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
        }
        
        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        public static SVector2[] FromArray(Vector2[] vArray)
        {
            SVector2[] array = new SVector2[vArray.Length];

            for(int i = 0; i < vArray.Length; i++)
                array[i] = new SVector2(vArray[i]);

            return array;
        }

        public static Vector2[] ToArray(SVector2[] sArray)
        {
            Vector2[] array = new Vector2[sArray.Length];

            for(int i = 0; i < sArray.Length; i++)
                array[i] = sArray[i].ToVector2();

            return array;
        }
    }

    [System.Serializable]
    public class SVector2Int
    {
        public int x;
        public int y;

        public SVector2Int(Vector2Int vector)
        {
            this.x = vector.x;
            this.y = vector.y;
        }
        
        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }

        public static SVector2Int[] FromDictionary<T>(Dictionary<Vector2Int, T> dictionary)
        {
            SVector2Int[] array = new SVector2Int[dictionary.Keys.Count];
            int i = 0;

            foreach(KeyValuePair<Vector2Int, T> value in dictionary)
            {
                array[i] = new SVector2Int(value.Key);
                i++;
            }

            return array;
        }

        public static Vector2Int[] ToArray(SVector2Int[] sArray)
        {
            Vector2Int[] array = new Vector2Int[sArray.Length];

            for(int i = 0; i < sArray.Length; i++)
                array[i] = sArray[i].ToVector2Int();

            return array;
        }
    }

    [System.Serializable]
    public class SLine
    {
        public float m;
        public float q;

        public SLine(Line line)
        {
            this.m = line.m;
            this.q = line.q;
        }

        public Line ToLine()
        {
            return new Line(m, q);
        }
    }

    [System.Serializable]
    public class SBlock
    {
        public SVector2 topLeft;
        public SVector2 topRight;
        public SVector2 bottomLeft;
        public SVector2 bottomRight;
        public SVector2 center;
        public bool direction;
        //false: horizontal
        //true: vertical
        public string end;
        public SLine leftEdge;
        public SLine topEdge;
        public SLine rightEdge;
        public SLine bottomEdge;
        public SVector2[] leftSidewalkPoints;
        public SVector2[] topSidewalkPoints;
        public SVector2[] rightSidewalkPoints;
        public SVector2[] bottomSidewalkPoints;

        public SVector2Int[] buildingsIds;
        public SBuilding[] buildings;

        public SBlock(Block block)
        {
            topLeft = new SVector2(block.topLeft);
            topRight = new SVector2(block.topRight);
            bottomLeft = new SVector2(block.bottomLeft);
            bottomRight = new SVector2(block.bottomRight);
            center = new SVector2(block.center);
            direction = block.direction;
            end = block.end;
            leftEdge = new SLine(block.leftEdge);
            topEdge = new SLine(block.topEdge);
            rightEdge =  new SLine(block.rightEdge);
            bottomEdge =  new SLine(block.bottomEdge);
            leftSidewalkPoints = SVector2.FromArray(block.leftSidewalkPoints.ToArray());
            topSidewalkPoints = SVector2.FromArray(block.topSidewalkPoints.ToArray());
            rightSidewalkPoints = SVector2.FromArray(block.rightSidewalkPoints.ToArray());
            bottomSidewalkPoints = SVector2.FromArray(block.bottomSidewalkPoints.ToArray());
            buildingsIds = SVector2Int.FromDictionary<Building>(block.buildings);
            buildings = SBuilding.FromDictionary<Vector2Int>(block.buildings);
        }

        public static SBlock[] FromDictionary<T>(Dictionary<T, Block> dictionary)
        {
            SBlock[] array = new SBlock[dictionary.Keys.Count];
            int i = 0;

            foreach(KeyValuePair<T, Block> value in dictionary)
            {
                array[i] = new SBlock(value.Value);
                i++;
            }

            return array;
        }

        public Block ToBlock()
        {
            Vector2 topLeft = this.topLeft.ToVector2();
            Vector2 topRight = this.topRight.ToVector2();
            Vector2 bottomLeft = this.bottomLeft.ToVector2();
            Vector2 bottomRight = this.bottomRight.ToVector2();
            Vector2 center = this.center.ToVector2();
            bool direction = this.direction;
            string end = this.end;
            Line leftEdge = this.leftEdge.ToLine();
            Line topEdge = this.topEdge.ToLine();
            Line rightEdge =  this.rightEdge.ToLine();
            Line bottomEdge =  this.bottomEdge.ToLine();
            List<Vector2> leftSidewalkPoints = new List<Vector2>(SVector2.ToArray(this.leftSidewalkPoints));
            List<Vector2> topSidewalkPoints = new List<Vector2>(SVector2.ToArray(this.topSidewalkPoints));
            List<Vector2> rightSidewalkPoints = new List<Vector2>(SVector2.ToArray(this.rightSidewalkPoints));
            List<Vector2> bottomSidewalkPoints = new List<Vector2>(SVector2.ToArray(this.bottomSidewalkPoints));
            Dictionary<Vector2Int, Building> buildings = SHelper.MixVectors<Vector2Int, Building>(SVector2Int.ToArray(buildingsIds), SBuilding.ToArray(this.buildings));

            Block block = new Block(topLeft, topRight, bottomLeft, bottomRight);
            block.center = center;
            block.direction = direction;
            block.end = end;
            block.leftEdge = leftEdge;
            block.topEdge = topEdge;
            block.rightEdge = rightEdge;
            block.bottomEdge = bottomEdge;
            block.leftSidewalkPoints = leftSidewalkPoints;
            block.topSidewalkPoints = topSidewalkPoints;
            block.rightSidewalkPoints = rightSidewalkPoints;
            block.bottomSidewalkPoints = bottomSidewalkPoints;
            block.buildings = buildings;
            
            return block;
        }

        public static Block[] ToArray(SBlock[] sArray)
        {
            Block[] array = new Block[sArray.Length];

            for(int i = 0; i < sArray.Length; i++)
                array[i] = sArray[i].ToBlock();

            return array;
        }
    }

    public class SHelper
    {
        public static Dictionary<T1, T2> MixVectors<T1, T2>(T1[] ids, T2[] values)
        {
            Dictionary<T1, T2> dictionary = new Dictionary<T1, T2>();

            for(int i = 0; i < ids.Length; i++)
                dictionary.Add(ids[i], values[i]);

            return dictionary;
        }
    }


    [System.Serializable]
    public class SBuilding
    {
        public SVector2 topLeftCorner;
        public SVector2 topRightCorner;
        public SVector2 bottomLeftCorner;
        public SVector2 bottomRightCorner;
        public int floorNumber;
        public string wallMaterial;

        public SBuilding(Building building)
        {
            topLeftCorner = new SVector2(building.topLeftCorner);
            topRightCorner = new SVector2(building.topRightCorner);
            bottomLeftCorner = new SVector2(building.bottomLeftCorner);
            bottomRightCorner = new SVector2(building.bottomRightCorner);
            floorNumber = building.floorsNumber;
            wallMaterial = building.wallMaterial;
        }

        public Building ToBuilding()
        {
            Building building = new Building(topLeftCorner.ToVector2(), topRightCorner.ToVector2(), bottomLeftCorner.ToVector2(), bottomRightCorner.ToVector2());
            building.floorsNumber = floorNumber;
            building.wallMaterial = wallMaterial;
            return building;
        }

        public static SBuilding[] FromDictionary<T>(Dictionary<T, Building> dictionary)
        {
            SBuilding[] array = new SBuilding[dictionary.Keys.Count];
            int i = 0;

            foreach(KeyValuePair<T, Building> value in dictionary)
            {
                array[i] = new SBuilding(value.Value);
                i++;
            }

            return array;
        }

        public static Building[] ToArray(SBuilding[] sArray)
        {
            Building[] array = new Building[sArray.Length];

            for(int i = 0; i < sArray.Length; i++)
                array[i] = sArray[i].ToBuilding();

            return array;
        }
    }
}