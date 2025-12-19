using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save()
    {
        var progress = PlayerDialogueProgress.Singleton;
        var data = progress.ToSaveData();

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"[SaveSystem] Saved to {SavePath}");
    }

    public static void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveSystem] No save file, starting fresh");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<PlayerDialogueProgress.SaveData>(json);

        PlayerDialogueProgress.Singleton.FromSaveData(data);

        Debug.Log($"[SaveSystem] Loaded from {SavePath}");
    }
}