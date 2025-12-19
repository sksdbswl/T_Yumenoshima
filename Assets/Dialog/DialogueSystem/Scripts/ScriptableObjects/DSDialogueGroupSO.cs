using UnityEngine;

public enum DialogueGroupType
{
    MainStory,
    NpcStory,
    Quest,
    Daily
}

public class DSDialogueGroupSO : ScriptableObject
{
    [field: SerializeField] public string GroupName { get; private set; }

    [Header("Meta")]
    [field: SerializeField] public DialogueGroupType GroupType { get; private set; }
    [field: SerializeField] public string NpcId { get; private set; }
    [field: SerializeField] public string QuestId { get; private set; }

    public void Initialize(string groupName)
    {
        GroupName = groupName;
    }
    
    public void SetMeta(DialogueGroupType type, string npcId, string questId)
    {
        GroupType = type;
        NpcId = npcId;
        QuestId = questId;
    }
}