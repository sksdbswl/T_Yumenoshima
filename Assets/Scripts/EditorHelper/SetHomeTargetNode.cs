using UnityEngine;

[CreateAssetMenu(menuName="AI/Nodes/Action/Set Home Target")]
public class SetHomeTargetNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    protected override BTNodeState OnUpdate()
    {
        if (runner == null || runner.homeTarget == null) return BTNodeState.Failure;
        runner.currentTarget = runner.homeTarget;
        return BTNodeState.Success;
    }
}