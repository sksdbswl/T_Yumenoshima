using System;

/// <summary>
/// Action Node는 실제로 어떤 행위를 하는 노드
/// 그렇기 때문에 Func() 델리게이트를 통해 행위를 전달받아 실행
/// </summary>
public sealed class BTActionNode : INode
{
    private readonly Func<INode.ENodeState> _onUpdate;

    public BTActionNode(Func<INode.ENodeState> onUpdate)
    {
        _onUpdate = onUpdate;
    }

    public INode.ENodeState Evaluate() => _onUpdate?.Invoke() ?? INode.ENodeState.ENS_Failure;
}
