using UnityEngine;
using Photon.Pun;

public class CoinCollectEffect : MonoBehaviourPunCallbacks
{
    [Header("Visual Effects")]
    public GameObject collectParticles;
    public float rotationSpeed = 1f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    private Vector3 startPosition;
    private Renderer coinRenderer;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
        coinRenderer = GetComponent<Renderer>();

        if (coinRenderer != null)
        {
            Material mat = coinRenderer.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.yellow * 0.5f);
        }
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, 1) * Time.deltaTime * rotationSpeed);

        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && !pv.IsMine)
            {
                return; // Не наш игрок
            }
            isCollected = true;

            if (collectParticles != null)
            {
                GameObject particles = Instantiate(collectParticles, transform.position, Quaternion.identity);

                Destroy(particles, 2f);
            }

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.AddCoin();
            }
            if (PhotonNetwork.IsConnected && photonView != null)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}