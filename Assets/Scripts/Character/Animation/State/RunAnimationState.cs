using System;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;
    using eMoveType = CharacterAnimationEnums.eMoveType;

    public class RunAnimationState : LocomotionAnimationState
    {
        public override eStateType StateType => eStateType.RUN;

        public override (bool isChange, eStateType nextType) NextStateType
        {
            get
            {
                var nextState = base.NextStateType;
                if (nextState.isChange)
                    return nextState;

                if (Movement.IsMoveInput)
                {
                    if (Movement.IsSprintInput)
                    {
                        if (Movement.IsSprint)
                            return (true, eStateType.SPRINT);
                    }
                    else if (Movement.IsWalkInput)
                    {
                        if (AnimationParameters.ForwardSpeed <= Mathf.Max(Movement.WalkSpeed, Movement.RunSpeed * 0.5f))
                            return (true, eStateType.WALK);
                    }

                    return (false, StateType);
                }
                else if (AnimationParameters.IsValidForwardSpeed)
                {
                    return (false, StateType);
                }

                return (false, DefaultStateType);
            }
        }

        protected override bool CanExitStopState
        {
            get
            {
                if (base.CanExitStopState)
                    return true;
                if (Movement.IsMoveInput && Movement.IsWalkInput)
                    return true;
                return false;
            }
        }

        public LinearMixerTransition MoveMixer => _moveMixer;

        protected override void OnEnable()
        {
            base.OnEnable();

            SetState(eMovementType.IDLE);

            if (!ApplyReservedMoveType())
            {
                if (Movement.IsMoveInput)
                {
                    if (!PlayQuickTurnAnimation())
                    {
                        Movement.IsCorrectionRootMotion = true;
                        PlayMoveAnimation(true);
                    }
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Character is LocalCharacter localCharacter && Animation.CurrentState == eStateType.WALK)
                localCharacter.ResetRootMotionVelocityQueue();
        }

        protected override void SetState(in eMovementType InType)
        {
            base.SetState(InType);

            switch (InType)
            {
                case eMovementType.MOVE:
                    Movement.CurrentMoveType = eMoveType.RUN;
                    break;
                case eMovementType.STOP:
                    Movement.IsSprintInput = false;
                    break;
            }
        }

        protected override void PlayIdleAnimation()
        {
            if (_moveMixer.State is { IsCurrent: false })
                InternalPlayAnimation(eAnimationType.IDLE);
            else
                base.PlayIdleAnimation();
        }

        public override bool IsPlayingAnimation(in float InNormalizedTime)
        {
            if (base.IsPlayingAnimation(InNormalizedTime))
                return true;
            if (Animancer.States.TryGet(_moveStart, out var state) && state.IsPlaying && state.NormalizedTime < InNormalizedTime)
                return true;
            return false;
        }

        protected override eAnimationType ConvertAnimationType(in eMoveAnimationType InMoveAnimationType)
        {
            switch (InMoveAnimationType)
            {
                case eMoveAnimationType.TURN_LEFT:
                    return eAnimationType.RUN_TURN_LEFT;
                case eMoveAnimationType.TURN_RIGHT:
                    return eAnimationType.RUN_TURN_RIGHT;
                case eMoveAnimationType.QUICK_TURN_LEFT:
                    return eAnimationType.RUN_QUICK_TURN_LEFT;
                case eMoveAnimationType.QUICK_TURN_RIGHT:
                    return eAnimationType.RUN_QUICK_TURN_RIGHT;
                case eMoveAnimationType.STAND_STOP:
                    return eAnimationType.RUN_STAND_STOP;
                case eMoveAnimationType.MOVE_STOP:
                    return eAnimationType.RUN_MOVE_STOP;
            }

            return eAnimationType.RUN;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            AnimancerState state = null;

            switch (InAnimationType)
            {
                case eAnimationType.IDLE:
                    SetState(eMovementType.IDLE);
                    state = Animation.PlayAnimation(InAnimationType, _moveMixer, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.RUN:
                    if (!IsLocal)
                        CheckFrontFootOnMoveAnimation();
                    SetState(eMovementType.MOVE);
                    state = Animation.PlayAnimation(InAnimationType, _moveMixer, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.RUN_START:
                    SetState(eMovementType.MOVE);
                    state = Animation.PlayAnimation(InAnimationType, _moveStart, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.RUN_TURN_LEFT:
                    state = PlayAnimation(eMoveAnimationType.TURN_LEFT, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.RUN_TURN_RIGHT:
                    state = PlayAnimation(eMoveAnimationType.TURN_RIGHT, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.RUN_QUICK_TURN_LEFT:
                    state = PlayAnimation(eMoveAnimationType.QUICK_TURN_LEFT, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.RUN_QUICK_TURN_RIGHT:
                    state = PlayAnimation(eMoveAnimationType.QUICK_TURN_RIGHT, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.RUN_STAND_STOP:
                    state = PlayAnimation(eMoveAnimationType.STAND_STOP, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.RUN_MOVE_STOP:
                    state = PlayAnimation(eMoveAnimationType.MOVE_STOP, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
            }

            SetUseRootMotion(state);

            return state;
        }

        public void OnAnimation_FinishCorrectionRootMotion()
        {
            Movement.IsCorrectionRootMotion = false;
        }
    }
}