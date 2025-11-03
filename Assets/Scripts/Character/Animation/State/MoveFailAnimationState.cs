using UnityEngine;
using Animancer;
using System;
using REIW.EventLock;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class MoveFailAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.MOVE_FAIL;

        [SerializeField] private ClipTransition _movefailWall;

        private eEventLockType _initEventLockType = eEventLockType.CharacterMoveAllAction;

        public override (bool isChange, eStateType nextType) NextStateType
        {
            get
            {
                if (_playingAniState != null)
                    return (true, eStateType.MOVE_FAIL);

                return (true, eStateType.IDLE);
            }
        }

        public override bool CanExitState => _playingAniState == null;

        public override void OnEnterState()
        {
            base.OnEnterState();
            //ChangeStaminaActionType(eStaminaActionType.Normal);
            _playingAniState = InternalPlayAnimation(eAnimationType.MOVE_FAIL_WALL);
            SetAnimationEndEvent(_playingAniState, OnAnimation_EndEvent_WallFail);
            _playingAniState.NormalizedTime = 0;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InAnimationType)
            {
                case eAnimationType.MOVE_FAIL_WALL:
                    var state = Animation.PlayAnimation(InAnimationType, _movefailWall, InAnimationSpeed, InCalculateSpeedFunc);
                    SetUseRootMotion(state);
                    return state;
                default:
                    return null;
            }
        }

        public override bool LateUpdateState()
        {
            if (_playingAniState != null)
                return false;

            return base.LateUpdateState();
        }

        private void OnAnimation_EndEvent_WallFail()
        {
            if (_playingAniState == null)
                return;

            _playingAniState = null;
        }

        public override eEventLockType CurrentEventLockType => _initEventLockType;
    }
}