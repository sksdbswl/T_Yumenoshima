using UnityEngine;

namespace TestBT
{
    public class BehaviorTreeRunner
    {
        // 루트 노드 실행기
        private readonly INode _rootNode;

        public BehaviorTreeRunner(INode rootNode)
        {
            _rootNode = rootNode;
        }

        public void Operate()
        {
            _rootNode?.Evaluate();
        }
    }
}

