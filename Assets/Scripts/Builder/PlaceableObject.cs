using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    //[Header("Optional: 벽 모드별 Mesh(필요 없으면 비워도 됨)")] [SerializeField]
    //private GameObject[] wallVariants; // 벽일 때 방향별 메쉬 등

    //[Header("스냅 포인트(없으면 비워도 됨)")] public Collider[] SnapPoints; // Transform 또는 Collider로 스냅표시
    // SnapPoint 레이어가 할당된 child collider 권장

    public PlaceableRole Role { get; private set; }

    public void Initialize(PlaceableRole role, int wallIdx = 0)
    {
        Role = role;
        
        //오브젝트의 레이어를 역할에 맞게 설정
        int layer = BuilderLayers.LayerFromRole(role);
        BuilderLayers.SetLayerRecursive(transform, layer);

        // if (role == PlaceableRole.Building && wallVariants != null && wallVariants.Length > 0)
        // {
        //     wallIdx = Mathf.Clamp(wallIdx, 0, wallVariants.Length - 1);
        //     for (int i = 0; i < wallVariants.Length; i++)
        //         wallVariants[i]?.SetActive(i == wallIdx);
        // }
    }
}
