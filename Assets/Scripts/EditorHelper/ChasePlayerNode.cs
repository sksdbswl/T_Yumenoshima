using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Chase Player")]
public class ChasePlayerNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;
    private NavMeshAgent agent;
    public float stopDistance = 1.5f;

    protected override void OnStart()
    {
        agent = runner.GetComponent<NavMeshAgent>();
    }

    protected override BTNodeState OnUpdate()
    {
        if (agent == null) return BTNodeState.Failure;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return BTNodeState.Failure;

        agent.stoppingDistance = stopDistance;
        agent.SetDestination(player.transform.position);

        float dist = Vector3.Distance(runner.transform.position, player.transform.position);
        if (dist <= stopDistance)
        {
            return BTNodeState.Success; // 공격 시퀀스로 넘어가게
        }

        return BTNodeState.Running;
    }
}