using UnityEngine;

[CreateAssetMenu(menuName = "AI/Nodes/Sequence")]
public class SequenceNode : BTNode
{
    private int _current;

    protected override void OnStart()
    {
        //Debug.Log("Change SequenceNode OnStart");
        _current = 0;
    }

    protected override BTNodeState OnUpdate() 
    {
        if (children.Count == 0) return BTNodeState.Failure;

        while (_current < children.Count) // 2 OR 1
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

            // Success면 다음 자식으로 진행
            _current++;
        }

        // 전부 성공
        _current = 0;
        return BTNodeState.Success;
    }

    protected override void OnStop()
    {
        // 혹시라도 외부 Abort로 끊겼을 때를 대비
        _current = 0;
    }
}

