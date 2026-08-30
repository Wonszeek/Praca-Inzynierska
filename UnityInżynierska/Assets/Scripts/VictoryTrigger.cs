using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (gameManager != null)
            gameManager.ShowVictory();
        else
            Debug.LogWarning("[VictoryTrigger] Nie znaleziono GameManager!");
    }
}