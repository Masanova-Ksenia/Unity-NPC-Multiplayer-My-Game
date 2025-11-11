using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    [Header("AI Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float detectionRange = 15f;
    public float coinDetectionRange = 10f;
    public float coinChaseDistance = 5f; 
    public float wanderTime = 3f;

    [Header("Combat")]
    public int maxHealth = 8;
    public int currentHealth;
    public int normalDamage = 15;
    public int angryDamage = 30;
    public float attackCooldown = 1f;
    public float attackRange = 2f;

    [Header("UI")]
    public Slider healthBar;
    public GameObject healthBarCanvas;

    [Header("Nest")]
    public int collectedCoins = 0;
    public GameObject coinPrefab;

    private enum AIState { Wandering, ChasingCoin, FleeingFromPlayer, Attacking }
    private AIState currentState = AIState.Wandering;

    private Transform player;
    private Transform targetCoin;
    private bool hasPickedCoin = false;
    private bool isAngry = false;
    private float lastAttackTime;

    private Vector3 wanderTarget;
    private float wanderTimer;
    private float wanderElapsedTime = 0f;
    private Rigidbody rb;
    private Animator animator;
    private NavMeshAgent agent;

    private Renderer npcRenderer;
    private Material npcMaterial;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        npcRenderer = GetComponent<Renderer>();
        if (npcRenderer != null)
        {
            npcMaterial = npcRenderer.material;
            if (npcMaterial != null)
            {
                originalColor = npcMaterial.color;
            }
        }

        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.acceleration = 8;
        }

        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // Найти игрока
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Скрыть полоску здоровья изначально
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

        SetNewWanderTarget();
    }

    void Update()
    {
        // Полоска здоровья всегда смотрит на камеру
        if (healthBarCanvas != null && healthBarCanvas.activeSelf)
        {
            healthBarCanvas.transform.LookAt(Camera.main.transform);
            healthBarCanvas.transform.Rotate(0, 180, 0);
        }

        switch (currentState)
        {
            case AIState.Wandering:
                Wander();
                CheckForCoins();
                CheckForPlayer();
                break;

            case AIState.ChasingCoin:
                ChaseCoin();
                CheckForPlayer();
                break;

            case AIState.FleeingFromPlayer:
                FleeFromPlayer();
                break;

            case AIState.Attacking:
                AttackPlayer();
                break;
        }
    }

    void Wander()
    {
        MoveTowards(wanderTarget, walkSpeed);

        if (Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            SetNewWanderTarget();
        }

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            SetNewWanderTarget();
        }
    }

    void SetNewWanderTarget()
    {
        float randomX = Random.Range(-15f, 15f);
        float randomZ = Random.Range(-15f, 15f);
        wanderTarget = new Vector3(randomX, transform.position.y, randomZ);
        wanderTimer = Random.Range(3f, 6f);
    }

    void CheckForCoins()
    {
        if (hasPickedCoin) return;

        wanderElapsedTime += Time.deltaTime;
        if (wanderElapsedTime < wanderTime)
        {
            return; // Ещё рано искать монеты
        }

        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        float closestDistance = coinDetectionRange;
        GameObject closestCoin = null;

        foreach (GameObject coin in coins)
        {
            float distance = Vector3.Distance(transform.position, coin.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCoin = coin;
            }
        }

        if (closestCoin != null)
        {
            targetCoin = closestCoin.transform;
            currentState = AIState.ChasingCoin;
        }
    }

    void ChaseCoin()
    {
        if (targetCoin == null)
        {
            currentState = AIState.Wandering;
            return;
        }

        MoveTowards(targetCoin.position, runSpeed);

        if (Vector3.Distance(transform.position, targetCoin.position) < 1f)
        {
            PickUpCoin();
        }
    }

    void PickUpCoin()
    {
        if (targetCoin != null)
        {
            Destroy(targetCoin.gameObject);
            hasPickedCoin = true;
            collectedCoins++;
            targetCoin = null;

            if (npcMaterial != null)
            {
                npcMaterial.EnableKeyword("_EMISSION");
                npcMaterial.SetColor("_EmissionColor", Color.yellow * 0.8f);
                Debug.Log(gameObject.name + " светится - есть монета!");
            }

            currentState = AIState.Wandering;
        }
    }

    void CheckForPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isAngry && distanceToPlayer < detectionRange)
        {
            // Убегать от игрока если не злой
            currentState = AIState.FleeingFromPlayer;
        }
        else if (isAngry && distanceToPlayer < attackRange)
        {
            // Атаковать если злой и близко
            currentState = AIState.Attacking;
        }
    }

    void FleeFromPlayer()
    {
        if (player == null)
        {
            currentState = AIState.Wandering;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange + 2f)
        {
            currentState = AIState.Wandering;
            return;
        }

        // Бежать в противоположную сторону от игрока
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * 5f;

        MoveTowards(fleeTarget, runSpeed);
    }

    void AttackPlayer()
    {
        if (player == null)
        {
            currentState = AIState.Wandering;
            return;
        }

        // Смотреть на игрока
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange + 1f)
        {
            // Преследовать игрока
            MoveTowards(player.position, runSpeed);
        }
        else if (distanceToPlayer <= attackRange)
        {
            // Атаковать
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    void PerformAttack()
    {
        if (player == null) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            int damage = isAngry ? angryDamage : normalDamage;
            playerController.TakeDamage(damage);
            Debug.Log(gameObject.name + " атакует игрока! Урон: " + damage);
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        if (agent != null && agent.enabled)
        {
            // Использовать NavMesh для движения
            agent.speed = speed;
            agent.SetDestination(target);
        }
        else
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0; // Двигаться только по горизонтали

            if (rb != null)
            {
                Vector3 velocity = rb.velocity;
                velocity.x = direction.x * speed;
                velocity.z = direction.z * speed;
                rb.velocity = velocity;
            }

            // Поворачивать в сторону движения
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Показать полоску здоровья
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(true);
        }

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        StartCoroutine(DamageFlash());

        // Эффект получения урона (подпрыгивание)
        if (rb != null)
        {
            rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
        }

        Debug.Log(gameObject.name + " получил урон! Здоровье: " + currentHealth + "/" + maxHealth);

        // Стать злым после 3 ударов
        if (currentHealth <= 2 && !isAngry)
        {
            BecomeAngry();
        }

        // Смерть
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        if (npcRenderer != null && npcMaterial != null)
        {
            Color flashColor = Color.red;
            Color savedColor = npcMaterial.color;
            npcMaterial.color = flashColor;

            yield return new WaitForSeconds(0.1f);

            // Вернуть оригинальный цвет или сохраненный
            npcMaterial.color = savedColor;
        }
    }

    void BecomeAngry()
    {
        isAngry = true;
        currentState = AIState.Attacking;
        Debug.Log(gameObject.name + " разозлился!");

        if (npcRenderer != null && npcMaterial != null)
        {
            npcMaterial.color = Color.red;
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " погиб!");

        // Если нес монету, выронить её
        if (hasPickedCoin)
        {
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            GameObject droppedCoin = Instantiate(coinPrefab, dropPosition, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            if (!droppedCoin.CompareTag("Coin"))
            {
                droppedCoin.tag = "Coin";
            }
        }
        if (npcRenderer != null && npcMaterial != null)
        {
            npcMaterial.DisableKeyword("_EMISSION");
        }

        Destroy(gameObject);
    }
}