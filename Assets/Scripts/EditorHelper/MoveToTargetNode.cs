using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Move To Target")]
public class MoveToTargetNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    public bool succeedWhenArrived = false;

    [Header("Stop Distance Override")]
    public bool overrideStopDistance = false;
    public float stopDistanceOverride = 1.5f;

    private NavMeshAgent agent;

    protected override void OnStart()
    {
        agent = runner.GetComponent<NavMeshAgent>();
    }

    protected override BTNodeState OnUpdate()
    {
        if (runner == null || agent == null) return BTNodeState.Failure;
        if (runner.currentTarget == null) return BTNodeState.Failure;

        float stop =
            overrideStopDistance
                ? stopDistanceOverride
                : (runner.profile != null
                    ? runner.profile.moveStopDistance
                    : agent.stoppingDistance);

        agent.stoppingDistance = stop;
        agent.SetDestination(runner.currentTarget.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return succeedWhenArrived ? BTNodeState.Success : BTNodeState.Running;
        }

        return BTNodeState.Running;
    }
}