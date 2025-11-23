using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float totalTime = 60f; // 1 минута (можете изменить)
    public TMP_Text timerText;

    [Header("References")]
    public GameManager gameManager;

    private float currentTime;
    private bool isRunning = true;
    private Color originalColor;

    void Start()
    {
        currentTime = totalTime;

        if (timerText != null)
        {
            originalColor = timerText.color;
        }

        UpdateTimerDisplay();

        // Найти GameManager если не назначен
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
            }
        }
    }

    void Update()
    {
        // Таймер работает только если игра идёт (не на паузе)
        if (isRunning && Time.timeScale > 0)
        {
            currentTime -= Time.deltaTime;

            // Проверка на окончание времени
            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                TimeUp();
            }

            UpdateTimerDisplay();

            // Мигание красным цветом когда осталось мало времени
            if (currentTime <= 10f && timerText != null)
            {
                // Плавное мигание между красным и жёлтым
                timerText.color = Color.Lerp(Color.red, Color.yellow, Mathf.PingPong(Time.time * 2, 1));
            }
            else if (currentTime <= 30f && timerText != null)
            {
                // Оранжевый цвет когда осталось меньше 30 секунд
                timerText.color = new Color(1f, 0.6f, 0f); // Оранжевый
            }
            else if (timerText != null)
            {
                // Обычный цвет
                timerText.color = originalColor;
            }
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            Debug.LogWarning("Timer Text is not assigned!");
        }
    }

    void TimeUp()
    {
        Debug.Log("Time's up! Game Over!");

        if (gameManager != null)
        {
            gameManager.ShowDeathPanel();
        }
        else
        {
            Debug.LogError("Cannot show death panel - GameManager is null");
        }
    }

    // Остановить таймер (вызывается при победе)
    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("Timer stopped");
    }

    // Добавить время (бонус)
    public void AddTime(float seconds)
    {
        currentTime += seconds;
        Debug.Log("Added " + seconds + " seconds to timer");
    }

    // Узнать оставшееся время
    public float GetRemainingTime()
    {
        return currentTime;
    }

    // Проверить, идёт ли таймер
    public bool IsRunning()
    {
        return isRunning;
    }
}