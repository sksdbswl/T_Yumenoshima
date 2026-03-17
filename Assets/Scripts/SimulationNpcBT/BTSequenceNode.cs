using System.Collections.Generic;
using AI.BT.Runtime;

public sealed class BTSequenceNode : INode
{
    public string Guid { get; }
    List<INode> _childs;

    public BTSequenceNode(string guid,List<INode> childs)
    {
        Guid = guid;
        _childs = childs;
    }

    public ENodeState Evaluate()
    {
        if (_childs == null || _childs.Count == 0)
            return ENodeState.ENS_Failure;

        foreach (var child in _childs)
        {
            switch (child.Evaluate())
            {
                case ENodeState.ENS_Running:
                    return ENodeState.ENS_Running;
                case ENodeState.ENS_Success:
                    continue;
                case ENodeState.ENS_Failure:
                    return ENodeState.ENS_Failure;
            }
        }

        return ENodeState.ENS_Success;
    }
}

