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
        if (_currentItem != null && Input.GetMouseButtonDown(0))
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

    private bool TryGetPlacementPosition(out Vector3 result)
    {
        // 기본값(0,0,0)
        result = default;
        
        // useMouse가 true면 마우스 위치 기준으로 Ray(광선) false면 화면 중앙에서 쏨
        Ray ray = useMouse
            ? _cam.ScreenPointToRay(Input.mousePosition)
            : _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));


        //~BuilderLayers.MASK_SNAP → MASK_SNAP 레이어만 무시
        // if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, ~BuilderLayers.MASK_SNAP))
        //     return false;

        // 변경 Ground, Tile만 제외하고 전체 true
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, BuilderLayers.MASK_RAYCAST_PLACEMENT))
            return false;
        
        Vector3 pos = hit.point;
        int count = Physics.OverlapSphereNonAlloc(
            pos, snapSearchRadius, _snapBuffer, BuilderLayers.MASK_SNAP, QueryTriggerInteraction.Collide
        );
        
        if (count > 0)
        {
            // 가장 가까운 스냅 포인트로 이동
            float min = float.MaxValue;
            int best = 0;
            for (int i = 0; i < count; i++)
            {
                float d2 = (pos - _snapBuffer[i].transform.position).sqrMagnitude;
                if (d2 < min) { min = d2; best = i; }
            }
            pos = _snapBuffer[best].transform.position;
        }
        else if (useGrid)
        {
            pos = ApplyGrid(pos, gridSize);
        }
        
        pos.y = Mathf.Max(0f, pos.y); // 바닥 아래로 안 내려가게
        result = pos;
        return true;
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

