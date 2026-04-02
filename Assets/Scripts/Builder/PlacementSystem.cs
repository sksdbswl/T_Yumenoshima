using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private PlaceableCatalogTable catalog;

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Collider groundArea;       // 배치 가능한 영역(반드시 지정 권장)
    [SerializeField] private bool useMouse = true;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Preview Materials")]
    [SerializeField] private Material previewMaterial;  // 정상 배치 프리뷰
    [SerializeField] private Material blockedMaterial;  // 배치 불가 프리뷰

    private Camera _cam;
    private PlaceableItem _currentItem;
    private GameObject _previewObj;
    private Vector3 _offsetFromCorner = Vector3.zero;

    public PlaceableCatalogTable CatalogTable => catalog;
    
    void Awake()
    {
        _cam = Camera.main;
        _currentItem = (catalog && catalog.Items.Length > 0) ? catalog.Items[0] : null;
        CreatePreview();
    }

    private void OnDisable()
    {
        _currentItem = null;
        _previewObj.SetActive(false);
    }

    void Update()
    {
        // 숫자 1~5 단축키로 카탈로그 선택
        for (int i = 0; i < 6; i++)
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
                Place(pos, true);
        }
        else if (_previewObj)
        {
            _previewObj.SetActive(false);
        }
    }

    // ---------------- Corner Snap + Stack/영역 체크 ----------------
    [SerializeField] private LayerMask groundMask; 

    private bool TryGetCorner(out Vector3 corner, out bool canPlace)
    {
        corner = default;
        canPlace = false;
        
        if (_currentItem ==null) return false;
        if (_cam == null || groundArea == null) return false;

        // 마우스/중앙 레이
        //* 0.5f (또는 / 2f) 를 해주면 x = 960, y = 540 즉 화면의 가운데 좌표
        Ray ray = useMouse
            ? _cam.ScreenPointToRay(Input.mousePosition) // 마우스 기준 레이
            : _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)); //카메라의 중심으로 방향으로 레이 생성 

        // 오직 groundMask에만 히트, 그리고 반드시 groundArea여야 함
        if (!Physics.Raycast(ray, out var hitInfo, maxRayDistance, groundMask)) return false;
        if (hitInfo.collider != groundArea) return false;

        // 히트 지점 기준으로 그리드 스냅
        var bounds = groundArea.bounds; // groundArea(바닥 콜라이더)의 전체 범위를 가져옴
        
        // origin: 그리드 계산을 어디서부터 시작할지를 알려주는 기준점(base point)
        Vector2 origin = new Vector2(bounds.min.x, bounds.min.z); // 바닥의 왼쪽-뒤 코너(min.x, min.z)를 그리드의 기준점(origin) 으로 설정.
        float gy = hitInfo.point.y; // 실제 지면 y :  hitInfo.point.y는 바닥의 실제 높이(y축 값).
        
        
        // hitInfo.point.x - origin.x → 바닥의 시작점으로부터 얼마나 떨어졌나 (x축 방향)
        // gridSize → 그걸 그리드 칸 단위로 변환
        float u = (hitInfo.point.x - origin.x) / gridSize; // 이 좌표가 그리드 기준으로 몇 칸 위치인지
        float v = (hitInfo.point.z - origin.y) / gridSize;

        //칸 번호를 다시 실제 거리로 환산 : 월드 좌표 기준
        float xCorner = Mathf.Round(u) * gridSize + origin.x;
        float zCorner = Mathf.Round(v) * gridSize + origin.y;
        corner = new Vector3(xCorner, gy, zCorner);

        // (선택) 바운즈 밖 스냅 차단: 코너+오프셋이 영역 밖이면 배치 불가/미리보기 끔
        Vector3 footprintMin = corner + _offsetFromCorner - new Vector3(gridSize * 0.5f, 0f, gridSize * 0.5f); // footprintMin: 오브젝트의 왼쪽-뒤 코너 (x, z 최소값)
        Vector3 footprintMax = corner + _offsetFromCorner + new Vector3(gridSize * 0.5f, 0f, gridSize * 0.5f); //  footprintMax: 오브젝트의 오른쪽-앞 코너 (x, z 최대값)
        
        // bounds.Contains(point) → 주어진 점이 groundArea의 범위 안에 있으면 true
        // 두 점(footprintMin, footprintMax)이 둘 다 영역 안에 있어야 “완전히 포함”
        // 하나라도 밖이면 배치 불가 → return false
        if (!bounds.Contains(new Vector3(footprintMin.x, gy, footprintMin.z)) ||
            !bounds.Contains(new Vector3(footprintMax.x, gy, footprintMax.z)))
        {
            // 미리보기까지 숨기려면 false 반환
            return false;
            // 미리보기는 보이되 빨간색으로만 보이게 하려면:
            // canPlace = false; return true;
        }

        // 스택 충돌 체크
        Vector3 cellCenter = corner + _offsetFromCorner + new Vector3(0, 0.5f, 0);
        Vector3 half = new Vector3(gridSize * 0.45f, 0.5f, gridSize * 0.45f);
        Collider[] cols = Physics.OverlapBox(cellCenter, half);
        foreach (var c in cols)
        {
            var po = c ? c.GetComponentInParent<PlaceableInteraction>() : null;
            if (po == null) continue;

            var otherItem = po.SourceItem;
            // if (otherItem == null)
            //     continue; // 소스 아이템이 없으면 비교 불가이므로 스킵 (혹은 배치 불가로 처리)

            // 1) 스택 불가 아이템이면 해당 위치 차단
            if (!otherItem.IsStack)
            {
                canPlace = false;
                return true; // 위치는 있지만 배치 불가(미리보기는 빨간색)
            }
            
            // 2) IsStack = true이지만 두개 이상은 차단
            if (otherItem.Role == _currentItem.Role)
            {
                canPlace = false;
                return true; 
            }
        }
        
        // foreach (var c in cols)
        // {
        //     var po = c.GetComponentInParent<PlaceableObject>();
        //     if (po != null && po.SourceItem != null && !po.SourceItem.IsStack)
        //     {
        //         canPlace = false;
        //         return true; // 위치는 있지만 배치 불가(미리보기는 빨간색)
        //     }
        //
        //     if (po.SourceItem.Role == _currentItem.Role)
        //     {
        //         canPlace = false;
        //         return true; // 위치는 있지만 배치 불가(미리보기는 빨간색)
        //     }
        // }

        canPlace = true;
        return true;
    }
    
    // groundArea 내부 포함 판정: ClosestPoint가 자기 자신이면 내부(또는 경계)로 간주
    private bool PointInsideGround(Vector3 p)
    {
        if (!groundArea) return true; // 미지정이면 통과
        // 경계면 부동소수 오차 완화용으로 살짝 위로
        p += Vector3.up * 0.01f;
        Vector3 cp = groundArea.ClosestPoint(p);
        return (cp - p).sqrMagnitude < 1e-6f;
    }

    // ---------------- Preview ----------------
    private void CreatePreview()
    {
        if (_previewObj) Destroy(_previewObj);
        if (_currentItem == null || _currentItem.Prefab == null) return;

        _previewObj = Instantiate(_currentItem.Prefab);
        _previewObj.name = "[Preview] " + _currentItem.DisplayName;

        ApplyMaterial(previewMaterial);
        CalculateOffsetFromCorner();  // "좌하단 corner에 맞춘 오프셋" 계산
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
    public void SelectCatalogBuilderID(int builderId)
    {
        Debug.Log($"SelectCatalogIndex: {builderId}");
        if (catalog == null || catalog.GetByBuilderId(builderId) == null) return;
        _currentItem = catalog.GetByBuilderId(builderId);
        CreatePreview();
    }
    
    public void SelectCatalogIndex(int index)
    {
        Debug.Log($"SelectCatalogIndex: {index}");
        if (catalog == null || catalog.Items == null) return;
        if (index < 0 || index >= catalog.Items.Length) return;
        _currentItem = catalog.Items[index];
        CreatePreview();
    }

    // ---------------- Place ----------------
    private void Place(Vector3 pos, bool save = false)
    {
        if (_currentItem == null || _currentItem.Prefab == null) return;

        GameObject go = Instantiate(_currentItem.Prefab, pos, Quaternion.identity);
        var obj = go.AddComponent<PlaceableInteraction>();
        
        //Debug.Log($"Placed {_currentItem.DisplayName} at {_currentItem.BuilderId}");
        
        obj.Initialize(_currentItem.Role, _currentItem, pos, save);
    }
    
    // 디버그: OverlapBox 시각화
    private void OnDrawGizmosSelected()
    {
        if (_previewObj == null) return;
        Vector3 cellCenter = _previewObj.transform.position + new Vector3(0, 0.5f, 0);
        Vector3 half = new Vector3(gridSize * 0.45f, 0.5f, gridSize * 0.45f);
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(cellCenter, half * 2f);
    }
    
    public void RebuildFromSave(IReadOnlyList<PlacedObjectData> datas)
    {
        // 기존 배치 오브젝트 싹 지우고 시작하려면 여기서 정리
        // ex) 현재 필드에 있는 PlaceableObject.FindObjectsOfType<PlaceableObject>() 제거

        //Debug.Log($"저장된 오브젝트 수량 ::{datas.Count}");
        
        foreach (var data in datas)
        {
            //Debug.Log($"data.id:: {data.id}");
            // role로 PlaceableItem 찾기
            var item = catalog.GetByBuilderId(data.id);
            if (item == null || item.Prefab == null)
                continue;

            // gridX, gridZ는 이미 월드 좌표로 저장돼 있다고 가정
            Vector3 worldPos = new Vector3(data.gridX, 0, data.gridZ);
            Quaternion rot = Quaternion.Euler(0f, data.rotationY, 0f);
            
            _currentItem = item;
            Place(worldPos);
        }

        _currentItem = null;
    }
}
