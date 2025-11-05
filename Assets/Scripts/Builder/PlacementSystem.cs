using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private PlaceableCatalogTable catalog;

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Collider groundArea;
    [SerializeField] private bool useMouse = true;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Preview Materials")]
    [SerializeField] private Material previewMaterial;  // 정상
    [SerializeField] private Material blockedMaterial;  // 배치 불가

    private Camera _cam;
    private PlaceableItem _currentItem;
    private GameObject _previewObj;
    private Vector3 _offsetFromCorner = Vector3.zero;

    void Awake()
    {
        _cam = Camera.main;
        _currentItem = (catalog && catalog.Items.Length > 0) ? catalog.Items[0] : null;
        CreatePreview();
    }

    void Update()
    {
        for (int i = 0; i < 5; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectCatalogIndex(i);

        if (TryGetCorner(out var corner, out bool canPlace))
        {
            Vector3 pos = corner + _offsetFromCorner;

            if (_previewObj)
            {
                _previewObj.SetActive(true);
                _previewObj.transform.position = pos;
                ApplyMaterial(canPlace ? previewMaterial : blockedMaterial);
            }

            if (canPlace && Input.GetMouseButtonDown(0))
                Place(pos);
        }
        else if (_previewObj)
        {
            _previewObj.SetActive(false);
        }
    }

    // ---------------- Corner Snap + Stack Check ----------------
    private bool TryGetCorner(out Vector3 corner, out bool canPlace)
    {
        corner = default;
        canPlace = false;
        if (_cam == null) return false;

        Ray ray = useMouse
            ? _cam.ScreenPointToRay(Input.mousePosition)
            : _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

        float gy = (groundArea ? groundArea.bounds.min.y : 0f);
        if (Mathf.Approximately(ray.direction.y, 0f)) return false;

        float t = (gy - ray.origin.y) / ray.direction.y;
        if (t < 0f || t > maxRayDistance) return false;

        Vector3 hit = ray.origin + ray.direction * t;
        hit.y = gy;

        Vector2 origin = groundArea ? new Vector2(groundArea.bounds.min.x, groundArea.bounds.min.z) : Vector2.zero;

        float u = (hit.x - origin.x) / gridSize;
        float v = (hit.z - origin.y) / gridSize;

        float xCorner = Mathf.Round(u) * gridSize + origin.x;
        float zCorner = Mathf.Round(v) * gridSize + origin.y;
        corner = new Vector3(xCorner, gy, zCorner);

        // ---- 스택 불가 체크 ----
        Vector3 cellCenter = corner + _offsetFromCorner + new Vector3(0, 0.5f, 0);
        Vector3 half = new Vector3(gridSize * 0.45f, 0.5f, gridSize * 0.45f);

        Collider[] cols = Physics.OverlapBox(cellCenter, half);
        foreach (var c in cols)
        {
            var po = c.GetComponentInParent<PlaceableObject>();
            if (po != null && po.SourceItem != null && !po.SourceItem.IsStack)
            {
                canPlace = false;
                return true; // 배치 위치는 있지만 불가
            }
        }

        canPlace = true;
        return true;
    }

    // ---------------- Preview ----------------
    private void CreatePreview()
    {
        if (_previewObj) Destroy(_previewObj);
        if (_currentItem == null || _currentItem.Prefab == null) return;

        _previewObj = Instantiate(_currentItem.Prefab);
        _previewObj.name = "[Preview] " + _currentItem.DisplayName;

        ApplyMaterial(previewMaterial);
        CalculateOffsetFromCorner();  // 🔹 진짜 핵심
        _previewObj.SetActive(false);
    }

    private void ApplyMaterial(Material mat)
    {
        if (_previewObj == null || mat == null) return;
        foreach (var r in _previewObj.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = mat;
    }

    // 프리팹의 월드 바운즈 크기 기반으로 "좌하단 모서리가 corner에 닿도록" 오프셋 계산
    private void CalculateOffsetFromCorner()
    {
        var rens = _previewObj.GetComponentsInChildren<Renderer>();
        if (rens.Length == 0) { _offsetFromCorner = Vector3.zero; return; }

        Bounds b = rens[0].bounds;
        for (int i = 1; i < rens.Length; i++) b.Encapsulate(rens[i].bounds);

        // 실제 풋프린트 크기를 grid 단위로 스냅
        float snapX = Mathf.Round(b.size.x / gridSize) * gridSize;
        float snapZ = Mathf.Round(b.size.z / gridSize) * gridSize;

        // 좌하단 corner에 맞추기 위해 반폭만큼 +X,+Z 이동
        _offsetFromCorner = new Vector3(snapX * 0.5f, 0f, snapZ * 0.5f);
    }

    // ---------------- Catalog ----------------
    public void SelectCatalogIndex(int index)
    {
        if (catalog == null || catalog.Items == null) return;
        if (index < 0 || index >= catalog.Items.Length) return;
        _currentItem = catalog.Items[index];
        CreatePreview();
    }

    // ---------------- Place ----------------
    private void Place(Vector3 pos)
    {
        if (_currentItem == null || _currentItem.Prefab == null) return;

        GameObject go = Instantiate(_currentItem.Prefab, pos, Quaternion.identity);
        var obj = go.AddComponent<PlaceableObject>();
        obj.Initialize(_currentItem.Role, _currentItem);
    }
}
