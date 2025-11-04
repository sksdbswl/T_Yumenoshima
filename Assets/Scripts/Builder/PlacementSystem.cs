using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private PlaceableCatalogTable catalog;

    [Header("Placement")]
    [SerializeField] private bool useMouse = true;      // 마우스 위치 기준 or 화면 중앙
    [SerializeField] private bool useGrid = true;       // 격자 스냅
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float snapSearchRadius = 0.25f;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Gizmo")]
    [SerializeField] private float gizmoSize = 0.25f;
    [SerializeField] private Color gizmoColor = new Color(0f,1f,0f,0.4f);

    private Camera _cam;
    private GameObject _gizmo;
    private MeshRenderer _gizmoRenderer;
    private static readonly Collider[] _snapBuffer = new Collider[8];

    private PlaceableItem _currentItem;   // 현재 선택된 카탈로그 아이템
    //private PlaceableObject _editingObj;  // 배치 직후 편집 중인 오브젝트(이동/회전)

    void Awake()
    {
        _cam = Camera.main;
        CreateGizmo();
    }

    void Update()
    {
        UpdateGizmo();
        ChangeSelectedItem();

        // 배치 시작(현재 선택된 아이템이 있고, 클릭 지점이 유효하면)
        if (_currentItem != null && Input.GetMouseButtonDown(0) && TryGetPlacementPosition(out Vector3 pos))
        {
            SpawnCurrentAtGizmo();
        }

        // 편집 입력(회전/확정/취소)
        // if (_editingObj != null)
        // {
        //     if (Input.GetKeyDown(KeyCode.R)) RotateEditing(90f);
        //     if (Input.GetKeyDown(KeyCode.Escape)) CancelEditing();
        //     if (Input.GetMouseButtonDown(0)) ConfirmEditing();
        // }
    }
    
    private void ChangeSelectedItem()
    {
        // 간단 입력 예시(프로토타입):
        // 1~5 : 아이템 선택
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SelectCatalogIndex(i);
        }
    }

    // -------- Catalog --------
    /// <summary>
    /// 아이템 선택
    /// </summary>
    public void SelectCatalogIndex(int index)
    {
        if (catalog == null || catalog.Items == null) return;
        if (index < 0 || index >= catalog.Items.Length) return;

        _currentItem = catalog.Items[index];
        Debug.Log($"Selected: {_currentItem.DisplayName} ({_currentItem.Role})");
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
        //if (_editingObj != null) { if (_gizmo.activeSelf) _gizmo.SetActive(false); return; }

        if (!TryGetPlacementPosition(out Vector3 pos))
        {
            if (_gizmo.activeSelf) _gizmo.SetActive(false);
            return;
        }

        if (!_gizmo.activeSelf) _gizmo.SetActive(true);
        _gizmo.transform.position = pos;
    }

    // private bool TryGetPlacementPosition(out Vector3 result)
    // {
    //     result = default;
    //
    //     Ray ray = useMouse
    //         ? _cam.ScreenPointToRay(Input.mousePosition)
    //         : _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
    //
    //     // 1) 스냅만 무시하고 “첫 충돌”을 구함 (Road/Building/Deco도 막아야 하니 포함!)
    //     int firstHitMask = ~BuilderLayers.MASK_SNAP;
    //     if (!Physics.Raycast(ray, out RaycastHit first, maxRayDistance, firstHitMask, QueryTriggerInteraction.Collide))
    //         return false;
    //
    //     int layer = first.collider.gameObject.layer;
    //
    //     // 2) 첫 충돌이 배치 가능한 표면인지 검사
    //     if (layer != BuilderLayers.LAYER_GROUND && layer != BuilderLayers.LAYER_TILE)
    //         return false;
    //
    //     // 3) 여기서부터는 배치 OK
    //     Vector3 pos = first.point;
    //
    //     // 근처 스냅 포인트 탐색(스냅 전용 레이어만)
    //     // int count = Physics.OverlapSphereNonAlloc(
    //     //     pos, snapSearchRadius, _snapBuffer, BuilderLayers.MASK_SNAP, QueryTriggerInteraction.Collide
    //     // );
    //
    //     pos = ApplyGrid(pos, gridSize);
    //     // if (count > 0)
    //     // {
    //     //     float min = float.MaxValue;
    //     //     int best = -1;
    //     //     for (int i = 0; i < count; i++)
    //     //     {
    //     //         float d2 = (pos - _snapBuffer[i].transform.position).sqrMagnitude;
    //     //         if (d2 < min) { min = d2; best = i; }
    //     //     }
    //     //     if (best >= 0) pos = _snapBuffer[best].transform.position;
    //     // }
    //     // else if (useGrid)
    //     // {
    //     //     pos = ApplyGrid(pos, gridSize);
    //     // }
    //
    //     pos.y = Mathf.Max(0f, pos.y);
    //     result = pos;
    //     return true;
    // }
    
    private bool TryGetPlacementPosition(out Vector3 result)
    {
        result = default;

        // 1) 화면 → 레이
        Ray ray = useMouse
            ? _cam.ScreenPointToRay(Input.mousePosition)
            : _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

        // 2) 첫 충돌을 구함 (Road/Building/Deco도 포함, Snap만 제외)
        if (!Physics.Raycast(ray, out RaycastHit firstHit, maxRayDistance, ~BuilderLayers.MASK_SNAP, QueryTriggerInteraction.Collide))
            return false;

        // 3) 첫 충돌이 Ground/Tile이 아니면 배치 불가
        int l = firstHit.collider.gameObject.layer;
        if (l != BuilderLayers.LAYER_GROUND && l != BuilderLayers.LAYER_TILE)
            return false;

        Vector3 pos = firstHit.point;

        // 4) SnapPoint 우선
        int count = Physics.OverlapSphereNonAlloc(
            pos, snapSearchRadius, _snapBuffer, BuilderLayers.MASK_SNAP, QueryTriggerInteraction.Collide
        );
        bool snapped = false;
        if (count > 0)
        {
            float min = float.MaxValue;
            int best = -1;
            for (int i = 0; i < count; i++)
            {
                float d2 = (pos - _snapBuffer[i].transform.position).sqrMagnitude;
                if (d2 < min) { min = d2; best = i; }
            }
            if (best >= 0)
            {
                pos = _snapBuffer[best].transform.position;
                snapped = true;
            }
        }

        // 5) 벽 스냅(스냅포인트가 없을 때만)
        if (!snapped)
        {
            // 배치 프리팹 크기에 맞춰 반가로/반폭 전달(예: 0.5f)
            if (TrySnapToBuildingWall(pos, halfSize: 0.5f, out var wallPos, out var wallRot))
            {
                pos = wallPos;
                // 필요 시 여기서 회전 적용(예: _preview.rotation = wallRot)
                snapped = true;
            }
        }

        // 6) 스냅이 전혀 안 됐으면 그리드
        if (!snapped && useGrid) pos = ApplyGrid(pos, gridSize);

        // 7) Ground 경계 클램프 (항상 마지막에)
        if (groundArea != null) pos = ClampToGroundBounds(pos);

        result = pos;
        return true;
    }
    
    [SerializeField] private Collider groundArea; // Ground에 붙은 Collider (권장: BoxCollider)

    private Vector3 ClampToGroundBounds(Vector3 p)
    {
        var b = groundArea.bounds;
        p.x = Mathf.Clamp(p.x, b.min.x, b.max.x);
        p.z = Mathf.Clamp(p.z, b.min.z, b.max.z);
        // 높이는 강제로 ground 높이에 맞추거나, 기존 y 유지
        p.y = Mathf.Max(p.y, b.min.y);
        return p;
    }

    
    // 배치물이 벽에 어느 정도 접근하면 붙이기
    [SerializeField] private float wallSnapRadius = 0.6f; // 탐색 반경
    [SerializeField] private float wallGap = 0.05f;       // 살짝 띄우기(겹침 방지)
    private static readonly Collider[] _wallBuffer = new Collider[8];

    private bool TrySnapToBuildingWall(Vector3 basePos, float halfSize, out Vector3 snappedPos, out Quaternion snappedRot)
    {
        snappedPos = basePos;
        snappedRot = Quaternion.identity;

        // 주변에 Building 콜라이더가 있는지 찾기
        int n = Physics.OverlapSphereNonAlloc(
            basePos, wallSnapRadius, _wallBuffer, 1 << BuilderLayers.LAYER_BUILDING, QueryTriggerInteraction.Ignore
        );
        if (n <= 0) return false;

        // 가장 가까운 빌딩 콜라이더 선택
        float min = float.MaxValue;
        int best = -1;
        for (int i = 0; i < n; i++)
        {
            float d2 = (basePos - _wallBuffer[i].ClosestPoint(basePos)).sqrMagnitude;
            if (d2 < min) { min = d2; best = i; }
        }
        if (best < 0) return false;

        var col = _wallBuffer[best];
        // 벽 표면의 가장 가까운 점
        Vector3 wallPoint = col.ClosestPoint(basePos);

        // 표면 법선 추정: basePos → wallPoint 방향의 반대
        Vector3 normal = (basePos - wallPoint);
        normal.y = 0f;
        if (normal.sqrMagnitude < 1e-6f) normal = (basePos - col.bounds.center); // 특이 케이스
        normal = normal.normalized;

        // 오브젝트를 벽에서 반대 방향으로 halfSize+gap 만큼 띄워 놓기
        snappedPos = wallPoint + normal * (halfSize + wallGap);
        // 회전도 벽을 바라보도록 정렬(필요 없으면 제거)
        snappedRot = Quaternion.LookRotation(-normal, Vector3.up);
        return true;
    }


    
    bool IsPlaceableSurface(Vector3 p)
    {
        Vector3 origin = p + Vector3.up * 2f;
        int mask = (1 << BuilderLayers.LAYER_GROUND) | (1 << BuilderLayers.LAYER_TILE);
        return Physics.Raycast(origin, Vector3.down, out _, 5f, mask);
    }

    private Vector3 ApplyGrid(Vector3 p, float size)
    {
        p.x = Mathf.Round(p.x / size) * size;
        p.z = Mathf.Round(p.z / size) * size;
        return p;
    }

    // -------- Placement flow --------
    /// <summary>
    /// 실제 배치 로직
    /// </summary>
    private void SpawnCurrentAtGizmo()
    {
        if (_currentItem == null || _currentItem.Prefab == null) return;
        
        GameObject go = Instantiate(_currentItem.Prefab, _gizmo.transform.position, Quaternion.identity);
        
        // 여기서 배치될 아이템의 정보인 PlaceableObject 컴포넌트를 할당 후 초기화 : 초기화 정보는 so기준
        //var obj = go.GetComponent<PlaceableObject>();
        var obj = go.AddComponent<PlaceableObject>();
        obj.Initialize(_currentItem.Role);

        //_editingObj = obj;
        // 배치 직후 편집(마우스로 위치 미세조정)
    }

    // private void RotateEditing(float deg)
    // {
    //     _editingObj.transform.Rotate(Vector3.up, deg, Space.World);
    // }
    //
    // private void ConfirmEditing()
    // {
    //     _editingObj = null;
    // }
    //
    // private void CancelEditing()
    // {
    //     if (_editingObj != null) Destroy(_editingObj.gameObject);
    //     _editingObj = null;
    // }
}

