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
        
        public string NodeName { 
            get => title; 
            set => title = value; 
        }
        
        public BTBaseNodeView(string nodeName)
        {
            Guid = System.Guid.NewGuid().ToString();
            
            // 1. 노드 상단 타이틀 설정
            NodeName = nodeName;
            
            // --- [핵심] 에디터에서 이름을 수정할 수 있도록 TextField 추가 ---
            TextField nameTextField = new TextField
            {
                value = nodeName,
                isDelayed = true // 엔터를 치거나 포커스를 잃었을 때만 반영 (성능 및 사용성)
            };

            // 텍스트가 변경되면 노드의 title도 같이 변경
            nameTextField.RegisterValueChangedCallback(evt => 
            {
                NodeName = evt.newValue;
            });

            // 타이틀 영역(노드 상단)에 TextField 삽입
            titleContainer.Insert(0, nameTextField);
            
            // 기존 라벨(title)은 숨기고 TextField만 보이게 하고 싶다면 아래 주석 해제
            // titleContainer.Q<Label>().style.display = DisplayStyle.None;
            // ---------------------------------------------------------
            
            
            _activeIndicator = new VisualElement();
            _activeIndicator.style.height = 6;
            _activeIndicator.style.marginTop = 4;
            _activeIndicator.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            extensionContainer.Add(_activeIndicator);
            
            // UI 갱신을 위해 호출 (필수)
            RefreshExpandedState();
            RefreshPorts();
        }
        
        // public BTBaseNodeView()
        // {
        //     // Node가 생성될 때 노드마다 가지는 각 고유의 guid 생성 = "3c47a2e7-b4f1-4a3e-8e56-..."의 형태
        //     Guid = System.Guid.NewGuid().ToString();
        //
        //     _activeIndicator = new VisualElement(); // 노드 밑에 붙을 bar
        //     _activeIndicator.style.height = 6;
        //     _activeIndicator.style.marginTop = 4;
        //     _activeIndicator.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
        //     extensionContainer.Add(_activeIndicator);
        // }

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