using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Options_Button_Manager : MonoBehaviour
{
    [SerializeField] private string lobbyScene = "Game_Scene";
    [SerializeField] private string trainStationSceneName = "TrainStation_Level";
    [SerializeField] private PlayerHandler user;

    private void Start()
    {
        // Find player by name
        GameObject serverUser = GameObject.Find("Server_FirstPersonController(Clone)");
        GameObject clientUser = GameObject.Find("Client_FirstPersonController(Clone)");

        // Store the game object in user variable
        if (serverUser != null)
        {
            user = serverUser.GetComponent<PlayerHandler>();
        }
        else if (clientUser != null)
        {
            user = clientUser.GetComponent<PlayerHandler>();
        }
    }

    public void ReturnLobby()
    {
        // Compare the name of the active scene
        if (SceneManager.GetActiveScene().name == trainStationSceneName)
        {
            SceneManager.LoadScene(lobbyScene);
            user.Respawn();
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("No está cargada la escena que toca");
        }
    }

    public void Respawn()
    {
        // Compare the name of the active scene
        if (SceneManager.GetActiveScene().name == trainStationSceneName)
        {
            user.Respawn();
        }
        else
        {
            Debug.LogError("No está cargada la escena que toca");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
