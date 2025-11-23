using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{
    [Header("References")]
    public PlayerWeapon playerWeapon; // Ссылка на оружие игрока

    [Header("Crosshair Settings")]
    public Color normalColor = Color.white;
    public Color enemyColor = Color.red;
    public Color canAttackColor = Color.green; // Новый: в радиусе атаки
    public float lineLength = 10f;
    public float lineThickness = 2f;

    private GameObject crosshairContainer;
    private Image[] crosshairLines;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Найти оружие если не назначено
        if (playerWeapon == null)
        {
            playerWeapon = FindObjectOfType<PlayerWeapon>();
        }

        CreateCrosshair();
    }

    void CreateCrosshair()
    {
        // Контейнер
        crosshairContainer = new GameObject("CrosshairContainer");
        crosshairContainer.transform.SetParent(transform, false);

        RectTransform containerRect = crosshairContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(40, 40);
        containerRect.anchoredPosition = Vector2.zero;

        // 4 линии
        crosshairLines = new Image[4];

        crosshairLines[0] = CreateLine("LineTop", lineThickness, lineLength, new Vector2(0, lineLength / 2 + 5));
        crosshairLines[1] = CreateLine("LineBottom", lineThickness, lineLength, new Vector2(0, -(lineLength / 2 + 5)));
        crosshairLines[2] = CreateLine("LineLeft", lineLength, lineThickness, new Vector2(-(lineLength / 2 + 5), 0));
        crosshairLines[3] = CreateLine("LineRight", lineLength, lineThickness, new Vector2(lineLength / 2 + 5, 0));
    }

    Image CreateLine(string name, float width, float height, Vector2 position)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(crosshairContainer.transform, false);

        RectTransform rect = lineObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = position;

        Image image = lineObj.AddComponent<Image>();
        image.color = normalColor;

        return image;
    }

    void Update()
    {
        if (playerWeapon == null) return;

        float attackRange = playerWeapon.attackRange;
        float attackAngle = playerWeapon.attackAngle;

        Vector3 rayOrigin = mainCamera.transform.position;
        Vector3 rayDirection = mainCamera.transform.forward;
        Vector3 playerPos = transform.position;

        bool foundNPC = false;
        bool inAttackRange = false;

        // РАСШИРЕННАЯ ПРОВЕРКА - несколько методов одновременно

        // Метод 1: Raycast из камеры
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange * 1.5f)) // Увеличен радиус проверки
        {
            NPCController npc = hit.collider.GetComponent<NPCController>();
            if (npc != null)
            {
                foundNPC = true;
                inAttackRange = true;
                Debug.DrawLine(rayOrigin, hit.point, Color.green, 0.05f);
            }
        }

        // Метод 2: SphereCast (толстый луч)
        if (!foundNPC)
        {
            if (Physics.SphereCast(rayOrigin, 0.5f, rayDirection, out hit, attackRange))
            {
                NPCController npc = hit.collider.GetComponent<NPCController>();
                if (npc != null)
                {
                    foundNPC = true;
                    inAttackRange = true;
                }
            }
        }

        // Метод 3: OverlapSphere вокруг игрока
        if (!foundNPC)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(playerPos, attackRange);

            foreach (Collider col in nearbyColliders)
            {
                NPCController npc = col.GetComponent<NPCController>();
                if (npc != null)
                {
                    Vector3 dirToNPC = (npc.transform.position - playerPos).normalized;
                    float angle = Vector3.Angle(rayDirection, dirToNPC);

                    if (angle < attackAngle * 1.2f) // Увеличен угол проверки на 20%
                    {
                        foundNPC = true;

                        float distance = Vector3.Distance(playerPos, npc.transform.position);
                        if (distance <= attackRange * 0.8f)
                        {
                            inAttackRange = true;
                        }

                        Debug.DrawLine(playerPos, npc.transform.position, Color.yellow, 0.05f);
                        break;
                    }
                }
            }
        }

        // Выбор цвета
        Color targetColor = normalColor;

        if (foundNPC)
        {
            if (inAttackRange)
            {
                targetColor = canAttackColor; // Зелёный
            }
            else
            {
                targetColor = enemyColor; // Красный
            }
        }

        // БЫСТРЫЙ переход цвета (было медленно)
        foreach (Image line in crosshairLines)
        {
            if (line != null)
            {
                line.color = Color.Lerp(line.color, targetColor, Time.deltaTime * 25f); // Увеличено с 15f
            }
        }
    }
}