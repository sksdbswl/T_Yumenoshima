using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;
    using eMoveType = CharacterAnimationEnums.eMoveType;

    public class WalkAnimationState : LocomotionAnimationState
    {
        public override eStateType StateType => eStateType.WALK;

        [SerializeField] private ClipTransition _moveStopRight;

        public override (bool isChange, eStateType nextType) NextStateType
        {
            get
            {
                var nextState = base.NextStateType;
                if (nextState.isChange)
                    return nextState;

                if (Movement.IsMoveInput || AnimationParameters.IsValidForwardSpeed)
                {
                    if (!Movement.IsWalkInput)
                        return (false, eStateType.RUN);
                    return (true, StateType);
                }

                return (false, DefaultStateType);
            }
        }

        public bool IsWalking => Animancer.IsPlaying(_moveMixer);

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!ApplyReservedMoveType())
                PlayMoveAnimation(true);
        }

        protected override void SetState(in eMovementType InType)
        {
            base.SetState(InType);

            if (InType == eMovementType.MOVE)
                Movement.CurrentMoveType = eMoveType.WALK;
        }

        protected override void UpdateMove()
        {
            if (AnimationParameters.ForwardSpeed > _currentForwardSpeed)
            {
                if (_currentMovementType is eMovementType.STOP)
                    PlayMoveAnimation();
            }
        }

        protected override void PlayMoveAnimation(in bool InCheckFoot = false)
        {
            if (InCheckFoot)
                CheckFrontFootOnMoveAnimation();

            if (Movement.IsMoveInput)
            {
                if (_moveMixer.State is { IsCurrent: true })
                {
                    SetState(eMovementType.MOVE);
                    return;
                }

                InternalPlayAnimation(eAnimationType.WALK);
            }
            else
            {
                SetState(eMovementType.MOVE);
            }

            ChangeStaminaActionType(eStaminaActionType.Normal);
        }

        public void SetMoveMixer(in LinearMixerTransition InMoveMixer)
        {
            _moveMixer = InMoveMixer;
        }

        protected override eAnimationType ConvertAnimationType(in eMoveAnimationType InMoveAnimationType)
        {
            switch (InMoveAnimationType)
            {
                case eMoveAnimationType.TURN_LEFT:
                    return eAnimationType.WALK_TURN_LEFT;
                case eMoveAnimationType.TURN_RIGHT:
                    return eAnimationType.WALK_TURN_RIGHT;
                case eMoveAnimationType.STAND_STOP:
                    return eAnimationType.WALK_STAND_STOP;
                case eMoveAnimationType.MOVE_STOP:
                    return eAnimationType.WALK_MOVE_STOP;
            }

            return eAnimationType.WALK;
        }

        protected override AnimancerState PlayAnimation(in eMoveAnimationType InMoveAnimationType,
            in float InAnimationSpeed = 1, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InMoveAnimationType)
            {
                case eMoveAnimationType.MOVE_STOP:
                    SetState(eMovementType.STOP);
                    var state = Animation.PlayAnimation(ConvertAnimationType(InMoveAnimationType),
                        Movement.FrontFoot == AvatarIKGoal.LeftFoot ? _moveStop : _moveStopRight, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    SetUseRootMotion(state);
                    return state;
                default:
                    return base.PlayAnimation(InMoveAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
            }
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InAnimationType)
            {
                case eAnimationType.RUN:
                case eAnimationType.WALK:
                    if (!IsLocal)
                        CheckFrontFootOnMoveAnimation();
                    SetState(eMovementType.MOVE);
                    var state = Animation.PlayAnimation(eAnimationType.WALK, _moveMixer, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    SetUseRootMotion(state);
                    return state;
                case eAnimationType.WALK_TURN_LEFT:
                    SetState(eMovementType.TURN);
                    return PlayAnimation(eMoveAnimationType.TURN_LEFT, InAnimationSpeed, InCalculateSpeedFunc);
                case eAnimationType.WALK_TURN_RIGHT:
                    SetState(eMovementType.TURN);
                    return PlayAnimation(eMoveAnimationType.TURN_RIGHT, InAnimationSpeed, InCalculateSpeedFunc);
                case eAnimationType.WALK_STAND_STOP:
                    SetState(eMovementType.STOP);
                    return PlayAnimation(eMoveAnimationType.STAND_STOP, InAnimationSpeed, InCalculateSpeedFunc);
                case eAnimationType.WALK_MOVE_STOP:
                    SetState(eMovementType.STOP);
                    return PlayAnimation(eMoveAnimationType.MOVE_STOP, InAnimationSpeed, InCalculateSpeedFunc);
            }

            return null;
        }
    }
}