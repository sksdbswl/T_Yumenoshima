using UnityEngine;

[CreateAssetMenu(menuName = "AI/Nodes/Condition/Is Target In Interaction Range")]
public class IsTargetInInteractionRangeNode : ConditionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    protected override bool CheckCondition()
    {
        if (runner == null || runner.profile == null) return false;

        var target = runner.currentTarget; // SetJobTargetNode가 잡아준 타겟
        if (target == null) return false;

        float d = Vector3.Distance(runner.transform.position, target.position);
        return d <= runner.profile.interactionRange;
    }
}