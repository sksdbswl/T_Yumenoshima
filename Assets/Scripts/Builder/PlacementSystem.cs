using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private PlaceableCatalogTable catalog;

    [Header("Grid")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Collider groundArea; // 없으면 월드 (0,0) 원점
    [SerializeField] private float maxRayDistance = 100f;
    [SerializeField] private bool useMouse = true;

    [Header("Preview (optional)")]
    [SerializeField] private Material previewMaterial;

    private Camera _cam;
    private PlaceableItem _currentItem;
    private GameObject _previewObj;

    // corner(셀 교점)에 둘 때, 프리팹 피벗을 corner로 옮기기 위한 오프셋(= 반폭 스냅)
    private Vector3 _offsetFromCorner = Vector3.zero;

    void Awake()
    {
        _cam = Camera.main;
        _currentItem = (catalog != null && catalog.Items.Length > 0) ? catalog.Items[0] : null;
        CreatePreview();
    }

    void Update()
    {
        // 간단한 카탈로그 전환
        for (int i = 0; i < 5; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectCatalogIndex(i);

        if (TryGetCorner(out var corner))
        {
            var pos = corner + _offsetFromCorner;
            if (_previewObj) { _previewObj.SetActive(true); _previewObj.transform.position = pos; }

            if (Input.GetMouseButtonDown(0))
                Place(pos);
        }
        else if (_previewObj) _previewObj.SetActive(false);
    }

    // -------- Corner Snap --------
    private bool TryGetCorner(out Vector3 corner)
    {
        corner = default;
        if (_cam == null) return false;

        var ray = useMouse
            ? _cam.ScreenPointToRay(Input.mousePosition)
            : _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

        float gy = (groundArea ? groundArea.bounds.min.y : 0f);
        if (Mathf.Approximately(ray.direction.y, 0f)) return false;

        float t = (gy - ray.origin.y) / ray.direction.y;
        if (t < 0f || t > maxRayDistance) return false;

        var hit = ray.origin + ray.direction * t; // 바닥 교점
        hit.y = gy;

        Vector2 origin = groundArea ? new Vector2(groundArea.bounds.min.x, groundArea.bounds.min.z) : Vector2.zero;

        // “셀 중앙”이 아니라 “셀 모서리(교점)”로 스냅
        float u = (hit.x - origin.x) / gridSize;
        float v = (hit.z - origin.y) / gridSize;

        float xCorner = Mathf.Round(u) * gridSize + origin.x;
        float zCorner = Mathf.Round(v) * gridSize + origin.y;

        corner = new Vector3(xCorner, gy, zCorner);
        return true;
    }

    // -------- Catalog / Preview --------
    public void SelectCatalogIndex(int index)
    {
        if (catalog == null || catalog.Items == null) return;
        if (index < 0 || index >= catalog.Items.Length) return;
        _currentItem = catalog.Items[index];
        CreatePreview();
    }

    private void CreatePreview()
    {
        if (_previewObj) Destroy(_previewObj);
        if (_currentItem == null || _currentItem.Prefab == null) return;

        _previewObj = Instantiate(_currentItem.Prefab);
        _previewObj.name = "[Preview] " + _currentItem.DisplayName;

        if (previewMaterial)
        {
            foreach (var r in _previewObj.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = previewMaterial;
                r.sharedMaterials = mats;
            }
        }

        // ▼ 핵심: 프리팹의 월드 바운즈로 반폭을 구해 “그리드 정배수”로 스냅 → 좌하단 모서리를 corner에 맞춤
        RecalculateCornerOffset();
        _previewObj.SetActive(false);
    }

    private void RecalculateCornerOffset()
    {
        var rens = _previewObj.GetComponentsInChildren<Renderer>();
        if (rens.Length == 0) { _offsetFromCorner = Vector3.zero; return; }

        Bounds b = rens[0].bounds;
        for (int i = 1; i < rens.Length; i++) b.Encapsulate(rens[i].bounds);

        // 바운즈 “가로/세로 길이”를 그리드 크기 배수로 스냅(오차/틈 방지)
        float snapX = Mathf.Round(b.size.x / gridSize) * gridSize;
        float snapZ = Mathf.Round(b.size.z / gridSize) * gridSize;

        // 피벗이 센터라고 가정하고, 좌하단 모서리가 코너에 오도록 반폭만큼 +X,+Z 이동
        _offsetFromCorner = new Vector3(snapX * 0.5f, 0f, snapZ * 0.5f);
    }

    // -------- Place --------
    private void Place(Vector3 pos)
    {
        if (_currentItem == null || _currentItem.Prefab == null) return;

        var go = Instantiate(_currentItem.Prefab, pos, Quaternion.identity);
        var obj = go.AddComponent<PlaceableObject>();
        obj.Initialize(_currentItem.Role);
    }
}
