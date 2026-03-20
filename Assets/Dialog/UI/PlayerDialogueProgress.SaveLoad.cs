using System;
using UnityEngine;

public partial class PlayerDialogueProgress
{
    private const string SaveKey = "PlayerDialogueProgress_Save";

    public void SaveToPrefs()
    {
        var data = ToSaveData();
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[PlayerDialogueProgress] Saved: {json}");
    }

    public void LoadFromPrefs()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("[PlayerDialogueProgress] No save data found.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[PlayerDialogueProgress] Save json is empty.");
            return;
        }

        var data = JsonUtility.FromJson<SaveData>(json);
        FromSaveData(data);

        Debug.Log($"[PlayerDialogueProgress] Loaded: {json}");
    }

    public void ClearPrefsSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        Debug.Log("[PlayerDialogueProgress] Save cleared.");
    }
}