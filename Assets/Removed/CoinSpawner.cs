using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    //[Header("Spawn Settings")]
    public GameObject coinPrefab;
    public int totalCoins = 8;
    public float minDistanceBetweenCoins = 3f;

    //[Header("Spawn Area")]
    //public Vector3 spawnAreaCenter = Vector3.zero;
    //public Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);
    //public float spawnHeight = 0.5f;

    //private List<Vector3> spawnedPositions = new List<Vector3>();

    public int coinsToSpawn = 8;
    private int coinsSpawned = 0;

    public Transform[] spawnPoints;

    void Start()
    {
        SpawnCoin();
    }

    public void SpawnCoin()
    {
        if (coinsSpawned < coinsToSpawn)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            Instantiate(coinPrefab, spawnPoints[idx].position, Quaternion.Euler(90,0,0));
            coinsSpawned++;
        }
    }
}