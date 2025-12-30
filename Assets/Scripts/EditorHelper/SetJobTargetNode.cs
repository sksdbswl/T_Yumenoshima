using UnityEngine;

[CreateAssetMenu(menuName="AI/Nodes/Action/Set Job Target")]
public class SetJobTargetNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    protected override BTNodeState OnUpdate()
    {
        if (runner == null || runner.job == null) return BTNodeState.Failure;

        runner.currentTarget = runner.job.GetPriorityTarget(runner);
        return runner.currentTarget != null ? BTNodeState.Success : BTNodeState.Failure;
    }
}