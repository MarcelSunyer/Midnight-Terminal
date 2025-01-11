using System;
using UnityEngine;
using UnityEngine.UI;

public class Can_Join_On_The_Game : MonoBehaviour
{
    public ConnectionValidator _connectionValidator;

    private bool hasJoined = false; // Para evitar llamar a SceneClient más de una vez

    private void Update()
    {
        // Si se puede unirse al juego y no se ha ejecutado SceneClient antes
        if (_connectionValidator.can_join && !hasJoined)
        {
            Debug.Log("Joining the game...");
            MenuManager.Instance.SceneClient();
            hasJoined = true; // Evitar ejecutar de nuevo
        }
        else if (!_connectionValidator.can_join)
        {
            
        }
    }
}