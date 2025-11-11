using UnityEngine;

public class SpawnPointsGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int numberOfPoints = 10;
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(20f, 0f, 20f);
    public float height = 1.5f; // Поднял выше
    public float minDistance = 5f; // Увеличил расстояние
    public LayerMask obstacleLayer; // Для проверки препятствий

    [ContextMenu("Generate Spawn Points")]
    public void GenerateSpawnPoints()
    {
        // Удалить старые точки
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Создать новые
        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector3 position = GetRandomPosition();

            GameObject spawnPoint = new GameObject("SpawnPoint_" + (i + 1));
            spawnPoint.transform.parent = transform;
            spawnPoint.transform.position = position;

            Debug.Log("Created spawn point " + (i + 1) + " at " + position);
        }

        Debug.Log("Generated " + numberOfPoints + " spawn points!");
    }

    Vector3 GetRandomPosition()
    {
        int attempts = 0;
        int maxAttempts = 50;
        Vector3 position;

        do
        {
            float randomX = Random.Range(
                areaCenter.x - areaSize.x / 2,
                areaCenter.x + areaSize.x / 2
            );
            float randomZ = Random.Range(
                areaCenter.z - areaSize.z / 2,
                areaCenter.z + areaSize.z / 2
            );

            position = new Vector3(randomX, height, randomZ);
            attempts++;

            if (attempts >= maxAttempts)
                break;

        } while (IsTooClose(position));

        return position;
    }

    bool IsTooClose(Vector3 position)
    {
        foreach (Transform child in transform)
        {
            if (Vector3.Distance(position, child.position) < minDistance)
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(areaCenter, areaSize);

        // Показать существующие точки
        Gizmos.color = Color.yellow;
        foreach (Transform child in transform)
        {
            Gizmos.DrawWireSphere(child.position, 0.5f);
        }
    }
}