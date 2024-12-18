using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame_Button : MonoBehaviour, IInteractable
{
    // Referencias a los prefabs que queremos teletransportar
    public GameObject prefab1;
    public GameObject prefab2;

    // Nombre de la escena de destino
    public string targetScene;

    // Posición de teletransporte
    public Vector3 teleportPosition = new Vector3(-3, 3, 0);


    public event Action OnSceneLoaded;

    public void Interact()
    {
        Debug.Log("Game will start soon!");

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("No se ha especificado la escena de destino.");
            return;
        }
        
        TeleportPrefabs(prefab1);
        TeleportPrefabs(prefab2);

        OnSceneLoaded?.Invoke(); // Notifica al servidor que la escena se ha cargado
        // Carga la nueva escena
        SceneManager.LoadScene(targetScene);
    }

    private void TeleportPrefabs(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Uno de los prefabs no está asignado en el inspector.");
            return;
        }

        // Encuentra todas las instancias de este prefab
        GameObject[] instances = GameObject.FindGameObjectsWithTag(prefab.tag);

        foreach (GameObject instance in instances)
        {
            Debug.Log($"Teleporting {instance.name} to the new scene.");
            instance.transform.position = teleportPosition; // Cambia la posición de la instancia
            DontDestroyOnLoad(instance); // Evita que se destruya al cargar la nueva escena
        }
    }
}

