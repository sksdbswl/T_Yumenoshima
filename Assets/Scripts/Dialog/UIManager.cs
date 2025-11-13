using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class UIManager:SingletonBase<UIManager>
{
    public async void OnClickGameStart()
    {
        await GameManager.Singleton.EnterIngameAsync();
    }
}