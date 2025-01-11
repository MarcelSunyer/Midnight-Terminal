using UnityEngine;
using System.Collections; // Necesario para usar corrutinas

public class EnemyWander : MonoBehaviour
{
    [Header("Wander Settings")]
    public Vector3 centerPoint = Vector3.zero; // Center of the wander area
    public float moveSpeedMin = 1f; // Minimum movement speed
    public float moveSpeedMax = 5f; // Maximum movement speed
    public float moveSpeedChangeRate = 0.2f; // Rate at which speed changes
    public float radiusMin = 2f; // Minimum radius for wandering
    public float radiusMax = 5f; // Maximum radius for wandering
    public float radiusChangeSpeed = 0.5f; // Speed at which radius changes

    [Header("Chase Settings")]
    public float chaseRange = 10f; // Distance at which the enemy starts chasing the player
    public float stopChasingRange = 12f; // Distance at which the enemy stops chasing
    public float chaseSpeed = 6f; // Speed while chasing the player

    private float currentAngle = 0f;
    private float currentRadius;
    private float currentMoveSpeed;
    private bool increasingRadius = true;
    private bool increasingSpeed = true;
    private Transform targetPlayer; // Reference to the player currently in range
    private bool isChasing = false;
    private bool isPaused = false; // Para controlar si el enemigo está pausado

    void Start()
    {
        currentRadius = radiusMin;
        currentMoveSpeed = moveSpeedMin;
    }

    void Update()
    {
        if (isPaused) return; // No hacer nada si el enemigo está pausado

        // Detect the closest player within chase range
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, chaseRange, LayerMask.GetMask("Player"));
        targetPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider player in playersInRange)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                targetPlayer = player.transform;
            }
        }

        if (targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (isChasing)
            {
                if (distanceToPlayer > stopChasingRange)
                {
                    isChasing = false;
                }
                else
                {
                    ChasePlayer();
                }
            }
            else
            {
                if (distanceToPlayer < chaseRange)
                {
                    isChasing = true;
                }
                Wander();
            }
        }
        else
        {
            Wander();
        }
    }

    void Wander()
    {
        if (increasingRadius)
        {
            currentRadius += radiusChangeSpeed * Time.deltaTime;
            if (currentRadius >= radiusMax)
                increasingRadius = false;
        }
        else
        {
            currentRadius -= radiusChangeSpeed * Time.deltaTime;
            if (currentRadius <= radiusMin)
                increasingRadius = true;
        }

        if (increasingSpeed)
        {
            currentMoveSpeed += moveSpeedChangeRate * Time.deltaTime;
            if (currentMoveSpeed >= moveSpeedMax)
                increasingSpeed = false;
        }
        else
        {
            currentMoveSpeed -= moveSpeedChangeRate * Time.deltaTime;
            if (currentMoveSpeed <= moveSpeedMin)
                increasingSpeed = true;
        }

        currentAngle += currentMoveSpeed * Time.deltaTime;
        if (currentAngle >= 360f)
            currentAngle -= 360f;

        float radianAngle = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radianAngle), 0, Mathf.Sin(radianAngle)) * currentRadius;
        Vector3 newPosition = centerPoint + offset;

        transform.position = newPosition;
        transform.rotation = Quaternion.LookRotation(offset);
    }

    void ChasePlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, chaseSpeed * Time.deltaTime);
        transform.LookAt(targetPlayer);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHandler playerHandler = collision.gameObject.GetComponent<PlayerHandler>();
            if (playerHandler != null)
            {
                playerHandler.ReciveHit(30f);
            }

            // Pausar el enemigo
            StartCoroutine(PauseMovement(1f));
        }
    }

    IEnumerator PauseMovement(float duration)
    {
        isPaused = true; // Detener el movimiento
        yield return new WaitForSeconds(duration); // Esperar la duración especificada
        isPaused = false; // Reanudar el movimiento
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPoint, radiusMin);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPoint, radiusMax);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
