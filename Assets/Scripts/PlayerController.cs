using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public int maxCoins = 8;

    private PhotonView photonView;

    private GameManager gameManager;


    void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView != null && !photonView.IsMine)
        {
            // Отключить управление
            enabled = false;

            // Отключить камеру
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;

            return;
        }
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        healthBar.value = health;
        CoinCounter.text = coins + "/" + maxCoins;

        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (photonView != null && !photonView.IsMine) return;

        if (Time.timeScale > 0)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

            Vector3 velocity = rb.velocity;
            velocity.x = move.x * speed;
            velocity.z = move.z * speed;
            rb.velocity = velocity;

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
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
        CoinCounter.text = coins + "/" + maxCoins;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoinSound();
        }

        if (coins >= maxCoins)
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