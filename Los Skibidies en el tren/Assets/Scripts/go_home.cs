using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class go_home : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Comprueba si el objeto de la colisión tiene el mismo tag que "home"

        if (collision.gameObject.CompareTag("Player")) // Cambia "HomeTag" por el tag correspondiente
        {
            SceneManager.LoadScene(6);
        }

    }
}

