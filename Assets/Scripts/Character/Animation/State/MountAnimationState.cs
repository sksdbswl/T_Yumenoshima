using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class MountAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.MOUNT;
        public override EventLock.eEventLockType CurrentEventLockType => EventLock.eEventLockType.CharacterGraple;

        [SerializeField] private ClipTransition scooterIdleAnimation;   // 1001번 Scooter   단일 애니메이션
        [SerializeField] private ClipTransition summonAnimation;        // 3000번 MotorBike 소환 애니메이션
        [SerializeField] private ClipTransition rideIdleAnimation;      // 3000번 MotorBike 대기 애니메이션
        [SerializeField] private ClipTransition rideAnimation;          // 3000번 MotorBike 이동 애니메이션

        [Space(10)]
        [SerializeField] private AnimationCurve _SprintFovSetAniamtionCurve;
        [SerializeField] private float _SprintFov;

        [Space(10)]
        
        [SerializeField] private ClipTransition rideSprintAnimation;    // 3000번 MotorBike 가속 애니메이션
        
        [Space(10)]
        [SerializeField] private AnimationCurve _SprintFovResetAniamtionCurve;
        // [SerializeField] private float _SprintFovResetSpeed;
        
        [Space(10)]
        
        [SerializeField] private ClipTransition rideBreakAnimation;     // 3000번 MotorBike 정지 애니메이션
        [SerializeField] private ClipTransition demountAnimation;       // 3000번 MotorBike 하차 애니메이션
        
        private enum MountingStateType
        {
            None,
            Summon_Waiting, // 소환 대기
            Summoning,      // 소환 중
            Riding,         // 탑승 중
            Demount_Waiting,// 하차 대기
            Demounting,     // 하차 중
            Demount_End,    // 하차 완료
        }

        private MountingStateType _mountState = MountingStateType.None;
        private eAnimationType _currentAnimationType = eAnimationType.NONE;
        private uint _currentMountSerial = 0;
        private bool _wasMoving = false;
        private bool _wasSprinting = false;

        private LocalCharacter _localCharacter =null;
        protected override void Start()
        {
            base.Start();
            if (Character != null)
            {
                if (Character.IsLocalCharacter)
                {
                    _localCharacter =  Character as LocalCharacter;

                    if (_localCharacter == null)
                    {
                        Debug.LogError("_localCharacter == null!! ");
                    }
                }    
            }
            else
            {
                Debug.LogError("Unknown Error: Character == null!! ");
            }
        }
        
        public bool IsInputBlocked => 
            _mountState == MountingStateType.Summon_Waiting || 
            _mountState == MountingStateType.Summoning || 
            _mountState == MountingStateType.Demount_Waiting ||
            _mountState == MountingStateType.Demounting;

        public override bool CanExitState =>
            _mountState == MountingStateType.Demount_End || _mountState == MountingStateType.None;

        public override (bool isChange, eStateType nextType) NextStateType
        {
            get
            {
                var nextState = base.NextStateType;
                if (nextState.isChange)
                    return nextState;

                if (CanExitState)
                {
                    // PlayerController.Instance.CurrentInputCommandModeType = eInputCommandModeType.Character;
                    // PlayerController.Instance.CurrentExecuteActionTypeStateType = eStaminaActionType.Normal;
                    return (true, HasMoveInput() ? eStateType.RUN : eStateType.IDLE);
                }

                return (false, StateType);
            }
        }
        
        public static bool IsAnimationBlocking()
        {
            var playerController = PlayerController.Instance;
            if (playerController == null) return false;
    
            var animState = playerController.LinkedCharacter?.CharacterAnimation?.StateMachine?.CurrentState as MountAnimationState;
            if (animState == null) return false;
    
            return animState.IsInputBlocked;
        }

        public void SetMountSerial(uint mountSerial)
        {
            _currentMountSerial = mountSerial;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            if (_currentMountSerial == 0)
            {
                // var playerMountData = UserDataModel.Singleton.PlayerMountData.TryGetEquippedMount(MountType.Riding);
                // if (playerMountData != null)
                // {
                //     _currentMountSerial = playerMountData.Serial;
                // }
            }

            _wasMoving = false;
            _wasSprinting = false;
            Movement.IsAirborne = false;
            _mountState = MountingStateType.Summon_Waiting;
            _currentAnimationType = eAnimationType.NONE;
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _mountState = MountingStateType.None;
            _currentAnimationType = eAnimationType.NONE;
            _currentMountSerial = 0;
        }

        public override bool UpdateState()
        {
            if (!base.UpdateState())
                return false;

            // Mount 상태 체크를 PlayerController와 동기화
            var playerController = PlayerController.Instance;
            if (playerController)
            {
                // 탑승 중인데 PlayerController가 하차 상태면 하차 처리
                if (_mountState == MountingStateType.Riding && !playerController.IsRidingMount)
                {
                    _mountState = MountingStateType.Demount_Waiting;
                }
            
                // 하차 완료 후 즉시 상태 전환
                if (_mountState == MountingStateType.Demount_End)
                {
                    // 다음 프레임에 상태 전환되도록 보장
                    CharacterStateMacnine.SetImmediateNextStateType(
                        HasMoveInput() ? eStateType.RUN : eStateType.IDLE
                    );
                    
                    // PlayerController.Instance.CurrentInputCommandModeType = eInputCommandModeType.Character;
                    // PlayerController.Instance.CurrentExecuteActionTypeStateType = eStaminaActionType.Normal;
                }
            }

            switch (_mountState)
            {
                case MountingStateType.Summon_Waiting:
                    ProcessSummon();
                    break;

                case MountingStateType.Riding:
                    UpdateRidingAnimation();
                    break;

                case MountingStateType.Demount_Waiting:
                    ProcessDemount();
                    break;
            }

            return true;
        }

        public void StartDeMountProcess()
        {
            // 하차 대기 상태로 변경 (애니메이션 실행 준비 단계)
            _mountState = MountingStateType.Demount_Waiting;
        }

        private void ProcessSummon()
        {
            _mountState = MountingStateType.Summoning;
            if (_currentMountSerial == 1001)
            {
                // Scooter는 소환 애니메이션 없이 바로 탑승
                _mountState = MountingStateType.Riding;
                InternalPlayAnimation(eAnimationType.MOUNT_IDLE_SCOOTER);
            }
            else if (_currentMountSerial == 3000)
            {
                // MotorBike는 소환 애니메이션 재생
                var summonState = InternalPlayAnimation(eAnimationType.MOUNT_SUMMON);
                SetAnimationEndEvent(summonState, () =>
                {
                    _mountState = MountingStateType.Riding;
                    _currentAnimationType = eAnimationType.NONE; // 다음 애니메이션 재생을 위해 리셋
                    PlayerController.Instance.OnMountSpawnEndCallback(true);
                });
            }
        }

        private void ProcessDemount()
        {
            _mountState = MountingStateType.Demounting;

            if (_currentMountSerial == 1001)
            {
                _mountState = MountingStateType.Demount_End;
                // 즉시 다음 상태로 전환 요청
                CharacterStateMacnine.SetImmediateNextStateType(
                    HasMoveInput() ? eStateType.RUN : eStateType.IDLE
                );
                
                // PlayerController.Instance.CurrentInputCommandModeType = eInputCommandModeType.Character;
                // PlayerController.Instance.CurrentExecuteActionTypeStateType = eStaminaActionType.Normal;
            }
            else if (_currentMountSerial == 3000)
            {
                var demountState = InternalPlayAnimation(eAnimationType.MOUNT_DEMOUNT);
                SetAnimationEndEvent(demountState, () => {
                    _mountState = MountingStateType.Demount_End;
                    // 애니메이션 완료 후 다음 상태로 전환 요청
                    CharacterStateMacnine.SetImmediateNextStateType(
                        HasMoveInput() ? eStateType.RUN : eStateType.IDLE
                    );
                    
                    // PlayerController.Instance.CurrentInputCommandModeType = eInputCommandModeType.Character;
                    // PlayerController.Instance.CurrentExecuteActionTypeStateType = eStaminaActionType.Normal;
                });
            }
        }

        private void UpdateRidingAnimation()
        {
            if (_currentMountSerial == 1001)
            {
                // Scooter는 단일 애니메이션만 사용
                InternalPlayAnimation(eAnimationType.MOUNT_IDLE_SCOOTER);
            }
            else if (_currentMountSerial == 3000)
            {
                var playerController = PlayerController.Instance;
                if (!playerController)
                {
                    LogUtil.LogWarning("PlayerController is null");
                    return;
                }

                // InputController에서 직접 입력 상태 가져오기
                Vector2 moveInput = InputController.Singleton?.Move ?? Vector2.zero;
                bool isMoving = moveInput.sqrMagnitude > 0.01f;
                bool isSprinting = playerController.IsSprinting;
                
                // Idle 애니메이션: 이동하지 않을 때
                if (!isMoving)
                {
                    //InternalPlayAnimation(eAnimationType.MOUNT_IDLE);
                    if (_currentAnimationType != eAnimationType.MOUNT_IDLE &&
                        _currentAnimationType != eAnimationType.MOUNT_BREAK)
                    {
                        InternalPlayAnimation(eAnimationType.MOUNT_IDLE);
                    }
                }
                // Sprint 애니메이션: 이동 중이며 스프린트
                else if (isMoving && isSprinting)
                {
                    if (_currentAnimationType != eAnimationType.MOUNT_SPRINT)
                    {
                        InternalPlayAnimation(eAnimationType.MOUNT_SPRINT);
                    }
                }
                // 일반 이동 애니메이션
                else if (isMoving && !isSprinting)
                {
                    if (_currentAnimationType != eAnimationType.MOUNT_RIDE)
                    {
                        InternalPlayAnimation(eAnimationType.MOUNT_RIDE);
                    }
                }

                // 이전 상태 저장
                _wasMoving = isMoving;
                _wasSprinting = isSprinting;
            }
        }

        private bool HasMoveInput()
        {
            // var linkedMount = PlayerController.Instance?.EquippedLocalMount;
            // return linkedMount != null && linkedMount.PlayerMoveInput.sqrMagnitude > 0.01f;
            
            return false;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            // 동일한 애니메이션이 이미 재생 중이면 스킵
            if (_currentAnimationType == InAnimationType)
                return _playingAniState;

            if (_currentAnimationType == eAnimationType.MOUNT_SPRINT && IsLocal)
            {
                // if (_localCharacter != null)
                //     _localCharacter.GetAniimationEventController().CameraFovResetInScript(1, _SprintFovResetAniamtionCurve);
            }

            _currentAnimationType = InAnimationType;

            ClipTransition clip = GetAnimationClip(InAnimationType);
            if (clip == null || !clip.Clip)
            {
                //LogUtil.LogWarning($"Animation clip for {InAnimationType} is missing for mount {_currentMountSerial}");
                return null;
            }

            _playingAniState = Animation.PlayAnimancerFade(InAnimationType, clip, 0.2f);
            SetUseRootMotion(_playingAniState);

            if (InAnimationType == eAnimationType.MOUNT_SPRINT && IsLocal)
            {
                //_localCharacter.GetAniimationEventController().CameraFovSetInScript( _SprintFov,1, _SprintFovSetAniamtionCurve);
            }
            
            return _playingAniState;
        }

        private ClipTransition GetAnimationClip(eAnimationType animType)
        {
            if (_currentMountSerial == 1001)
            {
                // Scooter는 모든 상태에서 단일 애니메이션 사용
                return scooterIdleAnimation;
            }
            else if (_currentMountSerial == 3000)
            {
                // MotorBike는 각 상태별 애니메이션 사용
                return animType switch
                {
                    eAnimationType.MOUNT_SUMMON => summonAnimation,
                    eAnimationType.MOUNT_IDLE => rideIdleAnimation,
                    eAnimationType.MOUNT_RIDE => rideAnimation,
                    eAnimationType.MOUNT_SPRINT => rideSprintAnimation,
                    eAnimationType.MOUNT_BREAK => rideBreakAnimation,
                    eAnimationType.MOUNT_DEMOUNT => demountAnimation,
                    _ => null
                };
            }

            return null;
        }

        // public override IngameCameraSystem_Event.CameraEventType CameraEventType
        // {
        //     get => IngameCameraSystem_Event.CameraEventType.Default;
        // }
    }
}