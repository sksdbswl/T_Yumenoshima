using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Nodes/Selector")]
public class SelectorNode : BTNode
{
    private int _current;

    protected override void OnStart()
    {
        _current = 0;
    }

    protected override BTNodeState OnUpdate()
    {
        if (children.Count == 0) return BTNodeState.Failure;
        
        _current = 0; 
        
        while (_current < children.Count)
        {
            var child = children[_current];
            var result = child.Tick();

            if (result == BTNodeState.Running)
                return BTNodeState.Running;

            if (result == BTNodeState.Success)
            {
                _current = 0;
                return BTNodeState.Success;
            }

            // Failure면 다음 자식으로
            _current++;
        }

        _current = 0;
        return BTNodeState.Failure;
    }
}