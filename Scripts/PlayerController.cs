using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    private bool isGrounded = true;

    public Slider healthBar;
    public TMPro.TMP_Text CoinCounter;
    public Image healthFill;

    public int health = 100;
    public int coins = 0;
    public int coinsNeeded = 8; // Монет нужно на игрока

    private GameManager gameManager;
    private PhotonView photonView;
    private bool isMultiplayer = false;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        // Проверка мультиплеера
        isMultiplayer = PhotonNetwork.IsConnected && photonView != null && photonView.IsMine;

        // Управление только своим игроком
        if (photonView != null && !photonView.IsMine)
        {
            enabled = false;
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;
            return;
        }
        FindUIElements();
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        if (healthBar != null)
        {
            healthBar.value = health;
            healthBar.gameObject.SetActive(true);
        }
        if (CoinCounter != null)
        {
            CoinCounter.gameObject.SetActive(true);
        }
        if (healthFill != null)
        {
            healthFill.gameObject.SetActive(true);
        }
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager не найден!");
        }
        UpdateCoinCounter();
        Debug.Log("PlayerController инициализирован. Мультиплеер: " + isMultiplayer);
    }
    void FindUIElements()
    {
        // Поиск HealthBar
        if (healthBar == null)
        {
            Slider[] allSliders = FindObjectsOfType<Slider>();
            foreach (Slider slider in allSliders)
            {
                if (slider.gameObject.name.Contains("Health"))
                {
                    healthBar = slider;
                    Debug.Log("HealthBar найден: " + slider.name);
                    break;
                }
            }
        }

        // Поиск CoinCounter
        if (CoinCounter == null)
        {
            TMPro.TMP_Text[] allTexts = FindObjectsOfType<TMPro.TMP_Text>();
            foreach (TMPro.TMP_Text text in allTexts)
            {
                if (text.gameObject.name.Contains("Coin"))
                {
                    CoinCounter = text;
                    Debug.Log("CoinCounter найден: " + text.name);
                    break;
                }
            }
        }

        // Поиск HealthFill
        if (healthFill == null && healthBar != null)
        {
            Transform fillTransform = healthBar.transform.Find("Fill Area/Fill");
            if (fillTransform != null)
            {
                healthFill = fillTransform.GetComponent<Image>();
                Debug.Log("HealthFill найден");
            }
        }

        // Финальная проверка
        if (healthBar == null) Debug.LogError("HealthBar НЕ НАЙДЕН!");
        if (CoinCounter == null) Debug.LogError("CoinCounter НЕ НАЙДЕН!");
        if (healthFill == null) Debug.LogWarning("HealthFill не найден");
    }
    void Update()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        if (photonView != null && !photonView.IsMine) return;

        if (Time.timeScale > 0)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

            if (rb != null)
            {
                Vector3 velocity = rb.velocity;
                velocity.x = move.x * currentSpeed;
                velocity.z = move.z * currentSpeed;
                rb.velocity = velocity;
            }

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                if (rb != null)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    isGrounded = false;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    public void AddCoin()
    {
        coins++;
        UpdateCoinCounter();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoinSound();
        }

        // Проверка победы
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        if (isMultiplayer)
        {
            // МУЛЬТИПЛЕЕР: проверить общее количество монет
            int totalCoinsCollected = GetTotalCoinsCollected();
            int totalCoinsNeeded = PhotonNetwork.CurrentRoom.PlayerCount * coinsNeeded;

            Debug.Log("Собрано монет: " + totalCoinsCollected + "/" + totalCoinsNeeded);

            if (totalCoinsCollected >= totalCoinsNeeded)
            {
                // Победа! Все монеты собраны
                photonView.RPC("RPC_ShowWinPanel", RpcTarget.All);
            }
        }
        else
        {
            // ОДИНОЧНАЯ ИГРА
            if (coins >= coinsNeeded)
            {
                if (gameManager != null)
                {
                    gameManager.ShowWinPanel();
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayWinSound();
                    }
                }
            }
        }
    }

    int GetTotalCoinsCollected()
    {
        int total = 0;

        // Собрать монеты всех игроков
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                total += player.coins;
            }
        }

        return total;
    }

    [PunRPC]
    void RPC_ShowWinPanel()
    {
        if (gameManager != null)
        {
            gameManager.ShowWinPanel();
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWinSound();
            }
        }
    }

    void UpdateCoinCounter()
    {
        if (isMultiplayer)
        {
            // Показать личный прогресс и общий
            int totalNeeded = PhotonNetwork.CurrentRoom.PlayerCount * coinsNeeded;
            int totalCollected = GetTotalCoinsCollected();
            CoinCounter.text = coins + "/" + coinsNeeded + " (Всего: " + totalCollected + "/" + totalNeeded + ")";
        }
        else
        {
            CoinCounter.text = coins + "/" + coinsNeeded;
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        healthBar.value = health;

        if (health <= 30)
            healthFill.color = Color.red;
        else
            healthFill.color = Color.green;

        if (health <= 0)
        {
            if (gameManager != null)
            {
                gameManager.ShowDeathPanel();
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDeathSound();
                }
            }
        }
    }
}