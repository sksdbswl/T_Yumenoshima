using UnityEngine;
using UnityEngine.UI;

namespace REIW
{
    public class MiniMapUI : MonoBehaviour
    {
        public static MiniMapUI Instance { get; private set; }

        [Header("미니맵 뷰 영역 (UI)")]
        [SerializeField] private RectTransform miniMapRect;   // 미니맵 네모 영역
        [SerializeField] private RectTransform iconParent;    // 아이콘들이 붙을 부모

        [Header("지형(월드)의 실제 크기 (X-Z 기준)")]
        [SerializeField] private float worldWidth = 100f;     // 필드 X 범위
        [SerializeField] private float worldHeight = 100f;    // 필드 Z 범위

        [Header("플레이어")]
        [SerializeField] private Transform player;            // 중앙에 둘 대상

        [Header("배경 이미지 (지형도)")]
        [SerializeField] private Image miniMapBgImage;        // 지형도 UI 이미지

        private Vector2 mapSize;         // 미니맵 RectTransform의 픽셀 크기
        private Vector2 miniMapCenter;   // 플레이어 위치의 미니맵 좌표

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (miniMapRect != null)
                mapSize = miniMapRect.sizeDelta;
        }

        private void LateUpdate()
        {
            if (player == null || iconParent == null || miniMapRect == null)
                return;

            // 플레이어 월드 좌표 → 미니맵 좌표
            miniMapCenter = ConvertWorldPosToMiniMapPos(player.position);

            // 플레이어를 가운데에 두기 위해, 아이콘 전체를 반대로 이동
            iconParent.anchoredPosition = -miniMapCenter;
        }

        /// <summary>
        /// 월드 XZ 좌표를 미니맵 UI 좌표로 변환
        /// </summary>
        public Vector2 ConvertWorldPosToMiniMapPos(Vector3 worldPos)
        {
            // 월드에서 0~worldWidth, 0~worldHeight 구간이라고 가정 (원하면 오프셋 추가해도 됨)
            float xRatio = worldPos.x / worldWidth;   // 0 ~ 1
            float yRatio = worldPos.z / worldHeight;  // 0 ~ 1

            Vector2 miniMapPos;
            // 비율을 -0.5 ~ 0.5 로 옮기고, 미니맵 픽셀 크기 곱해줌
            miniMapPos.x = mapSize.x * (xRatio - 0.5f);
            miniMapPos.y = mapSize.y * (yRatio - 0.5f);
            return miniMapPos;
        }

        /// <summary>
        /// 미니맵 아이콘 하나 생성
        /// </summary>
        public MiniMap_MapIconUI CreateIcon(Sprite sprite, Transform target, MiniMap_MapIconUI iconPrefab)
        {
            if (iconPrefab == null || iconParent == null)
            {
                Debug.LogWarning("[MiniMapUI] 아이콘 프리팹 또는 IconParent가 비어있음");
                return null;
            }

            var icon = Instantiate(iconPrefab, iconParent);
            icon.Init(sprite, target, this);
            return icon;
        }

        /// <summary>
        /// 지형도 스프라이트 세팅 (원하면 코드로 변경 가능)
        /// </summary>
        public void SetMiniMapBg(Sprite sprite)
        {
            if (miniMapBgImage == null) return;
            miniMapBgImage.sprite = sprite;
        }
    }
}
