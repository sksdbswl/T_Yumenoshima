using System.Collections.Generic;
using AI.BT.Runtime;

public sealed class BTSelectorNode : INode
{
    public string Guid { get; }
    
    List<INode> _childs;
    private INode _nodeImplementation;

    public BTSelectorNode(string guid, List<INode> childs)
    {
        Guid = guid;
        _childs = childs;
    }
    
    public ENodeState Evaluate()
    {
        if (_childs == null)
            return ENodeState.ENS_Failure;

        foreach (var child in _childs)
        {
            switch (child.Evaluate())
            {
                case ENodeState.ENS_Running:
                    return ENodeState.ENS_Running;
                case ENodeState.ENS_Success:
                    return ENodeState.ENS_Success;
            }
        }

        return ENodeState.ENS_Failure;
    }
}