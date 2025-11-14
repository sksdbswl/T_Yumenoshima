using UnityEngine;
using UnityEngine.UI;

public class MiniMap2D : MonoBehaviour
{
    [Header("뷰포트 / 배경 / 아이콘 부모")]
    [SerializeField] private RectTransform viewportRect;   // MiniMapViewport
    [SerializeField] private RectTransform bgRect;         // MiniMapBg
    [SerializeField] private RectTransform iconRootRect;   // IconRoot

    [Header("플레이어")]
    [SerializeField] private Transform player;             // 플레이어 Transform
    [SerializeField] private RectTransform playerIconRect; // PlayerIcon (중앙 고정)

    [Header("월드 범위 (미니맵이 커버하는 실제 좌표)")]
    // 예: X:-50~50, Z:-50~50 이면
    public float worldMinX = -50f;
    public float worldMaxX =  50f;
    public float worldMinZ = -50f;
    public float worldMaxZ =  50f;

    private Vector2 bgSize;

    private void Awake()
    {
        if (bgRect != null)
            bgSize = bgRect.sizeDelta;

        // 플레이어 아이콘은 뷰포트 중앙에 고정
        if (playerIconRect != null)
            playerIconRect.anchoredPosition = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (player == null || bgRect == null || iconRootRect == null)
            return;

        // 플레이어 월드 좌표 → 미니맵 좌표
        Vector2 playerMapPos = WorldToMiniMapPos(player.position);

        // 플레이어를 가운데 두기 위해 배경/아이콘 전체를 반대로 이동
        bgRect.anchoredPosition = -playerMapPos;
        iconRootRect.anchoredPosition = -playerMapPos;
        // PlayerIcon 은 anchoredPosition = (0,0) 그대로
    }

    /// <summary>
    /// 월드(XZ) 좌표를 미니맵 내 로컬 좌표로 변환
    /// </summary>
    public Vector2 WorldToMiniMapPos(Vector3 worldPos)
    {
        // 0~1 비율로 변환
        float xNorm = Mathf.InverseLerp(worldMinX, worldMaxX, worldPos.x); // 0~1
        float zNorm = Mathf.InverseLerp(worldMinZ, worldMaxZ, worldPos.z); // 0~1

        // -0.5 ~ 0.5 로 이동 후, 배경 사이즈 곱
        float x = (xNorm - 0.5f) * bgSize.x;
        float y = (zNorm - 0.5f) * bgSize.y;

        return new Vector2(x, y);
    }
}