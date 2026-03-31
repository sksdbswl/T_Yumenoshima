public class BuilderUI : UIBase
{
    public void Load()
    {
        PlacementManager.Singleton.Load();
    }
    
    public void Save()
    {
        PlacementManager.Singleton.Save();
    }
    
    public void Clear()
    {
        OnPlacementReset();
    }
    
    public void OnPlacementReset()
    {
        PlacementManager.Singleton.ClearAll();

        var objects = FindObjectsOfType<PlaceableInteraction>();
        foreach (var obj in objects)
            Destroy(obj.gameObject);

        PlacementSystem placement = FindObjectOfType<PlacementSystem>();
        placement.RebuildFromSave(PlacementManager.Singleton.PlacedObjects);
    }
}
