// Editor/BehaviourTreeEditor.cs
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class BehaviourTreeEditor : EditorWindow
{
    private BTTree tree;
    private BTNode selectedNode;

    private Vector2 panOffset;
    private Vector2 drag;

    // === Added: selected node inspector ===
    private Editor nodeInspector;
    private Vector2 inspectorScroll;

    // Optional: leaf menu filtering (keeps menu sane in bigger projects)
    private const string LeafNamespacePrefix = ""; // e.g. "AI"; leave "" to disable namespace filtering

    [MenuItem("Tools/Behaviour Tree Editor")]
    public static void OpenWindow()
    {
        GetWindow<BehaviourTreeEditor>("Behaviour Tree");
    }

    private void OnDisable()
    {
        if (nodeInspector != null)
        {
            DestroyImmediate(nodeInspector);
            nodeInspector = null;
        }
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (tree == null)
        {
            EditorGUILayout.HelpBox("편집할 Behaviour Tree 에셋을 선택하세요.", MessageType.Info);
            return;
        }

        DrawGrid(20, 0.2f);
        DrawGrid(100, 0.4f);

        ProcessEvents(Event.current);

        DrawConnections();
        DrawNodes();

        GUILayout.Space(8);
        DrawSelectedNodeInspector();

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        var newTree = (BTTree)EditorGUILayout.ObjectField(tree, typeof(BTTree), false, GUILayout.Width(250));
        if (newTree != tree)
        {
            tree = newTree;
            selectedNode = null;

            if (nodeInspector != null)
            {
                DestroyImmediate(nodeInspector);
                nodeInspector = null;
            }
        }

        if (tree != null)
        {
            if (GUILayout.Button("Sequence 추가", EditorStyles.toolbarButton))
                CreateNode<SequenceNode>();

            if (GUILayout.Button("Selector 추가", EditorStyles.toolbarButton))
                CreateNode<SelectorNode>();

            if (GUILayout.Button("Action/Condition 추가", EditorStyles.toolbarButton))
                ShowCreateLeafMenu(null);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void CreateNode<T>() where T : BTNode
    {
        Undo.RecordObject(tree, "Create Node");

        var node = tree.CreateNode<T>();
        node.position = position.size * 0.5f;

        if (tree.rootNode == null)
            tree.rootNode = node;

        EditorUtility.SetDirty(tree);
    }

    /// <summary>
    /// Leaf 생성 메뉴 (parent가 있으면 생성 후 즉시 parent에 연결)
    /// </summary>
    private void ShowCreateLeafMenu(BTNode parent)
    {
        var menu = new GenericMenu();

        var leafTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Array.Empty<Type>(); } // some assemblies may fail GetTypes()
            })
            .Where(t => t != null
                        && !t.IsAbstract
                        && !t.IsGenericType
                        && (t.IsSubclassOf(typeof(ActionNode)) || t.IsSubclassOf(typeof(ConditionNode))))
            .Where(t =>
            {
                if (string.IsNullOrEmpty(LeafNamespacePrefix)) return true;
                return t.Namespace != null && t.Namespace.StartsWith(LeafNamespacePrefix, StringComparison.Ordinal);
            })
            .OrderBy(t => t.Name)
            .ToArray();

        if (leafTypes.Length == 0)
        {
            menu.AddDisabledItem(new GUIContent("No leaf types found"));
            menu.ShowAsContext();
            return;
        }

        foreach (var type in leafTypes)
        {
            string group =
                type.IsSubclassOf(typeof(ActionNode)) ? "Action/" :
                type.IsSubclassOf(typeof(ConditionNode)) ? "Condition/" :
                "Leaf/";

            menu.AddItem(new GUIContent(group + type.Name), false, () =>
            {
                Undo.RecordObject(tree, "Create Leaf Node");

                var node = tree.CreateNode(type);
                node.position = position.size * 0.5f;

                if (tree.rootNode == null)
                    tree.rootNode = node;

                if (parent != null)
                {
                    Undo.RecordObject(tree, "Add Child");
                    tree.AddChild(parent, node);
                    // place child next to parent for convenience
                    node.position = parent.position + new Vector2(220, 0);
                }

                SelectNode(node);

                EditorUtility.SetDirty(tree);
            });
        }

        menu.ShowAsContext();
    }

    private void DrawGrid(float gridSpacing, float gridOpacity)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(0, 0, 0, gridOpacity);

        panOffset += drag * 0.5f;

        Vector3 offset = new Vector3(panOffset.x, panOffset.y, 0f);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(
                new Vector3(gridSpacing * i, 0, 0) + offset,
                new Vector3(gridSpacing * i, position.height, 0) + offset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(
                new Vector3(0, gridSpacing * j, 0) + offset,
                new Vector3(position.width, gridSpacing * j, 0) + offset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDrag:
                if (e.button == 2) // middle button pan
                {
                    OnDrag(e.delta);
                }
                break;

            case EventType.MouseDown:
                if (e.button == 0)
                {
                    // click empty space clears selection
                    // (only if we didn't click a node in DrawNode)
                    // We'll set a flag in DrawNode by consuming event; here keep it simple:
                    // do nothing.
                }
                break;
        }
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;
        GUI.changed = true;
    }

    private void DrawNodes()
    {
        foreach (var node in tree.nodes)
        {
            DrawNode(node);
        }
    }

    private void DrawNode(BTNode node)
    {
        var rect = new Rect(node.position.x, node.position.y, 180, 90);

        GUI.Box(rect, "", EditorStyles.helpBox);

        // title (rename)
        var titleRect = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, 18);
        string newName = EditorGUI.TextField(titleRect, node.name);

        if (newName != node.name)
        {
            Undo.RecordObject(tree, "Rename Node");
            node.name = newName;
            EditorUtility.SetDirty(tree);
        }

        // root button
        var rootRect = new Rect(rect.x + 5, rect.yMax - 20, 60, 18);
        if (GUI.Button(rootRect, tree.rootNode == node ? "Root ✓" : "Set Root"))
        {
            Undo.RecordObject(tree, "Set Root");
            tree.rootNode = node;
            EditorUtility.SetDirty(tree);
        }

        // add child button
        var addChildRect = new Rect(rect.xMax - 70, rect.yMax - 20, 65, 18);
        if (GUI.Button(addChildRect, "Add Child"))
        {
            ShowAddChildMenu(node);
        }

        // ==== runtime state outline ====
        if (Application.isPlaying)
        {
            Color c = Color.clear;

            switch (node.state)
            {
                case BTNodeState.Running: c = Color.yellow; break;
                case BTNodeState.Success: c = Color.green; break;
                case BTNodeState.Failure: c = Color.red; break;
            }

            if (c.a > 0f)
                DrawNodeOutline(rect, c, 2f);
        }

        // click/drag/context
        var e = Event.current;

        // left click select
        if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
        {
            SelectNode(node);
            GUI.changed = true;
            e.Use();
        }

        // right click context delete
        if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete Node"), false, () =>
            {
                if (selectedNode == node)
                {
                    selectedNode = null;
                    if (nodeInspector != null)
                    {
                        DestroyImmediate(nodeInspector);
                        nodeInspector = null;
                    }
                }

                Undo.RecordObject(tree, "Delete Node");
                tree.DeleteNode(node);
                EditorUtility.SetDirty(tree);
            });
            menu.ShowAsContext();
            e.Use();
        }

        // drag move
        if (e.type == EventType.MouseDrag && e.button == 0 && selectedNode == node)
        {
            Undo.RecordObject(tree, "Move Node");
            node.position += e.delta;
            EditorUtility.SetDirty(tree);
            GUI.changed = true;
            e.Use();
        }
    }

    private void SelectNode(BTNode node)
    {
        selectedNode = node;

        if (nodeInspector != null)
        {
            DestroyImmediate(nodeInspector);
            nodeInspector = null;
        }

        if (selectedNode != null)
            nodeInspector = Editor.CreateEditor(selectedNode);
    }

    private void ShowAddChildMenu(BTNode parent)
    {
        var menu = new GenericMenu();

        // existing nodes as child
        foreach (var node in tree.nodes)
        {
            if (node == parent) continue;

            menu.AddItem(new GUIContent("Existing/" + node.name), false, () =>
            {
                Undo.RecordObject(tree, "Add Child");
                tree.AddChild(parent, node);
                EditorUtility.SetDirty(tree);
            });
        }

        menu.AddSeparator("");

        // create and link
        menu.AddItem(new GUIContent("New/Sequence"), false, () =>
        {
            Undo.RecordObject(tree, "Create + Add Child");
            var child = tree.CreateNode<SequenceNode>();
            child.position = parent.position + new Vector2(220, 0);
            tree.AddChild(parent, child);

            if (tree.rootNode == null)
                tree.rootNode = parent;

            SelectNode(child);
            EditorUtility.SetDirty(tree);
        });

        menu.AddItem(new GUIContent("New/Selector"), false, () =>
        {
            Undo.RecordObject(tree, "Create + Add Child");
            var child = tree.CreateNode<SelectorNode>();
            child.position = parent.position + new Vector2(220, 0);
            tree.AddChild(parent, child);

            if (tree.rootNode == null)
                tree.rootNode = parent;

            SelectNode(child);
            EditorUtility.SetDirty(tree);
        });

        // ✅ improved: create leaf and auto-link to parent
        menu.AddItem(new GUIContent("New/Leaf (Create & Link)"), false, () =>
        {
            ShowCreateLeafMenu(parent);
        });

        menu.ShowAsContext();
    }

    private void DrawSelectedNodeInspector()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Selected Node Inspector", EditorStyles.boldLabel);

        if (selectedNode == null)
        {
            EditorGUILayout.HelpBox("노드를 선택하면 해당 노드의 설정을 여기서 바로 수정할 수 있습니다.", MessageType.Info);
            GUILayout.EndVertical();
            return;
        }

        if (nodeInspector == null)
            nodeInspector = Editor.CreateEditor(selectedNode);

        inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll, GUILayout.MinHeight(160));
        nodeInspector.OnInspectorGUI();
        EditorGUILayout.EndScrollView();

        if (GUI.changed && tree != null)
        {
            // ensure changes persist
            EditorUtility.SetDirty(selectedNode);
            EditorUtility.SetDirty(tree);
        }

        GUILayout.EndVertical();
    }

    private void DrawNodeOutline(Rect rect, Color color, float thickness)
    {
        Handles.BeginGUI();
        Handles.color = color;

        Vector3 p1 = new Vector3(rect.x, rect.y);
        Vector3 p2 = new Vector3(rect.xMax, rect.y);
        Vector3 p3 = new Vector3(rect.xMax, rect.yMax);
        Vector3 p4 = new Vector3(rect.x, rect.yMax);

        Handles.DrawAAPolyLine(thickness, p1, p2, p3, p4, p1);

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();
        Handles.color = Color.white;

        foreach (var parent in tree.nodes)
        {
            foreach (var child in parent.children)
            {
                if (child == null) continue;

                Vector2 start = parent.position + new Vector2(180, 45);
                Vector2 end = child.position + new Vector2(0, 45);

                Vector2 startTangent = start + Vector2.right * 50f;
                Vector2 endTangent = end + Vector2.left * 50f;

                Handles.DrawBezier(
                    start,
                    end,
                    startTangent,
                    endTangent,
                    Color.white,
                    null,
                    2f
                );
            }
        }

        Handles.EndGUI();
    }
}
