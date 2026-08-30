using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Player")]
    public PlayerHealth playerHealth;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    private bool gameEnded = false;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
            playerHealth.OnPlayerDied += ShowGameOver;
        else
            Debug.LogWarning("[GameManager] Nie znaleziono PlayerHealth!");
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnPlayerDied -= ShowGameOver;
    }

    private void ShowGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;

        Debug.Log("[GameManager] Game Over!");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        if (gameEnded) return;

        gameEnded = true;

        Debug.Log("[GameManager] Victory!");

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] Quit Game");
        Application.Quit();
    }
}