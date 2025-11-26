using UnityEngine;

[CreateAssetMenu(menuName = "AI/Nodes/Sequence")]
public class SequenceNode : BTNode
{
    private int _current;

    protected override void OnStart()
    {
        _current = 0;
    }

    protected override BTNodeState OnUpdate()
    {
        if (children.Count == 0) return BTNodeState.Failure;

        while (_current < children.Count)
        {
            var child = children[_current];
            var result = child.Tick();

            if (result == BTNodeState.Running)
                return BTNodeState.Running;

            if (result == BTNodeState.Failure)
            {
                _current = 0;
                return BTNodeState.Failure;
            }

            // Success면 다음 자식
            _current++;
        }

        _current = 0;
        return BTNodeState.Success;
    }
}