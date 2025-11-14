using UnityEngine;

/// <summary>
/// (Plane) Ground : 땅 ( 위에 배치 가능 )
/// Road : 길 ( 위에 배치 불가 )
/// Tile : 일반 타일 데코 ( 위에 배치 가능 )
/// Deco : 꾸미기 오브젝트 ( 위에 배치 불가 )
/// Building : 건물 ( 위에 배치 불가 )
/// </summary>
public enum PlaceableRole { Road, Tile, Deco, Building }

[CreateAssetMenu(menuName = "Builder/Placeable", fileName = "PlaceableItem")]
public class PlaceableItem : ScriptableObject
{
    public int BuilderID;
    public string DisplayName;
    public PlaceableRole Role;
    public GameObject Prefab;
    public Sprite Thumbnail;
    public bool IsStack;
}