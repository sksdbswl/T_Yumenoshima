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

    protected override BTNodeState OnUpdate()
    {
        if (children.Count == 0) return BTNodeState.Failure;

        // Reactive: 매 Tick마다 0부터 우선순위 재평가
        for (int i = 0; i < children.Count; i++) //3
        {
            var child = children[i];
            var result = child.Tick();

            // child node 상태가 아님 : 이번 tick에서 실행할 수 있는 노드가 아님
            if (result == BTNodeState.Failure)
                continue;

            // 선택된 child가 바뀌었고, 이전 child가 Running 중이었다면 끊어준다
            if (_runningChild != null && _runningChild != child)
            {
                _runningChild.Abort();
            }

            // Running Node 재선택
            _runningChild = (result == BTNodeState.Running) ? child : null;
            return result; // Running 또는 Success
        }

        // 아무 것도 성공/진행 못했으면: 이전 Running이 있었다면 끊고 Failure
        if (_runningChild != null)
        {
            _runningChild.Abort();
            _runningChild = null;
        }

        return BTNodeState.Failure;
    }
}
