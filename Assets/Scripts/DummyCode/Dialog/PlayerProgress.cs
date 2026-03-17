using UnityEngine;

public static class PlayerProgress
{
    // ─────────────────────
    // Builder 관리 (Object)
    // ─────────────────────
    private const string BUILDER_ID_KEY = "builder_total_cnt";

    public static int GetBuilder()
    {
        return PlayerPrefs.GetInt(BUILDER_ID_KEY, 0);
    }

    // ─────────────────────
    // NPC 스토리 Stage 관리 (NPC별)
    // ─────────────────────
    // 기본 = 1 (shin_s1_xx 같은 1부터 시작하는 구조 기준)
    public static int GetNpcStoryStage(int npcId) =>
        PlayerPrefs.GetInt($"npc_story_stage_{npcId}", 1);

    public static void SetNpcStoryStage(int npcId, int stage)
    {
        PlayerPrefs.SetInt($"npc_story_stage_{npcId}", stage);
        PlayerPrefs.Save();
    }

    // (원래 GetStage/SetStage를 쓰던 코드가 많다면, 아래처럼 래핑해둬도 됨)
    [System.Obsolete("Use GetNpcStoryStage instead.")]
    public static int GetStage(int npcId) => GetNpcStoryStage(npcId);

    [System.Obsolete("Use SetNpcStoryStage instead.")]
    public static void SetStage(int npcId, int stage) => SetNpcStoryStage(npcId, stage);

    // ─────────────────────
    // Order 관리 (NPC + NpcStoryStage별)
    // ─────────────────────
    public static int GetOrder(int npcId, int npcStoryStage) =>
        PlayerPrefs.GetInt($"order_{npcId}_{npcStoryStage}", 0); // 기본 Order = 0

    public static void SetOrder(int npcId, int npcStoryStage, int order)
    {
        PlayerPrefs.SetInt($"order_{npcId}_{npcStoryStage}", order);
        PlayerPrefs.Save();
    }

    public static void ResetOrder(int npcId, int npcStoryStage)
    {
        PlayerPrefs.SetInt($"order_{npcId}_{npcStoryStage}", 0);
        PlayerPrefs.Save();
    }
}