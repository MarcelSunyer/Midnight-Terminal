using System;
using UnityEngine;

[Serializable]
public class Position
{
    public float x, y, z;      // Posición
    public float rotX, rotY, rotZ, rotW; // Rotación (Quaternion)

    public Position(float x, float y, float z, Quaternion rotation)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.rotX = rotation.x;
        this.rotY = rotation.y;
        this.rotZ = rotation.z;
        this.rotW = rotation.w;
    }

    public static string Serialize(Position pos)
    {
        return JsonUtility.ToJson(pos);
    }

    public static Position Deserialize(string json)
    {
        return JsonUtility.FromJson<Position>(json);
    }
    public static bool TryDeserialize(string json, out Position position)
    {
        try
        {
            position = JsonUtility.FromJson<Position>(json);
            return true;
        }
        catch
        {
            position = null;
            return false;
        }
    }
}
