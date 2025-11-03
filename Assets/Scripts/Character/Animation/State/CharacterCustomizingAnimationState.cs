using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;
    
    public class CharacterCustomizingAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.CHARACTER_CUSTOMIZING;

        public override (bool isChange, eStateType nextType) NextStateType => (false, StateType);
        [SerializeField] private ClipTransition _mainAnimation;

        public override bool CanEnterState => true;
        public override bool CanExitState => true;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            PlayMainAnimation();

            // Movement?.CurrentMoveType = eMoveType.STAND;
        }

        private void PlayMainAnimation()
        {
            InternalPlayAnimation(eAnimationType.CHARACTER_CUSTOMIZING_IDLE);
        }
        
        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InAnimationType)
            {
                case eAnimationType.CHARACTER_CUSTOMIZING_IDLE:
                    Animation.Animancer.Stop();
                    var state = Animation.PlayAnimation(InAnimationType, _mainAnimation, InAnimationSpeed, InCalculateSpeedFunc);
                    SetUseRootMotion(state);
                    return state;
            }

            return null;
        }
    }
}
