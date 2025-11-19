using UnityEngine;

public class UIManager:SingletonBase<UIManager>
{
    void Awake()
    {
        base.Awake();
        PlacementSaveManager.Singleton.Load();
    }
    
    public void OnPlacementSave()
    {
        PlacementSaveManager.Singleton.Save();
    }
    
    public void OnPlacementReset()
    {
        PlacementSaveManager.Singleton.ClearAll();
        
        var objects = FindObjectsOfType<PlaceableObject>();
        foreach (var obj in objects)
            Destroy(obj.gameObject);
        
        PlacementSystem placement = FindObjectOfType<PlacementSystem>();
        placement.RebuildFromSave(PlacementSaveManager.Singleton.PlacedObjects);
    }
    
    public void OnPlacementReload()
    {
        PlacementSaveManager.Singleton.Load();
    }
    
    public async void OnClickGameStart()
    {
        await GameManager.Singleton.EnterIngameAsync();
    }
}