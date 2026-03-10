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

                var childEdges = edges.Where(e => e.output.node == nodeView).ToList();
                foreach (var edge in childEdges)
                {
                    if (edge.input.node is BTBaseNodeView childNode)
                        data.childrenGuids.Add(childNode.Guid);
                }

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
            string path = EditorUtility.OpenFilePanel("Load Behavior Tree", "Assets", "asset");
            if (string.IsNullOrEmpty(path))
                return;

            path = FileUtil.GetProjectRelativePath(path);
            var asset = AssetDatabase.LoadAssetAtPath<BTGraphAsset>(path);
            if (asset == null)
                return;

            ClearGraph(graphView);

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
        }
        
        /// <summary>
        /// 그래프 로드
        /// </summary>
        private static BTBaseNodeView CreateNodeFromData(BTNodeData data)
        {
            BTBaseNodeView node = data.nodeType switch
            {
                BTNodeType.Root => new BTRootNodeView(),
                BTNodeType.Selector => new BTCompositeNodeView("Selector", BTNodeType.Selector),
                BTNodeType.Sequence => new BTCompositeNodeView("Sequence", BTNodeType.Sequence),
                BTNodeType.Condition => new BTConditionNodeView(),
                BTNodeType.Action => new BTActionNodeView(),
                _ => null
            };

            node.Guid = data.guid;

            if (node is BTConditionNodeView conditionNode)
            {
                // 필요하면 setter 함수 추가해서 값 복원
            }

            if (node is BTActionNodeView actionNode)
            {
                // 필요하면 setter 함수 추가해서 값 복원
            }

            return node;
        }

        /// <summary>
        /// 그래프 초기화
        /// </summary>
        private static void ClearGraph(BTGraphView graphView)
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