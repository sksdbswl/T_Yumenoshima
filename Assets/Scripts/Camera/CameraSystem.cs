using Unity.Cinemachine;
using UnityEngine;

public class CameraSystem : SingletonBase<CameraSystem>
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private CinemachineCamera tpsCamera;
    [SerializeField] private CinemachineCamera freeCamera;

    [HideInInspector] public bool isFreeLook = false;
    
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 30f;
    [SerializeField] private float edgeThreshold = 10f; // 화면 끝 감지 픽셀
    
    private void Update()
    {
        if (!isFreeLook) return;
        HandleEdgeScroll();
    }
    
    public override void Init()
    {
        base.Init();
        inputHandler.OnFreeLook += ToggleFreeLook;
    }
    
    private void OnDisable()
    {
        inputHandler.OnFreeLook -= ToggleFreeLook; 
    }

    public void ToggleFreeLook()
    {
        isFreeLook = !isFreeLook;
        tpsCamera.gameObject.SetActive(!isFreeLook);
        freeCamera.gameObject.SetActive(isFreeLook);
    }
    
    private void HandleEdgeScroll()
    {
        Vector3 moveDir = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        // 화면 끝 감지
        if (mousePos.x < edgeThreshold)
            moveDir.x = -1f; // 왼쪽
        else if (mousePos.x > Screen.width - edgeThreshold)
            moveDir.x = 1f;  // 오른쪽

        if (mousePos.y < edgeThreshold)
            moveDir.z = -1f; // 아래
        else if (mousePos.y > Screen.height - edgeThreshold)
            moveDir.z = 1f;  // 위

        // 카메라 방향 기준으로 이동 (쿼터뷰 각도 유지)
        Vector3 forward = freeCamera.transform.forward;
        Vector3 right = freeCamera.transform.right;
        forward.y = 0f;
        right.y = 0f;

        Vector3 move = (forward * moveDir.z + right * moveDir.x).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
