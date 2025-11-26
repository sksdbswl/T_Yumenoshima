using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Patrol")]
public class PatrolNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;
    private NavMeshAgent agent;
    public float patrolRadius = 5f;

    protected override void OnStart()
    {
        agent = runner.GetComponent<NavMeshAgent>();
        if (agent != null)
            SetRandomDestination();
    }

    protected override BTNodeState OnUpdate()
    {
        if (agent == null) return BTNodeState.Failure;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("Reached destination");
            SetRandomDestination();
        }

        return BTNodeState.Running;
    }

    private void SetRandomDestination()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius + runner.transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}