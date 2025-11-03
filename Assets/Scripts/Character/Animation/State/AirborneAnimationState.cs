using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;
    using eMoveType = CharacterAnimationEnums.eMoveType;

    public class AirborneAnimationState : CharacterAnimationState
    {
        public enum eLandingType
        {
            NONE = 0,
            STAND,
            LEFT_FOOT,
            RIGHT_FOOT,
            AIRBORNE,
        }

        public override eStateType StateType => eStateType.AIRBORNE;

        [SerializeField] protected ClipTransition _fall;
        [SerializeField] protected MixerTransition2D _landingMixer;

        protected eLandingType _landingType = eLandingType.NONE;
        protected bool _isLanding = false;
        protected bool _enableAnyMovement = false;

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
                    if (AnimationParameters.ForwardSpeed > 0.1f)
                    {
                        if (Movement.IsSprintInput)
                            return (true, eStateType.SPRINT);
                        if (Movement.IsWalkInput)
                            return (true, eStateType.WALK);
                        if (!isMoveInput)
                            AnimationParameters.ReservedMoveType = LocomotionAnimationState.eMovementType.STOP;
                        return (true, eStateType.RUN);
                    }
                }

                return (false, DefaultStateType);
            }
        }

        public override bool CanEnterState => Movement.IsAirborne;
        public override bool CanExitState => base.CanExitState || Movement.IsGrappleInput ||
                                             (_isLanding && _playingAniState == null);

        public override bool ApplyRawRootMotion => true;

        protected override void OnEnable()
        {
            base.OnEnable();

            ChangeStaminaActionType(eStaminaActionType.Normal);
            PlayFallAnimation();

            Movement.IsLanding = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Reset();

            Movement.IsCorrectionRootMotion = false;
            Movement.SetAirbornStateGrounderIKWeight(false);
        }

        protected virtual void Reset()
        {
            _isLanding = false;
            _enableAnyMovement = false;
        }

        public override bool LateUpdateState()
        {
            UpdateAnyMovement();

            if (!base.LateUpdateState())
                return false;

            UpdateLanding();

            return true;
        }

        protected virtual bool UpdateAnyMovement()
        {
            if (!_enableAnyMovement)
                return false;

            if (Movement.IsAnyMovementInput)
                _playingAniState = null;
            else
            {
                switch (_landingType)
                {
                    case eLandingType.LEFT_FOOT:
                    case eLandingType.RIGHT_FOOT:
                        if (!Movement.IsMoveInput)
                            _playingAniState = null;
                        break;
                }
            }

            return true;
        }

        private void PlayFallAnimation()
        {
            Reset();

            if (Movement.IsFalling)
                Movement.CurrentMoveType = eMoveType.AIRBORNE;

            _playingAniState = InternalPlayAnimation(eAnimationType.AIRBORNE_FALL);
            SetAnimationEndEvent(_playingAniState, OnAnimation_FallEndEvent);

            if (Character is LocalCharacter localCharacter)
                localCharacter.SetRootMotionAirMoveSpeed((int)Movement.CurrentMoveType);
        }

        protected virtual void UpdateLanding()
        {
            if (_isLanding || !Movement.IsLanding)
                return;

            PlayLandingAnimation();
        }

        protected virtual bool PlayLandingAnimation()
        {
            if (_landingMixer is { IsValid: true })
            {
                _isLanding = true;

                if (Movement.IsFalling || _prevState == eStateType.GLIDING)
                    Movement.CurrentMoveType = eMoveType.AIRBORNE;
                else if (!Movement.IsMoveInput)
                    Movement.CurrentMoveType = eMoveType.STAND;
                else if (Movement.CurrentMoveType == eMoveType.DASH)
                    Movement.CurrentMoveType = eMoveType.SPRINT;

                _playingAniState = InternalPlayAnimation(GetLandingAnimationType());
                SetAnimationEndEvent(_playingAniState, OnAnimation_LandingEndEvent);
                return true;
            }

            return false;
        }

        protected virtual eAnimationType GetLandingAnimationType()
        {
            if (Movement.CurrentMoveType == eMoveType.AIRBORNE)
            {
                Character.AnimancerEvents.OnFxEventInt((int)eKnownEffect.FX_LAND);
                return eAnimationType.AIRBORNE_LANDIND;
            }
            else if (!Movement.IsMoveInput ||
                     (!Movement.IsMoving && AnimationParameters.ForwardSpeed < Movement.WalkSpeed))
            {
                return eAnimationType.JUMP_STANDING_LANDIND;
            }
            else
            {
                AvatarIKGoal footType = Movement.FrontFoot;
                switch (Movement.CurrentMoveType)
                {
                    case eMoveType.WALK:
                        return footType == AvatarIKGoal.LeftFoot
                            ? eAnimationType.JUMP_WALK_LEFT_FOOT_LANDING
                            : eAnimationType.JUMP_WALK_RIGHT_FOOT_LANDING;
                    case eMoveType.RUN:
                        return footType == AvatarIKGoal.LeftFoot
                            ? eAnimationType.JUMP_RUN_LEFT_FOOT_LANDING
                            : eAnimationType.JUMP_RUN_RIGHT_FOOT_LANDING;
                    case eMoveType.SPRINT:
                        return footType == AvatarIKGoal.LeftFoot
                            ? eAnimationType.JUMP_SPRINT_LEFT_FOOT_LANDING
                            : eAnimationType.JUMP_SPRINT_RIGHT_FOOT_LANDING;
                }
            }

            return eAnimationType.AIRBORNE_LANDIND;
        }

        public override bool IsPlayingAnimation(in float InNormalizedTime)
        {
            if (base.IsPlayingAnimation(InNormalizedTime))
                return true;
            if (Animancer.States.TryGet(_fall, out var state) && state.IsPlaying &&
                state.NormalizedTime < InNormalizedTime)
                return true;
            if (Animancer.States.TryGet(_landingMixer, out state) && state.IsPlaying &&
                state.NormalizedTime < InNormalizedTime)
                return true;
            return false;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            if (InAnimationType == eAnimationType.AIRBORNE_FALL)
            {
                var state = Animation.PlayAnimation(InAnimationType, _fall, InAnimationSpeed, InCalculateSpeedFunc);
                SetUseRootMotion(state);
                return state;
            }
            else
            {
                _landingType = InAnimationType switch
                {
                    eAnimationType.JUMP_STANDING_LANDIND => eLandingType.STAND,
                    eAnimationType.JUMP_WALK_LEFT_FOOT_LANDING or eAnimationType.JUMP_RUN_LEFT_FOOT_LANDING
                        or eAnimationType.JUMP_SPRINT_LEFT_FOOT_LANDING => eLandingType.LEFT_FOOT,
                    eAnimationType.JUMP_WALK_RIGHT_FOOT_LANDING or eAnimationType.JUMP_RUN_RIGHT_FOOT_LANDING
                        or eAnimationType.JUMP_SPRINT_RIGHT_FOOT_LANDING => eLandingType.RIGHT_FOOT,
                    eAnimationType.AIRBORNE_LANDIND => eLandingType.AIRBORNE,
                    _ => eLandingType.STAND
                };

                switch (InAnimationType)
                {
                    case eAnimationType.AIRBORNE_LANDIND:
                    case eAnimationType.JUMP_STANDING_LANDIND:
                    case eAnimationType.JUMP_WALK_LEFT_FOOT_LANDING:
                    case eAnimationType.JUMP_WALK_RIGHT_FOOT_LANDING:
                    case eAnimationType.JUMP_RUN_LEFT_FOOT_LANDING:
                    case eAnimationType.JUMP_RUN_RIGHT_FOOT_LANDING:
                    case eAnimationType.JUMP_SPRINT_LEFT_FOOT_LANDING:
                    case eAnimationType.JUMP_SPRINT_RIGHT_FOOT_LANDING:
                    {
                        var state = Animation.PlayAnimation(InAnimationType, _landingMixer, InAnimationSpeed, InCalculateSpeedFunc);
                        if (state.IsValid() && _landingMixer.State != null)
                        {
                            _landingMixer.State.Parameter = GetLandingAnimationParameter();
                            _landingMixer.State.RecalculateWeights();
                        }
                        SetUseRootMotion(state);
                        return state;
                    }
                }
            }

            return null;
        }

        public override AnimancerState PlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InAnimationType)
            {
                case eAnimationType.AIRBORNE_FALL:
                    Movement.CurrentMoveType = eMoveType.AIRBORNE;
                    return InternalPlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
                default:
                    if (!Animation.IsLocal)
                    {
                        if (InAnimationType == eAnimationType.JUMP_STANDING_LANDIND)
                            Movement.CurrentMoveType = eMoveType.STAND;
                    }

                    return InternalPlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
            }
        }

        protected virtual Vector2 GetLandingAnimationParameter()
        {
            return new Vector2((int)_landingType, (int)Movement.CurrentMoveType);
        }

        protected virtual void OnAnimation_FallEndEvent()
        {
            _playingAniState = null;
        }

        protected virtual void OnAnimation_LandingEndEvent()
        {
            _playingAniState = null;
            Movement.IsCorrectionRootMotion = false;
            Movement.SetAirbornStateGrounderIKWeight(false);
            
            if (Character is LocalCharacter localCharacter)
                localCharacter.CharacterLookDir = Vector3.zero;
        }

        public void OnAnimation_EnableAnyMovementLandingEvent(int InLandingType)
        {
            if ((int)_landingType != InLandingType)
                return;

            _enableAnyMovement = true;
            Movement.UseRootMotionRotation = CharacterRootMotionMode.Ignore;

            if (_landingType == eLandingType.STAND)
                Movement.UseHorizontalRootMotionPosition = CharacterRootMotionMode.Override;
        }

        public void OnAnimation_RestoreGrounderIKWeightEvent(int InLandingType)
        {
            if ((Movement.CurrentMoveType == eMoveType.RUN || Movement.CurrentMoveType == eMoveType.SPRINT) &&
                (int)_landingType != InLandingType)
                return;

            Movement.SetAirbornStateGrounderIKWeight(false);
        }
    }
}