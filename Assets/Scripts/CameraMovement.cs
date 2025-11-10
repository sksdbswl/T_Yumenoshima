using System;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace REIW
{
    public class CameraMovement : MonoBehaviour
    {
        public enum CameraType { Tps, Fps }

        [Header("Cameras")]
        [SerializeField] private CinemachineCamera tpsCamera;
        [SerializeField] private CinemachineCamera fpsCamera;

        [Header("Follow Target")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Rigidbody targetRigidbody; // 있으면 사용
        [SerializeField] private MonoBehaviour velocityProvider; // (선택) LocalCharacter 같은 컴포넌트
        // velocityProvider가 LocalCharacter라면 public Vector3 CurrentMoveVelocity 를 반영

        [Header("Damping Settings")]
        [Tooltip("정지 상태에서의 기본 댐핑")]
        [SerializeField] private Vector3 originDamping = new(0.2f, 0.2f, 0.2f);

        [Tooltip("이동 중 적용할 댐핑(가속 시 흔들림/추종감 조절)")]
        [SerializeField] private Vector3 movingDamping = new(0.5f, 0.5f, 0.5f);

        [Tooltip("댐핑 보간 속도(일반 프레임)")]
        [SerializeField] private float dampingLerpSpeed = 10f;

        [Tooltip("멈추는 '순간'에 원래 댐핑으로 돌아가는 빠른 보간 속도")]
        [SerializeField] private float stopSnapLerpSpeed = 18f;

        [Header("Speed Thresholds")]
        [Tooltip("이 속도 초과면 '이동 중'으로 판단")]
        [SerializeField] private float movingSpeedThreshold = 0.15f;

        [Tooltip("속도 정규화의 최대치(이 이상은 같은 취급)")]
        [SerializeField] private float maxConsideredSpeed = 6f;

        [Header("Input")]
        [Tooltip("카메라 전환 키(기본: 휠 클릭)")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Mouse2;

        public CameraType ActiveCameraType
        {
            get => _active;
            set
            {
                if (_active == value) return;
                SetActiveCamera(value);
            }
        }

        private CameraType _active = CameraType.Tps;
        private CinemachineThirdPersonFollow _tpsFollow;
        private Vector3 _lastPos;
        private bool _hadLastPos;
        private bool _wasMoving;

        private void Awake()
        {
            if (tpsCamera != null)
                _tpsFollow = tpsCamera.GetComponent<CinemachineThirdPersonFollow>();

            if (followTarget != null)
                SetFollowTarget(followTarget);

            SetActiveCamera(_active);

            if (_tpsFollow != null)
                _tpsFollow.Damping = originDamping;
        }

        private void Update()
        {
            // 마우스 전환
            if (Input.GetKeyDown(toggleKey))
                ToggleCameraView();

            // 속도 측정
            float speed = GetTargetSpeed();

            bool isMoving = speed > movingSpeedThreshold;

            // 이동 → 멈춤 전이 시점: 스냅처럼 빠르게 원래 댐핑으로 복귀
            float lerpSpeed = (!_wasMoving && !isMoving) ? dampingLerpSpeed
                            : (_wasMoving && !isMoving) ? stopSnapLerpSpeed
                            : dampingLerpSpeed;

            // TPS에서만 댐핑 조절(필요하면 FPS에도 동일 로직 넣어도 됨)
            if (_tpsFollow != null && _active == CameraType.Tps)
            {
                // 이동 중에는 movingDamping 쪽으로, 정지 중에는 originDamping 쪽으로
                Vector3 targetDamping = isMoving ? movingDamping : originDamping;

                _tpsFollow.Damping.x = Mathf.Lerp(_tpsFollow.Damping.x, targetDamping.x, Time.deltaTime * lerpSpeed);
                _tpsFollow.Damping.y = Mathf.Lerp(_tpsFollow.Damping.y, targetDamping.y, Time.deltaTime * lerpSpeed);
                _tpsFollow.Damping.z = Mathf.Lerp(_tpsFollow.Damping.z, targetDamping.z, Time.deltaTime * lerpSpeed);
            }

            _wasMoving = isMoving;
        }

        // ─────────────────────────────────────────────────────────────────────

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;

            if (tpsCamera != null) tpsCamera.Follow = target;
            if (fpsCamera != null) fpsCamera.Follow = target;

            // Rigidbody는 직접 할당하거나, 자동으로 찾아봄
            if (targetRigidbody == null && target != null)
                targetRigidbody = target.GetComponentInParent<Rigidbody>();

            _hadLastPos = false; // 위치 델타 추정 초기화
        }

        public void ToggleCameraView()
        {
            ActiveCameraType = (_active == CameraType.Tps) ? CameraType.Fps : CameraType.Tps;
        }

        private void SetActiveCamera(CameraType type)
        {
            _active = type;

            if (tpsCamera != null) tpsCamera.gameObject.SetActive(_active == CameraType.Tps);
            if (fpsCamera != null) fpsCamera.gameObject.SetActive(_active == CameraType.Fps);
        }

        private float GetTargetSpeed()
        {
            // 1) Rigidbody가 있으면 그 속도 사용
            if (targetRigidbody != null)
                return Mathf.Min(targetRigidbody.linearVelocity.magnitude, maxConsideredSpeed);

            // 2) LocalCharacter 같은 커스텀 컴포넌트에서 속도를 읽고 싶다면
            //    public Vector3 CurrentMoveVelocity 가 있다고 가정
            if (velocityProvider != null)
            {
                var type = velocityProvider.GetType();
                var prop = type.GetProperty("CurrentMoveVelocity");
                if (prop != null && prop.PropertyType == typeof(Vector3))
                {
                    Vector3 v = (Vector3)prop.GetValue(velocityProvider);
                    return Mathf.Min(v.magnitude, maxConsideredSpeed);
                }
            }

            // 3) fall-back: 위치 델타로 속도 추정
            if (followTarget == null) return 0f;

            if (!_hadLastPos)
            {
                _lastPos = followTarget.position;
                _hadLastPos = true;
                return 0f;
            }

            Vector3 delta = followTarget.position - _lastPos;
            _lastPos = followTarget.position;

            float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            return Mathf.Min(speed, maxConsideredSpeed);
        }
    }
}
