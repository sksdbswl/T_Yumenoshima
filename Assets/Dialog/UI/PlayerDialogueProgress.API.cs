using System;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerDialogueProgress
{
    public enum NextDialogueType { None, NpcStory, Quest, Daily }

    [Serializable]
    public struct NextDialogueResult
    {
        public NextDialogueType type;
        public int stageKey;        // 어떤 stageKey에서 나온 결과인지
        public int storyIndex;      // story면 사용
        public string questId;      // quest면 사용
        public string reason;
    }

    // -------------------------
    // 인스펙터 입력용 (List)
    // -------------------------
    [Serializable]
    public class StageStoryPair
    {
        [Range(1, 100)] public int stageKey = 1;
        public StoryGate gate;
    }

    [Serializable]
    public class StageQuestPair
    {
        [Range(1, 100)] public int stageKey = 1;
        public QuestGate gate;
    }

    [Serializable]
    public class StageDailyPair
    {
        [Range(1, 100)] public int stageKey = 1;
        public DailyGate gate;
    }

    [Serializable]
    public class StoryGate
    {
        public int storyIndex = 1;                 // 완료 시 npcStoryStage를 여기까지 올림
        public int requiredNpcStoryStage = 0;      // 선행 스토리 stage
        public string requiredFlag;                // 옵션
    }

    [Serializable]
    public class QuestGate
    {
        public string questId;
        public int requiredNpcStoryStage = 0;
        public QuestState requiredCurrentState = QuestState.NotStarted;
        public string requiredFlag;
    }

    [Serializable]
    public class DailyGate
    {
        public int requiredNpcStoryStage = 0;
        public string requiredQuestId;
        public QuestState requiredQuestState = QuestState.Accepted;
        public string requiredFlag;
    }

    [Serializable]
    public class NpcStagePlan
    {
        public string npcId;

        // 인스펙터 입력은 List
        public List<StageStoryPair> storiesByStage = new();
        public List<StageQuestPair> questsByStage = new();
        public List<StageDailyPair> dailyByStage = new();

        // ✅ 런타임 캐시(Dictionary)
        [NonSerialized] public Dictionary<int, StoryGate> storyMap;
        [NonSerialized] public Dictionary<int, QuestGate> questMap;
        [NonSerialized] public Dictionary<int, DailyGate> dailyMap;
        [NonSerialized] public bool built;

        public void Build()
        {
            if (built) return;

            storyMap = new Dictionary<int, StoryGate>();
            questMap = new Dictionary<int, QuestGate>();
            dailyMap = new Dictionary<int, DailyGate>();

            for (int i = 0; i < storiesByStage.Count; i++)
            {
                var p = storiesByStage[i];
                if (p == null || p.gate == null) continue;
                storyMap[p.stageKey] = p.gate; // stageKey 중복이면 마지막이 덮어씀
            }

            for (int i = 0; i < questsByStage.Count; i++)
            {
                var p = questsByStage[i];
                if (p == null || p.gate == null || string.IsNullOrEmpty(p.gate.questId)) continue;
                questMap[p.stageKey] = p.gate;
            }

            for (int i = 0; i < dailyByStage.Count; i++)
            {
                var p = dailyByStage[i];
                if (p == null || p.gate == null) continue;
                dailyMap[p.stageKey] = p.gate;
            }

            built = true;
        }
    }

    [Header("Stage Plans (per NPC)")]
    [SerializeField] private List<NpcStagePlan> npcStagePlans = new();

    // npcId -> plan 캐시
    private Dictionary<string, NpcStagePlan> planMap;
    private bool planCacheBuilt;

    private void EnsurePlanCache()
    {
        if (planCacheBuilt) return;

        planMap = new Dictionary<string, NpcStagePlan>(StringComparer.Ordinal);
        for (int i = 0; i < npcStagePlans.Count; i++)
        {
            var p = npcStagePlans[i];
            if (p == null || string.IsNullOrEmpty(p.npcId)) continue;
            p.Build();
            planMap[p.npcId] = p;
        }

        planCacheBuilt = true;
    }

    private NpcStagePlan GetPlan(string npcId)
    {
        EnsurePlanCache();
        return planMap.TryGetValue(npcId, out var p) ? p : null;
    }

    /// <summary>
    /// 핵심: stage까지(<=stage)의 stageKey들을 순차적으로 처리해서
    /// 1) 미완료 스토리 있으면 그거부터
    /// 2) 아니면 시작가능 퀘스트
    /// 3) 아니면 데일리
    /// </summary>
    public NextDialogueResult GetNextDialogueForNpc_ByStageKeys(string npcId, int stage)
    {
        stage = Mathf.Clamp(stage, 1, 100);

        var plan = GetPlan(npcId);
        if (plan == null)
            return new NextDialogueResult { type = NextDialogueType.None, reason = $"No plan for npcId={npcId}" };

        int npcStoryStage = GetNpcStoryStage(npcId);

        // -------------------------
        // 1) Story: stageKey 1..stage 중 "아직 완료 안 된 스토리"를 앞에서부터
        // -------------------------
        for (int s = 1; s <= stage; s++)
        {
            if (!plan.storyMap.TryGetValue(s, out var gate) || gate == null) continue;

            // 이미 storyIndex까지 완료되어 있으면 스킵
            if (npcStoryStage >= gate.storyIndex) continue;

            // 선행 스토리 조건
            if (gate.requiredNpcStoryStage > 0 && npcStoryStage < gate.requiredNpcStoryStage)
                continue;

            // 플래그 조건
            if (!string.IsNullOrEmpty(gate.requiredFlag) && !HasFlag(gate.requiredFlag))
                continue;

            // 이 스토리가 다음 진행 대상
            return new NextDialogueResult
            {
                type = NextDialogueType.NpcStory,
                stageKey = s,
                storyIndex = gate.storyIndex,
                reason = $"Story at stageKey={s} (storyIndex={gate.storyIndex})"
            };
        }

        // -------------------------
        // 2) Quest: stageKey 1..stage 중 시작 가능한 것
        // -------------------------
        for (int s = 1; s <= stage; s++)
        {
            if (!plan.questMap.TryGetValue(s, out var gate) || gate == null) continue;

            if (gate.requiredNpcStoryStage > 0 && npcStoryStage < gate.requiredNpcStoryStage)
                continue;

            if (!string.IsNullOrEmpty(gate.requiredFlag) && !HasFlag(gate.requiredFlag))
                continue;

            if (GetQuestState(gate.questId) != gate.requiredCurrentState)
                continue;

            return new NextDialogueResult
            {
                type = NextDialogueType.Quest,
                stageKey = s,
                questId = gate.questId,
                reason = $"Quest at stageKey={s} (questId={gate.questId})"
            };
        }

        // -------------------------
        // 3) Daily: 보통 "현재 stage" 기준으로만 체크(원하면 <=stage로 바꿔도 됨)
        // -------------------------
        if (plan.dailyMap.TryGetValue(stage, out var daily) && daily != null)
        {
            bool okStory = daily.requiredNpcStoryStage <= 0 || npcStoryStage >= daily.requiredNpcStoryStage;
            bool okFlag = string.IsNullOrEmpty(daily.requiredFlag) || HasFlag(daily.requiredFlag);
            bool okQuest = string.IsNullOrEmpty(daily.requiredQuestId) || GetQuestState(daily.requiredQuestId) == daily.requiredQuestState;

            if (okStory && okFlag && okQuest)
            {
                return new NextDialogueResult
                {
                    type = NextDialogueType.Daily,
                    stageKey = stage,
                    reason = $"Daily available at stageKey={stage}"
                };
            }
        }

        return new NextDialogueResult { type = NextDialogueType.None, reason = "No available dialogue" };
    }
    
    
    /// <summary>
    /// 현재 Accepted 상태인 퀘스트들을 반환
    /// </summary>
    public List<QuestEntry> GetAcceptedQuests()
    {
        EnsureCache();

        List<QuestEntry> result = new List<QuestEntry>();

        for (int i = 0; i < quests.Count; i++)
        {
            var q = quests[i];
            if (q == null) continue;

            if (q.state == QuestState.Accepted)
                result.Add(q);
        }

        return result;
    }

    /// <summary>
    /// 현재 Accepted 상태인 퀘스트 ID 목록 반환
    /// </summary>
    public List<string> GetAcceptedQuestIds()
    {
        EnsureCache();

        List<string> result = new List<string>();

        for (int i = 0; i < quests.Count; i++)
        {
            var q = quests[i];
            if (q == null || string.IsNullOrEmpty(q.questId)) continue;

            if (q.state == QuestState.Accepted)
                result.Add(q.questId);
        }

        return result;
    }

    /// <summary>
    /// Accepted 상태인 첫 번째 퀘스트 반환(없으면 null)
    /// </summary>
    public QuestEntry GetFirstAcceptedQuest()
    {
        EnsureCache();

        for (int i = 0; i < quests.Count; i++)
        {
            var q = quests[i];
            if (q == null) continue;

            if (q.state == QuestState.Accepted)
                return q;
        }

        return null;
    }
}
