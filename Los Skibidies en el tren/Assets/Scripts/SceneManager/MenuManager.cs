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

    private bool isClient;

    public static MenuManager Instance { get; private set; }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // M�todo para iniciar la escena de uni�n como cliente
    public void JoinClient()
    {
        Debug.Log("Ir a la escena de uni�n del cliente.");
        SceneManager.LoadScene(joinServer);
    }

    // M�todo para iniciar la escena de creaci�n de servidor
    public void CreateServer()
    {
        Debug.Log("Ir a la escena de creaci�n de servidor.");
        SceneManager.LoadScene(createServer);
    }

    // M�todo para configurar la escena de juego como cliente
    public void SceneClient()
    {
        isClient = true;
        Debug.Log("Configurando escena de juego como cliente.");
        SceneManager.LoadScene(sceneClient);
    }

    // M�todo para configurar la escena de juego como servidor
    public void SceneServer()
    {
        isClient = false;
        Debug.Log("Configurando escena de juego como servidor.");
        SceneManager.LoadScene(sceneServer);
    }

    // M�todo para volver al men� principal
    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenu);
    }

    // M�todo para salir del juego
    public void EndGame()
    {
        Application.Quit();
    }

    // M�todo para instanciar el prefab correcto en la escena de juego
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Escena cargada: " + scene.name);

        if (scene.name == "Game_Scene")
        {
            Debug.Log("Instanciando el prefab correspondiente en la escena de juego.");

            if (isClient && clientPrefab != null)
            {
                Instantiate(clientPrefab);
                Debug.Log("Prefab de cliente instanciado.");
            }
            else if (!isClient && serverPrefab != null)
            {
                Instantiate(serverPrefab);
                Debug.Log("Prefab de servidor instanciado.");
            }
            else
            {
                Debug.LogWarning("Prefab no asignado correctamente.");
            }
        }
    }
}
