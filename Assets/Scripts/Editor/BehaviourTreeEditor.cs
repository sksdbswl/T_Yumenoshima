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

    [MenuItem("Tools/Behaviour Tree Editor")]
    public static void OpenWindow()
    {
        GetWindow<BehaviourTreeEditor>("Behaviour Tree");
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

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        tree = (BTTree)EditorGUILayout.ObjectField(tree, typeof(BTTree), false, GUILayout.Width(250));

        if (tree != null)
        {
            if (GUILayout.Button("Sequence 추가", EditorStyles.toolbarButton))
                CreateNode<SequenceNode>();

            if (GUILayout.Button("Selector 추가", EditorStyles.toolbarButton))
                CreateNode<SelectorNode>();

            if (GUILayout.Button("Action/Condition 추가", EditorStyles.toolbarButton))
                ShowCreateLeafMenu();
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

    private void ShowCreateLeafMenu()
    {
        // ScriptableObject 중에서 ActionNode/ConditionNode 상속받은 타입 찾기
        var menu = new GenericMenu();

        var leafTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract &&
                       (t.IsSubclassOf(typeof(ActionNode)) || t.IsSubclassOf(typeof(ConditionNode))));

        foreach (var type in leafTypes)
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                Undo.RecordObject(tree, "Create Leaf Node");
                var node = tree.CreateNode(type);
                node.position = position.size * 0.5f;
                if (tree.rootNode == null)
                    tree.rootNode = node;
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
                if (e.button == 2) // 중간버튼으로 팬
                {
                    OnDrag(e.delta);
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

        // 타이틀 (이름 편집 가능하게)
        var titleRect = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, 18);
        string newName = EditorGUI.TextField(titleRect, node.name);

        if (newName != node.name)
        {
            Undo.RecordObject(tree, "Rename Node");
            node.name = newName;
            EditorUtility.SetDirty(tree);
        }

        // 루트 표시 버튼
        var rootRect = new Rect(rect.x + 5, rect.yMax - 20, 60, 18);
        if (GUI.Button(rootRect, tree.rootNode == node ? "Root ✓" : "Set Root"))
        {
            tree.rootNode = node;
            EditorUtility.SetDirty(tree);
        }

        // 자식 추가 버튼
        var addChildRect = new Rect(rect.xMax - 70, rect.yMax - 20, 65, 18);
        if (GUI.Button(addChildRect, "Add Child"))
        {
            ShowAddChildMenu(node);
        }

        // 선택 테두리
        if (selectedNode == node)
        {
            Handles.BeginGUI();
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(3,
                rect.min,
                new Vector3(rect.xMax, rect.y),
                rect.max,
                new Vector3(rect.x, rect.yMax),
                rect.min);
            Handles.color = Color.white;
            Handles.EndGUI();
        }

        // 클릭/드래그
        var e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (rect.Contains(e.mousePosition))
            {
                selectedNode = node;
                GUI.changed = true;
            }
        }
        
        // 우클릭으로 삭제 메뉴 열기
        if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete Node"), false, () =>
            {
                Undo.RecordObject(tree, "Delete Node");
                tree.DeleteNode(node);          // BTTree 안에 이미 구현된 함수
                EditorUtility.SetDirty(tree);
            });
            menu.ShowAsContext();
            e.Use();
        }

        // 드래그 (기존 코드 유지)
        if (e.type == EventType.MouseDrag && e.button == 0 && selectedNode == node)
        {
            Undo.RecordObject(tree, "Move Node");
            node.position += e.delta;
            EditorUtility.SetDirty(tree);
            GUI.changed = true;
        }
    }

    private void ShowAddChildMenu(BTNode parent)
    {
        var menu = new GenericMenu();

        // 이미 있는 노드를 자식으로 연결
        foreach (var node in tree.nodes)
        {
            if (node == parent) continue;
            menu.AddItem(new GUIContent("Existing/" + node.name), false, () =>
            {
                Undo.RecordObject(tree, "Add Child");
                tree.AddChild(parent, node);
            });
        }

        menu.AddSeparator("");

        // 새 노드 생성 후 자식으로
        menu.AddItem(new GUIContent("New/Sequence"), false, () =>
        {
            var child = tree.CreateNode<SequenceNode>();
            child.position = parent.position + new Vector2(200, 0);
            tree.AddChild(parent, child);
        });

        menu.AddItem(new GUIContent("New/Selector"), false, () =>
        {
            var child = tree.CreateNode<SelectorNode>();
            child.position = parent.position + new Vector2(200, 0);
            tree.AddChild(parent, child);
        });

        menu.AddItem(new GUIContent("New/Leaf..."), false, () =>
        {
            ShowCreateLeafMenu(); // Leaf 생성 후 수동으로 연결해도 되고,
            // 여기를 확장해서 "생성 + 즉시 parent에 연결"하는 버전도 가능
        });

        menu.ShowAsContext();
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();
        Handles.color = Color.white;

        foreach (var parent in tree.nodes)
        {
            foreach (var child in parent.children)
            {
                Vector2 start = parent.position + new Vector2(180, 45); // parent 오른쪽 중간
                Vector2 end   = child.position  + new Vector2(0, 45);   // child 왼쪽 중간

                Vector2 startTangent = start + Vector2.right * 50f;
                Vector2 endTangent   = end   + Vector2.left  * 50f;

                // Vector2 → Vector3는 자동 형변환 들어가서 그냥 전달해도 됨
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


