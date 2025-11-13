using UnityEngine;

public static class PlayerProgress
{
    public static int GetStage(int npcId) =>
        PlayerPrefs.GetInt($"stage_{npcId}", 1);

    public static void SetStage(int npcId, int stage)
    {
        PlayerPrefs.SetInt($"stage_{npcId}", stage);
        PlayerPrefs.Save();
    }

    public static bool IsStorySeen(string key) =>
        PlayerPrefs.GetInt($"seen_{key}", 0) == 1;

    public static void MarkStorySeen(string key)
    {
        PlayerPrefs.SetInt($"seen_{key}", 1);
        PlayerPrefs.Save();
    }
    
    // 현재 진행 중인 Order
    public static int GetOrder(int npcId, int stage) =>
        PlayerPrefs.GetInt($"order_{npcId}_{stage}", 0);

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