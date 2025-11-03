using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FLATBUFFERS;
using REIW.Network;
using UnityEngine;
using VContainer;

namespace REIW
{
    using REIW.EventLock;
    
    public partial class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }
        
        public bool IsStandalone => GetType() == typeof(PlayerStandaloneController);

        [field: Header("Control Settings")]
        [field: SerializeField]
        public bool IsActiveControl { get; set; } = true;

        [field: SerializeField] public bool IsActiveMovement { get; set; } = true;
        [field: SerializeField] public bool IsActiveLook { get; set; } = true;


        [field: Header("Character Settings")]
        [field: SerializeField, ReadOnly]
        public LocalCharacter LinkedCharacter { get; protected set; }

        [field: SerializeField] public LocalCharacter LocalCharacterPrefab { get; private set; }

        [field: Header("Camera Settings")]
        [field: SerializeField]
        private float CameraRotationSpeed { get; set; } = .2f;

        [field: SerializeField, Range(0f, 1f)] public float CameraSensitivityX { get; set; } = .5f;
        [field: SerializeField, Range(0f, 1f)] public float CameraSensitivityY { get; set; } = .5f;
        [field: SerializeField] private bool CameraInvertYAxis { get; set; } = true;
        [field: SerializeField] private float CameraRotationSharpness { get; set; } = 15f;
        [field: SerializeField] private float CameraTopClamp { get; set; } = 60f;
        [field: SerializeField] private float CameraBottomClmap { get; set; } = -60f;

        // 캐릭터 입력, 액션 상태 관리
        public eInputCommandModeType CurrentInputCommandModeType { get; set; } = eInputCommandModeType.Character;
        public eStaminaActionType CurrentExecuteActionTypeStateType { get; set; } = eStaminaActionType.Normal;

        public Transform CurrentTarget => currentTarget;
        private Transform currentTarget;
        [field: SerializeField] public InteractionSensor InteractionSensor { get; private set; }
        public CharacterBaseEventBus EventBus => LinkedCharacter?.EventBus;
        [SerializeField, ReadOnly] public InputActionStaminaValidator InputActionStaminaValidator; // Stamina Validator

        private bool jumpRequested = false;
        private bool jumpHold = false;
        private bool parkourRequested = false;
        private bool specialActionE_Requested = false;
        private bool specialActionT_Requested = false;
        private bool isWalking = false;
        private bool isSprinting = false;
        protected bool isRidingMount = false;
        [SerializeField]
        private MouseClickChecker _mouseClickChecker = new MouseClickChecker();
        
        // Dash / Sprint 입력 상태
        private bool dashHoldRMB = false;
        private bool dashHoldShift = false;
        private bool isDashPressedThisFrame = false;  // 이번 프레임 Dash 트리거
        private float dashHoldTimer = 0f;             // 홀드 지속 시간
        private bool IsDashHolding => dashHoldRMB || dashHoldShift;
        private const float SprintRequestHurdleTime = 0.1f; // 그대로 사용하셔도 됩니다.
        
        private bool IsInputRequested =>
            InputController.Singleton.Move != Vector2.zero ||
            jumpRequested || parkourRequested || specialActionE_Requested || specialActionT_Requested || isWalking || isSprinting || isDashPressedThisFrame;

        public bool IsSprinting => isSprinting;
        public bool IsRidingMount => isRidingMount;

        private IngameCameraSystem cameraSystem;
        private Camera mainCamera;
        private Transform cameraPivot;
        private float cameraTargetPitch;
        private Vector3 cameraPlanarDir = Vector3.zero;
        
        public Vector3 CameraPlanarDirection => cameraPlanarDir;
        public delegate void UpdateEventCameraRotateDelegate(ref Quaternion rotate);
        public UpdateEventCameraRotateDelegate EventCameraRotateAction { get; set; } = null;


        private CharacterMoveWallClimb wallClimbComp;
        private CharacterMoveGrapple grappleComp;
            
            
        [Inject]
        public void Construct(IngameCameraSystem cameraSystem)
        {
            this.cameraSystem = cameraSystem;
        }
        
        protected virtual void OnDrawGizmos() { }

        public virtual void Initialize()
        {
            #region Player Initialize

            if (LocalCharacterPrefab)
            {
                LinkedCharacter = Instantiate(LocalCharacterPrefab,
                    UserDataModel.Singleton.PlayerInfoData.SpawnPosition,
                    Quaternion.LookRotation(UserDataModel.Singleton.PlayerInfoData.SpawnDirection, Vector3.up));
                LinkedCharacter.CharacterLookDir = UserDataModel.Singleton.PlayerInfoData.SpawnDirection;
            }

            if (!LinkedCharacter)
                return;
            
            OwnerPlayerNetObject component = LinkedCharacter.gameObject.GetComponent<OwnerPlayerNetObject>();
            component.SetNetID(UserDataModel.UserCategory, UserDataModel.UserKIND, UserDataModel.UserSerial, UserDataModel.UserDataBaseID);
            component.SetOwnerINFO_USER();
            
            // Register To Field Subject System
            IngameFieldSubjectSystem.RegisterOwnerPlayer(component);
            
            // CharacterBase Initialize !!
            LinkedCharacter.Initialize();

            this.cameraPivot = LinkedCharacter.CameraTarget;
            
            mainCamera = cameraSystem.MainCamera;
            cameraSystem.FollowTarget = this.cameraPivot;
            UpdateCameraPlanarDirection(UserDataModel.Singleton.PlayerInfoData.SpawnDirection);
            UpdateCameraTargetPitch();

            cameraSystem.Brain.WorldUpOverride = LinkedCharacter.transform;

            GameObject interactionSensorGo = new GameObject("REIW.Interaction Sensor");
            interactionSensorGo.transform.SetParent(LinkedCharacter.transform);
            interactionSensorGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            InteractionSensor = interactionSensorGo.AddComponent<InteractionSensor>();

            EventBus.Register(LinkedCharacter);
            EventBus.Register(cameraSystem);

            #endregion

            #region Mount Initialize
            foreach (var mount in PlayerMountPrefabs)
            {
                if ((object)mount.Value == null) continue;
                var InstanceMount = Instantiate(mount.Value, Vector3.zero, Quaternion.identity);
                InstanceMount.gameObject.SetActive(false);
                PlayerMountList.Add(mount.Key, InstanceMount.gameObject);
            }
            #endregion

            // Stamina Validator
            InputActionStaminaValidator = new InputActionStaminaValidator();

#if UNITY_EDITOR
            LinkedCharacter?.transform.SetAsFirstSibling();
            EquippedLocalMount?.transform.SetAsFirstSibling();
#endif

            wallClimbComp = LinkedCharacter.CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveWallClimb>();
            grappleComp = LinkedCharacter.CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveGrapple>();
            
            
            //TESTPet Code
            if (!IsStandalone)
            {
                StartCoroutine(SpawnPetCo(UserDataModel.Singleton.PlayerInfoData.SpawnPosition, 
                    Quaternion.LookRotation(UserDataModel.Singleton.PlayerInfoData.SpawnDirection, Vector3.up), OnPetSpawned));    
            }
        }
        
        private void OnPetSpawned(DummyPet pet)
        {
            Debug.Log($"Pet spawned! name={pet.name}");
            pet.SetTarget(LinkedCharacter.transform );
            // 이후 로직...
        }
        
        private IEnumerator SpawnPetCo(Vector3 position, Quaternion rotation, Action<DummyPet> onSpawned)
        {
            var task = AssetManager.Singleton.SpawnTestPet(position, rotation); // UniTask<DummyPet>

            // 코루틴이 Task 완료까지 대기
            yield return new WaitUntil(() =>  task.GetAwaiter().IsCompleted);

            DummyPet pet = default;
            Exception ex = null;
            try
            {
                // 완료 후 결과 꺼내기
                pet = task.GetAwaiter().GetResult();
            }
            catch (Exception e) { ex = e; }

            if (ex != null)
                Debug.LogException(ex);
            else
                onSpawned?.Invoke(pet);
        }
        
        
        DummyPet _dummyPet =null;

        protected virtual void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // [InputController]
            //----------------------------------------------------------------------------------------------------
            InputController.Singleton.OnDownLMC += OnExecuteDownLMC;
            InputController.Singleton.OnUpLMC += OnExecuteUpLMC;
            InputController.Singleton.OnDownRMC += ExecuteDownRmc;
            InputController.Singleton.OnUpRMC += ExecuteUpRmc;
            InputController.Singleton.OnDownLeftShift += ExecuteDownLeftShift;
            InputController.Singleton.OnUpLeftShift += ExecuteUpLeftShift;
            InputController.Singleton.OnJump += OnExecuteJump;
            InputController.Singleton.OnJumpDown += OnStartJump;
            InputController.Singleton.OnJumpUp += OnCancelJump;
            InputController.Singleton.OnParkour += OnExecuteParkour;
            InputController.Singleton.OnMount += OnExecuteMount;
            InputController.Singleton.OnSkillActionE += OnExecuteSpecialActionE;
            InputController.Singleton.OnSkillActionT += OnExecuteSpecialActionT;
            InputController.Singleton.OnToggleWalk += OnExecuteToggleWalk;
            InputController.Singleton.OnCameraSwitch += OnCameraSwitch;
            InputController.Singleton.OnCharacterStageEnter += OnExecuteCharacterStageEnter;
            InputController.Singleton.OnInventoryOpen += OnExecuteInventoryOpen;
            
            // [UserDataModel]
            //----------------------------------------------------------------------------------------------------
            UserDataModel.Singleton.OnToolDestroyed += OnToolDestroyed;
            UserDataModel.Singleton.PlayerInfoData.StaminaEvents.OnStaminaDepleted += OnStaminaDepleted;

            // [Network Protocol]
            //----------------------------------------------------------------------------------------------------
            //ReNetworkClient.Singleton.AddMessageBufferHandler<FIELD_MOUNT_INSERT_ACK>(PROTOCOL.FIELD_MOUNT_INSERT_ACK, OnRecvFieldMountInsertAck);
        }

        private void OnDestroy()
        {
            Instance = null;

            if (InputController.Singleton  != null)
            {
                InputController.Singleton.OnDownLMC -= OnExecuteDownLMC;
                InputController.Singleton.OnUpLMC -= OnExecuteUpLMC;
                InputController.Singleton.OnDownRMC -= ExecuteDownRmc;
                InputController.Singleton.OnUpRMC -= ExecuteUpRmc;
                InputController.Singleton.OnDownLeftShift -= ExecuteDownLeftShift;
                InputController.Singleton.OnUpLeftShift -= ExecuteUpLeftShift;
                InputController.Singleton.OnJump -= OnExecuteJump;
                InputController.Singleton.OnJumpDown -= OnStartJump;
                InputController.Singleton.OnJumpUp -= OnCancelJump;
                InputController.Singleton.OnParkour -= OnExecuteParkour;
                InputController.Singleton.OnMount -= OnExecuteMount;
                InputController.Singleton.OnSkillActionE -= OnExecuteSpecialActionE;
                InputController.Singleton.OnSkillActionT -= OnExecuteSpecialActionT;
                InputController.Singleton.OnToggleWalk -= OnExecuteToggleWalk;
                InputController.Singleton.OnCameraSwitch -= OnCameraSwitch;
                InputController.Singleton.OnInventoryOpen -= OnExecuteInventoryOpen;
            }

            if (UserDataModel.Singleton != null)
            {
                UserDataModel.Singleton.OnToolDestroyed -= OnToolDestroyed;
                UserDataModel.Singleton.PlayerInfoData.StaminaEvents.OnStaminaDepleted -= OnStaminaDepleted;
            }

            EventBus?.Unregister(LinkedCharacter);
            EventBus?.Unregister(cameraSystem);
        }

        protected virtual void Update()
        {
            if (!LinkedCharacter || !IsActiveControl || !IsActiveMovement)
                return;

            Vector3 moveInput = InputController.Singleton.Move;
            bool isDashRequested = isDashPressedThisFrame;  // 1프레임 트리거
            bool isSprintRequested = false;
            if (IsDashHolding)
            {
                dashHoldTimer += Time.deltaTime;
                bool hasMove = (moveInput.sqrMagnitude > 0.000001f);
                if (hasMove && dashHoldTimer >= SprintRequestHurdleTime)
                {
                    isSprintRequested = true;
                }
            }
            else
            {
                dashHoldTimer = 0f;
            }

            switch (CurrentInputCommandModeType)
            {
                case eInputCommandModeType.Character:
                    var isGrappleRequested = UpdateGrappleRequested();
                    var isWallClimbRequested = UpdateWallClimbRequested();
                    
                    // WallClimb 상태 + Snapping + Ground 상태가 아니면 MoveInput 을 Vector3.zero 값으로 통제
                    if (wallClimbComp.IsActivateWallClimb && 
                        (wallClimbComp.CurrentState == CharacterMoveWallClimb.ClimbState.Snapping || false == LinkedCharacter.IsStableOnCollider))
                    {
                        moveInput = Vector3.zero;
                    }
                    
                    LinkedCharacter.SetInputs(new PlayerCharacterInputs()
                    {
                        Move = moveInput,
                        Look = InputController.Singleton.Look,
                        Jump = jumpRequested,
                        JumpHold = jumpHold,
                        Parkour = parkourRequested,
                        Grapple = isGrappleRequested,
                        Mount = isRidingMount,
                        Dash = isDashRequested,
                        Walk = isWalking,
                        Sprint = isSprintRequested,
                        WallClimb = isWallClimbRequested,
                        MouseClickChecker = _mouseClickChecker,
                    });
                        
                    EquippedLocalMount?.ResetInputs();
                    jumpRequested = false;
                    parkourRequested = false;
                    specialActionE_Requested = false;
                    specialActionT_Requested = false;
                    break;
                case eInputCommandModeType.Riding:
                    EquippedLocalMount?.SetInputs(new MountInputs()
                    {
                        Move = InputController.Singleton.Move,
                        Look = InputController.Singleton.Look,
                        IsRiding = isRidingMount,
                        IsSprint = isSprintRequested,
                    });
                    LinkedCharacter.ResetInputs();
                    jumpRequested = false;
                    jumpHold = false;
                    parkourRequested = false;
                    specialActionE_Requested = false;
                    specialActionT_Requested = false;
                    break;
                case eInputCommandModeType.IgnoreMovement:
                    break;
            }
            
            isDashPressedThisFrame = false;
            isSprinting = isSprintRequested;
        }

        private bool UpdateWallClimbRequested()
        {
            bool detected = wallClimbComp.IsDetectedWall;
            bool possible = wallClimbComp.IsPossibleWallClimb;
            bool active = wallClimbComp.IsActivateWallClimb;
            bool request = false;

            if (specialActionT_Requested)
            {
                if (possible)
                {
                    // 가능할 때만 토글
                    wallClimbComp.IsActivateWallClimb = !active;
                    active = wallClimbComp.IsActivateWallClimb;
                    request = active;
                }
                else
                {
                    // 불가능 상태에서 눌렀다면 강제로 OFF
                    if (active)
                    {
                        wallClimbComp.IsActivateWallClimb = false;
                        active = false;
                    }
                    request = false;
                }
            }

            // HUD 갱신
            PlayerControlHUD.Instance?.SetActiveWidgetWallClimb(active);
            PlayerControlHUD.Instance?.SetActiveWidgetWallClimbEffect(!active && possible);

            return request;
        }


        private bool UpdateGrappleRequested()
        {
            bool isDetectedGrapplePoint = grappleComp.IsPossibleGrapple;
            bool isGrappleRequested = false;
            if (isDetectedGrapplePoint && false == wallClimbComp.IsNotDownGravity)
            {
                PlayerControlHUD.Instance?.SetActiveWidgetGrapple(true);
                isGrappleRequested = specialActionE_Requested;
            }
            else
            {
                PlayerControlHUD.Instance?.SetActiveWidgetGrapple(false);
            }

            return isGrappleRequested;
        }
        
        // CameraPlanarDir를 카메라 타겟이 바라보는 방향으로 업데이트
        public void UpdateCameraPlanarDirection(Vector3 direction)
        {
            // direction 의 up Vector 계산 
            Vector3 worldUp = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(direction, worldUp)) > 0.99f)
                worldUp = Vector3.forward; // 다른 축으로 교체
            Vector3 right = Vector3.Cross(worldUp, direction).normalized;
            Vector3 up = Vector3.Cross(direction, right);
            
            Transform currentCameraTarget = isRidingMount ? EquippedLocalMount.CameraTarget : LinkedCharacter.CameraTarget;
            currentCameraTarget.rotation = Quaternion.LookRotation(direction);
            
            Vector3 forward = Vector3.ProjectOnPlane(direction, up);
            if(forward == Vector3.zero) 
                forward = Vector3.forward;
            
            cameraPlanarDir = forward.normalized;
        }

        // CameraPlanarDir를 카메라 타겟이 바라보는 방향으로 업데이트
        public void UpdateCameraPlanarDirection()
        {
            currentTarget = isRidingMount ? EquippedLocalMount.transform : LinkedCharacter.transform;
            Transform currentCameraTarget = isRidingMount ? EquippedLocalMount.CameraTarget : LinkedCharacter.CameraTarget;
            Vector3 currentUpTarget = currentTarget.up;

            Vector3 forward = currentCameraTarget.forward;
            forward = Vector3.ProjectOnPlane(forward, currentUpTarget);
            if(forward == Vector3.zero) 
                forward = Vector3.forward;
            
            cameraPlanarDir = forward.normalized;
        }

        // cameraTargetPitch를 카메라 타겟이 바라보는 방향의 Pitch로 업데이트
        public void UpdateCameraTargetPitch()
        {
            currentTarget = isRidingMount ? EquippedLocalMount.transform : LinkedCharacter.transform;
            Transform currentCameraTarget = isRidingMount ? EquippedLocalMount.CameraTarget : LinkedCharacter.CameraTarget;
            Vector3 up = currentTarget.up;
            Vector3 fwd = currentCameraTarget.forward;
        
            Vector3 fwdOnPlane = Vector3.ProjectOnPlane(fwd, up);
            if (fwdOnPlane == Vector3.zero)
            {
                cameraTargetPitch = 0f;
                return;
            }

            Vector3 right = Vector3.Cross(up, fwdOnPlane);
            if (right == Vector3.zero)
            {
                cameraTargetPitch = 0f;
                return;
            }
        
            cameraTargetPitch = Vector3.SignedAngle(fwdOnPlane, fwd, right);
            cameraTargetPitch = Mathf.Clamp(cameraTargetPitch, CameraBottomClmap, CameraTopClamp);
        }

        private void LateUpdate()
        {
            if (!LinkedCharacter || !IsActiveControl)
                return;

            if (IsActiveLook)
            {
                if (LinkedCharacter.IsEventLockType(eEventLockType.CameraRotate) == false)
                    CameraRotation(Time.deltaTime);

                if (isRidingMount)
                    EquippedLocalMount?.PostCameraUpdate();
                else
                    LinkedCharacter?.PostCameraUpdate();
            }
        }

        // === Recentering Settings ===
        [SerializeField] bool  recenterEnabled = true;
        private float recenterWait    = 2.5f;   // 입력 끊긴 뒤 대기
        // private float recenterTime    = 1.0f;   // 복귀 시간(초)
        
        // 디폴트 각도 (게임 시작 각도나 원하는 기준각)
        private float defaultPitchDeg = 10f;    // 위/아래
        Vector3 defaultPlanarDir;                        // 기준 헤딩(Up 평면상 방향 벡터)
        
        float recenterPitchSpeedDeg = 130f;
        
        // state
        float  idleTimer;
        bool   recentering;
        // float  recenterT;       
        
        private void CameraRotation(float deltaTime)
        {
            var look = InputController.Singleton.Look;
            if (CameraInvertYAxis)
                look.y = -look.y;

            currentTarget = isRidingMount ? EquippedLocalMount.transform : LinkedCharacter.transform;
            Transform currentCameraTarget = isRidingMount ? EquippedLocalMount.CameraTarget : LinkedCharacter.CameraTarget;
            Vector3 currentUpTarget = currentTarget.up;

            //
            var move = InputController.Singleton.Move;
            bool hasMove = (move.sqrMagnitude > 0.000001f);
            bool hasLook = (look.sqrMagnitude > 0.000001f);
            
            // 스틱 허용치 적용해야 함.
            
            //bool cancelRecentering = hasLook || hasMove;
            bool cancelRecentering = hasLook;
            
            
            //var planarInput = Quaternion.Euler(LinkedCharacter.Up * (look.x * CameraRotationSpeed * CameraSensitivityX));
            //cameraPlanarDir = planarInput * cameraPlanarDir;
            //cameraPlanarDir = Vector3.Cross(LinkedCharacter.Up, Vector3.Cross(cameraPlanarDir, LinkedCharacter.Up));
            //var planarRot = Quaternion.LookRotation(cameraPlanarDir, LinkedCharacter.Up);

            // ---------- 초기 기준 헤딩 세팅(한 번만) ----------
            if (defaultPlanarDir == Vector3.zero)
            {
                var fwdOnPlane = Vector3.ProjectOnPlane(currentCameraTarget.forward, currentUpTarget);
                if (fwdOnPlane.sqrMagnitude < 0.0001f) fwdOnPlane = Vector3.forward; // 안전장치
                defaultPlanarDir = fwdOnPlane.normalized;
            }
            
            // [Test Code]
            var planarInput = Quaternion.Euler(currentUpTarget * (look.x * CameraRotationSpeed * CameraSensitivityX));
            cameraPlanarDir = planarInput * cameraPlanarDir;
            cameraPlanarDir = Vector3.Cross(currentUpTarget, Vector3.Cross(cameraPlanarDir, currentUpTarget));
            if (cameraPlanarDir == Vector3.zero)
                cameraPlanarDir = Vector3.ProjectOnPlane(currentCameraTarget.forward, currentUpTarget);

            var planarRot = Quaternion.LookRotation(cameraPlanarDir, currentUpTarget);
            
            
            cameraTargetPitch += look.y * CameraRotationSpeed * CameraSensitivityY;
            cameraTargetPitch = Mathf.Clamp(cameraTargetPitch, CameraBottomClmap, CameraTopClamp);
            var verticalRot = Quaternion.Euler(cameraTargetPitch, 0f, 0f);
            
            // ---------- 입력 유무에 따른 리센터 상태 갱신 ----------
            if (cancelRecentering)
            {
                idleTimer = 0f;
                recentering = false;
                // recenterT   = 0f;

                // 입력이 있을 때는 “현재 방향”을 기준 헤딩으로 재설정해도 된다면 아래 라인: (선호도에 따라)
                // defaultPlanarDir = Vector3.ProjectOnPlane(planarRot * Vector3.forward, currentUpTarget).normalized;
                // defaultPitchDeg  = cameraTargetPitch;
            }
            else
            {
                if (recenterEnabled)
                {
                    if (hasMove)
                    {
                        // 이동 중엔 Pitch-only 리센터를 계속 진행 (wait 없이 바로 진행하려면 그대로)
                        // recentering = true;

                        // 만약 이동 중에도 지연을 원하면:
                        idleTimer += deltaTime;
                        if (idleTimer >= recenterWait) recentering = true;
                    }
                    else
                    {
                        idleTimer += deltaTime;
                        if (idleTimer >= recenterWait) recentering = true;
                    }
                    // idleTimer += deltaTime;
                    // if (recenterEnabled && idleTimer >= recenterWait)
                    //     recentering = true;
                }
            }

            Quaternion desiredRot;
            
            
            if (recenterEnabled && recentering)
            {
                Vector3 yawDir = cameraPlanarDir; // 이미 Up 평면의 단위 벡터여야 함
                if (yawDir.sqrMagnitude < 1e-6f)
                    yawDir = Vector3.ProjectOnPlane(Vector3.forward, currentUpTarget).normalized;

                var keepYawRot = Quaternion.LookRotation(yawDir, currentUpTarget);
                
                // // Pitch만 기본값으로 서서히 복귀
                // recenterT = (recenterTime <= 0f) ? 1f : Mathf.Clamp01(recenterT + deltaTime / recenterTime);
                // float t = EaseOutCubic(recenterT);
                //
                // float targetPitch = Mathf.Lerp(cameraTargetPitch, defaultPitchDeg, t);
                // targetPitch = Mathf.Clamp(targetPitch, CameraBottomClmap, CameraTopClamp);

                // ❗시간 보간 제거, 각속도 기반으로 Pitch만 이동
                float step = recenterPitchSpeedDeg * deltaTime;              // 이번 프레임 허용 각도
                float targetPitch = Mathf.MoveTowards(
                    cameraTargetPitch,
                    Mathf.Clamp(defaultPitchDeg, CameraBottomClmap, CameraTopClamp),
                    step
                );
                
                var pitchOnlyRot = Quaternion.Euler(targetPitch, 0f, 0f);
                desiredRot = keepYawRot * pitchOnlyRot;


                // if (recenterT >= 1f)
                // {
                //     recentering = false;// Yaw는 건드리지 않음!
                //     cameraTargetPitch = targetPitch; // Pitch만 스냅
                //     // cameraPlanarDir = ...  (수정 금지)
                // }
                // 상태 업데이트
                cameraTargetPitch = targetPitch; // 실제 내부 상태도 따라감
                // 끝났는지 체크
                if (Mathf.Approximately(cameraTargetPitch, Mathf.Clamp(defaultPitchDeg, CameraBottomClmap, CameraTopClamp)))
                {
                    recentering = false;
                    idleTimer = 0f;
                    // recenterT 제거했으니 관리 불필요
                }
            }
            else
            {
                // 평소엔 유저 입력으로 만든 회전 사용
                desiredRot = planarRot * verticalRot;
            }
            
            // 최종 회전 적용
            var slerpValue = 1f - Mathf.Exp(-CameraRotationSharpness * deltaTime);
            // var targetRotation = Quaternion.Slerp(LinkedCharacter.CameraTarget.rotation, planarRot * verticalRot, slerpValue); 
            // LinkedCharacter.CameraTarget.rotation = targetRotation;

            // [Test Code]
            // var targetRotation = Quaternion.Slerp(currentCameraTarget.rotation, planarRot * verticalRot, slerpValue);
            var targetRotation = Quaternion.Slerp(currentCameraTarget.rotation, desiredRot, slerpValue);
            
            EventCameraRotateAction?.Invoke(ref targetRotation);
            currentCameraTarget.rotation = targetRotation;
        }
        
        // 부드러운 리센터용 이징
        static float EaseOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - Mathf.Pow(1f - t, 3f);
        }
        
        static float EaseInOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return (t < 0.5f) ? 4f*t*t*t : 1f - Mathf.Pow(-2f*t + 2f, 3f)/2f;
        }

        void OnExecuteDownLMC()
        {
            _mouseClickChecker.OnMouse(MouseClickChecker.MouseButton.Left, MouseClickChecker.MouseAction.Down);
        }

        void OnExecuteUpLMC()
        {
            _mouseClickChecker.OnMouse(MouseClickChecker.MouseButton.Left, MouseClickChecker.MouseAction.Up);
        }
        
        void ExecuteDownRmc()
        {
            _mouseClickChecker.OnMouse(MouseClickChecker.MouseButton.Right, MouseClickChecker.MouseAction.Down);
    
            // 홀드로 처음 진입할 때만 Dash 트리거
            if (!IsDashHolding)
            {
                isDashPressedThisFrame = true;
                dashHoldTimer = 0f;
            }
            dashHoldRMB = true;
        }

        void ExecuteUpRmc()
        {
            _mouseClickChecker.OnMouse(MouseClickChecker.MouseButton.Right, MouseClickChecker.MouseAction.Up);
            dashHoldRMB = false;
            
            // 모든 홀드 해제 시 초기화
            if (!IsDashHolding)
            {
                dashHoldTimer = 0f;
            }
        }

        void ExecuteDownLeftShift()
        {
            if (!IsDashHolding)
            {
                isDashPressedThisFrame = true;
                dashHoldTimer = 0f;
            }
            dashHoldShift = true;
        }

        void ExecuteUpLeftShift()
        {
            dashHoldShift = false;

            if (!IsDashHolding)
            {
                dashHoldTimer = 0f;
            }
        }

        void OnExecuteJump()
        {
            jumpRequested = true;
        }

        void OnStartJump()
        {
            jumpHold = true;
        }

        void OnCancelJump()
        {
            jumpHold = false;
        }

        void OnExecuteParkour()
        {
            parkourRequested = true;
        }

        void OnExecuteSpecialActionE()
        {
            specialActionE_Requested = true;
        }

        void OnExecuteSpecialActionT()
        {
            specialActionT_Requested = true;
        }

        void OnExecuteToggleWalk()
        {
            isWalking = !isWalking;
        }

        void OnCameraSwitch()
        {
            // 3rd Person Camera / FPS Camera Switch
            IngameCameraSystem.Instance?.TogglePlayerCameraView();
        }

        void OnExecuteCharacterStageEnter()
        {
            CharacterStageController.Instance?.ActiveCharacterStage(true);
        }

        void OnExecuteInventoryOpen()
        {
            UIManager.Show<InventoryUI>(UIList.InventoryUI);
        }
        
        void OnToolDestroyed(ulong databaseID, ushort category , ushort kind, uint serial)
        {
            var itemData = GameDataModel.Singleton.ItemData.GetItemData(category, kind, serial);
            var strItemName = LocalizationManager.Singleton.GetItemName(itemData.ItemDataSO.Name);
            var toastText = LocalizationManager.Singleton.GetLocalizedString(
                LocalizationManager.LzTableContents, "gathering_notice_durability", new Dictionary<string, object>
                {
                    { "item_name", strItemName }
                });
            ToastUI.ShowToast(toastText);
        }
        
        void OnStaminaDepleted()
        {
            if (wallClimbComp != null && wallClimbComp.IsActivateWallClimb)
            {
                wallClimbComp.IsActivateWallClimb = false;
            }
        }
    }
}