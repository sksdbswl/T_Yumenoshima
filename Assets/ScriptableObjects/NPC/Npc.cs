using UnityEngine;

public enum NpcRole { Basic, Special }
    
[CreateAssetMenu(menuName = "Npc/NpcRole", fileName = "Npc")]
public class Npc:ScriptableObject
{
    public int Id;
    public string Name;
    public NpcRole Role;
    public GameObject Prefab;
    public Sprite Thumbnail;
}