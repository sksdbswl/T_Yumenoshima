using UnityEngine;


[System.Serializable]
public class PlacedObjectData
{
    public int id;              
    public PlaceableRole role;
    public float gridX;
    public float gridZ;
    public float rotationY;
}

public class PlaceableObject : MonoBehaviour
{
    //[Header("Optional: 벽 모드별 Mesh(필요 없으면 비워도 됨)")] [SerializeField]
    //private GameObject[] wallVariants; // 벽일 때 방향별 메쉬 등

    //[Header("스냅 포인트(없으면 비워도 됨)")] public Collider[] SnapPoints; // Transform 또는 Collider로 스냅표시
    // SnapPoint 레이어가 할당된 child collider 권장

    // public float uniqueId;
    // public float gridX;
    // public float gridZ;
    // public float rotationY;      // 간단하게 Y만
    
    public PlaceableRole Role { get; private set; }
    public PlaceableItem SourceItem { get; private set; }
    
    public void Initialize(PlaceableRole role, PlaceableItem item, Vector3 position, bool force = false)
    {
        Role = role;
        SourceItem = item;

        PlacedObjectData data = new PlacedObjectData();
        
        data.id = PlayerProgress.GenerateBuilderId(); 
        data.role = role;
        data.gridX = position.x;
        data.gridZ = position.z;
        data.rotationY = transform.eulerAngles.y;
        
        int layer = BuilderLayers.LayerFromRole(role);
        BuilderLayers.SetLayerRecursive(transform, layer);

        if (force)
            PlacementSaveManager.Singleton.RegisterPlacedObject(data);
    }
    
    // public void Initialize(PlaceableRole role, PlaceableItem item, Vector3 position)
    // {
    //     var getID = PlayerProgress.GenerateBuilderId();
    //     
    //     
    //     Role = role;
    //     SourceItem = item;
    //     gridX = position.x;
    //     gridZ = position.z;
    //     
    //     //TODO:: 배치 오브젝트 회전값 적용 필요
    //     rotationY = 0f; 
    //     
    //     //오브젝트의 레이어를 역할에 맞게 설정
    //     int layer = BuilderLayers.LayerFromRole(role);
    //     BuilderLayers.SetLayerRecursive(transform, layer);
    //     PlacementSaveManager.Singleton.RegisterPlacedObject(this);
    //     
    //     // if (role == PlaceableRole.Building && wallVariants != null && wallVariants.Length > 0)
    //     // {
    //     //     wallIdx = Mathf.Clamp(wallIdx, 0, wallVariants.Length - 1);
    //     //     for (int i = 0; i < wallVariants.Length; i++)
    //     //         wallVariants[i]?.SetActive(i == wallIdx);
    //     // }
    // }
}
