using UnityEngine;
using System.Collections.Generic;

public class ObstacleGenerator : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs; // Массив префабов препятствий (кубы, стены)
    public int numberOfObstacles = 50;
    public Vector3 spawnAreaCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new Vector3(30f, 0f, 30f);
    public float minDistanceBetweenObstacles = 2f;
    //public float obstacleHeight = 0.5f;

    [Header("Grid Distribution")]
    public bool useGridDistribution = true; // НОВОЕ: равномерное распределение
    public int gridSizeX = 8; // НОВОЕ: 8x8 сетка
    public int gridSizeZ = 8;

    [Header("Player Safe Zone")]
    public Transform player;
    public float playerSafeRadius = 8f; // Не спавнить препятствия рядом с игроком

    private List<Vector3> spawnedPositions = new List<Vector3>();

    [Header("Platform Protection")]
    public LayerMask platformLayer; // Назначьте Layer платформ
    public float platformCheckRadius = 1f;

    void Start()
    {
        GenerateObstacles();
    }

    void GenerateObstacles()
    {
        spawnedPositions.Clear();

        if (useGridDistribution)
        {
            GenerateObstaclesWithGrid();
        }
        else
        {
            GenerateObstaclesRandom();
        }
    }
    void GenerateObstaclesRandom()
    {
        for (int i = 0; i < numberOfObstacles; i++)
        {
            Vector3 spawnPosition = GetRandomUniquePosition();

            if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
            {
                CreateObstacle(spawnPosition, i);
            }
        }

        Debug.Log("Случайная генерация завершена: " + numberOfObstacles + " препятствий");
    }

    void CreateObstacle(Vector3 position, int index)
    {
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // УМЕНЬШЕННЫЙ масштаб для проходимости
        Vector3 scale = new Vector3(
            Random.Range(1f, 2.5f),  // Уменьшено для проходов
            Random.Range(2f, 5f),
            Random.Range(1f, 2.5f)
        );

        GameObject obstacle = Instantiate(prefab, new Vector3(position.x, 0, position.z), rotation);
        obstacle.transform.localScale = scale;
        obstacle.name = "Obstacle_" + index;

        // Выравнивание по полу
        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer != null)
        {
            float halfHeight = renderer.bounds.extents.y;
            obstacle.transform.position = new Vector3(position.x, halfHeight, position.z);
        }

        spawnedPositions.Add(position);
    }
    void GenerateObstaclesWithGrid()
    {
        // Создать сетку позиций
        List<Vector3> gridPositions = new List<Vector3>();

        float cellWidth = spawnAreaSize.x / gridSizeX;
        float cellDepth = spawnAreaSize.z / gridSizeZ;

        float startX = spawnAreaCenter.x - spawnAreaSize.x / 2;
        float startZ = spawnAreaCenter.z - spawnAreaSize.z / 2;

        // Заполнить сетку
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // Центр ячейки с небольшой рандомизацией
                float posX = startX + (x + 0.5f) * cellWidth + Random.Range(-cellWidth * 0.3f, cellWidth * 0.3f);
                float posZ = startZ + (z + 0.5f) * cellDepth + Random.Range(-cellDepth * 0.3f, cellDepth * 0.3f);

                Vector3 gridPos = new Vector3(posX, 0, posZ);

                // Проверить безопасность позиции
                if (!IsNearPlayer(gridPos) && !IsNearPlatform(gridPos))
                {
                    gridPositions.Add(gridPos);
                }
            }
        }

        // Перемешать позиции
        ShuffleList(gridPositions);

        // Создать препятствия из перемешанных позиций
        int created = 0;
        foreach (Vector3 pos in gridPositions)
        {
            if (created >= numberOfObstacles) break;

            if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
            {
                CreateObstacle(pos, created);
                created++;
            }
        }

        Debug.Log("Генерация с сеткой завершена: " + created + " препятствий");
    }
    Vector3 GetRandomUniquePosition()
    {
        Vector3 randomPosition;
        int attempts = 0;
        int maxAttempts = 100;

        do
        {
            float randomX = Random.Range(
                spawnAreaCenter.x - spawnAreaSize.x / 2,
                spawnAreaCenter.x + spawnAreaSize.x / 2
            );
            float randomZ = Random.Range(
                spawnAreaCenter.z - spawnAreaSize.z / 2,
                spawnAreaCenter.z + spawnAreaSize.z / 2
            );

            randomPosition = new Vector3(randomX, 0, randomZ);
            attempts++;

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Не удалось найти уникальную позицию после " + maxAttempts + " попыток");
                break;
            }

        } while (IsTooClose(randomPosition) || IsNearPlayer(randomPosition) || IsNearPlatform(randomPosition));

        return randomPosition;
    }

    bool IsTooClose(Vector3 position)
    {
        foreach (Vector3 spawnedPos in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minDistanceBetweenObstacles)
            {
                return true;
            }
        }
        return false;
    }

    bool IsNearPlayer(Vector3 position)
    {
        if (player == null) return false;

        return Vector3.Distance(position, player.position) < playerSafeRadius;
    }

    bool IsNearPlatform(Vector3 position)
    {
        // Проверка на наличие платформ рядом
        Collider[] nearbyPlatforms = Physics.OverlapSphere(position, platformCheckRadius, platformLayer);
        if (nearbyPlatforms.Length > 0)
        {
            Debug.Log("Позиция слишком близко к платформе");
            return true;
        }
        return false;
    }

    void ShuffleList(List<Vector3> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector3 temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);

        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, playerSafeRadius);
        }
        if (useGridDistribution)
        {
            Gizmos.color = Color.cyan;
            float cellWidth = spawnAreaSize.x / gridSizeX;
            float cellDepth = spawnAreaSize.z / gridSizeZ;
            float startX = spawnAreaCenter.x - spawnAreaSize.x / 2;
            float startZ = spawnAreaCenter.z - spawnAreaSize.z / 2;

            for (int x = 0; x <= gridSizeX; x++)
            {
                Vector3 start = new Vector3(startX + x * cellWidth, 0.5f, startZ);
                Vector3 end = new Vector3(startX + x * cellWidth, 0.5f, startZ + spawnAreaSize.z);
                Gizmos.DrawLine(start, end);
            }

            for (int z = 0; z <= gridSizeZ; z++)
            {
                Vector3 start = new Vector3(startX, 0.5f, startZ + z * cellDepth);
                Vector3 end = new Vector3(startX + spawnAreaSize.x, 0.5f, startZ + z * cellDepth);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}