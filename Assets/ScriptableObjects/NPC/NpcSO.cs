using UnityEngine;

public enum NpcRole { Basic, Special }
    
[CreateAssetMenu(menuName = "Npc/NpcRole", fileName = "Npc")]
public class NpcSO:ScriptableObject
{
    public int Id;
    public string Name;
    public NpcRole Role;
    public GameObject Prefab;
    public Sprite Thumbnail;
    public Vector3 spawnPoint;
    public int Stage;
}