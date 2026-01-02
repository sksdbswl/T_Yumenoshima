using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Move To Target")]
public class MoveToTargetNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;
    private NavMeshAgent agent;

    [Header("Arrival")]
    public bool succeedWhenArrived = true;

    [Header("Stop Distance Override")]
    public bool overrideStopDistance = true;
    public float stopDistanceOverride = 1.5f;

    protected override void OnStart()
    {
        if (runner == null)
        {
            Debug.LogError("[MoveToTargetNode] runner is NULL (binding missing)");
            return;
        }

        agent = runner.GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("[MoveToTargetNode] NavMeshAgent missing on runner GameObject");
    }

    protected override BTNodeState OnUpdate()
    {
        var target = runner.currentTarget;
        if (target == null) return BTNodeState.Failure;

        float stop = overrideStopDistance
            ? stopDistanceOverride
            : (runner.profile != null ? runner.profile.moveStopDistance : agent.stoppingDistance);

        agent.stoppingDistance = stop;
        agent.SetDestination(target.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            return succeedWhenArrived ? BTNodeState.Success : BTNodeState.Running;

        return BTNodeState.Running;
    }

    protected override void OnStop()
    {
        // 원하면 여기서 정지/애니 처리 가능
    }
}