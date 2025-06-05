using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

/// <summary>
/// Simple wandering AI for a dinosaur controlled on the server.
/// Randomly picks destinations on the NavMesh at a set interval.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class DinoWanderAI : NetworkBehaviour
{
    [Tooltip("Radius the dinosaur will roam within")]
    public float wanderRadius = 15f;

    [Tooltip("Seconds between selecting new wander destinations")]
    public float wanderInterval = 5f;

    private NavMeshAgent agent;
    private float timer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        timer += Time.deltaTime;
        if (timer >= wanderInterval)
        {
            timer = 0f;
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
            agent.SetDestination(newPos);
        }
    }

    private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, dist, layermask))
            return navHit.position;

        return origin;
    }
}
