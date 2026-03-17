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
        var radius = runner.profile != null ? runner.profile.patrolRadius : patrolRadius;
        var randomDir = Random.insideUnitSphere * radius + runner.transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            bool ok = agent.SetDestination(hit.position);
            //Debug.Log($"[Patrol] dest={hit.position} setOk={ok}");
        }
    }

}