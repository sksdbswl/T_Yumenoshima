using System;
using System.Collections.Generic;
using DS.ScriptableObjects;
using UnityEngine;

/// <summary>
/// 플레이어 대화/퀘스트/플래그 진행도 저장소
/// - NPC별 스토리 진행도(npcId -> storyStage)
/// - 퀘스트 진행도(questId -> state/step)
/// - 플래그
/// </summary>
public partial class PlayerDialogueProgress : SingletonBase<PlayerDialogueProgress>
{
    [Header("Main Story")]
    public int mainStoryStage = 0;

    [Header("NPC Story")]
    public List<NpcStoryEntry> npcStories = new List<NpcStoryEntry>();

    [Header("Quests")]
    public List<QuestEntry> quests = new List<QuestEntry>();

    [Header("Flags")]
    public List<string> flags = new List<string>();

    // =========================
    // ✅ (선택) 런타임 캐시: Find 반복 줄이기
    // =========================
    private Dictionary<string, NpcStoryEntry> npcStoryMap;
    private Dictionary<string, QuestEntry> questMap;
    private HashSet<string> flagSet;

    private bool cacheBuilt;

    private void EnsureCache()
    {
        if (cacheBuilt) return;

        npcStoryMap = new Dictionary<string, NpcStoryEntry>(StringComparer.Ordinal);
        for (int i = 0; i < npcStories.Count; i++)
        {
            var e = npcStories[i];
            if (e == null || string.IsNullOrEmpty(e.npcId)) continue;
            npcStoryMap[e.npcId] = e;
        }

        questMap = new Dictionary<string, QuestEntry>(StringComparer.Ordinal);
        for (int i = 0; i < quests.Count; i++)
        {
            var q = quests[i];
            if (q == null || string.IsNullOrEmpty(q.questId)) continue;
            questMap[q.questId] = q;
        }

        flagSet = new HashSet<string>(flags, StringComparer.Ordinal);
        cacheBuilt = true;
    }

    private void InvalidateCache() => cacheBuilt = false;

    // =========================
    // ✅ NPC Story API
    // =========================

    /// <summary>npcId의 스토리 진행 단계(없으면 0)</summary>
    public int GetNpcStoryStage(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return 0;

        EnsureCache();
        if (npcStoryMap.TryGetValue(npcId, out var entry))
            return entry.storyStage;

        return 0;
    }

    /// <summary>npcId의 스토리 단계 설정(없으면 생성)</summary>
    public void SetNpcStoryStage(string npcId, int stage)
    {
        if (string.IsNullOrEmpty(npcId)) return;

        stage = Mathf.Max(0, stage);

        EnsureCache();
        if (!npcStoryMap.TryGetValue(npcId, out var entry) || entry == null)
        {
            entry = new NpcStoryEntry { npcId = npcId, storyStage = stage };
            npcStories.Add(entry);
            npcStoryMap[npcId] = entry;
        }
        else
        {
            entry.storyStage = stage;
        }
    }

    /// <summary>스토리를 "완료" 처리: 최소 stage 이상으로 올림</summary>
    public void CompleteNpcStoryStage(string npcId, int completedStage)
    {
        int cur = GetNpcStoryStage(npcId);
        if (cur < completedStage)
            SetNpcStoryStage(npcId, completedStage);
    }

    /// <summary>npcId가 특정 stage 이상인지(= 해당 스토리 진행/완료 여부 확인용)</summary>
    public bool IsNpcStoryAtLeast(string npcId, int stage)
    {
        return GetNpcStoryStage(npcId) >= stage;
    }

    // =========================
    // ✅ Quest API
    // =========================

    public QuestState GetQuestState(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return QuestState.NotStarted;

        EnsureCache();
        if (questMap.TryGetValue(questId, out var q) && q != null)
            return q.state;

        return QuestState.NotStarted;
    }

    public int GetQuestStep(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return 0;

        EnsureCache();
        if (questMap.TryGetValue(questId, out var q) && q != null)
            return q.step;

        return 0;
    }

    public void SetQuestState(string questId, QuestState state, int step = 0)
    {
        if (string.IsNullOrEmpty(questId)) return;

        step = Mathf.Max(0, step);

        EnsureCache();
        if (!questMap.TryGetValue(questId, out var q) || q == null)
        {
            q = new QuestEntry { questId = questId, state = state, step = step };
            quests.Add(q);
            questMap[questId] = q;
        }
        else
        {
            q.state = state;
            q.step = step;
        }
    }

    public bool IsQuestAccepted(string questId) => GetQuestState(questId) == QuestState.Accepted;
    public bool IsQuestCompleted(string questId) => GetQuestState(questId) == QuestState.Completed;

    // =========================
    // ✅ Flag API
    // =========================

    public bool HasFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return false;

        EnsureCache();
        return flagSet.Contains(flag);
    }

    public void SetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;

        EnsureCache();
        if (flagSet.Add(flag))
            flags.Add(flag);
    }

    // =========================
    // ✅ "상태 확인" 규칙 API (네가 말한 정책 반영)
    // =========================

    /// <summary>
    /// Stage=1 정책 예시:
    /// - npcId의 storyStage가 0이면 Story1 가능
    /// - storyStage가 1(Story1 완료)면 Stage=1에선 Story2 불가
    ///
    /// 여기서 storyIndex는 "Story1=1, Story2=2..." 같은 규칙으로 쓰면 편함.
    /// </summary>
    public bool CanStartNpcStory(string npcId, int storyIndex, int currentStage)
    {
        // 예: Stage1에서는 Story1만 가능, Story2+는 잠김
        if (currentStage == 1 && storyIndex >= 2) return false;

        // "다음 스토리만 시작 가능" 정책 (이미 2까지 했으면 3만 가능)
        int npcStage = GetNpcStoryStage(npcId);

        // storyIndex=1 시작 조건: npcStage < 1
        // storyIndex=2 시작 조건: npcStage < 2 AND (보통은 1 완료 필요)
        // 여기선 "npcStage + 1 == storyIndex" 로 강제하면 깔끔
        return (npcStage + 1) == storyIndex;
    }

    /// <summary>
    /// Quest1 진행 가능 조건:
    /// - Story1 진행완료면 가능
    /// - 아니면 불가
    /// </summary>
    public bool CanStartQuest_RequiresNpcStory(string questId, string npcId, int requiredStoryStage, int currentStage)
    {
        // 스테이지 조건도 넣고 싶으면 여기서 처리(예: stage==1만 허용 등)
        // if (currentStage != 1) return false;

        // 선행 스토리 조건
        if (!IsNpcStoryAtLeast(npcId, requiredStoryStage))
            return false;

        // 이미 수락/완료 상태면 "시작"의 의미가 달라질 수 있으니 정책 선택
        // 여기선 NotStarted만 "시작 가능"으로 처리
        return GetQuestState(questId) == QuestState.NotStarted;
    }

    /// <summary>
    /// Daily 랜덤 반복 가능 조건:
    /// - Story1 완료 AND Quest1 수락 완료(Accepted)
    /// </summary>
    public bool CanUseDailyDialogue(string npcId, int requiredStoryStage, string questIdRequiredAccepted)
    {
        if (!IsNpcStoryAtLeast(npcId, requiredStoryStage)) return false;
        return GetQuestState(questIdRequiredAccepted) == QuestState.Accepted;
    }

    // =========================
    // JSON 세이브/로드 DTO
    // =========================

    [Serializable]
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

        InvalidateCache(); // ✅ 로드 후 캐시 재빌드 필요
    }
    
    
    // DSDialogueSO currentNode;
    //
    // ///-------------------------------------------------------------------
    // public int LoadProgress(DialogueGroupType type, DSDialogueSO current)
    // {
    //     currentNode = current;
    //     string key = MakeProgressKey(type);
    //     return PlayerPrefs.GetInt(key, 0);
    // }
    //
    // public void SaveProgress(DialogueGroupType type)
    // {
    //     string key = MakeProgressKey(type);
    //     int prev = PlayerPrefs.GetInt(key, 0);
    //
    //     // 더 큰 값만 저장(진행도는 상승만)
    //     if (currentNode.StageId > prev)
    //     {
    //         PlayerPrefs.SetInt(key, currentNode.StageId);
    //         PlayerPrefs.Save();
    //     }
    // }
    //
    // public string MakeProgressKey(DialogueGroupType type)
    // {
    //     return type switch
    //     {
    //         DialogueGroupType.NpcStory => $"{currentNode.NpcId}_story",
    //         DialogueGroupType.Quest    => string.IsNullOrEmpty(currentNode.NpcId) ? $"{currentNode.NpcId}_quest" : $"{currentNode.NpcId}_quest_{questId}",
    //         _                          => $"{currentNode.NpcId}_{type}"
    //     };
    // }
}
