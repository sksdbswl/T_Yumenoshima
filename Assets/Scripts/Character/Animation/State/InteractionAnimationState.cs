using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class InteractionAnimationState : PlayTargetAnimationState
    {
        public override eStateType StateType => eStateType.INTERACTION;

        [SerializeField] protected LinearMixerTransition _animationMixer;

        public override eAnimationType PlayAnimationType
        {
            set
            {
                base.PlayAnimationType = value;
                if (!enabled)
                    Movement.IsInteractionInput = PlayAnimationType != eAnimationType.NONE;
            }
        }

        protected override void PlayAnimationPostProcess(in AnimancerState InAnimationState)
        {
            base.PlayAnimationPostProcess(InAnimationState);
            Movement.IsInteractionInput = false;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            var state = Animation.PlayAnimation(InAnimationType, _animationMixer, InAnimationSpeed, InCalculateSpeedFunc);
            if (state.IsValid() && _animationMixer.State != null)
            {
                _animationMixer.State.Parameter = GetAnimationParameter(InAnimationType);
                _animationMixer.State.RecalculateWeights();
                PlayAnimationPostProcess(state);
            }

            return state;
        }

        protected virtual float GetAnimationParameter(in eAnimationType InAnimationType)
        {
            return InAnimationType - eAnimationType.INTERACTION_TYPE_START;
        }
    }
}
