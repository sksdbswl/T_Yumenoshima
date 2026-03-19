using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    public List<QuestMetaData> quests = new();
    private Dictionary<string, QuestMetaData> map;

    public void BuildMap()
    {
        map = new Dictionary<string, QuestMetaData>(StringComparer.Ordinal);

        for (int i = 0; i < quests.Count; i++)
        {
            var q = quests[i];
            if (q == null || string.IsNullOrEmpty(q.questId)) continue;
            map[q.questId] = q;
        }
    }

    public bool Contains(string questId)
    {
        if (map == null) BuildMap();
        return map.ContainsKey(questId);
    }

    public QuestMetaData GetQuest(string questId)
    {
        if (map == null) BuildMap();
        map.TryGetValue(questId, out var result);
        return result;
    }
}

[Serializable]
public class QuestMetaData
{
    public string questId;
    public string questName;
    [TextArea] public string description;
    public int money;
    public int exp;
    public int cleanliness;
}