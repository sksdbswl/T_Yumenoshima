using UnityEngine;

[CreateAssetMenu(menuName="AI/Nodes/Action/Perform Job Action")]
public class PerformJobActionNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    protected override BTNodeState OnUpdate()
    {
        if (runner == null) return BTNodeState.Failure;

        var job = runner.GetComponent<IJobHandler>();
        if (job == null) return BTNodeState.Failure;

        return job.PerformAction(runner, Time.deltaTime);
    }
}