using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public Transform player;
    public float chaseDistance = 10f;
    public float attackDistance = 2f;
    public int damage = 15;
    public float attackCooldown = 1f;

    private int currentPoint = 0;
    private NavMeshAgent agent;
    private float lastAttackTime = 0f;

    void Start()
    {
        transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < attackDistance)
        {
            agent.SetDestination(transform.position);
            if (Time.time > lastAttackTime + attackCooldown)
            {
                player.GetComponent<PlayerController>().TakeDamage(damage);
                lastAttackTime = Time.time;
            }
        }
        else if (distanceToPlayer < chaseDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            if (agent.remainingDistance < 0.5f)
            {
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPoint].position);
            }
        }
    }
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time > lastAttackTime + attackCooldown)
            {
                other.GetComponent<PlayerController>().TakeDamage(damage);
                lastAttackTime = Time.time;
            }
        }
    }
}