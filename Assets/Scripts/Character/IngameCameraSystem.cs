using System;
using System.Collections.Generic;
using REIW;
using Unity.Cinemachine;
using UnityEngine;


/// <summary>
/// Cinemachine 기반 카메라 구성(Brain / vcam 참조 / TPS 시점 토글 등)
/// IMoveWallClimbEventListener, IMoveParkourEventListener 이벤트에 반응하는 카메라 연출 포인트
/// (예: 중력 변화 시 카메라 오프셋/회전/댐핑을 다르게 주는 훅)
/// MainCamera, Brain 등 “씬에서 찾을 수 있게” getter 캐시 유지
/// </summary>
public class IngameCameraSystem : MonoBehaviour,
    IMoveWallClimbEventListener,
    IMoveParkourEventListener
{
    public static IngameCameraSystem Instance
    {
        get
        {
            if (_instance) return _instance;
            _instance = FindAnyObjectByType<IngameCameraSystem>();
            return _instance;
        }
    }
    private static IngameCameraSystem _instance;

    public enum CameraType
    {
        TpsCamera, FpsCamera, MenuCamera, MountTpsCamera, MountFpsCamera,
        BR_TpsCamera
    }

    public CameraType ActiveCameraType
    {
        get => _activeCameraType;
        private set
        {
            if (_prevCameraType != value)
                _prevCameraType = _activeCameraType;

            if (cameraDic.TryGetValue(_activeCameraType, out var curGo) && _activeCameraType != CameraType.TpsCamera)
                curGo.SetActive(false);

            _activeCameraType = value;

            if (cameraDic.TryGetValue(_activeCameraType, out var newGo))
                newGo.SetActive(true);
        }
    }
    private CameraType _activeCameraType = CameraType.TpsCamera;

    public Transform FollowTarget
    {
        get => TpsCamera.Follow;
        set
        {
            if (value == null) return;

            tpsCamera.Follow = value;
            fpsCamera.Follow = value;
            menuCamera.Follow = value.root;
            mountTpsCamera.Follow = value;
            mountFpsCamera.Follow = value;

            SetFollowLocalCharacter();
        }
    }

    public Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
                _mainCamera = GetComponentInChildren<Camera>();
            return _mainCamera;
        }
    }

    public CinemachineBrain Brain
    {
        get
        {
            if (_brain == null)
                _brain = GetComponentInChildren<CinemachineBrain>();
            return _brain;
        }
    }

    public CinemachineCamera TpsCamera
    {
        get
        {
            return ActiveCameraType switch
            {
                CameraType.MountTpsCamera => mountTpsCamera,
                CameraType.TpsCamera => tpsCamera,
                _ => tpsCamera
            };
        }
    }

    public CinemachineThirdPersonFollow ThirdPersonFollow => thirdPersonFollow;
    public CinemachineDecollider Decollider => decollider;

    public Func<float, Vector3?> UpdateInterpolateCameraShoulderOffset { get; set; }

    public Vector3 ShoulderOffset => ThirdPersonFollow.ShoulderOffset;

    private Vector3 baseShoulderOffset;
    private Vector3 targetShoulderOffset;
    private float changeShoulderOffsetSharpness;
    private REIW.LocalCharacter targetCharacter;
    private Vector3 originDamping;
    private float baseMountTpsCameraLensFieldOfView;

    [Header("Player Camera")]
    [SerializeField] private CinemachineCamera tpsCamera;
    [SerializeField] private CinemachineCamera fpsCamera;
    [SerializeField] private CinemachineCamera menuCamera;

    [Header("TPS Camera Config")]
    [SerializeField] private Vector3 onGravityChangeCamOffset = new (0, -.3f, .7f); // 중력 변환 중 카메라 오프셋 변경값
    [SerializeField] private float gravityChangeCamInSharpness = 8f;                // 중력 변환 중 카메라 오프셋 변경 속도
    [SerializeField] private float gravityChangeCamOutSharpness = 2f;               // 중력 변환 중 카메라 오프셋 복구 속도

    [Header("Velocity Damping (optional)")]
    [SerializeField] private Vector3 fastVelocity;                          // 캐릭터 velocity가 빠른 상태로 판정하는 값
    [SerializeField] private Vector3 maxVelocity;                           // 빠른 기준의 velocity max 값
    [SerializeField] private Vector3 fastDamping;                           // 캐릭터 velocity가 빠른 경우 적용 damping 값
    [SerializeField] private float dampingChangeSpeed = 10f;                // damping 변경 스피드

    private CinemachineThirdPersonFollow thirdPersonFollow;
    private CinemachineDecollider decollider;

    [Header("Mount Camera")]
    [SerializeField] private CinemachineCamera mountTpsCamera;
    [SerializeField] private CinemachineCamera mountFpsCamera;

    [Header("BR Camera")]
    [SerializeField] private CinemachineCamera brTpsCamera;

    private Dictionary<CameraType, GameObject> cameraDic = new();
    private CinemachineBrain _brain;
    private Camera _mainCamera;
    private CameraType _prevCameraType;

    [Header("Cinemachine Blend Settings (optional)")]
    [SerializeField] private CinemachineBlenderSettings ingameBlendSettings;
    [SerializeField] private CinemachineBlenderSettings characterStageBlendSettings;

    private void Awake()
    {
        _instance = this;

        _mainCamera = GetComponentInChildren<Camera>();
        _brain = GetComponentInChildren<CinemachineBrain>();

        thirdPersonFollow = tpsCamera.GetComponent<CinemachineThirdPersonFollow>();
        decollider = tpsCamera.GetComponent<CinemachineDecollider>();

        SetFollowLocalCharacter();

        baseShoulderOffset = ThirdPersonFollow.ShoulderOffset;
        targetShoulderOffset = ThirdPersonFollow.ShoulderOffset;
        originDamping = ThirdPersonFollow.Damping;
        baseMountTpsCameraLensFieldOfView = mountTpsCamera.Lens.FieldOfView;

        UpdateInterpolateCameraShoulderOffset = InterpolateCameraShoulderOffset;
        changeShoulderOffsetSharpness = gravityChangeCamOutSharpness;

        _activeCameraType = CameraType.TpsCamera;

        menuCamera.gameObject.SetActive(false);
        fpsCamera.gameObject.SetActive(false);
        mountTpsCamera.gameObject.SetActive(false);
        mountFpsCamera.gameObject.SetActive(false);

        cameraDic[CameraType.TpsCamera] = tpsCamera.gameObject;
        cameraDic[CameraType.FpsCamera] = fpsCamera.gameObject;
        cameraDic[CameraType.MenuCamera] = menuCamera.gameObject;
        cameraDic[CameraType.MountTpsCamera] = mountTpsCamera.gameObject;
        cameraDic[CameraType.MountFpsCamera] = mountFpsCamera.gameObject;
        cameraDic[CameraType.BR_TpsCamera] = brTpsCamera.gameObject;
    }

    private void OnDestroy() => _instance = null;

    private void Update()
    {
        Vector3? offset = UpdateInterpolateCameraShoulderOffset?.Invoke(Time.deltaTime);
        if (offset.HasValue)
            ThirdPersonFollow.ShoulderOffset = offset.Value;
    }

    private void LateUpdate()
    {
        if (!ThirdPersonFollow || !targetCharacter)
            return;

        var velocity = targetCharacter.CurrentMoveVelocity;

        float resultDamping = CalculateDamping(velocity.x, out var damping) ? damping : originDamping.x;
        ThirdPersonFollow.Damping.x = Mathf.Lerp(ThirdPersonFollow.Damping.x, resultDamping, Time.deltaTime * dampingChangeSpeed);

        resultDamping = CalculateDamping(velocity.y, out damping) ? damping : originDamping.y;
        ThirdPersonFollow.Damping.y = Mathf.Lerp(ThirdPersonFollow.Damping.y, resultDamping, Time.deltaTime * dampingChangeSpeed);

        resultDamping = CalculateDamping(velocity.z, out damping) ? damping : originDamping.z;
        ThirdPersonFollow.Damping.z = Mathf.Lerp(ThirdPersonFollow.Damping.z, resultDamping, Time.deltaTime * dampingChangeSpeed);
    }

    public void ResetCameraStates()
    {
        mountTpsCamera.Lens.FieldOfView = baseMountTpsCameraLensFieldOfView;
    }

    private void SetFollowLocalCharacter()
    {
        if (!TpsCamera || !TpsCamera.Follow)
            return;

        targetCharacter = TpsCamera.Follow.GetComponentInParent<REIW.LocalCharacter>(true);
        if (targetCharacter == null)
            targetCharacter = TpsCamera.Follow.GetComponentInChildren<REIW.LocalCharacter>(true);
    }

    private bool CalculateDamping(float velocity, out float damping)
    {
        damping = 0f;

        var absVelocity = Mathf.Abs(velocity);
        if (fastVelocity.x < absVelocity)
        {
            float t = Mathf.InverseLerp(fastVelocity.x, maxVelocity.x, absVelocity);
            damping = Mathf.Lerp(originDamping.x, fastDamping.x, t);
            return true;
        }

        return false;
    }

    public Vector3? InterpolateCameraShoulderOffset(float deltaTime)
    {
        var t = 1f - Mathf.Exp(-changeShoulderOffsetSharpness * deltaTime);
        return Vector3.Slerp(ThirdPersonFollow.ShoulderOffset, targetShoulderOffset, t);
    }

    #region WallClimb events (used for gravity change camera offset)
    public void OnGravityChangeStarted(bool isDownSnapping)
    {
        targetShoulderOffset = onGravityChangeCamOffset;
        changeShoulderOffsetSharpness = gravityChangeCamInSharpness;
    }

    public void OnGravityChangeFinished(bool worldGravity)
    {
        targetShoulderOffset = baseShoulderOffset;
        changeShoulderOffsetSharpness = gravityChangeCamOutSharpness;
    }

    public void OnWallClimbStarted() { }
    public void OnWallClimbFinished() { }
    public void OnFailedMovementToEdge(Vector3 failPoint, Vector3 failNormal) { }
    #endregion

    #region Parkour events (kept as empty hooks)
    public void OnParkourRequested(ParkourActionData actionData, Action<bool> funcStartParkour) { }
    public void OnParkourStarted(ParkourActionData actionData, Action funcFinishedParkour) { }
    public void OnParkourFinished() { }
    #endregion

    public void ToggleCameraView()
    {
        ActiveCameraType = _activeCameraType switch
        {
            CameraType.TpsCamera => CameraType.FpsCamera,
            CameraType.FpsCamera => CameraType.TpsCamera,
            CameraType.MountFpsCamera => CameraType.MountTpsCamera,
            CameraType.MountTpsCamera => CameraType.MountFpsCamera,
            _ => ActiveCameraType
        };
    }

    public void SetForceToTpsCamera()
    {
        if (ActiveCameraType == CameraType.FpsCamera) ActiveCameraType = CameraType.TpsCamera;
        if (ActiveCameraType == CameraType.MountFpsCamera) ActiveCameraType = CameraType.MountTpsCamera;
    }

    public void SetCameraModeToMount()
    {
        if (ActiveCameraType == CameraType.FpsCamera) ActiveCameraType = CameraType.MountFpsCamera;
        if (ActiveCameraType == CameraType.TpsCamera) ActiveCameraType = CameraType.MountTpsCamera;
    }

    public void SetCameraModeToPlayer()
    {
        if (ActiveCameraType == CameraType.MountFpsCamera) ActiveCameraType = CameraType.FpsCamera;
        if (ActiveCameraType == CameraType.MountTpsCamera) ActiveCameraType = CameraType.TpsCamera;
    }

    public void SetCameraModeToPrev() => ActiveCameraType = _prevCameraType;

    public void SetBrTpsCamera(Transform cameraTarget)
    {
        ActiveCameraType = CameraType.BR_TpsCamera;
        brTpsCamera.Follow = cameraTarget;
    }

    public void SetCameraModeToMenu() => ActiveCameraType = CameraType.MenuCamera;

    public void SetCinemachineBlendToIngame() => Brain.CustomBlends = ingameBlendSettings;
    public void SetCinemachineBlendToCharacterStage() => Brain.CustomBlends = characterStageBlendSettings;
}
