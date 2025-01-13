using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHandler : MonoBehaviour
{
    private float life = 100;
    private float maxLife = 100;
    private bool isTakingDamage = false;
    private bool isInvincible = false;
    private float lastDamageTime;
    public float lifeRegenDelay = 5f; // Time before health starts regenerating
    public float lifeRegenRate = 5f;  // Life points regenerated per second
    public Image damageOverlay; // UI element for the red vignette effect
    public float fadeSpeed = 0.5f;
    public float invincibilityDuration = 1f; // Duration of invincibility after getting hit

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(1, 0, 0, 0);
        }
    }

    void Update()
    {
        if (!isTakingDamage && life < maxLife && Time.time - lastDamageTime > lifeRegenDelay)
        {
            life += lifeRegenRate * Time.deltaTime;
            life = Mathf.Min(life, maxLife);
        }

        // Gradually fade the damage overlay
        if (damageOverlay != null && damageOverlay.color.a > 0)
        {
            damageOverlay.color = new Color(1, 0, 0, Mathf.Max(0, damageOverlay.color.a - Time.deltaTime * fadeSpeed));
        }

    }

    public void ReciveHit(float dmg)
    {
        if (isInvincible) return; // Prevent taking damage during invincibility

        isTakingDamage = true;
        isInvincible = true;
        lastDamageTime = Time.time;

        if (life >= dmg)
        {
            life -= dmg;
        }
        else
        {
            life = maxLife;
            transform.position = new Vector3(0, 5, 0);
        }

        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(1, 0, 0, 0.5f); // Show red vignette
        }

        StartCoroutine(StopTakingDamage());
        StartCoroutine(StopInvincibility());
    }

    private IEnumerator StopTakingDamage()
    {
        yield return new WaitForSeconds(1);
        isTakingDamage = false;
    }

    private IEnumerator StopInvincibility()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }


    //---PUBLIC FUCTIONS---
    public void Respawn()
    {
        life = maxLife;
        transform.position = new Vector3(0, 5, 0);
    }
}
