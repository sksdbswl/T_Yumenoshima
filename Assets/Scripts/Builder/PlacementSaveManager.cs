using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlacementSaveData
{
    public PlacedObjectData[] objects;
}

public class PlacementSaveManager : SingletonBase<PlacementSaveManager>
{
    private List<PlacedObjectData> _placedObjects = new List<PlacedObjectData>();
    public List<PlacedObjectData> PlacedObjects => _placedObjects;
    
    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "placement.json");

    public void RegisterPlacedObject(PlacedObjectData data)
    {
        _placedObjects.Add(data);
        
        Debug.Log("배치완료:: 정보 리스트에 저장 됨");
    }

    public void ClearAll()
    {
        _placedObjects.Clear();
    }

    public void Save()
    {
        var wrapper = new PlacementSaveData
        {
            objects = _placedObjects.ToArray()
        };
    
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Placement saved to: {SavePath}");
    }
    
    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No placement save file found.");
            return;
        }
    
        // 1) json 읽고
        string json = File.ReadAllText(SavePath);
        // 2) PlacedObjectData[]로 역직렬화
        var wrapper = JsonUtility.FromJson<PlacementSaveData>(json);
        _placedObjects = new List<PlacedObjectData>(wrapper.objects);
        
        // 실제 배치 복원은 여기서 PlacementSystem이나 별도 팩토리로 넘겨서 처리
        PlacementSystem placement = FindObjectOfType<PlacementSystem>();
        if (placement != null)
        {
            placement.RebuildFromSave(_placedObjects);
        }

        // 임시로 npc 바로 스폰 적용, 추후 제거 필요
        OnGameStart();
    }
    
    public async void OnGameStart()
    {
        await GameManager.Singleton.EnterIngameAsync();
    }
}