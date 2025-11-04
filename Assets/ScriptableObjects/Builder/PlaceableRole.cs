using UnityEngine;

public enum PlaceableRole { Road, Deco, Building }

[CreateAssetMenu(menuName = "Builder/Placeable", fileName = "PlaceableItem")]
public class PlaceableItem : ScriptableObject
{
    public string DisplayName;
    public PlaceableRole Role;
    public GameObject Prefab;
    public Sprite Thumbnail;
}