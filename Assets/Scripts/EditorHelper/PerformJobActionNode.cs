using UnityEngine;

[CreateAssetMenu(menuName="AI/Nodes/Action/Perform Job Action")]
public class PerformJobActionNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    protected override BTNodeState OnUpdate()
    {
        if (runner == null || runner.job == null) return BTNodeState.Failure;

        bool done = runner.job.PerformAction(runner, Time.deltaTime);
        return done ? BTNodeState.Success : BTNodeState.Running;
    }
}