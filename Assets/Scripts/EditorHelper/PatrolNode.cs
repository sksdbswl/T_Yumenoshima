using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Patrol")]
public class PatrolNode : ActionNode
{
    private static readonly int Walk = Animator.StringToHash("Walk");
    
    [System.NonSerialized] public BehaviourTreeRunner runner;
    private NavMeshAgent agent;
    public float patrolRadius = 5f;

    protected override void OnStart()
    {
        Debug.Log("PatrolNode OnStart :: 순찰 중");
        
        agent = runner.GetComponent<NavMeshAgent>();
        if (agent != null)
            SetRandomDestination();
        
        runner.GetComponent<Animator>()?.Play(Walk);
    }

    protected override BTNodeState OnUpdate()
    {
        if (agent == null) return BTNodeState.Failure;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
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