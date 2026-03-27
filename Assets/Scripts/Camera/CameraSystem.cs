using Unity.Cinemachine;
using UnityEngine;

public class CameraSystem : SingletonBase<CameraSystem>
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private CinemachineCamera tpsCamera;
    [SerializeField] private CinemachineCamera freeCamera;

    [HideInInspector] public bool isFreeLook = false;

    public override void Init()
    {
        base.Init();
        inputHandler = GetComponent<PlayerInputHandler>();
        inputHandler.OnFreeLook += ToggleFreeLook;
    }

    public void ToggleFreeLook()
    {
        isFreeLook = !isFreeLook;
        tpsCamera.gameObject.SetActive(!isFreeLook);
        freeCamera.gameObject.SetActive(isFreeLook);
    }
}
