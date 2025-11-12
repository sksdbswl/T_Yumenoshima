using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RadialRingScroller2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    public RectTransform center;   // 기준(원점)
    public GameObject item;        // 아이템 프리팹 (RectTransform 포함)

    [Header("Layout")]
    public int itemCount = 8;
    public float radius = 300f;
    [Tooltip("아이템이 항상 중심을 보게 회전할지, 화면(위)을 보게 고정할지")]
    public bool itemLookAtCenter = true;
    public bool clockwise = true;

    [Header("Motion")]
    public float snapDuration = 0.35f;
    public Ease snapEase = Ease.OutCubic;
    [Tooltip("드래그 1픽셀당 회전(도)")]
    public float dragToAngle = 0.25f;
    public bool loop = true; // 무한 순환

    RectTransform ring;            // 회전용 컨테이너
    readonly List<RectTransform> items = new();
    float anglePerItem;            // 슬롯 간격(도)
    float ringAngle;               // 현재 ring의 Z회전(도)
    Tween spinTween;

    Vector2 lastPos;
    bool built;

    void Start()
    {
        Build();
        Layout();
        SnapToNearest(true);
    }

    // 필요 시 인스펙터에서 호출
    [ContextMenu("Rebuild")]
    public void Build()
    {
        // 기존 ring 제거
        if (ring != null) DestroyImmediate(ring.gameObject);
        items.Clear();

        // ring 생성
        var go = new GameObject("Ring", typeof(RectTransform));
        ring = go.GetComponent<RectTransform>();
        ring.SetParent(center, false);
        ring.anchorMin = ring.anchorMax = new Vector2(0.5f, 0.5f);
        ring.pivot = new Vector2(0.5f, 0.5f);
        ring.anchoredPosition = Vector2.zero;
        ring.localScale = Vector3.one;

        anglePerItem = 360f / Mathf.Max(1, itemCount);

        for (int i = 0; i < itemCount; i++)
        {
            var inst = Instantiate(item, ring).GetComponent<RectTransform>();
            inst.name = $"Item_{i:D2}";
            items.Add(inst);
        }

        built = true;
    }

    void Layout()
    {
        if (!built) return;

        for (int i = 0; i < items.Count; i++)
        {
            float localAngle = i * anglePerItem * (clockwise ? 1f : -1f); // ring 기준 각
            Vector2 pos = AngleToPos(localAngle);
            var rt = items[i];
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;

            // 아이템 회전
            if (itemLookAtCenter)
            {
                // 중심을 보게: 법선 방향(=pos의 -각)을 사용
                float face = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg + 90f; // 위쪽이 앞이라고 가정
                rt.localRotation = Quaternion.Euler(0, 0, face);
            }
            else
            {
                rt.localRotation = Quaternion.identity; // 화면 고정
            }
        }

        ring.localRotation = Quaternion.Euler(0, 0, ringAngle);
    }

    Vector2 AngleToPos(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
    }

    // ---- Drag ----
    public void OnBeginDrag(PointerEventData e)
    {
        spinTween?.Kill();
        lastPos = e.position;
    }

    public void OnDrag(PointerEventData e)
    {
        var delta = e.position - lastPos;
        lastPos = e.position;

        // 수평 드래그를 회전에 매핑 (원한다면 delta.y도 함께 반영)
        ringAngle += -delta.x * dragToAngle;
        if (!loop) ringAngle = Mathf.Clamp(ringAngle, -180f, 180f);
        Layout();
    }

    public void OnEndDrag(PointerEventData e)
    {
        SnapToNearest(false);
    }

    // 가장 가까운 슬롯 각도 계산 및 스냅
    void SnapToNearest(bool instant)
    {
        // ring이 도는 구조라서 "정면 슬롯이 0도"가 기준.
        float snapped = Mathf.Round(ringAngle / anglePerItem) * anglePerItem;

        if (!loop) snapped = Mathf.Clamp(snapped, -180f, 180f);

        spinTween?.Kill();
        if (instant)
        {
            ringAngle = snapped;
            Layout();
        }
        else
        {
            spinTween = DOTween.To(() => ringAngle, v => { ringAngle = v; Layout(); },
                                   snapped, snapDuration).SetEase(snapEase);
        }
    }

    // 버튼/키로 한 칸 이동
    public void Move(int dir) // dir=+1(다음), -1(이전)
    {
        float step = (clockwise ? -1 : +1) * dir * anglePerItem;
        spinTween?.Kill();
        spinTween = DOTween.To(() => ringAngle, v => { ringAngle = v; Layout(); },
                               ringAngle + step, snapDuration).SetEase(snapEase);
    }

    // 현재 정면 아이템 인덱스 (0을 정면 기준)
    public int CurrentIndex()
    {
        float k = Mathf.Round(ringAngle / anglePerItem);
        // ringAngle이 +면 시계/반시계에 따라 보정
        int idx = (int)((items.Count - (int)k) % items.Count);
        if (idx < 0) idx += items.Count;
        return idx;
    }
}
