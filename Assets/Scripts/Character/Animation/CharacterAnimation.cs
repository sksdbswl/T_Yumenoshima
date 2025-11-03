
using System;
using Animancer;
using Animancer.FSM;
using RootMotion.FinalIK;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public partial class CharacterAnimation : AnimationBase<eAnimationType, eStateType, CharacterAnimationState, CharacterAnimationStateMachine, CharacterAnimation>
    {
        public CharacterBase Character => _clientCharacter?.LogicalCharacter;
        [SerializeField] private ClientCharacter _clientCharacter;
        
        public AnimancerEvents _animancerEvents;
        
        public CharacterAnimationMovement Movement => _movement;
        [SerializeField] private CharacterAnimationMovement _movement;

        public CharacterAnimationParameters Parameters => _parameters;
        [SerializeField] private CharacterAnimationParameters _parameters;

        [SerializeField] protected FullBodyBipedIK _bodyIK;
        [SerializeField] protected GrounderFBBIK _grounderIK;
        
        public override bool IsLocal => Character?.IsLocalCharacter ?? false;

        private OwnerPlayerNetObject _ownerPlayerNetObject;

        public event Action<(AvatarIKGoal footType, float footPower, eKnownSfxSound groundTag)> FootStepEvent
        {
            add    { if (Movement != null) Movement.FootStepEvent += value; }
            remove { if (Movement != null) Movement.FootStepEvent -= value; }
        }

        protected override int AnimationTypeBitDigits => CharacterAnimationEnums.ANIMATION_TYPE_BIT_DIGITS;

        public float ForwardSpeedParameter
        {
            get => _parameters.ForwardSpeed;
            set => _parameters.ForwardSpeed = value;
        }

        public float VerticalSpeedParameter
        {
            get => _parameters.VerticalSpeed;
            set => _parameters.VerticalSpeed = value;
        }

        public bool IsStopping => StateMachine.Run.IsStopping || StateMachine.Sprint.IsStopping || StateMachine.Dash.IsStopping;
        
        // 기존: public virtual bool IsMoving => IsMovingState(false);
        // 로컬이면 파라미터 체크(false), 원격이면 파라미터 체크(true) → NetworkCharacterAnimation의 IsMoving과 동일 동작
        public virtual bool IsMoving => IsMovingState(!IsLocal);

        protected override void Start()
        {
            base.Start();
            if (!IsLocal && Movement != null)
            {
                Movement.EnableIK(false);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RegisterEvents();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterEvents();
        }

        public override void Init()
        {
            base.Init();
            
            if (!IsLocal) 
                PlayAnimation(eAnimationType.IDLE);
        }

        protected override void InitializeComponents()
        {
            if (_clientCharacter == null)
            {
                _clientCharacter = GetComponent<ClientCharacter>();
                Debug.LogError("ClientCharacter has no client character.");
            }

            if (_animancerEvents == null)
            {
                _animancerEvents = GetComponent<AnimancerEvents>();
                Debug.LogError("AnimancerEvents have no animancer events.");
            }
            

            if (IsLocal)
                _ownerPlayerNetObject ??= Character.GetComponent<OwnerPlayerNetObject>();
        }

        protected override bool InitializeRootMotionSettings()
        {
            if (!IsLocal)
                return true;

            if (!base.InitializeRootMotionSettings())
                return false;

            string soName = string.Format(
                AnimationClipRootMotionSettingsSO.GetRootMotionSettingsSOFileNameFormat(eObjectType.Character),
                UserDataModel.Singleton.PlayerInfoData.Race.ToString().ToLower(),
                UserDataModel.Singleton.PlayerInfoData.Gender.ToString().ToLower());

            _rootMotionSettings = AssetManager.Singleton.GetAnimationClipRootMotionSettingsSO(
                $"{nameof(eObjectType.Character).ToLower()}/{soName}");
            return true;
        }

        protected override void InitializeStateMachine()
        {
            base.InitializeStateMachine();

            StateMachine.Walk?.SetMoveMixer(StateMachine.Run?.MoveMixer);
            StateMachine.Sprint?.SetMoveMixer(StateMachine.Run?.MoveMixer);
        }

        protected override void InitializeAnimationMovement()
        {
            base.InitializeAnimationMovement();
            Animancer.Animator.applyRootMotion = false;
            Movement.Initialize(this, _bodyIK, _grounderIK);
        }

        protected override void InitializeAnimationParameters()
        {
            Parameters.CreateParameters(_animancer);
        }

        public bool IsMovingState(bool InCheckParameters)
        {
            return Movement.IsMoving && (!InCheckParameters || ForwardSpeedParameter > 0f || VerticalSpeedParameter > 0f);
        }
        
        protected override AnimancerState InternalPlayAnimation(
            in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f,
            in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            if (IsLocal)
            {
                // 로컬은 기존 베이스 로직(트랜지션/믹서 등)을 그대로 사용
                return base.InternalPlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
            }

            // ====== 원격(네트워크) 전용 흐름 (기존 NetworkCharacterAnimation 동작) ======
            var state = (CharacterAnimationState)GetAnimationState(InAnimationType);
            if (!state)
                return null;

            var animancerState = state.PlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);

            if (state.StateType == CurrentState)
                return animancerState;

            if (StateMachine?.CurrentState != null)
                StateMachine.CurrentState.DisableStateNetwork();

            _prevState = StateMachine.CurrentState.StateType;
            StateMachine.CurrentState = state;

            StateMachine.CurrentState.EnableStateNetwork();
            _currentState = state.StateType;

            return animancerState;
        }
        
        public override bool CheckAnimationState()
        {
            if (!IsLocal)
                return false;

            return base.CheckAnimationState();
        }

        protected override AnimancerState PlayAnimancer(in eAnimationType InAnimationType, in ITransition InTransition,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            var state = base.PlayAnimancer(InAnimationType, InTransition, InAnimationSpeed, InCalculateSpeedFunc);
            if (state.IsValid())
            {
                ChangeAnimationNetObject(InAnimationType, state.Speed);
                return state;
            }

            return null;
        }

        public override AnimancerState PlayAnimancerFade(in eAnimationType InAnimationType,
            in ITransition InTransition, in float fadein = float.MinValue, in float animationSpeed = 1.0f)
        {
            var state = base.PlayAnimancerFade(InAnimationType, InTransition, fadein, animationSpeed);
            if (state.IsValid())
            {
                ChangeAnimationNetObject(InAnimationType, state.Speed);
                return state;
            }

            return null;
        }

        public void SetPlayTargetAnimation(eAnimationType InAnimationType)
        {
            InAnimationType = EnumUtils.GetUnpackValue(InAnimationType, AnimationTypeBitDigits);
            StateBehaviour state = GetAnimationState(InAnimationType);
            if (state != null && state is PlayTargetAnimationState playTargetAnimationState)
                playTargetAnimationState.PlayAnimationType = InAnimationType;
        }

        protected override bool ChangeAnimationNetObject(in eAnimationType InAnimationType, in float InAnimationSpeed)
        {
            if (!IsLocal)
                return false;
            
            if (base.ChangeAnimationNetObject(InAnimationType, InAnimationSpeed))
            {
                _ownerPlayerNetObject?.ChangeAnimType((int)InAnimationType, InAnimationSpeed);
                return true;
            }

            return false;
        }

        protected override bool TryForceSetAnimationState(in CharacterAnimationState InState)
        {
            if (!StateMachine.IsImmediateNextStateType(InState))
                return false;

            return base.TryForceSetAnimationState(InState);
        }
    }
}