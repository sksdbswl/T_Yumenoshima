using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using AI.BT.Runtime;

namespace AI.BT.Editor
{
    //각 노드가 공통으로 가져야 할 것
    //     GUID
    //     노드 타입
    //     input/output port
    //     Active 표시 함수
    public class BTBaseNodeView : Node
    {
        public string Guid;
        public BTNodeType NodeType;

        protected VisualElement _activeIndicator;

        public BTBaseNodeView()
        {
            Guid = System.Guid.NewGuid().ToString();

            _activeIndicator = new VisualElement();
            _activeIndicator.style.height = 6;
            _activeIndicator.style.marginTop = 4;
            _activeIndicator.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            extensionContainer.Add(_activeIndicator);
        }

        public virtual void SetActive(bool active)
        {
            _activeIndicator.style.backgroundColor = active
                ? new StyleColor(Color.coral)
                : new StyleColor(new Color(0.3f, 0.3f, 0.3f));
        }

        protected Port CreateInputPort(Port.Capacity capacity = Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Vertical, Direction.Input, capacity, typeof(bool));
        }

        protected Port CreateOutputPort(Port.Capacity capacity = Port.Capacity.Multi)
        {
            return InstantiatePort(Orientation.Vertical, Direction.Output, capacity, typeof(bool));
        }
    }
}