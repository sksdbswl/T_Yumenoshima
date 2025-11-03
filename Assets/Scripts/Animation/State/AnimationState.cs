using System;
using Animancer;
using Animancer.FSM;
using UnityEngine;

namespace REIW.Animations
{
    public abstract class AnimationState<TAnimationType, TStateType, TState, TStateMachine, TAnimation> : StateBehaviour, IOwnedState<TState>
        where TAnimationType : Enum
        where TStateType : Enum
        where TState : AnimationState<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TStateMachine : AnimationStateMachine<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TAnimation : AnimationBase<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
    {
        public abstract TStateType StateType { get; }

        public AnimancerComponent Animancer => Animation?.Animancer;
        protected TAnimation _animation;
        public TAnimation Animation => _animation;

        public StateMachine<TState> OwnerStateMachine => Animation.StateMachine;
        public TStateMachine StateMachine => (TStateMachine)OwnerStateMachine;

        protected bool IsLocal => Animation.IsLocal;

        protected virtual TStateType DefaultStateType => StateMachine.DefaultState.StateType;

        public virtual (bool isChange, TStateType nextType) NextStateType
        {
            get
            {
                var stateType = StateType;
                if (!CanExitState)
                    return (true, stateType);

                return (false, DefaultStateType);
            }
        }

        public override bool CanExitState => ExitState;
        public virtual bool ExitState { set; protected get; } = false;

        public virtual Vector3 RootDeltaPosition => Animancer.Animator.deltaPosition;
        public virtual Quaternion RootDeltaRotation => Animancer.Animator.deltaRotation;
        public virtual Quaternion RootMotionRotation => Animancer.Animator.rootRotation;

        public virtual bool ApplyRawRootMotion => false;

        protected AnimancerState _playingAniState;
        protected TStateType _prevState;

        public AnimancerState PlayingAniState
        {
            get => _playingAniState;
            set => _playingAniState = value;
        }

#if UNITY_EDITOR
        private string _stateName;
        private string _currentStateName;
#endif

        protected virtual void Awake()
        {
#if UNITY_EDITOR
            _stateName = name;
            _currentStateName = $"{name} (Current)";
#endif
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void OnEnable()
        {
            BaseOnEnable();
        }

        protected virtual void OnDisable()
        {
            BaseOnDisable();
        }

        protected virtual void BaseOnEnable()
        {
            ExitState = false;
            SetPrevState();
        }

        protected virtual void BaseOnDisable()
        {
            _playingAniState = null;
            ExitState = false;
        }

        public virtual void EnableStateNetwork()
        {
            
        }

        public virtual void DisableStateNetwork()
        {
            OnDisable();
        }

        protected virtual void Update()
        {
            UpdateState();
        }

        protected virtual void LateUpdate()
        {
            LateUpdateState();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            gameObject.GetComponentInParentOrChildren(ref _animation);
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            name = _currentStateName;
        }

        public override void OnExitState()
        {
            base.OnExitState();
            name = _stateName;
        }
#endif

        public virtual void Initialize(TAnimation InAnimation)
        {
            _animation = InAnimation;
        }

        public virtual bool UpdateState()
        {
            return true;
        }

        public virtual bool LateUpdateState()
        {
            if (Animation.CheckAnimationState())
                return false;

            UpdateAnimationParameters();

            return true;
        }

        protected virtual void UpdateAnimationParameters()
        {
        }

        protected virtual AnimancerState InternalPlayAnimation(in TAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            return null;
        }

        public virtual AnimancerState PlayAnimation(in TAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            return InternalPlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
        }

        public virtual bool IsPlayingAnimation(in float InNormalizedTime)
        {
            return false;
        }

        protected virtual void SetUseRootMotion(in AnimationClip InAnimationClip)
        {
        }

        protected void SetPrevState()
        {
            _prevState = Animation.CurrentState;
        }

        protected void AddIgnoreChangeNetObjectAnimationType(in TAnimationType InAnimationType)
        {
            Animation.AddIgnoreChangeNetObjectAnimationType(InAnimationType);
        }

        protected bool SetUseRootMotion(in AnimancerState InAnimancerState)
        {
            if (!IsLocal || !InAnimancerState.IsValid())
                return false;

            var clip = InAnimancerState.Clip;
            if (clip)
            {
                SetUseRootMotion(clip);
                return true;
            }

            // if (InAnimancerState is ManualMixerState mixerState)
            //     return SetUseRootMotion(mixerState.GetCurrentChildState());
            //
            // if (InAnimancerState is SequenceState sequenceState)
            // {
            //     var currentState = sequenceState.GetCurrentChildState();
            //     if (SetUseRootMotion(currentState))
            //     {
            //         if (sequenceState.GetNextChildState() is { } nextState)
            //             currentState.SetExitEvent(() => { SetUseRootMotion(nextState); });
            //         return true;
            //     }
            // }

            return false;
        }

        protected void SetAnimationEndEvent(in AnimancerState InAnimationState, in Action InOnEnd)
        {
            if (InAnimationState.IsValid())
            {
                // if (InAnimationState is ManualMixerState mixerState)
                // {
                //     var state = mixerState.GetCurrentChildState() ?? InAnimationState;
                //     ClipTransitionSequence clipTransitionSequence = state.Key switch
                //     {
                //         TransitionAsset { Transition: ClipTransitionSequence taCts } => taCts,
                //         ClipTransitionSequence cts => cts,
                //         _ => null
                //     };
                //
                //     if (clipTransitionSequence != null)
                //     {
                //         clipTransitionSequence.OnEnd = InOnEnd;
                //         return;
                //     }
                // }

                InAnimationState.Events(this).OnEnd = InOnEnd;
            }
        }
    }
}
