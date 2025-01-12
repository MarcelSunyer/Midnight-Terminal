using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRandomObjects : MonoBehaviour
{
    [Header("Spawn Area")]
    public Vector2 areaSize = new Vector2(10f, 10f); // Tamaño del área en XZ (ancho x profundidad)

    [Header("Spawn Settings")]
    public GameObject objectPrefab; // Prefab del objeto a generar
    public int objectCount = 5; // Cantidad de objetos a generar

    void Start()
    {
        if (objectPrefab == null)
        {
            Debug.LogError("No se ha asignado un prefab para generar.");
            return;
        }

        SpawnObjects();
    }

    void SpawnObjects()
    {
        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = GetRandomPosition();
            GameObject spawnedObject = Instantiate(objectPrefab, randomPosition, Quaternion.identity);

            // Configura el objeto generado como hijo de este GameObject
            spawnedObject.transform.SetParent(transform);
        }
    }

    Vector3 GetRandomPosition()
    {
        // Genera una posición aleatoria dentro del área en XZ, centrada en el padre
        float x = Random.Range(-areaSize.x / 2, areaSize.x / 2); // Movimiento en X
        float z = Random.Range(-areaSize.y / 2, areaSize.y / 2); // Movimiento en Z
        float y = transform.position.y; // Mantener la altura del padre

        // Retorna la posición relativa al padre
        return new Vector3(transform.position.x + x, y, transform.position.z + z);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja el área de spawn en el plano XZ en la vista de Scene
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0f, areaSize.y));
    }
}