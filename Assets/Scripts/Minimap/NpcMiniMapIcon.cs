using UnityEngine;

namespace REIW
{
    [RequireComponent(typeof(NpcInteraction))] 
    public class NpcMiniMapIcon : MonoBehaviour
    {
        [Header("미니맵에 표시할 스프라이트")]
        [SerializeField] private Sprite minimapSprite;

        [Header("아이콘 프리팹 (MiniMap_MapIconUI가 붙은 UI 프리팹)")]
        [SerializeField] private MiniMap_MapIconUI iconPrefab;

        private MiniMap_MapIconUI iconInstance;

        private void OnEnable()
        {
            if (MiniMapUI.Instance == null)
            {
                Debug.LogWarning("[NpcMiniMapIcon] MiniMapUI 인스턴스를 찾을 수 없음");
                return;
            }

            iconInstance = MiniMapUI.Instance.CreateIcon(minimapSprite, transform, iconPrefab);
        }

        private void OnDisable()
        {
            if (iconInstance != null)
            {
                Destroy(iconInstance.gameObject);
                iconInstance = null;
            }
        }
    }
}