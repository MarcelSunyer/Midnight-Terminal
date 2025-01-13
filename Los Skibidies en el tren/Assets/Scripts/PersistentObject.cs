using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private void Awake()
    {
        // Hace que este objeto no se destruya al cargar una nueva escena
        DontDestroyOnLoad(gameObject);
    }
}
