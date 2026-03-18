using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlacementSaveData
{
    public PlacedObjectData[] objects;
}

public class PlacementManager : SingletonBase<PlacementManager>
{
    [Header("Runtime Data")]
    [SerializeField] private List<PlacedObjectData> _placedObjects = new List<PlacedObjectData>();

    private readonly Dictionary<int, PlaceableInteraction> _instances = new Dictionary<int, PlaceableInteraction>();
    private readonly List<PlaceableInteraction> _buildingInstances = new List<PlaceableInteraction>();

    public IReadOnlyList<PlacedObjectData> PlacedObjects => _placedObjects;
    public IReadOnlyList<PlaceableInteraction> BuildingInstances => _buildingInstances;

    private PlacementSystem _placementSystem;

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "placement.json");

    private void Awake()
    {
        _placementSystem = GetComponent<PlacementSystem>();
    }

    // =======================
    // 런타임 인스턴스 관리
    // =======================

    public void RegisterInstance(PlaceableInteraction placeable)
    {
        if (placeable == null) return;
        if (placeable.BuilderId < 0) return;

        _instances[placeable.BuilderId] = placeable;

        if (placeable.Role == PlaceableRole.Building && !_buildingInstances.Contains(placeable))
        {
            _buildingInstances.Add(placeable);
        }
    }

    public void UnregisterInstance(PlaceableInteraction placeable)
    {
        if (placeable == null) return;

        if (placeable.BuilderId >= 0 && _instances.TryGetValue(placeable.BuilderId, out var current))
        {
            if (current == placeable)
            {
                _instances.Remove(placeable.BuilderId);
            }
        }

        if (placeable.Role == PlaceableRole.Building)
        {
            _buildingInstances.Remove(placeable);
        }
    }

    public PlaceableInteraction GetByBuilderId(int builderId)
    {
        _instances.TryGetValue(builderId, out var placeable);
        return placeable;
    }

    public IReadOnlyList<PlaceableInteraction> GetBuildings()
    {
        return _buildingInstances;
    }

    // =======================
    // 저장 데이터 관리
    // =======================

    public void RegisterPlacedObject(PlacedObjectData data)
    {
        if (data == null) return;

        _placedObjects.Add(data);
        Debug.Log("배치완료:: 정보 리스트에 저장 됨");
    }

    public void ClearAll()
    {
        Debug.Log("Placement cleared.");

        _placedObjects.Clear();
        _instances.Clear();
        _buildingInstances.Clear();
    }

    public void Save()
    {
        Debug.Log("Placement saved.");

        var wrapper = new PlacementSaveData
        {
            objects = _placedObjects.ToArray()
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"Placement saved to: {SavePath}");
    }
    
    /// <summary>
    /// BuilderId를 사용하여 특정 오브젝트를 삭제합니다.
    /// </summary>
    public void RemoveObject(int builderId)
    {
        if (!_instances.TryGetValue(builderId, out var instance)) return;
        
        _placedObjects.RemoveAll(data => data.id == builderId);

        UnregisterInstance(instance);

        if (instance != null) 
            Destroy(instance.gameObject);
    }

    /// <summary>
    /// PlaceableInteraction 컴포넌트를 직접 전달하여 삭제
    /// </summary>
    public void RemoveObject(PlaceableInteraction placeable)
    {
        if (placeable == null) return;
        RemoveObject(placeable.BuilderId);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No placement save file found.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var wrapper = JsonUtility.FromJson<PlacementSaveData>(json);

        _placedObjects = wrapper != null && wrapper.objects != null
            ? new List<PlacedObjectData>(wrapper.objects)
            : new List<PlacedObjectData>();

        // 런타임 인스턴스 목록은 로드 전에 비워두는 게 안전
        _instances.Clear();
        _buildingInstances.Clear();

        if (_placementSystem == null)
        {
            _placementSystem = FindObjectOfType<PlacementSystem>();
        }

        if (_placementSystem != null)
        {
            _placementSystem.RebuildFromSave(_placedObjects);
        }

        OnGameStart();
    }

    public async void OnGameStart()
    {
        // await GameManager.Singleton.EnterIngameAsync();
    }

    public void OnPlacementEdit()
    {
        if (_placementSystem == null)
        {
            _placementSystem = GetComponent<PlacementSystem>();
        }

        if (_placementSystem != null)
        {
            _placementSystem.enabled = !_placementSystem.enabled;
        }
    }
}