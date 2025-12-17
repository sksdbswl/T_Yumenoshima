using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 실제 노드의 기본구조 클래스로, 구성된 모든 노드에게 상속
/// `state` (Success / Failure / Running)
/// `Tick()` (OnStart → OnUpdate → OnStop 호출 흐름)
/// </summary>
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

    // Running 중인 노드를 끊을 때 OnStop까지 호출해주는 안전한 중단
    public void Abort()
    {
        if (_started)
        {
            OnStop();
            _started = false;
        }
        state = BTNodeState.Failure;
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
