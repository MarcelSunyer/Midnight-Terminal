using System;
using UnityEngine;

public class Position
{
    public float x, y, z;             // Posición
    public float rotX, rotY, rotZ, rotW; // Rotación (Quaternion)
    public string id;                 // Identificador opcional del cliente

    public Position(float x, float y, float z, Quaternion rotation, string id = null)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.rotX = rotation.x;
        this.rotY = rotation.y;
        this.rotZ = rotation.z;
        this.rotW = rotation.w;
        this.id = id;
    }

    /// <summary>
    /// Serializa un objeto Position a un string JSON.
    /// </summary>
    /// <param name="pos">El objeto Position a serializar.</param>
    /// <returns>Una cadena JSON que representa el objeto.</returns>
    public static string Serialize(Position pos)
    {
        return JsonUtility.ToJson(pos);
    }

    /// <summary>
    /// Deserializa una cadena JSON a un objeto Position.
    /// </summary>
    /// <param name="json">La cadena JSON que representa un objeto Position.</param>
    /// <returns>El objeto Position deserializado.</returns>
    public static Position Deserialize(string json)
    {
        return JsonUtility.FromJson<Position>(json);
    }

    /// <summary>
    /// Intenta deserializar una cadena JSON a un objeto Position, manejando errores.
    /// </summary>
    /// <param name="json">La cadena JSON que representa un objeto Position.</param>
    /// <param name="position">El objeto Position deserializado (si tiene éxito).</param>
    /// <returns>True si la deserialización tuvo éxito, false en caso contrario.</returns>
    public static bool TryDeserialize(string json, out Position position)
    {
        try
        {
            position = JsonUtility.FromJson<Position>(json);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deserializando Position: {ex.Message}");
            position = null;
            return false;
        }
    }

    /// <summary>
    /// Crea un Quaternion a partir de los datos de rotación.
    /// </summary>
    /// <returns>Un objeto Quaternion basado en los valores de rotación.</returns>
    public Quaternion GetRotation()
    {
        return new Quaternion(rotX, rotY, rotZ, rotW);
    }

    /// <summary>
    /// Crea un Vector3 a partir de los datos de posición.
    /// </summary>
    /// <returns>Un objeto Vector3 basado en los valores de posición.</returns>
    public Vector3 GetPosition()
    {
        return new Vector3(x, y, z);
    }
}
