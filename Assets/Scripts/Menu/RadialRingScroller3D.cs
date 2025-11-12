using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RadialRingScroller3D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    public RectTransform center;    // 기준(원점) – World/Screen Space 상관없음
    public Transform TilePivot;    // 아이템 -x축 회전 적용 
    public GameObject item;         // 아이템 프리팹(이미지/버튼 등, CanvasGroup 권장)
    public Camera uiCamera;         // Screen Space - Camera 또는 World Space일 때 할당 권장

    [Header("3D Orbit")]
    public int itemCount = 8;
    public float radiusX = 500f;    // x반지름
    public float radiusZ = 600f;    // z반지름(깊이)
    public float height = 0f;       // 필요 시 고리의 y 높이
    public bool clockwise = true;
    public float itemTiltX = 30f; 

    [Header("Facing / Rendering")]
    public bool itemLookAtCenter = false;   // 센터(또는 카메라)를 바라보게
    public bool lookAtCamera = true;        // 카메라 바라보기(권장)
    public Vector2 scaleRange = new Vector2(0.7f, 1.1f); // 뒤/앞 스케일
    public Vector2 alphaRange = new Vector2(0.25f, 1f);  // 뒤/앞 알파

    [Header("Motion")]
    public float snapDuration = 0.35f;
    public Ease snapEase = Ease.OutCubic;
    public float dragToAngle = 0.25f;   // 드래그 1px -> 각도(deg)
    public bool loop = true;

    RectTransform ring;            // 회전 컨테이너(Y축 회전)
    readonly List<RectTransform> items = new();
    float anglePerItem;
    float ringAngle;               // Y축 회전(deg)
    Tween spinTween;

    Vector2 lastPos;
    bool built;

    void Start()
    {
        Build();
        Layout();
        SnapToNearest(true);
    }

    [ContextMenu("Rebuild")]
    public void Build()
    {
        if (ring != null) DestroyImmediate(ring.gameObject);
        items.Clear();

        // 회전 컨테이너 생성 (TiltPivot의 자식)
        var go = new GameObject("Ring3D", typeof(RectTransform));
        ring = go.GetComponent<RectTransform>();
        ring.SetParent(TilePivot, false); // ← 외부에서 지정한 TiltPivot 사용
        ring.anchorMin = ring.anchorMax = new Vector2(0.5f, 0.5f);
        ring.pivot = new Vector2(0.5f, 0.5f);
        ring.anchoredPosition3D = new Vector3(0, height, 0);
        ring.localScale = Vector3.one;

        anglePerItem = 360f / Mathf.Max(1, itemCount);

        for (int i = 0; i < itemCount; i++)
        {
            var inst = Instantiate(item, ring).GetComponent<RectTransform>();
            inst.name = $"Item_{i:D2}";
            items.Add(inst);

            inst.anchorMin = inst.anchorMax = new Vector2(0.5f, 0.5f);
            inst.pivot = new Vector2(0.5f, 0.5f);
        }

        built = true;
    }
    
    void Layout()
    {
        if (!built) return;

        //ring.localRotation = Quaternion.Euler(0f, ringAngle, 0f);
        
        for (int i = 0; i < items.Count; i++)
        {
            // 링이 Y축으로 ringAngle만큼 회전했다고 보고, 각 슬롯의 로컬 각도 계산
            float localDeg = i * anglePerItem * (clockwise ? 1f : -1f);
            float worldDeg = localDeg + ringAngle;
            float rad = worldDeg * Mathf.Deg2Rad;

            // x–z 평면 타원 궤도
            float x = Mathf.Cos(rad) * radiusX;
            float z = Mathf.Sin(rad) * radiusZ;

            var rt = items[i];
            rt.localPosition = new Vector3(x, 0f, z);  // ring 기준 3D 위치
            // 기본 회전은 위를 보게
            rt.localRotation = Quaternion.identity;

            // 깊이 기반 스케일/알파(정면 z ≈ -radiusZ에 가까울수록 크게/선명하게)
            // 카메라가 +z에서 -z를 바라본다고 가정. 필요시 부호 반전하세요.
            float tDepth = 0.5f * ( -z / radiusZ + 1f ); // z=-radiusZ -> 1, z=+radiusZ -> 0
            float scale = Mathf.Lerp(scaleRange.x, scaleRange.y, tDepth);
            rt.localScale = Vector3.one * scale;

            var cg = rt.GetComponent<CanvasGroup>();
            if (cg)
                cg.alpha = Mathf.Lerp(alphaRange.x, alphaRange.y, tDepth);

            // 정면을 보게(센터/카메라)
            if (lookAtCamera && uiCamera != null)
            {
                // 빌보드: 카메라를 향해 회전 (Z축은 0으로)
                Vector3 worldPos = rt.position;
                var fwd = (worldPos - uiCamera.transform.position);
                fwd.y = 0f; // 수직 회전 최소화
                if (fwd.sqrMagnitude > 0.0001f)
                    rt.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
            }
            else if (itemLookAtCenter)
            {
                // 센터 바라보기
                Vector3 toward = (center.TransformPoint(Vector3.zero) - rt.position);
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.0001f)
                    rt.rotation = Quaternion.LookRotation(toward.normalized, Vector3.up);
            }
        }

        // Z순서(겹침) 보정: z가 큰(뒤) 아이템이 먼저, 작은(앞) 아이템이 나중에 렌더되게
        // Canvas가 "Screen Space - Camera" 일 때 정렬 안정화
        items.Sort((a, b) => b.localPosition.z.CompareTo(a.localPosition.z));
        for (int s = 0; s < items.Count; s++)
            items[s].SetSiblingIndex(s);
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

        // 수평 드래그 -> Y회전
        ringAngle += -delta.x * dragToAngle;
        if (!loop) ringAngle = Mathf.Clamp(ringAngle, -180f, 180f);
        Layout();
    }

    public void OnEndDrag(PointerEventData e)
    {
        SnapToNearest(false);
    }

    // 가장 가까운 슬롯 각도(Y회전)로 스냅
    void SnapToNearest(bool instant)
    {
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

    // 현재 정면 아이템 인덱스 (정면: z가 가장 작은(카메라 쪽) 항목)
    public int CurrentIndex()
    {
        int idx = 0;
        float best = float.MaxValue;
        for (int i = 0; i < items.Count; i++)
        {
            float z = items[i].localPosition.z;
            if (z < best) { best = z; idx = i; }
        }
        return idx;
    }
}
