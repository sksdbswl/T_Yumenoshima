using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "AI/Nodes/Selector")]
public class SelectorNode : BTNode
{
    private BTNode _active;

    protected override void OnStart()
    {
        Debug.Log($"[Selector:{name}] children order = {string.Join(" -> ", children.Select(c => c.name))}");
        _active = null;
    }

    protected override BTNodeState OnUpdate()
    {
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var result = child.Tick();

            if (result == BTNodeState.Failure)
                continue;

            if (_active != null && _active != child)
                _active.AbortRunningBranch();

            _active = (result == BTNodeState.Running) ? child : null;
            return result;
        }

        if (_active != null)
        {
            _active.AbortRunningBranch();
            _active = null;
        }

        return BTNodeState.Failure;
    }

    public override void AbortRunningBranch()
    {
        if (_active != null)
            _active.AbortRunningBranch();

        Abort();
    }
}

