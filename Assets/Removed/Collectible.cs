using UnityEngine;

public class Collectible : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<CoinSpawner>().SpawnCoin();
            Destroy(gameObject);
            other.GetComponent<PlayerController>().AddCoin();
        }
    }
}