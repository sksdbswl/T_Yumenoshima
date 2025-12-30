using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Move To Target")]
public class MoveToTargetNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;
    private NavMeshAgent agent;

    public bool succeedWhenArrived = false; // 필요하면 true로

    protected override void OnStart()
    {
        agent = runner.GetComponent<NavMeshAgent>();
    }

    protected override BTNodeState OnUpdate()
    {
        if (runner == null || agent == null) return BTNodeState.Failure;

        var target = runner.currentTarget;
        if (target == null) return BTNodeState.Failure;

        float stop = runner.profile != null ? runner.profile.moveStopDistance : agent.stoppingDistance;
        
        agent.stoppingDistance = stop;
        agent.SetDestination(target.position);

        // 도착 체크
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return succeedWhenArrived ? BTNodeState.Success : BTNodeState.Running;
        }

        return BTNodeState.Running;
    }
}