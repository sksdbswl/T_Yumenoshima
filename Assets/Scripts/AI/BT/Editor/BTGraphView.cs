using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using AI.BT.Editor.Game.AI.BT.Editor;
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

            Insert(0, new GridBackground());

            AddElement(CreateRootNode());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            Vector2 mousePosition = evt.localMousePosition;

            evt.menu.AppendAction("Create Node/Composite/Selector",
                _ => AddElement(CreateCompositeNode("Selector", BTNodeType.Selector, mousePosition)));

            evt.menu.AppendAction("Create Node/Composite/Sequence",
                _ => AddElement(CreateCompositeNode("Sequence", BTNodeType.Sequence, mousePosition)));

            evt.menu.AppendAction("Create Node/Leaf/Condition",
                _ => AddElement(CreateConditionNode(mousePosition)));

            evt.menu.AppendAction("Create Node/Leaf/Action",
                _ => AddElement(CreateActionNode(mousePosition)));
        }

        public override System.Collections.Generic.List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort != startPort &&
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        private BTBaseNodeView CreateRootNode()
        {
            var node = new BTRootNodeView();
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
            var node = new BTConditionNodeView();
            node.SetPosition(new Rect(mousePosition, new Vector2(250, 200)));
            return node;
        }

        private BTBaseNodeView CreateActionNode(Vector2 mousePosition)
        {
            var node = new BTActionNodeView();
            node.SetPosition(new Rect(mousePosition, new Vector2(250, 220)));
            return node;
        }

        public void SaveGraph()
        {
            BTGraphSaveUtility.Save(this);
        }

        public void LoadGraph()
        {
            BTGraphSaveUtility.Load(this);
        }
    }
}