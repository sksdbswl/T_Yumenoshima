
using Unity.Cinemachine;
using UnityEngine;

public class CameraSystem : SingletonBase<CameraSystem>
{
    [SerializeField] private Transform player;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private CinemachineCamera tpsCamera;
    [SerializeField] private CinemachineCamera freeCamera;

    [HideInInspector] public bool isFreeLook = false;
    
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 30f;
    [SerializeField] private float edgeThreshold = 10f; // 화면 끝 감지 픽셀
    
    private Vector3 lastMousePos;
    private bool isDragging = false;

    public override void Init()
    {
        base.Init();
        inputHandler.OnTab += ToggleFreeLook;
        //inputHandler.OnFreeLook += HandleEdgeScroll;
    }
    
    private void OnDisable()
    {
        inputHandler.OnTab -= ToggleFreeLook;
        //inputHandler.OnFreeLook += HandleEdgeScroll;
    }
    
    public void ToggleFreeLook()
    {
        isFreeLook = !isFreeLook;
        tpsCamera.gameObject.SetActive(!isFreeLook);
        freeCamera.gameObject.SetActive(isFreeLook);
    }
    
    private void Update()
    {
        if (!isFreeLook) return;

        if (Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
            isDragging = false;

        HandleMouseDrag(); 
        HandleEdgeScroll();
    }

    private void HandleMouseDrag()
    {
        if (!isDragging) return;

        Vector3 delta = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        Vector3 moveDir = new Vector3(-delta.x, 0f, -delta.y);
        MoveCamera(moveDir.normalized * (delta.magnitude * 0.1f));
    }

    private void MoveCamera(Vector3 moveDir)
    {
        // 카메라 forward 대신 월드 기준 앞뒤 사용
        Vector3 forward = new Vector3(0f, 0f, 1f); // 월드 Z축 고정
        Vector3 right   = new Vector3(1f, 0f, 0f); // 월드 X축 고정

        Vector3 move = forward * moveDir.z + right * moveDir.x;
        freeCamera.transform.position += move * moveSpeed * Time.deltaTime;
    }

    private void HandleEdgeScroll()
    {
        Vector3 moveDir = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x < edgeThreshold)
            moveDir.x = -1f;
        else if (mousePos.x > Screen.width - edgeThreshold)
            moveDir.x = 1f;

        if (mousePos.y < edgeThreshold)
            moveDir.z = -1f;
        else if (mousePos.y > Screen.height - edgeThreshold)
            moveDir.z = 1f;

        if (moveDir == Vector3.zero) return;

        MoveCamera(moveDir);
    }
    
    // private void MoveCamera(Vector3 moveDir)
    // {
    //     Vector3 forward = freeCamera.transform.forward;
    //     Vector3 right   = freeCamera.transform.right;
    //     forward.y = 0f;
    //     right.y   = 0f;
    //     forward.Normalize();
    //     right.Normalize();
    //
    //     Vector3 move = forward * moveDir.z + right * moveDir.x;
    //     freeCamera.transform.position += move * moveSpeed * Time.deltaTime;
    // }
}
