using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

public class AssetManager : SingletonBase<AssetManager>
{
    private NpcDataSO _cacheNpcData;

    // ───────────────── NPC 관련 ─────────────────
    public NpcSO GetNpcSO()
    {
        LoadDataScript<NpcSO>(AssetConstant.AddressNpcData, out var result);
        return result;
    }

    public NpcDataSO GetNpcDataSO()
    {
        if (_cacheNpcData == null)
        {
            _cacheNpcData = Addressables
                .LoadAssetAsync<NpcDataSO>(AssetConstant.AddressNpcData)
                .WaitForCompletion();

            _cacheNpcData.BuildDictionary();
        }

        return _cacheNpcData;
    }

    public GameObject InstantiateNpcModel(string prefabName)
    {
        string address = $"{AssetConstant.AddressPrefixNpcModel}{prefabName}";
        return LoadAssetClone<GameObject>(address);
    }

    public GameObject InstantiateNpcModel(GameObject prefab)
    {
        return Object.Instantiate(prefab);
    }

    // ───────────────── SO 전용 로더 ─────────────────

    private void LoadDataScript<T>(string address, out T result, bool fromcache = false)
        where T : ScriptableObject
    {
        if (fromcache)
        {
            result = LoadAssetFromCache<T>(address);
            return;
        }

        result = LoadAssetClone<T>(address, false);
    }

    // ───────────────── 공통 캐시 로드 ─────────────────

    private T LoadAssetFromCache<T>(string address) where T : Object
    {
        return Addressables.LoadAssetAsync<T>(address).WaitForCompletion();
    }

    // ───────────────── 공통 클론 로드 ─────────────────

    public T LoadAssetClone<T>(string address, bool instantiate = true)
        where T : Object
    {
        var original = Addressables.LoadAssetAsync<T>(address).WaitForCompletion();

        if (!instantiate)
            return original;

        return Object.Instantiate(original);
    }

    // ───────────────── 리소스 다운로드 ─────────────────

    /// <summary>
    /// stage에 필요한 리소스 다운로드 (임시로 "Npc" 라벨만 사용)
    /// </summary>
    public async UniTask DownloadStageResourcesAsync(int stage)
    {
        const string label = "Npc";
        Debug.Log($"[AssetManager] Download Stage Resources: {label}");

        var handle = Addressables.DownloadDependenciesAsync(label);
        await handle.Task;

        Debug.Log($"[AssetManager] Download Stage Resources Done: {label}");
    }

    public void UnloadStageResources(int stage)
    {
        string label = $"Stage_{stage}";
        Addressables.ClearDependencyCacheAsync(label);
    }

    // ───────────────── UI 전용 Addressables ─────────────────
    // Addressables Name 을 UIList enum 이름과 동일하게 맞춰서 사용
    //   예) UIList.BuilderUI  ->  Addressable Name: "BuilderUI"

    /// <summary>
    /// UI 프리팹 Addressables Instantiate (비동기)
    /// Addressables Name 은 key.ToString() 사용.
    /// </summary>
    public async UniTask<GameObject> InstantiateUIPrefabAsync(UIList key, Transform parent = null)
    {
        string address = $"{AssetConstant.AddressUIModel}{key}";

        var handle = Addressables.InstantiateAsync(address, parent);
        var go = await handle.Task;

        if (go == null)
        {
            Debug.LogError($"[AssetManager] UI {key} 로드/생성 실패. Addressables 키를 확인하세요. (Key: {address})");
        }

        return go;
    }
}

public static class AssetConstant
{
    // NpcSO 주소
    public const string AddressNpcData = "Assets/SoData/NPC/NpcSo";

    // NPC 프리팹 Prefix
    public const string AddressPrefixNpcModel = "Assets/Prefab/NPC/";

    // UI 관련 prefix가 필요하면 여기 추가해서 써도 됨
    public const string AddressUIModel = "Assets/UI/";
}

[Serializable]
public class UIPrefabEntry
{
    public UIList key;                       // 어떤 UI인지 (enum)
    public AssetReferenceGameObject prefab;  // Addressables 참조
}
