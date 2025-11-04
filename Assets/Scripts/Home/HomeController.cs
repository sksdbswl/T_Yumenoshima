using REIW.LoneGarden;
using UnityEngine;
using UnityEngine.EventSystems;

public class HomeController : MonoBehaviour
{
    [Header("Setup")]
    public Camera cam;
    public LayerMask groundMask;     // 바닥(레이캐스트)
    public LayerMask blockMask;      // 배치물 레이어
    public Material okMat;
    public Material badMat;
    public HomeGrid grid;    

    [Header("Runtime")]
    public PlaceItemSO[] selectedItems;
    public PlaceItemSO selectedItem;
    
    GameObject _ghost;
    Renderer[] _ghostRenderers;
    bool _valid;
    float _yaw;

    private void Awake()
    {
        selectedItem = selectedItems[0];
    }
    
    void Update()
    {
        ChangeSelectedItem();
        
        if (selectedItem == null || grid == null) return;
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        // 프리뷰 생성
        if (_ghost == null)
        {
            _ghost = Instantiate(selectedItem.prefab);
            _ghost.layer = LayerMask.NameToLayer("Ignore Raycast");
            foreach (Transform t in _ghost.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            _ghostRenderers = _ghost.GetComponentsInChildren<Renderer>(true);
            SetGhostMaterial(okMat);
            _yaw = 0f;
        }

        // 마우스 → 바닥 히트
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 1000f, groundMask))
            return;

        // 회전 (Q/E)
        if (Input.GetKeyDown(KeyCode.Q)) _yaw -= selectedItem.requireGrid ? 90f : 15f;
        if (Input.GetKeyDown(KeyCode.E)) _yaw += selectedItem.requireGrid ? 90f : 15f;

        Vector3 snappedWorld;
        Vector2Int topLeftCell;

        if (selectedItem.requireGrid)
        {
            // 1) 월드→그리드
            if (!grid.WorldToGrid(hit.point, out int gx, out int gy))
            {
                // 그리드 밖 → 무효
                _valid = false;
                SetGhostMaterial(badMat);
                return;
            }

            // 2) 회전에 따른 footprint (90도면 가로세로 뒤집기)
            var fp = selectedItem.footprint;
            if (Mathf.RoundToInt(Mathf.Abs(Mathf.Repeat(_yaw, 180f))) == 90)
                fp = new Vector2Int(fp.y, fp.x);

            // 3) 그리드 경계 클램프(좌상단 기준)
            gx = Mathf.Clamp(gx, 0, grid.Width - fp.x);
            gy = Mathf.Clamp(gy, 0, grid.Height - fp.y);

            // 4) 스냅된 월드 좌표(셀 센터)로 이동
            snappedWorld = grid.GridToWorld(gx, gy, hit.point.y);
            topLeftCell = new Vector2Int(gx, gy);

            _ghost.transform.SetPositionAndRotation(snappedWorld, Quaternion.Euler(0, _yaw, 0));

            // 5) 유효성 검사: (a) 영역 내부 (b) 겹침 없음
            _valid = ValidateInsideGrid(gx, gy, fp) && ValidateOverlap(snappedWorld, _yaw, fp);
        }
        else
        {
            // 자유 배치(그리드 없이): 위치만 높이 유지
            snappedWorld = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            topLeftCell = default;

            _ghost.transform.SetPositionAndRotation(snappedWorld, Quaternion.Euler(0, _yaw, 0));
            _valid = ValidateOverlap(snappedWorld, _yaw, selectedItem.footprint);
        }

        SetGhostMaterial(_valid ? okMat : badMat);

        // 배치 / 취소
        if (Input.GetMouseButtonDown(0) && _valid) Commit(snappedWorld);
        if (Input.GetMouseButtonDown(1)) Cancel();
    }

    bool ValidateInsideGrid(int gx, int gy, Vector2Int fp)
    {
        // 좌상단(gx,gy)에서 footprint 전체가 그리드 내부인지
        return gx >= 0 && gy >= 0 &&
               gx + fp.x <= grid.Width &&
               gy + fp.y <= grid.Height;
    }
    
    private void ChangeSelectedItem()
    {
        // 간단 입력 예시(프로토타입):
        // 1~5 : 아이템 선택
        for (int i = 0; i < 2; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedItem = selectedItems[i];
                previewGhost(selectedItem);
            }
        }
    }

    private void previewGhost(PlaceItemSO selectedItem)
    {
        _ghost = null;
       
        _ghost = Instantiate(selectedItem.prefab);
        _ghost.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Transform t in _ghost.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        _ghostRenderers = _ghost.GetComponentsInChildren<Renderer>(true);
        SetGhostMaterial(okMat);
        _yaw = 0f;
        
    }

    /// <summary>
    /// 겹침 판정 박스를 너무 딱 맞게 잡아서, 부동소수/접촉 오차 때문에 겹친 걸로 인식
    /// 큐브(1×1×1) ↔ 그리드 셀(1) 이면 옆 칸과 면-면 접촉(거리=정확히 1).
    /// OverlapBox는 구현상 “겹침(or 경계 접촉)”을 포함(≤ 비교 + float 오차).
    /// 그래서 “맞닿아만 있어도” 히트로 간주 → 배치 불가(빨강).
    /// </summary>
    // bool ValidateOverlap(Vector3 posWorld, float yaw, Vector2Int fp)
    // {
    //     // 스냅된 위치 + footprint 크기 기반 OverlapBox
    //     var sizeWorld = new Vector2(fp.x * grid.CellSize, fp.y * grid.CellSize);
    //     var center = posWorld + Vector3.up * 0.25f;
    //     var half = new Vector3(sizeWorld.x * 0.5f, 0.5f, sizeWorld.y * 0.5f);
    //
    //     var hits = Physics.OverlapBox(center, half, Quaternion.Euler(0, yaw, 0),
    //                                   blockMask, QueryTriggerInteraction.Ignore);
    //     return hits.Length == 0;
    // }
    
    bool ValidateOverlap(Vector3 pos, float yaw, Vector2Int f)
    {
        var sizeWorld = new Vector2(f.x * grid.CellSize, f.y * grid.CellSize);
        var center = pos + Vector3.up * 0.25f;

        // 여유치(패딩)만큼 가로/세로 반경을 감소
        const float epsilon = 0.02f; // 2cm 정도, 필요하면 0.01~0.03 사이로 튜닝
        float halfX = Mathf.Max(0.01f, (sizeWorld.x * 0.5f) - epsilon);
        float halfZ = Mathf.Max(0.01f, (sizeWorld.y * 0.5f) - epsilon);

        var half = new Vector3(halfX, 0.5f, halfZ);

        var hits = Physics.OverlapBox(
            center,
            half,
            Quaternion.Euler(0, yaw, 0),
            blockMask,
            QueryTriggerInteraction.Ignore
        );

        return hits.Length == 0;
    }


    void SetGhostMaterial(Material m)
    {
        if (_ghostRenderers == null) return;
        foreach (var r in _ghostRenderers)
            if (r) r.material = m;
    }

    void Commit(Vector3 finalPos)
    {
        var placed = Instantiate(selectedItem.prefab, finalPos, Quaternion.Euler(0, _yaw, 0));
        var placeLayer = LayerMaskToFirstLayer(blockMask);
        if (placeLayer >= 0) SetLayerRecursive(placed, placeLayer);
        // 고스트는 계속 유지(같은 아이템 연속 배치용)
    }

    void Cancel()
    {
        if (_ghost) Destroy(_ghost);
        _ghost = null;
        selectedItem = null;
    }

    static int LayerMaskToFirstLayer(LayerMask mask)
    {
        int m = mask.value;
        for (int i = 0; i < 32; i++)
            if ((m & (1 << i)) != 0) return i;
        return -1;
    }

    static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }
}
