using System;
using UnityEngine;

public abstract class PoolObject<TKey> : MonoBehaviour
{
    public BasePoolManager<TKey> PoolManager { get; set; }

    private bool applicationQuitting;
    public bool IsInPool { get; set; }

    public abstract TKey Key { get; }

    public virtual void Awake()
    {
        applicationQuitting = false;
    }

    private void OnApplicationQuit()
    {
        applicationQuitting = true;
    }

    public virtual void OnPop()
    {
        IsInPool = false;
    }

    public virtual void OnPush()
    {
        IsInPool = true;
    }

    public void ReturnToPool()
    {
        if (applicationQuitting) return;
        if (PoolManager == null)
            throw new InvalidOperationException("PoolManager is null");
        if (IsInPool) return;

        PoolManager.Push(this);
    }
}