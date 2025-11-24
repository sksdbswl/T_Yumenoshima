using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public enum RoutineState
{
    Morning,
    Noon,
    Night,
}

public class GameManager : SingletonBase<GameManager>
{
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
    
    //public int Stage = 1;
    
    private void Start()
    {
        //StartCoroutine(DayRoutineCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Stage++;
            Debug.Log($"Stage Changed: {Stage}");
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
            "Builder"
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
        if (!ok) return;

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

            var interaction = npcObj.GetComponent<NpcInteraction>();
            interaction.npcSO = npcData;
        }
    }

    // public void SpawnNpcForStage(int stage)
    // {
    //     var table = AssetManager.Singleton.GetNpcDataSO();
    //     
    //     foreach (var npcData in table.Items.Values)
    //     {
    //         if (npcData.Stage != stage)
    //             continue;
    //
    //         var npcObj = AssetManager.Singleton.InstantiateNpcModel(npcData.Name);
    //         npcObj.transform.position = npcData.spawnPoint;
    //         npcObj.GetComponent<NpcInteraction>().npcSO = npcData;
    //     }
    // }

    /// <summary>
    /// 특정 npc 스폰
    /// </summary>
    /// <param name="stage"></param>
    /// <summary>
    /// 특정 npc 스폰
    /// </summary>
    public void SpawnNpc(int id)
    {
        var table = AssetManager.Singleton.GetNpcDataSO();
    
        if (!table.Items.TryGetValue(id, out var npcSO))
        {
            Debug.LogError($"NPC ID {id} not found");
            return;
        }

        int worldStage = Stage; // GameManager의 현재 월드 스테이지

        // 현재 스테이지에서 등장 가능한 NPC인지 체크
        if (worldStage < npcSO.WorldStageMin || worldStage > npcSO.WorldStageMax)
            return;

        var npcObj = AssetManager.Singleton.InstantiateNpcModel(npcSO.Prefab);
    
        npcObj.transform.position = npcSO.spawnPoint;
    
        npcObj.GetComponent<NpcInteraction>().npcSO = npcSO;
    }

    
    /// <summary>
    /// 인게임 루틴 전환 : npc 상태 변경 또는 player 행동 제약 ( 임시 5분마다 변경 )
    /// </summary>
    
    public event System.Action<RoutineState> OnRoutineChanged;
    public RoutineState CurrentState { get; private set; }

    public IEnumerator DayRoutineCoroutine()
    {
        while (true)
        {
            SetState(RoutineState.Morning);
            yield return new WaitForSeconds(300f); // 300f = 5분

            SetState(RoutineState.Noon);
            yield return new WaitForSeconds(10f);

            SetState(RoutineState.Night);
            yield return new WaitForSeconds(10f);
        }
    }

    void SetState(RoutineState state)
    {
        CurrentState = state;
        Debug.Log($"State Changed: {state}");
        
        // TODO: 여기서 조명 변경, NPC 상태 변경 등 처리
        OnRoutineChanged?.Invoke(state); 
    }
}