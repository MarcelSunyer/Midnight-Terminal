using System.Collections;
using UnityEngine;

public class Clean_Debris : MonoBehaviour, IInteractable
{
    public float holdTime = 3f;         // Tiempo necesario para limpiar el objeto
    private float holdCounter = 0f;    // Contador del tiempo de pulsación
    private bool isHolding = false;    // Indica si el jugador está manteniendo la tecla
    private Vector3 originalScale;     // Tamaño original del objeto

    private void Start()
    {
        // Guarda el tamaño original del objeto
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Comprueba si el jugador está intentando moverse
        if (IsPlayerMoving())
        {
            if (isHolding)
            {
                Debug.Log("Movimiento detectado, limpieza reiniciada.");
                ResetCleaningProcess();
            }
        }

        // Si el jugador está manteniendo la tecla, actualiza el progreso
        if (isHolding)
        {
            holdCounter += Time.deltaTime;

            // Calcula el factor de reducción del tamaño
            float progress = Mathf.Clamp01(holdCounter / holdTime);
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);

            // Si se alcanza el tiempo necesario, destruye el padre del objeto
            if (holdCounter >= holdTime)
            {
                //TODO: Faltaría hacer que se actualice a todos los jugadores

                Debug.Log("Padre del objeto eliminado.");
                Destroy(transform.parent.gameObject); // Elimina el padre del GameObject
            }
        }
        else if (transform.localScale != originalScale)
        {
            // Si se suelta la tecla o se interrumpe, vuelve al tamaño original
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, 10f * Time.deltaTime);
        }
    }

    public void Interact()
    {
        // Inicia el proceso de limpieza si se mantiene la tecla
        StartCoroutine(HandleHold());
    }

    private IEnumerator HandleHold()
    {
        isHolding = true;

        // Mientras se mantiene la tecla "E", espera
        while (Input.GetKey(KeyCode.E))
        {
            yield return null;
        }

        // Si se suelta la tecla antes de terminar
        Debug.Log("Tecla soltada antes de terminar.");
        ResetCleaningProcess();
    }

    private bool IsPlayerMoving()
    {
        // Detecta si el jugador presiona alguna de las teclas de movimiento (WASD o Space)
        return Input.GetKey(KeyCode.W) ||
               Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) ||
               Input.GetKey(KeyCode.D) ||
               Input.GetKey(KeyCode.Space);
    }

    private void ResetCleaningProcess()
    {
        // Reinicia el estado de limpieza
        isHolding = false;
        holdCounter = 0f;
        transform.localScale = originalScale;
    }
}
