using AI.BT.Runtime;
using UnityEditor.Experimental.GraphView;

namespace AI.BT.Editor
{
    // Selector / Sequence 노드 설정
    public class BTCompositeNodeView : BTBaseNodeView
    {
        public BTCompositeNodeView(string name, BTNodeType type) : base(name)
        {
            title = name;
            NodeType = type;

            var input = CreateInputPort();
            input.portName = "In";
            inputContainer.Add(input);

            var output = CreateOutputPort(Port.Capacity.Multi);
            output.portName = "Children";
            outputContainer.Add(output);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}