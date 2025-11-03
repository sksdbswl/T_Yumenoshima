using System;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;
    //using TabType = CharacterStageUI.TabType;
    
    public class CharacterStageAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.CHARACTER_STAGE;

        public override (bool isChange, eStateType nextType) NextStateType => (false, StateType);
        [SerializeField] private ClipTransition idleClip;
        [SerializeField] private ClipTransition avatarClip;
        
        [SerializeField] private LinearMixerTransition idleToAvatarMixer;
        [SerializeField] private LinearMixerTransition avatarToIdleMixer;

        public override bool CanEnterState => true;
        public override bool CanExitState => true;
        private eAnimationType currentAnimationType = eAnimationType.NONE;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            PlayMainAnimation();

            // Movement?.CurrentMoveType = eMoveType.STAND;
        }

        private void PlayMainAnimation()
        {
            InternalPlayAnimation(eAnimationType.CHARACTER_STAGE_IDLE);
        }

        // [suhlee] TODO: Refactor: 탭 별 Transition 처리 필요
        // public void OnClickCharacterStageUITab(TabType prevTab, TabType currentTab)
        // {
        //     switch (currentTab)
        //     {
        //         case TabType.Character:
        //         case TabType.Tools:
        //         case TabType.Emotes:
        //         case TabType.Equipment:
        //             if (prevTab == TabType.Avatar)
        //             {
        //                 InternalPlayAnimation(eAnimationType.CHARACTER_STAGE_AVATAR_TO_IDLE); 
        //             }
        //             break;
        //         case TabType.Avatar:
        //             if (prevTab != TabType.Avatar)
        //             {
        //                 InternalPlayAnimation(eAnimationType.CHARACTER_STAGE_IDLE_TO_AVATAR);
        //             }
        //             break;
        //     }
        // }
        
        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            AnimancerState state = null;
            switch (InAnimationType)
            {
                case eAnimationType.CHARACTER_STAGE_IDLE:
                    state = Animation.PlayAnimation(InAnimationType, idleClip, InAnimationSpeed, InCalculateSpeedFunc); break;
                case eAnimationType.CHARACTER_STAGE_AVATAR:
                    state = Animation.PlayAnimation(InAnimationType, avatarClip, InAnimationSpeed, InCalculateSpeedFunc); break;
                
                case eAnimationType.CHARACTER_STAGE_IDLE_TO_AVATAR:
                    state = Animation.PlayAnimation(InAnimationType, idleToAvatarMixer, InAnimationSpeed, InCalculateSpeedFunc); break;
                case eAnimationType.CHARACTER_STAGE_AVATAR_TO_IDLE:
                    state = Animation.PlayAnimation(InAnimationType, avatarToIdleMixer, InAnimationSpeed, InCalculateSpeedFunc); break;
            }
            
            SetUseRootMotion(state);
            return state;
        }
    }
}
