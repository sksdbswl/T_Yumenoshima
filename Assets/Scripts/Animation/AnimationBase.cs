using System;
using System.Collections.Generic;
using Animancer;
using Animancer.FSM;
using UnityEngine;

namespace REIW.Animations
{
    public abstract class AnimationBase<TAnimationType, TStateType, TState, TStateMachine, TAnimation> : MonoBehaviour
        where TAnimationType : Enum
        where TStateType : Enum
        where TState : AnimationState<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TStateMachine : AnimationStateMachine<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TAnimation : AnimationBase<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
    {
        [SerializeField] protected AnimancerComponent _animancer;
        public AnimancerComponent Animancer => _animancer;

        [SerializeField] private TStateMachine _stateMachine;
        public TStateMachine StateMachine => _stateMachine;

        protected TStateType _currentState;
        protected TStateType _prevState;
        protected TAnimationType _currentAnimation;
        protected AnimationClipRootMotionSettingsSO _rootMotionSettings;

        private List<TAnimationType> _ignoreChangeNetObjectAnimationTypes = new();

        public TStateType CurrentState => _currentState;
        public TStateType PrevState => _prevState;

        public virtual bool IsLocal => false;

        public virtual int CurrentAnimation
        {
            get => _currentAnimation.ToInt();
            set => _currentAnimation = value.ToEnum<TAnimationType>();
        }

        public float CurrentAnimationSpeed
        {
            get => _animancer.States.Current?.Speed ?? 1f;
            set
            {
                if (_animancer.States.Current != null)
                    _animancer.States.Current.Speed = value;
            }
        }

        protected virtual int AnimationTypeBitDigits { get; }

        public event Action AnimatorMoveEvent;

        public virtual void Init()
        {
            InitializeComponents();
            InitializeAnimationMovement();
            InitializeAnimationEventListener();
        }
        
        protected virtual void Awake()
        {
            InitializeComponents();
            InitializeAnimationParameters();
            InitializeStateMachine();
        }

        protected virtual void Start()
        {
            InitializeRootMotionSettings();
        }

        protected virtual void OnEnable()
        {
            StateMachine.CurrentState = StateMachine.DefaultState;
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void InitializeComponents()
        {
        }

        protected virtual bool InitializeRootMotionSettings()
        {
            return IsLocal;
        }

        protected virtual void InitializeStateMachine()
        {
            StateMachine.Initialize((TAnimation)this);
            StateMachine.InitializeAfterDeserialize();
            
            this.StateMachine.DefaultState = StateMachine.GetAnimationState((TStateType)default(TStateType).NextValue());
            this.StateMachine.CurrentState = StateMachine.DefaultState;
        }

        protected virtual void InitializeAnimationMovement()
        {
        }

        protected virtual void InitializeAnimationParameters()
        {
        }

        protected virtual void InitializeAnimationEventListener()
        {
        }

        public void PlayAnimation(in int InAnimationType, in float InAnimationSpeed = 1f)
        {
            PlayAnimation(InAnimationType.ToEnum<TAnimationType>(), InAnimationSpeed);
        }

        public AnimancerState PlayAnimation(in TAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            return InternalPlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
        }

        protected virtual AnimancerState InternalPlayAnimation(in TAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            var animationType = EnumUtils.GetUnpackValue(InAnimationType, AnimationTypeBitDigits);
            if (GetAnimationState(animationType) is TState state)
                return state.PlayAnimation(animationType, InAnimationSpeed, InCalculateSpeedFunc);
            return null;
        }

        public AnimancerState PlayAnimation(in TAnimationType InAnimationType, in ITransition InTransition,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            return InTransition.IsValid ? PlayAnimancer(InAnimationType, InTransition, InAnimationSpeed, InCalculateSpeedFunc) : null;
        }
        
        protected virtual AnimancerState PlayAnimancer(in TAnimationType InAnimationType, in ITransition InTransition,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            _currentAnimation = EnumUtils.GetUnpackValue(InAnimationType, AnimationTypeBitDigits);
            var state = Animancer.Play(InTransition);
            if (state.IsValid())
            {
                if (InCalculateSpeedFunc != null)
                    state.Speed = InCalculateSpeedFunc(state);
                else
                    state.Speed = InAnimationSpeed;
                return state;
            }

            return null;
        }

        public virtual AnimancerState PlayAnimancerFade(in TAnimationType InAnimationType,
            in ITransition InTransition, in float fadein = 0.2f, in float animationSpeed = 1.0f)
        {
            _currentAnimation = EnumUtils.GetUnpackValue(InAnimationType, AnimationTypeBitDigits);
            AnimancerState state = Animancer.Play(InTransition, fadein);
            state.Speed = animationSpeed;
            return state;
        }

        public virtual bool CheckAnimationState()
        {
            _currentState = StateMachine.CurrentState.StateType;

            TState state = StateMachine.GetAnimationState(StateMachine.CurrentState.NextStateType.nextType);
            if (!state)
                return false;

            if (_currentState.Equals(state.StateType))
            {
                TState secondaryState = StateMachine.GetNextStateSecondaryCheck();
                if (secondaryState)
                    state = secondaryState;
            }

            if (state == StateMachine.CurrentState)
                return false;
            
            if (TryForceSetAnimationState(state))
                return true;

            if (StateMachine.TryResetState(state))
            {
                StateMachine.PreviousState = StateMachine.CurrentState;
                StateMachine.CurrentState = state;
                _prevState = CurrentState;
                return true;
            }

            return false;
        }

        protected virtual bool ChangeAnimationNetObject(in TAnimationType InAnimationType, in float InAnimationSpeed)
        {
            return IsChangeAnimationNetObject(InAnimationType);
        }

        protected bool IsChangeAnimationNetObject(in TAnimationType InAnimationType)
        {
            return !EnumUtils.GetUnpackFlag(InAnimationType, AnimationTypeBitDigits) && !_ignoreChangeNetObjectAnimationTypes.Contains(InAnimationType);
        }

        protected virtual bool TryForceSetAnimationState(in TState InState)
        {
            StateMachine.ForceSetState(InState);
            StateMachine.PreviousState = StateMachine.CurrentState;
            StateMachine.CurrentState = InState;
            _prevState = CurrentState;

            return true;
        }

        public bool TryForceSetAnimationState<T>() where T : TState
        {
            var animationState = StateMachine.GetAnimationState<T>();
            if (animationState == null)
                return false;

            return TryForceSetAnimationState(animationState);
        }

        public bool TryForceSetAnimationState(in TStateType InStateType)
        {
            var animationState = StateMachine.GetAnimationState(InStateType);
            if (animationState == null)
                return false;

            return TryForceSetAnimationState(animationState);
        }

        protected TStateType GetAnimationStateType(in TAnimationType InAnimationType)
        {
            return (InAnimationType.ToInt() / AnimationConsts.ANIMATIONTYPE_WITH_STATETYPE_CONVERSION_UNIT).ToEnum<TStateType>();
        }

        protected StateBehaviour GetAnimationState(in TAnimationType InAnimationType)
        {
            StateBehaviour state = StateMachine.GetAnimationState(GetAnimationStateType(InAnimationType));
            if (!state)
                state = StateMachine.DefaultState;
            return state;
        }

        public void AddIgnoreChangeNetObjectAnimationType(in TAnimationType InAnimationType)
        {
            if (!_ignoreChangeNetObjectAnimationTypes.Contains(InAnimationType))
                _ignoreChangeNetObjectAnimationTypes.Add(InAnimationType);
        }

        protected virtual void OnAnimatorMove()
        {
            AnimatorMoveEvent?.Invoke();
        }

        public RootMotionSettings GetRootMotionSettings(in AnimationClip InAnimationClip)
        {
            return _rootMotionSettings ? _rootMotionSettings.GetRootMotionSettings(InAnimationClip) : default;
        }
    }
}
