using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace AI.BT.Editor
{
    public static class BTNodeStyleUtility
    {
        private static StyleSheet _styleSheet;

        private static StyleSheet LoadStyleSheet()
        {
            if (_styleSheet == null)
            {
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/Scripts/SimulationNpcBT/AI/BT/Editor/NodeStyle.uss");
            }

            return _styleSheet;
        }
        
        // 기존 ApplyBaseStyle 유지
        public static void ApplyBaseStyle(VisualElement node)
        {
            var styleSheet = LoadStyleSheet();
            if (styleSheet != null && !node.styleSheets.Contains(styleSheet))
            {
                node.styleSheets.Add(styleSheet);
            }
            
            node.AddToClassList("node");
        }

        // Action 노드 전용 클래스 추가
        public static void ApplyRootStyle(VisualElement node)
        {
            node.AddToClassList("root-node");
        }
        
        public static void ApplyCompositeStyle(VisualElement node)
        {
            node.AddToClassList("composite-node");
        }
   
        public static void ApplyConditionStyle(VisualElement node)
        {
            node.AddToClassList("condition-node");
        }
        
        public static void ApplyActionStyle(VisualElement node)
        {
            node.AddToClassList("action-node");
        }
        
        // Enum 스타일 적용 ( Select Box )
        public static void ApplyEnumFieldStyle(EnumField field)
        {
            if (field == null) return;

            field.AddToClassList("bt-node__field");

            if (field.labelElement != null)
                field.labelElement.AddToClassList("bt-node__field-label");

            var input = field.Q(className: BaseField<System.Enum>.inputUssClassName);
            if (input != null)
                input.AddToClassList("bt-node__input");
        }
        
        // Title 스타일 적용 및 수정 기능 추가
        public static void MakeTitleEditable(VisualElement node, string initialValue, EventCallback<ChangeEvent<string>> onValueChanged)
        {
            // 1. 기존 타이틀 라벨 숨기기 (선택 사항)
            var titleLabel = node.Q<Label>("title-label");
            var arrowButton = node.Q("title-button-container"); // 화살표 버튼 컨테이너
            if (titleLabel != null) titleLabel.style.display = DisplayStyle.None;

            // 2. 새로운 TextField 생성 및 추가
            var titleField = new TextField { value = initialValue };
        
            // 스타일 클래스 추가 (위에서 작성한 USS와 연결)
            titleField.AddToClassList("unity-text-field");
        
            titleField.RegisterValueChangedCallback(onValueChanged);
        
            // 3. 타이틀 컨테이너에 삽입
            var titleContainer = node.Q("title");
           
            // 타이틀 컨테이너 자체의 레이아웃 조정
            titleContainer.Add(titleField);
            arrowButton.BringToFront();
        }
    }
}