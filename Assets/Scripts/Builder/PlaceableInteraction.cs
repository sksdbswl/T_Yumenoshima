using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// JSON에 저장될 때 사용될 데이터 형식
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

public class PlaceableInteraction : InteractionTarget, IInteractable
{
    public PlaceableRole Role { get; private set; }
    public PlaceableItem SourceItem { get; private set; }
    public DoorInteraction Door { get; private set; }
    
    public int BuilderId => SourceItem != null ? SourceItem.BuilderId : -1;
    
    public void Initialize(PlaceableRole role, PlaceableItem item, Vector3 position, bool save = false)
    {
        Role = role;
        SourceItem = item;
        if (item != null && item.Door)
        {
            Door = GetComponentInChildren<DoorInteraction>();
            if (Door != null)
            {
                Door.Place = this;
            }
        }

        int layer = BuilderLayers.LayerFromRole(role);
        BuilderLayers.SetLayerRecursive(transform, layer);

        // 씬 인스턴스 등록
        if (PlacementManager.Singleton != null)
        {
            PlacementManager.Singleton.RegisterInstance(this);
        }

        // 세이브 데이터 등록
        if (save && item != null && PlacementManager.Singleton != null)
        {
            var data = new PlacedObjectData
            {
                id = item.BuilderId,
                role = role,
                gridX = position.x,
                gridZ = position.z,
                rotationY = transform.eulerAngles.y
            };

            PlacementManager.Singleton.RegisterPlacedObject(data);
        }
    }

    private void OnDestroy()
    {
        if (PlacementManager.Singleton != null)
        {
            PlacementManager.Singleton.UnregisterInstance(this);
        }
    }

    // =======================
    // IInteractable 구현부
    // =======================

    public void CheckInteract(int stage) { }

    public async UniTask BeginInteract(Player player)
    {
        Debug.Log($"[PlaceableObject] Building Interact: {SourceItem?.DisplayName} (Role: {Role})");
        await UniTask.CompletedTask;
    }

    public void EndInteract(Player player)
    {
        Debug.Log($"[PlaceableObject] EndInteract: {SourceItem?.DisplayName}");
    }

    // =======================
    // 건물 상태 구현부
    // =======================
    public bool IsOnFire { get; private set; }
    public float FireDuration { get; private set; }
    private float destroyAfterFireSeconds = 30f;
    private Coroutine fireCoroutine;
    
    public void SetFire(bool on)
    {
        IsOnFire = on;
        Debug.Log($"{name} Fire State = {IsOnFire}");
        
        var rends = GetComponentsInChildren<Renderer>();

        // 여기서 파티클, 머티리얼, 이펙트 on/off 처리 : 임시 색상 처리
        foreach (var r in rends)
        {
            if (on)
            {
                FireDuration = 0f;
                fireCoroutine = StartCoroutine(FireCountdownCoroutine());
                r.material.color = Color.red;
            }
            else
            {
                r.material.color = Color.white;
            }
        }
    }
    
    private IEnumerator FireCountdownCoroutine()
    {
        while (IsOnFire)
        {
            FireDuration += Time.deltaTime;

            if (FireDuration >= destroyAfterFireSeconds)
            {
                DestroyBuilding();
                yield break;
            }

            yield return null;
        }
    }

    private void DestroyBuilding()
    {
        Debug.Log($"{name} 건물이 화재로 파괴됨");

        IsOnFire = false;
        FireDuration = 0f;

        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }

        // 저장 데이터에서도 제거
        PlacementManager.Singleton.RemoveObject(this);
        PlacementManager.Singleton.Save();
        
        Destroy(gameObject);
    }
}


