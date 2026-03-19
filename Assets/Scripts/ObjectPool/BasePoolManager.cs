using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePoolManager<TKey> : MonoBehaviour
{
    [Serializable]
    public class PoolEntry
    {
        public PoolObject<TKey> prefab;
        public int initialCount = 10;

        public TKey Key => prefab.Key;
    }

    [SerializeField] private List<PoolEntry> entries = new List<PoolEntry>();

    private readonly Dictionary<TKey, Queue<PoolObject<TKey>>>
        poolMap = new Dictionary<TKey, Queue<PoolObject<TKey>>>();

    private readonly Dictionary<TKey, PoolObject<TKey>> prefabMap = new Dictionary<TKey, PoolObject<TKey>>();

    protected virtual void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        poolMap.Clear();
        prefabMap.Clear();

        foreach (var entry in entries)
        {
            if (entry.prefab == null)
                continue;

            TKey key = entry.Key;

            if (poolMap.ContainsKey(key))
            {
                Debug.LogWarning($"Duplicate pool key detected: {key}");
                continue;
            }

            poolMap.Add(key, new Queue<PoolObject<TKey>>());
            prefabMap.Add(key, entry.prefab);

            for (int i = 0; i < entry.initialCount; i++)
            {
                PoolObject<TKey> obj = Create(key);
                PushInternal(obj);
            }
        }
    }

    private PoolObject<TKey> Create(TKey key)
    {
        if (!prefabMap.TryGetValue(key, out var prefab))
            throw new InvalidOperationException($"Prefab not found for key: {key}");

        PoolObject<TKey> instance = Instantiate(prefab, transform);
        instance.PoolManager = this;
        instance.gameObject.SetActive(false);
        instance.IsInPool = true;
        return instance;
    }

    public PoolObject<TKey> Pop(TKey key)
    {
        if (!poolMap.ContainsKey(key))
            throw new InvalidOperationException($"Pool not found for key: {key}");

        PoolObject<TKey> obj;

        if (poolMap[key].Count > 0)
        {
            obj = poolMap[key].Dequeue();
        }
        else
        {
            obj = Create(key);
        }

        obj.transform.SetParent(null);
        obj.gameObject.SetActive(true);
        obj.OnPop();
        return obj;
    }

    public T Pop<T>(TKey key) where T : PoolObject<TKey>
    {
        PoolObject<TKey> obj = Pop(key);

        if (obj is T typedObj)
            return typedObj;

        throw new InvalidCastException($"Popped object is not of type {typeof(T).Name}");
    }

    public void Push(PoolObject<TKey> obj)
    {
        if (obj == null)
            return;

        if (obj.IsInPool)
            return;

        PushInternal(obj);
    }

    private void PushInternal(PoolObject<TKey> obj)
    {
        if (!poolMap.ContainsKey(obj.Key))
            throw new InvalidOperationException($"Pool not found for key: {obj.Key}");

        obj.OnPush();
        obj.transform.SetParent(transform);
        obj.gameObject.SetActive(false);
        poolMap[obj.Key].Enqueue(obj);
    }
}