using UnityEngine;

public class EnemyWander : MonoBehaviour
{
    [Header("Wander Settings")]
    public Vector3 centerPoint = Vector3.zero; // Center of the movement area
    public float moveSpeedMin = 1f; // Minimum enemy speed
    public float moveSpeedMax = 5f; // Maximum enemy speed
    public float moveSpeedChangeRate = 0.2f; // Rate at which the speed changes
    public float radiusMin = 2f; // Minimum radius of the circle
    public float radiusMax = 5f; // Maximum radius of the circle
    public float radiusChangeSpeed = 0.5f; // Speed at which the circle radius changes

    private float currentAngle = 0f; // Current angle in degrees
    private float currentRadius; // Current radius of the circle
    private float currentMoveSpeed; // Current movement speed
    private bool increasingRadius = true; // Whether the radius is increasing
    private bool increasingSpeed = true; // Whether the speed is increasing

    void Start()
    {
        currentRadius = radiusMin; // Start with the minimum radius
        currentMoveSpeed = moveSpeedMin; // Start with the minimum speed
    }

    void Update()
    {
        // Update the radius
        if (increasingRadius)
        {
            currentRadius += radiusChangeSpeed * Time.deltaTime;
            if (currentRadius >= radiusMax)
            {
                currentRadius = radiusMax;
                increasingRadius = false;
            }
        }
        else
        {
            currentRadius -= radiusChangeSpeed * Time.deltaTime;
            if (currentRadius <= radiusMin)
            {
                currentRadius = radiusMin;
                increasingRadius = true;
            }
        }

        // Update the move speed
        if (increasingSpeed)
        {
            currentMoveSpeed += moveSpeedChangeRate * Time.deltaTime;
            if (currentMoveSpeed >= moveSpeedMax)
            {
                currentMoveSpeed = moveSpeedMax;
                increasingSpeed = false;
            }
        }
        else
        {
            currentMoveSpeed -= moveSpeedChangeRate * Time.deltaTime;
            if (currentMoveSpeed <= moveSpeedMin)
            {
                currentMoveSpeed = moveSpeedMin;
                increasingSpeed = true;
            }
        }

        // Calculate the new position along the circle
        currentAngle += currentMoveSpeed * Time.deltaTime;
        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
        }

        float radianAngle = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radianAngle), 0, Mathf.Sin(radianAngle)) * currentRadius;
        Vector3 newPosition = centerPoint + offset;

        // Move the enemy to the new position
        transform.position = newPosition;

        // Rotate to face the movement direction
        Vector3 direction = new Vector3(-Mathf.Sin(radianAngle), 0, Mathf.Cos(radianAngle));
        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the minimum and maximum radii in the scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPoint, radiusMin);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPoint, radiusMax);
    }
}
