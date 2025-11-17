using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBase<GameManager>
{
    public int Stage = 0;

    public async UniTask<bool> CheckAndDownloadStageResourcesAsync(int stage)
    {
        string label = "Npc";

        long size = await Addressables.GetDownloadSizeAsync(label).Task;
        Debug.Log($"[AssetManager] Required download size: {size} bytes");

        if (size <= 0)
        {
            Debug.Log("[AssetManager] All resources already downloaded.");
            return true;
        }

        var handle = Addressables.DownloadDependenciesAsync(label);
        await handle.Task;

        Debug.Log("[AssetManager] Resource download complete.");
        return true;
    }

    /// 인게임 진입 전체 흐름 (다운로드 → 씬 로드 → NPC 스폰)
    public async UniTask EnterIngameAsync()
    {
        // 1. 리소스 체크 & 다운로드
        bool ok = await CheckAndDownloadStageResourcesAsync(Stage);
        if (!ok) return;

        // 2. 씬 로드
        await SceneManager.LoadSceneAsync("DialogScene").ToUniTask();

        // 3. 씬 로드 끝난 뒤 NPC 스폰
        SpawnNpcForStage(Stage);
    }

    public void SpawnNpcForStage(int stage)
    {
        var table = AssetManager.Singleton.GetNpcDataSO();
        
        foreach (var npcData in table.Items.Values)
        {
            if (npcData.Stage != stage)
                continue;

            var npcObj = AssetManager.Singleton.InstantiateNpcModel(npcData.Name);
            npcObj.transform.position = npcData.spawnPoint;
            npcObj.GetComponent<NpcInteraction>().npcSO = npcData;
        }
    }

    /// <summary>
    /// 특정 npc 스폰
    /// </summary>
    /// <param name="stage"></param>
    public void SpawnNpc(int id)
    {
        var table = AssetManager.Singleton.GetNpcDataSO();
    
        if (!table.Items.TryGetValue(id, out var npcSO))
        {
            Debug.LogError($"NPC ID {id} not found");
            return;
        }
    
        if (Stage != npcSO.Stage)
            return;
    
        var npcObj = AssetManager.Singleton.InstantiateNpcModel(npcSO.Prefab);
    
        npcObj.transform.position = npcSO.spawnPoint;
    
        npcObj.GetComponent<NpcInteraction>().npcSO = npcSO;
    }
}