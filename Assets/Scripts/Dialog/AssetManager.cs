using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

public class AssetManager : SingletonBase<AssetManager>
{
    public NpcSO GetNpcSO()
    {
        LoadDataScript<NpcSO>(AssetConstant.AddressNpcData, out var result);
        return result;
    }
    
    public NpcDataSO GetNpcDataSO()
    {
        LoadDataScript<NpcDataSO>(AssetConstant.AddressNpcData, out var result);
        return result;
    }


    public GameObject InstantiateNpcModel(string prefabName)
    {
        // Addressables Address와 동일해야 함
        string address = $"{AssetConstant.AddressPrefixNpcModel}{prefabName}";
        return LoadAssetClone<GameObject>(address); 
    }

    public GameObject InstantiateNpcModel(GameObject prefab)
    {
        // Addressables에서 직접 instantiate 가능
     
        return Instantiate(prefab);
    }
    
    // ──────────────────────────────
    // SO 전용 로더
    // ──────────────────────────────
    private void LoadDataScript<T>(string address, out T result, bool fromcache = false)
        where T : ScriptableObject
    {
        if (fromcache)
        {
            result = LoadAssetFromCache<T>(address);
            return;
        }

        // 데이터 테이블 계속 들고 갈지 결정
        result = LoadAssetClone<T>(address, false);
    }

    // ──────────────────────────────
    // 공통 캐시 로드 (SO, GameObject 모두 가능)
    // ──────────────────────────────
    private T LoadAssetFromCache<T>(string address) where T : Object
    {
        return Addressables.LoadAssetAsync<T>(address).WaitForCompletion();
    }

    // ──────────────────────────────
    // 공통 클론 로드 (SO, GameObject 모두 가능)
    // ──────────────────────────────
    public T LoadAssetClone<T>(string address, bool instantiate = true)
        where T : Object
    {
        var original = Addressables.LoadAssetAsync<T>(address).WaitForCompletion();

        if (!instantiate)
            return original;      // 읽기 전용으로 쓸 때

        // SO든 Prefab이든 전부 Object.Instantiate로 복제
        return Object.Instantiate(original);
    }
    
    
    /// <summary>
    /// 리소스 다운 로직
    /// </summary>
    /// <param name="stage"></param>
    public async UniTask DownloadStageResourcesAsync(int stage)
    {
        string label = $"Npc";
        Debug.Log($"[AssetManager] Download Stage Resources: {label}");

        var handle = Addressables.DownloadDependenciesAsync(label);
        await handle.Task; // 에러처리 필요하면 try/catch 추가

        Debug.Log($"[AssetManager] Download Stage Resources Done: {label}");
    }

    public void UnloadStageResources(int stage)
    {
        string label = $"Stage_{stage}";
        Addressables.ClearDependencyCacheAsync(label); 
    }
}


public static class AssetConstant
{
    // NpcSO 주소 (Addressables 창에 Address를 이렇게 맞춰야 함)
    public const string AddressNpcData = "Assets/SoData/NPC/NpcSo";

    // NPC 프리팹 Prefix
    // 예: Addressables Address가 "Assets/Prefab/NPC/Shin" 이면
    // InstantiateNpcModel("Shin") -> "Assets/Prefab/NPC/Shin"
    public const string AddressPrefixNpcModel = "Assets/Prefab/NPC/";
}
