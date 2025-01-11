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

    // Método para iniciar la escena de unión como cliente
    public void JoinClient()
    {
        Debug.Log("Ir a la escena de unión del cliente.");
        SceneManager.LoadScene(joinServer);
    }

    // Método para iniciar la escena de creación de servidor
    public void CreateServer()
    {
        Debug.Log("Ir a la escena de creación de servidor.");
        SceneManager.LoadScene(createServer);
    }

    // Método para configurar la escena de juego como cliente
    public void SceneClient()
    {
        isClient = true;
        Debug.Log("Configurando escena de juego como cliente.");
        SceneManager.LoadScene(sceneClient);
    }

    // Método para configurar la escena de juego como servidor
    public void SceneServer()
    {
        isClient = false;
        Debug.Log("Configurando escena de juego como servidor.");
        SceneManager.LoadScene(sceneServer);
    }

    // Método para volver al menú principal
    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenu);
    }

    // Método para salir del juego
    public void EndGame()
    {
        Application.Quit();
    }

    // Método para instanciar el prefab correcto en la escena de juego
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
