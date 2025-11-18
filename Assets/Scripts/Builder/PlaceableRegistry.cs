using System.Collections.Generic;
using UnityEngine;

public class PlaceableRegistry : SingletonBase<PlaceableRegistry>
{
    public static PlaceableRegistry Instance { get; private set; }

    // BuilderID 기준으로 접근
    private Dictionary<int, PlaceableObject> _byId = new Dictionary<int, PlaceableObject>();
    private List<PlaceableObject> _buildings = new List<PlaceableObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(PlaceableObject obj, int builderId)
    {
        _byId[builderId] = obj;

        if (obj.Role == PlaceableRole.Building)
            _buildings.Add(obj);
    }

    public PlaceableObject GetById(int id)
    {
        _byId.TryGetValue(id, out var obj);
        return obj;
    }

    public IReadOnlyList<PlaceableObject> GetBuildings()
    {
        return _buildings;
    }
}