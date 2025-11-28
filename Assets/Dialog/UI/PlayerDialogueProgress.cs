using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueProgress : MonoBehaviour
{
    public static PlayerDialogueProgress Instance { get; private set; }

    [Header("Main Story")]
    public int mainStoryStage = 0;

    [Header("NPC Story")]
    public List<NpcStoryEntry> npcStories = new List<NpcStoryEntry>();

    [Header("Quests")]
    public List<QuestEntry> quests = new List<QuestEntry>();

    [Header("Flags")]
    public List<string> flags = new List<string>();   // "MetChief", "JobSelected" 등

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ───── 헬퍼 메서드들 ─────

    public int GetNpcStoryStage(string npcId)
    {
        var entry = npcStories.Find(e => e.npcId == npcId);
        return entry != null ? entry.storyStage : 0;
    }

    public void SetNpcStoryStage(string npcId, int stage)
    {
        var entry = npcStories.Find(e => e.npcId == npcId);
        if (entry == null)
        {
            entry = new NpcStoryEntry { npcId = npcId, storyStage = stage };
            npcStories.Add(entry);
        }
        else
        {
            entry.storyStage = stage;
        }
    }

    public QuestState GetQuestState(string questId)
    {
        var q = quests.Find(e => e.questId == questId);
        return q != null ? q.state : QuestState.NotStarted;
    }

    public void SetQuestState(string questId, QuestState state, int step = 0)
    {
        var q = quests.Find(e => e.questId == questId);
        if (q == null)
        {
            q = new QuestEntry { questId = questId, state = state, step = step };
            quests.Add(q);
        }
        else
        {
            q.state = state;
            q.step = step;
        }
    }

    public bool HasFlag(string flag) => flags.Contains(flag);

    public void SetFlag(string flag)
    {
        if (!flags.Contains(flag))
            flags.Add(flag);
    }

    // ───── JSON 세이브/로드용 DTO ─────

    [System.Serializable]
    public class SaveData
    {
        public int mainStoryStage;
        public List<NpcStoryEntry> npcStories;
        public List<QuestEntry> quests;
        public List<string> flags;
    }

    public SaveData ToSaveData()
    {
        return new SaveData
        {
            mainStoryStage = this.mainStoryStage,
            npcStories = new List<NpcStoryEntry>(this.npcStories),
            quests = new List<QuestEntry>(this.quests),
            flags = new List<string>(this.flags)
        };
    }

    public void FromSaveData(SaveData data)
    {
        if (data == null) return;

        mainStoryStage = data.mainStoryStage;
        npcStories = data.npcStories ?? new List<NpcStoryEntry>();
        quests = data.quests ?? new List<QuestEntry>();
        flags = data.flags ?? new List<string>();
    }
}
