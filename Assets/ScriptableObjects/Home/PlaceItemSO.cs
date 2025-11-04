using UnityEngine;

public enum PlaceCategory { Road, Deco, Building }

[CreateAssetMenu(menuName="Builder/Item")]
public class PlaceItemSO : ScriptableObject
{
    public string itemName;
    public PlaceCategory category;
    public GameObject prefab;
    [Tooltip("가로 x 세로 셀 점유(건물/도로용)")]
    public Vector2Int footprint = Vector2Int.one;
    [Tooltip("건물은 그리드 필수, 장식은 자유 배치 허용 등")]
    public bool requireGrid = true;
}

public enum PlaceableRole { Road, Deco, Building }

[CreateAssetMenu(menuName = "Builder/Placeable", fileName = "PlaceableItem")]
public class PlaceableItem : ScriptableObject
{
    public string DisplayName;
    public PlaceableRole Role;
    public GameObject Prefab;
    public Sprite Thumbnail;
}

