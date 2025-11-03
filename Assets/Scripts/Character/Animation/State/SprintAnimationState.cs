using System;
using System.Threading;
using Animancer;
using Animancer.Units;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;
    using eMoveType = CharacterAnimationEnums.eMoveType;

    public class SprintAnimationState : RunAnimationState
    {
        public override eStateType StateType => eStateType.SPRINT;

        [Tooltip("스탑 애니메이션이 실행되기 까지의 입력 간격")] [SerializeField, Seconds(Rule = Validate.Value.IsNotNegative)]
        private float _stopInputInterval = 0.05f;

        private float _noInputTime;
        private CancellationTokenSource _cts;

        public event Action StartSprintEvent;

        protected override bool CanExitStopState
        {
            get
            {
                if (Movement.IsAnyActionInput || Movement.IsAirborne)
                    return true;
                if (!Movement.IsMoveInput || UpdateTurn())
                    return false;
                return true;
            }
        }

        protected override bool CanExitMoveState
        {
            get
            {
                if (Movement.IsAnyActionInput || Movement.IsAirborne)
                    return true;
                if (_prevMovementType == eMovementType.QUICK_TURN)
                    return true;
                if (Movement.IsSprint)
                    return false;
                return true;
            }
        }

        public override (bool isChange, eStateType nextType) NextStateType
        {
            get
            {
                var nextState = base.NextStateType;
                if (nextState.isChange)
                    return nextState;

                bool isMoveInput = Movement.IsMoveInput;
                if (isMoveInput || AnimationParameters.IsValidForwardSpeed)
                {
                    if (Movement.IsSprint)
                        return (true, eStateType.SPRINT);
                }

                if (!isMoveInput)
                    AnimationParameters.ReservedMoveType = eMovementType.STOP;
                return (true, eStateType.RUN);
            }
        }

        protected override void OnEnable()
        {
            BaseOnEnable();

            _currentForwardSpeed = AnimationParameters.ForwardSpeed;

            if (!ApplyReservedMoveType())
            {
                PlayMoveAnimation(true);
                Movement.IsCorrectionRootMotion = true;
            }
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            //Character.CharacterEffectSound.LoopingFxUniTask(eKnownEffect.SPRINT_Loop, _cts.Token).Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Character is LocalCharacter localCharacter)
                localCharacter.ResetRootMotionVelocityQueue();

            StartSprintEvent = null;
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            //Character.CharacterEffectSound.StopLoopingFx(eKnownEffect.SPRINT_Loop);
        }

        protected override void SetState(in eMovementType InType)
        {
            base.SetState(InType);

            switch (InType)
            {
                case eMovementType.MOVE:
                    Movement.CurrentMoveType = eMoveType.SPRINT;
                    break;
            }
        }

        protected override void UpdateStop()
        {
            if (_currentMovementType != eMovementType.MOVE ||
                _currentForwardSpeed <= AnimationParameters.ForwardSpeed || Movement.IsMoveInput)
            {
                _noInputTime = 0f;
                return;
            }

            if (_noInputTime == 0f)
                _noInputTime = Time.time;

            if (Time.time - _noInputTime >= _stopInputInterval)
            {
                var state = PlayAnimation(eMoveAnimationType.MOVE_STOP);
                SetAnimationEndEvent(state, OnAnimation_EndEvent);
            }
        }

        protected override void UpdateMove()
        {
        }

        protected override void PlayMoveAnimation(in bool InCheckFoot = false)
        {
            base.PlayMoveAnimation(in InCheckFoot);
            StartSprintEvent?.Invoke();
        }

        protected override void PlayStopAnimation()
        {
            if (Movement.IsMoveInput && IsMoving)
            {
                if (_moveStop.IsValid)
                {
                    var state = PlayAnimation(eMoveAnimationType.MOVE_STOP);
                    SetAnimationEndEvent(state, OnAnimation_EndEvent);
                }
            }
            else
            {
                if (_standStop.IsValid)
                {
                    var state = PlayAnimation(eMoveAnimationType.STAND_STOP);
                    SetAnimationEndEvent(state, OnAnimation_EndEvent);
                }
            }

            if (_currentMovementType != eMovementType.STOP)
                SetState(eMovementType.IDLE);
        }

        public void SetMoveMixer(LinearMixerTransition InMoveMixer)
        {
            _moveMixer = InMoveMixer;
        }

        protected override eAnimationType ConvertAnimationType(in eMoveAnimationType InMoveAnimationType)
        {
            switch (InMoveAnimationType)
            {
                case eMoveAnimationType.QUICK_TURN_LEFT:
                    return eAnimationType.SPRINT_QUICK_TURN_LEFT;
                case eMoveAnimationType.QUICK_TURN_RIGHT:
                    return eAnimationType.SPRINT_QUICK_TURN_RIGHT;
                case eMoveAnimationType.STAND_STOP:
                    return eAnimationType.SPRINT_STAND_STOP;
                case eMoveAnimationType.MOVE_STOP:
                    return eAnimationType.SPRINT_MOVE_STOP;
            }

            return eAnimationType.SPRINT;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InAnimationType)
            {
                case eAnimationType.RUN:
                case eAnimationType.SPRINT:
                    if (!IsLocal)
                        CheckFrontFootOnMoveAnimation();
                    SetState(eMovementType.MOVE);
                    var state = Animation.PlayAnimation(eAnimationType.SPRINT, _moveMixer, InAnimationSpeed, InCalculateSpeedFunc);
                    SetUseRootMotion(state);
                    return state;
                case eAnimationType.SPRINT_QUICK_TURN_LEFT:
                    SetState(eMovementType.QUICK_TURN);
                    return PlayAnimation(eMoveAnimationType.QUICK_TURN_LEFT, InAnimationSpeed,
                        InCalculateSpeedFunc);
                case eAnimationType.SPRINT_QUICK_TURN_RIGHT:
                    SetState(eMovementType.QUICK_TURN);
                    return PlayAnimation(eMoveAnimationType.QUICK_TURN_RIGHT, InAnimationSpeed,
                        InCalculateSpeedFunc);
                case eAnimationType.SPRINT_STAND_STOP:
                    SetState(eMovementType.STOP);
                    return PlayAnimation(eMoveAnimationType.STAND_STOP, InAnimationSpeed, InCalculateSpeedFunc);
                case eAnimationType.SPRINT_MOVE_STOP:
                    SetState(eMovementType.STOP);
                    return PlayAnimation(eMoveAnimationType.MOVE_STOP, InAnimationSpeed, InCalculateSpeedFunc);
                default:
                    return base.InternalPlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
            }
        }

        protected override void OnAnimation_EndEvent()
        {
            Character.LockMoveInput = false;

            if (Movement.IsMoveInput)
            {
                if (_currentMovementType == eMovementType.QUICK_TURN)
                    AnimationParameters.ForwardSpeed = Movement.RunSpeed;

                PlayMoveAnimation(_currentMovementType == eMovementType.QUICK_TURN);
            }
            else
            {
                if (_currentMovementType == eMovementType.QUICK_TURN)
                    PlayStopAnimation();
                else
                    PlayIdleAnimation();
            }

            Movement.RootMotionPositionCorrectionFunc -= QuickTurnAnimationCorrectionPosition;
            Movement.RootMotionRotationCorrectionFunc -= TurnAnimationCorrectionRotation;
        }
    }
}
