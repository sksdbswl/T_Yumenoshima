using UnityEngine;

[CreateAssetMenu(menuName="AI/Nodes/Condition/Is Night")]
public class IsNightNode : ConditionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    protected override bool CheckCondition()
    {
        Debug.Log($"[IsNightNode] routine={runner?.routineState}");
        return runner != null && runner.routineState == RoutineState.Night;
    }
}