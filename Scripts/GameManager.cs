using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject deathPanel;
    public GameObject winPanel;

    private bool isPaused = false;
    private bool isGameOver = false;

    private GameTimer gameTimer;

    void Start()
    {
        // Скрыть все панели
        if (pausePanel != null) pausePanel.SetActive(false);
        if (deathPanel != null) deathPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        //Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;

        gameTimer = FindObjectOfType<GameTimer>();
    }

    void Update()
    {
        // Обработка паузы через ESC
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused && !isGameOver)
        {
            Debug.Log(">>> ESC pressed - calling PauseGame");
            PauseGame();
            
        }
    }

    public void PauseGame()
    {
        Debug.Log("=== PauseGame() called ===");
        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Cursor visible: " + Cursor.visible);
    }

    public void ResumeGame()
    {
        Debug.Log("=== ResumeGame() called - FROM BUTTON! ===");
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Game resumed, TimeScale: " + Time.timeScale);
    }

    public void ShowDeathPanel()
    {
        Debug.Log("=== ShowDeathPanel() called ===");
        isGameOver = true;

        if (gameTimer != null)
        {
            gameTimer.StopTimer();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayDeathSound();
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("DeathPanel is NULL!");
        }
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowWinPanel()
    {
        Debug.Log("=== ShowWinPanel() called ===");
        isGameOver = true;

        if (gameTimer != null)
        {
            gameTimer.StopTimer();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayWinSound();
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("WinPanel is NULL!");
        }
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartGame()
    {
        Debug.Log("=== RestartGame() called - FROM BUTTON! ===");
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RestartMusic();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Debug.Log("=== ExitToMenu() called - FROM BUTTON! ===");
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}