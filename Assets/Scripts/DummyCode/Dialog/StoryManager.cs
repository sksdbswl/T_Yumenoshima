using Cysharp.Threading.Tasks;
using UnityEngine;

public class StoryManager : SingletonBase<StoryManager>
{
    // Shin의 npcId (NpcSO에 들어있는 Id와 맞추면 됨)
    public int ShinId = 1;

    public async void OnNpcStoryCleared(int npcId, int npcStoryStage)
    {
        // Shin 스토리가 아닐 경우는 무시
        //if (npcId != ShinId) return;

        // Shin 1장 끝났을 때 → 섬으로 가는 연출(월드 스테이지 2)
        if (npcStoryStage == 1)
        {
            //await GoToIslandCutsceneAsync();
        }

        // Shin 2장 끝났을 때 → 섬에서 자유 시작 등
        if (npcStoryStage == 2)
        {
            //await AfterIslandIntroAsync();
        }
    }

    private async UniTask GoToIslandCutsceneAsync()
    {
        Debug.Log("[StoryManager] Shin 1장 클리어 → 섬으로 가는 컷씬 시작");

        // 1) 플레이어 조작 잠금
        
        //PlayerController.Singleton.SetControlEnabled(false);

        // 2) 화면 페이드아웃, 카메라 연출, 배 타는 컷씬 등
        // 예: Timeline 재생, 또는 따로 만든 CutsceneManager 호출
        
        //await CutsceneManager.Singleton.PlayAsync("GoToIsland");

        // 3) 섬 씬 로드 or 위치 이동
        // await SceneManager.LoadSceneAsync("IslandScene").ToUniTask();
        // 또는 플레이어/카메라만 섬 위치로 텔레포트

        //GameManager.Singleton.Stage = 3; // 섬 도착 상태

        // 4) 플레이어 조작 잠금 해제
        //PlayerController.Singleton.SetControlEnabled(true);

        // 5) 섬 도착 후 Shin 대사 자동 시작 등
        // 예: Shin 자동 스폰 + 대화 시작
        //GameManager.Singleton.SpawnNpcForStage(GameManager.Singleton.Stage);
    }

    private async UniTask AfterIslandIntroAsync()
    {
        Debug.Log("[StoryManager] Shin 2장 클리어 → 이후 흐름 처리");

        // 예: 튜토리얼 끝, 자유 탐험 시작, 신규 기능 오픈 등
        //GameManager.Singleton.Stage = 4;
        await UniTask.Yield();
    }
}
