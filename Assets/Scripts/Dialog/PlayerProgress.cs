using UnityEngine;

public static class PlayerProgress
{
    // ─────────────────────
    // Stage 관리 (NPC별)
    // ─────────────────────
    public static int GetStage(int npcId) =>
        PlayerPrefs.GetInt($"stage_{npcId}", 1);   // 기본 Stage = 1

    public static void SetStage(int npcId, int stage)
    {
        PlayerPrefs.SetInt($"stage_{npcId}", stage);
        PlayerPrefs.Save();
    }

    // ─────────────────────
    // Order 관리 (NPC + Stage별)
    // ─────────────────────
    public static int GetOrder(int npcId, int stage) =>
        PlayerPrefs.GetInt($"order_{npcId}_{stage}", 0); // 기본 Order = 0

    public static void SetOrder(int npcId, int stage, int order)
    {
        PlayerPrefs.SetInt($"order_{npcId}_{stage}", order);
        PlayerPrefs.Save();
    }

    public static void ResetOrder(int npcId, int stage)
    {
        PlayerPrefs.SetInt($"order_{npcId}_{stage}", 0);
        PlayerPrefs.Save();
    }
}