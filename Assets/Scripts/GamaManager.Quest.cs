using System.Collections.Generic;
using UnityEngine;

public partial class GameManager 
{
    [SerializeField] private QuestDatabase questDatabase;
    
    public QuestMetaData GetQuestData(string questId)
    {
        if (questDatabase == null)
        {
            Debug.LogWarning("[GameManager] QuestDatabase is null");
            return null;
        }
    
        return questDatabase.GetQuest(questId);
    }
}