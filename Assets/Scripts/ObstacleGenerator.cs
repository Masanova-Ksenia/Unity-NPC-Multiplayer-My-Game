using UnityEngine;
using System.Collections.Generic;

public class ObstacleGenerator : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs; // Массив префабов препятствий (кубы, стены)
    public int numberOfObstacles = 15;
    public Vector3 spawnAreaCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new Vector3(30f, 0f, 30f);
    public float minDistanceBetweenObstacles = 5f;
    public float obstacleHeight = 1f;

    [Header("Player Safe Zone")]
    public Transform player;
    public float playerSafeRadius = 8f; // Не спавнить препятствия рядом с игроком

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        GenerateObstacles();
    }

    void GenerateObstacles()
    {
        spawnedPositions.Clear();

        for (int i = 0; i < numberOfObstacles; i++)
        {
            Vector3 spawnPosition = GetRandomUniquePosition();

            if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
            {
                // Случайный префаб
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

                // Случайный поворот
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                // Случайный масштаб
                Vector3 scale = new Vector3(
                    Random.Range(0.8f, 2f),
                    Random.Range(1f, 3f),
                    Random.Range(0.8f, 2f)
                );

                GameObject obstacle = Instantiate(prefab, spawnPosition, rotation);
                obstacle.transform.localScale = scale;
                obstacle.name = "Obstacle_" + i;

                spawnedPositions.Add(spawnPosition);

                Debug.Log("Создано препятствие " + i + " в позиции " + spawnPosition);
            }
        }

        Debug.Log("Генерация завершена: " + numberOfObstacles + " препятствий");
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

            randomPosition = new Vector3(randomX, obstacleHeight, randomZ);
            attempts++;

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Не удалось найти уникальную позицию после " + maxAttempts + " попыток");
                break;
            }

        } while (IsTooClose(randomPosition) || IsNearPlayer(randomPosition));

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
    }
}