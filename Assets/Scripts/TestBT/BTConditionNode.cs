using System;

namespace TestBT
{
    /// <summary>
    /// NpcBlackboard의 값만 확인하고 상태값만 반환하도록 구현
    /// </summary>
    public sealed class BTConditionNode : INode
    {
        private readonly Func<bool> _condition;

        public BTConditionNode(Func<bool> condition)
        {
            _condition = condition;
        }

        public INode.ENodeState Evaluate()
        {
            return _condition != null && _condition.Invoke()
                ? INode.ENodeState.ENS_Success
                : INode.ENodeState.ENS_Failure;
        }
    }
}
