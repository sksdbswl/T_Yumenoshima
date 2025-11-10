using System;
using Unity.Cinemachine;
using UnityEngine;

 [RequireComponent(typeof(CinemachineCamera))]
public class AnimalCrossingCameraCM3 : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CinemachineCamera cmCamera;              // 이 스크립트를 붙인 CM3 카메라
    [SerializeField] private Transform trackingTarget;                // 인스펙터에서 AC_Camera의 Tracking Target과 동일하게 지정

    [Header("Angles")]
    [SerializeField, Range(15f, 70f)] private float pitch = 40f;      // 고정 피치(동물의 숲 느낌 35~50)
    [SerializeField] private float yaw = 0f;                          // 현재 yaw
    [SerializeField] private float yawSnapStep = 45f;                 // Q/E로 회전 스냅 각도
    [SerializeField] private float yawLerpSpeed = 7f;                 // 부드러운 회전 속도

    [Header("Distance/Height")]
    [SerializeField] private float height = 6f;                       // 카메라 높이
    [SerializeField] private float distance = 10f;                    // 타깃으로부터의 거리
    [SerializeField] private Vector2 distanceRange = new(6f, 14f);    // 줌 범위
    [SerializeField] private float zoomStep = 1.5f;                   // 줌 증감량

    [Header("Position Control (ThirdPersonFollow)")]
    [SerializeField] private float dampingX = 1.0f;
    [SerializeField] private float dampingY = 1.5f;
    [SerializeField] private float dampingZ = 1.0f;

    [Header("Input Keys (예시)")]
    [SerializeField] private KeyCode yawLeftKey  = KeyCode.Q;
    [SerializeField] private KeyCode yawRightKey = KeyCode.E;
    [SerializeField] private KeyCode zoomInKey   = KeyCode.Z;
    [SerializeField] private KeyCode zoomOutKey  = KeyCode.X;

    private CinemachineThirdPersonFollow _tpf;                        // CM3 Position Control
    private float _targetYaw;

    private void Reset()
    {
        cmCamera = GetComponent<CinemachineCamera>();
    }

    private void Awake()
    {
        if (!cmCamera) cmCamera = GetComponent<CinemachineCamera>();

        // CM3: Position Control 컴포넌트 가져오기
        _tpf = cmCamera != null ? cmCamera.GetComponent<CinemachineThirdPersonFollow>() : null;
        if (_tpf == null)
        {
            Debug.LogError("CinemachineThirdPersonFollow(Position Control)을 이 카메라에 추가하세요.");
            enabled = false;
            return;
        }

        // 초기 세팅
        _tpf.Damping = new Vector3(dampingX, dampingY, dampingZ);
        _tpf.CameraDistance = distance;         // CM3: 거리
        _tpf.ShoulderOffset = Vector3.zero;     // 기본 0 (필요하면 사용)

        _targetYaw = yaw;

        // Pitch/Yaw를 즉시 반영
        ApplyRotationImmediate();
        ApplyDistance();
    }

    private void Update()
    {
        if (!cmCamera) return;

        // 1) yaw 스냅 입력
        if (Input.GetKeyDown(yawLeftKey))  _targetYaw -= yawSnapStep;
        if (Input.GetKeyDown(yawRightKey)) _targetYaw += yawSnapStep;

        // 2) 줌
        if (Input.GetKeyDown(zoomInKey))
            distance = Mathf.Max(distanceRange.x, distance - zoomStep);
        if (Input.GetKeyDown(zoomOutKey))
            distance = Mathf.Min(distanceRange.y, distance + zoomStep);

        // 3) 부드러운 yaw 보간
        yaw = Mathf.LerpAngle(yaw, _targetYaw, 1f - Mathf.Exp(-yawLerpSpeed * Time.deltaTime));

        // 4) 회전/거리 반영
        ApplyRotation();   // Rotation Control = Do Nothing 이므로 카메라 Transform을 직접 회전
        ApplyDistance();   // ThirdPersonFollow의 거리 업데이트
    }

    private void ApplyRotationImmediate()
    {
        yaw = _targetYaw;
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        // 카메라의 최종 회전 = 고정 pitch + 가변 yaw
        // Rotation Control이 Do Nothing이므로 cmCamera.transform.rotation을 직접 제어
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        cmCamera.transform.rotation = rot;

        // 높이(월드 업 방향)도 반영
        // CM3 ThirdPersonFollow는 타깃 월드 위치를 기준으로 distance만큼 뒤로 배치.
        // 여기선 높이 분리를 위해 타깃 위치 자체를 올려보기보다,
        // 타깃은 그대로 두고 카메라 회전 + 거리만 사용하고,
        // 필요하면 ShoulderOffset.y를 쓸 수도 있음(여기선 rot로 처리하니 생략)
    }

    private void ApplyDistance()
    {
        if (_tpf == null) return;
        _tpf.CameraDistance = distance;

        // ThirdPersonFollow는 타깃 회전 영향을 받지 않고(=월드 기준) distance만큼 뒤에 둡니다.
        // 타깃 루트가 -90°더라도, Rotation=DoNothing + 우리가 직접 yaw/pitch를 지정 → 안전.
    }

    // (선택) 런타임에 Tracking Target을 바꾸고 싶다면
    public void SetTrackingTarget(Transform target)
    {
        // CM3에선 보통 인스펙터에서 Tracking Target을 지정합니다.
        // 코드로 접근하는 속성/필드는 버전별 차이가 있어 여기선 안전하게 '참조만' 유지합니다.
        trackingTarget = target;
        // 인스펙터의 Tracking Target은 그대로 유지하되,
        // 필요한 경우 CameraTarget 빈 오브젝트를 target의 자식으로 두고 그 Transform을 Tracking Target으로 사용하세요.
    }
}


// namespace REIW
// {
//     public class CameraMovement : MonoBehaviour
//     {
//         public enum CameraType { Tps, Fps }
//
//         [Header("Cameras")]
//         [SerializeField] private CinemachineCamera tpsCamera;
//         [SerializeField] private CinemachineCamera fpsCamera;
//
//         [Header("Follow Target")]
//         [SerializeField] private Transform followTarget;
//         [SerializeField] private Rigidbody targetRigidbody; // 있으면 사용
//         [SerializeField] private MonoBehaviour velocityProvider; // (선택) LocalCharacter 같은 컴포넌트
//         // velocityProvider가 LocalCharacter라면 public Vector3 CurrentMoveVelocity 를 반영
//
//         [Header("Damping Settings")]
//         [Tooltip("정지 상태에서의 기본 댐핑")]
//         [SerializeField] private Vector3 originDamping = new(0.2f, 0.2f, 0.2f);
//
//         [Tooltip("이동 중 적용할 댐핑(가속 시 흔들림/추종감 조절)")]
//         [SerializeField] private Vector3 movingDamping = new(0.5f, 0.5f, 0.5f);
//
//         [Tooltip("댐핑 보간 속도(일반 프레임)")]
//         [SerializeField] private float dampingLerpSpeed = 10f;
//
//         [Tooltip("멈추는 '순간'에 원래 댐핑으로 돌아가는 빠른 보간 속도")]
//         [SerializeField] private float stopSnapLerpSpeed = 18f;
//
//         [Header("Speed Thresholds")]
//         [Tooltip("이 속도 초과면 '이동 중'으로 판단")]
//         [SerializeField] private float movingSpeedThreshold = 0.15f;
//
//         [Tooltip("속도 정규화의 최대치(이 이상은 같은 취급)")]
//         [SerializeField] private float maxConsideredSpeed = 6f;
//
//         [Header("Input")]
//         [Tooltip("카메라 전환 키(기본: 휠 클릭)")]
//         [SerializeField] private KeyCode toggleKey = KeyCode.Mouse2;
//
//         public CameraType ActiveCameraType
//         {
//             get => _active;
//             set
//             {
//                 if (_active == value) return;
//                 SetActiveCamera(value);
//             }
//         }
//
//         private CameraType _active = CameraType.Tps;
//         private CinemachineThirdPersonFollow _tpsFollow;
//         private Vector3 _lastPos;
//         private bool _hadLastPos;
//         private bool _wasMoving;
//
//         private void Awake()
//         {
//             if (tpsCamera != null)
//                 _tpsFollow = tpsCamera.GetComponent<CinemachineThirdPersonFollow>();
//
//             if (followTarget != null)
//                 SetFollowTarget(followTarget);
//
//             SetActiveCamera(_active);
//
//             if (_tpsFollow != null)
//                 _tpsFollow.Damping = originDamping;
//         }
//
//         private void Update()
//         {
//             // 마우스 전환
//             if (Input.GetKeyDown(toggleKey))
//                 ToggleCameraView();
//
//             // 속도 측정
//             float speed = GetTargetSpeed();
//
//             bool isMoving = speed > movingSpeedThreshold;
//
//             // 이동 → 멈춤 전이 시점: 스냅처럼 빠르게 원래 댐핑으로 복귀
//             float lerpSpeed = (!_wasMoving && !isMoving) ? dampingLerpSpeed
//                             : (_wasMoving && !isMoving) ? stopSnapLerpSpeed
//                             : dampingLerpSpeed;
//
//             // TPS에서만 댐핑 조절(필요하면 FPS에도 동일 로직 넣어도 됨)
//             if (_tpsFollow != null && _active == CameraType.Tps)
//             {
//                 // 이동 중에는 movingDamping 쪽으로, 정지 중에는 originDamping 쪽으로
//                 Vector3 targetDamping = isMoving ? movingDamping : originDamping;
//
//                 _tpsFollow.Damping.x = Mathf.Lerp(_tpsFollow.Damping.x, targetDamping.x, Time.deltaTime * lerpSpeed);
//                 _tpsFollow.Damping.y = Mathf.Lerp(_tpsFollow.Damping.y, targetDamping.y, Time.deltaTime * lerpSpeed);
//                 _tpsFollow.Damping.z = Mathf.Lerp(_tpsFollow.Damping.z, targetDamping.z, Time.deltaTime * lerpSpeed);
//             }
//
//             _wasMoving = isMoving;
//         }
//
//         // ─────────────────────────────────────────────────────────────────────
//
//         public void SetFollowTarget(Transform target)
//         {
//             followTarget = target;
//
//             if (tpsCamera != null) tpsCamera.Follow = target;
//             if (fpsCamera != null) fpsCamera.Follow = target;
//
//             // Rigidbody는 직접 할당하거나, 자동으로 찾아봄
//             if (targetRigidbody == null && target != null)
//                 targetRigidbody = target.GetComponentInParent<Rigidbody>();
//
//             _hadLastPos = false; // 위치 델타 추정 초기화
//         }
//
//         public void ToggleCameraView()
//         {
//             ActiveCameraType = (_active == CameraType.Tps) ? CameraType.Fps : CameraType.Tps;
//         }
//
//         private void SetActiveCamera(CameraType type)
//         {
//             _active = type;
//
//             if (tpsCamera != null) tpsCamera.gameObject.SetActive(_active == CameraType.Tps);
//             if (fpsCamera != null) fpsCamera.gameObject.SetActive(_active == CameraType.Fps);
//         }
//
//         private float GetTargetSpeed()
//         {
//             // 1) Rigidbody가 있으면 그 속도 사용
//             if (targetRigidbody != null)
//                 return Mathf.Min(targetRigidbody.velocity.magnitude, maxConsideredSpeed);
//
//             // 2) LocalCharacter 같은 커스텀 컴포넌트에서 속도를 읽고 싶다면
//             //    public Vector3 CurrentMoveVelocity 가 있다고 가정
//             if (velocityProvider != null)
//             {
//                 var type = velocityProvider.GetType();
//                 var prop = type.GetProperty("CurrentMoveVelocity");
//                 if (prop != null && prop.PropertyType == typeof(Vector3))
//                 {
//                     Vector3 v = (Vector3)prop.GetValue(velocityProvider);
//                     return Mathf.Min(v.magnitude, maxConsideredSpeed);
//                 }
//             }
//
//             // 3) fall-back: 위치 델타로 속도 추정
//             if (followTarget == null) return 0f;
//
//             if (!_hadLastPos)
//             {
//                 _lastPos = followTarget.position;
//                 _hadLastPos = true;
//                 return 0f;
//             }
//
//             Vector3 delta = followTarget.position - _lastPos;
//             _lastPos = followTarget.position;
//
//             float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
//             return Mathf.Min(speed, maxConsideredSpeed);
//         }
//     }
// }
