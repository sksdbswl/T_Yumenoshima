using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        SaveSystem.Load();  // 저장된 진행도 불러오기 (없으면 무시)
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save();  // 종료 시 자동 저장
    }
}