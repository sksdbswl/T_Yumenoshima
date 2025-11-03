using System;
using Animancer;
using RootMotion.FinalIK;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class ParkourAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.PARKOUR;

        [SerializeField] protected LinearMixerTransition _vaultMixer;
        [SerializeField] protected LinearMixerTransition _jumpMixer;
        [SerializeField] protected LinearMixerTransition _climbMixer;
        [SerializeField] protected LinearMixerTransition _wallRunMixer;

        public override bool CanExitState => base.CanExitState && _playingAniState == null;

        private float GetVaultAnimationParameter(eAnimationType InAnimationType)
        {
            return InAnimationType - eAnimationType.PARKOUR_TYPE_START;
        }

        private float GetJumpAnimationParameter(eAnimationType InAnimationType)
        {
            return InAnimationType - eAnimationType.PARKOUR_JUMP_TYPE_START;
        }

        private float GetClimbAnimationParameter(eAnimationType InAnimationType)
        {
            return InAnimationType - eAnimationType.PARKOUR_CLIMB_TYPE_START;
        }

        private float GetWallRunAnimationParameter(eAnimationType InAnimationType)
        {
            return InAnimationType - eAnimationType.PARKOUR_WALL_RUN_TYPE_START;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InAnimationType)
            {
                case eAnimationType.PARKOUR_VAULT_OVER_LEFT:
                case eAnimationType.PARKOUR_VAULT_OVER_RIGHT:
                case eAnimationType.PARKOUR_VAULT_ON:
                {
                    var state = Animation.PlayAnimation(InAnimationType, _vaultMixer, InAnimationSpeed, InCalculateSpeedFunc);
                    if (state.IsValid() && _vaultMixer.State != null)
                        _vaultMixer.State.Parameter = GetVaultAnimationParameter(InAnimationType);
                    return state;
                }
                case eAnimationType.PARKOUR_JUMP_LEFT_FOOT:
                case eAnimationType.PARKOUR_JUMP_RIGHT_FOOT:
                case eAnimationType.PARKOUR_JUMP_LONG:
                {
                    var state = Animation.PlayAnimation(InAnimationType, _jumpMixer, InAnimationSpeed, InCalculateSpeedFunc);
                    if (state.IsValid() && _jumpMixer.State != null)
                        _jumpMixer.State.Parameter = GetJumpAnimationParameter(InAnimationType);
                    return state;
                }
                case eAnimationType.PARKOUR_CLIMB_JUMP:
                case eAnimationType.PARKOUR_CLIMB_UP_LEDGE:
                case eAnimationType.PARKOUR_CLIMB_OFF_LEDGE:
                {
                    var state = Animation.PlayAnimation(InAnimationType, _climbMixer, InAnimationSpeed, InCalculateSpeedFunc);
                    if (state.IsValid() && _climbMixer.State != null)
                        _climbMixer.State.Parameter = GetClimbAnimationParameter(InAnimationType);
                    return state;
                }
                case eAnimationType.PARKOUR_WALL_RUN_LEFT:
                case eAnimationType.PARKOUR_WALL_RUN_RIGHT:
                {
                    var state = Animation.PlayAnimation(InAnimationType, _wallRunMixer, InAnimationSpeed, InCalculateSpeedFunc);
                    if (state.IsValid() && _wallRunMixer.State != null)
                        _wallRunMixer.State.Parameter = GetWallRunAnimationParameter(InAnimationType);
                    return state;
                }
            }

            return base.InternalPlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
        }
    }
}