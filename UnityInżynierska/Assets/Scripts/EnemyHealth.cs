using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHealth = 3;
    private int currentHealth;

    public event System.Action<int, int> OnHealthChanged;

    [Header("Nietykalność po trafieniu")]
    public float invincibilityDuration = 0.3f;
    private bool isInvincible = false;

    [Header("Knockback")]
    public float knockbackForce = 4f;
    public float knockbackUpForce = 1f;

    [Header("Feedback wizualny")]
    public float flashDuration = 0.08f;
    public int flashCount = 3;

    [Header("Śmierć")]
    public float destroyFallbackDelay = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Color originalColor;
    private bool isDead = false;
    private EnemyFSM enemyFSM;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyFSM = GetComponent<EnemyFSM>();
        anim = GetComponent<Animator>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, null);
    }

    public void TakeDamage(int amount, Transform attacker)
    {
        if (isDead) return;
        if (isInvincible) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[EnemyHealth] {name} otrzymał {amount} obrażeń. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        ApplyKnockback(attacker);

        if (enemyFSM != null)
            enemyFSM.ApplyHitStun();

        StartCoroutine(HitFeedback());
    }

    private void ApplyKnockback(Transform attacker)
    {
        if (attacker == null || rb == null) return;

        Vector2 direction = ((Vector2)transform.position - (Vector2)attacker.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(
            new Vector2(direction.x * knockbackForce, knockbackUpForce),
            ForceMode2D.Impulse
        );
    }

    private IEnumerator HitFeedback()
    {
        isInvincible = true;

        for (int i = 0; i < flashCount; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.red;

            yield return new WaitForSeconds(flashDuration);

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(flashDuration);
        }

        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("### TEST DIE 777 - TO JEST AKTUALNY EnemyHealth.cs ###");
        if (isDead) return;

        isDead = true;
        Debug.Log($"[EnemyHealth] {name} zginął!");

        StopAllCoroutines();

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        OnHealthChanged?.Invoke(0, maxHealth);

        if (enemyFSM != null)
            enemyFSM.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        if (anim != null)
            anim.SetTrigger("EnemyDeathTrigger");
        else
            DestroyEnemy();

        Destroy(gameObject, destroyFallbackDelay);
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}