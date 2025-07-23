using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameDataReader
{
    public int Version { get; }
    public BinaryReader reader;

    public GameDataReader(BinaryReader reader , int version)
    {
        this.reader = reader;
        this.Version = version;
    }

    public float ReadFloat()
    {
        return reader.ReadSingle();
    }
    
    public int ReadInt()
    {
        return reader.ReadInt32();
    }

    public Quaternion ReadQuaternion()
    {
        Quaternion value;
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        value.w = reader.ReadSingle();
        return value;
    }
    public Vector3 ReadVector3()
    {
        Vector3 value;
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        return value;
    }

    public Vector2Int ReadVector2Int()
    {
        Vector2Int value = new Vector2Int();
        value.x = ReadInt();
        value.y = ReadInt();
        return value;
    }

    public Color ReadColor()
    {
        Color color;
        color.r = reader.ReadSingle();
        color.g = reader.ReadSingle();
        color.b = reader.ReadSingle();
        color.a = reader.ReadSingle();
        return color;
    }

    public Random.State ReadRandomState()
    {
        //return JsonUtility.FromJson(reader.ReadString());
        return JsonUtility.FromJson<Random.State>(reader.ReadString());
    }

}
