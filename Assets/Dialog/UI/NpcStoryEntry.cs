[System.Serializable]
public class NpcStoryEntry
{
    public string npcId;   // "VillageChief", "BlackSmith" 등
    public int storyStage; // 이 NPC와 어느 단계까지 진행했는지 (0,1,2,...)
}