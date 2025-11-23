using System.Collections.Generic;
using UnityEngine;

public class CoinSpawnerDynamic : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject coinPrefab;
    public int coinsPerPlayer = 8;
    public Vector3 spawnAreaCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new Vector3(100f, 0f, 100f);
    public float coinHeight = 2.5f; // ПОВЫШЕНО с 1.5 до 2.5 (выше платформ!)
    public float minDistanceBetweenCoins = 8f;
    public float minDistanceFromObstacles = 2f;

    [Header("Multiplayer")]
    public int numberOfPlayers = 1;

    [Header("Debug")]
    public bool showDebugSpheres = false;

    private List<Vector3> spawnedCoinPositions = new List<Vector3>();
    private List<GameObject> spawnedCoins = new List<GameObject>();

    void Start()
    {
        GenerateCoins();
    }

    public void GenerateCoins()
    {
        ClearOldCoins();

        int totalCoins = coinsPerPlayer * numberOfPlayers;

        Debug.Log("=== Генерация монет ===");
        Debug.Log("Карта: " + spawnAreaSize.x + "x" + spawnAreaSize.z);
        Debug.Log("Монет: " + totalCoins);

        List<Vector3> availableZones = CreateZones();

        int successfulSpawns = 0;

        for (int i = 0; i < totalCoins; i++)
        {
            Vector3 coinPos = GetRandomSafePosition(availableZones);

            if (coinPrefab != null)
            {
                GameObject coin = Instantiate(
                    coinPrefab,
                    coinPos,
                    Quaternion.identity
                );

                if (!coin.CompareTag("Coin"))
                {
                    coin.tag = "Coin";
                }

                spawnedCoins.Add(coin);
                spawnedCoinPositions.Add(coinPos);
                successfulSpawns++;
            }
        }

        Debug.Log("Создано монет: " + successfulSpawns + "/" + totalCoins);
    }

    List<Vector3> CreateZones()
    {
        List<Vector3> zones = new List<Vector3>();

        int gridSize = 10; // 10x10 сетка
        float zoneWidth = spawnAreaSize.x / gridSize;
        float zoneDepth = spawnAreaSize.z / gridSize;

        float startX = spawnAreaCenter.x - spawnAreaSize.x / 2f;
        float startZ = spawnAreaCenter.z - spawnAreaSize.z / 2f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 zoneCenter = new Vector3(
                    startX + (x + 0.5f) * zoneWidth,
                    coinHeight,
                    startZ + (z + 0.5f) * zoneDepth
                );
                zones.Add(zoneCenter);
            }
        }

        // Перемешать
        for (int i = 0; i < zones.Count; i++)
        {
            Vector3 temp = zones[i];
            int randomIndex = Random.Range(i, zones.Count);
            zones[i] = zones[randomIndex];
            zones[randomIndex] = temp;
        }

        return zones;
    }

    Vector3 GetRandomSafePosition(List<Vector3> preferredZones)
    {
        Vector3 position;
        int attempts = 0;
        int maxAttempts = 200; // УМЕНЬШЕНО для скорости

        do
        {
            if (preferredZones != null && preferredZones.Count > 0 && attempts < 100)
            {
                Vector3 zone = preferredZones[attempts % preferredZones.Count];
                float zoneRadius = spawnAreaSize.x / 20f;

                position = new Vector3(
                    zone.x + Random.Range(-zoneRadius, zoneRadius),
                    coinHeight,
                    zone.z + Random.Range(-zoneRadius, zoneRadius)
                );
            }
            else
            {
                // Случайная позиция
                float randomX = Random.Range(
                    spawnAreaCenter.x - spawnAreaSize.x / 2f,
                    spawnAreaCenter.x + spawnAreaSize.x / 2f
                );
                float randomZ = Random.Range(
                    spawnAreaCenter.z - spawnAreaSize.z / 2f,
                    spawnAreaCenter.z + spawnAreaSize.z / 2f
                );

                position = new Vector3(randomX, coinHeight, randomZ);
            }

            attempts++;

            if (attempts >= maxAttempts)
            {
                // Просто вернуть текущую позицию без проверок
                Debug.LogWarning("Использована позиция после " + maxAttempts + " попыток");
                break;
            }

        } while (IsTooCloseToOtherCoins(position) || HasObstacleDirectlyBelow(position));

        return position;
    }

    bool IsTooCloseToOtherCoins(Vector3 position)
    {
        foreach (Vector3 coinPos in spawnedCoinPositions)
        {
            float distance = Vector3.Distance(position, coinPos);
            if (distance < minDistanceBetweenCoins)
            {
                return true;
            }
        }
        return false;
    }

    // НОВЫЙ МЕТОД: Проверка препятствия ПРЯМО ПОД монетой (не вокруг!)
    bool HasObstacleDirectlyBelow(Vector3 position)
    {
        // Raycast вниз от позиции монеты
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, coinHeight + 2f))
        {
            // Если под монетой препятствие или стена - это OK
            // Игнорируем только если это триггер
            if (hit.collider.isTrigger)
            {
                return true;
            }
        }

        return false;
    }

    void ClearOldCoins()
    {
        foreach (GameObject coin in spawnedCoins)
        {
            if (coin != null)
            {
                Destroy(coin);
            }
        }

        spawnedCoins.Clear();
        spawnedCoinPositions.Clear();
    }

    public void OnPlayerJoined()
    {
        numberOfPlayers++;
        GenerateCoins();
    }

    void OnDrawGizmosSelected()
    {
        // Область спавна
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);

        // Сетка
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        int gridSize = 10;
        float zoneWidth = spawnAreaSize.x / gridSize;
        float zoneDepth = spawnAreaSize.z / gridSize;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 zoneCenter = new Vector3(
                    spawnAreaCenter.x - spawnAreaSize.x / 2f + (x + 0.5f) * zoneWidth,
                    coinHeight,
                    spawnAreaCenter.z - spawnAreaSize.z / 2f + (z + 0.5f) * zoneDepth
                );
                Gizmos.DrawWireCube(zoneCenter, new Vector3(zoneWidth, 1, zoneDepth));
            }
        }

        // Позиции монет
        if (showDebugSpheres)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 coinPos in spawnedCoinPositions)
            {
                Gizmos.DrawWireSphere(coinPos, 1f);
            }
        }
    }
}