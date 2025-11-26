using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON에 저장될때 사용될 데이터 형식
/// </summary>
[System.Serializable]
public class PlacedObjectData
{
    public int id;              
    public PlaceableRole role;
    public float gridX;
    public float gridZ;
    public float rotationY;
}

public class PlaceableInteraction: InteractionTarget, IInteractable
{
    // ==== 정적 레지스트리 (씬에 존재하는 인스턴스들) ====
    private static Dictionary<int, PlaceableInteraction> _instances = new Dictionary<int, PlaceableInteraction>();
    private static List<PlaceableInteraction> _buildingInstances = new List<PlaceableInteraction>();

    public static PlaceableInteraction GetByInstanceId(int instanceId)
    {
        _instances.TryGetValue(instanceId, out var obj);
        return obj;
    }

    public static IReadOnlyList<PlaceableInteraction> GetBuildings()
    {
        return _buildingInstances;
    }

    // ==== 인스턴스 정보 ====
    public PlaceableRole Role { get; private set; }
    public PlaceableItem SourceItem { get; private set; }
    public DoorInteraction Door { get; private set; }
    
    public int BuilderId => SourceItem != null ? SourceItem.BuilderId : -1;

    // ==== 초기화 ====
    public void Initialize(PlaceableRole role, PlaceableItem item, Vector3 position, bool save = false)
    {
        Role = role;
        SourceItem = item;

        if (item.Door)
        {
            Door = GetComponentInChildren<DoorInteraction>();
            Door.Place = this;
        }
        
        int layer = BuilderLayers.LayerFromRole(role);
        BuilderLayers.SetLayerRecursive(transform, layer);

        // 정적 레지스트리에 등록
        RegisterToStaticRegistry();

        // 세이브 데이터에 기록할 때만
        if (save)
        {
            PlacedObjectData data = new PlacedObjectData();
            data.id  = item.BuilderId;
            data.role       = role;
            data.gridX      = position.x;
            data.gridZ      = position.z;
            data.rotationY  = transform.eulerAngles.y;

            PlacementSaveManager.Singleton.RegisterPlacedObject(data);
        }
    }

    private void RegisterToStaticRegistry()
    {
        _instances[BuilderId] = this;

        if (Role == PlaceableRole.Building)
        {
            _buildingInstances.Add(this);
        }
    }

    private void OnDestroy()
    {
        // 파괴될 때 정적 레지스트리에서도 제거
        if (_instances != null)
        {
            _instances.Remove(BuilderId);
        }

        if (_buildingInstances != null && Role == PlaceableRole.Building)
        {
            _buildingInstances.Remove(this);
        }
    }

    // =======================
    // IInteractable 구현부
    // =======================
    
    public void CheckInteract(int stage) { }

    public void BeginInteract(Player player)
    {
        // TODO:: 건물 상호작용 (예: 건물 정보창, 상점 UI 등)
        Debug.Log($"[PlaceableObject] Building Interact: {SourceItem?.DisplayName} (Role: {Role})");
    }

    public void EndInteract(Player player)
    {
        // TODO:: 건물 상호작용 종료 시 처리 (필요하다면)
        Debug.Log($"[PlaceableObject] EndInteract: {SourceItem?.DisplayName}");
    }
}
