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
        PlacementSaveManager.Singleton.ClearAll();
    }
}
