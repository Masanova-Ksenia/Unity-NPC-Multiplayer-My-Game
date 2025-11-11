using System.Collections.Generic;
using UnityEngine;

public class CoinSpawnerFixed : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject coinPrefab;
    public int coinsToSpawn = 8;
    public Transform[] spawnPoints;

    private int coinsSpawned = 0;
    private List<int> usedSpawnPoints = new List<int>();

    void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn points не настроены!");
            return;
        }

        if (coinPrefab == null)
        {
            Debug.LogError("Coin Prefab не назначен!");
            return;
        }

        // Заспавнить все монеты сразу
        for (int i = 0; i < coinsToSpawn; i++)
        {
            SpawnCoin();
        }
    }

    public void SpawnCoin()
    {
        if (coinsSpawned >= coinsToSpawn)
        {
            Debug.Log("Все монеты заспавнены!");
            return;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("Нет точек спавна!");
            return;
        }

        // Найти свободную точку спавна
        int spawnIndex = GetRandomUnusedSpawnPoint();

        if (spawnIndex == -1)
        {
            // Все точки использованы, можем повторно использовать
            spawnIndex = Random.Range(0, spawnPoints.Length);
        }

        // Создать монету
        if (spawnPoints[spawnIndex] != null)
        {
            // ИСПРАВЛЕНО: правильный поворот монеты (стоя на ребре)
            GameObject coin = Instantiate(
                coinPrefab,
                spawnPoints[spawnIndex].position,
                Quaternion.Euler(90, 0, 0) // Вертикально, можно добавить случайный Y для разнообразия
            );

            // Или если хотите случайный поворот вокруг Y:
            // Quaternion.Euler(0, Random.Range(0f, 360f), 0)

            // Добавить тег если его нет
            if (!coin.CompareTag("Coin"))
            {
                coin.tag = "Coin";
            }

            coinsSpawned++;
            Debug.Log("Монета заспавнена в точке " + spawnIndex + ". Всего: " + coinsSpawned);
        }
        else
        {
            Debug.LogError("Spawn point " + spawnIndex + " is null!");
        }
    }

    int GetRandomUnusedSpawnPoint()
    {
        // Если все точки использованы, очистить список
        if (usedSpawnPoints.Count >= spawnPoints.Length)
        {
            usedSpawnPoints.Clear();
        }

        // Найти неиспользованную точку
        int attempts = 0;
        int maxAttempts = 50;

        while (attempts < maxAttempts)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);

            if (!usedSpawnPoints.Contains(randomIndex))
            {
                usedSpawnPoints.Add(randomIndex);
                return randomIndex;
            }

            attempts++;
        }

        // Не нашли свободную точку
        return -1;
    }

    // Вызывается когда монета подобрана
    public void OnCoinCollected()
    {
        // Можно добавить логику пересоздания монеты
        // SpawnCoin();
    }

    // Визуализация точек спавна в редакторе
    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                    Gizmos.DrawLine(point.position, point.position + Vector3.up * 2f);
                }
            }
        }
    }
}