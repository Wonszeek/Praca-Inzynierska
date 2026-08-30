using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referencje")]
    public PlayerHealth playerHealth;
    public Slider healthSlider;
    public GameObject healthBarRoot;

    [Header("Opcje")]
    public bool hideWhenDead = true;

    private void Awake()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (healthBarRoot == null)
            healthBarRoot = gameObject;

        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void Start()
    {
        if (playerHealth != null)
            UpdateHealthBar(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        else
            Debug.LogWarning("[PlayerHealthUI] Nie znaleziono PlayerHealth!");
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthSlider == null) return;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (hideWhenDead && currentHealth <= 0)
        {
            if (healthBarRoot != null)
                healthBarRoot.SetActive(false);
        }
    }
}