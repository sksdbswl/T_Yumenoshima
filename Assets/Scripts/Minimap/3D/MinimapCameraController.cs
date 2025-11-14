using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    [Header("따라갈 타겟 (플레이어)")]
    public Transform target;

    [Header("타겟 위로 얼마나 띄울지")]
    public float height = 50f;

    [Header("타겟 기준 평면 오프셋 (원하면 사용)")]
    public Vector3 offset = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        // 타겟 기준 위치 계산
        Vector3 targetPos = target.position + offset;
        targetPos.y += height;

        Vector3.Lerp(transform.position, targetPos, 0.1f);
        
        // 북쪽(월드 +Y) 고정 내려다보기
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}