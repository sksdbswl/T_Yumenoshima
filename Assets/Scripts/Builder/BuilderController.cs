using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderController : MonoBehaviour
{
    [Header("Setup")]
    public Camera cam;
    public LayerMask groundMask;   // 바닥(레이캐스트 용)
    public LayerMask blockMask;    // 충돌 금지 레이어(설치물들)
    public float gridSize = 1f;
    public Material okMat;
    public Material badMat;

    [Header("Runtime")]
    public PlaceItemSO selectedItem;

    GameObject _ghost;
    Renderer[] _ghostRenderers;
    bool _valid;
    float _yaw;

    void Update()
    {
        if (selectedItem == null) return;
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        // 프리뷰 생성
        if (_ghost == null)
        {
            _ghost = Instantiate(selectedItem.prefab);
            _ghost.layer = LayerMask.NameToLayer("Ignore Raycast"); // 마우스 히트 방해 X
            foreach (Transform t in _ghost.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            _ghostRenderers = _ghost.GetComponentsInChildren<Renderer>(true);
            SetGhostMaterial(okMat);
            _yaw = 0f;
        }

        // 마우스 → 바닥 히트
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000f, groundMask))
        {
            var pos = hit.point;

            // 그리드 스냅
            if (selectedItem.requireGrid)
            {
                pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
                pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
            }

            _ghost.transform.position = new Vector3(pos.x, hit.point.y, pos.z);
            _ghost.transform.rotation = Quaternion.Euler(0, _yaw, 0);

            // 회전 (Q/E)
            if (Input.GetKeyDown(KeyCode.Q)) _yaw -= selectedItem.requireGrid ? 90f : 15f;
            if (Input.GetKeyDown(KeyCode.E)) _yaw += selectedItem.requireGrid ? 90f : 15f;

            // 충돌/푸트프린트 검사
            _valid = Validate(_ghost.transform.position, _yaw);
            SetGhostMaterial(_valid ? okMat : badMat);

            // 배치 / 취소
            if (Input.GetMouseButtonDown(0) && _valid) Commit();
            if (Input.GetMouseButtonDown(1)) Cancel();
        }
    }

    bool Validate(Vector3 pos, float yaw)
    {
        // 1) footprint 크기 박스로 겹침 검사 (OverlapBox)
        var size = FootprintSizeWorld(yaw);
        var center = pos + new Vector3(0, 0.25f, 0); // 살짝 위로
        var half = new Vector3(size.x * 0.5f, 0.5f, size.y * 0.5f);

        var hits = Physics.OverlapBox(center, half, Quaternion.Euler(0, yaw, 0), blockMask, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0) return false;

        // 2) 추가 규칙 (예시) : 길은 그리드 필수
        if (selectedItem.category == PlaceCategory.Road && !selectedItem.requireGrid)
            return false;

        return true;
    }

    Vector2 FootprintSizeWorld(float yaw)
    {
        // 90도 단위 회전 시 footprint 전환 (2x3 ↔ 3x2)
        var f = selectedItem.footprint;
        if (selectedItem.requireGrid)
        {
            var rot90 = Mathf.RoundToInt(Mathf.Abs(Mathf.Repeat(yaw, 180f)));
            if (rot90 == 90) return new Vector2(f.y * gridSize, f.x * gridSize);
        }
        return new Vector2(f.x * gridSize, f.y * gridSize);
    }

    void SetGhostMaterial(Material m)
    {
        if (_ghostRenderers == null) return;
        foreach (var r in _ghostRenderers)
        {
            // 머티리얼 인스턴스화 방지: 공유 머티리얼 변경은 피하고, 간단히 material 사용
            if (r) r.material = m;
        }
    }

    void Commit()
    {
        // 프리뷰를 실제 설치물로 전환: 새로 Instantiate해서 충돌 레이어에 배치
        var placed = Instantiate(selectedItem.prefab, _ghost.transform.position, _ghost.transform.rotation);
        // 설치물은 blockMask에 포함된 레이어 사용 (예: "Placeable")
        var placeLayer = LayerMaskToFirstLayer(blockMask);
        if (placeLayer >= 0) SetLayerRecursive(placed, placeLayer);

        // 계속 같은 아이템을 깔고 싶다면 고스트 유지, 아니면 제거
        // 여기선 유지
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
