using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using AI.BT.Runtime;

namespace AI.BT.Editor
{
    // 각 노드가 공통으로 가져야 할 것
    //     GUID
    //     노드 타입
    //     input/output port
    //     Active 표시 함수
    public class BTBaseNodeView : Node
    {
        public string Guid;
        public BTNodeType NodeType;

        // VisualElement = UI Toolkit의 기본 UI 요소
        protected VisualElement _activeIndicator;

        public BTBaseNodeView()
        {
            // Node가 생성될 때 노드마다 가지는 각 고유의 guid 생성 = "3c47a2e7-b4f1-4a3e-8e56-..."의 형태
            Guid = System.Guid.NewGuid().ToString();

            _activeIndicator = new VisualElement(); // 노드 밑에 붙을 bar
            _activeIndicator.style.height = 6;
            _activeIndicator.style.marginTop = 4;
            _activeIndicator.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            extensionContainer.Add(_activeIndicator);
        }

        //노드가 실행 중인지 표시, 런타임에서 호출
        public virtual void SetActive(bool active)
        {
            _activeIndicator.style.backgroundColor = active
                ? new StyleColor(Color.coral)
                : new StyleColor(new Color(0.3f, 0.3f, 0.3f));
        }

        protected Port CreateInputPort(Port.Capacity capacity = Port.Capacity.Single) // 단일 연결
        {
            return InstantiatePort(Orientation.Vertical, Direction.Input, capacity, typeof(bool));
        }

        protected Port CreateOutputPort(Port.Capacity capacity = Port.Capacity.Multi) // 다중 연결
        {
            return InstantiatePort(Orientation.Vertical, Direction.Output, capacity, typeof(bool));
        }
    }
}