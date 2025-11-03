using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using KinematicCharacterController;
using REIW.Animations;
using UnityEngine;

namespace REIW
{
    using REIW.EventLock;
    
    public partial class LocalCharacter : CharacterBase
    {
        public static LocalCharacter Instance { get; private set; }
        public static Transform Transform { get; private set; }
        
        public override bool IsLocalCharacter => true;

        private void Awake()
        {
            Instance = this;
            Transform = transform;
        }
        
        /// <summary>
        /// 루트모션 속력 저장을 위한 지정 크기 큐
        /// 지정 크기보다 카운트가 커지면 오래된 값 제거함
        /// </summary>
        public class FixedSizeQueue<T> where T : struct, IConvertible
        {
            private readonly Queue<T> queue = new Queue<T>();
            private readonly int maxSize;
    
            public int Count => queue.Count;
            public int Capacity => maxSize;
    
            public FixedSizeQueue(int size)
            {
                maxSize = size;
            }

            public void Clear()
            {
                queue.Clear();
            }
    
            public void Enqueue(T item)
            {
                queue.Enqueue(item);
        
                while (queue.Count > maxSize)
                {
                    queue.Dequeue();
                }
            }
    
            public T Dequeue()
            {
                return queue.Dequeue();
            }
    
            public double GetAverage()
            {
                if (queue.Count == 0)
                    return 0;
        
                double sum = 0;
                foreach (var item in queue)
                {
                    sum += Convert.ToDouble(item);
                }
        
                return sum / queue.Count;
            }
    
            public T GetMin()
            {
                if (queue.Count == 0)
                    throw new InvalidOperationException("Queue is empty");
            
                return queue.Min();
            }
    
            public T GetMax()
            {
                if (queue.Count == 0)
                    throw new InvalidOperationException("Queue is empty");
            
                return queue.Max();
            }
        }
        
        // CharacterController가 아닌 CapsuleCollider를 사용한 캐릭터의 이동시, 충돌이나 지면검사 등의 제어를 해주는 에셋
        // 상세는 노션에 올려둔 메뉴얼 확인 부탁드립니다
        // https://www.notion.so/voyagerga`mes/KinematicCharacterController-KCC-224229b506f280b6863bf051789f0688
        [SerializeField] private KinematicCharacterMotor motor;
        public KinematicCharacterMotor Motor => motor;

        public GameObject PlayerCharacterModel;
        
        [Space(5)][Header("Jumping")]
        [SerializeField] protected float jumpScalableForwardSpeed = 0f; // 점프 순간 이동 입력값에 대한 추가 속도 배율
        [SerializeField] protected float jumpHeight = 1.5f;             // 점프 높이
        [SerializeField] protected float jumpUpGravityMagnitude = 30f;  // 점프 상승시 적용할 중력 가속도
        [SerializeField] protected float jumpDownGravityMagnitude = 30f;// 점프 하강시 적용할 중력 가속도
        [SerializeField] protected float jumpCollisionHitNormalDotProduct = -0.7f;// 점프 상승시 상단의 오브젝트와 충돌 체크할 법선의 내적값

        [Space(5)] [Header("Rootmotion value memory queue")] 
        [SerializeField] protected int rootmotionValueQueueCapacity = 20;

        [Space(5)] [Header("Rootmotion speed")]
        [SerializeField] private CharacterMoveSpeedData  rootMotionSpeedData;
        
        [Space(5)][Header("Camera")]
        [SerializeField] private Transform cameraTarget;
        public Transform CameraTarget => cameraTarget;
        
//        private bool holdSprintRequestUntilDashEnds = false;             // 대시 중일 때, 스프린트 상태 전환 보류 플래그
        
        public void SetCommandModeType(eInputCommandModeType type) => CurrentEInputCommandModeType = type;
        public eInputCommandModeType CurrentEInputCommandModeType { get; private set; } = eInputCommandModeType.Character;

        // 루트모션 포지션 변화값의 평면 변화값 사용 모드
        [SerializeField] private CharacterRootMotionMode modeRootMotionHorizotalPos;
        public override CharacterRootMotionMode ModeRootMotionHorizontalPos
        {
            get => modeRootMotionHorizotalPos;
            set => modeRootMotionHorizotalPos = value;
        }
        
        // 루트모션 포지션 변화값의 수직 변화값 사용 모드
        [SerializeField] private CharacterRootMotionMode modeRootMotionVerticalPos;
        public override CharacterRootMotionMode ModeRootMotionVerticalPos
        {
            get => modeRootMotionVerticalPos;
            set => modeRootMotionVerticalPos = value;
        }

        // 루트모션 회전 변화값의 사용 모드
        [SerializeField] private CharacterRootMotionMode modeRootMotionRotation;
        public override CharacterRootMotionMode ModeRootMotionRotation
        {
            get => modeRootMotionRotation;
            set => modeRootMotionRotation = value;
        }
        
        // 플레이어의 이동 입력을 막을때 사용(대시에 사용하게 위해 작성)
        [SerializeField] private bool lockMoveInput;
        public override bool LockMoveInput
        {
            get
            {
                return CharacterEventLockController.IsEventLockType(eEventLockType.CharacterInputLock) || lockMoveInput;
            }
            set
            {
                lockMoveInput = value;   
            }
        }
        
        [Space(5)][Header("GrapplePoint")]
        [SerializeField] private Transform GrapplePoint;
        
        public Transform GrapplePointInstant => _grapplePointInstant;
        private Transform _grapplePointInstant =null;
        
        public override Vector3 CharacterLookDir
        {
            get => base.CharacterLookDir;
            set
            {
                base.CharacterLookDir = value;
                lastLookDirection = value;
            }
        }

        public TransformLinker ColliderTransformLinker => colliderTransformLinker;

        // 플레이어의 키 입력
        private PlayerCharacterInputs prevInputs = new();
        private PlayerCharacterInputs curInputs = new();

        public override PlayerCharacterInputs CurrentInputs => curInputs;

        // 플레이어의 방향키 입력 값을 (x, 0, y)로 표현한 벡터
        private Vector3 playerMoveInput;
        public Vector3 PlayerMoveInput => playerMoveInput;
        
        // 플레이어의 방향키 입력에 따른 최종 시선 방향
        protected Vector3 lastLookDirection;
        
        // 루트모션 저장용 버퍼 변수
        private Vector3 animatorDeltaPosition;
        private Quaternion animatorDeltaRotation;
        
        // 루트모션 속력 저장 큐
        private FixedSizeQueue<float> rootmotionVelocityQueue;
        
        public bool UpdateRootmotionVelocity { set; private get; } = true;

        private Vector3 moveInput;
        private Vector3 prevMoveInput;
        private float moveInputThresholdSqr = 0.01f; // 이동 여부를 판단을 위한 수치

        private CharacterMoveComponentsHandler _characterMoveComponentsHandler;
        public CharacterMoveComponentsHandler CharacterMoveComponentsHandler => _characterMoveComponentsHandler;
        
        private CharacterEventLockController _characterEventLockController;
        public CharacterEventLockController CharacterEventLockController => _characterEventLockController ??= new CharacterEventLockController(this);
        
        private Rigidbody _rigidbody;

        public bool IsEventLockType(eEventLockType locktype)
        {
            return CharacterEventLockController.IsEventLockType(locktype);
        }

        private AnimationEventController _animationEventController = null;

        public AnimationEventController GetAniimationEventController()
        {
            return _animationEventController;
        }
        
        // public override void Initialize()
        // {
        //     Debug.LogWarning("Initializing Character");
        //     base.Initialize();
        //     
        //     motor.CharacterController = this;
        //     if (motor.Capsule != null)
        //         motor.Capsule.enabled = false;
        //     motor.Capsule = collider;
        //     motor.ValidateData();
        //
        //     rootmotionVelocityQueue = new FixedSizeQueue<float>(rootmotionValueQueueCapacity);
        //
        //     _characterMoveComponentsHandler ??= MyGameObject.GetorAddComponent<CharacterMoveComponentsHandler>();
        //     _characterMoveComponentsHandler.Initialize(this, this.CharacterAnimation.StateMachine);
        //
        //     _animationEventController = CharacterAnimation.gameObject.GetorAddComponent<AnimationEventController>();
        //     _animationEventController.Initialize(this);
        //
        //     _rigidbody ??= GetComponent<Rigidbody>();
        //
        //     if (GrapplePoint != null)
        //     {
        //         _grapplePointInstant = Instantiate(GrapplePoint);
        //         _grapplePointInstant.position = Vector3.zero;
        //     }
        //
        //     RefreshAppearanceAndClothes();
        //     void RefreshAppearanceAndClothes()
        //     {
        //         RefreshAppearance(UserDataModel.Singleton.PlayerInfoData.AvatarAppearance);
        //         EquipClothes
        //         (
        //             UserDataModel.Singleton.PlayerEquipData.GetEquipData(EnumParts.Top),
        //             UserDataModel.Singleton.PlayerEquipData.GetEquipData(EnumParts.Bottom),
        //             UserDataModel.Singleton.PlayerEquipData.GetEquipData(EnumParts.Shoes)
        //         );
        //     }
        //
        //     UserDataModel.Singleton.OnItemEquipStateChanged += OnItemEquipStateChanged;
        // }
        //
        // protected override void OnDestroy()
        // {
        //     base.OnDestroy();
        //     UserDataModel.Singleton.OnItemEquipStateChanged -= OnItemEquipStateChanged;
        // }

        // private void OnItemEquipStateChanged(ulong databaseID, ushort category, ushort kind, uint serial, bool isEquipped)
        // {
        //     EquipClothes
        //     (
        //         UserDataModel.Singleton.PlayerEquipData.GetEquipData(EnumParts.Top),
        //         UserDataModel.Singleton.PlayerEquipData.GetEquipData(EnumParts.Bottom),
        //         UserDataModel.Singleton.PlayerEquipData.GetEquipData(EnumParts.Shoes)
        //     );
        // }

        protected override void Update()
        {
            base.Update();

            CharacterEventLockController.UpdateEventLock(curInputs, CurrentState);
            HandleCharacterInput();
        }
        
        public void SetInputs(PlayerCharacterInputs newInput)
        {
            curInputs = newInput;
        }
        
        public void ResetInputs()
        {
            curInputs.Move = Vector2.zero;
            curInputs.Look = Vector2.zero;
            curInputs.Jump = false;
            curInputs.JumpHold = false;
            curInputs.Parkour = false;
            curInputs.Grapple = false;
            curInputs.Walk = false;
            curInputs.Sprint = false;
            curInputs.Dash = false;
            curInputs.Mount = false;
            curInputs.WallClimb = false;

            lastLookDirection = Vector3.zero;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Quaternion GetCameraPlanarRotation()
        {
            Vector3 fwd = Vector3.ProjectOnPlane(cameraTarget.rotation * Vector3.forward, Up).normalized;
            if (fwd.sqrMagnitude == 0f)
                fwd = Vector3.ProjectOnPlane(cameraTarget.rotation * Vector3.up, Up).normalized;
            return Quaternion.LookRotation(fwd, Up);
        }
        
        private void HandleCharacterInput()
        {
            if (cameraTarget == null)
                return;

            playerMoveInput = new Vector3(curInputs.Move.x, 0, curInputs.Move.y);
            playerMoveInput = Vector3.ClampMagnitude(playerMoveInput, 1f);
            
            // lockMoveInput이 true일 경우, 이동 입력값을 무시
            moveInput = lockMoveInput ? Vector3.zero : playerMoveInput;
            
            // 카메라 방향을 기반으로 이동 입력값을 실제 방향값으로 변환
            var cameraPlanarRotation = GetCameraPlanarRotation();
            
            characterMoveDir = cameraPlanarRotation * playerMoveInput;
            
            // 이동 입력값이 있을 경우 lastLookDirection을 입력값 방향으로 설정
            if (moveInput.sqrMagnitude > moveInputThresholdSqr)
            {
                lastLookDirection = cameraPlanarRotation * moveInput.normalized;

                if (IsStableOnCollider && prevMoveInput == Vector3.zero)
                    EventBus.Post<ICharacterBaseEventListener>(_ => _.OnMoveStarted());
            }
            // 캐릭터의 시선 방향 결정
            CharacterLookDir = lastLookDirection != Vector3.zero ? lastLookDirection : transform.forward;

            // update 순서 문제로 외부에서 update
            _characterMoveComponentsHandler.UpdateMoveComponents();
            // 남은 입력 값의 처리
            BroadcastInputEvent();

            prevMoveInput = moveInput;
        }
        
        private void BroadcastInputEvent()
        {   
            if (curInputs.Jump)
            {
                EventBus.Post<ICharacterBaseEventListener>(_=>_.OnJumpRequested());
            }
            
            if (curInputs.Dash)
            {
                if (PlayerController.Instance.InputActionStaminaValidator.CanStaminaAction(eStaminaActionType.Dash))
                {
                    EventBus.Post<ICharacterBaseEventListener>(_ => _.OnDashRequested(() =>
                    {
                        PlayerController.Instance.CurrentExecuteActionTypeStateType =
                            PlayerController.Instance.InputActionStaminaValidator.CanStaminaAction(eStaminaActionType
                                .Sprint)
                                ? eStaminaActionType.Sprint
                                : eStaminaActionType.Normal;
                    }));
                }
            }

            // Gadget 입력 변화 감지 (토글 방식)
            switch (prevInputs.Mount)
            {
                case true when !curInputs.Mount:
                    EventBus.Post<ICharacterBaseEventListener>(_ => _.OnMountReleased());
                    break;
                case false when curInputs.Mount:
                    EventBus.Post<ICharacterBaseEventListener>(_ => _.OnMountRequested());
                    break;
            }

            // 이전 프레임 입력값과 비교하여 Request, Release 구분
            // 아예 Input에 구분해서 넣어도 될것 같은데 일단은 이렇게 처리.
            switch (prevInputs.Sprint)
            {
                case true when !curInputs.Sprint:
                    EventBus.Post<ICharacterBaseEventListener>(_=>_.OnSprintReleased());
                    break;
                case false when curInputs.Sprint:
                    if (PlayerController.Instance.InputActionStaminaValidator.CanStaminaAction(eStaminaActionType.Sprint))
                        EventBus.Post<ICharacterBaseEventListener>(_=>_.OnSprintRequested());
                    break;
            }

            // 이전 프레임 입력값과 비교하여 Request, Release 구분
            switch (prevInputs.Walk)
            {
                case true when !curInputs.Walk:
                    EventBus.Post<ICharacterBaseEventListener>(_=>_.OnWalkReleased());
                    break;
                case false when curInputs.Walk:
                    EventBus.Post<ICharacterBaseEventListener>(_=>_.OnWalkRequested());
                    break;
            }

            prevInputs = curInputs;
        }

        public override void StartJump(bool directly)
        {
            base.StartJump(directly);
            
            // 모터에 이거 호출 안해주면 바닥에 항상 스내핑되므로 주의
            motor.ForceUnground();
            motor.SetGroundSolvingActivation(false);
            
            // 점프 높이 기반으로 점프 속도 계산
            jumpUpVelocity = Mathf.Sqrt(jumpHeight * 2f * jumpUpGravityMagnitude);
            jumpRequested = true;
            
            // 공중 속도의 최대치를 저장된 루트모션 속력 평균 값으로 설정
            // >>> 기본 이동 제어에 루트모션 값만을 사용한다는 전제하에 작성한 코드이므로 주의해주세요 
            curMaxAirMoveSpeed = (float)rootmotionVelocityQueue.GetAverage();
            if (curMaxAirMoveSpeed < rootMotionAirMoveSpeed)
                curMaxAirMoveSpeed = rootMotionAirMoveSpeed;
        }
        
        public override void SetGravity(Vector3 gravity)
        {
            gravityDir = gravity;
        }

        public override void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool directly = true)
        {
            if (directly || !motor.enabled)
            {
                // 1. Roll 회전을 포함한 원하는 회전/위치를 Transform에 즉시 적용
                transform.SetPositionAndRotation(position, rotation);

                // 2. 모터 내부 캐시도 같은 값으로 맞춰 둔다. (모터 활성/비활성과 무관하게 안전)
                motor?.SetPositionAndRotation(position, rotation);
            }
            else
            {
                // 일반 경로
                motor.SetPositionAndRotation(position, rotation);
            }
        }

        public override void AddRootMotionPosition(Vector3 deltaPos)
        {
            // 루트모션 이동값을 저장
            // 저장해둔 값은 주기적으로 UpdateVelocity를 통해 적용 
            animatorDeltaPosition += deltaPos;
        }

        public override void AddRootMotionRotation(Quaternion deltaRot)
        {
            // 루트모션 회전값을 저장
            // 저장해둔 값은 주기적으로 UpdateVelocity를 통해 적용
            animatorDeltaRotation *= deltaRot;
        }
        
        public override void GroundedCheck(ref CharacterSurfaceContactReport report)
        {
            // 현재 지표면 상태 검사
            // 로컬 캐릭터는 Motor에서 검사를 진행해 주므로, Motor의 결과값을 report에 저장하는 것만 실행
            report.IsStableOnSurface = motor.GroundingStatus.IsStableOnGround;
            report.SurfaceNormal = motor.GroundingStatus.GroundNormal;
            report.SurfaceCollider = motor.GroundingStatus.GroundCollider;
            report.SurfacePoint = motor.GroundingStatus.GroundPoint;
        }

        public void PostCameraUpdate()
        {
            // LateUpdate에서 카메라 회전 이후, 캐릭터 시선 방향 재계산
            var cameraPlanarDirection = GetCameraPlanarRotation();
            
            var moveInput = LockMoveInput ? Vector3.zero : playerMoveInput;
            if (moveInput.sqrMagnitude > 0.02f)
            {
                lastLookDirection = cameraPlanarDirection * moveInput.normalized;
            }
            
            CharacterLookDir = lastLookDirection != Vector3.zero ? lastLookDirection : transform.forward;
        }

        public void ResetRootMotionVelocityQueue()
        {
            rootmotionVelocityQueue?.Clear();
        }

        public void SetRootMotionAirMoveSpeed(int InMoveType)
        {
            if (rootMotionSpeedData != null)
                rootMotionAirMoveSpeed = rootMotionSpeedData.GetSpeed(InMoveType);
        }
        
        public void ResetMovement()
        {
            currentMoveVelocity = Vector3.zero;
            animatorDeltaPosition = Vector3.zero;
        }

        public void SetActiveCharacterPhysics(bool active)
        {
            motor.ForceUnground(0f);
            motor.BaseVelocity = Vector3.zero;
            motor.SetMovementCollisionsSolvingActivation(active);
            motor.SetGroundSolvingActivation(active);
            motor.enabled = active;
            _rigidbody.isKinematic = !active;
        }
    }
}

