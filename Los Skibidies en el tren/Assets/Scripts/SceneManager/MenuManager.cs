using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string createClient = "LoadServer";
    [SerializeField] private string joinServer = "JoinServer";
    [SerializeField] private string sceneClient = "Game_Scene";
    [SerializeField] private string sceneServer = "Game_Scene";
    [SerializeField] private string mainMenu = "Main Menu";

    // Prefabs para cliente y servidor
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private GameObject serverPrefab;
    
    bool isClient = false;

    private void Awake()
    {
        Debug.Log("---------------- Suscribirse al evento de cambio de escena ----------------");
        DontDestroyOnLoad(gameObject);  // Mantener el objeto entre escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("Prefab de cliente inicial asignado: " + (clientPrefab != null ? clientPrefab.name : "null"));
        Debug.Log("Prefab de servidor inicial asignado: " + (serverPrefab != null ? serverPrefab.name : "null"));
    }

    private void OnDestroy()
    {
        // Desuscribirse al evento cuando el objeto se destruya
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void JoinClient()
    {
        isClient = true; // Marcar como cliente
        SceneManager.LoadScene(createClient);
    }

    public void JoinServer()
    {
        isClient = false; // Marcar como servidor
        SceneManager.LoadScene(joinServer);
    }

    public void SceneClient()
    {
        isClient = true;
        Debug.Log("Cambiando a escena de cliente: " + sceneClient);
        SceneManager.LoadScene(sceneClient);
    }

    public void SceneServer()
    {
        isClient = false;
        Debug.Log("Cambiando a escena de servidor: " + sceneServer);
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
                Debug.Log("Prefab de cliente asignado: " + serverPrefab.name);
                Instantiate(serverPrefab);
                Debug.Log("Prefab de cliente instanciado.");
            }
            else if (!isClient && clientPrefab != null)
            {
                Debug.Log("Prefab de servidor asignado: " + clientPrefab.name);
                Instantiate(clientPrefab);
                Debug.Log("Prefab de servidor instanciado.");
            }
            else
            {
                Debug.LogWarning("Prefab no asignado correctamente.");
            }
        }
    }


}
