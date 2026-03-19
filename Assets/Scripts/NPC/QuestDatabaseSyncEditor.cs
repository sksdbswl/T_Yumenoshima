#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DS.ScriptableObjects;

public static class QuestDatabaseSyncEditor
{
    [MenuItem("Tools/Quests/Sync From Dialogues")]
    public static void SyncFromDialogues()
    {
        var db = FindQuestDatabase();
        if (db == null)
        {
            Debug.LogError("[QuestSync] QuestDatabase asset not found.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:DSDialogueContainerSO");
        Dictionary<string, QuestMetaData> discoveredQuests = new Dictionary<string, QuestMetaData>();

        for (int g = 0; g < guids.Length; g++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[g]);
            var container = AssetDatabase.LoadAssetAtPath<DSDialogueContainerSO>(path);
            if (container == null) continue;

            CollectFromContainer(container, discoveredQuests);
        }

        db.BuildMap();

        int added = 0;
        int updated = 0;

        foreach (var pair in discoveredQuests.OrderBy(x => x.Key))
        {
            string questId = pair.Key;
            QuestMetaData found = pair.Value;

            if (string.IsNullOrWhiteSpace(questId))
                continue;

            var existing = db.GetQuest(questId);
            if (existing == null)
            {
                db.quests.Add(new QuestMetaData
                {
                    questId = found.questId,
                    questName = found.questName,
                    description = found.description,
                    money = found.money,
                    exp = found.exp,
                    cleanliness = found.cleanliness
                });

                added++;
                Debug.Log($"[QuestSync] Added: {questId}");
            }
            else
            {
                existing.questName = found.questName;
                existing.description = found.description;
                existing.money = found.money;
                existing.exp = found.exp;
                existing.cleanliness = found.cleanliness;

                updated++;
                Debug.Log($"[QuestSync] Updated: {questId}");
            }
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[QuestSync] Done. Found={discoveredQuests.Count}, Added={added}, Updated={updated}");
    }

    private static QuestDatabase FindQuestDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:QuestDatabase");
        if (guids.Length == 0) return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<QuestDatabase>(path);
    }

    private static void CollectFromContainer(DSDialogueContainerSO container, Dictionary<string, QuestMetaData> questMap)
    {
        if (container == null) return;

        if (container.DialogueGroups != null)
        {
            foreach (var pair in container.DialogueGroups)
            {
                var list = pair.Value;
                if (list == null) continue;

                for (int i = 0; i < list.Count; i++)
                    CollectFromNode(list[i], questMap);
            }
        }

        if (container.UngroupedDialogues != null)
        {
            for (int i = 0; i < container.UngroupedDialogues.Count; i++)
                CollectFromNode(container.UngroupedDialogues[i], questMap);
        }
    }

    private static void CollectFromNode(DSDialogueSO node, Dictionary<string, QuestMetaData> questMap)
    {
        if (node == null || node.Actions == null) return;

        for (int i = 0; i < node.Actions.Count; i++)
        {
            var action = node.Actions[i];
            if (action == null) continue;
            if (action.type != DSDialogueActionType.SetQuestState) continue;

            string questId = null;

            if (action.questMeta != null && !string.IsNullOrWhiteSpace(action.questMeta.questId))
                questId = action.questMeta.questId;
            else if (!string.IsNullOrWhiteSpace(action.questId))
                questId = action.questId;

            if (string.IsNullOrWhiteSpace(questId))
                continue;

            var meta = new QuestMetaData
            {
                questId = questId,
                questName = action.questMeta?.questName ?? questId,
                description = action.questMeta?.description ?? "",
                money = action.questMeta?.money ?? 0,
                exp = action.questMeta?.exp ?? 0,
                cleanliness = action.questMeta?.cleanliness ?? 0
            };

            if (!questMap.ContainsKey(questId))
            {
                questMap.Add(questId, meta);
            }
            else
            {
                // 나중 노드가 더 풍부한 정보를 갖고 있으면 덮어쓰기
                var existing = questMap[questId];

                if (string.IsNullOrWhiteSpace(existing.questName) && !string.IsNullOrWhiteSpace(meta.questName))
                    existing.questName = meta.questName;

                if (string.IsNullOrWhiteSpace(existing.description) && !string.IsNullOrWhiteSpace(meta.description))
                    existing.description = meta.description;

                if (existing.money == 0) existing.money = meta.money;
                if (existing.exp == 0) existing.exp = meta.exp;
                if (existing.cleanliness == 0) existing.cleanliness = meta.cleanliness;
            }
        }
    }
}
#endif