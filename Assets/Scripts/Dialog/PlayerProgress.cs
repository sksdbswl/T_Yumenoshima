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

    public static bool IsStorySeen(int key) =>
        PlayerPrefs.GetInt($"seen_{key}", 0) == 1;

    public static void MarkStorySeen(int key)
    {
        PlayerPrefs.SetInt($"seen_{key}", 1);
        PlayerPrefs.Save();
    }
}