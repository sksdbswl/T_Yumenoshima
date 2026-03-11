using System;
using AI.BT.Runtime;

namespace TestBT
{
    /// <summary>
    /// NpcBlackboard의 값만 확인하고 상태값만 반환하도록 구현
    /// </summary>
    public sealed class BTConditionNode : INode
    {
        public string Guid { get; }
        private readonly Func<bool> _condition;
        private INode _nodeImplementation;

        public BTConditionNode(string guid, Func<bool> condition)
        {
            _condition = condition;
        }

        public ENodeState Evaluate()
        {
            BTEditorDebugger.SetActive(Guid);
            
            return _condition != null && _condition.Invoke()
                ? ENodeState.ENS_Success
                : ENodeState.ENS_Failure;
        }
    }
}
