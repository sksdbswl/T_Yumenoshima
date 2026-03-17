using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using AI.BT.Runtime;

namespace AI.BT.Editor
{
    public class BTGraphView : GraphView
    {
        private BTEditorWindow _window;

        // 생성자
        public BTGraphView(BTEditorWindow window)
        {
            _window = window; // window = BTEditorWindow

            style.flexGrow = 1; // 부모 안에서 가능한 공간을 최대한 차지

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale); // 줌 인/아웃 기능 활성화, 마우스 휠로 확대/축소 가능

            // GraphView의 기본 조작 UX = 즉, 그래프 편집기다운 조작감을 붙이는 부분
            this.AddManipulator(new ContentDragger()); // 배경 드래그로 캔버스 이동
            this.AddManipulator(new SelectionDragger()); // 노드 선택 후 드래그 이동
            this.AddManipulator(new RectangleSelector()); // 드래그해서 여러 노드 박스 선택

            Insert(0, new GridBackground()); // 배경 격자 추가

            //초기 루트 노드를 하나 자동으로 추가
            AddElement(CreateRootNode());
        }
        
        public void HandleNodeActive(string guid)
        {
            foreach (var node in nodes.ToList())
            {
                if (node is BTBaseNodeView btNode)
                {
                    btNode.SetActive(btNode.Guid == guid);
                }
            }
        }
        
        /// <summary>
        /// 직접 메뉴 생성
        /// 사용자 우클릭 -> Unity 내부에서 ContextMenu 이벤트 발생
        /// </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt); 

            Vector2 mousePosition = evt.localMousePosition;
            
            // 우클릭 메뉴를 직접 구성
            evt.menu.AppendAction("Create Node/Root",
                _ => AddElement(CreateRootNode()));
            
            evt.menu.AppendAction("Create Node/Composite/Selector",
                _ => AddElement(CreateCompositeNode("Selector", BTNodeType.Selector, mousePosition)));

            evt.menu.AppendAction("Create Node/Composite/Sequence",
                _ => AddElement(CreateCompositeNode("Sequence", BTNodeType.Sequence, mousePosition)));

            evt.menu.AppendAction("Create Node/Leaf/Condition",
                _ => AddElement(CreateConditionNode(mousePosition)));

            evt.menu.AppendAction("Create Node/Leaf/Action",
                _ => AddElement(CreateActionNode(mousePosition)));
        }

        #region BuildContextualMenu ISSUE

        // 문제 코드 = > BuildContextualMenu 로 변경해서 해결
        // private void AddSearchWindow()
        // {
        //     nodeCreationRequest = ctx =>
        //     {
        //         var menu = new GenericMenu();
        //         menu.AddItem(new GUIContent("Composite/Selector"), false,
        //             () => AddElement(CreateCompositeNode("Selector", BTNodeType.Selector, ctx.screenMousePosition)));
        //         menu.AddItem(new GUIContent("Composite/Sequence"), false,
        //             () => AddElement(CreateCompositeNode("Sequence", BTNodeType.Sequence, ctx.screenMousePosition)));
        //         menu.AddItem(new GUIContent("Leaf/Condition"), false,
        //             () => AddElement(CreateConditionNode(ctx.screenMousePosition)));
        //         menu.AddItem(new GUIContent("Leaf/Action"), false,
        //             () => AddElement(CreateActionNode(ctx.screenMousePosition)));
        //         menu.ShowAsContext(); // 메뉴 안에서 또 메뉴를 띄우는 구조
        //     };
        // }
        
        // 문제 1: nodeCreationRequest의 용도와 GenericMenu.ShowAsContext() 조합
        // nodeCreationRequest는 보통 GraphView에서 노드 생성 UX를 연결하는 지점이에요. 주로 SearchWindow와 연결하는 데 자주 씀
        // 문제 2 : ctx.screenMousePosition = 스크린 좌표 ( 모니터 상의 좌표 )
        
        #endregion
        

        /// <summary>
        /// 포트 연결 조건
        /// 자기 자신은 안 됨 & 방향이 같으면 안 됨 & 같은 노드끼리 안 됨
        /// </summary>
        public override System.Collections.Generic.List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort != startPort &&
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        /// <summary>
        /// 노트 생성 함수
        /// 마우스 위치를 기준으로, 노드를 생성 및 그 위치에 배치하고 반환
        /// </summary>
        private BTBaseNodeView CreateRootNode()
        {
            var node = new BTRootNodeView("Root");
            node.SetPosition(new Rect(new Vector2(300, 100), new Vector2(200, 150)));
            return node;
        }

        private BTBaseNodeView CreateCompositeNode(string title, BTNodeType type, Vector2 mousePosition)
        {
            var node = new BTCompositeNodeView(title, type);
            node.SetPosition(new Rect(mousePosition, new Vector2(220, 180)));
            return node;
        }

        private BTBaseNodeView CreateConditionNode(Vector2 mousePosition)
        {
            var node = new BTConditionNodeView("Condition");
            node.SetPosition(new Rect(mousePosition, new Vector2(250, 200)));
            return node;
        }

        private BTBaseNodeView CreateActionNode(Vector2 mousePosition)
        {
            var node = new BTActionNodeView("Action");
            node.SetPosition(new Rect(mousePosition, new Vector2(250, 220)));
            return node;
        }

        /// <summary>
        /// 그래프 저장
        /// </summary>
        public void SaveGraph()
        {
            BTGraphSaveUtility.Save(this);
        }
        
        /// <summary>
        /// 그래프 로드
        /// </summary>
        public void LoadGraph()
        {
            BTGraphSaveUtility.Load(this);
        }
        
         
        /// <summary>
        /// 그래프 초기화
        /// </summary>
        public void ResetGraph()
        {
            BTGraphSaveUtility.ResetGraph(this);
        }
    }
}