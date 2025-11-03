using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class GatheringAnimationState : InteractionAnimationState
    {
        public override eStateType StateType => eStateType.GATHERING;

        [SerializeField] private ClipTransition _success;

        public override eAnimationType PlayAnimationType
        {
            set
            {
                base.PlayAnimationType = value;
                if (!enabled)
                    Movement.IsGatheringInput = PlayAnimationType != eAnimationType.NONE;
            }
        }

        protected override void PlayAnimationPostProcess(in AnimancerState InAnimationState)
        {
            base.PlayAnimationPostProcess(InAnimationState);
            Movement.IsGatheringInput = false;
        }

        protected override float GetAnimationParameter(in eAnimationType InAnimationType)
        {
            return InAnimationType - eAnimationType.GATHERING_TYPE_START;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            switch (InAnimationType)
            {
                case eAnimationType.GATHERING_SUCCESS:
                    Movement.IsGatheringInput = false;
                    var state = Animation.PlayAnimation(InAnimationType, _success, InAnimationSpeed, InCalculateSpeedFunc);
                    if (state.IsValid())
                        SetAnimationEndEvent(state, OnAnimation_CheckSuccessCancel);
                    SetUseRootMotion(state);
                    return state;
                default:
                    return base.InternalPlayAnimation(InAnimationType, InAnimationSpeed, InCalculateSpeedFunc);
            }
        }

        // public eAnimationType ConvertToAnimationType(in EnumGathering InGatheringType)
        // {
        //     return InGatheringType switch
        //     {
        //         EnumGathering.Gathering => eAnimationType.GATHERING_GATHERING,
        //         EnumGathering.Felling => eAnimationType.GATHERING_FELLING,
        //         EnumGathering.Mining => eAnimationType.GATHERING_MINING,
        //         EnumGathering.Toshovel => eAnimationType.GATHERING_TO_SHOVEL,
        //         EnumGathering.IceCarving_01 => eAnimationType.GATHERING_ICECARVING_1,
        //         EnumGathering.IceCarving_02 => eAnimationType.GATHERING_ICECARVING_2,
        //         EnumGathering.Hunting => eAnimationType.GATHERING_HUNTING,
        //         EnumGathering.SheepShearing => eAnimationType.GATHERING_SHEEP_SHEARING,
        //         EnumGathering.Petting => eAnimationType.GATHERING_PETTING_1,
        //         _ => eAnimationType.GATHERING_GATHERING,
        //     };
        // }

        private void OnAnimation_CheckSuccessCancel()
        {
            if (Movement.IsAnyMovementInput)
                OnAnimation_Finished();
        }
    }
}
