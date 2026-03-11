using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AI.BT.Editor
{
    //     EditorWindow는 유니티 상단 메뉴에서 열리는 커스텀 툴 창
    //     rootVisualElement 위에 Toolbar, GraphView를 올립니다.
    //     저장/로드 버튼도 여기서 추가 ( Toolbar )
    // EditorWindow가 창이라면, GraphView는 그림판/작업영역
    // 즉. 그래프를 직접 편집하는 코드가 아니라 그래프뷰에게 명령을 내리는 UI
    public class BTEditorWindow : EditorWindow
    {
        private BTGraphView _graphView;
        
        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
            BTEditorDebugger.OnNodeActive -= _graphView.HandleNodeActive;
        }
        
        // Editor가 열릴때 실행
        [MenuItem("Tools/AI Behavior Tree Editor")] 
        public static void Open()
        {
            var window = GetWindow<BTEditorWindow>(); // Editor 열기
            window.titleContent = new GUIContent("AI BT Editor"); // 제목 설정
        }
        
        /// <summary>
        /// UI Toolkit 방식에서 창의 UI를 실제로 구성하는 함수
        /// </summary>
        private void CreateGUI()
        {
            // rootVisualElement = 현재 Editor 창의 최상위 루트
            // 따라서, 창 안의 모든 UI를 담는 최상위 컨테이너
            rootVisualElement.Clear(); 
            
            CreateToolbar();
            CreateGraphView();
        }

        /// <summary>
        /// 메뉴 생성
        /// 창 안에 실제 편집 영역인 BTGraphView를 추가하는 부분
        /// </summary>
        private void CreateGraphView()
        {
            Debug.Log("CreateGraphView");
            
            _graphView = new BTGraphView(this) // BTGraphView 생성 및 이름 지정
            {
                name = "Behavior Tree Graph"
            };
            
            _graphView.style.flexGrow = 1; // 남는 공간을 다 차지하게 함 
            //_graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        /// <summary>
        /// Editor Toolbar 설정
        /// </summary>
        private void CreateToolbar()
        {
            var toolbar = new Toolbar();

            var saveButton = new Button(() => _graphView.SaveGraph())
            {
                text = "Save"
            };

            var loadButton = new Button(() => _graphView.LoadGraph())
            {
                text = "Load"
            };

            toolbar.Add(saveButton);
            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }
    }
}