using UnityEngine;

public class MiniMap2D : MonoBehaviour
{
    public RectTransform viewportRect;
    public RectTransform bgRect;
    public RectTransform iconRootRect;

    public Transform player;
    public RectTransform playerIconRect;

    public Camera minimapCamera; // 구울 때 사용한 카메라

    float worldMinX, worldMaxX;
    float worldMinZ, worldMaxZ;

    Vector2 bgSize;
    Vector2 viewportSize;

    void Awake()
    {
        bgSize = bgRect.sizeDelta;
        viewportSize = viewportRect.sizeDelta;

        playerIconRect.anchoredPosition = Vector2.zero;

        ComputeWorldBounds();
    }

    void ComputeWorldBounds()
    {
        float size = minimapCamera.orthographicSize;
        float aspect = minimapCamera.aspect;

        Vector3 c = minimapCamera.transform.position;

        // 카메라가 찍은 실제 월드 범위
        worldMinX = c.x - size * aspect;
        worldMaxX = c.x + size * aspect;

        worldMinZ = c.z - size;
        worldMaxZ = c.z + size;

        Debug.Log($"World Bounds: X({worldMinX},{worldMaxX}), Z({worldMinZ},{worldMaxZ})");
    }

    void LateUpdate()
    {
        Vector2 p = WorldToMiniMapPos(player.position);

        // 플레이어 중앙 고정 → 배경 이동
        bgRect.anchoredPosition = -p;

        if (iconRootRect != null)
            iconRootRect.anchoredPosition = -p;
    }

    Vector2 WorldToMiniMapPos(Vector3 worldPos)
    {
        float xNormalized = Mathf.InverseLerp(worldMinX, worldMaxX, worldPos.x);
        float zNormalized = Mathf.InverseLerp(worldMinZ, worldMaxZ, worldPos.z);

        float x = (xNormalized - 0.5f) * bgSize.x;
        float y = (zNormalized - 0.5f) * bgSize.y;

        return new Vector2(x, y);
    }
}