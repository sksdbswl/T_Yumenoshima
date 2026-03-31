using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum RoutineState
{
    Morning,
    Noon,
    Night,
}
public partial class GameManager : SingletonBase<GameManager>
{
    private readonly List<Npc> _spawnedNpcStatuses = new();
    public IReadOnlyList<Npc> SpawnedNpcStatuses => _spawnedNpcStatuses;
    
    // 씬에 대한 정보, 이름 변경해줘도 좋을 듯 :: 해당 값을 이용해서 npc spawn
    private int _stage = 1;
    public int Stage
    {
        get => _stage;
        private set
        {
            if (_stage == value) return;
            _stage = value;
            OnStageChanged?.Invoke(_stage);
            Debug.Log($"Stage Changed: {_stage}");
        }
    }

    public event System.Action<int> OnStageChanged;
    
    private void Start()
    {
        TestLog();
        StartCoroutine(DayRoutineCoroutine());
    }
    
    private void TestLog()
    {
        var accepted = PlayerDialogueProgress.Singleton.GetAcceptedQuests();

        Debug.Log($"Accepted Quest Count: {accepted.Count}");

        for (int i = 0; i < accepted.Count; i++)
        {
            var entry = accepted[i];
            var data = GameManager.Singleton.GetQuestData(entry.questId);

            if (data != null)
                Debug.Log($"퀘스트 이름: {data.questName}");
            else
                Debug.LogWarning($"퀘스트 데이터 없음: {entry.questId}");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Stage++;
            SpawnNpcForStage(Stage);
            Debug.Log($"Stage Changed: {Stage}");
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            PlacementManager.Singleton.OnPlacementEdit();
        }
    }
    
    public async UniTask<bool> CheckAndDownloadStageResourcesAsync(
        int stage,
        Action<float> onProgress = null // 0 ~ 1
    )
    {
        // 0스테이지에서 필요한 라벨들
        // 나중엔 stage에 따라 리스트 구성 다르게 하면 됨
        var labels = new List<object>
        {
            "Npc",
            "Builder",
            "UI"
        };

        // 1) 두 라벨 전체의 다운로드 필요 용량 체크
        var sizeHandle = Addressables.GetDownloadSizeAsync(labels);
        long size = await sizeHandle.Task;
        Addressables.Release(sizeHandle);

        Debug.Log($"[AssetManager] Required download size (Npc + Builder): {size} bytes");

        if (size <= 0)
        {
            Debug.Log("[AssetManager] All resources already downloaded.");
            onProgress?.Invoke(1f); // 혹시 로딩바 쓰고 있으면 바로 100%
            return true;
        }

        // 2) 두 라벨의 의존성(번들)을 한 번에 다운로드
        var downloadHandle = Addressables.DownloadDependenciesAsync(labels);

        // 3) 로딩바 업데이트 루프
        while (!downloadHandle.IsDone)
        {
            float progress = downloadHandle.PercentComplete; // 0 ~ 1
            onProgress?.Invoke(progress);
            await UniTask.Yield(); // 다음 프레임까지 대기
        }

        // 다운로드 결과 확인 (보통 성공이면 Status == Succeeded)
        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("[AssetManager] Resource download complete. (Npc + Builder)");
            onProgress?.Invoke(1f);
            Addressables.Release(downloadHandle);
            return true;
        }
        else
        {
            Debug.LogError("[AssetManager] Resource download failed.");
            Addressables.Release(downloadHandle);
            return false;
        }
    }

    /// 인게임 진입 전체 흐름 (다운로드 → 씬 로드 → NPC 스폰)
    public async UniTask EnterIngameAsync()
    {
        // 1. 리소스 체크 & 다운로드
        bool ok = await CheckAndDownloadStageResourcesAsync(Stage);
        
        Debug.Log($"[GameManager] CheckAndDownloadStageResourcesAsync: {ok}");
        if (!ok) return;

        await UIManager.Show<BuilderUI>(UIList.BuilderUI);
        await UIManager.Show<PlayerHUD>(UIList.PlayerHUD);
        
        // 2. 씬 로드
        //await SceneManager.LoadSceneAsync("DialogScene").ToUniTask();

        // 3. 씬 로드 끝난 뒤 NPC 스폰
        SpawnNpcForStage(Stage);
    }

    public void SpawnNpcForStage(int worldStage)
    {
        var table = AssetManager.Singleton.GetNpcDataSO();

        foreach (var npcData in table.Items.Values)
        {
            // 이 스테이지에 등장 가능한 NPC인가?
            if (worldStage < npcData.WorldStageMin || worldStage > npcData.WorldStageMax)
                continue;

            // 실제 프리팹 생성
            var npcObj = AssetManager.Singleton.InstantiateNpcModel(npcData.Name);
            npcObj.transform.position = npcData.spawnPoint;

            var status = npcObj.GetComponent<Npc>();
            if (status != null)
            {
                _spawnedNpcStatuses.Add(status);
            }
        }
    }
}