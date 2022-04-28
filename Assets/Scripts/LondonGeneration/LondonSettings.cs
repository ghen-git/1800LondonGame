using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LondonSettings
{
    #region General

    public const int renderDistance = 4;
    public static Vector2Int perlinOffset;

    #endregion

    #region Blocks

    public const float blockSize =200f;
    public const float blockSizeVariation = 200f / 4;
    public const string groundMat = "Ground/SecondaryRoad";
    public const float perlinFrequency = 10.0f;

    #endregion
    
    #region Roads

    public const float roadSize = 25f;
    public const float roadSizeVariation = 15f;
    public const float sidewalkSize = 3f;
    public const float sidewalkHeight = 0.25f;
    public const string westEndRoadMat = "Ground/Road";
    public const string eastEndRoadMat = "Ground/SecondaryRoad";
    public const string sidewalkMat = "Ground/Sidewalk";
    public const float sidewalkMatScale = 0.1f;
    public const float offsetTolerance = 100f;
    public const float fencePostOffset = 5.5f;
    public const float fencePostDistance = 40f;

    #endregion

    #region Buildings

    //floors
    public const float floorHeight = 5f;
    public const int minFloors = 2;
    public const int maxFloors = 5;

    //dimentions
    public const int minBuildWidth = 1;
    public const int maxBuildWidth = 3;
    public const int minBuildInset = 0;
    public const int maxBuildInset = 2;
    public const float secondaryRoadSize = 3f;

    //scales
    public const float buildingScale = 10f;
    public const float insetScale = 2f;

    //chances
    public const int startWithHouseChance = 50;
    public const int secondaryRoadChance = 30;
    public const int identicalBuildingsSegmentChance = 30;
    public const int impasseChance = 30;

    //identical buildings segment
    public const int identicalBuildingsMinAmount = 3;
    public const int identicalBuildingsMaxAmount = 5;

    //materials
    public static string[] westEndWallMats = new string[] { "Walls/WornRedBricks", "Walls/BlackBricks", "Walls/BeigeBricks" };
    public static string[] eastEndWallMats = new string[] { "Walls/WornRedBricks", "Walls/BrownBricks", "Walls/Cobblestone", "Walls/WoodenWall" };

    #endregion
}
