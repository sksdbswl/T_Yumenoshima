using AI.BT.Runtime;
using UnityEditor.Experimental.GraphView;

namespace AI.BT.Editor
{
    // 루트는 보통 input이 없고 output 하나만 있으면 됩니다.
    public class BTRootNodeView : BTBaseNodeView
    {
        public BTRootNodeView()
        {
            title = "Root";
            NodeType = BTNodeType.Root;

            var output = CreateOutputPort(Port.Capacity.Single);
            output.portName = "Child";
            outputContainer.Add(output);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}