// 퀘스트 진행도
[System.Serializable]
public class QuestEntry
{
    public string questId;    // "HUNT_SLIME"
    public QuestState state;
    public int step;          // 퀘스트 내부 단계(옵션)
}