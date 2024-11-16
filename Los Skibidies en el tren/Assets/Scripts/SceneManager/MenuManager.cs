using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string createServer = "LoadServer";
    [SerializeField] private string joinServer = "JoinServer";
    [SerializeField] private string sceneClient = "Game_Scene";
    [SerializeField] private string sceneServer = "Game_Scene";
    [SerializeField] private string mainMenu = "Main Menu";

    // Prefabs para cliente y servidor
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private GameObject serverPrefab;

    bool isClient;

    private void Awake()
    {
        Debug.Log("---------------- Suscribirse al evento de cambio de escena ----------------");
        SceneManager.sceneLoaded += OnSceneLoaded;
        //Debug.Log("Prefab de cliente inicial asignado: " + (clientPrefab != null ? clientPrefab.name : "null"));
        //Debug.Log("Prefab de servidor inicial asignado: " + (serverPrefab != null ? serverPrefab.name : "null"));
    }

    private void OnDestroy()
    {
        // Desuscribirse al evento cuando el objeto se destruya
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void JoinClient()
    {        
        Debug.Log("JoinClient -> isClient:" + isClient);
        SceneManager.LoadScene(joinServer);
    }

    public void CreateServer()
    {
        Debug.Log("JoinServer -> isClient:" + isClient);
        SceneManager.LoadScene(createServer);
    }

    public void SceneClient()
    {
        isClient = true;
        Debug.Log("Cambiando a escena de cliente: " + sceneClient);
        Debug.Log("SceneClient -> isClient:" + isClient);
        SceneManager.LoadScene(sceneClient);
    }

    public void SceneServer()
    {
        isClient = false;
        Debug.Log("Cambiando a escena de servidor: " + sceneServer);
        Debug.Log("SceneServer -> isClient:" + isClient);
        SceneManager.LoadScene(sceneServer);
    }


    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenu);
    }

    public void EndGame()
    {
        Application.Quit();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Escena cargada: " + scene.name);

        if (scene.name == "Game_Scene")
        {
            Debug.Log("Instanciando prefab en Game_Scene");

            if (isClient && serverPrefab != null)
            {
                Debug.Log("Prefab de cliente asignado: " + clientPrefab.name);
                Instantiate(clientPrefab);
                Debug.Log("Prefab de cliente instanciado.");
                isClient = false;
            }
            else if (!isClient && clientPrefab != null)
            {
                Debug.Log("Prefab de servidor asignado: " + serverPrefab.name);
                Instantiate(serverPrefab);
                Debug.Log("Prefab de servidor instanciado.");
                isClient = false;
            }
            else
            {
                Debug.LogWarning("Prefab no asignado correctamente.");
            }
        }
    }


}
