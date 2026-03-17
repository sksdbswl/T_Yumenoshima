// Assets/Scripts/AI/BT/Editor/BTActionNodeView.cs
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using AI.BT.Runtime;
using UnityEngine;

namespace AI.BT.Editor
{
    public class BTActionNodeView : BTBaseNodeView
    {
        public BTActionType ActionType { get; set; }
        public string AnimationStateName { get; set; }

        public BTActionNodeView(string name, BTActionType actionType = BTActionType.None) : base(name)
        {
            // Title Settings
            title = name;
            NodeType = BTNodeType.Action;
            ActionType = actionType;
            
            // Style Settings
            BTNodeStyleUtility.ApplyBaseStyle(this);   // 공통 스타일 (node)
            BTNodeStyleUtility.ApplyActionStyle(this); // Action 전용 스타일 (action-node)
            BTNodeStyleUtility.MakeTitleEditable(this, name, evt => {
                title = evt.newValue; 
            });
            
            // Port Settings   
            var input = CreateInputPort();
            input.portName = "In";
            inputContainer.Add(input);

            var output = CreateOutputPort(Port.Capacity.Single);
            output.portName = "Out";
            outputContainer.Add(output);
            
            // Extension Settings
            var actionField = new EnumField("Action", ActionType);
            actionField.RegisterValueChangedCallback(evt =>
            {
                ActionType = (BTActionType)evt.newValue;
            });
            BTNodeStyleUtility.ApplyEnumFieldStyle(actionField);
            extensionContainer.Add(actionField);

            // Animation State
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