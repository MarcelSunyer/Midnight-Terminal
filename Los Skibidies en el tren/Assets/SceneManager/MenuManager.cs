using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string createClient = "LoadServer";
    [SerializeField] private string joinServer = "JoinServer";
    [SerializeField] private string sceneClient = "Exercise1_Client";
    [SerializeField] private string sceneServer = "Exercise1_Server";

    public void CreateClient()
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
    public void EndGame()
    {
        Application.Quit();
    }
}
