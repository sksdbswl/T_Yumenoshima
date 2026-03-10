// Assets/Scripts/AI/BT/Editor/BTActionNodeView.cs
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using AI.BT.Runtime;

namespace AI.BT.Editor
{
    public class BTActionNodeView : BTBaseNodeView
    {
        public BTActionType ActionType { get; private set; }
        public string AnimationStateName { get; private set; }

        public BTActionNodeView()
        {
            title = "Action";
            NodeType = BTNodeType.Action;

            var input = CreateInputPort();
            input.portName = "In";
            inputContainer.Add(input);

            var output = CreateOutputPort(Port.Capacity.Single);
            output.portName = "Out";
            outputContainer.Add(output);

            var actionField = new EnumField("Action", BTActionType.None);
            actionField.RegisterValueChangedCallback(evt =>
            {
                ActionType = (BTActionType)evt.newValue;
            });
            extensionContainer.Add(actionField);

            var animationField = new TextField("Animation");
            animationField.RegisterValueChangedCallback(evt =>
            {
                AnimationStateName = evt.newValue;
            });
            extensionContainer.Add(animationField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}