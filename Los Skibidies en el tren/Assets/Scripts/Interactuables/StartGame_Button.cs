using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame_Button : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Game will start soon!");
    }
}
