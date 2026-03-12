using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using AI.BT.Runtime;

namespace AI.BT.Editor
{
    // Condition node 설정
    // Condition은 enum popup 하나만 있어도 충분히 시작 가능합니다.
    public class BTConditionNodeView : BTBaseNodeView
    {
        private EnumField _enumField;
        public BTConditionType ConditionType { get; set; }
        
        public BTConditionNodeView(string name, BTConditionType conditionType = BTConditionType.None) : base(name)
        {
            title = name;
            NodeType = BTNodeType.Condition;
            ConditionType = conditionType;
            
            BTNodeStyleUtility.ApplyBaseStyle(this);   
            BTNodeStyleUtility.ApplyConditionStyle(this); 
            BTNodeStyleUtility.MakeTitleEditable(this, name, evt => {
                title = evt.newValue; 
            });
            
            var input = CreateInputPort();
            input.portName = "In";
            inputContainer.Add(input);

            var output = CreateOutputPort(Port.Capacity.Single);
            output.portName = "Out";
            outputContainer.Add(output);

            _enumField = new EnumField("Condition", ConditionType);
            BTNodeStyleUtility.ApplyEnumFieldStyle(_enumField);
            _enumField.RegisterValueChangedCallback(evt =>
            {
                ConditionType = (BTConditionType)evt.newValue;
            });
            extensionContainer.Add(_enumField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
