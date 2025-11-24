using UnityEngine;

public class QuestMarkerUI : MonoBehaviour
{
    [Header("참조")]
    public Transform target;                 // 따라갈 대상 (NPC 머리 혹은 bone)
    public Vector3 worldOffset = new Vector3(0, 2f, 0); // 머리 위로 띄울 오프셋
    public RectTransform markerRect;         // 캔버스 안의 아이콘 RectTransform

    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
        target = gameObject.transform;
        markerRect = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (target == null || markerRect == null || _mainCam == null)
            return;

        // 1. 월드 좌표 계산 (머리 위치 + 오프셋)
        Vector3 worldPos = target.position + worldOffset;

        // 2. 월드 → 스크린 좌표 변환
        Vector3 screenPos = _mainCam.WorldToScreenPoint(worldPos);

        // 카메라 뒤에 있으면 안 보이도록 처리
        // if (screenPos.z < 0f)
        // {
        //     gameObject.SetActive(false);
        //     return;
        // }

        // 3. 스크린 좌표를 그대로 RectTransform의 position에 넣으면
        //    Screen Space - Overlay 캔버스 기준으로 잘 따라감
        markerRect.position = screenPos;
    }

    /// <summary>
    /// 퀘스트가 있을 때 UI 켜고/끄는 함수
    /// </summary>
    public void SetQuestActive(bool isActive)
    { 
        gameObject.SetActive(isActive);
        //if (markerRect != null)
    }
}