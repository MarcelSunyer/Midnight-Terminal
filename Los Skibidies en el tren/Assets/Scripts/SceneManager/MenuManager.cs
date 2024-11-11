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

    public void JoinClient()
    {
        SceneManager.LoadScene(createClient);
    }
    public void JoinServer()
    {
        SceneManager.LoadScene(joinServer);
    }
    public void SceneClient()
    {
        SceneManager.LoadScene(sceneClient);
    }
    public void SceneServer()
    {
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
}
