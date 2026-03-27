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
    [SerializeField] private float edgeThreshold = 10f;

    private void Update()
    {
        if (!isFreeLook) return;
        HandleEdgeScroll();
    }

    public override void Init()
    {
        base.Init();
        inputHandler.OnTab      += ToggleFreeLook;
        inputHandler.OnFreeLook += HandleFreeLookMouse; // 우클릭 구독
    }

    private void OnDisable()
    {
        inputHandler.OnTab      -= ToggleFreeLook;
        inputHandler.OnFreeLook -= HandleFreeLookMouse;
    }

    public void ToggleFreeLook()
    {
        isFreeLook = !isFreeLook;
        tpsCamera.Priority  = isFreeLook ? 0 : 20;
        freeCamera.Priority = isFreeLook ? 20 : 0;

        // 프리캠 진입 시 플레이어 위치에서 시작
        if (isFreeLook)
        {
            freeCamera.transform.position = new Vector3(
                player.position.x,
                freeCamera.transform.position.y,
                player.position.z
            );
        }
    }

    // 우클릭 - 필요시 추가 로직
    private void HandleFreeLookMouse()
    {
        if (!isFreeLook) return;
        // 현재는 EdgeScroll이 메인이라 우클릭은 비워둬도 됨
        // 필요하면 여기서 드래그 이동 처리
    }

    // V키 - 플레이어 위치로 복귀
    public void ReturnToPlayer()
    {
        if (!isFreeLook) return;
        freeCamera.transform.position = new Vector3(
            player.position.x,
            freeCamera.transform.position.y,
            player.position.z
        );
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

        Vector3 forward = freeCamera.transform.forward;
        Vector3 right   = freeCamera.transform.right;
        forward.y = 0f;
        right.y   = 0f;

        Vector3 move = (forward * moveDir.z + right * moveDir.x).normalized;
        freeCamera.transform.position += move * moveSpeed * Time.deltaTime; 
    }
}