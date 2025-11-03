using System;
using Animancer.Units;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;
    using eMoveType = CharacterAnimationEnums.eMoveType;

    public class IdleAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.IDLE;

        [SerializeField] private ClipTransition _mainAnimation;
        [SerializeField] private ClipTransition[] _randomAnimations;

        [SerializeField, Seconds] private float _firstRandomizeDelay = 5;
        [SerializeField, Seconds] private float _minRandomizeInterval = 0;
        [SerializeField, Seconds] private float _maxRandomizeInterval = 20;

        private float _randomizeTime;

        public override (bool isChange, eStateType nextType) NextStateType
        {
            get
            {
                var nextState = base.NextStateType;
                if (nextState.isChange)
                    return nextState;

                if (Movement.IsMoveInput || AnimationParameters.IsValidForwardSpeed)
                {
                    if (Movement.IsWalkInput)
                        return (true, eStateType.WALK);
                    return (false, eStateType.RUN);
                }

                return (false, StateType);
            }
        }

        public override bool CanEnterState => Movement.IsGrounded;
        public override bool CanExitState => true;

        protected override void OnEnable()
        {
            base.OnEnable();

            ChangeStaminaActionType(eStaminaActionType.Normal);
            PlayMainAnimation();

            _randomizeTime += _firstRandomizeDelay;
            Movement.CurrentMoveType = eMoveType.STAND;
        }

        private void PlayMainAnimation()
        {
            _randomizeTime = UnityEngine.Random.Range(_minRandomizeInterval, _maxRandomizeInterval);
            InternalPlayAnimation(eAnimationType.IDLE);
        }

        public override bool LateUpdateState()
        {
            if (!base.LateUpdateState())
                return false;

            AnimancerState state = Animancer.States.Current;
            if (state == _mainAnimation.State && state.Time >= _randomizeTime)
                PlayRandomAnimation();

            return true;
        }

        private void PlayRandomAnimation()
        {
            if (_randomAnimations.Length == 0)
                return;

            int index = UnityEngine.Random.Range(0, _randomAnimations.Length);
            ClipTransition animation = _randomAnimations[index];
            AnimancerState state = Animancer.Play(animation);
            state.FadeGroup.SetEasing(Easing.Sine.InOut);
            SetAnimationEndEvent(state, PlayMainAnimation);
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InAnimationType)
            {
                case eAnimationType.IDLE:
                    Movement.CurrentMoveType = eMoveType.STAND;
                    var state = Animation.PlayAnimation(InAnimationType, _mainAnimation, InAnimationSpeed, InCalculateSpeedFunc);
                    SetUseRootMotion(state);
                    return state;
            }

            return null;
        }
    }
}