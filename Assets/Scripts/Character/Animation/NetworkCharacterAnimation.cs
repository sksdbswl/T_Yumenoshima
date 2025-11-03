using System;
using Animancer;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;

    public class NetworkCharacterAnimation : CharacterAnimation
    {
        public override bool IsLocal => false;

        public override bool IsMoving => IsMovingState(true);

        protected override void Start()
        {
            base.Start();
            Movement.EnableIK(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            PlayAnimation(eAnimationType.IDLE);
        }

        protected override bool InitializeRootMotionSettings()
        {
            return true;
        }

        public override bool CheckAnimationState()
        {
            return false;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            var state = (CharacterAnimationState)GetAnimationState(InAnimationType);
            if (!state)
                return null;

            var animancerState = state.PlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);

            if (state.StateType == CurrentState)
                return animancerState;

            StateMachine.CurrentState.DisableStateNetwork();
            _prevState = StateMachine.CurrentState.StateType;
            StateMachine.CurrentState = state;
            StateMachine.CurrentState.EnableStateNetwork();
            _currentState = state.StateType;
            return animancerState;
        }

        protected override bool ChangeAnimationNetObject(in eAnimationType InAnimationType, in float InAnimationSpeed)
        {
            return false;
        }
    }
}
