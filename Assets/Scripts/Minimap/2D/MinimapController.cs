using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("UI")]
    public RectTransform maskRect; // 미니맵이 보여질 영역
    public RectTransform mapRect; // 실제 사용될 맵 이미지
    public RectTransform playerIconRect; // 플레이어 아이콘

    [Header("World")]
    public Transform player; // 실제 플레이어 위치

    [Header("구울 때 사용한 MinimapCamera")]
    //맵 이미지를 구울 때 사용한 orthographic 카메라
    //해당 카메라의 orthoSize / aspect / position 을 이용해
    //“미니맵 이미지가 커버한 월드 범위”를 역으로 계산하려고 함
    public Camera minimapCamera; 

    // 미니맵이 커버하는 실제 월드 좌표 범위: 내부에서 자동 계산 ( 정사영 카메라로 확보한 월드 범위 )
    // 카메라 x=0, size=20 → worldMinX = -20, worldMaxX = +20
    // 카메라 z=0, size=20 → worldMinZ = -20, worldMaxZ = +20
    // → 즉, 미니맵 이미지가 찍은 월드 영역의 최소/최대 좌표
    // 여기서 사용되는 size = 30 의 값은 MinimapCamera의 Orthographic Size 값이다.
    float worldMinX, worldMaxX;
    float worldMinZ, worldMaxZ;
 
    Vector2 mapSize;
    Vector2 maskSize;

    void Awake()
    {
        if (maskRect == null)
            maskRect = (RectTransform)transform;

        mapSize  = mapRect.rect.size; 
        maskSize = maskRect.rect.size;

        if (playerIconRect != null)
            playerIconRect.anchoredPosition = Vector2.zero;

        ComputeWorldBoundsFromCamera();
    }

    void ComputeWorldBoundsFromCamera()
    {
        if (minimapCamera == null || !minimapCamera.orthographic)
        {
            Debug.LogError("[MiniMapController] minimapCamera 미설정 또는 비-정사영");
            return;
        }

        // MinimapCamera의 Orthographic Size 값 = 20
        float size   = minimapCamera.orthographicSize;
        // aspect = 화면가로 / 화면세로 
        // 예를 들어 aspect = 1(정사각형) 이면,
        // 가로 방향도 -20 ~ +20
        float aspect = minimapCamera.aspect;
        Vector3 minimapCam = minimapCamera.transform.position; // ( 0, 5, 0);

        // 카메라가 내려다보고 있다고 가정 (90도)
        // 여기서 x에만 aspect를 곱해주는 이유는
        // Orthographic 카메라는 
        // Z 방향(세로 방향)은 orthographicSize 로 결정되고
        // X 방향(가로 방향)은 orthographicSize × aspect 로 결정되기 때문이다.
        // orthographicSize = 화면 세로 방향의 절반 크기
        // 가로(Width)는 aspect = width / height
        // [ 결론 ]
        // 세로 방향(U) 범위 = [-size, +size]
        // 가로 방향(V) 범위 = [-size * aspect, +size * aspect]
        worldMinX = minimapCam.x - size * aspect; // (0 - 20 * 1) = -20
        worldMaxX = minimapCam.x + size * aspect; // (0 + 20 * 1) = 20
        worldMinZ = minimapCam.z - size; // (0 - 20) = -20
        worldMaxZ = minimapCam.z + size; // (0 + 20) = 20

        Debug.Log($"MiniMap bounds X[{worldMinX}, {worldMaxX}] Z[{worldMinZ}, {worldMaxZ}]");
    }

    void LateUpdate()
    {
        if (player == null) return;

        //월드의 (player.position.x, player.position.z) 를 미니맵 UI 좌표로 변환
        Vector2 p = WorldToMapPos(player.position);

        //플레이어를 미니맵 중앙에 두기 위해 미니맵 배경을 반대로 이동시킴
        Vector2 offset = -p;

        // (원하면 여기서 clamp)
        mapRect.anchoredPosition = offset;
    }

    Vector2 WorldToMapPos(Vector3 worldPlayerPos)
    {
        // InverseLerp
        // 월드 X,Z를 0~1 비율로 정규화
        // 플레이어가 월드 맵 전체 중 몇 % 지점에 있는가? = 0 ~ 1
        float xNorm = Mathf.InverseLerp(worldMinX, worldMaxX, worldPlayerPos.x); // Mathf.InverseLerp(-20, +20, 0) :: 결과 = 0.5
        float zNorm = Mathf.InverseLerp(worldMinZ, worldMaxZ, worldPlayerPos.z); // Mathf.InverseLerp(-20, +20, 0) :: 결과 = 0.5

        Debug.Log(xNorm + " " + zNorm);

        // 1) xNorm - 0.5f : 중앙 맞추기
        // 2) (1) * mapSize.x : 픽셀 값으로 변환
        float x = (xNorm - 0.5f) * mapSize.x;
        float y = (zNorm - 0.5f) * mapSize.y;

        return new Vector2(x, y);
    }
}
