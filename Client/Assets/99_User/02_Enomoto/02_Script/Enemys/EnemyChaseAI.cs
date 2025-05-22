using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseAI : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] float distance;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void DoChase(GameObject target)
    {
        float now = Vector2.Distance(transform.position, target.transform.position);
        if (distance < now)
        {

        }
        agent.destination = target.transform.position;
        Vector2? vector = new Vector2(
            agent.destination.x - transform.position.x,
            agent.destination.y - transform.position.y
        );
    }
}
