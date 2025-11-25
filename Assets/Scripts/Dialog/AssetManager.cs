using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class AssetManager : SingletonBase<AssetManager>
{
    private NpcDataSO _cacheNpcData;

    public NpcSO GetNpcSO()
    {
        LoadDataScript<NpcSO>(AssetConstant.AddressNpcData, out var result);
        return result;
    }

    public NpcDataSO GetNpcDataSO()
    {
        if (_cacheNpcData == null)
        {
            _cacheNpcData = Addressables.LoadAssetAsync<NpcDataSO>(AssetConstant.AddressNpcData).WaitForCompletion();
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
        return UnityEngine.Object.Instantiate(prefab);
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
            return original;

        return UnityEngine.Object.Instantiate(original);
    }

    /// <summary>
    /// 리소스 다운 로직
    /// </summary>
    public async UniTask DownloadStageResourcesAsync(int stage)
    {
        // 임시 npc만 리소스 다운
        string label = $"Npc";
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

    // ───────────────── UI 전용 Addressables 설정 ─────────────────

    [Header("UI Addressables")]
    [SerializeField]
    private List<UIPrefabEntry> uiPrefabs = new();

    /// <summary>
    /// UIList → Addressables 프리팹 레퍼런스
    /// </summary>
    private readonly Dictionary<UIList, AssetReferenceGameObject> _uiPrefabLookup =
        new Dictionary<UIList, AssetReferenceGameObject>();

    private void BuildUiLookupIfNeeded()
    {
        if (_uiPrefabLookup.Count > 0)
            return;

        foreach (var entry in uiPrefabs)
        {
            if (entry == null || entry.prefab == null)
                continue;

            if (!_uiPrefabLookup.ContainsKey(entry.key))
            {
                _uiPrefabLookup.Add(entry.key, entry.prefab);
            }
        }
    }

    /// <summary>
    /// UI 프리팹 Addressables Instantiate (동기, UIManager에서 사용)
    /// </summary>
    public bool InstantiateUIPrefabSync(UIList key, out GameObject loadedUI)
    {
        BuildUiLookupIfNeeded();
        loadedUI = null;

        if (!_uiPrefabLookup.TryGetValue(key, out var reference) || reference == null)
        {
            Debug.LogError($"[AssetManager] UIList {key} 에 대한 UI Addressable 설정이 없습니다.");
            return false;
        }

        try
        {
            AsyncOperationHandle<GameObject> handle = reference.InstantiateAsync();
            var instance = handle.WaitForCompletion();

            if (instance == null)
            {
                Debug.LogError($"[AssetManager] UI {key} Instantiate 실패");
                return false;
            }

            loadedUI = instance;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AssetManager] UI {key} Instantiate 중 예외 발생");
            Debug.LogException(e);
            return false;
        }
    }

    /// <summary>
    /// 필요하면 쓸 수 있는 비동기 버전
    /// </summary>
    public async UniTask<GameObject> InstantiateUIPrefabAsync(UIList key, Transform parent)
    {
        BuildUiLookupIfNeeded();

        if (!_uiPrefabLookup.TryGetValue(key, out var reference) || reference == null)
        {
            Debug.LogError($"[AssetManager] UIList {key} 에 대한 UI Addressable 설정이 없습니다.");
            return null;
        }

        var instance = await reference.InstantiateAsync(parent).ToUniTask();
        if (!instance)
        {
            Debug.LogError($"[AssetManager] UI {key} Instantiate 실패");
            return null;
        }

        return instance;
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
