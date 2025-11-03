using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations
{
    [Serializable]
    public class PlayTargetAnimationStateLogic<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TAnimationType : Enum
        where TStateType : Enum
        where TState : AnimationState<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TStateMachine : AnimationStateMachine<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TAnimation : AnimationBase<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
    {
        [SerializeField] protected TAnimationType _playAnimationType;

        private TState _state;
        private float _playAnimationSpeed = 1f;

        public bool CanEnterState => !_playAnimationType.Equals(default(TAnimationType));
        public bool CanExitState => _state.PlayingAniState == null;

        public TAnimationType PlayAnimationType
        {
            get => _playAnimationType;
            set
            {
                _playAnimationType = value;
                if (_playAnimationType.Equals(default(TAnimationType)))
                    _state.PlayingAniState = null;
                else if (_state.enabled)
                    PlayAnimation();
            }
        }

        public float PlayAnimationSpeed
        {
            set { _playAnimationSpeed = value; }
        }

        public void OnEnable()
        {
            PlayAnimation();
        }

        public void OnDisable()
        {
            PlayAnimationType = default;
            _playAnimationSpeed = 1f;
        }

        public void SetState(TState InState)
        {
            _state = InState;
        }

        public void PlayAnimation()
        {
            _state.PlayingAniState = _state.PlayAnimation(_playAnimationType, _playAnimationSpeed);
        }

        public AnimancerState PlayAnimation(TAnimationType InAnimationType,
            float InAnimationSpeed = 1f, Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            return _state.Animation.PlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
        }

        public bool IsPlayingAnimation(float InNormalizedTime)
        {
            if (_state.PlayingAniState is { IsPlaying: true } && _state.PlayingAniState.NormalizedTime < InNormalizedTime)
                return true;
            return false;
        }

        public void OnAnimation_Finished()
        {
            if (_state.PlayingAniState == null)
                return;

            _state.PlayingAniState = null;
        }
    }
}
