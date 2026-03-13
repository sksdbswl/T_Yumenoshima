using System;
using AI.BT.Runtime;

/// <summary>
/// Action Node는 실제로 어떤 행위를 하는 노드
/// 그렇기 때문에 Func() 델리게이트를 통해 행위를 전달받아 실행
/// </summary>
public sealed class BTActionNode : INode
{
    public string Guid { get; }
    private readonly Func<ENodeState> _onUpdate;

    public BTActionNode(string guid, Func<ENodeState> onUpdate)
    {
        Guid = guid;
        _onUpdate = onUpdate;
    }
    
    public ENodeState Evaluate()
    {
        BTEditorDebugger.SetActive(Guid);

        return _onUpdate?.Invoke() ?? ENodeState.ENS_Failure;
    }
}
