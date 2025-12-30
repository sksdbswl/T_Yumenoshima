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
        
        Debug.Log($"[Patrol] agent null? {agent==null}, onNavMesh? {agent != null && agent.isOnNavMesh}, pos={runner.transform.position}");
        
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
        float radius = runner.profile != null ? runner.profile.patrolRadius : patrolRadius;

        Vector3 randomDir = Random.insideUnitSphere * radius + runner.transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            bool ok = agent.SetDestination(hit.position);
            Debug.Log($"[Patrol] dest={hit.position} setOk={ok}");
        }
        else
        {
            Debug.LogWarning($"[Patrol] SamplePosition FAILED radius={radius} from={runner.transform.position}");
        }
    }

}