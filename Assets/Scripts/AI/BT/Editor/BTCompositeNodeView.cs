using AI.BT.Runtime;

namespace AI.BT.Editor
{
    // Selector / Sequence 노드 설정
    public class BTCompositeNodeView : BTBaseNodeView
    {
        public BTCompositeNodeView(string name, BTNodeType type) : base(name)
        {
            title = name;
            NodeType = type;
            titleContainer.AddToClassList("bt-node__title");
            titleContainer.AddToClassList("bt-node__title--action");
            
            BTNodeStyleUtility.ApplyBaseStyle(this);   
            BTNodeStyleUtility.ApplyCompositeStyle(this); 
            BTNodeStyleUtility.MakeTitleEditable(this, name, evt => {
                title = evt.newValue; 
            });
            
            var input = CreateInputPort();
            input.portName = "In";
            inputContainer.Add(input);

            var output = CreateOutputPort();
            output.portName = "Children";
            outputContainer.Add(output);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}