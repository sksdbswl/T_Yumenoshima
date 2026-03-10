using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using AI.BT.Runtime;

namespace AI.BT.Editor
{
    // Condition node 설정
    // Condition은 enum popup 하나만 있어도 충분히 시작 가능합니다.
    public class BTConditionNodeView : BTBaseNodeView
    {
        public BTConditionType ConditionType { get; private set; }

        public BTConditionNodeView()
        {
            title = "Condition";
            NodeType = BTNodeType.Condition;

            var input = CreateInputPort();
            input.portName = "In";
            inputContainer.Add(input);

            var output = CreateOutputPort(Port.Capacity.Single);
            output.portName = "Out";
            outputContainer.Add(output);

            var enumField = new EnumField("Condition", BTConditionType.None);
            enumField.RegisterValueChangedCallback(evt =>
            {
                ConditionType = (BTConditionType)evt.newValue;
            });
            extensionContainer.Add(enumField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}