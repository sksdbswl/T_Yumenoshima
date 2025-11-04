using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private PlaceableCatalogTable catalog;

    [Header("Placement")]
    [SerializeField] private bool useMouse = true;      // 마우스 위치 기준 or 화면 중앙
    [SerializeField] private bool useGrid = true;       // 격자 스냅 사용 여부
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Preview")]
    [SerializeField] private Material previewMaterial; // 투명 미리보기용 머티리얼

    [Header("Gizmo")]
    [SerializeField] private float gizmoSize = 0.25f;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.4f);

    [Header("Ground Clamp (선택)")]
    [SerializeField] private Collider groundArea; // Ground에 붙은 Collider (바닥 영역)

    private Camera _cam;
    private GameObject _gizmo;
    private MeshRenderer _gizmoRenderer;

    private PlaceableItem _currentItem;
    private GameObject _previewObj; // 미리보기 오브젝트

    void Awake()
    {
        _cam = Camera.main;
        _currentItem = catalog != null && catalog.Items.Length > 0 ? catalog.Items[0] : null;
        CreateGizmo();
        if (_currentItem != null) CreatePreviewObject();
    }

    void Update()
    {
        UpdateGizmo();
        ChangeSelectedItem();

        if (_currentItem != null && Input.GetMouseButtonDown(0) && TryGetPlacementPosition(out Vector3 pos))
        {
            ConfirmPlacement();
        }
    }

    // -------- Catalog --------
    private void ChangeSelectedItem()
    {
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SelectCatalogIndex(i);
        }
    }

    public void SelectCatalogIndex(int index)
    {
        if (catalog == null || catalog.Items == null) return;
        if (index < 0 || index >= catalog.Items.Length) return;

        _currentItem = catalog.Items[index];
        Debug.Log($"Selected: {_currentItem.DisplayName} ({_currentItem.Role})");
        CreatePreviewObject();
    }

    // -------- Gizmo --------
    private void CreateGizmo()
    {
        _gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(_gizmo.GetComponent<Collider>());
        _gizmo.transform.localScale = Vector3.one * gizmoSize;

        var mr = _gizmo.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = gizmoColor;
        mr.sharedMaterial = mat;
        _gizmoRenderer = mr;

        _gizmo.SetActive(false);
    }

    private void UpdateGizmo()
    {
        if (!TryGetPlacementPosition(out Vector3 pos))
        {
            if (_gizmo.activeSelf) _gizmo.SetActive(false);
            if (_previewObj) _previewObj.SetActive(false);
            return;
        }

        if (!_gizmo.activeSelf) _gizmo.SetActive(true);
        _gizmo.transform.position = pos;

        if (_previewObj)
        {
            _previewObj.SetActive(true);
            _previewObj.transform.position = pos;
        }
    }

    // -------- Preview Logic --------
    private void CreatePreviewObject()
    {
        if (_previewObj != null) Destroy(_previewObj);

        if (_currentItem == null || _currentItem.Prefab == null)
        {
            Debug.LogWarning("No prefab assigned for preview.");
            return;
        }

        _previewObj = Instantiate(_currentItem.Prefab);
        _previewObj.name = "[Preview] " + _currentItem.DisplayName;

        ApplyPreviewMaterial(_previewObj);
    }

    private void ApplyPreviewMaterial(GameObject obj)
    {
        if (previewMaterial == null) return;

        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = previewMaterial;
            r.sharedMaterials = mats;
        }
    }

    // -------- Placement Logic --------
    /// <summary>
    /// 레이가 무엇을 맞아도 최종 좌표는 "바닥 y평면"과의 교점으로 계산한다.
    /// 이렇게 해야 이미 놓은 타일 상단을 맞아도 점프(건너뛰기)가 발생하지 않는다.
    /// </summary>
    private bool TryGetPlacementPosition(out Vector3 result)
    {
        result = default;

        // 1) 레이 얻기 (마우스 혹은 화면 중앙)
        Ray ray = useMouse
            ? _cam.ScreenPointToRay(Input.mousePosition)
            : _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

        // 2) 바닥 y 결정
        float groundY = 0f;
        if (groundArea != null)
            groundY = groundArea.bounds.min.y; // Ground 콜라이더의 최하단을 바닥 y로 사용

        // 3) 레이와 y=groundY 평면 교점 계산 (물리 Raycast 의존 X)
        float dirY = ray.direction.y;
        if (Mathf.Approximately(dirY, 0f)) return false;           // 평행: 교점 없음
        float t = (groundY - ray.origin.y) / dirY;
        if (t < 0f || t > maxRayDistance) return false;            // 뒤쪽이거나 너무 멀면 무시

        Vector3 pos = ray.origin + ray.direction * t;
        pos.y = groundY; // 보정(숫자오차 방지)

        // 4) 격자 스냅
        if (useGrid)
        {
            pos = _currentItem != null && _currentItem.UseHalfOffset
                ? ApplyGridHalf(pos, gridSize)
                : ApplyGridCenter(pos, gridSize);
        }
        
        // if (_currentItem != null && _currentItem.UseHalfOffset)
        // {
        //     if (!IsEdgeNotCorner(pos, gridSize))
        //         return false; // 모서리나 정중앙 등은 무효 처리
        // }

        // 5) 경계 클램프(선택)
        if (groundArea != null)
        {
            float half = gridSize * 0.5f;
            pos = ClampToGroundBounds(pos, half, half);
        }

        result = pos;
        return true;
    }

    // -------- Grid Snapping --------
    private Vector3 ApplyGridCenter(Vector3 p, float size)
    {
        p.x = Mathf.Round(p.x / size) * size;
        p.z = Mathf.Round(p.z / size) * size;
        return p;
    }

    private Vector3 ApplyGridHalf(Vector3 p, float size)
    {
        p.x = Mathf.Floor(p.x / size) * size + size * 0.5f;
        p.z = Mathf.Floor(p.z / size) * size + size * 0.5f;
        return p;
    }

    private Vector3 ClampToGroundBounds(Vector3 p, float halfX, float halfZ)
    {
        var b = groundArea.bounds;
        p.x = Mathf.Clamp(p.x, b.min.x + halfX, b.max.x - halfX);
        p.z = Mathf.Clamp(p.z, b.min.z + halfZ, b.max.z - halfZ);
        p.y = Mathf.Max(p.y, b.min.y);
        return p;
    }

    // -------- Placement Confirm --------
    
    private void ConfirmPlacement()
    {
        if (_currentItem == null || _currentItem.Prefab == null) return;
        if (!_previewObj) return;

        Vector3 pos = _previewObj.transform.position;
        Quaternion rot = _previewObj.transform.rotation;

        GameObject go = Instantiate(_currentItem.Prefab, pos, rot);
        var obj = go.AddComponent<PlaceableObject>();
        obj.Initialize(_currentItem.Role);

        Debug.Log($"Placed: {_currentItem.DisplayName} at {pos}");
        // 미리보기는 유지 (필요 시 재생성 로직 추가)
    }
    
    // 모서리 금지
    const float EPS = 0.001f;

    private bool IsEdgeNotCorner(Vector3 p, float size)
    {
        // 그리드 좌표계에서 0~1 구간의 위치
        float gx = Mathf.Repeat(p.x / size, 1f);
        float gz = Mathf.Repeat(p.z / size, 1f);

        bool xHalf = Mathf.Abs(gx - 0.5f) < EPS; // x가 정확히 0.5칸
        bool zHalf = Mathf.Abs(gz - 0.5f) < EPS; // z가 정확히 0.5칸

        // 엣지 = 축 중 하나만 0.5칸(모서리는 둘 다 0.5칸이라 false)
        return xHalf ^ zHalf;
    }

}
