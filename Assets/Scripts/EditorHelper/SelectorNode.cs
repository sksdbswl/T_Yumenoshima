using UnityEngine;

[CreateAssetMenu(menuName = "AI/Nodes/Selector")]
public class SelectorNode : BTNode
{
    private BTNode _runningChild;

    protected override void OnStart()
    {
        Debug.Log("SelectorNode OnStart");
        
        _runningChild = null;
    }

    private BTNode _active;
    
    protected override BTNodeState OnUpdate()
    {
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var result = child.Tick();

            if (result == BTNodeState.Failure)
                continue;

            // 선택이 바뀌면 이전 행동을 강제 종료
            if (_active != null && _active != child)
                _active.AbortRunningBranch();

            _active = (result == BTNodeState.Running) ? child : null;
            return result;
        }

        if (_active != null)
        {
            _active.AbortRunningBranch();
            _active = null;
        }
        
        return BTNodeState.Failure;
    }

    public override void AbortRunningBranch()
    {
        if (_active != null)
            _active.AbortRunningBranch();

        Abort();
    }
}
