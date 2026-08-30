using UnityEngine;
using System.Collections;
/// <summary>
/// Zarządza życiem gracza. Dołącz do obiektu gracza.
/// Wywołaj TakeDamage(int) z dowolnego miejsca (np. z NPC).
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHealth = 5;
    private int currentHealth;
 
    [Header("Nietykalność po trafieniu")]
    public float invincibilityDuration = 1f;   // sekundy nietykalności po uderzeniu
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;
    private bool isDead = false;
 
    [Header("Knockback")]
    public float knockbackForce = 4f;
    private Rigidbody2D rb;
    
    [Header("Feedback wizualny")]
    public float flashDuration = 0.08f;
    public int flashCount = 5;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
 
    // Zdarzenia – subskrybuj w UI lub innych skryptach
    public System.Action<int, int> OnHealthChanged;   // (current, max)
    public System.Action OnPlayerDied;
 
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }
 
    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
 
    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
                isInvincible = false;
        }
    }
 
    /// <summary>
    /// Wywołaj tę metodę żeby zadać graczowi obrażenia.
    /// attacker – transform wroga (do obliczenia kierunku knockbacku), może być null.
    /// </summary>
    public void TakeDamage(int amount, Transform attacker = null)
    {
        if (isInvincible) return;
 
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
 
        Debug.Log($"[PlayerHealth] Gracz otrzymał {amount} obrażeń. HP: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
 
        // Krótki knockback
        if (attacker != null && rb != null)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }
 
        // Włącz nietykalność
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        StartCoroutine(HitFeedback());
 
        if (currentHealth <= 0)
            Die();
    }
 
    private IEnumerator HitFeedback()
    {
        for (int i = 0; i < flashCount; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.red;

            yield return new WaitForSeconds(flashDuration);

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(flashDuration);
        }
    }
    
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
 
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log("[PlayerHealth] Gracz zginął!");
        OnPlayerDied?.Invoke();
        
    }
}