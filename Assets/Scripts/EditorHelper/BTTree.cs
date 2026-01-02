using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SO로 관리될 대상에게 적용할 정보
/// Behaviour Tree 자체를 ScriptableObject로 저장하는 클래스
/// </summary>
[CreateAssetMenu(menuName = "AI/Behaviour Tree")]
public class BTTree : ScriptableObject
{
    public BTNode rootNode;// selectorNode
    public List<BTNode> nodes = new List<BTNode>(); //포함된 모든 노드 목록

    public BTNodeState Update()
    {
        if (rootNode == null)
        {
            Debug.LogWarning("[BT] Root node is null");
            return BTNodeState.Failure;
        }

        return rootNode.Tick();
    }

#if UNITY_EDITOR
    public T CreateNode<T>() where T : BTNode
    {
        T node = ScriptableObject.CreateInstance<T>();
        node.name = typeof(T).Name;
        node.tree = this;
        nodes.Add(node);

        UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        return node;
    }

    public BTNode CreateNode(System.Type type)
    {
        var node = ScriptableObject.CreateInstance(type) as BTNode;
        node.name = type.Name;
        node.tree = this;
        nodes.Add(node);

        UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        return node;
    }

    public void AddChild(BTNode parent, BTNode child)
    {
        if (!parent.children.Contains(child))
            parent.children.Add(child);
        child.parent = parent;
        UnityEditor.EditorUtility.SetDirty(this);
    }

    public void DeleteNode(BTNode node)
    {
        nodes.Remove(node);

        foreach (var n in nodes)
        {
            n.children.Remove(node);
            if (n.parent == node) n.parent = null;
        }

        UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }
    
    public void MoveChildUp(BTNode parent, BTNode child)
    {
        int i = parent.children.IndexOf(child);
        if (i > 0)
        {
            Undo.RecordObject(this, "Move Child Up");
            (parent.children[i - 1], parent.children[i]) = (parent.children[i], parent.children[i - 1]);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    public void MoveChildDown(BTNode parent, BTNode child)
    {
        int i = parent.children.IndexOf(child);
        if (i >= 0 && i < parent.children.Count - 1)
        {
            Undo.RecordObject(this, "Move Child Down");
            (parent.children[i + 1], parent.children[i]) = (parent.children[i], parent.children[i + 1]);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    
#endif
}