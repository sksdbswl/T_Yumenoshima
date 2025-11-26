using UnityEngine;

public abstract class ConditionNode : BTNode
{
    protected sealed override BTNodeState OnUpdate()
    {
        return CheckCondition() ? BTNodeState.Success : BTNodeState.Failure;
    }

    protected abstract bool CheckCondition();
}