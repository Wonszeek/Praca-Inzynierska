using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("Referencje")]
    public EnemyHealth enemyHealth;
    public Slider healthSlider;

    [Header("Opcje")]
    public bool hideWhenFull = true;
    public bool hideWhenDead = true;

    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void Start()
    {
        if (enemyHealth != null)
            UpdateHealthBar(enemyHealth.GetCurrentHealth(), enemyHealth.GetMaxHealth());
        else
            Debug.LogWarning("[EnemyHealthUI] Nie znaleziono EnemyHealth!");
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthSlider == null) return;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (canvas == null) return;

        if (hideWhenDead && currentHealth <= 0)
        {
            canvas.enabled = false;
            return;
        }

        if (hideWhenFull)
        {
            canvas.enabled = currentHealth < maxHealth;
        }
        else
        {
            canvas.enabled = true;
        }
    }
}