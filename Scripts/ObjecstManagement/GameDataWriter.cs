using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameDataWriter
{
    public BinaryWriter writer;

    public GameDataWriter(BinaryWriter writer)
    {
        this.writer = writer;
    }
    public void Write(float value)
    {
        writer.Write(value);
    }

    public void Write(int value)
    {
        writer.Write(value);
    }

    public void Write(Quaternion value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }

    public void Write(Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }

    public void Write(Vector2Int value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
    }

    public void Write(Color color)
    {
        writer.Write(color.r);
        writer.Write(color.g);
        writer.Write(color.b);
        writer.Write(color.a);
    }

    public void Write(Random.State value)
    {
        //打印信息看JSON字符串
        Debug.Log("Saved Randomness : " + JsonUtility.ToJson(value));
        //写入字符串
        writer.Write(JsonUtility.ToJson(value));
    }
}