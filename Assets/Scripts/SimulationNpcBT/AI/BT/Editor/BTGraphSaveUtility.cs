using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using AI.BT.Runtime;

namespace AI.BT.Editor
{
    public static class BTGraphSaveUtility
    {
        /// <summary>
        /// 그래프 저장
        /// </summary>
        public static void Save(BTGraphView graphView)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Behavior Tree",
                "NewBTGraph",
                "asset",
                "Save BT Graph");

            if (string.IsNullOrEmpty(path))
                return;

            var asset = ScriptableObject.CreateInstance<BTGraphAsset>();
            asset.nodes.Clear();

            var nodeViews = graphView.nodes.ToList().OfType<BTBaseNodeView>().ToList();
            var edges = graphView.edges.ToList();

            foreach (var nodeView in nodeViews)
            {
                var data = new BTNodeData
                {
                    guid = nodeView.Guid,
                    nodeName = nodeView.title,
                    nodeType = nodeView.NodeType,
                    position = nodeView.GetPosition().position
                };

                if (nodeView is BTConditionNodeView conditionNode)
                {
                    data.conditionType = conditionNode.ConditionType;
                }

                if (nodeView is BTActionNodeView actionNode)
                {
                    data.actionType = actionNode.ActionType;
                    data.animationStateName = actionNode.AnimationStateName;
                }

                data.childrenGuids = edges
                    .Where(e => e.output.node == nodeView)
                    .Select(e => e.input.node as BTBaseNodeView)
                    .Where(n => n != null)
                    .OrderBy(n => n.GetPosition().x)
                    .Select(n => n.Guid)
                    .ToList();
                
                // var childEdges = edges.Where(e => e.output.node == nodeView).ToList();
                // foreach (var edge in childEdges)
                // {
                //     if (edge.input.node is BTBaseNodeView childNode)
                //         data.childrenGuids.Add(childNode.Guid);
                // }

                asset.nodes.Add(data);

                if (nodeView is BTRootNodeView)
                    asset.rootGuid = nodeView.Guid;
            }

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"BT Graph saved: {path}");
        }

        /// <summary>
        /// 그래프 불러오기
        /// </summary>
        public static void Load(BTGraphView graphView)
        {
            string path = EditorUtility.OpenFilePanel("Load Behavior Tree", "Assets/ScriptableObjects/AI", "asset");
            if (string.IsNullOrEmpty(path))
                return;

            path = FileUtil.GetProjectRelativePath(path);
            var asset = AssetDatabase.LoadAssetAtPath<BTGraphAsset>(path);
            if (asset == null)
                return;

            ResetGraph(graphView);

            var guidToNode = new Dictionary<string, BTBaseNodeView>();

            foreach (var nodeData in asset.nodes)
            {
                BTBaseNodeView nodeView = CreateNodeFromData(nodeData);
                graphView.AddElement(nodeView);
                nodeView.SetPosition(new Rect(nodeData.position, new Vector2(220, 180)));
                guidToNode[nodeData.guid] = nodeView;
            }

            foreach (var nodeData in asset.nodes)
            {
                if (!guidToNode.TryGetValue(nodeData.guid, out var parent))
                    continue;

                foreach (var childGuid in nodeData.childrenGuids)
                {
                    if (!guidToNode.TryGetValue(childGuid, out var child))
                        continue;

                    var parentOut = parent.outputContainer[0] as Port;
                    var childIn = child.inputContainer[0] as Port;

                    var edge = parentOut.ConnectTo(childIn);
                    graphView.AddElement(edge);
                }
            }
            
            BTEditorDebugger.OnNodeActive += graphView.HandleNodeActive;
        }
        
        /// <summary>
        /// 그래프 로드
        /// </summary>
        private static BTBaseNodeView CreateNodeFromData(BTNodeData data)
        {
            // data.nodeName이 있으면 그 이름을 사용하고, 없으면 기본 타입 이름을 사용합니다.
            string initialName = string.IsNullOrEmpty(data.nodeName) ? data.nodeType.ToString() : data.nodeName;

            BTBaseNodeView node = data.nodeType switch
            {
                // 모든 생성자에 initialName을 전달하여 에디터에서 수정한 이름이 복원되게 합니다.
                BTNodeType.Root => new BTRootNodeView(initialName), 
                BTNodeType.Selector => new BTCompositeNodeView(initialName, BTNodeType.Selector),
                BTNodeType.Sequence => new BTCompositeNodeView(initialName, BTNodeType.Sequence),
                BTNodeType.Condition => new BTConditionNodeView(initialName, data.conditionType),
                BTNodeType.Action => new BTActionNodeView(initialName, data.actionType),
                _ => null
            };

            if (node == null) return null;

            // GUID 복원 (매우 중요: 연결 관계 복구용)
            node.Guid = data.guid;

            // 추가 데이터 복원 (Condition, Action 등)
            if (node is BTConditionNodeView conditionNode)
            {
                conditionNode.ConditionType = data.conditionType;
            }

            if (node is BTActionNodeView actionNode)
            {
                actionNode.ActionType = data.actionType;
                actionNode.AnimationStateName = data.animationStateName;
            }

            return node;
        }
        
        // private static BTBaseNodeView CreateNodeFromData(BTNodeData data)
        // {
        //     BTBaseNodeView node = data.nodeType switch
        //     {
        //         BTNodeType.Root => new BTRootNodeView(),
        //         BTNodeType.Selector => new BTCompositeNodeView("Selector", BTNodeType.Selector),
        //         BTNodeType.Sequence => new BTCompositeNodeView("Sequence", BTNodeType.Sequence),
        //         BTNodeType.Condition => new BTConditionNodeView("Condition"),
        //         BTNodeType.Action => new BTActionNodeView("Action"),
        //         _ => null
        //     };
        //
        //     node.Guid = data.guid;
        //
        //     if (node is BTConditionNodeView conditionNode)
        //     {
        //         // 필요하면 setter 함수 추가해서 값 복원
        //     }
        //
        //     if (node is BTActionNodeView actionNode)
        //     {
        //         // 필요하면 setter 함수 추가해서 값 복원
        //     }
        //
        //     return node;
        // }

        /// <summary>
        /// 그래프 초기화
        /// </summary>
        public static void ResetGraph(BTGraphView graphView)
        {
            var edges = graphView.edges.ToList();
            foreach (var edge in edges)
                graphView.RemoveElement(edge);

            var nodes = graphView.nodes.ToList().OfType<BTBaseNodeView>().ToList();
            foreach (var node in nodes)
                graphView.RemoveElement(node);
        }
    }
}