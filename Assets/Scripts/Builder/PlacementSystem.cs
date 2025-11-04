using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private PlaceableCatalogTable catalog;

    [Header("Placement")]
    [SerializeField] private bool useMouse = true;      // 마우스 위치 기준 or 화면 중앙
    [SerializeField] private bool useGrid = true;       // 격자 스냅 사용 여부
    [SerializeField] private bool useHalfOffset = true; // 0.5칸(셀 경계) 기준 스냅 여부
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float snapSearchRadius = 0.25f;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Gizmo")]
    [SerializeField] private float gizmoSize = 0.25f;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.4f);

    private Camera _cam;
    private GameObject _gizmo;
    private MeshRenderer _gizmoRenderer;
    private static readonly Collider[] _snapBuffer = new Collider[8];

    private PlaceableItem _currentItem;   // 현재 선택된 카탈로그 아이템

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
            return;
        }

        if (!_gizmo.activeSelf) _gizmo.SetActive(true);
        _gizmo.transform.position = pos;
    }

    // -------- Placement logic --------
    private bool TryGetPlacementPosition(out Vector3 result)
    {
        result = default;

        Ray ray = useMouse
            ? _cam.ScreenPointToRay(Input.mousePosition)
            : _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

        // 스냅 제외하고 첫 충돌(건물, 도로, 데코 포함)
        int mask = ~BuilderLayers.MASK_SNAP;
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, mask, QueryTriggerInteraction.Collide))
            return false;

        int layer = hit.collider.gameObject.layer;

        // 배치 가능한 표면만 허용
        if (layer != BuilderLayers.LAYER_GROUND && layer != BuilderLayers.LAYER_TILE)
            return false;

        Vector3 pos = hit.point;

        // 그리드 기준 스냅
        if (useGrid)
        {
            pos = useHalfOffset ? ApplyGridHalf(pos, gridSize)  // ✅ 경계 기준
                                : ApplyGridCenter(pos, gridSize); // 중앙 기준
        }

        pos.y = Mathf.Max(0f, pos.y);
        result = pos;
        return true;
    }

    // -------- Grid snapping --------
    // 중앙(셀 중심) 기준 스냅
    private Vector3 ApplyGridCenter(Vector3 p, float size)
    {
        p.x = Mathf.Round(p.x / size) * size;
        p.z = Mathf.Round(p.z / size) * size;
        return p;
    }

    // ✅ 셀 경계(0.5 오프셋) 기준 스냅
    private Vector3 ApplyGridHalf(Vector3 p, float size)
    {
        p.x = Mathf.Floor(p.x / size) * size + size * 0.5f;
        p.z = Mathf.Floor(p.z / size) * size + size * 0.5f;
        return p;
    }

    // -------- Placement spawn --------
    private void SpawnCurrentAtGizmo()
    {
        if (_currentItem == null || _currentItem.Prefab == null) return;

        GameObject go = Instantiate(_currentItem.Prefab, _gizmo.transform.position, Quaternion.identity);

        var obj = go.AddComponent<PlaceableObject>();
        obj.Initialize(_currentItem.Role);
    }

    // -------- Utility --------
    bool IsPlaceableSurface(Vector3 p)
    {
        Vector3 origin = p + Vector3.up * 2f;
        int mask = (1 << BuilderLayers.LAYER_GROUND) | (1 << BuilderLayers.LAYER_TILE);
        return Physics.Raycast(origin, Vector3.down, out _, 5f, mask);
    }
}
