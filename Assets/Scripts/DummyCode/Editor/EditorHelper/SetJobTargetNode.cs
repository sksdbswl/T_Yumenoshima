using UnityEngine;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Set Job Target")]
public class SetJobTargetNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    protected override BTNodeState OnUpdate()
    {
        var job = runner.GetComponent(typeof(IJobHandler)) as IJobHandler;
        if (job == null) return BTNodeState.Failure;

        var target = job.GetPriorityTarget(runner);
        runner.currentTarget = target;

        return target != null ? BTNodeState.Success : BTNodeState.Failure;
    }
}