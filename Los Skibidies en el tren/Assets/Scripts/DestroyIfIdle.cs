using UnityEngine;

public class DestroyIfIdle : MonoBehaviour
{
    private Vector3 lastPosition; // �ltima posici�n registrada del objeto
    private float idleTime; // Tiempo acumulado de inactividad
    public float timeToDestroy = 30f; // Tiempo en segundos para destruir el objeto si no se mueve

    void Start()
    {
        // Inicializa la posici�n y el tiempo
        lastPosition = transform.position;
        idleTime = 0f;
    }

    void Update()
    {
        // Comprueba si la posici�n ha cambiado
        if (transform.position != lastPosition)
        {
            // Si se mueve, reinicia el contador de inactividad
            idleTime = 0f;
            lastPosition = transform.position;
        }
        else
        {
            // Si no se mueve, incrementa el tiempo de inactividad
            idleTime += Time.deltaTime;

            // Destruye el objeto si supera el tiempo l�mite
            if (idleTime >= timeToDestroy)
            {
                Destroy(gameObject);
            }
        }
    }
}
