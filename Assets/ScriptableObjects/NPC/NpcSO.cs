using UnityEngine;

public enum JobType
{
    None,
    Doctor,
    Firefighter,
    // Farmer, Police 등등 추가
}

[CreateAssetMenu(menuName = "Npc/NpcRole", fileName = "Npc")]
public class NpcSO:ScriptableObject
{
    public int BuilderId;
    public string Name;
    public JobType Job;
    public GameObject Prefab;
    public Sprite Thumbnail;
    public Vector3 spawnPoint;
    public int Stage;
}