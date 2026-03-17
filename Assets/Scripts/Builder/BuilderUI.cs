public class BuilderUI : UIBase
{
    public void Load()
    {
        PlacementSaveManager.Singleton.Load();
    }
    
    public void Save()
    {
        PlacementSaveManager.Singleton.Save();
    }
    
    public void Clear()
    {
        OnPlacementReset();
    }
    
    public void OnPlacementReset()
    {
        PlacementSaveManager.Singleton.ClearAll();

        var objects = FindObjectsOfType<PlaceableInteraction>();
        foreach (var obj in objects)
            Destroy(obj.gameObject);

        PlacementSystem placement = FindObjectOfType<PlacementSystem>();
        placement.RebuildFromSave(PlacementSaveManager.Singleton.PlacedObjects);
    }
}
