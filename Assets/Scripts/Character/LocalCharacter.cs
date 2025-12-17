using UnityEngine;

namespace REIW
{
    /// <summary>
    /// 로컬 입력 → 카메라 기준 이동/시선 계산만
    /// cameraTarget (카메라가 따라갈 트랜스폼)
    ///입력을 넣는 API: SetInputs(...), ResetInputs()
    ///핵심 로직:
    ///GetCameraPlanarRotation() : 카메라 Forward를 평면(XZ) 기준 회전으로 변환
    ///HandleCharacterInput() : Move 입력을 카메라 기준으로 바꿔 characterMoveDir 생성
    ///PostCameraUpdate() : 카메라 업데이트 후 시선/룩 방향을 정리
    ///더미 이동값은 “속도=moveDir”로 단순화 (currentMoveVelocity = characterMoveDir)
    ///    → 물리 이동이 아닌 “값만 존재”하게 한 겁니다.
    ///    즉, 이 파일은 “로컬 캐릭터의 입력/방향 데이터 생성기 역할만
    /// </summary>
    public class LocalCharacter : CharacterBase
    {
        public static LocalCharacter Instance { get; private set; }
        public static Transform TransformCache { get; private set; }

        public override bool IsLocalCharacter => true;

        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;
        public Transform CameraTarget => cameraTarget;

        private PlayerCharacterInputs curInputs = new();
        public override PlayerCharacterInputs CurrentInputs => curInputs;

        private Vector3 lastLookDirection = Vector3.forward;

        private void Awake()
        {
            Instance = this;
            TransformCache = transform;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (lastLookDirection == Vector3.zero)
                lastLookDirection = transform.forward;
        }

        public void SetInputs(PlayerCharacterInputs newInput)
        {
            if (newInput == null) return;
            curInputs = newInput;
        }

        public void ResetInputs()
        {
            curInputs.Move = Vector2.zero;
            curInputs.Look = Vector2.zero;
            curInputs.Jump = false;
            curInputs.JumpHold = false;
            curInputs.Parkour = false;
            curInputs.Walk = false;
            curInputs.Sprint = false;
            curInputs.Dash = false;
            curInputs.Mount = false;
            curInputs.WallClimb = false;

            lastLookDirection = transform.forward;
        }

        private void Update()
        {
            HandleCharacterInput();
        }

        private Quaternion GetCameraPlanarRotation()
        {
            if (cameraTarget == null)
                return Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Up).normalized, Up);

            Vector3 fwd = Vector3.ProjectOnPlane(cameraTarget.rotation * Vector3.forward, Up).normalized;
            if (fwd.sqrMagnitude == 0f)
                fwd = Vector3.ProjectOnPlane(cameraTarget.rotation * Vector3.up, Up).normalized;

            return Quaternion.LookRotation(fwd, Up);
        }

        private void HandleCharacterInput()
        {
            var playerMoveInput = new Vector3(curInputs.Move.x, 0, curInputs.Move.y);
            playerMoveInput = Vector3.ClampMagnitude(playerMoveInput, 1f);

            var cameraPlanarRotation = GetCameraPlanarRotation();
            characterMoveDir = cameraPlanarRotation * playerMoveInput;

            if (!LockMoveInput && playerMoveInput.sqrMagnitude > 0.0001f)
            {
                lastLookDirection = (cameraPlanarRotation * playerMoveInput.normalized);
                EventBus.Post<ICharacterBaseEventListener>(_ => _.OnMoveStarted());
            }

            CharacterLookDir = (lastLookDirection.sqrMagnitude > 0.0001f) ? lastLookDirection : transform.forward;

            // Dummy velocity: replace with your own movement logic if needed.
            currentMoveVelocity = characterMoveDir;
        }

        public void PostCameraUpdate()
        {
            var cameraPlanarRotation = GetCameraPlanarRotation();
            var moveInput = new Vector3(curInputs.Move.x, 0, curInputs.Move.y);

            if (!LockMoveInput && moveInput.sqrMagnitude > 0.0004f)
                lastLookDirection = (cameraPlanarRotation * moveInput.normalized);

            CharacterLookDir = (lastLookDirection.sqrMagnitude > 0.0001f) ? lastLookDirection : transform.forward;
        }
    }
}
