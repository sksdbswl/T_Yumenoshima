using System.Collections.Generic;
using UnityEngine;

public abstract class BTNode : ScriptableObject
{
    [HideInInspector] public BTTree tree;
    [HideInInspector] public BTNode parent;
    [HideInInspector] public List<BTNode> children = new List<BTNode>();
    [HideInInspector] public Vector2 position;

    private bool _started;
    public BTNodeState state { get; protected set; }

    public BTNodeState Tick()
    {
        if (!_started)
        {
            OnStart();
            _started = true;
        }

        state = OnUpdate();

        if (state == BTNodeState.Success || state == BTNodeState.Failure)
        {
            OnStop();
            _started = false;
        }

        return state;
    }

    public void ResetState()
    {
        state = BTNodeState.Failure;
        _started = false;
    }
    
    protected virtual void OnStart() { }
    protected virtual void OnStop() { }
    protected abstract BTNodeState OnUpdate();
}