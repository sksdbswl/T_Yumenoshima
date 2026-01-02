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

        // Non-memory: 매 Tick마다 처음부터 평가
        for (int i = 0; i < children.Count; i++)
        {
            var result = children[i].Tick();

            if (result == BTNodeState.Running)
                return BTNodeState.Running;

            if (result == BTNodeState.Failure)
                return BTNodeState.Failure;
        }

        return BTNodeState.Success;
    }

    public override void AbortRunningBranch()
    {
        // 실행중인 자식이 있을 수 있으니 전부 안전 종료
        for (int i = 0; i < children.Count; i++)
            children[i].AbortRunningBranch();

        Abort();
    }

    
    // protected override BTNodeState OnUpdate() 
    // {
    //     if (children.Count == 0) return BTNodeState.Failure;
    //
    //     while (_current < children.Count) // 2 OR 1
    //     {
    //         var child = children[_current];
    //         var result = child.Tick();
    //
    //         if (result == BTNodeState.Running)
    //             return BTNodeState.Running;
    //
    //         if (result == BTNodeState.Failure)
    //         {
    //             _current = 0;
    //             return BTNodeState.Failure;
    //         }
    //
    //         // Success면 다음 자식으로 진행
    //         _current++;
    //     }
    //
    //     // 전부 성공
    //     _current = 0;
    //     return BTNodeState.Success;
    // }

    protected override void OnStop()
    {
        // 혹시라도 외부 Abort로 끊겼을 때를 대비
        _current = 0;
    }

    // public override void AbortRunningBranch()
    // {
    //     // 지금 진행 중인 자식이 있으면 그쪽만 끊기
    //     if (_current >= 0 && _current < children.Count)
    //         children[_current].AbortRunningBranch();
    //
    //     // 그리고 나도 끊기
    //     Abort();
    // }
}

