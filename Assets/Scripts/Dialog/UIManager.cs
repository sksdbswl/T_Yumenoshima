using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

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
        PlacementSaveManager.Singleton.Save();
    }
    
    public async void OnClickGameStart()
    {
        await GameManager.Singleton.EnterIngameAsync();
    }
}