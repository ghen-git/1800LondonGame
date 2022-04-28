using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using SerializableTypes;

[Serializable]
public class LondonData
{
    public SVector2Int[] ids;
    public SBlock[] blocks;
    public int seed;

    public LondonData(SVector2Int[] ids, SBlock[] blocks, int seed)
    {
        this.ids = ids;
        this.blocks = blocks;
        this.seed = seed;
    }
}

public class LondonSerializer : MonoBehaviour
{
    string gameName;
    int seed;

    public Dictionary<Vector2Int, Block> savedMap;

    public int Init(string gameName)
    {
        this.gameName = gameName;

        if (File.Exists(Application.persistentDataPath + $"/{gameName}-level.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + $"/{gameName}-level.dat", FileMode.Open);
            LondonData savedData = (LondonData)bf.Deserialize(file);
            file.Close();

            seed = savedData.seed;
            savedMap = SHelper.MixVectors<Vector2Int, Block>(SVector2Int.ToArray(savedData.ids), SBlock.ToArray(savedData.blocks));
        }
        else
        {
            seed = System.DateTime.Now.GetHashCode();
            savedMap = new Dictionary<Vector2Int, Block>();
        }

        return seed;
    }

    void OnApplicationQuit()
    {
        Vector2Int[] idsArray = GetIDsArray(savedMap);
        LondonData savedData = new LondonData(SVector2Int.FromDictionary<Block>(savedMap), SBlock.FromDictionary<Vector2Int>(savedMap), seed);

        BinaryFormatter bf = new BinaryFormatter(); 
        FileStream file = File.Create(Application.persistentDataPath + $"/{gameName}-level.dat"); 
        bf.Serialize(file, savedData);
        file.Close();
    }

    Vector2Int[] GetIDsArray(Dictionary<Vector2Int, Block> data)
    {
        Vector2Int[] idsArray = new Vector2Int[data.Keys.Count];
        int i = 0;

        foreach(KeyValuePair<Vector2Int, Block> value in data)
        {
            idsArray[i] = value.Key;
            i++;
        }

        return idsArray;
    }
}
