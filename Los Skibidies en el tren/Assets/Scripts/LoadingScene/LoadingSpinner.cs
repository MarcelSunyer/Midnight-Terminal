using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSpinner : MonoBehaviour
{
    // Velocidad de rotación del objeto en grados por segundo.
    public float rotationSpeed = 100f;

    void Start()
    {
        // Inicia la corrutina para cargar la escena "Main_Menu" después de 1 segundo.
        StartCoroutine(LoadMainMenu());
    }

    void Update()
    {
        // Rota el objeto constantemente alrededor del eje Z.
        transform.Rotate(-Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    private System.Collections.IEnumerator LoadMainMenu()
    {
        // Espera N segundo.
        yield return new WaitForSeconds(0.5f);

        // Carga la escena "Main_Menu".
        SceneManager.LoadScene("Main_Menu");
    }
}
