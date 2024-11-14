using System;
using UnityEngine;

[Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;

    public Position(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static string Serialize(Position pos)
    {
        return JsonUtility.ToJson(pos);
    }

    public static Position Deserialize(string json)
    {
        return JsonUtility.FromJson<Position>(json);
    }
}
