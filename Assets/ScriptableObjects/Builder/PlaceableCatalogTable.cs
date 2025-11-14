using UnityEngine;


[CreateAssetMenu(menuName = "Builder/Placeable Catalog", fileName = "PlaceableTable")]
public class PlaceableCatalogTable : ScriptableObject
{
    public PlaceableItem[] Items;

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
