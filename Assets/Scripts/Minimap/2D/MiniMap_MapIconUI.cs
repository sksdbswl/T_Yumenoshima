using UnityEngine;
using UnityEngine.UI;

namespace REIW
{
    public class MiniMap_MapIconUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;

        private Transform target;   // 따라다닐 대상 (NPC, 플레이어 등)
        private MiniMapUI miniMap;
        private RectTransform rect;

        public void Init(Sprite sprite, Transform target, MiniMapUI miniMap)
        {
            this.target = target;
            this.miniMap = miniMap;

            if (iconImage != null)
                iconImage.sprite = sprite;

            rect = (RectTransform)transform;
        }

        private void LateUpdate()
        {
            if (miniMap == null || target == null || rect == null)
                return;

            // 타겟을 미니맵 좌표로 변환해서 아이콘 위치 갱신
            Vector2 pos = miniMap.ConvertWorldPosToMiniMapPos(target.position);
            rect.anchoredPosition = pos;
        }
    }
}