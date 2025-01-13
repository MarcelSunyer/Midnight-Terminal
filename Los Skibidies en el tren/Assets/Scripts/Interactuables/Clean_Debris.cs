using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Clean_Debris : MonoBehaviour, IInteractable
{
    public float holdTime = 3f;       
    private float holdCounter = 0f;   
    private bool isHolding = false;  
    private Vector3 originalScale;    

    // Audio
    public AudioClip cleaningAudio;   
    private AudioSource audioSource;   

    public bool isDebrisDestroyed = false;

    public Progress_bar progress_bar;
    public float points;
    private void Start()
    {
        GameObject sliderBarObject = GameObject.Find("Slider_Bar");

        if (sliderBarObject != null)
        {
            // Obtiene el componente Progress_bar del GameObject encontrado
            progress_bar = sliderBarObject.GetComponent<Progress_bar>();
        }
        // Guarda el tamaño original del objeto
        originalScale = transform.localScale;

        // Obtén o agrega un AudioSource al objeto
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
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
                DestroyDebris();
                isDebrisDestroyed = true;

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
        PlayAudio(); // Reproducir el audio al iniciar la limpieza

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
        StopAudio(); // Parar el audio

        // Reinicia el estado de limpieza
        isHolding = false;
        holdCounter = 0f;
        transform.localScale = originalScale;
    }

    private void PlayAudio()
    {
        if (audioSource != null && cleaningAudio != null)
        {
            audioSource.clip = cleaningAudio;
            audioSource.loop = true; // Audio en bucle mientras se limpia
            audioSource.Play();
        }
    }

    private void StopAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    public void DestroyDebris()
    {
        progress_bar.act += points;
        isDebrisDestroyed = true;
        StopAudio();
        Destroy(transform.parent.gameObject);
    }
}
