using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Builder/Placeable Catalog", fileName = "PlaceableTable")]
public class PlaceableCatalogTable : ScriptableObject
{
    public PlaceableItem[] Items;
    private Dictionary<int, PlaceableItem> _byBuilderId;
    
    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        _byBuilderId = new Dictionary<int, PlaceableItem>();

        if (Items == null) return;

        for (int i = 0; i < Items.Length; i++)
        {
            var it = Items[i];
            if (it == null) continue;

            if (_byBuilderId.ContainsKey(it.BuilderId))
            {
                Debug.LogWarning($"중복 BuilderId : {it.BuilderId} in {it.DisplayName}");
                continue;
            }

            _byBuilderId.Add(it.BuilderId, it);
        }
    }
    
    // Id에 맞는 첫 번째 아이템 반환
    public PlaceableItem GetByBuilderId(int builderId)
    {
        if (_byBuilderId == null || _byBuilderId.Count == 0)
            BuildLookup();

        _byBuilderId.TryGetValue(builderId, out var item);
        return item;
    }
    
    // role에 맞는 첫 번째 아이템 반환
    public PlaceableItem GetByRole(PlaceableRole role)
    {
        if (Items == null) return null;

        for (int i = 0; i < Items.Length; i++)
        {
            var it = Items[i];
            if (it == null) continue;
            if (it.Role == role)
                return it;
        }

        return null;
    }
}
