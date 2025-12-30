using UnityEngine;

[CreateAssetMenu(menuName="AI/Nodes/Condition/Is Night")]
public class IsNightNode : ConditionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    protected override bool CheckCondition()
    {
        return runner != null && runner.routineState == RoutineState.Night;
    }
}