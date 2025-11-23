using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public int damage = 1;
    public float attackRange = 3f;
    public float attackCooldown = 0.5f;
    public Transform weaponTransform; // Модель оружия в руке

    [Header("Attack Detection")]
    public LayerMask npcLayer;
    public float attackAngle = 45f; 
    public bool useQKeyAttack = true; 

    [Header("Animation")]
    public float swingSpeed = 10f;
    public float swingAngle = 45f;

    private float lastAttackTime;
    private bool isSwinging = false;
    private Quaternion originalRotation;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (weaponTransform != null)
        {
            originalRotation = weaponTransform.localRotation;
        }
    }

    void Update()
    {
        Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * attackRange, Color.cyan);
        if (useQKeyAttack && Input.GetKeyDown(KeyCode.Q) &&
            Time.time - lastAttackTime >= attackCooldown &&
            Time.timeScale > 0)
        {
            AttackWithQ();
            lastAttackTime = Time.time;
        }

        if (!useQKeyAttack && Input.GetMouseButtonDown(0) &&
            Time.time - lastAttackTime >= attackCooldown &&
            Time.timeScale > 0 &&
            !IsPointerOverUI())
        {
            AttackWithMouse();
            lastAttackTime = Time.time;
        }
        // Анимация взмаха
        if (isSwinging && weaponTransform != null)
        {
            AnimateSwing();
        }
    }
    void AttackWithQ()
    {
        Debug.Log("=== АТАКА НА Q! ===");

        // Анимация
        if (weaponTransform != null)
        {
            isSwinging = true;
            StartCoroutine(SwingWeapon());
        }

        // Ищем всех NPC в радиусе перед игроком
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        bool hitSomething = false;

        foreach (Collider col in hitColliders)
        {
            NPCController npc = col.GetComponent<NPCController>();

            if (npc != null)
            {
                // Проверить что NPC перед игроком
                Vector3 directionToNPC = (npc.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(mainCamera.transform.forward, directionToNPC);

                Debug.Log("NPC найден: " + npc.name + ", угол: " + angle);

                if (angle < attackAngle) // Широкий конус атаки
                {
                    npc.TakeDamage(damage);
                    Debug.Log("ПОПАДАНИЕ! Ударил " + npc.name);
                    hitSomething = true;

                    // Визуальная линия попадания (для отладки)
                    Debug.DrawLine(transform.position, npc.transform.position, Color.green, 1f);
                }
                else
                {
                    Debug.Log("Промах - NPC вне угла атаки");
                    Debug.DrawLine(transform.position, npc.transform.position, Color.yellow, 1f);
                }
            }
        }

        if (!hitSomething)
        {
            Debug.Log("Атака не попала ни в кого");
        }
    }
    void AttackWithMouse()
    {
        Debug.Log("=== АТАКА ЛКМ! ===");

        // Анимация
        if (weaponTransform != null)
        {
            isSwinging = true;
            StartCoroutine(SwingWeapon());
        }

        // Raycast из камеры (точная атака курсором)
        RaycastHit hit;
        Vector3 rayOrigin = mainCamera.transform.position;
        Vector3 rayDirection = mainCamera.transform.forward;

        Debug.DrawRay(rayOrigin, rayDirection * attackRange, Color.red, 1f);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange))
        {
            Debug.Log("Raycast попал в: " + hit.collider.gameObject.name);

            NPCController npc = hit.collider.GetComponent<NPCController>();
            if (npc != null)
            {
                npc.TakeDamage(damage);
                Debug.Log("Ударил NPC через Raycast!");
                return;
            }
        }

        // Запасной вариант - OverlapSphere
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (Collider col in hitColliders)
        {
            NPCController npc = col.GetComponent<NPCController>();
            if (npc != null && col.gameObject != gameObject)
            {
                Vector3 directionToNPC = (npc.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(mainCamera.transform.forward, directionToNPC);

                if (angle < 60f)
                {
                    npc.TakeDamage(damage);
                    Debug.Log("Ударил NPC через OverlapSphere!");
                    return;
                }
            }
        }

        Debug.Log("Атака не попала");
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    System.Collections.IEnumerator SwingWeapon()
    {
        if (weaponTransform == null) yield break;

        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float angle = Mathf.Lerp(0, swingAngle, elapsedTime / duration);
            weaponTransform.localRotation = originalRotation * Quaternion.Euler(-angle, 0, 0);
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float angle = Mathf.Lerp(swingAngle, 0, elapsedTime / duration);
            weaponTransform.localRotation = originalRotation * Quaternion.Euler(-angle, 0, 0);
            yield return null;
        }

        weaponTransform.localRotation = originalRotation;
        isSwinging = false;
    }
    //void Attack()
    //{
        //Debug.Log("Атака!");

        // Анимация взмаха
        //if (weaponTransform != null)
        //{
            //isSwinging = true;
            //StartCoroutine(SwingWeapon());
        //}

        // Raycast для обнаружения врагов
        //RaycastHit hit;
        //Vector3 rayOrigin = Camera.main.transform.position;
        //Vector3 rayDirection = Camera.main.transform.forward;

       // if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange))
        //{
            //Debug.Log("Попал в: " + hit.collider.gameObject.name);

            // Проверить, это NPC?
            //NPCController npc = hit.collider.GetComponent<NPCController>();
            //if (npc != null)
            //{
                //npc.TakeDamage(damage);
                //Debug.Log("Ударил NPC!");
            //}
        //}

        // Альтернативный метод - проверка всех объектов в радиусе
        //Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        //foreach (Collider hitCollider in hitColliders)
        //{
            //NPCController npc = hitCollider.GetComponent<NPCController>();
            //if (npc != null && hitCollider.gameObject != gameObject)
            //{
                // Проверить, что NPC перед игроком
                //Vector3 directionToNPC = (npc.transform.position - transform.position).normalized;
                //float angle = Vector3.Angle(Camera.main.transform.forward, directionToNPC);

                //if (angle < 60f) // В пределах 60 градусов перед игроком
                //{
                    //npc.TakeDamage(damage);
                //}
            //}
        //}
    //}

    //System.Collections.IEnumerator SwingWeapon()
    //{
        //float elapsedTime = 0f;
        //float duration = 0.2f;

        // Взмах вперёд
        //while (elapsedTime < duration)
        //{
            //elapsedTime += Time.deltaTime;
            //float angle = Mathf.Lerp(0, swingAngle, elapsedTime / duration);
           // weaponTransform.localRotation = originalRotation * Quaternion.Euler(-angle, 0, 0);
            //yield return null;
        //}
//
        // Возврат назад
        //elapsedTime = 0f;
        //while (elapsedTime < duration)
        //{
            //elapsedTime += Time.deltaTime;
            //float angle = Mathf.Lerp(swingAngle, 0, elapsedTime / duration);
            //weaponTransform.localRotation = originalRotation * Quaternion.Euler(-angle, 0, 0);
            //yield return null;
        //}

        //weaponTransform.localRotation = originalRotation;
        //isSwinging = false;
    //}
    void OnDrawGizmosSelected()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Сфера атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Луч из камеры
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * attackRange);

        // Конус атаки для Q
        if (useQKeyAttack)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Vector3 forward = mainCamera.transform.forward * attackRange;
            Vector3 right = Quaternion.Euler(0, attackAngle, 0) * forward;
            Vector3 left = Quaternion.Euler(0, -attackAngle, 0) * forward;

            Gizmos.DrawLine(transform.position, transform.position + right);
            Gizmos.DrawLine(transform.position, transform.position + left);
            Gizmos.DrawLine(transform.position, transform.position + forward);
        }
    }
    void AnimateSwing()
    {
        // Дополнительная анимация если нужна
    }
}