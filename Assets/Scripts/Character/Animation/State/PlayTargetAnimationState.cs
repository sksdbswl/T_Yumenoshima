using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class PlayTargetAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.PLAY_TARGET_ANIMATION;

        [SerializeField] protected PlayTargetAnimationStateLogic<eAnimationType, eStateType,
            CharacterAnimationState, CharacterAnimationStateMachine, CharacterAnimation> _logic;

        public override bool CanEnterState => _logic.CanEnterState;
        public override bool CanExitState => base.CanExitState || _logic.CanExitState;

        public virtual eAnimationType PlayAnimationType
        {
            get => _logic.PlayAnimationType;
            set => _logic.PlayAnimationType = value;
        }

        public float PlayAnimationSpeed
        {
            set => _logic.PlayAnimationSpeed = value;
        }

        protected override void Awake()
        {
            base.Awake();

            _logic.SetState(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _logic.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _logic.OnDisable();
        }

        protected virtual void PlayAnimation()
        {
            _logic.PlayAnimation();
        }

        protected virtual void PlayAnimationPostProcess(in AnimancerState InAnimationState)
        {
            SetUseRootMotion(InAnimationState);
            SetAnimationEndEvent(InAnimationState, OnAnimation_Finished);
        }

        public override bool IsPlayingAnimation(in float InNormalizedTime)
        {
            if (base.IsPlayingAnimation(InNormalizedTime))
                return true;
            return _logic.IsPlayingAnimation(InNormalizedTime);
        }

        public virtual void OnAnimation_Finished()
        {
            _logic.OnAnimation_Finished();
        }
    }
}
